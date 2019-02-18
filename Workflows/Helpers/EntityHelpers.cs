using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace KED365.Workflows.Helpers
{
    public static class EntityHelpers
    {
        public static Entity GetLastNoteByFileName(CodeActivityContext activityContext, IOrganizationService crmService, string fileName, EntityReference sourceRecordEr)
        {
            var qEannotationFilename = fileName;
            var qEannotationIncidentIncidentid = sourceRecordEr.Id;
            var qEannotation = new QueryExpression("annotation");
            qEannotation.ColumnSet = new ColumnSet(true);
            qEannotation.AddOrder("modifiedon", OrderType.Descending);
            qEannotation.Criteria.AddCondition("filename", ConditionOperator.Equal, qEannotationFilename);
            var qEannotationIncident = qEannotation.AddLink("incident", "objectid", "incidentid");
            qEannotationIncident.EntityAlias = "ab";
            qEannotationIncident.LinkCriteria.AddCondition("incidentid", ConditionOperator.Equal, qEannotationIncidentIncidentid);
            EntityCollection results = crmService.RetrieveMultiple(qEannotation);
            return results.Entities.FirstOrDefault();
        }
    }
}

