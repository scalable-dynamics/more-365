using System;

namespace more365.SharePoint
{
    public sealed class SharePointClientException : Exception
    {
        public static string MessageTemplate = "The application encountered an error while requesting data from SharePoint. Url: {0}";

        public string Details { get; }

        internal SharePointClientException(string url, string details)
            : base(string.Format(MessageTemplate, url))
        {
            Details = details;
        }

        private SharePointClientException() { }

        public override string ToString()
        {
            return base.ToString() + "\n\nDetails: " + Details;
        }
    }
}