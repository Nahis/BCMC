using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Client.Services; //OrganizationService
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

using System.Xml;


/// <summary>
/// Summary description for UtilityFunctions
/// </summary>
public class UtilityFunctions
{
	public UtilityFunctions(OrganizationService service)
	{
		this._service = service;
	}

	private OrganizationService _service;
	public String getHoliday(DateTime date)
	{
		return getHoliday(date.Month, date.Day, date.Year);
	}

	public String getHoliday(int month, int day, int year)
	{
		QueryExpression query = 
			new QueryExpression("bcmc_holidaycalendar");

		ColumnSet columns = new ColumnSet();
		columns.AddColumns(new string[] { "bcmc_name", "bcmc_holidayid" });
		query.ColumnSet = columns;
		query.Criteria = new FilterExpression();
		query.Criteria.FilterOperator = LogicalOperator.And;

		ConditionExpression condDay = new ConditionExpression();
		condDay.AttributeName = "bcmc_day";
		condDay.Operator = ConditionOperator.Equal;
		condDay.Values.Add(day);

		ConditionExpression condMonth = new ConditionExpression();
		condMonth.AttributeName = "bcmc_month";
		condMonth.Operator = ConditionOperator.Equal;
		condMonth.Values.Add(month);

		ConditionExpression condYear = new ConditionExpression();
		condYear.AttributeName = "bcmc_year";
		condYear.Operator = ConditionOperator.Equal;
		condYear.Values.Add(year);

		ConditionExpression condFixed = new ConditionExpression();
		condFixed.AttributeName = "bcmc_fixedholiday";
		condFixed.Operator = ConditionOperator.Equal;
		condFixed.Values.Add("1");

		FilterExpression fltYear = new FilterExpression();
		fltYear.FilterOperator = LogicalOperator.Or;
		fltYear.Conditions.AddRange(new ConditionExpression[] { condYear, condFixed });

		FilterExpression fltMain = new FilterExpression();
		fltMain.FilterOperator = LogicalOperator.And;
		fltMain.Conditions.AddRange(new ConditionExpression[] { condDay, condMonth });
		fltMain.Filters.Add(fltYear);

		query.Criteria = fltMain;
		query.Criteria.FilterOperator = LogicalOperator.And;

		var rsp = _service.RetrieveMultiple(query);

		if (rsp.Entities.Count() == 0)
			return string.Empty;

		//bcmc_holidaycalendar
		var holiday = (Entity)rsp.Entities[0];
		return ((EntityReference)holiday["bcmc_holidayid"]).Name; 
	}
}