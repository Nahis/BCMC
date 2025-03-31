using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Xrm; //XrmEarlyBinding;	

using System.Linq;
using Microsoft.Xrm.Sdk.Linq;
using Microsoft.Xrm.Sdk.Query;

namespace RoomBlocks2
{
    public partial class Grid : System.Web.UI.Page
	{
		//Dynamicgridview.aspx?
		//	recordid=%7BB54E175A-36B0-E311-93E7-534E57000000%7D
		//	&username=maria leahy
		//	&status		null => new with no roompatterns
		//	&rpStatus	SetStatusToActiveAndSetRoomPatternUser if not null

		/*
// Testing
// Dynamicgridview.aspx?username=TestUser&recordid=%7B19D3942D-2D8D-E311-9D78-534E57000000%7D			35th Infantry Test 2 Roomblocks   ;Roomblock of 3-5 Feb 2014
//49D728F7-BC8D-E211-8F60-534E57000000
//1D036A11-D88C-E311-9D78-534E57000000 35th Infantry Test1
//B2B60147-238F-E311-B8EF-534E57000000   // 7-Eleven: Test 3 Roomblocks - 2015   // Roomblocks 1-7 July 2015

// 1105 MEDIA, Inc: TEst REad Only - 2015
// 4CA85EE6-0793-E311-ACA7-534E57000000

// (Cloned) Test 2 - 
//		Missing Last DAy
// Dynamicgridview.aspx?username=TestUser&recordid=%7BEF57D39E-96F7-E311-AF0B-534E57000000%7D

//http://10.0.0.3:8082/RoomBlocks/Dynamicgridview.aspx?username=TestUser&recordid=%7B19D3942D-2D8D-E311-9D78-534E57000000%7D		
		
		 *Test new event (no total/peak roomblock)
		 *D32D4BDD-A994-E311-ACA7-534E57000000
		 *Dynamicgridview.aspx?recordid=%7BB54E175A-36B0-E311-93E7-534E57000000%7D&username=maria leahy
		 *Dynamicgridview.aspx?recordid=%7BD32D4BDD-A994-E311-ACA7-534E57000000%7D&username=maria leahy
		 
		 */

		public static Grid ThisIsMe = null;

		//ytodo remove
		public class SomeDTO
		{
			public string bolID { get; set; }
			public string controlID { get; set; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			//RoomBlocks2.Grid.dtPersistant = (System.Data.DataTable)ViewState["datatable"];

			ThisIsMe = this;

			if (!IsPostBack)
			{
				//Page.DataBind();
				PageLoad();
			}
			else
			{
				pageload_EventData = null;

				string senderid = Request["__EVENTTARGET"];
				string parameter = Request["__EVENTARGUMENT"];
				if ("zzz" == senderid)
					btnDummy_EventHandler_OnClick(parameter, null);

				/*if (parameter == "param1")
					MyButton_Click(sender, e);

				SomeDTO deserializedArgs = 
					  JsonConvert.DeserializeObject<SomeDTO>(Request["__EVENTARGUMENT"]);
				  var bolID = deserializedArgs.bolID;
				  var controlID = deserializedArgs.controlID;
				}*/

			}
		}

		bool saveSuccessfull = false;
		protected void btnSave_Clicked(object sender, EventArgs e)
		{
			try
			{
				RoomBlocks2.Grid._service = CrmServiceManager.GetCrmService();
				SaveUpdateGrid();
			}
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
			catch (Exception ex)
			{
				logger.Error("btnsave_Click", ex);
			}
		}

		protected void btnSaveOriginal_Clicked(object sender, EventArgs e)
		{
			RoomBlocks2.Grid._service = CrmServiceManager.GetCrmService();
			BackupRoomblocksAsOriginals();
		}

