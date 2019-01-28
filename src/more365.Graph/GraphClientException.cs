using System;

namespace more365.Graph
{
    public sealed class GraphClientException : Exception
    {
        public static string MessageTemplate = "The application encountered an error while requesting data from Graph. Url: {0}";

        public string Details { get; }

        internal GraphClientException(string url, string details)
            : base(string.Format(MessageTemplate, url))
        {
            Details = details;
        }

        private GraphClientException() { }

        public override string ToString()
        {
            return base.ToString() + "\n\nDetails: " + Details;
        }
    }
}