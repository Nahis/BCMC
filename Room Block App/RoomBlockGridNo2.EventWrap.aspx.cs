using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using log4net;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;

using System.Data;
using System.Collections;

//using System.Web.UI.HtmlControls;
//using System.Web.UI.WebControls.WebParts;
using Xrm; //XrmEarlyBinding;

using System.Configuration;

namespace RoomBlocks2
{
	using System.Linq;
	using Microsoft.Xrm.Sdk.Linq;
	using Microsoft.Xrm.Sdk.Query;

	public partial class Grid : System.Web.UI.Page
	{
		bool ActualsVisible(GridView gv)
		{
			int actualIndex;
			actualIndex = (int)RoomBlocks2.Grid.Columns.ActualBlock;

			return ((actualIndex + 1) <= gv.Columns.Count) && gv.Columns[actualIndex].Visible; //column may not exist, besides not being visible.
		}

		public class EventEntityDataBase
		{
			protected Opportunity eventEntity = null;

			public Opportunity Event { get { return eventEntity; } }

			protected bool ShowDebugColumns { get {
				return "1" == ConfigurationManager.AppSettings["testing"];
				//return false; 
			} }
			protected bool DisableSave2DB { get { return false; } }

			protected int extendDays = 2;
			protected virtual int ExtendDaysActual { get { return extendDays; } }

			public virtual bool Bcmc_Actualized
			{
				get
				{
					return /*ShowDebugColumns ?
							true:*/
						this.eventEntity.bcmc_ReportActualized.HasValue ?
						this.eventEntity.bcmc_ReportActualized.Value : false;
				}
			}

			public int StatusCode
			{
				get
				{
					return 
						this.eventEntity.StatusCode.Value;
						//(int)RoomBlocks2.Grid.EventStatusCodes.Actualized;
				}
			}

			protected int DefaultExtendedDaysForDev(int? day)
			{
				if (day.HasValue && null != day)
					return day.Value;

				return 0;
			}

			public virtual int DaysPriorEvent
			{
				get
				{
					return //ShowDebugColumns ?
						"1" == ConfigurationManager.AppSettings["testingActualizedPrior"] ?
						ExtendDaysActual :
						DefaultExtendedDaysForDev(this.eventEntity.bcmc_ActualizedDaysPriorEvent);
				}
			}
			public virtual int DaysPostEvent
			{
				get
				{
					return //ShowDebugColumns ?
						"1" == ConfigurationManager.AppSettings["testingActualizedPost"] ?
						ExtendDaysActual:
						DefaultExtendedDaysForDev(this.eventEntity.bcmc_ActualizedDaysPostEvent);
				}
			}
			/*
			 o.bcmc_ActualizedDaysPostEvent;
			 o.bcmc_ActualizedDaysPriorEvent;
			 o.bcmc_ReportActualized;
			 */

			// Normalize date - get rid of times
			public DateTime arrivalDate
			{
				get
				{
					return Convert.ToDateTime((this.eventEntity.New_arrivaldate ?? DateTime.MinValue).ToShortDateString());
				}
			}
			public DateTime departureDate
			{
				get
				{
					return Convert.ToDateTime((this.eventEntity.New_departuredate ?? DateTime.MinValue).ToShortDateString());
				}
			}
			public bool IsNewDateRange(DateTime dtArrive, DateTime dtDepart)
			{
				return (dtArrive != this.arrivalDate || dtDepart != this.departureDate);
			}
			public bool WithinPlannedDuration(DateTime dtDay)
			{
				return (dtDay >= this.arrivalDate && dtDay < this.departureDate);
			}

			public virtual string StatusCodeStr
			{
				get
				{
					return this.eventEntity.StatusCode.Value.ToString();
				}
			}


			public static bool DebugShowSecondGrid = new EventEntityDataBase().ShowDebugColumns;
			public static bool DebugDisableSaveToDB = new EventEntityDataBase().DisableSave2DB && new EventEntityDataBase().ShowDebugColumns; //only allow when showing debug data on screen (safety)
		}

		public class EventActualRoomblocks : EventEntityDataBase
		{
			enum AddToGridPos
			{
				top, bottom,
			};

			public Grid gridviewClass = null;
			private RoomPattern roomPattern_ = null;

			private string eventid;

			public EventActualRoomblocks(Grid gridviewClass, string eventid, bool delayedActualCalcuations = false)
			{
				this.gridviewClass = gridviewClass;
				this.eventid = eventid;

				FetchEventData();

				// We only want to all the pre/prior dates to the range
				// after the actual range has been loaded.
				// ytodo make this function call explicit i.e. remove from here.
				if (!delayedActualCalcuations)
					AddAdditionalActualRows();
			}

			public string EventId {get{
				return this.eventid;
			}}

			public GridView GV {get{
				return this.gridviewClass.GridView1;
			}}

			public RoomPattern roomPattern {get{
				if (null == roomPattern_)
					roomPattern_ =  new RoomPattern(this);
				return roomPattern_;
			}}

			public override bool Bcmc_Actualized
			{
				get
				{
					if (ShowDebugColumns && this.gridviewClass.dbg.Visible)
					{
						if (this.gridviewClass.txtDbgActualized.Text == "1")
							return true;
						else if (this.gridviewClass.txtDbgActualized.Text == "0")
							return false;
						else if (this.gridviewClass.txtDbgActualized2.Text == "1")
							return true;
						else if (this.gridviewClass.txtDbgActualized2.Text == "0")
							return false;
					}

					if ("1" == ConfigurationManager.AppSettings["testingActualized"])
						return true;
					else if ("0" == ConfigurationManager.AppSettings["testingActualized"])
						return false;

					return base.Bcmc_Actualized;
				}
			}

			public override int DaysPriorEvent
			{
				get
				{
					string value = ConfigurationManager.AppSettings["testingActualizedPrior"];
					if (null != value && "" != value)
					{
						int val;
						try
						{
							val = Convert.ToInt32(value);
							return val;
						}
						catch { }
					}

					return base.DaysPriorEvent;
				}
			}
			public override int DaysPostEvent
			{
				get
				{
					string value = ConfigurationManager.AppSettings["testingActualizedPost"];
					if (null != value && "" != value)
					{
						int val;
						try { 
							val = Convert.ToInt32(value);
							return val;
						}
						catch { }
					}

					return base.DaysPostEvent;
				}
			}

			protected override int ExtendDaysActual
			{
				get
				{
					if (ShowDebugColumns && this.gridviewClass.Debug_ExtendedActualDays() >= 0)
					{
						return this.gridviewClass.Debug_ExtendedActualDays();
					}

					return base.ExtendDaysActual;
				}
			}

			/*public override string StatusCodeStr
			{
				get
				{
					return
						ShowDebugColumns && "" != this.gridviewClass.txtDbgStatusCode.Text ? 
							this.gridviewClass.txtDbgStatusCode.Text
							:base.StatusCodeStr;
				}
			}*/
			
			public bool Debugging
			{
				get
				{
					return ShowDebugColumns || DebugShowSecondGrid;
				}
			}