		// ONLY FOR DEBUGGING
		private void DebuggingInterface(EventActualRoomblocks eventWrapper)
		{
			// from main textbox  d0 or p0 to modify extend_actual_days
			Debug_DeleteRowZeroBased(eventWrapper);
		}
		// ONLY FOR DEBUGGING
		private int Debug_extractExtendday(string s)
		{
			System.Text.RegularExpressions.Regex getRow = new System.Text.RegularExpressions.Regex("[^\\d]");
			string extendedActualDays = getRow.Replace(s, "");
			try
			{
				return Convert.ToInt32(extendedActualDays);
			}
			catch
			{
				return -1;
			}
		}
		public int Debug_ExtendedActualDays()
		{
			if (!this.dbg.Visible)
				return -1;

			int days = Debug_extractExtendday(this.txtDbgExtendDays.Text);
			if (-1 != days)
				return days;
			else{
				if (this.txtDbgActualized.Text.IndexOf("p") >= 0) //p from prior - post
					return Debug_extractExtendday(this.txtDbgActualized.Text);
			}

			return -1;

			//// Allow delete record for testing d1 => delete row number 1
			//if (this.txtDbgActualized.Text.IndexOf("p") >= 0)
			//{
			//	System.Text.RegularExpressions.Regex getRow = new System.Text.RegularExpressions.Regex("[^\\d]");
			//	string extendedActualDays = getRow.Replace(this.txtDbgActualized.Text, "");
			//	try
			//	{
			//		return Convert.ToInt32(extendedActualDays);
			//	}
			//	catch
			//	{
			//		return -1;
			//	}
			//}
			//return -1;
		}
		// ONLY FOR DEBUGGING
		private void Debug_DeleteRowZeroBased(EventActualRoomblocks eventWrapper)
		{
			if (!this.dbg.Visible)
				return;

			// Allow delete record for testing d1 => delete row number 1
			if (this.txtDbgActualized.Text.IndexOf("d") >= 0)
			{
				System.Text.RegularExpressions.Regex getRow = new System.Text.RegularExpressions.Regex("[^\\d]");
				string rownum = getRow.Replace(this.txtDbgActualized.Text, "");
				try
				{
					int row = Convert.ToInt32(rownum);
					CrmServiceManager.GetCrmService().Delete(New_roompattern.EntityLogicalName,
						new Guid(new GridViewData(GridView1, eventWrapper.PersistantData).RoomGuid(row)));
				}
				catch
				{
					return;
				}
			}
		}
		// ONLY FOR DEBUGGING
		protected void btnDbugUpdateDateRange_click(object sender, EventArgs e)
		{
			RoomBlocks2.Grid._service = CrmServiceManager.GetCrmService();
			EventActualRoomblocks eventWrapper = new EventActualRoomblocks(this, this.EventId, true);

			DebuggingInterface(eventWrapper);

			DateTime dtDbgArrive = Convert.ToDateTime(txtDbgArrivalDate.Text);
			DateTime dtDbgDepart = Convert.ToDateTime(txtDbgDepartureDate.Text);

			if (eventWrapper.IsNewDateRange(dtDbgArrive, dtDbgDepart))
			{
				// If Range has been changed set it for Event
				Opportunity o = new Opportunity();
				o.Id = new Guid(EventId);
				o.New_arrivaldate = dtDbgArrive;
				o.New_departuredate = dtDbgDepart;
				Update(o);
			}

			ReloadGrid();
		}

		public void ReloadGrid()
		{
			// Reload grid
			// First reload Events with new updated values/dates
			EventActualRoomblocks
			eventWrapper = new EventActualRoomblocks(this, this.EventId, true);
			eventWrapper.LoadRoomPatternGrid();
			eventWrapper.AddAdditionalActualRows();
			//ytodo see to combine these 2 above into one function
			// note called after
			//		UpdateTheOneAndOnly(, LoadRoomPatternGrid(
			//		the first actually calls the 2nd
			//		so incorporate into LoadRoomPatternGrid( and problem solved.
			//		checkout and implement
		}

