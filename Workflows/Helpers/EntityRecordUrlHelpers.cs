using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata.Query;

namespace KED365.Workflows.Helpers
{
    public static class EntityRecordUrlHelpers
    {
        public static string GetQueryStringParameter(string url, string parameterName)
        {
            int startOfParametersIndex = url.LastIndexOf('?');
            string parameters = url.Substring(startOfParametersIndex);
            string[] parametersArray = parameters.Split('&');
            foreach (string parameter in parametersArray)
            {
                if (parameter.Contains(parameterName + '='))
                {
                    string[] keyValueParameter = parameter.Split('=');
                    if (keyValueParameter.Length == 2)
                        return keyValueParameter[1];
                    else
                        return string.Empty;
                }
            }
            return null;
        }

        public static string GetEntityNameFromEtc(int etc, IOrganizationService service)
        {
            EntityQueryExpression metadataQuery = new EntityQueryExpression();
            metadataQuery.Properties = new MetadataPropertiesExpression();
            metadataQuery.Properties.PropertyNames.Add("LogicalName");
            metadataQuery.Criteria.Conditions.Add(new MetadataConditionExpression("ObjectTypeCode", MetadataConditionOperator.Equals, etc));

            var retrieveMetadataChangesRequest = new RetrieveMetadataChangesRequest
            {
                Query = metadataQuery,
                ClientVersionStamp = null,
                DeletedMetadataFilters = DeletedMetadataFilters.OptionSet
            };

            RetrieveMetadataChangesResponse response = service.Execute(retrieveMetadataChangesRequest) as RetrieveMetadataChangesResponse;
            if (response.EntityMetadata.Count > 0)
                return response.EntityMetadata[0].LogicalName;
            else
                throw new Exception(string.Format("Entity with Object Type Code '{0}' couldn't be found", etc));

        }

        public static EntityReference ConvertRecordUrLtoEntityReference(string recordContextRecordUrl, IOrganizationService organizationService)
        {
            Uri validatedRecordContextRecordUrl;

            if (Uri.TryCreate(recordContextRecordUrl, UriKind.Absolute, out validatedRecordContextRecordUrl))
            {
                const string guidStart = "%7b";
                const string guidEnd = "%7d";
                string id = GetQueryStringParameter(recordContextRecordUrl, "id").Replace(guidStart, string.Empty).Replace(guidEnd, string.Empty);
                string etn = GetQueryStringParameter(recordContextRecordUrl, "etn");

                if (string.IsNullOrEmpty(etn))
                {
                    string etc = GetQueryStringParameter(recordContextRecordUrl, "etc");
                    int etcInteger;

                    if (int.TryParse(etc, out etcInteger))
                    {
                        etn = GetEntityNameFromEtc(etcInteger, organizationService);
                    }
                    else
                        throw new ArgumentException($"Record Context URL has an unexpected etc parameter. Object Type Code '{etc}' must be an integer");

                }
                return new EntityReference(etn, new Guid(id));
            }

            throw new ArgumentException($"Invalid Record Context URL '{recordContextRecordUrl}'");
        }

    }
}
