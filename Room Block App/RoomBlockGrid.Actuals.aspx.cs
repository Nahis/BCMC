using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Xrm; //XrmEarlyBinding;	
using System.Data;

namespace RoomBlock1
{
	using System.Linq;
	using Microsoft.Xrm.Sdk.Linq;
	using Microsoft.Xrm.Sdk.Query;

    public partial class Grid : System.Web.UI.Page
	{
		//protected global::System.Web.UI.WebControls.GridView GridView1; //in designer
		protected global::System.Web.UI.WebControls.GridView GridView2;

		bool ActualsVisible(GridView gv)
		{
			int actualIndex;
			actualIndex = (int)RoomBlocks2.Grid.Columns.ActualBlock;

			return ((actualIndex + 1) <= gv.Columns.Count) && gv.Columns[actualIndex].Visible; //column may not exist, besides not being visible.
		}

		public class EventEntityDataBase
		{
			protected Opportunity eventEntity = null;

			protected bool ShowDebugColumns { get { return true; } }
			protected bool DisableSave2DB { get { return false; } }

			public virtual bool Bcmc_Actualized {get{
				return 
					true;
				//this.eventEntity.bcmc_ReportActualized.Value;
			}}

			public int StatusCode {get{
				return 
					//this.eventEntity.StatusCode.Value;
					(int)RoomBlocks2.Grid.EventStatusCodes.Actualized;
			}}
			public int DaysPriorEvent {get{
				return
					2;
					//this.eventEntity.bcmc_ActualizedDaysPriorEvent.Value; //ytodo check
			}}
			public int DaysPostEvent {get{
				return 2; 
					//this.eventEntity.bcmc_ActualizedDaysPostEvent.Value;
			}}
			/*
			 o.bcmc_ActualizedDaysPostEvent;
			 o.bcmc_ActualizedDaysPriorEvent;
			 o.bcmc_ReportActualized;
			 */

			// Normalize date - get rid of times
			public DateTime arrivalDate {get{
				return Convert.ToDateTime((this.eventEntity.New_arrivaldate ?? DateTime.MinValue).ToShortDateString());
			}}
			public DateTime departureDate {get{
				return Convert.ToDateTime((this.eventEntity.New_departuredate ?? DateTime.MinValue).ToShortDateString());
			}}
			public bool IsNewDateRange(DateTime dtArrive, DateTime dtDepart)
			{
				return (dtArrive != this.arrivalDate && dtDepart != this.departureDate);
			}

			public string StatusCodeStr{get{
				return this.eventEntity.StatusCode.Value.ToString();
			}}


			public static bool DebugShowSecondGrid = new EventEntityDataBase().ShowDebugColumns;
			public static bool DebugDisableSaveToDB = new EventEntityDataBase().DisableSave2DB && new EventEntityDataBase().ShowDebugColumns; //only allow when showing debug data on screen (safety)
		}

		public class EventActualRoomblocks : EventEntityDataBase
		{
			enum AddToGridPos
			{
				top, bottom,
			};

			Grid gridviewClass = null;

			public EventActualRoomblocks(Grid gridviewClass, bool delayedActualCalcuations = false)
			{
				this.gridviewClass = gridviewClass;

				FetchEventData();

				// We only want to all the pre/prior dates to the range
				// after the actual range has been loaded.
				// ytodo make this function call explicit i.e. remove from here.
				if (!delayedActualCalcuations)
					AddAdditionalActualRows();
			}

			public override bool Bcmc_Actualized{get{
				if (ShowDebugColumns)
				{
					if (this.gridviewClass.txtDbgActualized.Text == "1")
						return true;
					else if (this.gridviewClass.txtDbgActualized.Text == "0")
						return false;
				}

				return base.Bcmc_Actualized;
			}}

			public bool Debugging{get{
				return ShowDebugColumns || DebugShowSecondGrid;
			}}
	
