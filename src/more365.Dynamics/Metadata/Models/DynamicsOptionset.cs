namespace more365.Dynamics.Metadata
{
    public class DynamicsOptionset
    {
        public string AttributeLogicalName { get; }

        public string AttributeDisplayName { get; }

        public string Label { get; }

        public int Value { get; }

        internal DynamicsOptionset(string attributeLogicalName, string attributeDisplayName, string label, int value)
        {
            AttributeLogicalName = attributeLogicalName;
            AttributeDisplayName = attributeDisplayName;
            Label = label;
            Value = value;
        }
    }
}