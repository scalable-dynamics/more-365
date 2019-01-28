using System;

namespace more365.Graph
{
    internal class GraphSite
    {
        public string Id { get; set; }

        public string WebUrl { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime LastModifiedDateTime { get; set; }
    }
}