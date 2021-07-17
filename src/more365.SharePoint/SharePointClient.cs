using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace more365.SharePoint
{
    public sealed class SharePointClient : ISharePointClient
    {
        private readonly HttpClient _httpClient;

        public SharePointClient(HttpClient authenticatedHttpClient)
        {
            if (authenticatedHttpClient.BaseAddress == null)
            {
                throw new SharePointClientException("Invalid Base Address", "Must provide a BaseAddress for SharePoint Site on HttpClient");
            }

            _httpClient = authenticatedHttpClient;
        }

        public Task<SharePointFolder> GetFolder(string documentLibraryName, string folderPath = "")
        {
            if (!string.IsNullOrEmpty(folderPath))
            {
                documentLibraryName += "/" + folderPath.TrimStart('/');
            }
            var url = $"/_api/web/GetFolderByServerRelativeUrl('{Uri.EscapeDataString(documentLibraryName)}')";
            url += "?$select=ServerRelativeUrl,Name,ItemCount,TimeCreated,TimeLastModified";
            url += "&$expand=Files,Folders";
            return request<SharePointFolder>(url);
        }

        public async Task<string> GetFilePreviewUrl(string filePath)
        {
            var url = $"/_api/web/GetFileByServerRelativeUrl('{Uri.EscapeDataString(filePath)}')/ListItemAllFields/ServerRedirectedEmbedUri";
            var value = await request<SharePointValue>(url);
            return value.Value;
        }

        public Task<byte[]> DownloadFile(string filePath)
        {
            var url = $"/_api/web/GetFileByServerRelativeUrl('{Uri.EscapeDataString(filePath)}')/$value";
            return download(url);
        }

        public Task<SharePointFolder> CreateFolder(string documentLibraryName, string folderPath)
        {
            var url = $"/_api/web/folders";
            return request<SharePointFolder>(url, HttpMethod.Post, new
            {
                ServerRelativeUrl = documentLibraryName + "/" + folderPath.TrimStart('/')
            });
        }

        public Task<SharePointFile> UploadFile(string fileName, byte[] file, string documentLibraryName, string folderPath = "")
        {
            var filePath = documentLibraryName;
            if (!string.IsNullOrEmpty(folderPath))
            {
                filePath += "/" + folderPath.TrimStart('/');
            }
            var url = $"/_api/web/GetFolderByServerRelativeUrl('{Uri.EscapeDataString(filePath)}')/Files/Add(url='{Uri.EscapeDataString(fileName)}',overwrite=true)";
            return request<SharePointFile>(url, HttpMethod.Post, file);
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
                    var requestJson = JsonSerializer.Serialize(requestBody);
                    request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                }
            }

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (method == null || method == HttpMethod.Get)
                {
                    throw new SharePointClientException(url, json);
                }
                else
                {
                    throw new SharePointClientException(url, json);
                }
            }

            return JsonSerializer.Deserialize<T>(json);
        }

        private async Task<byte[]> download(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                throw new SharePointClientException(url, json);
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}