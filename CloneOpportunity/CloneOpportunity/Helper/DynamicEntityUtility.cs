//-----------------------------------------------------------------------
// <copyright file="DynamicEntityUtility.cs" company="Microsoft">
//		eService Accelerator V1.0
//		Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
//using Microsoft.Crm.Sdk;
using System.Xml;
using System.Web.Services.Protocols;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using CrmService = Microsoft.Xrm.Client.Services.OrganizationService;
using CrmDateTime = System.DateTime;
using CrmBoolean = System.Boolean;
using Picklist = Microsoft.Xrm.Sdk.OptionSetValue;
using Lookup = Microsoft.Xrm.Sdk.EntityReference;
using CrmNumber = System.Int32;
using CrmDecimal = System.Decimal;
using CrmFloat = System.Single;
using CrmMoney = Microsoft.Xrm.Sdk.Money;
//using Customer =
//using Owner
//using Status
//using Key
using log4net;
using CrmServiceManager = CrmService2011;

/// <summary>
/// Summary description for DynamicEntityUtility
/// </summary>
public static class DynamicEntityUtility
{
	private static ILog logger = LogManager.GetLogger(typeof(DynamicEntityUtility));
	/// <summary>
	/// Retrieve metadata for a single CRM record
	/// </summary>
	/// <param name="service">An instance of the crm web service</param>
	/// <param name="metadataService">An instance of the crm metadata service</param>
	/// <param name="entityName">A string name of the entity type being retrieved e.g. contact</param>
	/// <param name="entityId">A Guid representing the record id we want to retrieve</param>
	/// <param name="attributes">An array of strings representing the list of attributes we want to retrieve</param>
	/// <returns></returns>
	public static List<AttributeData> GetAttributeDataByEntity(CrmService service, String entityName, Guid entityId, params String[] attributes)
	{
		List<AttributeData> attributeData = new List<AttributeData>();
		Boolean isExistingEntity = false;

		Entity existingRecord = null;
		// OPTION: Metadata can be cached for better performance.
		List<AttributeMetadata> allAttributesMetadata = MetadataUtility.GetAllAttributesMetadataByEntity(service, entityName);
		//if the guid has been supplied then try and retrieve the record
		if (entityId != Guid.Empty)
		{
			service = CrmServiceManager.GetCrmService();

			existingRecord = RetrieveByIdAsDynamicEntity(service, entityName, entityId, attributes);
			isExistingEntity = true;
		}

		//for each attribute
		foreach (String attribute in attributes)
		{
			AttributeData data = new AttributeData();
			data.IsUnsupported = false;

			// Attribute label and type apply to all attributes, as they are metadata info.
			AttributeMetadata metadata = allAttributesMetadata.Find(delegate(AttributeMetadata a) { return (a.SchemaName.Equals(attribute, StringComparison.CurrentCultureIgnoreCase)); });
			if (metadata != null)
			{
				switch (metadata.AttributeType.Value)
				{
					case AttributeTypeCode.Boolean:
						BooleanAttributeData booleanData = new BooleanAttributeData();
						BooleanAttributeMetadata booleanMetadata = (BooleanAttributeMetadata)metadata;

						booleanData.BooleanOptions = new OptionMetadata[] { 
							booleanMetadata.OptionSet.TrueOption, 
							booleanMetadata.OptionSet.FalseOption };

						data = booleanData;
						break;

					case AttributeTypeCode.Picklist:
						PicklistAttributeData picklistData = new PicklistAttributeData();
						picklistData.PicklistOptions = ((PicklistAttributeMetadata)metadata).OptionSet.Options;

						data = picklistData;
						break;

					case AttributeTypeCode.Status:
						PicklistAttributeData statusData = new PicklistAttributeData();
						List<OptionSetMetadata> options = new List<OptionSetMetadata>(); //ytodo check
						/*foreach (OptionSetMetadata option in ((StatusAttributeMetadata)metadata).OptionSet.Options)
						{
							options.Add(option);
						}
						statusData.PicklistOptions = options.ToArray();*/
						statusData.PicklistOptions = ((StatusAttributeMetadata)metadata).OptionSet.Options;

						data = statusData;
						break;

					case AttributeTypeCode.String:
						StringAttributeData stringData = new StringAttributeData();
						stringData.MaxLength = ((StringAttributeMetadata)metadata).MaxLength.Value;

						data = stringData;
						break;
					case AttributeTypeCode.Customer:
					//case AttributeTypeCode.Internal:
					case AttributeTypeCode.Lookup:
					case AttributeTypeCode.Owner:
					case AttributeTypeCode.PartyList:
					//case AttributeTypeCode.PrimaryKey:
					case AttributeTypeCode.Virtual:
						data.IsUnsupported = true;
						break;
					default: //ytodo remove
						//data.IsUnsupported = true;
						/*throw new NotSupportedException(String.Format("Metatype {0} has not been implemented",
							metadata.AttributeType.Value.ToString()));*/
						//ytodo remove
						logger.Info(String.Format("Metatype {0} has not been implemented",
							metadata.AttributeType.Value.ToString()));
						break;
					 
				}

				data.SchemaName = attribute;
				data.AttributeLabel = metadata.DisplayName.UserLocalizedLabel.Label;
				data.AttributeType = metadata.AttributeType.Value;
			}

			// Display value and actual value only apply to attributes tied to a record
			if (isExistingEntity)
			{
				foreach (KeyValuePair<string,object> property in existingRecord.Attributes)
				{
					if (property.Key.Equals(attribute, StringComparison.OrdinalIgnoreCase))
					{
						data.DisplayValue = QueryUtilities.GetEntityObject_DisplayValue(existingRecord, property.Key);
						data.ActualValue = property.Value;
						break;
					}
				}
			}
			attributeData.Add(data);
		}
		return attributeData;
	}
#if false
	public static List<AttributeData> GetAttributeDataByEntity(CrmService service, MetadataService metadataService, String entityName, params String[] attributes)
	{
		return GetAttributeDataByEntity(service, metadataService, entityName, Guid.Empty, attributes);
	}

