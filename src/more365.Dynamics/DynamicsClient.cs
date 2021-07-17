using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace more365.Dynamics
{
    public sealed class DynamicsClient : IDynamicsClient
    {
        private const string WebApiVersion = "v9.1";
        private const int WebApiMaxBatchRequests = 100;

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public DynamicsClient(HttpClient authenticatedHttpClient)
        {
            if (authenticatedHttpClient.BaseAddress == null)
            {
                throw new DynamicsClientException("Invalid Base Address", "Must provide a BaseAddress for Dynamics on HttpClient");
            }

            _httpClient = authenticatedHttpClient;
            _jsonSerializerOptions = new JsonSerializerOptions();
        }

        public async Task<IEnumerable<T>> ExecuteBatch<T>(params BatchRequest[] requests)
        {
            //https://docs.microsoft.com/en-us/dynamics365/customer-engagement/developer/webapi/execute-batch-operations-using-web-api
            //Note: Batch requests can contain up to 100 individual requests and cannot contain other batch requests
            var batches = requests.Select((r, i) => new { Request = r, Batch = i / WebApiMaxBatchRequests })
                                  .GroupBy(g => g.Batch, g => g.Request)
                                  .Select(x => x.ToArray())
                                  .ToArray();
            var results = new List<T>();
            foreach (var page in batches)
            {
                results.AddRange(await batch<T>(page));
            }
            return results;
        }

        public async Task<IEnumerable<T>> ExecuteQuery<T>(string url)
        {
            if (url.Length > 2000)
            {
                return (await batch<T[]>(url)).First();
            }
            else
            {
                return await request<T[]>(url);
            }
        }

        public Task<T> ExecuteSingle<T>(string url)
        {
            return request<T>(url);
        }

        public async Task<T> Get<T>(string entitySetName, Guid id, params string[] columns)
        {
            var url = $"/{entitySetName}({id})";
            if (columns.Length > 0)
            {
                url += "?$select=" + string.Join(",", columns);
            }
            var results = await ExecuteQuery<T>(url);
            return results.FirstOrDefault();
        }

        public async Task<Guid> Save(string entitySetName, object data, Guid? id = null)
        {
            var request = id.HasValue ? new BatchRequest(entitySetName, data, id.Value) : new BatchRequest(entitySetName, data);
            var ids = await batch<Guid>(request);
            if (ids.Length == 1)
            {
                return ids.Single();
            }
            else
            {
                throw new DynamicsClientException(request.Url, "Save Error: No id returned");
            }
        }

        private async Task<T> request<T>(string url)
        {
            var requestUrl = createWebApiUrl(url);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("OData-MaxVersion", "4.0");
            request.Headers.Add("OData-Version", "4.0");
            request.Headers.Add("Prefer", "odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\"");

            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsStringAsync();
            var result = new DynamicsValue<T>();

            if (response.Content.Headers.ContentType.MediaType.Equals("application/json") && !string.IsNullOrEmpty(data))
            {
                if (data.Contains("\"value\":["))
                {
                    result = JsonSerializer.Deserialize<DynamicsValue<T>>(data, _jsonSerializerOptions);
                }
                else
                {
                    result.value = JsonSerializer.Deserialize<T>(data, _jsonSerializerOptions);
                }
            }

            var hasError = (result != null && (result.error != null || !string.IsNullOrWhiteSpace(result.message)));
            if (!response.IsSuccessStatusCode || hasError)
            {
                if (response.Content.Headers.ContentType.MediaType.Equals("text/html"))
                {
                    var text = Regex.Replace(data, "<style>.*</style>|<.*?>", string.Empty);

                    throw new DynamicsClientException(url, text);
                }
                else if (hasError)
                {
                    throw new DynamicsClientException(url, result.message ?? result.error.message);
                }
                else
                {
                    throw new DynamicsClientException(url, data);
                }
            }

            return result.value;
        }

        private async Task<T[]> batch<T>(params BatchRequest[] requests)
        {
            var batchUrl = createWebApiUrl("$batch");
            var request = new HttpRequestMessage(HttpMethod.Post, batchUrl);
            var batchId = Guid.NewGuid();
            var changesetId = Guid.NewGuid();
            var changesetIndex = 1;
            var multipartContent = new MultipartContent("mixed", "batch_" + batchId);

            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("OData-MaxVersion", "4.0");
            request.Headers.Add("OData-Version", "4.0");

            if (requests.Any(r => r.Method != HttpMethod.Get))
            {
                var changesetContent = new MultipartContent("mixed", "changeset_" + changesetId);
                foreach (var item in requests.Where(r => r.Method != HttpMethod.Get))
                {
                    var requestUrl = createWebApiUrl(item.Url);
                    var entryContent = new StringBuilder();
                    entryContent.AppendLine($"{item.Method.Method} {requestUrl} HTTP/1.1");
                    entryContent.AppendLine("Content-Type: application/json;type=entry");
                    entryContent.AppendLine();

                    if (item.Body != null)
                    {
                        var json = JsonSerializer.Serialize(item.Body, _jsonSerializerOptions);
                        entryContent.AppendLine(json);
                    }

                    var content = new StringContent(entryContent.ToString());
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
                    content.Headers.Add("Content-Transfer-Encoding", "binary");
                    content.Headers.Add("Content-ID", (changesetIndex++).ToString());

                    changesetContent.Add(content);
                }
                multipartContent.Add(changesetContent);
            }

            foreach (var item in requests.Where(r => r.Method == HttpMethod.Get))
            {
                var requestUrl = createWebApiUrl(item.Url);
                var requestContent = new StringBuilder();
                requestContent.AppendLine($"GET {requestUrl} HTTP/1.1");
                requestContent.AppendLine("Accept: application/json");
                requestContent.AppendLine("Prefer: odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\"");
                var content = new StringContent(requestContent.ToString());
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
                content.Headers.Add("Content-Transfer-Encoding", "binary");
                multipartContent.Add(content);
            }

            request.Content = multipartContent;

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                throw new DynamicsClientException(batchUrl, data);
            }

            var results = new List<T>();
            var responses = await response.Content.ReadAsMultipartAsync();
            foreach (var content in responses.Contents)
            {
                var json = await content.ReadAsStringAsync();

                if (json.StartsWith("HTTP/1.1 500 Internal Server Error"))
                {
                    throw new DynamicsClientException(batchUrl, json);
                }

                if (json.StartsWith("--changesetresponse"))
                {
                    var changes = await content.ReadAsMultipartAsync();
                    foreach (var change in changes.Contents)
                    {
                        var data = await change.ReadAsStringAsync();

                        if (data.StartsWith("HTTP/1.1 500 Internal Server Error"))
                        {
                            throw new DynamicsClientException(batchUrl, data);
                        }
                        else if (data.StartsWith("HTTP/1.1 204 No Content") && data.Contains("OData-EntityId"))
                        {
                            var id = data.Substring(data.IndexOf("OData-EntityId"));
                            if (id.Contains("("))
                            {
                                var entityId = id.Split('(', ')')[1];
                                var entityIdJson = $"\"{entityId}\"";
                                var guid = JsonSerializer.Deserialize<T>(entityIdJson, _jsonSerializerOptions);

                                results.Add(guid);
                            }
                        }
                        else
                        {
                            throw new DynamicsClientException(batchUrl, data);
                        }
                    }
                }
                else
                {
                    json = json.Substring(json.IndexOf("{"));
                    var result = JsonSerializer.Deserialize<DynamicsValue<T>>(json, _jsonSerializerOptions);

                    if (result != null && (result.error != null || !string.IsNullOrWhiteSpace(result.message)))
                    {
                        throw new DynamicsClientException(batchUrl, result.message ?? result.error.message);
                    }

                    results.Add(result.value);
                }
            }
            return results.ToArray();
        }

        private string createWebApiUrl(string absoluteUrl)
        {
            var DynamicsUri = _httpClient.BaseAddress;
            var apiPrefix = "api/data";
            absoluteUrl = absoluteUrl.TrimStart('/');
            if (absoluteUrl.ToLower().StartsWith(apiPrefix))
            {
                return DynamicsUri + "/" + absoluteUrl;
            }
            else
            {
                return DynamicsUri + "/" + apiPrefix + "/" + WebApiVersion + "/" + absoluteUrl;
            }
        }
    }
}