		public void Debug_OnPostGridLoad_ShowAllRecords(EventActualRoomblocks eventWrapper)
		{
			if (eventWrapper.Debugging || "1" == System.Configuration.ConfigurationManager.AppSettings["testingRealRecords"])
			{
				Microsoft.Xrm.Sdk.EntityCollection entityCollection =
					eventWrapper.roomPattern.RecordsOrderedByDate();

				// Simulate totals/peak - add to the test grid
				// Get updated totals and peak
				EventActualRoomblocks eventWrapperUpdates = new EventActualRoomblocks(this, this.EventId, true);
				New_roompattern rp = new New_roompattern();
				rp.New_name = "Totals/Peak";
				/*rp.Id = new Guid(
					String.Format("Totals__-Peak-____-____-{0}", 
						eventWrapperUpdates.Bcmc_Actualized ? 
							"ActualsBased" : 
							"____________"));*/
				rp.Bcmc_OriginalpercentofPeak = eventWrapperUpdates.Event.New_HotelRoomNights;
				rp.Bcmc_OriginalRoomBlock = eventWrapperUpdates.Event.New_PeakHotelRoomNights;
				entityCollection.Entities.Add(rp);

				this.GridView_AllRecords.Style["border-top"] = "5px solid #E0E0E0";
				this.GridView_AllRecords.Style["margin-top"] = "45px";
				System.Data.DataTable dataTable = eventWrapperUpdates
					.BindToGrid(entityCollection, false, this.GridView_AllRecords);

				/*New_EventSite eventHistory = new New_EventSite();
				eventHistory.New_EventId = new Microsoft.Xrm.Sdk.EntityReference(Opportunity.EntityLogicalName, new Guid(this.EventId));
				ConditionExpression	Conditions = new Microsoft.Xrm.Sdk.DataCollection<ConditionExpression>{
					new ConditionExpression("new_eventid", ConditionOperator.Equal, new Microsoft.Xrm.Sdk.EntityReference(Opportunity.EntityLogicalName, new Guid(this.EventId))
				};*/

				tailEnd.InnerHtml = "";
				//Show event info
				FilterExpression filter = new FilterExpression();
				filter.AddCondition(new ConditionExpression(
						"opportunityid", ConditionOperator.Equal,
					//new Microsoft.Xrm.Sdk.EntityReference(Opportunity.EntityLogicalName, new Guid(this.EventId))
						String.Format("{{{0}}}", this.EventId)
						));
				QueryExpression q = new QueryExpression
				{
					EntityName = Opportunity.EntityLogicalName,
					ColumnSet = new ColumnSet(new string[] {
						"new_hotelroomnights",
						"new_peakhotelroomnights",
						"bcmc_actualizedentrycomplete",
						"bcmc_actualizeddayspriorevent", 
						"bcmc_actualizeddayspostevent",
						"bcmc_reportactualized",
						"new_arrivaldate",
						"new_departuredate"}), 
					Criteria = filter
				};
				DebugShowTable(q);

				//Display any event history too..
				filter = new FilterExpression();
				filter.AddCondition(new ConditionExpression(
						"new_eventid", ConditionOperator.Equal,
					//new Microsoft.Xrm.Sdk.EntityReference(Opportunity.EntityLogicalName, new Guid(this.EventId))
						String.Format("{{{0}}}", this.EventId)
						));
				QueryExpression query = new QueryExpression
				{
					EntityName = New_EventSite.EntityLogicalName,
					ColumnSet = new ColumnSet(true),
					Criteria = filter
				};

				DebugShowTable(query, new string[] { "new_cityid" });
			}
		}

		private void DebugShowTable(QueryExpression q, string[] forceViewOfFields = null)
		{
			bool tablenameWritten = false;
			Microsoft.Xrm.Sdk.EntityCollection resultCollection = _service.RetrieveMultiple(q);
			string htmlRows = "";
			foreach(var row in resultCollection.Entities)
			{
				string htmlCols = "";
				string htmlHeader = "";
				foreach(var col in row.Attributes)
				{
					string value = col.Value.ToString();

					if (col.Value is Microsoft.Xrm.Sdk.EntityReference
						|| col.Value is Microsoft.Xrm.Sdk.OptionSetValue
						|| col.Value is Guid)
					{
						if (null != forceViewOfFields && forceViewOfFields.Contains(col.Key))
						{
							if (col.Value is Microsoft.Xrm.Sdk.EntityReference)
								value = (col.Value as Microsoft.Xrm.Sdk.EntityReference).Name;
						}
						else
							continue; //don't want to see these now
					}

					if ("" == htmlRows)
					{
						htmlHeader = String.Format("{0}<td>{1}</td>", htmlHeader, col.Key.ToString());
					}
					htmlCols = String.Format("{0}<td>{1}</td>", htmlCols, value);
				}
				if ("" != htmlHeader)
				{
					if (!tablenameWritten)
						htmlHeader = String.Format("<th>{0}</th>", q.EntityName) + htmlHeader;
					htmlRows = String.Format("<tr>{0}</tr>", htmlHeader);
				}
				if ("" != htmlCols)
					htmlRows = String.Format("{0}<tr>{1}</tr>", htmlRows, htmlCols);
			}
			if ("" != htmlRows)
			{
				htmlRows = String.Format(@"
									<style type=text/css>
										.dbgOutput {{ border: solid black 1px;}}
									</style>
									<table class='dbgOutput'>{0}</table>", 
									htmlRows);
				tailEnd.InnerHtml += htmlRows;
			}
		}

