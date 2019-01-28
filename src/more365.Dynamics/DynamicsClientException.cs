using System;

namespace more365.Dynamics
{
    public sealed class DynamicsClientException : Exception
    {
        public static string MessageTemplate = "The application encountered an error while requesting data from Dynamics. Url: {0}";

        public string Details { get; }

        internal DynamicsClientException(string url, string details)
            : base(string.Format(MessageTemplate, url))
        {
            Details = details;
        }

        private DynamicsClientException() { }

        public override string ToString()
        {
            return base.ToString() + "\n\nDetails: " + Details;
        }
    }
}