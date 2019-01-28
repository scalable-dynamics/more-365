using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace more365.Graph
{
    public sealed class GraphClient : IGraphClient
    {
        public static string MicrosoftGraphVersion = "v1.0";
        public static Uri MicrosoftGraphUrl = new Uri("https://graph.microsoft.com");

        private readonly HttpClient _httpClient;

        public GraphClient(HttpClient authenticatedHttpClient)
        {
            if (authenticatedHttpClient.BaseAddress == null)
            {
                throw new GraphClientException("Invalid Base Address", "Must provide a BaseAddress for Graph Site on HttpClient");
            }

            _httpClient = authenticatedHttpClient;
        }

        public async Task<byte[]> DownloadFileAsPdf(string filePath)
        {
            var GraphUrl = _httpClient.BaseAddress.GetLeftPart(UriPartial.Authority);
            var folderPath = filePath.Substring(0, filePath.LastIndexOf('/')) + "/";
            var folderPathUrl = Uri.EscapeUriString(GraphUrl + "/" + folderPath.TrimStart('/'));
            var filePathUrl = Uri.EscapeUriString(GraphUrl + "/" + filePath.TrimStart('/'));
            var graphUrl = await getGraphGraphSiteUrl();
            var drives = await request<GraphContext<GraphDrive>>(graphUrl + "/drives?$filter=driveType eq 'documentLibrary'");
            var drive = drives.Value.FirstOrDefault(d => folderPathUrl.StartsWith(d.WebUrl + "/", StringComparison.InvariantCultureIgnoreCase));
            if (drive == null)
            {
                throw new GraphClientException(Uri.UnescapeDataString(filePathUrl), "documentLibrary not found");
            }
            var url = $"{graphUrl}/drives/{drive.Id}/root:/{filePathUrl.Replace(drive.WebUrl, "")}:/content?format=pdf";
            return await download(url);
        }

        public async Task SendOutlookEmail(string subject, string content, string fromSender, params string[] toRecipients)
        {
            var url = $"{this.getGraphGraphUrl()}/users/{fromSender}/sendMail";
            await request<HttpResponseMessage>(url, HttpMethod.Post, new
            {
                message = new
                {
                    subject,
                    body = new { content, contentType = "text/html" },
                    toRecipients = toRecipients.Select(r => new { emailAddress = new { address = r } })
                },
                saveToSentItems = true
            });
        }

        private async Task<T> request<T>(string url, HttpMethod method = null, object requestBody = null)
        {
            var request = new HttpRequestMessage(method ?? HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");

            if (requestBody != null)
            {
                if (requestBody is Stream stream)
                {
                    request.Content = new StreamContent(stream);
                }
                else if (requestBody is byte[] bytes)
                {
                    request.Content = new ByteArrayContent(bytes);
                }
                else
                {
                    var requestJson = JsonConvert.SerializeObject(requestBody);
                    request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                }
            }

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (method == null || method == HttpMethod.Get)
                {
                    throw new GraphClientException(url, json);
                }
                else
                {
                    throw new GraphClientException(url, json);
                }
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        private async Task<byte[]> download(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                throw new GraphClientException(url, json);
            }

            return await response.Content.ReadAsByteArrayAsync();
        }

        private string getGraphGraphUrl()
        {
            return MicrosoftGraphUrl.ToString().TrimEnd('/') + "/" + MicrosoftGraphVersion;
        }

        private async Task<string> getGraphGraphSiteUrl()
        {
            var GraphUri = _httpClient.BaseAddress;
            var graphUrl = getGraphGraphUrl() + "/sites/" + GraphUri.Host;

            if (GraphUri.AbsolutePath != "/")
            {
                graphUrl += ":" + GraphUri.AbsolutePath.TrimEnd('/') + ":";
            }
            if (graphUrl.Contains(":"))
            {
                var site = await request<GraphSite>(graphUrl);
                graphUrl = getGraphGraphUrl() + "/sites/" + site.Id;
            }
            return graphUrl;
        }
    }
}