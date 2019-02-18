
// <copyright file="RecordHasNoteAttachment.cs" company="">
// Copyright (c) 2019 All Rights Reserved
// </copyright>
// <author></author>
// <date>2/18/2019 8:56:26 AM</date>
// <summary>Implements the RecordHasNoteAttachment Plugin.</summary>
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
// </auto-generated>
using System;
using System.Linq;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using KED365.Workflows.Helpers;
using Microsoft.Xrm.Sdk.Query;

namespace KED365.Workflows
{

    public class RecordHasNoteAttachment: WorkFlowActivityBase
    {

        [Input("Note Source Record")]
        public InArgument<string> SourceRecord { get; set; }

        [Output("HasAttachment")]
        public OutArgument<bool> HasAttachment { get; set; }

        [Output("IsSuccess")]
        public OutArgument<bool> IsSuccess { get; set; }

        [Output("Message")]
        public OutArgument<string> Message { get; set; }

        protected override void Execute(CodeActivityContext activityContext, IWorkflowContext workflowContext, IOrganizationService crmService,
            ITracingService tracingService)
        {
            try
            {
                // Get the Source Record URL
                string sourceRecordUrl = SourceRecord.Get(activityContext);

                if (string.IsNullOrEmpty(sourceRecordUrl) )
                {
                    Message.Set(activityContext, "Source Record URL cannot be empty or null");
                    return;
                }

                // Convert Record URL to ER
                EntityReference sourceRecordEr = EntityRecordUrlHelpers.ConvertRecordUrLtoEntityReference(sourceRecordUrl, crmService);
                if (sourceRecordEr == null)
                {
                    Message.Set(activityContext, "Failed to convert record URL into Entity Reference. Please make sure you provide valid record URL");
                    return;
                }

                var recordId = sourceRecordEr.Id;
                var annotation_isdocument = true;
                var query = new QueryExpression(sourceRecordEr.LogicalName);
                query.Distinct = true;
                query.ColumnSet.AddColumns("createdon");
                query.Criteria.AddCondition($"{sourceRecordEr.LogicalName}id", ConditionOperator.Equal, recordId);
                var queryAnnotation = query.AddLink("annotation", $"{sourceRecordEr.LogicalName}id", "objectid");
                queryAnnotation.EntityAlias = "aa";
                queryAnnotation.LinkCriteria.AddCondition("isdocument", ConditionOperator.Equal, annotation_isdocument);
                queryAnnotation.LinkCriteria.AddCondition("filename", ConditionOperator.NotNull);
                var results = crmService.RetrieveMultiple(query);

                if (!results.Entities.Any())
                {
                    // No attachments found
                    HasAttachment.Set(activityContext, false);
                    return;
                }

                // Attachments found
                HasAttachment.Set(activityContext, true);
                IsSuccess.Set(activityContext, true);
            }
            catch (Exception ex)
            {
                IsSuccess.Set(activityContext, false);
                Message.Set(activityContext, "An error occurred while check for attachments -" + ex.Message);
            }
        }
    }
}