			/* //ytodo not used cleanup in and aspx
			public bool DataFetched
			{
				get
				{
					return ((HiddenField)this.gridviewClass.FindControl(GridCtrlNames.HiddenField22.ToString())).Value == "ok";
				}

				set
				{
					((HiddenField)this.gridviewClass.FindControl(GridCtrlNames.HiddenField22.ToString())).Value = "ok";
				}
			}
			*/

			public void FetchEventData()
			{
				//if (!DataFetched)
				{
					BcmcLinqContext LinqProvider = new BcmcLinqContext(_service);
					var eventEntity = LinqProvider
						.OpportunitySet
						.Where(o => o.Id == new Guid(this.gridviewClass.EventId))
						//.Select(s => s.Id)
						//"bcmc_eventname", "statuscode"
						.ToList()
						;

					this.eventEntity = eventEntity[0]; //ytodo no good as we don't have the db data
					//DataFetched = true;
				}
			}

			public void AddAdditionalActualRows()
			{
				bool ReportActualizedRoomBlocksChecked = this.Bcmc_Actualized;
				int State = this.StatusCode;

				if (State == (int)RoomBlocks2.Grid.EventStatusCodes.Actualized
					&& ReportActualizedRoomBlocksChecked)
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

					// RemoveExtraDays
					// Remove days from top of grid
					if (RemoveDaysFromGrid(this.arrivalDate.AddDays(-this.DaysPriorEvent), gv, AddToGridPos.top))
						changesMade++;
					// Remove days from bottom of grid
					// Remove one day, as departure day is not included, as a booked day.
					if (RemoveDaysFromGrid(this.departureDate.AddDays(this.DaysPostEvent - 1), gv, AddToGridPos.bottom))
						changesMade++;

					FixDayNumber(gv);
					gv.DataBind();

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
			}