			public void FetchEventData()
			{
				//if (!DataFetched)
				{
					if (Debugging)
					{
						// To Test Definite Status we need to post to db
						// so that plugins will not interfere.
						if ("" != this.gridviewClass.txtDbgStatusCode.Text)
						{
							Opportunity o = new Opportunity();
							o.Id = new Guid(this.eventid);
							o.StatusCode = new OptionSetValue(Convert.ToInt32(this.gridviewClass.txtDbgStatusCode.Text));
								//RoomBlocks2.Grid.EventStatusCodes.Definite;
							this.gridviewClass.Update(o);
						}
					}
					BcmcLinqContext LinqProvider = new BcmcLinqContext(RoomBlocks2.Grid._service);
					var eventEntity = LinqProvider
						.OpportunitySet
						.Where(o => o.Id == new Guid(this.eventid))
						//.Select(s => s.Id)
						// ?"bcmc_eventname", 
						// new_departuredate, new_arrivaldate
						//	"statuscode"
						.ToList()
						;
					//ytodo specify fields
					this.eventEntity = eventEntity[0];

					InitEventData();
				}
			}

			int TotalRoomBlocks_DBValue { get {
				int totalRooms = this.eventEntity.New_HotelRoomNights.HasValue ?
					this.eventEntity.New_HotelRoomNights.Value :
					0;
				return totalRooms;
			}
			}
			int PeakRoomBlocks_DBValue { get{
				int peakRooms = this.eventEntity.New_PeakHotelRoomNights.HasValue ?
					this.eventEntity.New_PeakHotelRoomNights.Value :
					0;
				return peakRooms;
			}
			}

			void InitEventData()
			{
				RoomBlocks2.Grid.logger.Info(String.Format(
					"Dbg:InitEventData()"
					)); 
				((HiddenField)gridviewClass.form1.FindControl("hdnfdEventID")).Value = this.eventid; //backup of viewstate?
				this.gridviewClass.EventId = this.eventid;


				RoomBlocks2.Grid.logger.Info(String.Format(
					"Dbg:InitEventData {0},{1}",
					this.gridviewClass.EventId, 0
					));

				//this.gridviewClass.EventStatusCode = ""; //ytodo what is this needed for
				this.gridviewClass.EventStatusCode = this.StatusCodeStr;

				this.gridviewClass.ViewState["actualized"] = this.Bcmc_Actualized;

				Debug_StatusBarUpdate(TotalRoomBlocks_DBValue, PeakRoomBlocks_DBValue);

				this.gridviewClass.AddDayButtonsAbove.Visible = this.Bcmc_Actualized;
				this.gridviewClass.AddDayButtonsBelow.Visible = this.Bcmc_Actualized;
				this.gridviewClass.RemoveActualDayAbove.Visible = this.DaysPriorEvent > 0;
				this.gridviewClass.RemoveActualDayBelow.Visible = this.DaysPostEvent > 0;
				//logger.Info("ViewState: evenit= " + eventid);

				this.gridviewClass.ActualEntryComplete.Visible = this.Bcmc_Actualized;
				if (this.Bcmc_Actualized)
				{
					if (this.eventEntity.Bcmc_ActualizedEntryComplete.HasValue
						&& null != this.eventEntity.Bcmc_ActualizedEntryComplete
						&& true == this.eventEntity.Bcmc_ActualizedEntryComplete.Value)
					{
						(this.gridviewClass.ActualEntryCompleted as global::System.Web.UI.HtmlControls.HtmlSelect)
							.Items.FindByText("Yes").Selected = true;
						(this.gridviewClass.ActualEntryCompleted as global::System.Web.UI.HtmlControls.HtmlSelect)
							.Items.FindByText("No").Selected = false;
					}
					else
					{
						(this.gridviewClass.ActualEntryCompleted as global::System.Web.UI.HtmlControls.HtmlSelect)
							.Items.FindByText("No").Selected = true;
						(this.gridviewClass.ActualEntryCompleted as global::System.Web.UI.HtmlControls.HtmlSelect)
							.Items.FindByText("Yes").Selected = false;
					}
				}
			}

			public void AddAdditionalActualRows()
			{
				bool ReportActualizedRoomBlocksChecked = this.Bcmc_Actualized;
				//int State = this.StatusCode;

				if (/*State == (int)EventStatusCodes.Actualized //does this need to handle definite too? //ytodo EventStatusCodes.Definite
					&&*/ ReportActualizedRoomBlocksChecked)
				{
					this.gridviewClass.ButtonMoveToOriginal.Enabled = false;
					ShowActuals(true);

					GridView gv = GetGridView;
					if (null == gv.DataSource)
						return; //probably an irrelevant debugging call using this class unappropriately i.e. in btnDbgClick...

					int changesMade = 0;

					int extraDay;
					// Add days to top of grid
					for (extraDay = 1; extraDay <= DaysPriorEvent; extraDay++)
					{
						if (AddDayToGrid(arrivalDate.AddDays(-1 * extraDay), gv, AddToGridPos.top))
							changesMade++;
					}
					// Add days below grid
					for (extraDay = 1; extraDay <= DaysPostEvent; extraDay++)
					{
						// Remove one day, as departure day is not included, as a booked day.
						if (AddDayToGrid(departureDate.AddDays(extraDay - 1), gv, AddToGridPos.bottom))
							changesMade++;
					}

					int ResaveNewDayNumberCount = 0; //!!WARNING!! Remove can cause actual db record delete.
					// RemoveExtraDays
					// Remove days from top of grid
					if (RemoveDaysFromGrid(this.arrivalDate.AddDays(-this.DaysPriorEvent), gv, AddToGridPos.top))
					{
						changesMade++;
						ResaveNewDayNumberCount++;
					}
					// Remove days from bottom of grid
					// Remove one day, as departure day is not included, as a booked day.
					if (RemoveDaysFromGrid(this.departureDate.AddDays(this.DaysPostEvent - 1), gv, AddToGridPos.bottom))
					{
						changesMade++;
						ResaveNewDayNumberCount++;
					}

					FixDayNumber(gv);
					gv.DataBind();
					string javascript = "";
					if (ResaveNewDayNumberCount > 0)
						this.gridviewClass.UpdateTheOneAndOnly(this);

					// This is now actual values mode, so we do the Total & Peak calculating,
					// from actuals and not from planned.
					// and only if we have made some changes
					// //ytodo seem would make sense to only update from Save/Update...
					if (changesMade > 0)
						RecalculateTotalNightsAndPeak();

				}
				else
				{
					ShowActuals(false);
					this.gridviewClass.ButtonMoveToOriginal.Enabled = true;
				}

				// Allow Actuals to be renders un editable by removing the textbox
				// when switching from actualEntryComplete yes/no.
				this.gridviewClass.pageload_EventData = this;
				this.gridviewClass.GridView1_PreRender(this.gridviewClass.GridView1, new HideMeTxtBox());
				this.gridviewClass.pageload_EventData = null;


				this.gridviewClass.Debug_OnPostGridLoad_ShowAllRecords(this);

				// Show update succesful alert post pageload if relevant.
				if (this.gridviewClass.saveSuccessfull)
				{
					this.gridviewClass.saveSuccessfull = false;

					this.gridviewClass.Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript",
						"notifyUpdateSuccessfull = true;",
						true);
				}
			}

