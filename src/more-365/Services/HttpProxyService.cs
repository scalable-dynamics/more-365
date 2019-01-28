using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace more365
{
    public class HttpProxyService
    {
        public static readonly string[] IgnoredRequestHeaders = new[] { "Host", "Origin", "Referrer" };
        public static readonly string[] IgnoredResponseHeaders = new[] { "Strict-Transport-Security", "Transfer-Encoding", "Set-Cookie", "Server", "Access-Control-Allow-Origin", "Access-Control-Expose-Headers" };

        private readonly HttpClient _httpClient;

        public HttpProxyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public virtual async Task ProxyRequest(HttpRequest request, HttpResponse response, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var url = request.Path.Value + request.QueryString.ToString();
                var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method), url);

                if (HttpMethods.IsPost(request.Method) || HttpMethods.IsPut(request.Method) || HttpMethods.IsPatch(request.Method))
                {
                    requestMessage.Content = new StreamContent(request.Body);
                }

                SetRequestHeaders(request.Headers, requestMessage);

                var responseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                SetResponseHeaders(responseMessage.Headers, response);

                SetResponseHeaders(responseMessage.Content.Headers, response);

                response.StatusCode = (int)responseMessage.StatusCode;

                using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
                {
                    await responseStream.CopyToAsync(response.Body, 81920, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.ContentType = "text/plain";
                await response.WriteAsync(ex.ToString());
            }
        }

        protected virtual void SetRequestHeaders(IHeaderDictionary headers, HttpRequestMessage requestMessage)
        {
            foreach (var header in headers)
            {
                var headerValues = header.Value.ToArray();
                if (headerValues.Length > 0
                    && !IgnoredRequestHeaders.Contains(header.Key)
                    && !requestMessage.Headers.TryAddWithoutValidation(header.Key, headerValues)
                    && requestMessage.Content != null)
                {
                    requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, headerValues);
                }
            }
        }

        protected virtual void SetResponseHeaders(HttpHeaders headers, HttpResponse response)
        {
            foreach (var header in headers)
            {
                var headerValues = header.Value.ToArray();
                if (headerValues.Length > 0
                    && !IgnoredResponseHeaders.Contains(header.Key))
                {
                    response.Headers[header.Key] = headerValues;
                }
            }
        }
    }
}