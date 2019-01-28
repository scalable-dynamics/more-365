using System;

namespace more365
{
    public class More365Configuration
    {
        public Uri DynamicsUrl { get; set; }

        public Uri SharePointUrl { get; set; }

        public Guid AzureADTenantId { get; set; }

        public Guid AzureADApplicationId { get; set; }

        public string AzureADAppCertificateKey { get; set; }

        public string AzureADAppClientSecretKey { get; set; }
    }
}