		protected void btnDbugTestPluginONLY_click(object sender, EventArgs e)
		{
			BcmcLinqContext LinqProvider = new BcmcLinqContext(RoomBlocks2.Grid._service);
			var eventList = LinqProvider
				.OpportunitySet
				.Where(o => o.Id == new Guid(EventId))
				.ToList()
				;
			//ytodo specify fields

			Opportunity event_ = eventList[0];
			Response.Write(String.Format("totals/peak: {2} {3}", 
				event_.New_HotelRoomNights,
				event_.New_PeakHotelRoomNights));

			DbgInfo.InnerHtml = String.Format(
				"<div>arrival/departure: {0} {1}. </div>" +
				"<div> totals/peak: {2} {3}",
				event_.New_arrivaldate.Value.ToShortDateString(),
				event_.New_departuredate.Value.ToShortDateString(),
				event_.New_HotelRoomNights,
				event_.New_PeakHotelRoomNights
				);
		}


		// some hocus pocus which didn't work in the end
		private void DisabledTxtboxWithViewState(GridViewRowEventArgs e, string id)
		{
			Control ctlActualPercentOfPeak = e.Row.FindControl(id);
			if (null != ctlActualPercentOfPeak)
			{
				// This won't be served to the grid until there is actualized data
				TextBox txtActualPercentOfPeak = (TextBox)ctlActualPercentOfPeak;
				txtActualPercentOfPeak.Attributes.Add("readonly", "readonly"); //ensure will be in postback
				/*txtActualPercentOfPeak.BorderStyle = BorderStyle.None;
				txtActualPercentOfPeak.BorderWidth = Unit.Pixel(0);*/
				//taken care of in css
				txtActualPercentOfPeak.Enabled = false; //prevent focus and caret
			}
		}

