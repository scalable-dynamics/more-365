using System;

namespace more365.Dynamics
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class AttributeNameAttribute : Attribute
    {
        public string PropertyName { get; }

        public AttributeNameAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}