using System;
using System.Net.Http;

namespace more365.Dynamics
{
    public struct BatchRequest
    {
        public readonly HttpMethod Method;
        public readonly string Url;
        public readonly object Body;

        public BatchRequest(string entitySetName, object entity)
        {
            Method = HttpMethod.Post;
            Url = $"/{entitySetName}()";
            Body = entity;
        }

        public BatchRequest(string entitySetName, object entity, Guid entityId)
        {
            Method = new HttpMethod("PATCH");
            Url = $"/{entitySetName}({entityId})";
            Body = entity;
        }

        public BatchRequest(string url, object postBody, HttpMethod method = null)
        {
            Method = method ?? HttpMethod.Post;
            Url = url;
            Body = postBody;
        }

        public BatchRequest(string url)
        {
            if (url.ToLower().StartsWith("http") || !url.StartsWith("/"))
            {
                throw new ArgumentException("Dynamics BatchRequest url must be an absolute url that begins with '/'", "url");
            }
            Method = HttpMethod.Get;
            Url = url;
            Body = null;
        }

        public static implicit operator BatchRequest(string url)
        {
            return new BatchRequest(url);
        }
    }
}