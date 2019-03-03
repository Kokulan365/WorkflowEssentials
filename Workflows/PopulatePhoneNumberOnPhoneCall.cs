using System;
using System.Collections.Generic;
using System.Linq;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;

namespace KED365.Workflows
{
    public class PopulatePhoneNumberOnPhoneCall : WorkFlowActivityBase
    {
        [RequiredArgument]
        [Input("Phone Call")]
        [ReferenceTarget("phonecall")]
        public InArgument<EntityReference> PhoneCall { get; set; }

        [Output("Success")] public OutArgument<bool> Success { get; set; }

        [Output("Message")] public OutArgument<string> Message { get; set; }

        protected override void Execute(CodeActivityContext activityContext, IWorkflowContext workflowContext,
            IOrganizationService CrmService, ITracingService trace)
        {
            try
            {
                EntityReference phoneCallRef = PhoneCall.Get(activityContext);

                if (phoneCallRef == null)
                {
                    Message.Set(activityContext, "Phone Call is Null or Empty");
                    return;
                }

                var phoneCallRecord = CrmService.Retrieve(phoneCallRef.LogicalName, phoneCallRef.Id,
                    new ColumnSet("to", "phonenumber"));

                if (phoneCallRecord.Attributes.Contains("to"))
                {
                    var to = phoneCallRecord.GetAttributeValue<EntityCollection>("to");
                    var phoneNumber = string.Empty;

                    foreach (var entity in to.Entities)
                    {
                        var listOfNumber = new List<string>();
                        var id = entity.Id;
                        if (entity.Contains("partyid"))
                        {
                            var contact = entity["partyid"] as EntityReference;
                            var QEcontact = new QueryExpression("contact");
                            QEcontact.ColumnSet.AddColumns("fullname", "contactid", "telephone1", "telephone3", "mobilephone", "home2", "telephone2", "address3_telephone3", "address3_telephone2", "address2_telephone2", "address2_telephone1", "address1_telephone3", "address1_telephone2", "address1_telephone1");
                            QEcontact.AddOrder("fullname", OrderType.Ascending);
                            QEcontact.Criteria.AddCondition("contactid", ConditionOperator.Equal, contact.Id);

                            var results = CrmService.RetrieveMultiple(QEcontact);

                            var result = results.Entities.First();

                            listOfNumber.Add(result.Contains("telephone2")
                                ? result.GetAttributeValue<string>("telephone2")
                                : string.Empty);
                            listOfNumber.Add(result.Contains("telephone1")
                                ? result.GetAttributeValue<string>("telephone1")
                                : string.Empty);
                            listOfNumber.Add(result.Contains("telephone3")
                                ? result.GetAttributeValue<string>("telephone3")
                                : string.Empty);
                            listOfNumber.Add(result.Contains("mobilephone")
                                ? result.GetAttributeValue<string>("mobilephone")
                                : string.Empty);
                            listOfNumber.Add(
                                result.Contains("home2") ? result.GetAttributeValue<string>("home2") : null);
                            listOfNumber.Add(result.Contains("address3_telephone3")
                                ? result.GetAttributeValue<string>("address3_telephone3")
                                : string.Empty);
                            listOfNumber.Add(result.Contains("address3_telephone2")
                                ? result.GetAttributeValue<string>("address3_telephone2")
                                : string.Empty);
                            listOfNumber.Add(result.Contains("address2_telephone2")
                                ? result.GetAttributeValue<string>("address2_telephone2")
                                : string.Empty);
                            listOfNumber.Add(result.Contains("address2_telephone1")
                                ? result.GetAttributeValue<string>("address2_telephone1")
                                : string.Empty);
                            listOfNumber.Add(result.Contains("address1_telephone3")
                                ? result.GetAttributeValue<string>("address1_telephone3")
                                : string.Empty);
                            listOfNumber.Add(result.Contains("address1_telephone2")
                                ? result.GetAttributeValue<string>("address1_telephone2")
                                : string.Empty);
                            listOfNumber.Add(result.Contains("address1_telephone1")
                                ? result.GetAttributeValue<string>("address1_telephone1")
                                : string.Empty);

                            var cleanListOfNumbers = listOfNumber.Where(x => !string.IsNullOrEmpty(x)).ToList();

                            if (cleanListOfNumbers.Any())
                                phoneNumber = string.Join(", ", cleanListOfNumbers.ToArray());
                        }

                        if (!string.IsNullOrEmpty(phoneNumber) &&
                            phoneNumber != phoneCallRecord.GetAttributeValue<string>("phonenumber"))
                        {
                            CrmService.Update(new Entity(phoneCallRef.LogicalName, phoneCallRef.Id)
                            {
                                ["phonenumber"] = phoneNumber
                            });
                        }
                    }
                }

                Success.Set(activityContext, true);
            }
            catch (Exception ex)
            {
                Message.Set(activityContext, $"Failed to populate phone number : {ex.Message}  -  {ex}");
            }
        }
    }
}
