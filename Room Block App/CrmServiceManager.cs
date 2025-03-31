//-----------------------------------------------------------------------
// <copyright file="CrmServiceManager.cs" company="Microsoft">
//		eService Accelerator V1.0
//		Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
#if true
using System.Configuration;
using log4net;
using Microsoft.Xrm.Client; 
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;

/// <summary>
/// Summary description for CrmServiceManager
/// </summary>
public static class CrmServiceManager
{
    private static ILog logger = 
		LogManager.GetLogger(typeof(CrmServiceManager));

	public static OrganizationService GetCrmService()
    {
		//logger.Info("GetCrmService");

		//Verify that the web.config has the right settings to create a CrmService
		//instance.
		ValidateWebConfig();

        //Read the web.config to get the relevant settings to create an instance
        //of the CrmService.
		String connectionString = ConfigurationManager.AppSettings["crmconnection"];
		Microsoft.Xrm.Client.CrmConnection connection =
			CrmConnection.Parse(connectionString);

		return new OrganizationService(connection);
    }

	public static OrganizationService GetMetadataService()
    {
		//'/XRMServices/2011/Organization.svc/web'
		return GetCrmService();
    }

    private static void ValidateWebConfig()
    {
        //Read the web.config and check to make sure we have enough information
        //to create an instance of a crm web service.

		String connectionString = ConfigurationManager.AppSettings["crmconnection"];
		if (String.IsNullOrEmpty(connectionString))
		{
			throw new NullReferenceException("You must define a connectionstring in the web config.");
		}

		if (!isValidConnectionString(connectionString))
			throw new Exception("Invalid connectionstring in web.config");
    }

	
	/// <summary>
	/// Gets web service connection information from the app.config file.
	/// If there is more than one available, the user is prompted to select
	/// the desired connection configuration by name.
	/// </summary>
	/// <returns>A String containing web service connection configuration information.</returns>
	public static String GetConnectionString()
	{
		// No valid connections strings found. Write out and error message.
		if (ConfigurationManager.ConnectionStrings.Count == 0)
		{
			throw new Exception("An web.config file containing at least one valid Microsoft Dynamics CRM " +
				"connection String configuration must exist in the run-time folder.");
		}

		return ConfigurationManager.ConnectionStrings[0].ConnectionString;
	}

	/// <summary>
	/// Verifies if a connection String is valid for Microsoft Dynamics CRM.
	/// </summary>
	/// <returns>True for a valid String, otherwise False.</returns>
	private static Boolean isValidConnectionString(String connectionString)
	{
		// At a minimum, a connection String must contain one of these arguments.
		if (connectionString.Contains("Url=") ||
			connectionString.Contains("Server=") ||
			connectionString.Contains("ServiceUri="))
			return true;

		return false;
	}
}
#endif