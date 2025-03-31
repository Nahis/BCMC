using System;
//using System.Collections.Generic;
using System.Web;
using Microsoft.Xrm.Client.Services; //OrganizationService
using System.Xml;
using System.Configuration;

public class HelperFunctions
{
	/// <summary>
	/// returns null if entity is not in the list
	/// </summary>
	/// <param name="entityname"></param>
	/// <returns></returns>
	public static bool IsEntityInConfig_ExceptionList(string entityname, System.Web.Caching.Cache Cache)
	{
		//Load config
		string configxml = (string)Cache["msa.eService.PortalConfigXML"];
		if (null == configxml)
			throw new Exception("Config xml not found!");

		XmlDocument xmlConfig = new XmlDocument();
		xmlConfig.LoadXml(configxml);

		//Extract field changes for specific entity (XmlNodeList)
		return xmlConfig.SelectNodes(
			String.Format("/Entities/Entity[@name='actual_field_names']/Field[@entityname='{0}' and @exists='no']", entityname)).Count > 0;
	}

	public static string getHoliday(DateTime date, OrganizationService orgSrv, System.Web.Caching.Cache Cache)
	{
		return getHoliday(date.Month, date.Day, date.Year, orgSrv, Cache);
	}

	public static string getHoliday(int month, int day, int year, OrganizationService orgSrv, System.Web.Caching.Cache Cache)
	{
		try
		{
			//this will exception if the appsetting is missing and thus will be good for production...?
			ConfigurationManager.AppSettings["development_server"].ToString();

			bool entity_is_missing = false;
			try
			{
				string exclude_entities_list = ConfigurationManager.AppSettings["missing_entities_list"].ToString();
				string[] list = exclude_entities_list.Split(',');
				var x = new System.Collections.ArrayList(list);
				if (x.Contains("bcmc_holidaycalendar"))
					entity_is_missing = true;
			}
			catch (Exception)
			{}

			if (entity_is_missing || IsEntityInConfig_ExceptionList("bcmc_holidaycalendar", Cache)) //ytodo remove eventually
			{
				DateTime date = new DateTime(year, month, day);
				if (date.DayOfWeek.ToString() == "Saturday")
					return "Holiday";
				else
					return "";
			}
		}
		catch (Exception)
		{ }

		return new UtilityFunctions(orgSrv).getHoliday(month, day, year);
	}
}
