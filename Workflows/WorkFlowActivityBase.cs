﻿// <copyright file="WorkFlowActivityBase.cs" company="">
// Copyright (c) 2019 All Rights Reserved
// </copyright>
// <author></author>
// <date>2/18/2019 8:34:42 AM</date>
// <summary>Implements the WorkFlowActivityBase Workflow Activity.</summary>
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
// </auto-generated>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Runtime.Serialization;

namespace KED365.Workflows
{
    public abstract class WorkFlowActivityBase : CodeActivity
    {
        protected abstract void Execute(CodeActivityContext activityContext, IWorkflowContext workflowContext,
            IOrganizationService orgService, ITracingService tracingService);

        protected sealed override void Execute(CodeActivityContext activityContext)
        {
            // Create the tracing service
            ITracingService tracingService = activityContext.GetExtension<ITracingService>();

            if (tracingService == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve tracing service.");
            }

            tracingService.Trace("Entered QualifyLeads.Execute(), Activity Instance Id: {0}, Workflow Instance Id: {1}",
                activityContext.ActivityInstanceId,
                activityContext.WorkflowInstanceId);

            // Create the context
            IWorkflowContext workflowContext = activityContext.GetExtension<IWorkflowContext>();

            if (workflowContext == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve workflow context.");
            }

            tracingService.Trace("QualifyLeads.Execute(), Correlation Id: {0}, Initiating User: {1}",
                workflowContext.CorrelationId, workflowContext.InitiatingUserId);

            IOrganizationServiceFactory serviceFactory = activityContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);

            try
            {
                Execute(activityContext, workflowContext, service, tracingService);
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                tracingService.Trace("Exception: {0}", e.ToString());
                throw;
            }

            tracingService.Trace("Exiting QualifyLeads.Execute(), Correlation Id: {0}", workflowContext.CorrelationId);
        }
    }
}