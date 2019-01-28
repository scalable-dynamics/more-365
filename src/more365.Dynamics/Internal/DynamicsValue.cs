namespace more365.Dynamics
{
    internal class DynamicsValue<T>
    {
        public T value { get; set; }

        public DynamicsError error { get; set; }

        public string message { get; set; }
    }

    internal class DynamicsError
    {
        public string message { get; set; }
    }
}