	/// <summary>
	/// Retrieve metadata for a single CRM record
	/// </summary>
	/// <param name="service">An instance of the crm web service</param>
	/// <param name="metadataService">An instance of the crm metadata service</param>
	/// <param name="entityName">A string name of the entity type being retrieved e.g. contact</param>
	/// <param name="entityId">A Guid representing the record id we want to retrieve</param>
	/// <param name="attributes">An array of strings representing the list of attributes we want to retrieve</param>
	/// <returns></returns>
	public static List<AttributeData> GetAttributeDataByEntity(CrmService service, MetadataService metadataService, String entityName, Guid entityId, params String[] attributes)
	{
		List<AttributeData> attributeData = new List<AttributeData>();
		Boolean isExistingEntity = false;

		DynamicEntity existingRecord = null;
		// OPTION: Metadata can be cached for better performance.
		List<AttributeMetadata> allAttributesMetadata = MetadataUtility.GetAllAttributesMetadataByEntity(metadataService, entityName);
		//if the guid has been supplied then try and retrieve the record
		if (entityId != Guid.Empty)
		{
			service = CrmServiceManager.GetCrmService();

			existingRecord = RetrieveByIdAsDynamicEntity(service, entityName, entityId, attributes);
			isExistingEntity = true;
		}

		//for each attribute
		foreach (String attribute in attributes)
		{
			AttributeData data = new AttributeData();
			data.IsUnsupported = false;

			// Attribute label and type apply to all attributes, as they are metadata info.
			AttributeMetadata metadata = allAttributesMetadata.Find(delegate(AttributeMetadata a) { return (a.SchemaName.Equals(attribute, StringComparison.CurrentCultureIgnoreCase)); });
			if (metadata != null)
			{
				switch (metadata.AttributeType.Value)
				{
					case AttributeType.Boolean:
						BooleanAttributeData booleanData = new BooleanAttributeData();
						BooleanAttributeMetadata booleanMetadata = (BooleanAttributeMetadata)metadata;

						booleanData.BooleanOptions = new Option[] { booleanMetadata.TrueOption, booleanMetadata.FalseOption };

						data = booleanData;
						break;
					case AttributeType.Picklist:
						PicklistAttributeData picklistData = new PicklistAttributeData();
						picklistData.PicklistOptions = ((PicklistAttributeMetadata)metadata).Options;

						data = picklistData;
						break;
					case AttributeType.Status:
						PicklistAttributeData statusData = new PicklistAttributeData();
						List<Option> options = new List<Option>();
						foreach (Option option in ((StatusAttributeMetadata)metadata).Options)
						{
							options.Add(option);
						}
						statusData.PicklistOptions = options.ToArray();

						data = statusData;
						break;
					case AttributeType.String:
						StringAttributeData stringData = new StringAttributeData();
						stringData.MaxLength = ((StringAttributeMetadata)metadata).MaxLength.Value;

						data = stringData;
						break;
					case AttributeType.Customer:
					case AttributeType.Internal:
					case AttributeType.Lookup:
					case AttributeType.Owner:
					case AttributeType.PartyList:
					case AttributeType.PrimaryKey:
					case AttributeType.Virtual:
						data.IsUnsupported = true;
						break;
				}

				data.SchemaName = attribute;
				data.AttributeLabel = metadata.DisplayName.UserLocLabel.Label;
				data.AttributeType = metadata.AttributeType;
			}

			// Display value and actual value only apply to attributes tied to a record
			if (isExistingEntity)
			{
				foreach (Property property in existingRecord.Properties)
				{
					if (property.Name.Equals(attribute, StringComparison.OrdinalIgnoreCase))
					{
						data.DisplayValue = GetPropertyDisplayValue(property);
						data.ActualValue = GetPropertyActualValue(property);
						break;
					}
				}
			}
			attributeData.Add(data);
		}
		return attributeData;
	}
#endif
	public static object GetPropertyActualValue(Object property)
	{
		if (property is String)
		{
			return property;
		}
		else if (property is Picklist)
		{
			return ((Picklist)property).Value;
		}
		else if (property is CrmBoolean)
		{
			return property;
		}
		else if (property is CrmDateTime)
		{
			return property;
		}
		/*else if (property is Customer)
		{
			return ((Customer)property).Value;
		}*/
		else if (property is Lookup)
		{
			return ((Lookup)property).Id;
		}
		else if (property is CrmNumber)
		{
			return property;
		}
		else if (property is CrmDecimal)
		{
			return property;
		}
		else if (property is CrmFloat)
		{
			return property;
		}
		else if (property is CrmMoney)
		{
			return property;
		}
		/*else if (property is Owner)
		{
			return ((Owner)property).Value;
		}
		else if (property is Status)
		{
			return ((Status)property).Value;
		}
		else if (property is Key)
		{
			return ((Key)property).Value;
		}*/
		else
		{
			throw new NotImplementedException(String.Format("AttributeType {0} is not implemented.", property.GetType()));
		}
	}

