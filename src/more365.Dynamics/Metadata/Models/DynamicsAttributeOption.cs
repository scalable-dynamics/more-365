namespace more365.Dynamics.Metadata
{
    public class DynamicsAttributeOptionCollection
    {
        public DynamicsAttributeOption[] Options { get; set; }
    }

    public class DynamicsAttributeOption
    {
        public string DisplayLabel => Label.UserLocalizedLabel.Label;

        public DynamicsLabel Label { get; set; }

        public int Value { get; set; }
    }
}