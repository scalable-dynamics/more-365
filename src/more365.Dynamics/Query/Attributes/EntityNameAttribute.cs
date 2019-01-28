using System;

namespace more365.Dynamics
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class EntityNameAttribute : Attribute
    {
        public string EntityLogicalName { get; }

        public string EntitySetName { get; }

        public EntityNameAttribute(string EntityLogicalName, string EntitySetName)
        {
            this.EntityLogicalName = EntityLogicalName;
            this.EntitySetName = EntitySetName;
        }
    }
}