			static int callNumber = 0;
			void DisableCol(GridView gv, string coltitle, bool show)
			{
				/*table = document.getElementById('{0}'); 
				if (internalData.row == 0) {{ //show header text
					if (table !== undefined) 
						document.write('first col=' +table.rows[0].cells[0].innerHTML+'<br/>');
					document.write(internalData.rownum+' '+ internalData.colnum+' '+ col.innerHTML+'<br/>');
				}}*/

				this.gridviewClass.Page.ClientScript.RegisterStartupScript(this.GetType(),
					"dbg" + callNumber++.ToString(),
					String.Format(@" extData = {{}}; extData.show = {2};
						ActOnColTitled('{0}', '{1}', extData, function (col, extData, internalData) {{ 
							show = extData.show; 
    						col.disabled = !show;
							inputs = col.getElementsByTagName('Input');
							for(i=0; i<inputs.length; i++) {{
								ipTxt = inputs[i];
document.write(ipTxt.id+' '+ipTxt.type+'   '+ipTxt.value+'   '+ipTxt.disabled+'<br/>');
								if (undefined !== ipTxt && null !== ipTxt) {{
									if ('hidden' == ipTxt.type) {{
									/*
									ipTxt.disabled = false;
									ipTxt.readOnly = false;
									*/
									}} else {{
				/*
				parent = ipTxt.parentNode;
				value = ipTxt.value;
				parent.removeChild(ipTxt);
				InsertDebugElement('lblCurrentBlock', parent.id, 'label', value);
				*/

									/*
									ipTxt.disabled = !show;
									ipTxt.readOnly = !show;
									*/
									}}
								}}
document.write(ipTxt.id+' '+ipTxt.type+'   '+ipTxt.value+'   '+ipTxt.disabled+'<br/>');
							}}
						}});
						",
						 gv.ID,
						 coltitle,//colnum, title has proven to be more reliable the column index
						 show.ToString().ToLower(),
						 GridCtrlNames.txtCBlock.ToString())
					, true);
			}

			int GetColNumfromTitle(GridView gridView, string title)
			{
				/*
				//GridView gridView = this.gridviewClass.gvRoomBlock;
				((DataControlField)gridView.Columns
							   .Cast<DataControlField>()
							   .Where(fld => (fld.HeaderText == title))
							   .SingleOrDefault()).Visible = false;
				 */
				int colnum = 0;
				foreach (DataControlField f in gridView.Columns)
				{
					if (title == f.HeaderText)
						return colnum;
					colnum++;
				}
				return -1;
			}

			void ShowColumn_Js2(GridView gv, Columns col, bool show)
			{
				this.gridviewClass.Page.ClientScript.RegisterStartupScript(this.GetType(),
					"dbg" + callNumber++.ToString(),
					String.Format(@"externalData={{}}; externalData.show={2}; externalData.title='{1}';
						ActOnColTitled('{0}', '{1}', externalData, 
							function (col, d) {{
								/*col.disabled = !{2};
								col.style.visibility = 'visible';*/
								if (d.show)
									col.className = col.className.replace( /(?:^|\s)hidden_boundfld(?!\S)/ , '' ); //remove
								else
									col.className += ' hidden_boundfld';
							}});
						",//document.write(d.title+' tag='+col.tagName +' id='+ col.id + '  class=' + col.className + '<br/>');
						 gv.ID,
						 Heading(col),
						 show.ToString().ToLower())
					, true);
			}

			void ShowColumn_Js____(GridView gv, int colnum, bool show)
			{
				this.gridviewClass.Page.ClientScript.RegisterStartupScript(this.GetType(),
					"dbg" + callNumber++.ToString(),
					String.Format(@"
						ActOnCol('{0}', {1}, {2}, function (col, show) {{
							col.disabled = !{2};
							col.style.visibility = 'visible';
							if (show)
								col.className = col.className.replace( /(?:^|\s)hidden_boundfld(?!\S)/ , '' ); //remove
							else
								col.className += ' hidden_boundfld';
						}});
						",
						 gv.ID,
						 colnum,
						 show.ToString().ToLower())
					, true);
			}

			void ShowColumn(GridView gv, int col, bool show)
			{
				gv.Columns[col].Visible = show;
			}

			void ActualActuallyExistIlegally()
			{
				int row = 0;
				int actualcount = 0;
				foreach (GridViewRow editRow in this.GV.Rows)
				{
					GridViewData g = new GridViewData(this.GV, this.PersistantData);
					if (g.ActualBlock(row++).HasValue)
						actualcount++;
				}
				this.gridviewClass.showActuals = actualcount > 0;
			}

			void ShowActuals(bool show)
			{
				/*
				GridView gridView = this.gridviewClass.gvRoomBlock;
				((DataControlField)gridView.Columns
							   .Cast<DataControlField>()
							   .Where(fld => (fld.HeaderText == "Current Block"))
							   .SingleOrDefault()).Visible = false;
				 */
				GridView gv;

				if (this.gridviewClass.GridView1.Columns.Count > 0)
				{
					gv = this.gridviewClass.GridView1;

					//ytodoRemovedTemp//--ShowColumn(this.gridviewClass.GridView1, (int)Columns.DayNumber, ShowDebugColumns);
					//ytodoRemovedTemp//--ShowColumn(this.gridviewClass.GridView1, (int)Columns.RoomBlockId, ShowDebugColumns); //ytodofix not to be relied on. not visible

					ShowColumn_Js2(this.gridviewClass.GridView1, Columns.DayNumber, ShowDebugColumns);
					ShowColumn_Js2(this.gridviewClass.GridView1, Columns.RoomBlockId, ShowDebugColumns);

					ActualActuallyExistIlegally();

					ShowColumn(this.gridviewClass.GridView1, 
						(int)Columns.ActualBlock /*+ (ShowDebugColumns ? 0:-3)*/, //ytodo instead of magic numbers use column titles
						show || this.gridviewClass.showActuals);
					ShowColumn(this.gridviewClass.GridView1, 
						(int)Columns.ActualPercentOfPeak /*+ (ShowDebugColumns ? 0 : -3)*/, 
						show || this.gridviewClass.showActuals);

					if (show)
					{
						DisableCol(gv,
							RoomBlocks2.Grid.Heading(Columns.CurrentBlock),
							//GetColNumfromTitle(gv, this.gridviewClass.GridColumnTitles[(int)Columns.CurrentBlock]),
							//(int)Columns.CurrentBlock, 
							false);
					}
				}

				//if (this.gridviewClass.GridView2.Columns.Count > 0)
				//{
				//	gv = this.gridviewClass.GridView2;

				//	ShowColumn(this.gridviewClass.GridView2, (int)Columns.ActualBlock, show);
				//	ShowColumn(this.gridviewClass.GridView2, (int)Columns.ActualPercentOfPeak, show);

				//	if (show)
				//	{
				//		DisableCol(gv,
				//			RoomBlocks2.Grid.Heading(Columns.CurrentBlock),
				//			//5, //GetColNumfromTitle(gv, this.gridviewClass.GridColumnTitles[(int)Columns.CurrentBlock]),
				//			//(int)Columns.CurrentBlock, 
				//			false);
				//	}

				//	ShowColumn(this.gridviewClass.GridView2, (int)Columns.DayNumber, ShowDebugColumns);
				//	ShowColumn(this.gridviewClass.GridView2, (int)Columns.RoomBlockId, ShowDebugColumns);
				//}

				//if (ShowDebugColumns)
				{
				}

				DebugView();
			}

