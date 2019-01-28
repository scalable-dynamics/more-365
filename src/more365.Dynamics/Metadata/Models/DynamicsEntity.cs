using System.Linq;

namespace more365.Dynamics.Metadata
{
    public class DynamicsEntity
    {
        public string LogicalName { get; set; }

        public string EntitySetName { get; set; }

        public string PrimaryIdAttribute { get; set; }

        public string PrimaryNameAttribute { get; set; }

        public string IconSmallName { get; set; }

        public bool IsActivity { get; set; }

        public bool IsCustomEntity { get; set; }

        public DynamicsAttribute[] Attributes { get; set; }

        public string DisplayLabel => DisplayName.UserLocalizedLabel?.Label ?? LogicalName;

        public DynamicsLabel DisplayName { get; set; }
    }
}