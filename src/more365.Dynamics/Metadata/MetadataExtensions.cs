using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace more365.Dynamics.Metadata
{
    public static class MetadataExtensions
    {
        private static string[] EntityProperties = new[] {
            "EntitySetName", "Description", "DisplayName", "LogicalName", "PrimaryIdAttribute",
            "PrimaryNameAttribute", "IconSmallName", "IsActivity", "IsCustomEntity"
        };

        private static string[] AttributeProperties = new[] {
            "AttributeType", "DisplayName", "LogicalName", "SchemaName", "IsCustomAttribute"
        };

        public static Task<IEnumerable<DynamicsEntity>> GetAllEntityMetadata(this IDynamicsClient dynamicsClient, bool includeSystemEntities = false)
        {
            var properties = string.Join(",", EntityProperties);
            var url = $"/EntityDefinitions?$select={properties}";
            if (!includeSystemEntities)
            {
                url += "&$filter=IsValidForAdvancedFind eq true and IsChildEntity eq false and IsBPFEntity eq false";
            }
            return dynamicsClient.ExecuteQuery<DynamicsEntity>(url);
        }

        public static async Task<DynamicsEntity> GetEntityMetadata(this IDynamicsClient dynamicsClient, string entityLogicalName)
        {
            var properties = string.Join(",", EntityProperties);
            var url = $"/EntityDefinitions(LogicalName='{entityLogicalName}')?$select={properties}";
            var entity = await dynamicsClient.ExecuteSingle<DynamicsEntity>(url);
            var attributes = await dynamicsClient.GetEntityAttributes(entityLogicalName);
            entity.Attributes = attributes.ToArray();
            return entity;
        }

        public static async Task<IEnumerable<DynamicsAttribute>> GetEntityAttributes(this IDynamicsClient dynamicsClient, string entityLogicalName)
        {
            var properties = string.Join(",", AttributeProperties);
            var attributes = $"/EntityDefinitions(LogicalName='{entityLogicalName}')/Attributes?$select={properties}&$filter=AttributeType ne Microsoft.Dynamics.CRM.AttributeTypeCode'Lookup' and AttributeType ne Microsoft.Dynamics.CRM.AttributeTypeCode'Picklist' and AttributeType ne Microsoft.Dynamics.CRM.AttributeTypeCode'Status' and AttributeType ne Microsoft.Dynamics.CRM.AttributeTypeCode'State'";
            var lookups = $"/EntityDefinitions(LogicalName='{entityLogicalName}')/Attributes/Microsoft.Dynamics.CRM.LookupAttributeMetadata?$select={properties},Targets";
            var picklists = $"/EntityDefinitions(LogicalName='{entityLogicalName}')/Attributes/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$select={properties}&$expand=OptionSet($select=Options),GlobalOptionSet($select=Options)";
            var status = $"/EntityDefinitions(LogicalName='{entityLogicalName}')/Attributes/Microsoft.Dynamics.CRM.StatusAttributeMetadata?$select={properties}&$expand=OptionSet($select=Options)";
            var state = $"/EntityDefinitions(LogicalName='{entityLogicalName}')/Attributes/Microsoft.Dynamics.CRM.StateAttributeMetadata?$select={properties}&$expand=OptionSet($select=Options)";
            var all = await dynamicsClient.ExecuteBatch<DynamicsAttribute[]>(attributes, lookups, picklists, status, state);
            return all.SelectMany(a => a);
        }

        public static async Task<IEnumerable<DynamicsOptionset>> GetAttributeOptions(this IDynamicsClient dynamicsClient, string entityLogicalName, string attributeLogicalName)
        {
            var url = $"/EntityDefinitions(LogicalName='{entityLogicalName}')/Attributes(LogicalName='{attributeLogicalName}')/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$select=LogicalName&$expand=OptionSet($select=Options),GlobalOptionSet($select=Options)";
            var attribute = await dynamicsClient.ExecuteSingle<DynamicsAttribute>(url);
            return attribute.Options;
        }
    }
}
