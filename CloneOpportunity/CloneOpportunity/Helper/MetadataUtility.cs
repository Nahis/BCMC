//-----------------------------------------------------------------------
// <copyright file="MetadataUtility.cs" company="Microsoft">
//		eService Accelerator V1.0
//		Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq; // Added for LINQ support (e.g., Any, Select)
using Microsoft.Xrm.Sdk; // Modern SDK namespace
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Messages;
using IOrganizationService = Microsoft.Xrm.Sdk.IOrganizationService; // Alias for clarity

/// <summary>
/// Utility class for retrieving and caching CRM metadata.
/// </summary>
public static class MetadataUtility
{
    private static readonly Dictionary<string, EntityMetadata> _entityMetadataCache = new Dictionary<string, EntityMetadata>();
    private static readonly Dictionary<string, AttributeMetadata> _attributeMetadataCache = new Dictionary<string, AttributeMetadata>();
    private static readonly object _lockObject = new object();
    private static DateTime _metadataLastValidatedAt;

    /// <summary>
    /// Retrieves metadata for a specified entity, caching the result.
    /// </summary>
    public static EntityMetadata GetEntityMetadata(IOrganizationService service, string entityName)
    {
        ValidateMetadata();

        if (!_entityMetadataCache.TryGetValue(entityName, out EntityMetadata entityMetadata))
        {
            lock (_lockObject)
            {
                if (!_entityMetadataCache.TryGetValue(entityName, out entityMetadata))
                {
                    var request = new RetrieveEntityRequest
                    {
                        LogicalName = entityName,
                        EntityFilters = EntityFilters.All
                    };

                    var response = (RetrieveEntityResponse)service.Execute(request);
                    entityMetadata = response.EntityMetadata;
                    _entityMetadataCache.Add(entityName, entityMetadata);
                    _metadataLastValidatedAt = DateTime.Now;
                }
            }
        }

        return entityMetadata;
    }

    /// <summary>
    /// Retrieves metadata for a specific attribute, caching the result.
    /// </summary>
    public static AttributeMetadata GetAttributeMetadata(IOrganizationService service, string entityName, string attributeName)
    {
        string entityAndAttribute = $"{entityName}.{attributeName}";
        ValidateMetadata();

        if (!_attributeMetadataCache.TryGetValue(entityAndAttribute, out AttributeMetadata attributeMetadata))
        {
            lock (_lockObject)
            {
                if (!_attributeMetadataCache.TryGetValue(entityAndAttribute, out attributeMetadata))
                {
                    var request = new RetrieveAttributeRequest
                    {
                        EntityLogicalName = entityName,
                        LogicalName = attributeName
                    };

                    var response = (RetrieveAttributeResponse)service.Execute(request);
                    attributeMetadata = response.AttributeMetadata;
                    _attributeMetadataCache.Add(entityAndAttribute, attributeMetadata);
                    _metadataLastValidatedAt = DateTime.Now;
                }
            }
        }

        return attributeMetadata;
    }

    /// <summary>
    /// Gets all attribute metadata for an entity.
    /// </summary>
    public static List<AttributeMetadata> GetAllAttributesMetadataByEntity(IOrganizationService service, string entityName)
    {
        EntityMetadata entityMetadata = GetEntityMetadata(service, entityName);
        return entityMetadata.Attributes.ToList(); // Convert array to List using LINQ
    }

    /// <summary>
    /// Gets the schema names of required attributes for an entity.
    /// </summary>
    public static List<string> GetRequiredAttributesByEntity(IOrganizationService service, string entityName)
    {
        return GetAllAttributesMetadataByEntity(service, entityName)
            .Where(attribute => attribute.RequiredLevel?.Value == AttributeRequiredLevel.ApplicationRequired ||
                               attribute.RequiredLevel?.Value == AttributeRequiredLevel.SystemRequired)
            .Select(attribute => attribute.SchemaName.ToLower())
            .ToList();
    }

    /// <summary>
    /// Maps a status code to its corresponding state code label.
    /// </summary>
    public static string GetStateCodeFromStatusCode(IOrganizationService service, string entityName, int status)
    {
        var statusMetadata = (StatusAttributeMetadata)GetAttributeMetadata(service, entityName, "statuscode");
        var stateMetadata = (StateAttributeMetadata)GetAttributeMetadata(service, entityName, "statecode");

        int stateCode = statusMetadata.OptionSet.Options
            .OfType<StatusOptionMetadata>()
            .FirstOrDefault(option => option.Value == status)?.State ?? 0;

        return stateMetadata.OptionSet.Options
            .OfType<StateOptionMetadata>()
            .FirstOrDefault(option => option.Value == stateCode)?.Label.UserLocalizedLabel.Label ?? string.Empty;
    }

    /// <summary>
    /// Clears metadata caches if they are older than 1 hour.
    /// </summary>
    private static void ValidateMetadata()
    {
        // TODO: Move timeout to web.config for configurability
        if (DateTime.Now.Subtract(_metadataLastValidatedAt).TotalHours > 1)
        {
            _metadataLastValidatedAt = DateTime.Now;
            _attributeMetadataCache.Clear();
            _entityMetadataCache.Clear();
        }
    }
}