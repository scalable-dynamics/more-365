using System;

namespace more365.SharePoint
{
    public class SharePointFile
    {
        public Guid UniqueId { get; set; }

        public string ServerRelativeUrl { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public int Length { get; set; }

        public DateTime TimeCreated { get; set; }

        public DateTime TimeLastModified { get; set; }
    }
}
