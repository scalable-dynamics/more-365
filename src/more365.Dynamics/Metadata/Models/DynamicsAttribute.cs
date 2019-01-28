using System.Linq;

namespace more365.Dynamics.Metadata
{
    public class DynamicsAttribute
    {
        public string LogicalName { get; set; }

        public string SchemaName { get; set; }

        public bool IsCustomAttribute { get; set; }

        public string AttributeType { get; set; }

        public string[] Targets { get; set; }

        public string DisplayLabel => DisplayName.UserLocalizedLabel?.Label ?? LogicalName;

        public DynamicsOptionset[] Options => AttributeType == "Picklist" || AttributeType == "Status" || AttributeType == "State" ? (GlobalOptionSet ?? OptionSet).Options.Select(o => new DynamicsOptionset(LogicalName, DisplayLabel, o.DisplayLabel, o.Value)).ToArray() : null;

        public DynamicsLabel DisplayName { get; set; }

        public DynamicsAttributeOptionCollection OptionSet { get; set; }

        public DynamicsAttributeOptionCollection GlobalOptionSet { get; set; }
    }
}