using System;
using System.Collections.Generic;

namespace more365.SharePoint
{
    public class SharePointFolder
    {
        public Guid UniqueId { get; set; }

        public string ServerRelativeUrl { get; set; }

        public string Name { get; set; }

        public int ItemCount { get; set; }

        public DateTime TimeCreated { get; set; }

        public DateTime TimeLastModified { get; set; }

        public SharePointFile[] Files { get; set; }

        public SharePointFolder[] Folders { get; set; }
    }
}
