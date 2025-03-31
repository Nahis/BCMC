using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

using System.Net;
using System.Net.Security;
using System.Security;

using System.ServiceModel.Description;
/*using System.Text;
using System.Web.Services.Protocols;
using System.Security.Cryptography.X509Certificates;*/

// Added System.ServiceModel.dll assembly for this

namespace RoomBlocks2
{
#if false
	public sealed class CrmConnect
	{
		private Uri _organizationUri, _discoveryUri;
/*		private string _adminName;
        private SecureString _adminPassword;*/

		private DiscoveryServiceProxy _discServiceProxy;
		private OrganizationServiceProxy _orgServiceProxy;

		public static string orgSvcUrl = "/xrmservices/2011/organization.svc";
		public static string discovSvcUrl = "/xrmservices/2011/discovery.svc";

		public CrmConnect(string hostorg = "https://boots.smclaims02.com:555", //for example
			string hostDiscovery = "")
		{
			if ("" == hostDiscovery)
				hostDiscovery = hostorg;

			this.Connect(new Uri(hostorg + orgSvcUrl),
				new Uri(hostDiscovery + discovSvcUrl));
		}

		public void Connect(Uri organizationUri, Uri discoveryUri, string adminName = "", string adminPassword = "")
		{
			//to ignore certificates errors
			/*ServicePointManager.ServerCertificateValidationCallback =
				new RemoteCertificateValidationCallback(AcceptAllCertificatePolicy);

			_organizationUri = organizationUri;
			_discoveryUri = discoveryUri;
			_adminName = adminName;
			_adminPassword = ConvertToSecureString(adminPassword);*/

			ClientCredentials userCredentials = new ClientCredentials();
			//authenticating the user and obtaining the SecurityToken from the STS		
			/*userCredentials.UserName.UserName = _adminName;
			userCredentials.UserName.Password = ConvertToUnsecureString(_adminPassword);*/
			userCredentials = (ClientCredentials)System.Net.CredentialCache.DefaultCredentials;

			IServiceConfiguration<IDiscoveryService> discoveryConfiguration =
				ServiceConfigurationFactory.CreateConfiguration<IDiscoveryService>(_discoveryUri);
			SecurityTokenResponse userResponseWrapper = discoveryConfiguration.Authenticate(userCredentials);
			_discServiceProxy = new DiscoveryServiceProxy(discoveryConfiguration, userResponseWrapper);

			IServiceConfiguration<IOrganizationService> serviceConfiguration =
				ServiceConfigurationFactory.CreateConfiguration<IOrganizationService>(_organizationUri);
			_orgServiceProxy = new OrganizationServiceProxy(serviceConfiguration, userResponseWrapper);
			_orgServiceProxy.EnableProxyTypes();
		}
	}


	public class ServiceFactory : IOrganizationServiceFactory
	{
		public IOrganizationService CreateOrganizationService(string hosturl)
		{
			Uri serviceUrl = new System.Uri(hosturl  + CrmConnect.orgSvcUrl);
			ClientCredentials creds = new ClientCredentials();
			//creds.Windows.ClientCredential = new System.Net.NetworkCredential("username", "pass", "domain");
			creds.Windows.ClientCredential = 
				(System.Net.NetworkCredential)System.Net.CredentialCache.DefaultCredentials;

			OrganizationServiceProxy _serviceProxy = 
				new Microsoft.Xrm.Sdk.Client.OrganizationServiceProxy(serviceUrl, null, creds, null);

			_serviceProxy.ServiceConfiguration.CurrentServiceEndpoint.Behaviors.Add(new ProxyTypesBehavior());
			return _serviceProxy;
		}
	}
#endif
}