using System;

namespace more365
{
    public class More365Exception : Exception
    {
        public static string MessageTemplate = "The application encountered an error while requesting data from {1}. Url: {0}";

        public string Details { get; }

        internal More365Exception(string message, string details)
            : base(message)
        {
            Details = details;
        }

        internal More365Exception(string message, Exception innerException)
            : base(message)
        {
            Details = innerException.Message;
        }

        private More365Exception() { }

        public override string ToString()
        {
            return base.ToString() + "\n\nDetails: " + Details;
        }
    }
}