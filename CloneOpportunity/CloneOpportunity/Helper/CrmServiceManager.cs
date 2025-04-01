using System;
using System.Configuration;
using log4net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Crm.Sdk.Utility
{
    /// <summary>
    /// Summary description for CrmServiceManager
    /// </summary>
    public static class CrmServiceManager
    {
        private static ILog logger = LogManager.GetLogger(typeof(CrmServiceManager));

        public static IOrganizationService GetCrmService()
        {
            logger.Info("GetCrmService Started");

            ValidateWebConfig();

            string connectionString = ConfigurationManager.AppSettings["crmconnection"];
            logger.Info("Connection String: " + connectionString);

            Uri orgUri = new Uri(ExtractUrl(connectionString));
            logger.Info("Using CRM URI: " + orgUri.ToString());

            try
            {
                OrganizationServiceProxy service = new OrganizationServiceProxy(orgUri, null, null, null);
                logger.Info("Service created, testing connection...");
                // Test with a simple query
                Microsoft.Xrm.Sdk.Query.QueryExpression query = new Microsoft.Xrm.Sdk.Query.QueryExpression("systemuser");
                query.TopCount = 1;
                service.RetrieveMultiple(query);
                logger.Info("CRM connection successful");
                return service;
            }
            catch (Exception ex)
            {
                logger.Error("CRM connection failed: " + ex.Message, ex);
                throw;
            }
        }

        private static void ValidateWebConfig()
        {
            string connectionString = ConfigurationManager.AppSettings["crmconnection"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new NullReferenceException("You must define a 'crmconnection' key in the web.config.");
            }
            if (!isValidConnectionString(connectionString))
            {
                throw new Exception("Invalid connection string in web.config.");
            }
        }

        private static string ExtractUrl(string connectionString)
        {
            var parts = connectionString.Split(new char[] { '=', ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Trim().ToLower() == "url" && i + 1 < parts.Length)
                {
                    return parts[i + 1];
                }
            }
            throw new ArgumentException("Connection string must contain a 'Url=' parameter.");
        }

        private static bool isValidConnectionString(string connectionString)
        {
            return connectionString.Contains("Url=") ||
                connectionString.Contains("Server=") ||
                connectionString.Contains("ServiceUri=");
        }
    }
}