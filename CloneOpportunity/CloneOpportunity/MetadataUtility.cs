//-----------------------------------------------------------------------
// <copyright file="MetadataUtility.cs" company="Microsoft">
//		eService Accelerator V1.0
//		Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Metadata;
using MetadataService = Microsoft.Xrm.Client.Services.OrganizationService;
using Microsoft.Xrm.Sdk.Messages;

/// <summary>
/// Summary description for MetadataUtility
/// </summary>
public static class MetadataUtility
{
	private static Dictionary<String, EntityMetadata> _entityMetadataCache = new Dictionary<String, EntityMetadata>();
	private static Dictionary<String, AttributeMetadata> _attributeMetadataCache = new Dictionary<String, AttributeMetadata>();
	private static Object _lockObject = new Object();
	private static DateTime _metadataLastValidatedAt;

	public static EntityMetadata GetEntityMetadata(MetadataService service, String entityName)
	{
		EntityMetadata entityMetadata;

		ValidateMetadata();

		if (!_entityMetadataCache.TryGetValue(entityName, out entityMetadata))
		{
			lock (_lockObject)
			{
				if (!_entityMetadataCache.TryGetValue(entityName, out entityMetadata))
				{
					RetrieveEntityRequest request = new RetrieveEntityRequest();
					request.LogicalName = entityName;
					request.EntityFilters = EntityFilters.All;

					RetrieveEntityResponse response = (RetrieveEntityResponse)service.Execute(request);

					entityMetadata = response.EntityMetadata;
					_entityMetadataCache.Add(entityName, entityMetadata);

					_metadataLastValidatedAt = DateTime.Now;
				}
			}
		}

		return entityMetadata;
	}

	public static AttributeMetadata GetAttributeMetadata(MetadataService service, String entityName, String attributeName)
	{
		String entityAndAttribute = String.Format("{0}.{1}", entityName, attributeName);
		AttributeMetadata attributeMetadata;

		ValidateMetadata();

		if (!_attributeMetadataCache.TryGetValue(entityAndAttribute, out attributeMetadata))
		{
			lock (_lockObject)
			{
				if (!_attributeMetadataCache.TryGetValue(entityAndAttribute, out attributeMetadata))
				{
					RetrieveAttributeRequest request = new RetrieveAttributeRequest();
					request.EntityLogicalName = entityName;
					request.LogicalName = attributeName;

					RetrieveAttributeResponse response = (RetrieveAttributeResponse)service.Execute(request);

					attributeMetadata = response.AttributeMetadata;
					_attributeMetadataCache.Add(String.Format("{0}.{1}", entityName, attributeName), attributeMetadata);

					_metadataLastValidatedAt = DateTime.Now;
				}
			}
		}

		return attributeMetadata;
	}

	public static List<AttributeMetadata> GetAllAttributesMetadataByEntity(MetadataService service, String entityName)
	{
		EntityMetadata entityMetadata = GetEntityMetadata(service, entityName);

		List<AttributeMetadata> results = new List<AttributeMetadata>();
		foreach (AttributeMetadata attribute in entityMetadata.Attributes)
		{
			results.Add(attribute);
		}

		return results;
	}

	public static List<String> GetRequiredAttributesByEntity(MetadataService service, String entityName)
	{
		List<String> results = new List<String>();

		foreach (AttributeMetadata attribute in GetAllAttributesMetadataByEntity(service, entityName))
		{
			if (attribute.RequiredLevel.Value == AttributeRequiredLevel.ApplicationRequired || attribute.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired)
			{
				results.Add(attribute.SchemaName.ToLower());
			}
		}

		return results;
	}

	public static String GetStateCodeFromStatusCode(MetadataService service, String entityName, int status)
	{
		int stateCode = 0;

		StatusAttributeMetadata statusMetadata = (StatusAttributeMetadata)GetAttributeMetadata(service, entityName, "statuscode");
		StateAttributeMetadata stateMetadata = (StateAttributeMetadata)GetAttributeMetadata(service, entityName, "statecode");

		//foreach (StatusOption option in statusMetadata.OptionSet)
		foreach (StatusOptionMetadata option in statusMetadata.OptionSet.Options)
		{
			if (option.Value.Value == status)
			{
				stateCode = option.State.Value;
				break;
			}
		}

		//foreach (StateOption option in stateMetadata.Options)
		foreach (StatusOptionMetadata option in statusMetadata.OptionSet.Options)
		{
			if (option.Value.Value == stateCode)
			{
				return option.Label.UserLocalizedLabel.Label;
			}
		}

		return String.Empty;
	}

	private static void ValidateMetadata()
	{
		// ToDo: Move timeout to web.config to make it configurable.
		if (DateTime.Now.Subtract(_metadataLastValidatedAt).TotalHours > 1)
		{
			_metadataLastValidatedAt = DateTime.Now;
			_attributeMetadataCache.Clear();
			_entityMetadataCache.Clear();
		}
	}
}