	/*public static int getOptionSetValue(string entityName, string attributeName, string  optionsetText)
    {
       int optionSetValue = 0;
       RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest();
       retrieveAttributeRequest.EntityLogicalName = entityName;
       retrieveAttributeRequest.LogicalName = attributeName;
       retrieveAttributeRequest.RetrieveAsIfPublished = true;

       RetrieveAttributeResponse retrieveAttributeResponse = 
         (RetrieveAttributeResponse)OrganizationService.Execute(retrieveAttributeRequest);
       StateAttributeMetadata picklistAttributeMetadata = 
         (StateAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

       OptionSetMetadata optionsetMetadata = picklistAttributeMetadata.OptionSet;

       foreach (OptionMetadata optionMetadata in optionsetMetadata.Options)
         {
           if (optionMetadata.Label.UserLocalizedLabel.Label.ToLower() == optionsetText.ToLower())
            {
                optionSetValue = optionMetadata.Value.Value;
                return optionSetValue;
            }

         }
         return optionSetValue;
   }*/

  public static string getOptionSetText(CrmService crmService, string entityName, string attributeName, int optionsetValue)
   {
       string optionsetText = string.Empty;
	   Microsoft.Xrm.Sdk.Messages.RetrieveAttributeRequest retrieveAttributeRequest = 
		   new Microsoft.Xrm.Sdk.Messages.RetrieveAttributeRequest();
       retrieveAttributeRequest.EntityLogicalName = entityName;
       retrieveAttributeRequest.LogicalName = attributeName;
       retrieveAttributeRequest.RetrieveAsIfPublished = true;

	   Microsoft.Xrm.Sdk.Messages.RetrieveAttributeResponse retrieveAttributeResponse =
		 (Microsoft.Xrm.Sdk.Messages.RetrieveAttributeResponse)crmService.Execute(retrieveAttributeRequest);
       StateAttributeMetadata stateMeta = 
         (StateAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

	   OptionSetMetadata OptionSetMeta = stateMeta.OptionSet;

	   foreach (OptionMetadata optionMetadata in OptionSetMeta.Options)
       {
            if (optionMetadata.Value == optionsetValue)
             {
                optionsetText = optionMetadata.Label.UserLocalizedLabel.Label;
                 return optionsetText;
             }

       }
       return optionsetText;
  }

  public static string getStatusText(CrmService crmService, string entityName, string attributeName, int optionsetValue)
  {
	  string optionsetText = string.Empty;
	  Microsoft.Xrm.Sdk.Messages.RetrieveAttributeRequest retrieveAttributeRequest =
		  new Microsoft.Xrm.Sdk.Messages.RetrieveAttributeRequest();
	  retrieveAttributeRequest.EntityLogicalName = entityName;
	  retrieveAttributeRequest.LogicalName = attributeName;
	  retrieveAttributeRequest.RetrieveAsIfPublished = true;

	  Microsoft.Xrm.Sdk.Messages.RetrieveAttributeResponse retrieveAttributeResponse =
		(Microsoft.Xrm.Sdk.Messages.RetrieveAttributeResponse)crmService.Execute(retrieveAttributeRequest);
	  StatusAttributeMetadata stateMeta =
		(StatusAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

	  OptionSetMetadata OptionSetMeta = stateMeta.OptionSet;

	  foreach (OptionMetadata optionMetadata in OptionSetMeta.Options)
	  {
		  if (optionMetadata.Value == optionsetValue)
		  {
			  optionsetText = optionMetadata.Label.UserLocalizedLabel.Label;
			  return optionsetText;
		  }

	  }
	  return optionsetText;
  }

  /*ytodo remove
   * public static Dictionary<int, string> GetOptionSetLabels(CrmService metaService, string entityname, string attributename)
  {
	  Dictionary<int, string> hashTable = new Dictionary<int, string>();
	  try
	  {
		  Microsoft.Xrm.Sdk.Messages.RetrieveAttributeRequest request = new Microsoft.Xrm.Sdk.Messages.RetrieveAttributeRequest();
		  request.EntityLogicalName = entityname;
		  request.LogicalName = attributename;
		  request.RetrieveAsIfPublished = true;
		  Microsoft.Xrm.Sdk.Messages.RetrieveAttributeResponse response = 
			  (Microsoft.Xrm.Sdk.Messages.RetrieveAttributeResponse)metaService.Execute(request);

		  response.AttributeMetadata.DisplayName
		  foreach (StatusOptionMetadata statecode in response.AttributeMetadata.)
		  {
			  hashTable[statecode.Value.Value] = statecode.Label.UserLocalizedLabel.Label;
		  }
	  }
	  catch (SoapException ex)
	  {
		  //log.logError("GetLeadStateCodes---->" + ex.Detail.InnerText);
		  throw ex;
	  }
	  catch (Exception ex)
	  {
		  //log.logError("GetLeadStateCodes---->" + ex.Message);
		  throw ex;
	  }

	  return hashTable;
  }*/

  public static object ExtractValue(object value)
  {
	  if (value is EntityReference)
		  value = (value as EntityReference).Name;
	  else if (value is AliasedValue)
	  {
		  value = ExtractValue((value as AliasedValue).Value);
	  }
	  else if (value is OptionSetValue)
		  value = (value as OptionSetValue).Value;

	  return value;
  }

#if false

	public static object GetPropertyActualValue(Property property)
	{
		if (property is StringProperty)
		{
			return ((StringProperty)property).Value;
		}
		else if (property is PicklistProperty)
		{
			return ((PicklistProperty)property).Value.Value;
		}
		else if (property is CrmBooleanProperty)
		{
			return ((CrmBooleanProperty)property).Value.Value;
		}
		else if (property is CrmDateTimeProperty)
		{
			return ((CrmDateTimeProperty)property).Value.Value;
		}
		else if (property is CustomerProperty)
		{
			return ((CustomerProperty)property).Value.Value;
		}
		else if (property is LookupProperty)
		{
			return ((LookupProperty)property).Value.Value;
		}
		else if (property is CrmNumberProperty)
		{
			return ((CrmNumberProperty)property).Value.Value;
		}
		else if (property is CrmDecimalProperty)
		{
			return ((CrmDecimalProperty)property).Value.Value;
		}
		else if (property is CrmFloatProperty)
		{
			return ((CrmFloatProperty)property).Value.Value;
		}
		else if (property is CrmMoneyProperty)
		{
			return ((CrmMoneyProperty)property).Value.Value;
		}
		else if (property is OwnerProperty)
		{
			return ((OwnerProperty)property).Value.Value;
		}
		else if (property is StatusProperty)
		{
			return ((StatusProperty)property).Value.Value;
		}
		else if (property is StateProperty)
		{
			return ((StateProperty)property).Value;
		}
		else if (property is KeyProperty)
		{
			return ((KeyProperty)property).Value.Value;
		}
		else
		{
			throw new NotImplementedException(String.Format("AttributeType {0} is not implemented.", property.GetType()));
		}
	}

	public static String GetPropertyDisplayValue(Object property)
	{
		if (property is String)
		{
			return property.ToString();
		}
		else if (property is Picklist)
		{
			return ((Picklist)property).name;
		}
		else if (property is CrmBoolean)
		{
			return ((CrmBoolean)property).name;
		}
		else if (property is CrmDateTime)
		{
			DateTime results;
			if (DateTime.TryParse(((CrmDateTime)property).Value, out results))
			{
				String localResults = results.ToString();
				foreach (String culture in HttpContext.Current.Request.UserLanguages)
				{
					try
					{
						localResults = results.ToString(CultureInfo.GetCultureInfoByIetfLanguageTag(culture));
					}
					catch (ArgumentException)
					{
						continue;
					}
					break;
				}
				return localResults;
			}
			else
			{
				return String.Empty;
			}
		}
		else if (property is Customer)
		{
			return ((Customer)property).name;
		}
		else if (property is Lookup)
		{
			return ((Lookup)property).name;
		}
		else if (property is CrmNumber)
		{
			return ((CrmNumber)property).formattedvalue;
		}
		else if (property is CrmDecimal)
		{
			return ((CrmDecimal)property).formattedvalue;
		}
		else if (property is CrmFloat)
		{
			return ((CrmFloat)property).formattedvalue;
		}
		else if (property is CrmMoney)
		{
			return ((CrmMoney)property).Value.ToString();
		}
		else if (property is Owner)
		{
			return ((Owner)property).name;
		}
		else if (property is Status)
		{
			return ((Status)property).name;
		}
		else if (property is Key)
		{
			return ((Key)property).Value.ToString();
		}
		else
		{
			throw new NotImplementedException(String.Format("AttributeType {0} is not implemented.", property.GetType()));
		}
	}
#endif
#if false
	public static String GetPropertyDisplayValue(Property property)
	{
		if (property is StringProperty)
		{
			return ((StringProperty)property).Value;
		}
		else if (property is PicklistProperty)
		{
			return ((PicklistProperty)property).Value.name;
		}
		else if (property is CrmBooleanProperty)
		{
			return ((CrmBooleanProperty)property).Value.name;
		}
		else if (property is CrmDateTimeProperty)
		{
            //DateTime results;
            //if (DateTime.TryParse(((CrmDateTimeProperty)property).Value.date, out results))
            //{
            //    String localResults = results.ToShortDateString();
            //    //TODO Mahesh
            //    return localResults;
                
            //    foreach (String culture in HttpContext.Current.Request.UserLanguages)
            //    {
            //        try
            //        {
            //            localResults = results.ToString(CultureInfo.GetCultureInfoByIetfLanguageTag(culture));
            //        }
            //        catch (ArgumentException)
            //        {
            //            continue;
            //        }
            //        break;
            //    }
            //    return localResults;
            //}
            //else
            //{
            //    return String.Empty;
            //}

            return ((CrmDateTimeProperty)property).Value.date;

		}
		else if (property is CustomerProperty)
		{
			return ((CustomerProperty)property).Value.name;
		}
		else if (property is LookupProperty)
		{
			return ((LookupProperty)property).Value.name;
		}
		else if (property is CrmNumberProperty)
		{
			return ((CrmNumberProperty)property).Value.formattedvalue;
		}
		else if (property is CrmDecimalProperty)
		{
			return ((CrmDecimalProperty)property).Value.formattedvalue;
		}
		else if (property is CrmFloatProperty)
		{
			return ((CrmFloatProperty)property).Value.formattedvalue;
		}
		else if (property is CrmMoneyProperty)
		{
			//return ((CrmMoneyProperty)property).Value.Value.ToString();
            return ((CrmMoneyProperty)property).Value.formattedvalue;
		}
		else if (property is OwnerProperty)
		{
			return ((OwnerProperty)property).Value.name;
		}
		else if (property is StatusProperty)
		{
			return ((StatusProperty)property).Value.name;
		}
		else if (property is StateProperty)
		{
			return ((StateProperty)property).Value;
		}
		else if (property is KeyProperty)
		{
			return ((KeyProperty)property).Value.Value.ToString();
		}
		else
		{
			throw new NotImplementedException(String.Format("AttributeType {0} is not implemented.", property.GetType()));
		}
	}

    public static String GetPropertyDisplayValue(string type,XmlNode attributeNode)
    {
        string value=string.Empty;
        switch (type.ToLower())
        {
            case "string":
                value=attributeNode.InnerText;
                break;
            
           case "money":
           case "decimal":
           case "float":
           case "integer":
                if (attributeNode.Attributes["formattedvalue"] != null)
                {
                    value = attributeNode.Attributes["formattedvalue"].Value;
                }
                else
                {

                    value = attributeNode.InnerText;
                }
              
                break;
            case "boolean":
                if (attributeNode.Attributes["name"] != null)
                {
                    value = attributeNode.Attributes["name"].Value;
                }
                else
                {

                    value = attributeNode.InnerText;
                }
              
                break;
            case "datetime":
                if (attributeNode.Attributes["date"] != null)
                {
                    value = attributeNode.Attributes["date"].Value;
                }
                else
                {

                    value = attributeNode.InnerText;
                }
               
                break;
            
            case "lookup":
                if (attributeNode.Attributes["name"] != null)
                {
                    value = attributeNode.Attributes["name"].Value;
                }
                else
                {
                    value = attributeNode.InnerText;
                }
                break;
            case "key":
                if (attributeNode.Attributes["name"] != null)
                {
                    value = attributeNode.Attributes["name"].Value;
                }
                else
                {
                    value = attributeNode.InnerText;
                }
                break;
            case "primarykey":
                value = attributeNode.InnerText;
                break;
            case "owner":
                if (attributeNode.Attributes["name"] != null)
                {
                    value = attributeNode.Attributes["name"].Value;
                }
                else
                {
                    value = attributeNode.InnerText;
                }
                break;
            case "customer":
                if (attributeNode.Attributes["name"] != null)
                {
                    value = attributeNode.Attributes["name"].Value;
                }
                else
                {
                    value = attributeNode.InnerText;
                }
                break;
          
            case "picklist":
                if (attributeNode.Attributes["name"] != null)
                {
                    value = attributeNode.Attributes["name"].Value;
                }
                else
                {
                    value = attributeNode.InnerText;
                }
                break;
               
            case "status":
                if (attributeNode.Attributes["name"] != null)
                {
                    value = attributeNode.Attributes["name"].Value;
                }
                else
                {
                    value = attributeNode.InnerText;
                }
                break;
               
           
            case "state":
                if (attributeNode.Attributes["name"] != null)
                {
                    value = attributeNode.Attributes["name"].Value;
                }
                else
                {
                    value = attributeNode.InnerText;
                }
                break;
            case "memo":
             

                break;


        }

        return value;
        
    }
#endif
	//Return a single record as a dynamic entity based on a given Guid
	public static Entity RetrieveByIdAsDynamicEntity(CrmService service, String entityName, Guid entityId, params String[] attributes)
	{
		ColumnSet columns = new ColumnSet();
		if (null != attributes && attributes.Length > 0)
			foreach (string col in attributes)
				columns.AddColumn(col);
		else
			columns.AllColumns = true;
		return service.Retrieve(entityName, entityId, columns);
	}

	//Returns a list of Dynamic Entities based on a given query criteria.
	public static List<Entity> RetrieveMultipleAsDynamicEntities(CrmService service, QueryBase query)
	{
        try
        {
			EntityCollection entities = service.RetrieveMultiple(query);
            List<Entity> results = new List<Entity>();
			foreach (Entity entity in entities.Entities)
            {
                results.Add(entity);
            }
            return results;
        }
        catch (System.Web.Services.Protocols.SoapException ex)
        {
            throw ex;
        }
        catch (Exception ex)
        {
            throw ex;
        }
	}

#if false
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public static StringProperty GetStringProperty(string fieldName, string fieldValue)
    {

        try
        {
            StringProperty prop = new StringProperty();
            prop.Name = fieldName;
            prop.Value = fieldValue;

            return prop;
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
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public static CrmMoneyProperty GetCrmMoneyProperty(string fieldName, decimal fieldValue, Boolean isSetNull)
    {
        try
        {
            CrmMoneyProperty prop = new CrmMoneyProperty();
            prop.Name = fieldName;
            prop.Value = new CrmMoney();

            //Check if value is to be set as null in database
            if (isSetNull == false)
            {
                prop.Value.Value = fieldValue;
            }
            else
            {
                prop.Value.IsNull = true;
                prop.Value.IsNullSpecified = true;

            }

            return prop;
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
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public static CrmDecimalProperty GetCrmDecimalProperty(string fieldName, decimal fieldValue, Boolean isSetNull)
    {
        try
        {
            CrmDecimalProperty prop = new CrmDecimalProperty();
            prop.Name = fieldName;
            prop.Value = new CrmDecimal();

            //check if value is to be set as null in database
            if (isSetNull == false)
            {
                prop.Value.Value = fieldValue;
            }
            else
            {
                prop.Value.IsNull = true;
                prop.Value.IsNullSpecified = true;
            }

            return prop;
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
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public static CrmFloatProperty GetCrmFloatProperty(string fieldName, double fieldValue, Boolean isSetNull)
    {
        try
        {
            CrmFloatProperty prop = new CrmFloatProperty();
            prop.Name = fieldName;
            prop.Value = new CrmFloat();

            //check if value is to be set as null in database
            if (isSetNull == false)
            {
                prop.Value.Value = fieldValue;
            }
            else
            {
                prop.Value.IsNull = true;
                prop.Value.IsNullSpecified = true;
            }

            return prop;
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
    /// Creates status property for crm
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public static StatusProperty GetStatusProperty(string fieldName, Int32 fieldValue, Boolean isSetNull)
    {
        try
        {

            StatusProperty prop = new StatusProperty();
            prop.Name = fieldName;
            prop.Value = new Status();


            //check if value is to be set as null in database
            if (isSetNull == false)
            {
                prop.Value.Value = fieldValue;

            }
            else
            {
                prop.Value.IsNull = true;
                prop.Value.IsNullSpecified = true;
            }

            return prop;
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
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public static CrmBooleanProperty GetCrmBooleanProperty(string fieldName, bool fieldValue, bool isSetNull)
    {
        try
        {
            CrmBooleanProperty prop = new CrmBooleanProperty();
            prop.Name = fieldName;
            prop.Value = new CrmBoolean();

            //Checks if value needs to be set as blank in database.
            if (isSetNull == false)
            {
                prop.Value.Value = fieldValue;

            }
            else
            {
                prop.Value.IsNull = true;
                prop.Value.IsNullSpecified = true;
            }

            return prop;
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
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public static CrmDateTimeProperty GetCrmDateTimeProperty(string fieldName, string fieldValue, Boolean isSetNull)
    {
        try
        {
            CrmDateTimeProperty prop = new CrmDateTimeProperty();
            prop.Name = fieldName;
            prop.Value = new CrmDateTime();
            //Check if value is to be set as null in database
            if (isSetNull == false)
            {
                prop.Value.Value = Convert.ToDateTime(fieldValue).ToString("MM/dd/yyyy HH:mm:ss");
                
            }
            else
            {
                prop.Value.IsNull = true;
                prop.Value.IsNullSpecified = true;
            }

            return prop;
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
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public static LookupProperty GetLookupProperty(string fieldName, Guid fieldValue, string entityName)
    {
        try
        {
            LookupProperty prop = new LookupProperty();
            prop.Name = fieldName;



            if (fieldValue == Guid.Empty)
            {
                prop.Value = new Lookup();
                prop.Value.IsNull = true;
                prop.Value.IsNullSpecified = true;
            }
            else if (fieldName.Equals("regardingobjectid") || fieldName.Equals("parentid"))
            {
                Lookup lk = new Lookup();
                lk.type = entityName;
                lk.Value = fieldValue;
                prop.Value = lk;
            }
            else
            {
                prop.Value = new Lookup();
                prop.Value.Value = fieldValue;
            }

            return prop;
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
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public static KeyProperty GetKeyProperty(string fieldName, Guid fieldValue)
    {
        try
        {
            KeyProperty prop = new KeyProperty();
            prop.Name = fieldName;
            prop.Value = new Key();
            prop.Value.Value = fieldValue;
            return prop;
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
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <param name="ownerType"></param>
    /// <returns></returns>
    public static OwnerProperty GetOwnerProperty(string fieldName, Guid fieldValue, string ownerType, Boolean isSetNull)
    {
        try
        {
            OwnerProperty prop = new OwnerProperty();
            prop.Name = fieldName;
            prop.Value = new Owner();
            //Check if value is to be set as null in database
            if (isSetNull == false)
            {
                prop.Value.type = ownerType;
                prop.Value.Value = fieldValue;
            }
            else
            {
                prop.Value.IsNull = true;
                prop.Value.IsNullSpecified = true;
            }



            return prop;
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
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <param name="customerType"></param>
    /// <returns></returns>
    public static CustomerProperty GetCustomerProperty(string fieldName, Guid fieldValue, string customerType)
    {
        try
        {
            CustomerProperty prop = new CustomerProperty();
            prop.Name = fieldName;

            if (fieldValue.Equals(Guid.Empty))
            {
                Customer aa = new Customer();
                aa.IsNull = true;
                aa.IsNullSpecified = true;
                prop.Value = aa;
            }
            else
            {
                Customer aa = new Customer();
                aa.type = customerType;
                aa.Value = fieldValue;
                prop.Value = aa;
            }

            return prop;
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
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public static CrmNumberProperty GetCrmNumberProperty(string fieldName, int fieldValue, Boolean isSetNull)
    {
        try
        {
            CrmNumberProperty prop = new CrmNumberProperty();
            prop.Name = fieldName;
            prop.Value = new CrmNumber();

            if (isSetNull == false)
            {
                prop.Value.Value = fieldValue;
            }
            else
            {
                prop.Value.IsNull = true;
                prop.Value.IsNullSpecified = true;
            }

            return prop;
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
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public static PicklistProperty GetPicklistProperty(string fieldName, int fieldValue, Boolean isSetNull)
    {
        try
        {
            PicklistProperty prop = new PicklistProperty();
            prop.Name = fieldName;
            prop.Value = new Picklist();

            //Check if value is to be set as null
            if (isSetNull == false)
            {
                prop.Value.Value = fieldValue;
            }
            else
            {
                prop.Value.IsNull = true;
                prop.Value.IsNullSpecified = true;
            }

            return prop;
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

#endif
}