			static int callNumber = 0;
			void DisableCol(GridView gv, string coltitle, bool show)
			{
				this.gridviewClass.Page.ClientScript.RegisterStartupScript(this.GetType(),
					"dbg" + callNumber++.ToString(),
					String.Format(@" extData = {{}}; extData.show = {2};
						ActOnColTitled('{0}', '{1}', extData, function (col, extData, internalData) {{ 
table = document.getElementById('gvRoomBlock'); 
if (internalData.row == 0) {{ //show header text
	if (table !== undefined) 
		document.write('first col=' +table.rows[0].cells[0].innerHTML+'<br/>');
	document.write(internalData.rownum+' '+ internalData.colnum+' '+ col.innerHTML+'<br/>');
}}
							show = extData.show; 
    						col.disabled = !show;
							ipTxt = col.getElementsByTagName('Input')[0];
							if (undefined !== ipTxt && null !== ipTxt) {{
								ipTxt.disabled = !show;
								ipTxt.readOnly = !show;
							}}
						}});
						",
						 gv.ID, 
						 coltitle,//colnum,
						 show.ToString().ToLower(),
						 RoomBlocks2.Grid.GridCtrlNames.txtCBlock.ToString())
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

			void ShowColumn_Js(GridView gv, int colnum, bool show)
			{
				this.gridviewClass.Page.ClientScript.RegisterStartupScript(this.GetType(),
					"dbg" + callNumber++.ToString(),
					String.Format(@"
						ActOnCol('{0}', {1}, function (col, show) {{
							col.disabled = !{2};
							col.style.visibility = 'visible';
							if (show)
								col.className = col.className.replace( /(?:^|\s)hidden_boundfld(?!\S)/ , '' ); //remove
							else
								col.className += ' hidden_boundfld';
						}}, {2});
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

					ShowColumn(this.gridviewClass.GridView1, (int)RoomBlocks2.Grid.Columns.ActualBlock, show);
					ShowColumn(this.gridviewClass.GridView1, (int)RoomBlocks2.Grid.Columns.ActualPercentOfPeak, show);

					if (show)
					{
						DisableCol(gv,
							RoomBlocks2.Grid.Heading(RoomBlocks2.Grid.Columns.CurrentBlock),
							//GetColNumfromTitle(gv, this.gridviewClass.GridColumnTitles[(int)GridColumnsRoomBlock.CurrentBlock]),
							//(int)GridColumnsFormLoad.CurrentBlock, 
							false);
					}

					ShowColumn(this.gridviewClass.GridView1, (int)RoomBlocks2.Grid.Columns.DayNumber, ShowDebugColumns);
					ShowColumn(this.gridviewClass.GridView1, (int)RoomBlocks2.Grid.Columns.RoomBlockId, ShowDebugColumns);
				}

				if (this.gridviewClass.GridView2.Columns.Count > 0)
				{
					gv = this.gridviewClass.GridView2;

					ShowColumn(this.gridviewClass.GridView2, (int)RoomBlocks2.Grid.Columns.ActualBlock, show);
					ShowColumn(this.gridviewClass.GridView2, (int)RoomBlocks2.Grid.Columns.ActualPercentOfPeak, show);

					if (show)
					{
						DisableCol(gv,
							RoomBlocks2.Grid.Heading(RoomBlocks2.Grid.Columns.CurrentBlock),
							//5, //GetColNumfromTitle(gv, this.gridviewClass.GridColumnTitles[(int)GridColumnsRoomBlock.CurrentBlock]),
							//(int)GridColumnsRoomBlock.CurrentBlock, 
							false);
					}

					ShowColumn(this.gridviewClass.GridView2, (int)RoomBlocks2.Grid.Columns.DayNumber, ShowDebugColumns);
					ShowColumn(this.gridviewClass.GridView2, (int)RoomBlocks2.Grid.Columns.RoomBlockId, ShowDebugColumns);
					//ShowColumn(this.gridviewClass.gvRoomBlock, (int)GridColumnsRoomBlock.EventId, ShowDebugColumns);  //ytodo cleanup
				}

				//if (ShowDebugColumns)
				{
				}

				DebugView();
			}

			// Some debug stuff is in ShowActuals
			void DebugView()
			{
				this.gridviewClass.txtDbgArrivalDate.Visible = ShowDebugColumns;
				this.gridviewClass.txtDbgDepartureDate.Visible = ShowDebugColumns;
				this.gridviewClass.txtDbgActualized.Visible = ShowDebugColumns;
				this.gridviewClass.btnDbgUpdateDateRange.Visible = ShowDebugColumns;

				if (ShowDebugColumns)
				{
					this.gridviewClass.txtDbgArrivalDate.Text = this.arrivalDate.ToShortDateString();
					this.gridviewClass.txtDbgDepartureDate.Text = this.departureDate.ToShortDateString();
				}

				this.gridviewClass.flTitle.Visible = ShowDebugColumns;
				this.gridviewClass.rbTitle.Visible = ShowDebugColumns;
				this.gridviewClass.flTitle.Style["display"] = ShowDebugColumns ? "block;":"none;";
				this.gridviewClass.rbTitle.Style["display"] = ShowDebugColumns ? "block;":"none;";
				this.gridviewClass.CheckBox_TogglePreAndPostWeek.Style["visibility"] = "none";
				this.gridviewClass.CheckBox_TogglePreAndPostWeek.Style["display"] = "none";

				if (ShowDebugColumns)
				{
					this.gridviewClass.DbgInfo.Visible = true;
					this.gridviewClass.DbgInfo.Style["display"] = "block";

					this.gridviewClass.DbgInfo.InnerHtml = String.Format(
						"<div>arrival/departure: {0} {1}. </div>"+
						"<div>add top/below: {2} {3}.</div>",
						this.eventEntity.New_arrivaldate.Value.ToShortDateString(),
						this.eventEntity.New_departuredate.Value.ToShortDateString(),
						this.DaysPriorEvent,
						this.DaysPostEvent
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
				ShowColumn_Js(this.gridviewClass.GridView2, (int)RoomBlocks2.Grid.Columns.RoomBlockId, ShowDebugColumns);
				ShowColumn_Js(this.gridviewClass.GridView1, (int)RoomBlocks2.Grid.Columns.RoomBlockId, ShowDebugColumns);

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
							GridColumnsFormLoad.RoomBlockId.ToString(),
							GridColumnsRoomBlock.RoomBlockId.ToString())
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

			GridView GetGridView{ get {
				GridView gv;
				int fl = this.gridviewClass.GridView1.Rows.Count;
				int rb = this.gridviewClass.GridView2.Rows.Count;
				if ((fl == rb && fl == 0)
					||  rb > fl)
					gv = this.gridviewClass.GridView2;
				else
					gv = this.gridviewClass.GridView1;

				return gv;
			}}

			bool DayAlreadyInGrid(DateTime day)
			{
				string _day = day.ToShortDateString();
				for (int rownum = 0; rownum < this.GetGridView.Rows.Count; rownum++)
				{
					//*[@id="gvRoomBlock"]/tbody/tr[8]/td[2]
					// Extract 'Date' Column
					if (_day == this.GetGridView.Rows[rownum].Cells[(int)RoomBlocks2.Grid.Columns.Date].Text)
						return true;
				}
				return false;
			}

			int GetInsertPositionInGrid(DateTime day, AddToGridPos addToGridPos)
			{
				for (int rownum = 0; rownum < this.GetGridView.Rows.Count; rownum++)
				{
					DateTime GridDay = Convert.ToDateTime(this.GetGridView.Rows[rownum].Cells[(int)RoomBlocks2.Grid.Columns.Date].Text);
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
					dr["DayNumber"] = 0; // this will be corrected by the fix function.
					dr["DayofWeek"] = dateToBeAdded.DayOfWeek.ToString();
					
					dr["Date"] = dateToBeAdded.ToShortDateString();
					//if (dr.ItemArray.Contains("GUIDID"))
					//	dr["GUIDID"] = this.eventEntity.Id.ToString(); //ytodo cleanup
					dr["RoomGUID"] = "";
					int insertPos = GetInsertPositionInGrid(dateToBeAdded, addToGridPos);
					if (-1 == insertPos)
						dt.Rows.Add(dr);
					else
						dt.Rows.InsertAt(dr, insertPos);
					/* ytodo remove
					if (AddToGridPos.bottom == addToGridPos)
						dt.Rows.Add(dr);
					else
						dt.Rows.InsertAt(dr, insertPos);
					*/

					gv.DataSource = dt;

					return true;
				}

				return false;
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
					DateTime latestDate = Convert.ToDateTime(dt.Rows[dt.Rows.Count-1]["Date"]);
					TimeSpan period = date - latestDate;
					if (period.TotalDays > 0)
					{
						for (int i = 0; i < period.TotalDays; i++)
							dt.Rows.RemoveAt(dt.Rows.Count - 1);
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

						logger.Error(String.Format("RemoveDaysFromGrid date={0}: {1}", dt.Rows[0]["Date"], ex.Message));

						return true;
					}

					TimeSpan period = date - earliestDate;
					if (period.TotalDays > 0)
					{
						for (int i = 0; i < period.TotalDays; i++)
							dt.Rows.RemoveAt(0);
						return true;
					}
				}

				return false;
			}

			void FixDayNumber(GridView gv)
			{
				int rownum = 1;

				DataTable dt = gv.DataSource as DataTable;
				foreach(DataRow row in dt.Rows)
					row["DayNumber"] = (rownum++).ToString();

				gv.DataSource = dt;
			}

			void RecalculateTotalNightsAndPeak()
			{
				this.gridviewClass.UpdateEvent_withCalc_TotalRoomblocksAndPeak(
					this.GetGridView,
					this.gridviewClass.EventId);

				//ResetPeakRoomNightsIfNoRoomblocks
			}

		}//EventActualRoomBlocks class

		//ytodo js, btnSave/Update

	}
}