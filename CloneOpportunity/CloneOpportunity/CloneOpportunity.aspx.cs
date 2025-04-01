#define included_code
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Crm.Sdk.Utility;
using log4net;
using System.Linq;

namespace CloneOpportunity
{
    public partial class CloneOpportunity : System.Web.UI.Page
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CloneOpportunity));

        private IOrganizationService _service;
        private CrmServiceUtility _crmUtility;
        private string _configFile = string.Empty;
        private XmlDocument _doc = null;
        private bool _isFailed = false;
        private List<string> _errors = null;
        private bool _isHousingCompany = false;

        public string[] OpportunityIds
        {
            set { ViewState["OpporunitiIds"] = value; }
            get
            {
#if TESTDATA
                return new string[] { "F9C89FD3-D27C-DE11-B4CF-00012E0B81A2" };
#else
                return (string[])ViewState["OpporunitiIds"];
#endif
            }
        }

        public string OrganisationName
        {
            set { ViewState["Organisation"] = value; }
            get
            {
#if TESTDATA
                return "hotel-portal";
#else
                return ViewState["Organisation"]?.ToString();
#endif
            }
        }

        public string SelectedEntityName
        {
            set { ViewState["entityName"] = value; }
            get
            {
                return ViewState["entityName"]?.ToString() ?? "opportunity";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();

                if (!IsPostBack)
                {
                    ClientScript.GetPostBackEventReference(this, string.Empty);
                    ClientScript.RegisterForEventValidation(btnClone.ClientID, "FireCloneOnclick");

                    char[] separator = { ',' };
#if !TESTDATA
                    string idStringList = Request.QueryString["id"];
                    if (!string.IsNullOrEmpty(idStringList))
                    {
                        hdnEventId.Value = idStringList;
                        OpportunityIds = idStringList.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        throw new Exception("Entity id(s) was not provided!");
                    }

                    string entityName = Request.QueryString["typename"];
                    SelectedEntityName = !string.IsNullOrEmpty(entityName) ? entityName.Trim() : "opportunity";

                    string org = Request.QueryString["orgname"];
                    if (!string.IsNullOrEmpty(org))
                        OrganisationName = org.Trim();
#else
                    hdnEventId.Value = "02338B28-59E0-E111-B998-005056926F0E";
                    ViewState["OpporunitiIds"] = hdnEventId.Value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    ViewState["Organisation"] = "hotel-portal";
#endif
                    _crmUtility = new CrmServiceUtility(OrganisationName);
                    _service = _crmUtility.GetService();
                    if (!_crmUtility.IsValidCredentials())
                        throw new Exception("Failed to validate CRM credentials.");
                    _configFile = GetConfigFullPath();

                    LoadChildEntities();
                }
                else if (Request["__EVENTARGUMENT"] == "FireCloneOnclick")
                {
                    JavascriptCallToClone();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in Page_Load: {ex.Message}", ex);
                StoreErrors($"Unexpected error: {ex.Message}");
                DisplayMessage("An unexpected error occurred. Check event log or contact administrator.", false);
            }
        }

        private string GetConfigFullPath()
        {
            return System.IO.Path.Combine(Request.PhysicalApplicationPath, "CloneEntity.xml");
        }

        private int OppRelationship(IOrganizationService service, string entityName, string customerId)
        {
            var oppRelation = _crmUtility.FetchOpportunityRelationr(entityName, new[] { "opportunityid", "customerid" }, "", customerId);
            return oppRelation.Entities?.Count ?? 0;
        }

        protected List<ListItem> GetOneToManyRelationshipList(string entityName)
        {
            var oneToManyEntities = new List<ListItem>();
            var retrieveEntity = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Relationships,
                LogicalName = entityName
            };

            try
            {
                var result = (RetrieveEntityResponse)_service.Execute(retrieveEntity);
                foreach (var relatedEntity in result.EntityMetadata.OneToManyRelationships)
                {
                    oneToManyEntities.Add(new ListItem(relatedEntity.ReferencingEntity, relatedEntity.ReferencingAttribute));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve one-to-many relations of {entityName}", ex);
            }

            return oneToManyEntities;
        }

        public override void VerifyRenderingInServerForm(Control control) { }

        protected void btnClone_Click(object sender, EventArgs e)
        {
            DoTheCloning();
        }

        protected void JavascriptCallToClone()
        {
            btnClone.ToolTip = "we're in postback";
            btnClone.Enabled = true;
            btnClone.Style["display"] = "none";
            btnCancel.Style["display"] = "none";

            DoTheCloning();

            btnClone.Style["display"] = "inline";
            btnCancel.Style["display"] = "inline";
        }

        protected void DoTheCloning()
        {
            try
            {
                Guid housingCompanyId = Guid.Empty;

                _crmUtility = new CrmServiceUtility(OrganisationName);
                _service = _crmUtility.GetService();
                if (!_crmUtility.IsValidCredentials())
                    throw new Exception("Failed to validate CRM credentials.");
                _configFile = GetConfigFullPath();

                _doc = new XmlDocument();
                _doc.Load(_configFile);

                if (OpportunityIds.Length <= 0)
                {
                    DisplayMessage("No events selected for cloning. Please close the window and try again.", false);
                    return;
                }

                var selectedAttributes = trvClone.Nodes[0].ChildNodes;
                var oneToManyEntities = GetOneToManyRelationshipList("opportunity");
                var manyToManyEntities = new List<ListItem>();

                var retrieveEntity = new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.Relationships,
                    LogicalName = "opportunity"
                };
                var result = (RetrieveEntityResponse)_service.Execute(retrieveEntity);
                foreach (var relatedEntity in result.EntityMetadata.ManyToManyRelationships)
                {
                    var entity = new ListItem(relatedEntity.Entity2LogicalName, relatedEntity.IntersectEntityName);
                    entity.Attributes.Add("relationName", relatedEntity.SchemaName);
                    manyToManyEntities.Add(entity);
                }

                foreach (string opportunityId in OpportunityIds)
                {
                    try
                    {
                        Entity opportunity = _service.Retrieve("opportunity", new Guid(opportunityId), new ColumnSet(true));
                        if (opportunity.Attributes.Contains("bcmc_housingcompanyid") && opportunity["bcmc_housingcompanyid"] != null)
                        {
                            _isHousingCompany = true;
                            housingCompanyId = ((EntityReference)opportunity["bcmc_housingcompanyid"]).Id;
                        }

                        Guid clonedEntityId = CloneMainEntity(opportunity);

                        foreach (TreeNode childEntity in selectedAttributes)
                        {
                            bool isManyToMany = false;
                            string manyToManyTable = string.Empty;
                            EntityCollection entitiesToClone = null;
                            EntityMetadata childEntityAttributesMetadata = _crmUtility.RetrieveAttributes(SelectedEntityName);
                            string referenceColumn = GetReferingColumn(childEntity.Value, oneToManyEntities);

                            if (string.IsNullOrEmpty(referenceColumn))
                            {
                                manyToManyTable = GetRelationshipInfo(childEntity.Value, manyToManyEntities, false);
                                if (string.IsNullOrEmpty(manyToManyTable))
                                {
                                    logger.Info($"Cloning of child entity {childEntity.Value} skipped; no relationship with parent.");
                                    continue;
                                }
                                isManyToMany = true;
                            }

                            if (isManyToMany)
                            {
                                entitiesToClone = _crmUtility.RetrieveMultipleManyToManyRecord(manyToManyTable,
                                    childEntity.Value, "opportunity", "opportunityid", opportunityId, new[] { childEntity.Value + "id" });
                                string relationName = GetRelationshipInfo(childEntity.Value, manyToManyEntities, true);
                                CloneManyToManyChildEntity(entitiesToClone, childEntity.Value, clonedEntityId, relationName);
                            }
                            else
                            {
                                bool isStatecode = childEntityAttributesMetadata.Attributes.Any(attr => attr.LogicalName.ToLower() == "statecode");
                                entitiesToClone = _crmUtility.RetrieveMultipleDynamicEntityAllColumns(
                                    childEntity.Value, referenceColumn, opportunityId, isStatecode);

                                if (entitiesToClone.Entities.Count > 0)
                                    CloneOneToManyChildEntity(entitiesToClone, childEntity.Value, childEntityAttributesMetadata, clonedEntityId, referenceColumn, housingCompanyId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _isFailed = true;
                        StoreErrors($"Cloning of event failed for event id={opportunityId}. Details: {ex.Message}");
                        logger.Error($"Cloning of event failed for event id={opportunityId}", ex);
                    }
                }

                if (_isFailed)
                    DisplayMessage("Cloning of at least one event or its child failed. Check event log for details.", false);
                else
                {
                    btnClone.Visible = false;
                    btnCancel.Text = "Close";
                    DisplayMessage("Event has been cloned successfully!", true);
                }

                AddTracing("Cloning process completed!");
            }
            catch (Exception ex)
            {
                StoreErrors($"Unexpected error: {ex.Message}");
                logger.Error(ex.Message, ex);
                DisplayMessage("An unexpected error occurred. Check event log or contact administrator.", false);
            }
        }

        private void LoadChildEntities()
        {
            try
            {
                var nodeChildEntity = new TreeNode("Select All", "childentities");
                _doc = new XmlDocument();
                _doc.Load(_configFile);

                var entityTag = _doc.GetElementsByTagName("parentEntity");
                foreach (XmlNode entity in entityTag)
                {
                    if (entity.Attributes["name"]?.Value == SelectedEntityName)
                    {
                        foreach (XmlNode childEntity in entity.ChildNodes)
                        {
                            string childEntityName = childEntity.Attributes["name"].Value;
                            EntityMetadata childEntityMetadata = _crmUtility.RetrieveAttributes(childEntityName);
                            var relatedEntityNode = new TreeNode(
                                childEntityMetadata.DisplayName.UserLocalizedLabel.Label,
                                childEntityMetadata.LogicalName);
                            nodeChildEntity.ChildNodes.Add(relatedEntityNode);
                        }
                    }
                }
                trvClone.Nodes.Add(nodeChildEntity);
            }
            catch (Exception ex)
            {
                logger.Error($"Error loading child entities: {ex.Message}", ex);
                throw;
            }
        }

        private List<string> GetSelectedAttributes(TreeNodeCollection selectedNodes, bool isNotChildEntity)
        {
            var selectedAttributes = new List<string>();
            foreach (TreeNode node in selectedNodes)
            {
                if (isNotChildEntity && !node.Value.Equals("childentities"))
                    selectedAttributes.Add(node.Value);
                else
                    selectedAttributes.Add(node.Value);
            }
            return selectedAttributes;
        }

        private Dictionary<string, AttributeMetadata> BuildAttributeMetadataDictionary(IOrganizationService service, string entityName, EntityMetadata inEntityMetadata = null)
        {
            var entityMetadata = inEntityMetadata ?? _crmUtility.RetrieveAttributes(entityName);
            var dictAttribMeta = new Dictionary<string, AttributeMetadata>();
            foreach (var attribMeta in entityMetadata.Attributes)
                dictAttribMeta[attribMeta.LogicalName] = attribMeta;
            return dictAttribMeta;
        }

        private Guid CloneEntity(Entity sourceEntity, bool topLevelParentEntity = false, EntityMetadata entityMetadata = null,
            string parentEntityName = null, string parentColumnName = null, string newParentId = null)
        {
            try
            {
                entityMetadata = ((RetrieveEntityResponse)_service.Execute(new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.Attributes,
                    LogicalName = sourceEntity.LogicalName
                })).EntityMetadata;

                var destinationEntity = new Entity(sourceEntity.LogicalName);
                var attributes = BuildAttributeMetadataDictionary(_service, sourceEntity.LogicalName, entityMetadata);

                foreach (var attribute in sourceEntity.Attributes)
                {
                    if (attributes.ContainsKey(attribute.Key) && !_crmUtility.IsCloneable(attributes[attribute.Key]))
                        continue;

                    if (topLevelParentEntity && attribute.Key.EndsWith("name", StringComparison.OrdinalIgnoreCase))
                        destinationEntity[attribute.Key] = $"{attribute.Value} (Cloned)";
                    else if (attribute.Value is EntityReference entityRef)
                    {
                        if (attribute.Key.Equals($"{parentEntityName}id", StringComparison.OrdinalIgnoreCase) ||
                            attribute.Key.Equals(parentColumnName, StringComparison.OrdinalIgnoreCase))
                            destinationEntity[attribute.Key] = new EntityReference(parentEntityName, new Guid(newParentId));
                        else
                            destinationEntity[attribute.Key] = entityRef;
                    }
                    else
                        destinationEntity[attribute.Key] = attribute.Value;
                }

                return _service.Create(destinationEntity);
            }
            catch (Exception ex)
            {
                logger.Error($"Error cloning entity: {ex.Message}", ex);
                throw;
            }
        }

        private Guid CloneMainEntity(Entity entity)
        {
            return CloneEntity(entity, true);
        }

        private Guid CloneChildEntity(Entity childEntity, string childEntityName, EntityMetadata childEntityMetadata,
            Guid parentId, string parentColumnName, string parentEntityName, bool clone2ndGeneration = true)
        {
            Guid sourceChildEntityId = childEntity.GetAttributeValue<Guid>(childEntityName + "id");
            Guid newChildEntityGuid = CloneEntity(childEntity, false, childEntityMetadata,
                parentEntityName, parentColumnName, parentId.ToString());

            if (clone2ndGeneration)
            {
                var relatedEntities = GetOneToManyRelationshipList(childEntityName);
                CloneAnnotation(sourceChildEntityId, childEntityName, newChildEntityGuid, relatedEntities);
            }

            return newChildEntityGuid;
        }

        private Guid CloneLevelTwoChildEntity(Entity retrievedEntity, string entityName, EntityMetadata entityMetadata,
            string parentEntityName, Guid parentId, string parentColumnName)
        {
            return CloneChildEntity(retrievedEntity, entityName, entityMetadata, parentId, parentColumnName, parentEntityName, false);
        }

        private void CloneAnnotation(Guid masterParentId, string masterEntityName, Guid cloneParentId, List<ListItem> relatedEntities)
        {
            string childEntityName = "annotation";
            try
            {
                var entityMetadata = _crmUtility.RetrieveAttributes(childEntityName);
                string referenceColumn = GetReferingColumn(childEntityName, relatedEntities);

                if (string.IsNullOrEmpty(referenceColumn))
                {
                    logger.Info($"Entity {masterEntityName}: No child {childEntityName}s relationship.");
                    return;
                }

                bool isStatecode = entityMetadata.Attributes.Any(attr => attr.LogicalName.ToLower() == "statecode");
                var entitiesToClone = _crmUtility.RetrieveMultipleDynamicEntityAllColumns(
                    childEntityName, referenceColumn, masterParentId.ToString(), isStatecode);

                foreach (Entity dynamicEntity in entitiesToClone.Entities)
                {
                    try
                    {
                        CloneLevelTwoChildEntity(dynamicEntity, childEntityName, entityMetadata, masterEntityName, cloneParentId, referenceColumn);
                    }
                    catch (Exception ex)
                    {
                        _isFailed = true;
                        StoreErrors($"Cloning for one child entity of type {childEntityName} failed: {ex.Message}");
                        logger.Error($"Cloning for one child entity of type {childEntityName} failed", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _isFailed = true;
                StoreErrors($"Cloning for child entities {childEntityName} failed: {ex.Message}");
                logger.Error($"Cloning for child entities {childEntityName} failed", ex);
            }
        }

        private string GetReferingColumn(string childEntityName, List<ListItem> relatedEntities)
        {
            return relatedEntities.Find(lst => lst.Text == childEntityName)?.Value ?? string.Empty;
        }

        private string GetRelationshipInfo(string childEntityName, List<ListItem> relatedEntities, bool isReturnRelationName)
        {
            var entity = relatedEntities.Find(lst => lst.Text == childEntityName);
            return isReturnRelationName ? entity?.Attributes["relationName"] : entity?.Value ?? string.Empty;
        }

        private bool IsCreateChildFromEntity(string relatedEntity)
        {
            var entityTag = _doc.GetElementsByTagName("parentEntity");
            foreach (XmlNode entity in entityTag)
            {
                if (entity.Attributes["name"]?.Value == SelectedEntityName)
                {
                    foreach (XmlNode child in entity.ChildNodes)
                    {
                        if (child.Attributes["name"]?.Value == relatedEntity)
                            return child.Attributes["generateQuoteFromOpportunity"]?.Value == "true";
                    }
                }
            }
            return false;
        }

        private void CreateQuoteFromOpportunity(Guid opportunityId)
        {
            try
            {
                Entity opportunity = _service.Retrieve("opportunity", opportunityId, new ColumnSet("name", "customerid", "pricelevelid"));
                Entity quote = new Entity("quote")
                {
                    ["name"] = opportunity.GetAttributeValue<string>("name") + " (Generated)",
                    ["opportunityid"] = new EntityReference("opportunity", opportunityId),
                    ["customerid"] = opportunity.GetAttributeValue<EntityReference>("customerid"),
                    ["pricelevelid"] = opportunity.GetAttributeValue<EntityReference>("pricelevelid")
                };
                _service.Create(quote);
            }
            catch (Exception ex)
            {
                logger.Error($"Error creating quote from opportunity {opportunityId}: {ex.Message}", ex);
                throw;
            }
        }

        private void CreateInvoiceFromOpportunity(Guid opportunityId)
        {
            try
            {
                Entity opportunity = _service.Retrieve("opportunity", opportunityId, new ColumnSet("name", "customerid", "pricelevelid"));
                Entity invoice = new Entity("invoice")
                {
                    ["name"] = opportunity.GetAttributeValue<string>("name") + " (Generated)",
                    ["opportunityid"] = new EntityReference("opportunity", opportunityId),
                    ["customerid"] = opportunity.GetAttributeValue<EntityReference>("customerid"),
                    ["pricelevelid"] = opportunity.GetAttributeValue<EntityReference>("pricelevelid")
                };
                _service.Create(invoice);
            }
            catch (Exception ex)
            {
                logger.Error($"Error creating invoice from opportunity {opportunityId}: {ex.Message}", ex);
                throw;
            }
        }

        private void CreateSalesOrderFromOpportunity(Guid opportunityId)
        {
            try
            {
                Entity opportunity = _service.Retrieve("opportunity", opportunityId, new ColumnSet("name", "customerid", "pricelevelid"));
                Entity salesOrder = new Entity("salesorder")
                {
                    ["name"] = opportunity.GetAttributeValue<string>("name") + " (Generated)",
                    ["opportunityid"] = new EntityReference("opportunity", opportunityId),
                    ["customerid"] = opportunity.GetAttributeValue<EntityReference>("customerid"),
                    ["pricelevelid"] = opportunity.GetAttributeValue<EntityReference>("pricelevelid")
                };
                _service.Create(salesOrder);
            }
            catch (Exception ex)
            {
                logger.Error($"Error creating sales order from opportunity {opportunityId}: {ex.Message}", ex);
                throw;
            }
        }

        private string[] GetIgnoreFields(string entityName, string attributeName)
        {
            var entityTag = _doc.GetElementsByTagName("parentEntity");
            if (entityName == "opportunity")
            {
                return entityTag[0].Attributes[attributeName]?.Value.Split(',') ?? Array.Empty<string>();
            }

            foreach (XmlNode entity in entityTag)
            {
                if (entity.Attributes["name"]?.Value == SelectedEntityName)
                {
                    foreach (XmlNode child in entity.ChildNodes)
                    {
                        if (child.Attributes["name"]?.Value == entityName)
                        {
                            var value = child.Attributes[attributeName]?.Value;
                            return value?.Contains(",") == true ? value.Split(',') : new[] { value ?? string.Empty };
                        }
                    }
                }
            }
            return Array.Empty<string>();
        }

        private string GetConfigValues(string relatedEntity, string attributeName)
        {
            var entityTag = _doc.GetElementsByTagName("parentEntity");
            foreach (XmlNode entity in entityTag)
            {
                if (entity.Attributes["name"]?.Value == SelectedEntityName)
                {
                    foreach (XmlNode child in entity.ChildNodes)
                    {
                        if (child.Attributes["name"]?.Value == relatedEntity)
                            return child.Attributes[attributeName]?.Value ?? string.Empty;
                    }
                }
            }
            return string.Empty;
        }

        private List<string> GetChildForEntity(string rootEntity, string relatedEntity)
        {
            var childEntity = new List<string>();
            var entityTag = _doc.GetElementsByTagName("parentEntity");
            foreach (XmlNode entity in entityTag)
            {
                if (entity.Attributes["name"]?.Value == rootEntity)
                {
                    foreach (XmlNode child in entity.ChildNodes)
                    {
                        if (child.Attributes["name"]?.Value == relatedEntity)
                        {
                            foreach (XmlNode node in child.ChildNodes)
                                childEntity.Add(node.InnerText);
                            return childEntity;
                        }
                    }
                }
            }
            return childEntity;
        }

        private void DisplayMessage(string message, bool isSuccess)
        {
            if (isSuccess)
            {
                dvClone.Visible = false;
                lblErrorMessage.Text = message;
                lblErrorMessage.CssClass = "normalText";
            }
            else
            {
                if (ConfigurationManager.AppSettings["TraceToEventLog"]?.ToLower() == "true" && _errors != null)
                {
                    dvClone.Visible = false;
                    dvErrors.Visible = true;
                    gvError.DataSource = _errors;
                    gvError.DataBind();
                }
                lblErrorMessage.Text = message;
                lblErrorMessage.CssClass = "Errmsg";
            }
        }

        private void CloneOneToManyChildEntity(EntityCollection entitiesToClone, string entityName,
            EntityMetadata entityMetadata, Guid parentId, string referenceColumn, Guid housingCompanyId)
        {
            foreach (Entity entity in entitiesToClone.Entities)
            {
                try
                {
                    if (IsCreateChildFromEntity(entityName))
                    {
                        switch (entityName.ToLower())
                        {
                            case "quote":
                                CreateQuoteFromOpportunity(parentId);
                                break;
                            case "invoice":
                                CreateInvoiceFromOpportunity(parentId);
                                break;
                            case "salesorder":
                                CreateSalesOrderFromOpportunity(parentId);
                                break;
                        }
                    }
                    else if (entityName == "new_eventsite")
                    {
                        CloneChildEntity(entity, entityName, entityMetadata, parentId, referenceColumn, "opportunity");
                    }
                    else if (entity.Attributes.Contains("customerid") &&
                             ((EntityReference)entity["customerid"]).Id != housingCompanyId)
                    {
                        CloneChildEntity(entity, entityName, entityMetadata, parentId, referenceColumn, "opportunity");
                    }
                }
                catch (Exception ex)
                {
                    _isFailed = true;
                    StoreErrors($"Cloning of one child entity of type {entityName} failed for event id={parentId}: {ex.Message}");
                    logger.Error($"Cloning of one child entity of type {entityName} failed for event id={parentId}", ex);
                }
            }
        }

        private void CloneManyToManyChildEntity(EntityCollection entitiesToClone, string childEntity, Guid parentId, string relationshipName)
        {
            try
            {
                var request = new AssociateRequest
                {
                    Target = new EntityReference("opportunity", parentId),
                    Relationship = new Relationship(relationshipName),
                    RelatedEntities = new EntityReferenceCollection()
                };

                foreach (Entity dynamicEntity in entitiesToClone.Entities)
                    request.RelatedEntities.Add(new EntityReference(childEntity, dynamicEntity.Id));

                _service.Execute(request);
            }
            catch (Exception ex)
            {
                _isFailed = true;
                StoreErrors($"Cloning of one child entity (Many to Many) of type {childEntity} failed for event id={parentId}: {ex.Message}");
                logger.Error($"Cloning of one child entity (Many to Many) of type {childEntity} failed for event id={parentId}", ex);
            }
        }

        private void StoreErrors(string message)
        {
            if (ConfigurationManager.AppSettings["TraceToEventLog"]?.ToLower() == "true")
            {
                _errors = new List<string>();
                _errors.Add(message);
            }
        }

        private void AddTracing(string message)
        {
            if (ConfigurationManager.AppSettings["TraceToEventLog"]?.ToLower() == "true")
                logger.Info(message);
        }
    }
}