using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services.Protocols;
using Microsoft.Crm.Sdk.Utility;
using System.Xml;
using System.Configuration;
using Xrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using log4net;

// TestData
// &typename=opportunity&orgname=BCMC
// &hdnallGuids=,,,

namespace CloneOpportunity
{
    public partial class CloneOpportunity : System.Web.UI.Page
    {
		private static ILog logger = LogManager.GetLogger(typeof(CloneOpportunity));

        private OrganizationService _service;
        CrmServiceUtility _crmUtility = null;
		string _confingFile = string.Empty; //TODO

        XmlDocument _doc = null;
        Boolean _isFailed = false;
        List<string> _errors = null;
        Boolean _IsHousingCompany = false;

        public string[] OpportunityIds
        {
            set
            {
                ViewState["OpporunitiIds"] = value;
            }
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
            set
            {
                ViewState["Organisation"] = value;
            }
            get
            {
#if TESTDATA
                // ViewState["Organisation"] = "Hotel-Portal";
#else
				return ViewState["Organisation"].ToString();
#endif
			}
        }

        public string SelectedEntityName
        {
            /*set
            {
                ViewState["entityName"] = value;
            }*/
            get
            {
				ViewState["entityName"] = Opportunity.EntityLogicalName;
                return ViewState["entityName"].ToString();
				//return Opportunity.EntityLogicalName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    char[] seperator = { ',' };
#if !TESTDATA
                    hdnEventId.Value = Request.Form["hdnallGuids"];
                    ViewState["OpporunitiIds"] = hdnEventId.Value.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
                    ViewState["Organisation"] = Request.QueryString["orgname"].ToString().Trim();
                    ViewState["entityName"] = Request.QueryString["typename"].Trim();
#else
                    //hdnEventId.Value = "7FF24115-C8D0-E111-B997-005056926F0E";
                    hdnEventId.Value = "02338B28-59E0-E111-B998-005056926F0E";
                    ViewState["OpporunitiIds"] = hdnEventId.Value.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
                    //ViewState["Organisation"] = "sentri";
                    //ViewState["entityName"] = "opportunity";
                    ViewState["Organisation"] = "hotel-portal";
                    //ViewState["OpporunitiIds"] = "{01744A32-5B74-DF11-8ADE-00012E0B81A2}";
#endif
					//Initialize the CRM Web Service
                    _crmUtility = new CrmServiceUtility(OrganisationName);
					_service = CrmService2011.GetCrmService();
                    _confingFile = _crmUtility.GetConfigUrl();

                    //Load Related child entities
                    LoadChildEntities();
                    /*
					string _opportunityid = ViewState["OpporunitiIds"].ToString();
                    string _entityname = ViewState["entityName"].ToString();
					 */
                }
            }
            catch (SoapException ex)
            {
                log.logError(ex.Detail.InnerText);

                //store errors
                StoreErrors("Some unexpected error has occured.Details are " + ex.Detail.InnerText);

                DisplayMessage("Some unexpected error has occured, please check event log for details or contact administrator", false);
            }
            catch (Exception ex)
            {
                log.logError(ex.Message);

                //store errors
                StoreErrors("Some unexpected error has occured.Details are " + ex.Message);

                DisplayMessage("Some unexpected error has occured, please check event log for details or contact administrator", false);
            }

        }

		private int OppRelationship(OrganizationService _service, string _entityname, string _customerId)
        {
            EntityCollection oppRelation = _crmUtility.FetchOpportunityRelationr(_service, 
				Opportunity.EntityLogicalName, new string[] { 
				"opportunityid", 
				"customerid" }, 
				"", 
				_customerId);

            int count = 0;
            if (oppRelation.Entities != null)
                count = oppRelation.Entities.Count;
            return count;
        }