		// hocus pocus - which works!!??
		// ReadOnly/Disabled field IN VIEWSTATE!!
		public bool showActuals = false;
		public void GridView1_PreRender(object sender, EventArgs e)
		{
			try
			{
				bool actualized = (null == ViewState["actualized"] ? false : (bool)ViewState["actualized"]);

	
				int row = 0;
				int actualcount = 0;
				foreach (GridViewRow editRow in GridView1.Rows)
				{
					GridViewData g = new GridViewData(GridView1, null);
					if (g.ActualBlock(row++).HasValue)
						actualcount++;
				}
				showActuals = actualcount > 0;

	
				//if (/*GridView1.EditIndex != -1 &&*/ actualized)
				{
					foreach (GridViewRow editRow in GridView1.Rows)
					{
						if (actualized)
						{
							// Remove disabled textbox from Current Roomblocks
							TextBox tb = (TextBox)editRow.FindControl(GridCtrlNames.txtCBlock.ToString());
							if (null != tb)//disabled
							{
								TableCell cell = (TableCell)tb.Parent;
								cell.Text = tb.Text;
							}
						}

						// Show Actuals without Textbox i.e. no option for data entry
						if (!actualized && showActuals 
							|| (null != pageload_EventData 
								&& pageload_EventData.Event.Bcmc_ActualizedEntryComplete.HasValue
								&& null != pageload_EventData.Event.Bcmc_ActualizedEntryComplete
								&& true == pageload_EventData.Event.Bcmc_ActualizedEntryComplete
								|| "1" == System.Configuration.ConfigurationManager.AppSettings["testingActualizedEntryComplete"])
							)
						{
							// Remove disabled textbox from Actual Roomblocks
							TextBox tb = (TextBox)editRow.FindControl(GridCtrlNames.txtActualBlock.ToString());
							if (null != tb) //disabled
							{
								TableCell cell = (TableCell)tb.Parent;
								cell.Text = tb.Text;

								if (e.ToString().IndexOf("HideMeTxtBox") >= 0)
								{
									tb.Visible = false;
									//tb.Style.Add("display", "none");
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(String.Format("_PreRender {0(}", ex));
			}
		}

		protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			// Add any holidays to the day or week.
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				string date = e.Row.Cells[(int)RoomBlocks2.Grid.Columns.Date].Text;
				date = GridViewData.FilterSpaces(date);

				if ("" == date)
					return;
				//if ((sender as GridView).id == "GridView1")
				//	; or sender == GridView1

				try
				{
					String holiday = HelperFunctions.getHoliday(DateTime.Parse(date), _service, Cache);
					if (String.IsNullOrEmpty(holiday))
						return;

					e.Row.Cells[(int)Columns.DayOfWeek].Text += "<span> (" + holiday + ")</span>";
					if ((holiday.ToLower() != "holiday" && holiday != "")
						|| (holiday.ToLower() == "holiday" /*&& e.Row.RowIndex % 2 == 0*/) )
						e.Row.Cells[(int)Columns.DayOfWeek].Attributes["Style"] = "background-color:#fafbe1;";//LightYellow;";
				}
				catch (Exception)
				{
					logger.Error(String.Format("_RowDataBound date={0}", date));
				}
			}
		}

		protected void btnDummy_EventHandler_OnClick(object sender, EventArgs e)
		{
			try
			{
				//Opportunity Event = new Opportunity();
				//Event.bcmc_ActualizedDaysPostEvent.
				//	++ save

				// Handler custom events
				if (sender is string)
				{
					string ctrlid = (sender as string).Split(',')[0];
					if (null != ctrlid && "" != ctrlid)
					{
						switch (ctrlid)
						{
							case "AddActualDayAbove":
								SaveActualsInPostSession(false, true);
								AddActualDays(1, false);
								break;
							case "RemoveActualDayAbove":
								SaveActualsInPostSession(true, true);
								AddActualDays(-1, false);
								break;
							case "AddActualDayBelow":
								SaveActualsInPostSession(false, false);
								AddActualDays(1);
								break;
							case "RemoveActualDayBelow":
								SaveActualsInPostSession(true, false);
								AddActualDays(-1);
								break;
							case "ActualEntryCompleted":
								string[] values = (sender as string).Split(',');
								if (values.Length > 1)
								{
									RoomBlocks2.Grid._service = CrmServiceManager.GetCrmService();
									Opportunity o = new Opportunity();
									o.Id = new Guid(this.EventId);

									if ("1" == values[1].Trim()) //yes
									{
										o.Bcmc_ActualizedEntryComplete = true;
									}
									else
									{
										o.Bcmc_ActualizedEntryComplete = false;
									}
									RoomBlocks2.Grid._service.Update(o);
									EventActualRoomblocks eventWrapper = new EventActualRoomblocks(this, this.EventId, true);
									if (true == o.Bcmc_ActualizedEntryComplete.Value)
									{
										eventWrapper.UpdateActualizedEventHistory();
									}
									//Assume Update/Save will always be called before this...? //ytodo maybe do xtra save
									//RoomBlocks2.Grid._service = CrmServiceManager.GetCrmService();
									//SaveUpdateGrid();
								}
 
								break;
						}
					}
				}
				ReloadGrid(); //regardless to ensure show hide actuals/ and debug grid columns gets setup as it should
				//logger.Info(String.Format("EventHandlerClicked"));
			}
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
			catch (Exception ex)
			{
				logger.Error("btnDummy_EventHandler_OnClick", ex);
			}
		}

		public class HideMeTxtBox : EventArgs
		{
			public HideMeTxtBox() { }
		}

		private void AddActualDays(int amount, bool below = true)
		{
			RoomBlocks2.Grid._service = CrmServiceManager.GetCrmService();
			EventActualRoomblocks eventWrapper = new EventActualRoomblocks(this, this.EventId, true);

			int newValue = -1;
			if (below)
				newValue = eventWrapper.DaysPostEvent + amount;
			else
				newValue = eventWrapper.DaysPriorEvent + amount;

			if (newValue >= 0)
			{
				bool doUpdate = false;
				Opportunity o = new Opportunity();
				if (below)
				{
					/*if ((o.bcmc_ActualizedDaysPostEvent.HasValue && null != o.bcmc_ActualizedDaysPostEvent
							&& newValue != o.bcmc_ActualizedDaysPostEvent)
						|| ((!o.bcmc_ActualizedDaysPostEvent.HasValue || null == o.bcmc_ActualizedDaysPostEvent)
							&& 0 != newValue))*/
					if (eventWrapper.DaysPostEvent != newValue)
					{
						o.bcmc_ActualizedDaysPostEvent = newValue;
						doUpdate = true;
					}
				}
				else
				{
					/*if ((o.bcmc_ActualizedDaysPriorEvent.HasValue && null != o.bcmc_ActualizedDaysPriorEvent
						&& newValue != o.bcmc_ActualizedDaysPriorEvent)
						|| ((!o.bcmc_ActualizedDaysPriorEvent.HasValue || null == o.bcmc_ActualizedDaysPriorEvent)
							&& 0 != newValue))*/
					if (eventWrapper.DaysPriorEvent != newValue)
					{
						o.bcmc_ActualizedDaysPriorEvent = newValue;
						doUpdate = true;
					}
				}
				if (doUpdate)
				{
					o.Id = new Guid(eventWrapper.EventId);
					eventWrapper.gridviewClass.Update(o);
					//ReloadGrid();
				}
			}
		}

	}
}