			// Some debug stuff is in ShowActuals
			void Debug_StatusBarUpdate(int total, int peak)
			{
#if false
				string statusBarSeparatorStyle = " crmStyle-statusbar-separator"; //statusbar
				string statusBarValue = "crmStyle-statusbar-value"; //inlineValue

				this.gridviewClass.divTotalsLine.InnerHtml = //Replace all the HTML completely!!
				String.Format(@"
							<span class='inlineValue{8}'>Arrival/Departure: {2} - {3}</span>
							<span class='inlineValue{8}'>Status: {4}{5}</span>" +
							"{6}" + //actualized
							"<span class='inlineValue {7}'>Total Room Nights:  </span>"+
							"<span id='totalRooms' class='inlineValue statusbar{7}'>{0}</span>"+

							"<span class='inlineValue {7}'>Peak Room Nights:  </span>" +
							@"<span id='peakRooms' class='inlineValue{8}{7}'>{1}</span>
",
							/*0*/total,
							/*1*/peak,
							/*2*/this.eventEntity.New_arrivaldate.Value.ToShortDateString(),
							/*3*/this.eventEntity.New_departuredate.Value.ToShortDateString(),
							/*4*/this.eventEntity.StatusCode.Value,
							/*5*/this.eventEntity.StatusCode.Value == (int)RoomBlocks2.Grid.EventStatusCodes.Definite
												? " (Definite)" : "",
							/*6*/! this.Bcmc_Actualized ? "" : String.Format(
								"<span class='inlineValue{2} crmStyle-actualized'>ACTUALIZED  "+
									"<span class='crmStyle-actualized'>" +
										"<div class='actualized-days'>Prior: {0}</div>" +
										"<div class='actualized-days'>Post: {1}</div></span></span>",
									this.DaysPriorEvent,	//0
									this.DaysPostEvent,		//1
									statusBarSeparatorStyle),//2" crmStyle-statusbar-separator"
							/*7*/this.Bcmc_Actualized ? " crmStyle-actualized": "",
							/*8*/statusBarSeparatorStyle
							);
#endif
#if true
				if (this.gridviewClass.mySB_lhs.Visible)
				this.gridviewClass.mySB_lhs.InnerHtml = String.Format(
					@"<span >Arrival/Departure: <span id=mySB_AD class='fld-value'>{0} - {1}</span></span>
						<span> Status: <span class='fld-value'>{2}{3}</span></span>
						" + (! this.Bcmc_Actualized ? "" : String.Format(
						@"<span ><span class='fld-value'>ACTUALIZED  </span>
							<div >Prior: <span class='fld-value'>{0}</span></div>
							<div >Post: <span class='fld-value'>{1}</span></div></span>
						",
							this.DaysPriorEvent,	//0
							this.DaysPostEvent)) +
						@"<span >Total Room Nights: <span class='fld-value'>{4}</span></span>
						<span >Peak Room Nights: <span class='fld-value'>{5}</span></span>
						</span>",
						/*0*/this.eventEntity.New_arrivaldate.Value.ToShortDateString(),
						/*1*/this.eventEntity.New_departuredate.Value.ToShortDateString(),
						/*2*/this.eventEntity.StatusCode.Value,
						/*3*/this.eventEntity.StatusCode.Value == (int)RoomBlocks2.Grid.EventStatusCodes.Definite
											? " (Definite)" : "",
						/*4*/total,
						/*5*/peak
					);
#endif
			}
			void DebugView()
			{
				this.gridviewClass.dbg.Visible = ShowDebugColumns;

				//this.gridviewClass.divTotalsLine.Visible = ShowDebugColumns || "1" == ConfigurationManager.AppSettings["ShowStatusBar"];
				this.gridviewClass.mySB_lhs.Visible = ShowDebugColumns || "1" == ConfigurationManager.AppSettings["ShowStatusBar"];

				this.gridviewClass.AddDayButtonsAbove.Visible = this.Bcmc_Actualized && (ShowDebugColumns || "1" == ConfigurationManager.AppSettings["ShowAddDayBtns"]);
				this.gridviewClass.AddDayButtonsBelow.Visible = this.Bcmc_Actualized && (ShowDebugColumns || "1" == ConfigurationManager.AppSettings["ShowAddDayBtns"]);

				this.gridviewClass.txtDbgArrivalDate.Visible = ShowDebugColumns;
				this.gridviewClass.txtDbgDepartureDate.Visible = ShowDebugColumns;
				this.gridviewClass.txtDbgActualized.Visible = ShowDebugColumns;
				this.gridviewClass.btnDbgUpdateDateRange.Visible = ShowDebugColumns;

				if (ShowDebugColumns)
				{
					this.gridviewClass.txtDbgArrivalDate.Text = this.arrivalDate.ToShortDateString();
					this.gridviewClass.txtDbgDepartureDate.Text = this.departureDate.ToShortDateString();
				}

				//this.gridviewClass.flTitle.Visible = ShowDebugColumns;
				this.gridviewClass.rbTitle.Visible = ShowDebugColumns;
				//this.gridviewClass.flTitle.Style["display"] = ShowDebugColumns ? "block;" : "none;";
				this.gridviewClass.rbTitle.Style["display"] = ShowDebugColumns ? "block;" : "none;";
				//this.gridviewClass.CheckBox_TogglePreAndPostWeek.Style["visibility"] = "none";
				//this.gridviewClass.CheckBox_TogglePreAndPostWeek.Style["display"] = "none";

				if (ShowDebugColumns)
				{
					this.gridviewClass.DbgInfo.Visible = true;
					this.gridviewClass.DbgInfo.Style["display"] = "block";

					this.gridviewClass.DbgInfo.InnerHtml = String.Format(
						"<div>arrival/departure: {0} {1}. </div>" +
						"<div>add top/below: {2} {3}.</div>" +
						"<div> totals/peak: {4} {5}",
						this.eventEntity.New_arrivaldate.Value.ToShortDateString(),
						this.eventEntity.New_departuredate.Value.ToShortDateString(),
						this.DaysPriorEvent,
						this.DaysPostEvent,
						this.eventEntity.New_HotelRoomNights,
						this.eventEntity.New_PeakHotelRoomNights
						);
				}

				// Javascript show/hides
				/*
				 * this.gridviewClass.Page.ClientScript.RegisterStartupScript(this.GetType(),
					"dbg",
					@"function setColAttr(tbl, colNum, attr, val) {
						var table = document.getElementById(tbl);
						if (null === table)
							return;
						for (var i = 0, row; row = table.rows[i]; i++) {
						   //iterate through rows
						   //rows would be accessed using the 'row' variable assigned in the for loop
						   for (var j = 0, col; col = row.cells[j]; j++) {
							 //iterate through columns
							 //columns would be accessed using the 'col' variable assigned in the for loop
							 //console.log(col.innerHTML)
							 if (j == colNum)
							 {
								//col.setAttribute('style','background-color:green;');
								// cancel the hidden_boundfld style and show column
								col.setAttribute(attr, val);
							}
						   }  
						}
					 }"
					, true);
				 */

				// cancel the hidden_boundfld style and show column
				//ShowColumn_Js(this.gridviewClass.GridView2, (int)Columns.RoomBlockId, ShowDebugColumns);
				if (ShowDebugColumns)
					ShowColumn_Js2(this.gridviewClass.GridView1, Columns.RoomBlockId, ShowDebugColumns); //ytodofix base on title Visible can mess this up by removing a col

				/*
				this.gridviewClass.Page.ClientScript.RegisterStartupScript(this.GetType(), 
					"dbg1",
					String.Format(
					@"
					showCol('gvRoomBlock', {3}, {1});
					showCol('gvFormLoad', {2}, {1});
						",
							(ShowDebugColumns ? "" : "hidden_boundfld"),
							ShowDebugColumns.ToString().ToLower(),
							Columns.RoomBlockId.ToString(),
							Columns.RoomBlockId.ToString())
					, true);
				*/
				/*					@"  //function onloadScripts() {}
				//alert('hey1');
										setColAttr('gvRoomBlock', 8, 'class', '{0}');
										setColAttr('gvFormLoad', 8, 'class', '{0}');
										var g = document.getElementById('gvFormLoad');
										if (null !== g)
											g.setAttribute('visibility', 'visible');

										//runOnLoad();
				//alert('hey2');
										",
							 */
			}

			public void UpdateMode()
			{
				this.gridviewClass.ButtonSave.Text = "Update";
				this.gridviewClass.ButtonMoveToOriginal.Enabled = true;
			}

			GridView GetGridView
			{
				get
				{
					return GV;
					//GridView gv;
					//int fl = this.gridviewClass.GridView1.Rows.Count;
					//int rb = this.gridviewClass.GridView2.Rows.Count;
					//if ((fl == rb && fl == 0)
					//	|| rb > fl)
					//	gv = this.gridviewClass.GridView2;
					//else
					//	gv = this.gridviewClass.GridView1;

					//return gv;
				}
			}

			bool DayAlreadyInGrid(DateTime day)
			{
				string _day = day.ToShortDateString();
				for (int rownum = 0; rownum < this.GetGridView.Rows.Count; rownum++)
				{
					//*[@id="gvRoomBlock"]/tbody/tr[8]/td[2]
					// Extract 'Date' Column
					if (_day == this.GetGridView.Rows[rownum].Cells[(int)Columns.Date].Text)
						return true;
				}
				return false;
			}

			int GetInsertPositionInGrid(DateTime day, AddToGridPos addToGridPos)
			{
				for (int rownum = 0; rownum < this.GetGridView.Rows.Count; rownum++)
				{
					DateTime GridDay = Convert.ToDateTime(this.GetGridView.Rows[rownum].Cells[(int)Columns.Date].Text);
					if (day < GridDay)
						return rownum;
				}
				return -1;
			}

			bool AddDayToGrid(DateTime dateToBeAdded, GridView gv, AddToGridPos addToGridPos)
			{
				if (!DayAlreadyInGrid(dateToBeAdded))
				{
					DataTable dt = gv.DataSource as DataTable;
					if (null == dt)
						return false;

					DataRow dr = dt.NewRow();
					dr[Heading(Columns.DayNumber)] = 0; // this will be corrected by the fix function.
					dr[Heading(Columns.DayOfWeek)] = dateToBeAdded.DayOfWeek.ToString();

					dr[Heading(Columns.Date)] = dateToBeAdded.ToShortDateString();
					dr[Heading(Columns.RoomBlockId)] = "";

					//restore html post session values, that were not saved to the db
					// but were uploaded to the server in viewstate when adding/removing actual days
					// This is done in fix up day once we the complete grid and indexing.


					int insertPos = GetInsertPositionInGrid(dateToBeAdded, addToGridPos);
					if (-1 == insertPos)
						dt.Rows.Add(dr);
					else
						dt.Rows.InsertAt(dr, insertPos);

					gv.DataSource = dt;

					return true;
				}

				return false;
			}

			void RemoveDayOutsideOfPriorPostActualExtendedRange(DataTable dataTable, GridView gv, int row)
			{
				RoomBlocks2.Grid.logger.Info(String.Format(
					"Deleting Actual Out of Range Date {0},{1}",
					row, dataTable.Rows[row].Field<string>(Heading(Columns.Date))
					));
				RoomBlocks2.Grid._service.Delete(New_roompattern.EntityLogicalName, 
					new Guid(//new GridViewData(gv, this.PersistantData).RoomGuid(row))
						dataTable.Rows[row].Field<string>(Heading(Columns.RoomBlockId))
						));
				dataTable.Rows.RemoveAt(row);
			}

			bool RemoveDaysFromGrid(DateTime date, GridView gv, AddToGridPos addToGridPos)
			{
				DataTable dt = gv.DataSource as DataTable;
				if (null == dt)
				{
					logger.Error("RemoveDaysFromGrid datasource null");
					return false;
				}

				if (AddToGridPos.bottom == addToGridPos)
				{
					DateTime latestDate = Convert.ToDateTime(dt.Rows[dt.Rows.Count - 1]["Date"]);
					TimeSpan period = latestDate - date;
					if (period.TotalDays > 0)
					{
						for (int i = 0; i < period.TotalDays; i++)
						{
							logger.Info(String.Format("Deleting out of range date {0}...", dt.Rows[dt.Rows.Count - 1]["Date"]));
							RemoveDayOutsideOfPriorPostActualExtendedRange(dt, gv, dt.Rows.Count - 1);
						}
						return true;
					}
				}
				else
				{
					DateTime earliestDate;
					try
					{
						earliestDate = Convert.ToDateTime(dt.Rows[0]["Date"]);
					}
					catch (Exception ex)
					{
						// Shouldn't occur if it does => bad record
						// Delete it and report the error, should NOT occur in production.
						dt.Rows.RemoveAt(0);
						//RemoveDayOutsideOfPriorPostActualExtendedRange(dt, gv, 0);

						logger.Error(String.Format("RemoveDaysFromGrid date={0}: {1}", dt.Rows[0]["Date"], ex.Message));

						return true;
					}

					TimeSpan period = date - earliestDate;
					if (period.TotalDays > 0)
					{
						for (int i = 0; i < period.TotalDays; i++)
						{
							logger.Info(String.Format("Deleting out of range date {0}...", dt.Rows[0]["Date"]));
							RemoveDayOutsideOfPriorPostActualExtendedRange(dt, gv, 0);
						}
						return true;
					}
				}

				return false;
			}

			void FixDayNumber(GridView gv)
			{
				//Calculate which ActualBlocks rows may need to be saved.
				int firstActualRowToBeSaved, 
					lastActualRowToBeSaved;
				new EventActualRoomblocks.ActualRowsToBeSaved(this)
					.FirstLastRows(
						out firstActualRowToBeSaved, 
						out lastActualRowToBeSaved, 
						true);

				DataTable dt = gv.DataSource as DataTable;

				int gridIndex = 0, daynumber = 1;
				foreach (DataRow row in dt.Rows)
				{
					// Don't number rows that will not be saved.
					if (gridIndex >= firstActualRowToBeSaved && gridIndex <= lastActualRowToBeSaved)
						row[Heading(Columns.DayNumber)] = (daynumber++).ToString();
					else
						row[Heading(Columns.DayNumber)] = "";
					gridIndex++;


					//restore html post session values, that were not saved to the db
					// but were uploaded to the server in viewstate when adding/removing actual days
					// This is done in fix up day once we the complete grid and indexing.
					Dictionary<string, string> myDataTablePersisted = (Dictionary<string, string>)RoomBlocks2.Grid.ThisIsMe.
						ViewState["tableData"];

					if (null != myDataTablePersisted)
					{
						string value;
						string hash = String.Format("{0}{1}", RoomBlocks2.Grid.Heading(Columns.ActualPercentOfPeak), gridIndex-1);
						//if (myDataTablePersisted.Contains(hash))
						{
							value = null;
							int? intValue = null;
							try
							{
								value = myDataTablePersisted[hash];
							}
							catch { }
							if (null != value && "" != value)
							{
								intValue = Convert.ToInt32(value);
								if (intValue.Value == 0)
									intValue = null;
							}
							DR(row, Columns.ActualPercentOfPeak, intValue);
						};

						hash = String.Format("{0}{1}", RoomBlocks2.Grid.Heading(Columns.ActualBlock), gridIndex-1);
						//if (myTable.Contains(hash))
						{
							value = null;
							int? intValue = null;
							try { value = myDataTablePersisted[hash]; }
							catch { }
							if (null != value && "" != value)
							{
								intValue = Convert.ToInt32(value);
								if (intValue.Value == 0)
									intValue = null;
							}
							DR(row, Columns.ActualBlock, intValue);
						};
					}
				}

				gv.DataSource = dt;
			}

			public class ActualRowsToBeSaved
			{
				public ActualRowsToBeSaved(EventActualRoomblocks eventWrapper)
				{
					g = new GridViewData(eventWrapper.GV, eventWrapper.PersistantData);
					This = eventWrapper;
				}
				GridViewData g = null;
				EventActualRoomblocks This = null;

				bool HasActual(int row)
				{
					int? actual = g.ActualBlock(row);
					return actual.HasValue && null != actual;
				}

				// Calculate which of the prior/post days to save.
				public void FirstLastRows(out int firstrownum, out int lastrownum, 
					bool LoadingUserDataSource = false)
				{
					g.fromDataSource = LoadingUserDataSource; //loading to OR saving from grid

					int extraDay;

					// First Prior Actual day
					firstrownum = This.DaysPriorEvent;

					for (extraDay = 0; extraDay < This.DaysPriorEvent; extraDay++)
					{
						if (HasActual(extraDay))
						{
							firstrownum = extraDay;
							break;
						}
					}


					// First ActualBlock post to Planned Event Duration
					int firstPostRow = this.g.Count - This.DaysPostEvent; //Count is ALL rows, also pre & post.
					lastrownum = firstPostRow - 1;

					for (extraDay = firstPostRow; extraDay < firstPostRow+This.DaysPostEvent; extraDay++)
					{
						if (HasActual(extraDay))
						{
							lastrownum = extraDay;
						}
					}
				}
			}


			/// <summary>
			/// /////////////////////////////////////////////
			/// Update tables: Event/Opportunity and New_Roompattern
			/// </summary>

			public string RecalculateTotalNightsAndPeak()
			{
				return UpdateEvent_withCalc_TotalRoomblocksAndPeak(
					this.GetGridView,
					this.gridviewClass.EventId);

				//ResetPeakRoomNightsIfNoRoomblocks
			}

			public string UpdateEvent_withCalc_TotalRoomblocksAndPeak(GridView gv, string _eventID)
			{
				// Use actual values if possible
				string ctlId;
				if (this.Bcmc_Actualized)// gv.Columns[(int)Columns.ActualBlock].Visible)
					ctlId = GridCtrlNames.txtActualBlock.ToString();
				else
					ctlId = GridCtrlNames.txtCBlock.ToString();

				int _peakRoomNight = 0;
				int _totalRoomBlock = 0;
				try
				{
					for (int i = 0; i < gv.Rows.Count; i++)
					{
						int current_block = 0;
						TextBox txtboxCurrentRoomblock = (TextBox)gv.Rows[i].FindControl(ctlId);
						if (null != txtboxCurrentRoomblock.Text
							&& txtboxCurrentRoomblock.Text != string.Empty)
							current_block = int.Parse(txtboxCurrentRoomblock.Text);

						_totalRoomBlock += current_block;

						if (current_block > _peakRoomNight)
							_peakRoomNight = current_block;
					}
					//logger.InfoFormat("BindPeakRoom: TotalHotelRooms={0} Peak={1} Event={2}",
					//	_totalRoomBlock, _peakRoomNight, _eventID);

					return UpdateEventRoomNights(_eventID, _totalRoomBlock, _peakRoomNight);
				}
				catch (System.Web.Services.Protocols.SoapException ex)
				{
					logger.Error(ex.Detail.InnerText, ex);
				}
				catch (Exception ex)
				{
					logger.Error("BindPeakroom " + ex.ToString());
				}

				// Build the JavaScript code to be used to update the CRM event form.
				return String.Format("updateParent({0}, {1}); ",
								_totalRoomBlock, _peakRoomNight); //ytodo not currently working
			}

			public string UpdateEventRoomNights(string eventid, int totalRoomBlocks, int peakRoomBlock)
			{
				try
				{
					this.gridviewClass.error.Visible = false;

					if (this.eventEntity.New_HotelRoomNights.HasValue
						&& null != this.eventEntity.New_HotelRoomNights
						&& this.eventEntity.New_HotelRoomNights.Value == totalRoomBlocks
						&& this.eventEntity.New_PeakHotelRoomNights.HasValue
						&& null != this.eventEntity.New_PeakHotelRoomNights
						&& this.eventEntity.New_PeakHotelRoomNights.Value == peakRoomBlock)
						return ""; //no need for an update.

					Opportunity o = new Opportunity();
					o.OpportunityId = new Guid(eventid);
					o.New_HotelRoomNights = totalRoomBlocks;
					o.New_PeakHotelRoomNights = peakRoomBlock;
					o.bcmc_actualhotelroomnights = totalRoomBlocks;
					o.bcmc_actualpeakhotelroomnights = peakRoomBlock;

					this.gridviewClass.Update(o);

					// prevent a second save... a coarse solution
					// by updating to what should now be in db.
					this.eventEntity.New_PeakHotelRoomNights = peakRoomBlock;
					this.eventEntity.New_HotelRoomNights = totalRoomBlocks;

				}
				catch (System.Web.Services.Protocols.SoapException ex)
				{
					logger.Error(ex.Detail.InnerText, ex);
				}
				catch (Exception ex)
				{
					logger.Error("UpdateEventRoomNights " + ex.ToString());
					this.gridviewClass.error.InnerHtml = "Error: Failed to update CRM!<br/>Please retry or report to administrator<br/>"+ ex.Message;
					this.gridviewClass.error.Visible = true;
				}

				Debug_StatusBarUpdate(totalRoomBlocks, peakRoomBlock);

				// Build the JavaScript code to be used to update the CRM event form.
				return String.Format("updateParent({0}, {1}); ",
								totalRoomBlocks, peakRoomBlock); //ytodo not currently working
			}

			public void UpdateActualizedEventHistory()
			{
				UpdateActualizedEventHistory(
					this.TotalRoomBlocks_DBValue, 
					this.PeakRoomBlocks_DBValue
					/*this.eventEntity.New_HotelRoomNights.Value, 
					this.eventEntity.New_PeakHotelRoomNights.Value*/);
			}

			public void UpdateActualizedEventHistory(int totalRoomNights, int peakRoomNights)
			{
				// Update Event History
				/*if (this.eventEntity.Bcmc_ActualizedEntryComplete.HasValue
					&& null != this.eventEntity.Bcmc_ActualizedEntryComplete
					&& true == this.eventEntity.Bcmc_ActualizedEntryComplete
					|| "1" == System.Configuration.ConfigurationManager.AppSettings["testingActualizedEntryComplete"])*/
				{
					New_EventSite eventHistory = new New_EventSite();
					eventHistory.New_EventId = new EntityReference(Opportunity.EntityLogicalName, new Guid(this.EventId));
					eventHistory.New_History = this.eventEntity.BCMC_EventYear;
					eventHistory.BCMC_TotalRoomNights = totalRoomNights;
					eventHistory.BCMC_PeakBlockActualized = peakRoomNights;
					eventHistory.New_CityId = new EntityReference(Competitor.EntityLogicalName, 
						new Guid(System.Configuration.ConfigurationManager.AppSettings["eventSite_History_CompetitorCity_Guid"]));
					//eventHistory.Bcmc_ActualizedEntryComplete = true;
					eventHistory.New_Notes = "Actualized room nights have been updated.";
					this.gridviewClass.Create(eventHistory);
				}
			}

			public DataTable BindToGrid(EntityCollection entityCollection, bool RoomBlockHasNullDates = false, GridView GenericGV = null)
			{
				GridView gv = null == GenericGV ? this.GV : GenericGV;

				// Create Datatable fields /Grid Headers
				DataTable dataTable = InitDataTable();
				int HasRoomBlocks = 0;

				Dictionary<string, string> myDataTablePersisted = (Dictionary<string, string>)this.gridviewClass.ViewState["tableData"];
				if (null == myDataTablePersisted) 
					myDataTablePersisted = new Dictionary<string, string>();

				// Grid data rows from new_roompattern records
				for (int i = 0; i < entityCollection.Entities.Count; i++)
				{
					DataRow dataRow = dataTable.NewRow();
					New_roompattern new_roompatten = (New_roompattern)entityCollection.Entities[i];

					if (new_roompatten.New_roompatternId.HasValue)
						dataRow[RoomBlocks2.Grid.Heading(Columns.RoomBlockId)] = new_roompatten.New_roompatternId.Value.ToString();

					//dataRow[Heading(Columns.DayNumber)] = "0"; //default in case daynumber is null
					DR(dataRow, Columns.DayNumber, new_roompatten.New_DayNumber);
					DR(dataRow, Columns.DayOfWeek, new_roompatten.New_name);
					DR(dataRow, Columns.Date, new_roompatten.Bcmc_Date);
					DR(dataRow, Columns.OriginalPercentOfPeak, new_roompatten.Bcmc_OriginalpercentofPeak);
					DR(dataRow, Columns.OriginalBlock, new_roompatten.Bcmc_OriginalRoomBlock);

					// Definite Event and Duration Changed
					bool IgnorePrevRoomBlockValue = 
							!RoomBlockHasNullDates
							&& new_roompatten.Bcmc_Date.HasValue
							&& null != new_roompatten.Bcmc_Date
							&& !WithinPlannedDuration(new_roompatten.Bcmc_Date.Value);
					if (!IgnorePrevRoomBlockValue || null != GenericGV)
					{
						HasRoomBlocks +=
						DR(dataRow, Columns.CurrentPercentOfPeak, new_roompatten.New_PercentofPeak);
						DR(dataRow, Columns.CurrentBlock, new_roompatten.New_RoomBlock);
					}
					DR(dataRow, Columns.ActualPercentOfPeak, new_roompatten.Bcmc_ActualPercentOfPeak);
					DR(dataRow, Columns.ActualBlock, new_roompatten.Bcmc_ActualBlock);

					dataTable.Rows.Add(dataRow);

					if (null == GenericGV)
					{
						myDataTablePersisted[String.Format("{0}{1}", "RoomGuid", i)] = dataRow.Field<string>(Heading(Columns.RoomBlockId));
					}
				}

				// Show Generic all when no records available!
				if (null != GenericGV)
				{
					if (0 == entityCollection.Entities.Count)
					{
						DataRow dataRow = dataTable.NewRow();
						DR(dataRow, Columns.DayNumber, "No records found!");
						DR(dataRow, Columns.OriginalBlock, "No records found!");
						dataTable.Rows.Add(dataRow);
					}
				}

				if (null == GenericGV && HasRoomBlocks > 0)
				{
					UpdateMode();
				}

				// Update grid containing data from roomblock db 
				// for current date range in roomblocks.
				gv.DataSource = dataTable;
				gv.DataBind();

				if (null == GenericGV)
				{
					//gv.Columns[(int)Columns.RoomBlockId].Visible = EventEntityDataBase.DebugShowSecondGrid || bSHOW_ROOM_GUID ? true : false; //ytodofix
					//gv.Columns[(int)Columns.DayNumber].Visible = EventEntityDataBase.DebugShowSecondGrid || bSHOW_ROOM_GUID ? true : false;

					//PersistantData = dataTable; //ytodo get rid of
					this.gridviewClass.ViewState["tableData"] = myDataTablePersisted;
				}

				gv.Visible = true;

				return dataTable;
			}

			public object PersistantData
			{
				set //ytodo bad stuff needs fixing, I believe currently obsolete.
				{
					this.gridviewClass.ViewState["datatable"] = (DataTable)value;
					RoomBlocks2.Grid.dtPersistant = (DataTable)value;
				}
				get
				{
					DataTable dtPersistant = (DataTable)this.gridviewClass.ViewState["datatable"];
					if (null != dtPersistant)
						RoomBlocks2.Grid.dtPersistant = dtPersistant;
					return dtPersistant;
				}
			}

			public void LoadRoomPatternGrid(AllowRoomBlocksDelete AllowDeleteAllRoomBlocks = AllowRoomBlocksDelete.False)
			{
				try
				{
					if (!this.gridviewClass.HasEventId)
						return;

					// Get event dates.
					DateTime dtArrivalDate = this.arrivalDate;
					DateTime dtDepartureDate = this.departureDate;

					try
					{
						EntityCollection entityCollection = this.roomPattern.RecordsOrderedByDate();
						Hashtable hshTableRoomPattern = new Hashtable();
						if (entityCollection.Entities.Count == 0)
						{
							Grid.InitNewEmptyRoomBlockGrid(this);
							logger.Info(String.Format("Dbg:ld:InitNewEmptyRoomBlockGrid {0}", 0));
						}
						else
						{
							int NullDates = 0; //ytodo save doing a query, by using this instead
							int ActualDates = 0;

							//Detect date range changes
							// Fill in existing dates from new_roompattern records
							// into hash i.e. all dates will be unqiue
							logger.Info(String.Format("Dbg:ld:InitRoomBlockGrid {0}", entityCollection.Entities.Count));
							for (int j = 0; j < entityCollection.Entities.Count; j++)
							{
								New_roompattern new_roompatten = (New_roompattern)entityCollection.Entities[j];
								if (new_roompatten.Bcmc_ActualBlock.HasValue && null != new_roompatten.Bcmc_ActualBlock)
									ActualDates++;

								if (new_roompatten.Bcmc_Date != null)
									hshTableRoomPattern.Add("bcmc_date" + j, new_roompatten.Bcmc_Date.Value.ToShortDateString());
								else
								{
									NullDates++;
									hshTableRoomPattern.Add("bcmc_date" + j, "01/01/0001");
								}
							}

							// Date of first & last 
							// new_roompattern record
							string FirstRoomPatternRecordDate = hshTableRoomPattern["bcmc_date0"].ToString(); //"bcmc_date" + j
							string LastRoomPatternRecordDate = hshTableRoomPattern["bcmc_date" + (hshTableRoomPattern.Count - 1).ToString()].ToString();

							bool NOTFirstRoompatternDateIsNull = (FirstRoomPatternRecordDate != "01/01/0001"); // i.e. do they have Dates?

							if (this.gridviewClass.EventStatusIsActualizedOrDefinite
								|| NOTFirstRoompatternDateIsNull || NullDates > 0)
							{
								dtDepartureDate = dtDepartureDate.AddDays(-1);

								bool NoEventDurationChange_OR_Actualized =
									// If actualized there should not be anymore date range changes, only pre and prior values may change.
									this.Bcmc_Actualized
									|| (!this.Bcmc_Actualized && this.Debugging) //allow entry in debugging mode even though its not a relevant state
									// If Event.arrival and departure dates
									// same as first and last dates in new_roompattern records
									|| (hshTableRoomPattern["bcmc_date0"].ToString() == dtArrivalDate.ToShortDateString()
											&& dtDepartureDate.ToShortDateString() ==
											hshTableRoomPattern["bcmc_date" + (hshTableRoomPattern.Count - 1).ToString()].ToString());

								if (this.gridviewClass.EventStatusIsActualizedOrDefinite
									|| NoEventDurationChange_OR_Actualized
									|| ActualDates > 0
									|| (int)this.eventEntity.StateCode.Value == 2) //otherwise allow possible deletion of roomblocks
								{
									EntityCollection NullDateRoomPatternDays = this.roomPattern.NullDates();
									bool RoomBlockHasNullDates = (NullDateRoomPatternDays.Entities.Count > 0);

									DataTable dataTable = BindToGrid(entityCollection/*, RoomBlockHasNullDates*/);

									// Definite Event & Duration has changed
									// Disable days out of range
									if (true/*!RoomBlockHasNullDates*/)
										for (int i = 0; i < dataTable.Rows.Count; i++)
										{
											if (!WithinPlannedDuration(Convert.ToDateTime(dataTable.Rows[i][Heading(Columns.Date)].ToString())))
											{
												logger.Info(String.Format("Disable date row {0}",
													dataTable.Rows[i][Heading(Columns.Date)].ToString()));
												GV.Rows[i].Enabled = false;
											}
										}

									// Removed this code cause it doesn't make any sense
									//if (!formloadWithExistingRecords)
									//{
									//	EntityCollection NullDateRoomPatternDays = this.roomPattern.NullDates();
									//	bool RoomBlockHasNullDates = (NullDateRoomPatternDays.Entities.Count > 0);

									//	// Reset CurrentBlock & %ofPeak if outside Arrival-Departure Range
									//	if (!RoomBlockHasNullDates)
									//	{
									//		// reset roomblock days not in the event range to 0

									//		// Compare date range between that in event
									//		// to that fetched from roomblocks.
									//		// Initialize missing roomblock days/date to 0
									//		for (int i = 0; i < dataTable.Rows.Count; i++) //roomblocks records
									//		{
									//			string dayDate = dataTable.Rows[i]["Date"].ToString();
									//			//Added this code for check the existing records in crm, 
									//			//if those records does not having the date and has null means, 
									//			// we check the record and update it.


									//			// If a new date out of original range has been added
									//			// initialize roomblock to blank/0
									//			if (!IsDateInRange(dtArrivalDate, dtDepartureDate, dayDate))
									//			{
									//				dataTable.Rows[i]["Current Block"] = "";
									//				dataTable.Rows[i]["Current % of Peak"] = "";
									//			}
									//		}
									//	}

									//	// backup snapshot of current datatable? does this keep a full copy?
									//	dtRoomBlock = dataTable;

									//	// Update grid containing data from roomblock db 
									//	// for current date range in roomblocks.
									//	// with days not in the event range reset to 0
									//	GridView1.DataSource = dataTable;
									//	GridView1.DataBind();

									//	for (int i = 0; i < dataTable.Rows.Count; i++)
									//	{
									//		string recordDate = dataTable.Rows[i]["Date"].ToString();
									//		//string roomID = dataTable.Rows[i]["RoomGUID"].ToString();

									//		if (!RoomBlockHasNullDates
									//			&& !IsDateInRange(dtArrivalDate, dtDepartureDate, recordDate))
									//		{
									//			GridView1.Rows[i].Enabled = false;
									//		}
									//	}
									//}
								}
								else
								{
									if (AllowRoomBlocksDelete.True == AllowDeleteAllRoomBlocks)
									{
										logger.Info("Warning DeleteAll Roomblocks");
										this.roomPattern.DeleteRoomblock();
									}
									logger.Info(String.Format("Warning DeleteAll-new empty grid: E.statuscode={0}"
										+ " noChange/Actualized: {1}"
										+ " actualized: {2}"
										+ " {3}=={4}"
										+ " {5}=={6}"
										+ " actual dates {7}"
										+ " statecode {8}"
										,
										this.gridviewClass.EventStatusCode,
										NoEventDurationChange_OR_Actualized,
										this.Bcmc_Actualized,
										hshTableRoomPattern["bcmc_date0"].ToString(),
											dtArrivalDate.ToShortDateString(),
										dtDepartureDate.ToShortDateString(),
											hshTableRoomPattern["bcmc_date" + (hshTableRoomPattern.Count - 1).ToString()].ToString(),
										ActualDates,
										this.eventEntity.StateCode.Value
										));
									InitNewEmptyRoomBlockGrid(this);
								}
							}
							else
							{
								BindToGrid(entityCollection);
								this.gridviewClass.ViewState["DisplayDateisNULL"] = "DisplayDateisNULL";
							}
						}
					}
					catch (System.Web.Services.Protocols.SoapException ex)
					{
						logger.Error(ex.Detail.InnerText, ex);
					}
					catch (Exception ex)
					{
						//LogLastSuccessMsg("RetrieveBasedonStatus(0" + (formloadWithExistingRecords ? " otherFn" : ""));
						logger.Error("720 RetrieveBasedonStatus" + ex.ToString());
					}
				}
				catch (System.Web.Services.Protocols.SoapException ex)
				{
					logger.Error(ex.Detail.InnerText, ex);
				}
				catch (Exception ex)
				{
					//LogLastSuccessMsg("RetrieveBasedonStatus(1");
					logger.Error("721 RetrieveBasedonStatus" + ex.ToString());
				}

				this.gridviewClass.Debug_OnPostGridLoad_ShowAllRecords(this);
			}


		}//EventActualRoomBlocks class
	}
}