        /// <summary>
        /// Clones the user selected entity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnClone_Click(object sender, EventArgs e)
        {
            try
            {
                Guid _housingCompanyId = new Guid();

				_crmUtility = new CrmServiceUtility(OrganisationName);
				_service = CrmService2011.GetCrmService();
				_confingFile = _crmUtility.GetConfigUrl();

                //for testing purpose
                _crmUtility.isValidCredentials(_service);

                _doc = new XmlDocument();
                _doc.Load(_confingFile);

                //Check if user has selected at least one opportunity for cloning
                if (OpportunityIds.Length <= 0)
                {
                    DisplayMessage("There are no events selected for cloning, Please close window to try again.", false);
                    return;
                }

                //get user selected child entities to clone
                TreeNodeCollection selectedAttributes = trvClone.Nodes[0].ChildNodes;

                //Get the metadata for current entity
                //ytodo do we need this?
				//MetaService.EntityMetadata entityMetadata = _crmUtility.RetrieveAttributes(_service, SelectedEntityName);

                //for (int i = 0; i < entityMetadata.Attributes.Length; i++)
                //{
                //    if ((entityMetadata.Attributes[i]).SchemaName == "bcmc_housingcompanyid")
                //    {
                //    }
                //}

                //Clone the opportunity attribute for each opportunity and for its child entity
                foreach (string opportunityId in OpportunityIds)
                {
                    try
                    {
                        // Be aware that using AllColumns may adversely affect
                        // performance and cause unwanted cascading in subsequent 
                        // updates. A best practice is to retrieve the least amount of 
                        // data required.
						Entity opportunity = _service.Retrieve(
							Opportunity.EntityLogicalName, 
							new Guid(opportunityId), 
							new ColumnSet(true));

						if (opportunity.Attributes.Contains("bcmc_housingcompanyid"))
						{
							_IsHousingCompany = true;
							_housingCompanyId = (opportunity.Attributes["bcmc_housingcompanyid"] as Guid?).Value;
						}

                        Guid clonedEntityId = CloneMainEntity(opportunity);

                        AddTracing("Getting all related child entities and theire referencing columns(One To many relation)");
                        //Get all related child entities and theire referencing columns(One To many relation)
                        List<ListItem> oneToManyEntities = GetRelatedOneToManyInfo(SelectedEntityName);
						//ytodo check this out
						foreach (KeyValuePair<Relationship, EntityCollection>  relationshipCollection in opportunity.RelatedEntities)
						{
							Relationship r = relationshipCollection.Key;
							foreach (Entity entity in relationshipCollection.Value.Entities)
							{
								//r
							}
						}

                        AddTracing("Get all related child entities and theire referencing columns(many To many relation)");
                        //Get all related child entities and theire referencing columns(many To many relation)
                        List<ListItem> manyToManyEntities = GetRelatedManyToManyInfo(SelectedEntityName);

                        //Create clone of each selected child entity
                        foreach (TreeNode childEntity in selectedAttributes)
                        {
                            if (childEntity.Checked == true)
                            {
                                try
                                {
                                    //Flag to check if many to many relationship
                                    Boolean isManyToMany = false;

                                    //Table name for mant to many relationship
                                    string manyToManyTable = string.Empty;

                                    //array to store child entities
                                    BusinessEntity[] entitiesToClone = null;


                                    //Rretrieve all attributes for child entity
                                    MetaService.EntityMetadata childEntityMetadata = _crmUtility.RetrieveAttributes(_service, childEntity.Value);

                                    //Gets reference column name for given child entity (One to many)
                                    string referenceColumn = GetReferingColumn(childEntity.Value, oneToManyEntities);


                                    //if child entity is not having reference entity column the skip cloning of current child entity
                                    if (referenceColumn.Trim().Equals(string.Empty))
                                    {
                                        //Check if child entity is having many to many relationship
                                        manyToManyTable = GetRelationshipInfo(childEntity.Value, manyToManyEntities, false);
                                        if (manyToManyTable.Trim().Equals(string.Empty))
                                        {
                                            log.logInfo("Cloning of child enity-:" + childEntity.Value + " skipped, as it is not having any relashionship with parent entity.");
                                            continue;
                                        }
                                        else
                                        {
                                            //Set relation is many to many for this child entity
                                            isManyToMany = true;
                                        }
                                    }

                                    //check raeltion type between parent and child entity
                                    if (isManyToMany == true)
                                    {
                                        //get the all child entities for current opportunity  for current selected child entity type  
                                        entitiesToClone = _crmUtility.RetrieveMultipleManyToManyRecord(_service, manyToManyTable, childEntity.Value, SelectedEntityName, SelectedEntityName + "id", opportunityId.ToString(), new[] { childEntity.Value + "id" });

                                        string relationName = GetRelationshipInfo(childEntity.Value, manyToManyEntities, true);

                                        //Clone many to many records current child entity
                                        CloneManyToManyChildEntity(entitiesToClone, childEntity.Value, clonedEntityId, relationName);
                                    }
                                    else
                                    {
                                        //Checks if entity has state code attribute
                                        Boolean isStatecode = false;
                                        for (int y = 0; y < childEntityMetadata.Attributes.Length; y++)
                                        {
                                            if (childEntityMetadata.Attributes[y].LogicalName.ToLower() == "statecode")
                                            {

                                                isStatecode = true;
                                                break;
                                            }
                                        }

                                        //get the all child entities for current opportunity  for current selected child entity type  
                                        entitiesToClone = _crmUtility.RetrieveMultipleDynamicEntityAllColumns(_service, childEntity.Value, referenceColumn, opportunityId.ToString(), isStatecode);

                                        //if (!_IsHousingCompany)
                                        //clone all records for current selected child enity

                                        CloneOneToManyChildEntity(entitiesToClone, childEntity.Value, childEntityMetadata, clonedEntityId, referenceColumn, _housingCompanyId);
                                    }
                                }
                                catch (SoapException ex)
                                {
                                    _isFailed = true;

                                    //Store exact error mesages
                                    StoreErrors("Cloning of child entities-:" + childEntity.Value + " failed for event id=" + opportunityId + ".Details are " + ex.Detail.InnerText);

                                    //log the error if cloning of any child entity type fails
                                    log.logError("Cloning of child entities-:" + childEntity.Value + " failed for event id=" + opportunityId + ".Details are " + ex.Detail.InnerText);

                                }
                                catch (Exception ex)
                                {
                                    _isFailed = true;

                                    //Store exact error mesages
                                    StoreErrors("Cloning of child entities-:" + childEntity.Value + " failed for event id=" + opportunityId + ".Details are " + ex.Message);

                                    //log the error if cloning of any child entity type fails
                                    log.logError("Cloning of child entities-:" + childEntity.Value + " failed for event id=" + opportunityId + ".Details are " + ex.Message);
                                }
                            }
                        }
                    }
                    catch (SoapException ex)
                    {
                        _isFailed = true;

                        //Store exact error mesages
                        StoreErrors("Cloning of event failed for event id=" + opportunityId + ".Details are " + ex.Detail.InnerText);

                        //log the error if cloning of any child entity type fails
                        log.logError("Cloning of event failed for event id=" + opportunityId + ".Details are " + ex.Detail.InnerText);
						log.logError(ex.ToString());
                    }
                    catch (Exception ex)
                    {
                        _isFailed = true;

                        //Store exact error mesages
                        StoreErrors("Cloning of event failed for event id=" + opportunityId + ".Details are " + ex.Message);

                        //log the error if cloning of any child entity type fails
                        log.logError("Cloning of event failed for event id=" + opportunityId + ".Details are " + ex.Message);
						log.logError(ex.ToString());
                    }
                }


                //Check if all entities has been cloned succesfully and display message to user
                if (_isFailed == true)
                {
                    DisplayMessage("Cloning of at least one event or its child has been failed, please refer event log for details.", false);
                }
                else
                {
                    btnClone.Visible = false;
                    btnCancel.Text = "Close";
                    DisplayMessage("Event has been cloned successfully!!", true);
                }


                AddTracing("Cloning process completed!!!");
            }
            catch (SoapException ex)
            {
                //Store exact error mesages
                StoreErrors("Some unexpected error has occured.Details are: " + ex.Detail.InnerText);

                log.logError(ex.Detail.InnerText);
                DisplayMessage("Some unexpected error has occured, please check event log for details or contact administrator", false);
            }
            catch (Exception ex)
            {
                //Store exact error mesages
                StoreErrors("Some unexpected error has occured, Details are" + ex.Message);

                log.logError(ex.Message);
                DisplayMessage("Some unexpected error has occured, please check event log for details or contact administrator", false);
            }
        }

