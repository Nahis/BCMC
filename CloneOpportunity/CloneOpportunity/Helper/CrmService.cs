//#define comment_out_is_false 
using System;
using Microsoft.Win32;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages; // Re-added for RetrieveEntityRequest/Response
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;
using log4net;

namespace Microsoft.Crm.Sdk.Utility
{
    public class CrmServiceUtility
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CrmServiceUtility));
        private readonly string _OrgName;
        private readonly IOrganizationService _service;

        public CrmServiceUtility(string orgName)
        {
            _OrgName = orgName;
            _service = CrmServiceManager.GetCrmService(); // Assumes this returns a 9.1-compatible service
            logger.Info($"CrmServiceUtility initialized for org: {_OrgName}");
        }

        public IOrganizationService GetService() => _service;

        public string GetConfigUrl()
        {
            try
            {
                using (RegistryKey regkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\MSCRM"))
                {
                    if (regkey == null) throw new Exception("MSCRM registry key not found.");
                    string serverUrl = regkey.GetValue("WebSitePath")?.ToString();
                    return serverUrl + @"\bin\CloneEntity.xml";
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error getting config URL: {ex.Message}", ex);
                throw;
            }
        }

        public EntityMetadata RetrieveAttributes(string entityName)
        {
            try
            {
                var req = new RetrieveEntityRequest
                {
                    LogicalName = entityName,
                    EntityFilters = EntityFilters.Attributes
                };
                var resp = (RetrieveEntityResponse)_service.Execute(req);
                logger.Info($"Retrieved attributes for entity: {entityName}");
                return resp.EntityMetadata;
            }
            catch (Exception ex)
            {
                logger.Error($"Error retrieving attributes for {entityName}: {ex.Message}", ex);
                throw;
            }
        }

        public EntityCollection RetrieveMultipleDynamicEntityAllColumns(string entityName, string filterCriteria, string filterValues, bool isApplyStatecodeFilter)
        {
            try
            {
                var query = new QueryExpression(entityName)
                {
                    ColumnSet = new ColumnSet(true)
                };
                var filter = new FilterExpression();

                filter.Conditions.Add(new ConditionExpression(filterCriteria, ConditionOperator.Equal, filterValues));

                if (isApplyStatecodeFilter)
                {
                    var stateCodeExpression = new ConditionExpression("statecode", ConditionOperator.Equal, 0);
                    switch (entityName.ToLower())
                    {
                        case "quote":
                        case "invoice":
                            stateCodeExpression.Operator = ConditionOperator.NotEqual;
                            stateCodeExpression.Values[0] = 3;
                            break;
                        case "salesorder":
                            stateCodeExpression.Operator = ConditionOperator.NotEqual;
                            stateCodeExpression.Values[0] = 2;
                            break;
                        case "serviceappointment":
                        case "appointment":
                            stateCodeExpression.Operator = ConditionOperator.In;
                            stateCodeExpression.Values.Clear();
                            stateCodeExpression.Values.AddRange(new object[] { 0, 3 });
                            break;
                    }
                    filter.Conditions.Add(stateCodeExpression);
                }

                query.Criteria = filter;
                EntityCollection result = _service.RetrieveMultiple(query);
                logger.Info($"Retrieved {result.Entities.Count} records for {entityName} with filter {filterCriteria}={filterValues}");
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Error retrieving multiple entities for {entityName}: {ex.Message}", ex);
                throw;
            }
        }

        public EntityCollection RetrieveMultipleDynamicEntityAllColumns(string entityName, string filterCriteria, string filterValues, bool isApplyStatecodeFilter, OrderExpression order)
        {
            try
            {
                var query = new QueryExpression(entityName)
                {
                    ColumnSet = new ColumnSet(true)
                };
                var filter = new FilterExpression();

                filter.Conditions.Add(new ConditionExpression(filterCriteria, ConditionOperator.Equal, filterValues));

                if (isApplyStatecodeFilter)
                {
                    var stateCodeExpression = new ConditionExpression("statecode", ConditionOperator.Equal, 0);
                    switch (entityName.ToLower())
                    {
                        case "quote":
                        case "invoice":
                            stateCodeExpression.Operator = ConditionOperator.NotEqual;
                            stateCodeExpression.Values[0] = 3;
                            break;
                        case "salesorder":
                            stateCodeExpression.Operator = ConditionOperator.NotEqual;
                            stateCodeExpression.Values[0] = 2;
                            break;
                        case "serviceappointment":
                        case "appointment":
                            stateCodeExpression.Operator = ConditionOperator.In;
                            stateCodeExpression.Values.Clear();
                            stateCodeExpression.Values.AddRange(new object[] { 0, 3 });
                            break;
                    }
                    filter.Conditions.Add(stateCodeExpression);
                }

                query.Criteria = filter;
                query.Orders.Add(order);
                EntityCollection result = _service.RetrieveMultiple(query);
                logger.Info($"Retrieved {result.Entities.Count} records for {entityName} with filter {filterCriteria}={filterValues}, ordered by {order.AttributeName}");
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Error retrieving multiple entities for {entityName} with order: {ex.Message}", ex);
                throw;
            }
        }

        public EntityCollection RetrieveMultipleManyToManyRecord(string manyToManyTable, string entityToRetrieve,
            string entityAsFilter, string filterCriteria, string filterValues, string[] columnSet)
        {
            try
            {
                var query = new QueryExpression(entityToRetrieve)
                {
                    ColumnSet = new ColumnSet(columnSet)
                };

                var leToRetrieve = new LinkEntity(entityToRetrieve, manyToManyTable,
                    entityToRetrieve + "id", entityToRetrieve + "id", JoinOperator.Inner);

                var leAsFilter = new LinkEntity(manyToManyTable, entityAsFilter,
                    filterCriteria, filterCriteria, JoinOperator.Inner)
                {
                    LinkCriteria = new FilterExpression()
                };
                leAsFilter.LinkCriteria.Conditions.Add(new ConditionExpression(filterCriteria, ConditionOperator.Equal, filterValues));

                leToRetrieve.LinkEntities.Add(leAsFilter);
                query.LinkEntities.Add(leToRetrieve);

                EntityCollection result = _service.RetrieveMultiple(query);
                logger.Info($"Retrieved {result.Entities.Count} many-to-many records for {entityToRetrieve}");
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Error retrieving many-to-many records: {ex.Message}", ex);
                throw;
            }
        }

        public void SetStateDynamicEntity(string entityName, Guid entityId, int entityState, int entityStatus)
        {
            try
            {
                var entity = new Entity(entityName) { Id = entityId };
                entity["statecode"] = new OptionSetValue(entityState);
                entity["statuscode"] = new OptionSetValue(entityStatus);
                _service.Update(entity);
                logger.Info($"Set state for {entityName} ID {entityId} to state={entityState}, status={entityStatus}");
            }
            catch (Exception ex)
            {
                logger.Error($"Error setting entity state for {entityName}: {ex.Message}", ex);
                throw;
            }
        }

        public bool IsCloneable(AttributeMetadata a)
        {
            bool isCloneable;

            switch (a.AttributeType.Value)
            {
                case AttributeTypeCode.Boolean:
                case AttributeTypeCode.DateTime:
                case AttributeTypeCode.Decimal:
                case AttributeTypeCode.Money:
                case AttributeTypeCode.Double:
                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.Integer:
                case AttributeTypeCode.String:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Owner:
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Uniqueidentifier:
                case AttributeTypeCode.Memo:
                    isCloneable = true;
                    break;
                case AttributeTypeCode.Status:
                case AttributeTypeCode.State:
                    isCloneable = false;
                    break;
                default:
                    isCloneable = false;
                    break;
            }

            return isCloneable && a.IsValidForRead.Value && a.IsValidForCreate.Value && !a.IsPrimaryId.Value;
        }

        public bool IsValidCredentials()
        {
            try
            {
                var userId = GetCurrentUserId();
                bool isValid = userId != Guid.Empty;
                logger.Info($"Credential validity result is: {isValid}");
                return isValid;
            }
            catch (Exception ex)
            {
                logger.Error($"Error checking credential validity: {ex.Message}", ex);
                return false;
            }
        }

        public EntityCollection FetchOpportunityRelationr(string eventName, string[] columnSet, string eventId, string customerId)
        {
            try
            {
                var query = new QueryExpression("customeropportunityrole")
                {
                    ColumnSet = new ColumnSet(columnSet),
                    Criteria = new FilterExpression()
                };
                query.Criteria.Conditions.Add(new ConditionExpression("customerid", ConditionOperator.Equal, customerId));

                EntityCollection result = _service.RetrieveMultiple(query);
                logger.Info($"Fetched {result.Entities.Count} opportunity relations for customer {customerId}");
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Error fetching opportunity relations: {ex.Message}", ex);
                throw;
            }
        }

        public Guid GetCurrentUserId()
        {
            try
            {
                var request = new OrganizationRequest("WhoAmI");
                var response = _service.Execute(request);
                Guid userId = response.Results.Contains("UserId") ? (Guid)response.Results["UserId"] : Guid.Empty;
                logger.Info($"Current user ID retrieved: {userId}");
                return userId;
            }
            catch (Exception ex)
            {
                logger.Error($"Error getting current user ID: {ex.Message}", ex);
                return Guid.Empty;
            }
        }
    }
}