using System;

namespace more365.AzureAD
{
    public sealed class AuthenticationClientException : Exception
    {
        public static string MessageTemplate = "The application encountered an error while requesting data from AzureAD. Url: {0}";

        public string Details { get; }

        internal AuthenticationClientException(string url, string details)
            : base(string.Format(MessageTemplate, url))
        {
            Details = details;
        }

        private AuthenticationClientException() { }

        public override string ToString()
        {
            return base.ToString() + "\n\nDetails: " + Details;
        }
    }
}