        /// <summary>
        /// Load child entities and their attributes
		/// to the tree view
        /// </summary>
        private void LoadChildEntities()
        {
            try
            {
                //create new node for child entities
                TreeNode nodeChildEntity = new TreeNode("Select All", "childentities");

                //Create the XmlDocument.
                _doc = new XmlDocument();
                _doc.Load(_confingFile);

				XmlNodeList entityTag = _doc.GetElementsByTagName("parentEntity");
				//ytodo: use "parentEntity[name='opportunity']" instead above
                for (int i = 0; i < entityTag.Count; i++)
                {
					// Filter out the requested parent_entity (in url) only,
 					// with its related child entities;
					// and ignore the other parent_entities in the config file.
                    if (entityTag[i].Attributes["name"].Value.Equals(SelectedEntityName))
                    {
                        XmlNodeList childEntityTag = entityTag[i].ChildNodes;
                        for (int j = 0; j < childEntityTag.Count; j++)
                        {
                            //Get All attributes for child entity
							string childEntityName = childEntityTag[j].Attributes["name"].Value;
							EntityMetadata childEnityMetadata = MetadataUtility.GetEntityMetadata(_service,
								childEntityName);

                            //Create new node to insert into tree for child entity
							TreeNode RelatedEntity_TreeNode = new TreeNode(
								childEnityMetadata.DisplayName.UserLocalizedLabel.Label.ToString(),
								childEnityMetadata.LogicalName.ToString());

                            // Get the childs of main root node and add related enity node into it    
							nodeChildEntity.ChildNodes.Add(RelatedEntity_TreeNode);
                        }
                    }

                    // Insert to TreeView
                    trvClone.Nodes.Add(nodeChildEntity);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the selected attributes for entity
        /// </summary>
        /// <param name="selectedNodes"></param>
        /// <returns></returns>
        private List<string> GetSelectedAttributes(TreeNodeCollection selectedNodes, Boolean isNotChildEntity)
        {
            List<string> selectedAttributes = new List<string>();
            try
            {
                foreach (TreeNode node in selectedNodes)
                {
                    //Need to avoid attributes of child entities node
                    if (isNotChildEntity)
                    {
                        //Get the user selected attributes
                        if (node.Checked && !node.Value.Equals("childentities"))
                        {
                            selectedAttributes.Add(node.Value);
                        }
                    }
                    else
                    {
                        //Get the user selected attributes
                        if (node.Checked)
                        {
                            selectedAttributes.Add(node.Value);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return selectedAttributes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityName"></param>
        /// <param name="entityMetadata"></param>
        /// <returns></returns>
        private Guid CloneMainEntity(Entity entity)
        {
            Guid cloneEntityGuid;
            string[] ignoreColumns;

            try
            {
                ignoreColumns = GetIgnoreFields(entity.LogicalName, "ignoreField");

				// Remove unwanted attributes from clone (Set in config file CloneEntity.xml)
				foreach (string attrib in ignoreColumns)
				{
					entity.Attributes[attrib] = null;
				}

				// Probably not necessary...
				entity.Attributes[entity.LogicalName + "id"] = null;

				// New name is 'orig_name (Cloned)'
				if (entity.Attributes.Contains("name"))
				{
					entity.Attributes["name"] = String.Format("{0} (1)", 
						entity.Attributes["name"] as string, 
						ConfigurationManager.AppSettings["CloneEntitySuffix"] as string);
				}
				
				cloneEntityGuid = _service.Create(entity);

                // If opportunity is in Open state then copy the statuscode
				if (entity.Attributes.Contains("statuscode"))
				{
					string statuscode = entity.FormattedValues["statuscode"];
					if (null != statuscode && statuscode.ToLower() == "open")
					{
						// This may have been done automatically during the clone...to check...ytodo
						_crmUtility.SetStateDynamicEntity(_service, entity.LogicalName, cloneEntityGuid, 
							(entity.Attributes["statuscode"] as OptionSetValue).Value, 
							1);
					}
				}
            }
            catch (SoapException ex)
            {
				logger.Error(String.Format("CloneMainEntity: {0}", ex.Message), ex);
                throw ex;
            }
            catch (Exception ex)
            {
				logger.Error(String.Format("CloneMainEntity: {0}", ex.Message), ex);
				throw ex;
            }

            return cloneEntityGuid;
        }

        /// <summary>
        /// Makes the clone of given entity object for selected attributes
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityName"></param>
        /// <param name="entityIdColumnName"></param>
        /// <param name="selectedAttributes"></param>
        /// <returns></returns>
        private Guid CloneChildEntity(DynamicEntity retrievedEntity, string entityName, MetaService.EntityMetadata entityMetadata,
            Guid parentID, string parentColumnName)
        {

            //stores guid for master enity guid( entity from which we are cloning attribues)
            Guid masterEntityId = new Guid();

            //Stores guid of newly cretaed enity
            Guid cloneEnityGuid = Guid.Empty;

            //Commented not required 20090218
            //set the status of newly created entity
            //Store the status info of child entity
            //int mainEntityStatus = 0;
            //string mainEntityState = string.Empty;
            string[] ignoreColumns = null;
            try
            {


                DynamicEntity newEntity = new DynamicEntity();
                newEntity.Name = entityName;

                List<Property> cloneProperties = new List<Property>();

                ignoreColumns = GetIgnoreFields(entityName, "ignoreField");
                bool donotAdd = false;
                for (int ui = 0; ui < retrievedEntity.Properties.Length; ui++)
                {

                    donotAdd = false;
                    if (ignoreColumns != null)
                    {
                        for (int i = 0; i < ignoreColumns.Length; i++)
                        {
                            if (ignoreColumns[i] == retrievedEntity.Properties[ui].Name)
                            {
                                donotAdd = true;
                                break;
                            }
                        }
                    }

                    if (donotAdd == false)
                    {

                        for (int y = 0; y < entityMetadata.Attributes.Length; y++)
                        {
                            if (entityMetadata.Attributes[y].LogicalName != null)
                            {
                                if (entityMetadata.Attributes[y].LogicalName == "bcmc_housingcompanyid")
                                {
                                }
                                if (entityMetadata.Attributes[y].LogicalName == retrievedEntity.Properties[ui].Name)
                                {

                                    if (_crmUtility.IsCloneable(entityMetadata.Attributes[y]))
                                    {
                                        // Set the properties of the contact.


                                        switch (retrievedEntity.Properties[ui].GetType().Name)
                                        {
                                            case "StringProperty":

                                                StringProperty stringProperty = null;

                                                //If property is name then append clonetext to cloned entity
                                                if (retrievedEntity.Properties[ui].Name.Equals(GetConfigValues(entityName, "entityNameField")))
                                                {
                                                    string cloneSuffix = ConfigurationManager.AppSettings["CloneEntitySuffix"];
                                                    StringProperty mainStringProperty = ((StringProperty)retrievedEntity.Properties[ui]);
                                                    string clonedName = mainStringProperty.Value + " (" + cloneSuffix + ")";
                                                    stringProperty = _crmUtility.GetStringProperty(retrievedEntity.Properties[ui].Name, clonedName);
                                                }
                                                else
                                                {
                                                    stringProperty = _crmUtility.GetStringProperty(retrievedEntity.Properties[ui].Name, ((StringProperty)retrievedEntity.Properties[ui]).Value);
                                                }
                                                cloneProperties.Add(stringProperty);
                                                break;
                                            case "CrmMoneyProperty":
                                                CrmMoneyProperty moneyProperty = _crmUtility.GetCrmMoneyProperty(retrievedEntity.Properties[ui].Name, ((CrmMoneyProperty)retrievedEntity.Properties[ui]).Value.Value);
                                                cloneProperties.Add(moneyProperty);
                                                break;
                                            case "CrmDecimalProperty":
                                                CrmDecimalProperty decimalProperty = _crmUtility.GetCrmDecimalProperty(retrievedEntity.Properties[ui].Name, ((CrmDecimalProperty)retrievedEntity.Properties[ui]).Value.Value);
                                                cloneProperties.Add(decimalProperty);
                                                break;
                                            case "CrmFloatProperty":
                                                CrmFloatProperty floatProperty = _crmUtility.GetCrmFloatProperty(retrievedEntity.Properties[ui].Name, ((CrmFloatProperty)retrievedEntity.Properties[ui]).Value.Value);
                                                cloneProperties.Add(floatProperty);
                                                break;
                                            case "CrmBooleanProperty":
                                                CrmBooleanProperty booleanProperty = _crmUtility.GetCrmBooleanProperty(retrievedEntity.Properties[ui].Name, ((CrmBooleanProperty)retrievedEntity.Properties[ui]).Value.Value);
                                                cloneProperties.Add(booleanProperty);
                                                break;
                                            case "CrmDateTimeProperty":
                                                CrmDateTimeProperty datetimeProperty = _crmUtility.GetCrmDateTimeProperty(retrievedEntity.Properties[ui].Name, ((CrmDateTimeProperty)retrievedEntity.Properties[ui]).Value.Value);
                                                cloneProperties.Add(datetimeProperty);
                                                break;
                                            case "LookupProperty":

                                                LookupProperty lookupProperty = null;
                                                //Check if its parent reference column the assign GUID of parent entity
                                                if (retrievedEntity.Properties[ui].Name.Equals(parentColumnName))
                                                {
                                                    lookupProperty = _crmUtility.GetLookupProperty(retrievedEntity.Properties[ui].Name, parentID, SelectedEntityName);

                                                }
                                                else
                                                {
                                                    lookupProperty = _crmUtility.GetLookupProperty(retrievedEntity.Properties[ui].Name, ((LookupProperty)retrievedEntity.Properties[ui]).Value.Value, SelectedEntityName);
                                                }

                                                cloneProperties.Add(lookupProperty);

                                                break;
                                            case "KeyProperty":
                                                KeyProperty keyProperty = _crmUtility.GetKeyProperty(retrievedEntity.Properties[ui].Name, ((KeyProperty)retrievedEntity.Properties[ui]).Value.Value);
                                                cloneProperties.Add(keyProperty);


                                                break;
                                            case "OwnerProperty":
                                                OwnerProperty mainOwnerProperty = (OwnerProperty)retrievedEntity.Properties[ui];
                                                OwnerProperty ownerProperty = _crmUtility.GetOwnerProperty(retrievedEntity.Properties[ui].Name, mainOwnerProperty.Value.Value, mainOwnerProperty.Value.type);
                                                cloneProperties.Add(ownerProperty);
                                                break;
                                            case "CustomerProperty":
                                                CustomerProperty mainCustomerProperty = (CustomerProperty)retrievedEntity.Properties[ui];
                                                CustomerProperty customer = _crmUtility.GetCustomerProperty(retrievedEntity.Properties[ui].Name, mainCustomerProperty.Value.Value, mainCustomerProperty.Value.type);

                                                cloneProperties.Add(customer);
                                                break;
                                            case "CrmNumberProperty":
                                                CrmNumberProperty mainNumberProperty = (CrmNumberProperty)retrievedEntity.Properties[ui];
                                                CrmNumberProperty number = _crmUtility.GetCrmNumberProperty(retrievedEntity.Properties[ui].Name, mainNumberProperty.Value.Value);

                                                cloneProperties.Add(number);
                                                break;

                                            case "PicklistProperty":
                                                PicklistProperty mainPicklistProperty = (PicklistProperty)retrievedEntity.Properties[ui];
                                                PicklistProperty pickList = _crmUtility.GetPicklistProperty(retrievedEntity.Properties[ui].Name, mainPicklistProperty.Value.Value);

                                                cloneProperties.Add(pickList);
                                                break;



                                        }
                                        //Event 

                                    }
                                    else
                                    {


                                        //If current property is StateProperty/StatusProperty then get the status for current enity
                                        //this status will be used set the status of newly created cloned enity

                                        switch (retrievedEntity.Properties[ui].GetType().Name)
                                        {

                                            //Commented not required 20090218
                                            //set the status of newly created entity

                                            //case "StateProperty":
                                            //    if (retrievedEntity.Properties[ui].Name.Equals("statecode"))
                                            //    {
                                            //        StateProperty stateProperty = (StateProperty)retrievedEntity.Properties[ui];
                                            //        mainEntityState = stateProperty.Value;
                                            //    }
                                            //    break;
                                            //case "StatusProperty":
                                            //    if (retrievedEntity.Properties[ui].Name.Equals("statuscode"))
                                            //    {
                                            //        StatusProperty statusProperty = (StatusProperty)retrievedEntity.Properties[ui];
                                            //        mainEntityStatus = statusProperty.Value.Value;
                                            //    }
                                            //    break;

                                            case "KeyProperty":
                                                //Need to get oportunity id of master id) as it is not clonable will have to retrieve here
                                                //Check if key column with guid(Primary key)
                                                if (retrievedEntity.Properties[ui].Name.Equals(entityName + "id"))
                                                {
                                                    KeyProperty mainKeyProperty = (KeyProperty)(retrievedEntity.Properties[ui]);
                                                    masterEntityId = mainKeyProperty.Value.Value;

                                                    //KeyProperty keyProperty = _crmUtility.GetKeyProperty(retrievedEntity.Properties[ui].Name, ((KeyProperty)retrievedEntity.Properties[ui]).Value.Value);
                                                    //cloneProperties.Add(keyProperty);

                                                }
                                                break;
                                        }


                                    }

                                }
                            }
                        }
                    }
                }




                //If cloning entity is annotation(note) then add additional column
                if (entityName.Equals(EntityName.annotation.ToString()))
                {
                    EntityNameReferenceProperty refProperty = new EntityNameReferenceProperty();
                    refProperty.Name = "objecttypecode";
                    EntityNameReference entRef = new EntityNameReference();
                    entRef.Value = SelectedEntityName;
                    refProperty.Value = entRef;
                    cloneProperties.Add(refProperty);

                }




                //assign cloned property to new entity
                newEntity.Properties = cloneProperties.ToArray();

                //Create new entity
                TargetCreateDynamic target = new TargetCreateDynamic();
                target.Entity = newEntity;
                CreateRequest createReq = new CreateRequest();
                createReq.Target = target;
                CreateResponse createResp = (CreateResponse)_service.Execute(createReq);
                cloneEnityGuid = createResp.id;


                //cloneEnityGuid = _service.Create(newEntity);

                //Commented not required 20090218
                //set the status of newly created entity
                //_crmUtility.SetStateDynamicEntity(_service, entityName, cloneEnityGuid, mainEntityState, mainEntityStatus);


                //Checks if current child entity has child defined in config file
                List<string> childEntity = GetChildForEntity(SelectedEntityName, entityName);


                //get related entities for entity
                List<ListItem> relatedEntities = GetRelatedOneToManyInfo(entityName);


                //creates child enity for current enity
                //foreach (string childEntityName in childEntity)
                //{                    
                //    //clone the all child entity
                Clone("", masterEntityId, entityName, cloneEnityGuid, relatedEntities);
                //}


            }

            catch (SoapException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return cloneEnityGuid;
        }

        /// <summary>
        /// Makes the clone of given entity object for selected attributes
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityName"></param>
        /// <param name="entityIdColumnName"></param>
        /// <param name="selectedAttributes"></param>
        /// <returns></returns>
        private Guid CloneLevelTwoChildEntity(DynamicEntity retrievedEntity, string entityName, MetaService.EntityMetadata entityMetadata, string parentEntityName, Guid parentID, string parentColumnName)
        {

            //Commented not required 20090218
            //set the status of newly created entity
            //Store the status info of child entity
            //int mainEntityStatus = 0;
            //string mainEntityState = string.Empty;
            string[] ignoreColumns = null;
            Guid cloneEnityGuid;
            try
            {


                DynamicEntity newEntity = new DynamicEntity();
                newEntity.Name = entityName;

                List<Property> cloneProperties = new List<Property>();
                ignoreColumns = GetIgnoreFields(entityName, "ignoreField");
                bool donotAdd = false;
                for (int ui = 0; ui < retrievedEntity.Properties.Length; ui++)
                {
                    donotAdd = false;
                    if (ignoreColumns != null)
                    {
                        for (int i = 0; i < ignoreColumns.Length; i++)
                        {
                            if (ignoreColumns[i] == retrievedEntity.Properties[ui].Name)
                            {
                                donotAdd = true;
                                break;
                            }
                        }
                    }

                    if (donotAdd == false)
                    {
                        for (int y = 0; y < entityMetadata.Attributes.Length; y++)
                        {
                            if (entityMetadata.Attributes[y].LogicalName != null)
                            {
                                if (entityMetadata.Attributes[y].LogicalName == retrievedEntity.Properties[ui].Name)
                                {

                                    if (_crmUtility.IsCloneable(entityMetadata.Attributes[y]))
                                    {
                                        // Set the properties of the contact.


                                        switch (retrievedEntity.Properties[ui].GetType().Name)
                                        {
                                            case "StringProperty":

                                                StringProperty stringProperty = null;

                                                //If property is name then append clonetext to cloned entity
                                                if (retrievedEntity.Properties[ui].Name.Equals(GetConfigValues(entityName, "entityNameField")))
                                                {
                                                    string cloneSuffix = ConfigurationManager.AppSettings["CloneEntitySuffix"];
                                                    StringProperty mainStringProperty = ((StringProperty)retrievedEntity.Properties[ui]);
                                                    string clonedName = mainStringProperty.Value + " (" + cloneSuffix + ")";
                                                    stringProperty = _crmUtility.GetStringProperty(retrievedEntity.Properties[ui].Name, clonedName);
                                                }
                                                else
                                                {
                                                    stringProperty = _crmUtility.GetStringProperty(retrievedEntity.Properties[ui].Name, ((StringProperty)retrievedEntity.Properties[ui]).Value);
                                                }
                                                cloneProperties.Add(stringProperty);
                                                break;
                                            case "CrmMoneyProperty":
                                                CrmMoneyProperty moneyProperty = _crmUtility.GetCrmMoneyProperty(retrievedEntity.Properties[ui].Name, ((CrmMoneyProperty)retrievedEntity.Properties[ui]).Value.Value);
                                                cloneProperties.Add(moneyProperty);
                                                break;
                                            case "CrmDecimalProperty":
                                                CrmDecimalProperty decimalProperty = _crmUtility.GetCrmDecimalProperty(retrievedEntity.Properties[ui].Name, ((CrmDecimalProperty)retrievedEntity.Properties[ui]).Value.Value);
                                                cloneProperties.Add(decimalProperty);
                                                break;
                                            case "CrmFloatProperty":
                                                CrmFloatProperty floatProperty = _crmUtility.GetCrmFloatProperty(retrievedEntity.Properties[ui].Name, ((CrmFloatProperty)retrievedEntity.Properties[ui]).Value.Value);
                                                cloneProperties.Add(floatProperty);
                                                break;
                                            case "CrmBooleanProperty":
                                                CrmBooleanProperty booleanProperty = _crmUtility.GetCrmBooleanProperty(retrievedEntity.Properties[ui].Name, ((CrmBooleanProperty)retrievedEntity.Properties[ui]).Value.Value);
                                                cloneProperties.Add(booleanProperty);
                                                break;
                                            case "CrmDateTimeProperty":
                                                CrmDateTimeProperty datetimeProperty = _crmUtility.GetCrmDateTimeProperty(retrievedEntity.Properties[ui].Name, ((CrmDateTimeProperty)retrievedEntity.Properties[ui]).Value.Value);
                                                cloneProperties.Add(datetimeProperty);
                                                break;
                                            case "LookupProperty":

                                                LookupProperty lookupProperty = null;
                                                //Check if its parent reference column the assign GUID of parent entity
                                                if (retrievedEntity.Properties[ui].Name.Equals(parentColumnName))
                                                {
                                                    lookupProperty = _crmUtility.GetLookupProperty(retrievedEntity.Properties[ui].Name, parentID, parentEntityName);

                                                }
                                                else
                                                {
                                                    lookupProperty = _crmUtility.GetLookupProperty(retrievedEntity.Properties[ui].Name, ((LookupProperty)retrievedEntity.Properties[ui]).Value.Value, parentEntityName);
                                                }

                                                cloneProperties.Add(lookupProperty);

                                                break;
                                            case "KeyProperty":
                                                KeyProperty keyProperty = _crmUtility.GetKeyProperty(retrievedEntity.Properties[ui].Name, ((KeyProperty)retrievedEntity.Properties[ui]).Value.Value);
                                                cloneProperties.Add(keyProperty);
                                                break;
                                            case "OwnerProperty":
                                                OwnerProperty mainOwnerProperty = (OwnerProperty)retrievedEntity.Properties[ui];
                                                OwnerProperty ownerProperty = _crmUtility.GetOwnerProperty(retrievedEntity.Properties[ui].Name, mainOwnerProperty.Value.Value, mainOwnerProperty.Value.type);
                                                cloneProperties.Add(ownerProperty);
                                                break;
                                            case "CustomerProperty":
                                                CustomerProperty mainCustomerProperty = (CustomerProperty)retrievedEntity.Properties[ui];
                                                CustomerProperty customer = _crmUtility.GetCustomerProperty(retrievedEntity.Properties[ui].Name, mainCustomerProperty.Value.Value, mainCustomerProperty.Value.type);

                                                cloneProperties.Add(customer);
                                                break;
                                            case "CrmNumberProperty":
                                                CrmNumberProperty mainNumberProperty = (CrmNumberProperty)retrievedEntity.Properties[ui];
                                                CrmNumberProperty number = _crmUtility.GetCrmNumberProperty(retrievedEntity.Properties[ui].Name, mainNumberProperty.Value.Value);

                                                cloneProperties.Add(number);
                                                break;

                                            case "PicklistProperty":
                                                PicklistProperty mainPicklistProperty = (PicklistProperty)retrievedEntity.Properties[ui];
                                                PicklistProperty pickList = _crmUtility.GetPicklistProperty(retrievedEntity.Properties[ui].Name, mainPicklistProperty.Value.Value);

                                                cloneProperties.Add(pickList);
                                                break;


                                        }


                                    }
                                    else
                                    {


                                        //Commented not required 20090218

                                        //If current property is StateProperty/StatusProperty then get the status for current enity
                                        //this status will be used set the status of newly created cloned enity

                                        //switch (retrievedEntity.Properties[ui].GetType().Name)
                                        //{
                                        //    case "StateProperty":
                                        //        if (retrievedEntity.Properties[ui].Name.Equals("statecode"))
                                        //        {
                                        //            StateProperty stateProperty = (StateProperty)retrievedEntity.Properties[ui];
                                        //            mainEntityState = stateProperty.Value;
                                        //        }
                                        //        break;
                                        //    case "StatusProperty":
                                        //        if (retrievedEntity.Properties[ui].Name.Equals("statuscode"))
                                        //        {
                                        //            StatusProperty statusProperty = (StatusProperty)retrievedEntity.Properties[ui];
                                        //            mainEntityStatus = statusProperty.Value.Value;
                                        //        }
                                        //        break;

                                        //}
                                    }
                                }
                            }
                        }
                    }
                }



                //If cloning entity is annotation(note) then add additional column
                if (entityName.Equals(EntityName.annotation.ToString()))
                {
                    EntityNameReferenceProperty refProperty = new EntityNameReferenceProperty();
                    refProperty.Name = "objecttypecode";
                    EntityNameReference entRef = new EntityNameReference();
                    entRef.Value = parentEntityName;
                    refProperty.Value = entRef;
                    cloneProperties.Add(refProperty);

                }

                //assign cloned property to new entity
                newEntity.Properties = cloneProperties.ToArray();

                //Create new entity
                TargetCreateDynamic target = new TargetCreateDynamic();
                target.Entity = newEntity;
                CreateRequest createReq = new CreateRequest();
                createReq.Target = target;
                CreateResponse createResp = (CreateResponse)_service.Execute(createReq);
                cloneEnityGuid = createResp.id;

                //Commented not required 20090218
                //set the status of newly created entity
                //_crmUtility.SetStateDynamicEntity(_service, entityName, cloneEnityGuid, mainEntityState, mainEntityStatus);


            }

            catch (SoapException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return cloneEnityGuid;
        }

        /// <summary>
        /// Clones the child enity of all child entities
        /// </summary>
        /// <param name="enityName"></param>
        /// <param name="masterParentId"></param>
        /// <param name="cloneParentId"></param>
        private void Clone(string childEnityName, Guid masterParentId, string masterEntityName, Guid cloneParentId, List<ListItem> relatedEntities)
        {

            try
            {
                childEnityName = EntityName.annotation.ToString();
                //Rretrieve all attributes for child entity
                MetaService.EntityMetadata entityMetadata = _crmUtility.RetrieveAttributes(_service, childEnityName);

                //Gets reference column name for given child entity
                string referenceColumn = GetReferingColumn(childEnityName, relatedEntities);

                //if child entity is not having reference entity column the skip cloning of current child entity
                if (referenceColumn.Equals(string.Empty))
                {
                    log.logInfo("Cloning of child enity-:" + childEnityName + " has failed, as it is not having any relashionship with parent entity.");
                    return;
                }



                //Checks if entity has state code attribute
                Boolean isStatecode = false;
                for (int y = 0; y < entityMetadata.Attributes.Length; y++)
                {
                    if (entityMetadata.Attributes[y].LogicalName.ToLower() == "statecode")
                    {

                        isStatecode = true;
                        break;
                    }
                }



                //get the all child entities for current opportunity  for current selected child entity type  
                BusinessEntity[] entitiesToClone = _crmUtility.RetrieveMultipleDynamicEntityAllColumns(_service, childEnityName, referenceColumn, masterParentId.ToString(), isStatecode);

                //Clone each child entity
                foreach (BusinessEntity beEntity in entitiesToClone)
                {

                    try
                    {

                        //this case does not copy related enity of child entity
                        DynamicEntity dynamicEntity = (DynamicEntity)beEntity;
                        CloneLevelTwoChildEntity(dynamicEntity, childEnityName, entityMetadata, masterEntityName, cloneParentId, referenceColumn);

                    }
                    catch (SoapException ex)
                    {
                        _isFailed = true;

                        //Store errors
                        StoreErrors("Cloning for one child entity of type -:" + childEnityName + " failed.Details are " + ex.Detail.InnerText);

                        //log the error if cloning of any child entity type fails
                        log.logError("Cloning for one child entity of type -:" + childEnityName + " failed.Details are " + ex.Detail.InnerText);

                    }
                    catch (Exception ex)
                    {
                        _isFailed = true;

                        //store errors
                        StoreErrors("Cloning for one child entity of type -:" + childEnityName + " failed.Details are " + ex.Message);

                        //log the error if cloning of any child entity type fails
                        log.logError("Cloning for one child entity of type -:" + childEnityName + " failed.Details are " + ex.Message);
                    }


                }


            }
            catch (SoapException ex)
            {
                _isFailed = true;

                //store error
                StoreErrors("Cloning for child entities-:" + childEnityName + " failed.Details are " + ex.Detail.InnerText);

                //log the error if cloning of any child entity type fails
                log.logError("Cloning for child entities-:" + childEnityName + " failed.Details are " + ex.Detail.InnerText);

            }
            catch (Exception ex)
            {
                _isFailed = true;

                //store error
                StoreErrors("Cloning for child entity-:" + childEnityName + " failed.Details are " + ex.Message);

                //log the error if cloning of any child entity type fails
                log.logError("Cloning for child entity-:" + childEnityName + " failed.Details are " + ex.Message);
            }
        }

        /// <summary>
        /// Gets child entities for currently selected entity and add them in list
        /// </summary>
        private List<ListItem> GetRelatedOneToManyInfo(string entityName)
        {
            List<ListItem> oneToManyData = null;
            try
            {
                AddTracing("About to retrieve RelationMetadata for entity");
                //retrieve all attributes for opportunity enity
                MetaService.EntityMetadata entityRelationMetadata = _crmUtility.RetrieveRelatedAttributes(_service, entityName);

                //retrieve all attributes for related enity
                oneToManyData = new List<ListItem>();

                AddTracing("Getting One to many relation for opportunity");
                //gets the one to many related information for opportunity
                foreach (MetaService.OneToManyMetadata relatedEnity in entityRelationMetadata.OneToManyRelationships)
                {
                    //Add all related enity and theire columns in list
                    ListItem entity = new ListItem(relatedEnity.ReferencingEntity, relatedEnity.ReferencingAttribute);
                    //insert each item in list to use it later
                    oneToManyData.Add(entity);
                }

                AddTracing("Completed GetRelatedOneToManyInfo");

            }
            catch (SoapException ex)
            {

                throw ex;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return oneToManyData;
        }

        /// <summary>
        /// Gets Many to many relation information for cloning entity
        /// </summary>
        private List<ListItem> GetRelatedManyToManyInfo(string entityName)
        {
            AddTracing("Inside GetRelatedManyToManyInfo");

            List<ListItem> manyToManyData = null;

            try
            {
                AddTracing("Getting relation data for entity");

                //retrieve all relation attribute for opportunity enity
                MetaService.EntityMetadata entityRelationMetadata = _crmUtility.RetrieveRelatedAttributes(_service, entityName);

                //retrieve all attributes for related enity
                manyToManyData = new List<ListItem>();


                AddTracing("Fetching Many to many realtions for entity");
                //gets the many to many related information for opportunity
                foreach (MetaService.ManyToManyMetadata relatedEnity in entityRelationMetadata.ManyToManyRelationships)
                {
                    //Add all related enity and theire columns in list
                    ListItem entity = new ListItem(relatedEnity.Entity2LogicalName, relatedEnity.IntersectEntityName);

                    //add relationshipname as attribute
                    entity.Attributes.Add("relationName", relatedEnity.SchemaName);

                    //insert each item in list to use it later
                    manyToManyData.Add(entity);
                }
                AddTracing("Completed GetRelatedManyToManyInfo");
            }
            catch (SoapException ex)
            {

                throw ex;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return manyToManyData;
        }

        /// <summary>
        /// Returns referencing column name for given child entity
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        private string GetReferingColumn(string childEntityName, List<ListItem> relatedEnities)
        {
            try
            {
                foreach (ListItem lstEntity in relatedEnities)
                {
                    if (lstEntity.Text.Equals(childEntityName))
                    {
                        return lstEntity.Value;
                    }

                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return string.Empty;
        }


        /// <summary>
        /// Returns referencing column name or ralationshipname for given child entity
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        private string GetRelationshipInfo(string childEntityName, List<ListItem> relatedEnities, Boolean isReturnRelationName)
        {
            try
            {
                foreach (ListItem lstEntity in relatedEnities)
                {
                    if (lstEntity.Text.Equals(childEntityName))
                    {

                        if (isReturnRelationName == true)
                        {
                            return lstEntity.Attributes["relationName"];
                        }
                        else
                        {
                            return lstEntity.Value;
                        }
                    }

                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relatedEntity"></param>
        /// <returns></returns>
        private Boolean IsCreateChildFromEntity(string relatedEntity)
        {
            try
            {

                XmlNodeList entityTag = _doc.GetElementsByTagName("parentEntity");

                for (int i = 0; i < entityTag.Count; i++)
                {
                    if (entityTag[i].Attributes["name"].Value.Equals(SelectedEntityName))
                    {
                        XmlNodeList childEntityTag = entityTag[i].ChildNodes;
                        for (int j = 0; j < childEntityTag.Count; j++)
                        {
                            if (childEntityTag[j].Attributes["name"].Value.Equals(relatedEntity))
                            {
                                if (childEntityTag[j].Attributes["generateQuoteFromOpportunity"] != null)
                                {
                                    if (childEntityTag[j].Attributes["generateQuoteFromOpportunity"].Value.Equals("true"))
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opportunityId"></param>
        private void GenerateQuoteFromOpportunity(Guid opportunityId)
        {
            try
            {
                // Create the request.
                GenerateQuoteFromOpportunityRequest generate = new GenerateQuoteFromOpportunityRequest();

                // Determine the columns that will be transferred.
                generate.ColumnSet = new AllColumns();

                // OpportunityId is the GUID of the opportunity that generates the quote.
                generate.OpportunityId = opportunityId;

                // Execute the request.
                GenerateQuoteFromOpportunityResponse generated = (GenerateQuoteFromOpportunityResponse)_service.Execute(generate);

            }
            catch (SoapException ex)
            {

                throw ex;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opportunityId"></param>
        private void GenerateInvoiceFromOpportunity(Guid opportunityId)
        {
            try
            {
                // Create the request.
                GenerateInvoiceFromOpportunityRequest generate = new GenerateInvoiceFromOpportunityRequest();

                // Determine the columns that will be transferred.
                generate.ColumnSet = new AllColumns();

                // OpportunityId is the GUID of the opportunity that generates the quote.
                generate.OpportunityId = opportunityId;

                // Execute the request.
                GenerateInvoiceFromOpportunityResponse generated = (GenerateInvoiceFromOpportunityResponse)_service.Execute(generate);

            }
            catch (SoapException ex)
            {

                throw ex;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opportunityId"></param>
        private void GenerateSalesOrderFromOpportunity(Guid opportunityId)
        {
            try
            {
                // Create the request.
                GenerateSalesOrderFromOpportunityRequest generate = new GenerateSalesOrderFromOpportunityRequest();

                // Determine the columns that will be transferred.
                generate.ColumnSet = new AllColumns();

                // OpportunityId is the GUID of the opportunity that generates the quote.
                generate.OpportunityId = opportunityId;

                // Execute the request.
                GenerateSalesOrderFromOpportunityResponse generated = (GenerateSalesOrderFromOpportunityResponse)_service.Execute(generate);

            }
            catch (SoapException ex)
            {

                throw ex;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private string[] GetIgnoreFields(string entityName, string attributeName)
        {
            string[] ignoreColumns = null;
            XmlNodeList entityTag = null;
            try
            {
                entityTag = _doc.GetElementsByTagName("parentEntity");
                if (entityName == "opportunity")
                {
                    if (entityTag[0].Attributes[attributeName] != null)
                    {
                        ignoreColumns = entityTag[0].Attributes[attributeName].Value.ToString().Split(',');
                    }
                }
                else
                {
                    for (int i = 0; i < entityTag.Count; i++)
                    {
                        if (entityTag[i].Attributes["name"].Value.Equals(SelectedEntityName))
                        {
                            XmlNodeList childEntityTag = entityTag[i].ChildNodes;
                            for (int c = 0; c < childEntityTag.Count; c++)
                            {
                                if (childEntityTag[c].Attributes["name"].Value == entityName)
                                {
                                    if (childEntityTag[c].Attributes[attributeName] != null)
                                    {
                                        if (childEntityTag[c].Attributes[attributeName].Value.ToString().Contains(","))
                                        {
                                            ignoreColumns = childEntityTag[c].Attributes[attributeName].Value.ToString().Split(',');
                                        }
                                        else
                                        {
                                            ignoreColumns = new string[1];
                                            ignoreColumns[0] = childEntityTag[c].Attributes[attributeName].Value.ToString();
                                        }
                                    }
                                }
                                //childEntityTag[0].Attributes["name"] .Value 
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ignoreColumns;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relatedEntity"></param>
        /// <returns></returns>
        private string GetConfigValues(string relatedEntity, string attributeName)
        {
            try
            {

                XmlNodeList entityTag = _doc.GetElementsByTagName("parentEntity");

                for (int i = 0; i < entityTag.Count; i++)
                {
                    if (entityTag[i].Attributes["name"].Value.Equals(SelectedEntityName))
                    {
                        XmlNodeList childEntityTag = entityTag[i].ChildNodes;
                        for (int j = 0; j < childEntityTag.Count; j++)
                        {
                            if (childEntityTag[j].Attributes["name"].Value.Equals(relatedEntity))
                            {
                                if (childEntityTag[j].Attributes[attributeName] != null)
                                {
                                    return childEntityTag[j].Attributes[attributeName].Value;

                                }
                                return string.Empty;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return string.Empty;
        }

        /// <summary>
        /// Retreivs child for entity from config file
        /// </summary>
        /// <param name="relatedEntity"></param>
        /// <returns></returns>
        private List<string> GetChildForEntity(string rootEntity, string relatedEntity)
        {
            List<string> childEntity = new List<string>();
            try
            {

                XmlNodeList entityTag = _doc.GetElementsByTagName("parentEntity");

                for (int i = 0; i < entityTag.Count; i++)
                {
                    if (entityTag[i].Attributes["name"].Value.Equals(rootEntity))
                    {
                        XmlNodeList childEntityTag = entityTag[i].ChildNodes;
                        for (int j = 0; j < childEntityTag.Count; j++)
                        {
                            if (childEntityTag[j].Attributes["name"].Value.Equals(relatedEntity))
                            {
                                foreach (XmlNode node in childEntityTag[j].ChildNodes)
                                {
                                    childEntity.Add(node.InnerText);//new_name
                                }
                                // childEntity.Add("notescontrol");
                                return childEntity;
                            }
                        }
                    }
                }

                return childEntity;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Display messages to user
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isSuccess"></param>
        private void DisplayMessage(string message, Boolean isSuccess)
        {
            if (isSuccess)
            {
                dvClone.Visible = false;
                lblErrorMessage.Text = message;
                lblErrorMessage.CssClass = "normalText";


            }
            else
            {
				if (ConfigurationManager.AppSettings["TraceToEventLog"].ToLower().Equals("true"))
                {

                    if (_errors != null)
                    {
                        dvClone.Visible = false;
                        dvErrors.Visible = true;
                        gvError.DataSource = _errors;
                        gvError.DataBind();
                    }
                }

                lblErrorMessage.Text = message;
                lblErrorMessage.CssClass = "Errmsg";
            }
        }

        /// <summary>
        /// Clones records of selected child enity(One to many)
        /// </summary>
        /// <param name="entitiesToClone"></param>
        /// <param name="enitityName"></param>
        /// <param name="entityMetadata"></param>
        /// <param name="parentId"></param>
        /// <param name="referenceColumn"></param>
        /// 


        private void CloneOneToManyChildEntity(BusinessEntity[] entitiesToClone, string enitityName, MetaService.EntityMetadata entityMetadata, Guid parentId,
            string referenceColumn, Guid _housingCompanyId)
        {
            Guid _customerId = Guid.Empty;
            //Clone each child entity
            foreach (BusinessEntity beEntity in entitiesToClone)
            {

                try
                {

                    //checks if child entity is to be directly copied from opportunity
                    //this case copies all the related child enities of child entity
                    if (IsCreateChildFromEntity(enitityName))
                    {

                        switch (enitityName)
                        {

                            case "quote":
                                GenerateQuoteFromOpportunity(parentId);
                                break;
                            case "invoice":
                                GenerateInvoiceFromOpportunity(parentId);
                                break;
                            case "salesorder":
                                GenerateSalesOrderFromOpportunity(parentId);
                                break;
                        }

                    }
                    else
                    {
                        DynamicEntity dynamicEntity = (DynamicEntity)beEntity;
                        //Add By ZSL Team on 30_Jan_2013

                        if (enitityName == "new_eventsite")
                        {
                            CloneChildEntity(dynamicEntity, enitityName, entityMetadata, parentId, referenceColumn);
                        }
                        else
                        {

                            for (int i = 0; i < dynamicEntity.Properties.Length; i++)
                            {
                                if (dynamicEntity.Properties[i].Name == "customerid")
                                {
                                    if (((CustomerProperty)dynamicEntity.Properties[i]).Value.Value == _housingCompanyId)
                                    {
                                        _IsHousingCompany = true;
                                        //LookupProperty lookupProperty = _crmUtility.GetLookupProperty(oppEntity.Properties[i].Name, ((LookupProperty)oppEntity.Properties[i]).Value.Value, SelectedEntityName);
                                        _customerId = ((CustomerProperty)dynamicEntity.Properties[i]).Value.Value;
                                    }
                                    else
                                    {
                                        //this case does not copy related enity of child entity
                                        //_customerId = ((CustomerProperty)dynamicEntity.Properties[i]).Value.Value;
                                        CloneChildEntity(dynamicEntity, enitityName, entityMetadata, parentId, referenceColumn);
                                    }
                                }
                            }
                        }

                    }
                }
                catch (SoapException ex)
                {
                    _isFailed = true;

                    //Store messages
                    StoreErrors("Cloning of one child entity of type -:" + enitityName + " failed for event id=" + parentId + ".Details are " + ex.Detail.InnerText);

                    //log the error if cloning of any child entity type fails
                    log.logError("Cloning of one child entity of type -:" + enitityName + " failed for event id=" + parentId + ".Details are " + ex.Detail.InnerText);

                }
                catch (Exception ex)
                {
                    _isFailed = true;

                    //Store Errors
                    StoreErrors("Cloning of one child entity of type -:" + enitityName + " failed for event id=" + parentId + ".Details are " + ex.Message);

                    //log the error if cloning of any child entity type fails
                    log.logError("Cloning of one child entity of type -:" + enitityName + " failed for event id=" + parentId + ".Details are " + ex.Message);
                }


            }
        }

        /// <summary>
        /// Clones records of selected child entity(Many to many)
        /// </summary>
        /// <param name="childEntities"></param>
        /// <param name="childEntity"></param>
        /// <param name="parentId"></param>
        /// <param name="relation"></param>
        private void CloneManyToManyChildEntity(BusinessEntity[] entitiesToClone, string childEntity, Guid parentId, string relationshipName)
        {

            Moniker Moniker1 = new Moniker();
            Moniker1.Id = parentId;
            Moniker1.Name = SelectedEntityName;

            foreach (BusinessEntity beEntity in entitiesToClone)
            {

                try
                {

                    DynamicEntity dynamicEntity = (DynamicEntity)beEntity;

                    // Create a request.
                    AssociateEntitiesRequest request = new AssociateEntitiesRequest();

                    // Assign the request a moniker for both entities that need to be disassociated.
                    Moniker Moniker2 = new Moniker();
                    Guid childEnityId = ((KeyProperty)dynamicEntity.Properties[0]).Value.Value;
                    Moniker2.Id = childEnityId;
                    Moniker2.Name = childEntity;

                    request.Moniker1 = Moniker1;
                    request.Moniker2 = Moniker2;

                    // Set the relationship name that associates the two entities.
                    request.RelationshipName = relationshipName;

                    // Execute the request.
                    AssociateEntitiesResponse response = (AssociateEntitiesResponse)_service.Execute(request);
                }
                catch (SoapException ex)
                {

                    _isFailed = true;

                    StoreErrors("Cloning of one child entity(Many to Many) of type -:" + childEntity + " failed for event id=" + parentId + ".Details are " + ex.Detail.InnerText);

                    log.logError("Cloning of one child entity(Many to Many) of type -:" + childEntity + " failed for event id=" + parentId + ".Details are " + ex.Detail.InnerText);
                }
                catch (Exception ex)
                {
                    _isFailed = true;

                    StoreErrors("Cloning of one child entity(Many to Many) of type -:" + childEntity + " failed for event id=" + parentId + ".Details are " + ex.Message);

                    log.logError("Cloning of one child entity(Many to Many) of type -:" + childEntity + " failed for event id=" + parentId + ".Details are " + ex.Message);
                }

            }




        }

        /// <summary>
        /// Stores exact messages to diplay to user
        /// </summary>
        /// <param name="message"></param>
        private void StoreErrors(string message)
        {
			if (ConfigurationManager.AppSettings["TraceToEventLog"].ToLower().Equals("true"))
            {

                if (_errors == null)
                {
                    _errors = new List<string>();
                }

                _errors.Add(message);
            }

        }

        /// <summary>
        /// Stores exact messages to diplay to user
        /// </summary>
        /// <param name="message"></param>
        private void AddTracing(string message)
        {
            if (ConfigurationManager.AppSettings["TraceToEventLog"].ToLower().Equals("true"))
            {
                log.logInfo(message);
            }
        }











    }
}
