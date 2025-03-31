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



namespace RoomBlocks2
{
	public partial class Grid : System.Web.UI.Page
	{
		private static bool bSHOW_ROOM_GUID = false; //always false, for debugging presumably

		public static ILog logger = LogManager.GetLogger(typeof(Grid));
		public static OrganizationService _service;

		protected global::System.Web.UI.HtmlControls.HtmlGenericControl rbTitle;
		protected global::System.Web.UI.HtmlControls.HtmlGenericControl flTitle;
		public global::System.Web.UI.WebControls.TextBox txtDbgActualized;
		//protected global::System.Web.UI.HtmlControls.HtmlGenericControl divTotalsLine;
		public global::System.Web.UI.HtmlControls.HtmlGenericControl ActualEntryComplete;
		public global::System.Web.UI.HtmlControls.HtmlSelect ActualEntryCompleted;


		string lastSuccessLogMsg = "";


		#region  Page Load Functions

		bool NoStatus_NothingToDoYet{get{
			return (HttpContext.Current.Request.QueryString["status"] == null);
		}}
		public bool HasEventId{get{
			return (HttpContext.Current.Request.QueryString["recordid"] != null);
		}}
		bool SetRoomPatternStatusToActiveAndUser{get{
			return (HttpContext.Current.Request.QueryString["rpStatus"] != null);
		}}

		#endregion


		private string EventStatusCode
		{
			get
			{
				return (this.ViewState["getEventStatus"] != null
					? (string)this.ViewState["getEventStatus"]
					: string.Empty);
			}
			set
			{
				this.ViewState["getEventStatus"] = value;
			}
		}

		public string EventId
		{
			get
			{
				return ViewState["ThisEventId"].ToString();
			}
			set { ViewState["ThisEventId"] = value; }
		}

		public string GetUrlEventId
		{
			get
			{
				return Request.QueryString["recordid"].ToString().Replace("{", "").Replace("}", "");
			}
		}

		public string GetUrlEventIdWithClosures
		{
			get
			{
				return Request.QueryString["recordid"].ToString();
			}
		}

		public bool InvalidEventId
		{
			get
			{
				return (HttpContext.Current.Request.QueryString["recordid"] == null);
			}
		}

		public bool EventStatusIsActualizedOrDefinite
		{
			get { return EventStatusCode == ((int)RoomBlocks2.Grid.EventStatusCodes.Definite).ToString(); }
		}

		private string Username
		{
			get { return (string)this.ViewState["username"]; }
			set { this.ViewState["username"] = value; }
		}

		void InitState()
		{
			log4net.Config.XmlConfigurator.Configure();
			_service = CrmServiceManager.GetCrmService();

			this.Username = Request.QueryString["username"].ToString();
		}

		private EventActualRoomblocks pageload_EventData = null;
		protected void PageLoad()
		{
#if true
			form1.Visible = false;
			loading.InnerHtml = "Loading...";
			loading.Visible = true;

			//ytodo remove string eventid = string.Empty;
			EventActualRoomblocks ActualRoomBlockLogicAndEventData = null;

			try
			{
				InitState();

				//logger.Info("Page_Load");
				logger.Info(String.Format("Page_Load evntid={0}", GetUrlEventId));

				//ytodo for a single load or functionCall the 
				//actual EventQuery should be performed only once!!

				ActualRoomBlockLogicAndEventData = new EventActualRoomblocks(this, GetUrlEventId, true);
				int dbglevel = 0;
				logger.Info(String.Format("Dbg:Page_Load {0}", ++dbglevel));
				pageload_EventData = ActualRoomBlockLogicAndEventData;

				logger.Info(String.Format("Dbg:Page_Load {0}", ++dbglevel));
				if (NoStatus_NothingToDoYet)
				{
#if true
					if (HasEventId)
					{
						bool setRoomPatternStatusToActiveAndUser = SetRoomPatternStatusToActiveAndUser;

						if (setRoomPatternStatusToActiveAndUser)
							UpdateStatusActiveAndUpdateUserForAllRoomPatternDays(ActualRoomBlockLogicAndEventData);

						if (EventStatusCode != string.Empty
							&& EventStatusIsActualizedOrDefinite)
						{
							logger.Info(String.Format("Dbg:Page_Load actual/definite {0}", ++dbglevel));
							ActualRoomBlockLogicAndEventData.LoadRoomPatternGrid();
							ActualRoomBlockLogicAndEventData.UpdateMode();
							logger.Info(String.Format("Dbg:Page_Load actual/definite {0}", ++dbglevel));
						}
						else
						{
							ActualRoomBlockLogicAndEventData.LoadRoomPatternGrid(RoomBlocks2.Grid.AllowRoomBlocksDelete.False);
							logger.Info(String.Format("Dbg:Page_Load ldGrid {0}", ++dbglevel));
						}
					}
					else
					{
						// No eventid provided.
						ButtonSave.Visible = false;
						ButtonMoveToOriginal.Visible = false;
					}
#endif
				}
				else
				{
					// status urlParm provided
					// signal delete old records (on condition)
					ActualRoomBlockLogicAndEventData.LoadRoomPatternGrid(AllowRoomBlocksDelete.True); //the only place deletion is allowed
					ButtonSave.Text = "Save";
					ButtonMoveToOriginal.Enabled = false;
				}

				ActualRoomBlockLogicAndEventData.AddAdditionalActualRows();

				form1.Visible = true;
				loading.Visible = false;
			}
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
				loading.InnerHtml = "Connection error!<br/>Failed to load Roomblock Grid from CRM Database. <br/><br/>Attempt Refresh F5 or consult Administrator.<br/><br/>" + ex.Message;
			}
			catch (Exception ex)
			{
				logger.Error("Page_Load", ex);
				loading.InnerHtml = "Connection error!<br/>Failed to load Roomblock Grid from CRM Database. <br/><br/>Attempt Refresh F5 or consult Administrator.<br/><br/>" + ex.Message;
			}
            finally //ytodo why is this in finally!!?
			{
				if (form1.Visible)
				{
					string _eventID = GetUrlEventId;
					if (_eventID != null && _eventID != string.Empty)
					{
						if (null == ActualRoomBlockLogicAndEventData)
						{
							ButtonSave.Enabled = false;
							ButtonMoveToOriginal.Enabled = false;

							//throw new Exception("Connection to database failed!");
						}
						else
						{
							EntityCollection entityCollection = ActualRoomBlockLogicAndEventData.roomPattern.
								RecordsOrderedByDate();

							// Roompatterns records and HAVE Dates
							if (entityCollection.Entities.Count > 0 && ViewState["DisplayDateisNULL"] == null)
							{
								ButtonSave.Text = "Update";
								ButtonMoveToOriginal.Enabled = true;
							}
							// Roompatterns records and NULL Dates
							else if (ViewState["DisplayDateisNULL"] != null)
							{
								ButtonSave.Enabled = false;
								ButtonMoveToOriginal.Enabled = false;
							}
							// NO Roompatterns records
							else
							{
								ButtonSave.Text = "Save";
								ButtonMoveToOriginal.Enabled = false;
								if (entityCollection.Entities.Count == 0)
									ActualRoomBlockLogicAndEventData.UpdateEventRoomNights(ActualRoomBlockLogicAndEventData.EventId, 0, 0);
							}
						}
					}

					if (!NoStatus_NothingToDoYet)
					{
						if (null == ActualRoomBlockLogicAndEventData)
							ActualRoomBlockLogicAndEventData = new EventActualRoomblocks(this, EventId, true);

						if (ActualRoomBlockLogicAndEventData.Bcmc_Actualized) //does this make sense for no status..?
							ButtonMoveToOriginal.Enabled = false;
					}
				}
			}
#endif
		}

		private void SaveUpdateGrid()
		{
			EventActualRoomblocks eventWrapper = new EventActualRoomblocks(this, this.EventId, true);
			string javascript = UpdateTheOneAndOnly(eventWrapper);
			eventWrapper.AddAdditionalActualRows();
			if ("" != javascript)
				saveSuccessfull = true;
				Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript",
					javascript,
					true);


			//if (ButtonSave.Text == "Save")
			//{
			//	//SaveGrid(eventWrapper.EventId);
			//	UpdateTheOneAndOnly(eventWrapper); //, EventStatusCodes.NotDefinite, AllowRoomBlocksDelete.False); //NO______ NUll handling
			//}
			//else if (ButtonSave.Text == "Update")
			//	UpdateTheOneAndOnly(eventWrapper);
		}

		private string UpdateTheOneAndOnly(EventActualRoomblocks eventWrapper)
		{
			//EventStatusCode == "10" => NULL Handling //ytodo needs a little investigation
			string javascript = UpdateRoomblockRecords_fromGrid(eventWrapper); //, EventStatusCodes.NotDefinite, AllowRoomBlocksDelete.False);
			if ("" != javascript)
			{
				this.saveSuccessfull = true;
			}
			return javascript;

			/*if (EventStatusCode != "10")
				UpdateRoomblockRecords_fromGrid(eventWrapper, GridView1, EventStatusCodes.NotDefinite, AllowRoomBlocksDelete.False);
			else
				UpdateRoomblockDefiniteStatus(eventWrapper);
			*/
		}

		private void BackupRoomblocksAsOriginals()
		{
			try
			{
				EventActualRoomblocks eventWrapper = new EventActualRoomblocks(this, this.EventId, true);

				// Copy from CurrentBlock,PercentPeak to Originals for all records
				// 
				GridViewData g = new GridViewData(eventWrapper.GV, eventWrapper.PersistantData);
				foreach (GridViewRow Row in GridView1.Rows)
				{
					g.OriginalPercentPeakValue = g.PercentofPeak(g.rowCount);
					g.OriginalRoomBlockValue = g.CurrentBlock(g.rowCount);

					/*TextBox txtcurrent = (TextBox)Row.FindControl("txtCBlock");
					string currentroom = txtcurrent.Text;

					System.Web.UI.WebControls.Label txtoriginal = (System.Web.UI.WebControls.Label)Row.FindControl("lblOblock");
					txtoriginal.Text = currentroom;

					HiddenField txt1 = (HiddenField)Row.FindControl("hdnfdValue");
					string current = txt1.Value;
					System.Web.UI.WebControls.Label originalPeakCtrl = (System.Web.UI.WebControls.Label)Row.FindControl("lblOPeak");
					originalPeakCtrl.Text = current;*/

					g.rowCount++;
				}

				string javascript = UpdateTheOneAndOnly(eventWrapper);
				eventWrapper.AddAdditionalActualRows();
				if ("" != javascript)
					saveSuccessfull = true;
					Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript",
						javascript,
						true);
			}
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
			catch (Exception ex)
			{
				logger.Info(String.Format("{0}", lastSuccessLogMsg));
				logger.Error("", ex);
			}
		}

		public class GridViewData
		{
			DataTable dt;
			GridView gv;
			public int rowCount = 0;
			public bool fromDataSource = false; //loading to OR saving from grid

			public GridViewData(GridView gv, object PersistantData = null, bool LoadingDataFromDataSource = false)
			{
				this.dt = (DataTable)PersistantData;
				this.gv = gv;
				rowCount = 0;
				this.fromDataSource = LoadingDataFromDataSource;
			}

			public int Count { get {
				if (this.fromDataSource)
					return ((DataTable)gv.DataSource).Rows.Count;
				else
					return gv.Rows.Count;
			}}

			//We alway want to get this value, where Visible or not Disabled or not!!!!
			public string RoomGuid(int row)
			{
				//BoundField
				// or via GridView1.DataKeys[rowIndex].Values[0].ToString();

				string value;
				/*if (!gv.Rows[row].Cells[(int)Columns.RoomBlockId].Visible)
				{
					if (null == dt)
						throw new Exception("Must provide PersistantData");


					value = this.dt.Rows[row].Field<string>(Heading(Columns.RoomBlockId));
				}
				else*/
				{
					//((DataTable)gv.DataSource).Rows[row].Field<string>(Heading(Columns.RoomBlockId)));
					value = gv.Rows[row].Cells[(int)Columns.RoomBlockId].Text; //cannot use this, as Visible taking away cols breaks it.
				}
				if (null == value || "" == value)
				{
					Dictionary<string, string> myTable = (Dictionary<string, string>)RoomBlocks2.Grid.ThisIsMe.ViewState["tableData"];
					if (null != myTable)
						value = myTable[String.Format("{0}{1}", "RoomGuid", row)];

					/*if (null != RoomBlocks2.Grid.dtPersistant)
					{
						value = RoomBlocks2.Grid.dtPersistant.Rows[row].Field<string>(Heading(Columns.RoomBlockId));
					}*/
				}
				return FilterSpaces(value);
			}

			public int? OriginalPercentPeak(int row)
			{
				return ColInt(row, Columns.OriginalPercentOfPeak);
			}
			public int? OriginalPercentPeakValue { set {
				SetCol(this.rowCount, Columns.OriginalPercentOfPeak, value);
			}}

			public int? OriginalRoomBlock(int row)
			{
				return ColInt(row, Columns.OriginalBlock);
			}
			public int? OriginalRoomBlockValue { set {
				SetCol(this.rowCount, Columns.OriginalBlock, value);
			}}

			public int? PercentofPeak(int row)
			{
				return ColInt(row, Columns.CurrentPercentOfPeak);
			}
			public int? PercentofPeakValue { set {
				SetCol(this.rowCount, Columns.CurrentPercentOfPeak, value);
			}}

			public int? CurrentBlock(int row)
			{
				return ColInt(row, Columns.CurrentBlock);
			}

			public int? ActualPercentOfPeak(int row)
			{
				return ColInt(row, Columns.ActualPercentOfPeak);
			}

			public int? ActualBlock(int row)
			{
				return ColInt(row, Columns.ActualBlock);
			}

			public string ColValue(int row, Columns col)
			{
				/*int colIndex = (int)col;
				return ColValue(row, colIndex);
			}

			public string ColValue(int row, int col)
			{*/
				string outValue;
				if (this.fromDataSource) //i.e. database data
				{
					DataTable dt = ((DataTable)gv.DataSource);
					outValue = dt.Rows[row].Field<string>(Heading(col));
				}
				else
				{
					string dbug = String.Format("{0}({1}): ", col.ToString(), row);
					outValue = null;
					string value = "";
					try
					{
						value = gv.Rows[row].Cells[(int)col].Text; //ytodofix not reliable
						if (null != value && "" != value && null == outValue)
						{
							outValue = value;
						}

						foreach (var c in gv.Rows[row].Cells[(int)col].Controls) //ytodofix not reliable
						{
							bool hiddenFieldOverride = false;
							value = GetCtlTxtVal(c, ref hiddenFieldOverride);
							if (null != value && "" != value
								&& (hiddenFieldOverride || null == outValue))
								outValue = value;

							dbug += String.Format("{0}={1}; ", (c as Control).ID, value);
						}
						//logger.InfoFormat("Extract: {0} {1}", value, dbug);
					}
					catch (Exception ex)
					{
						logger.ErrorFormat("Extract:: {0} {1}\n{2}", value, dbug, ex);
					}
				}

				//return null == outValue? "": outValue;
				return outValue;
			}

			public int? ColInt(int row, Columns col)
			{
				int? intVal = null;
				string value = ColValue(row, col);
				if (null == value)
					return intVal;
				else
				{
					intVal = ToInt(value);
					return intVal;
				}
			}

			private string GetCtlTxtVal(object c, ref bool hiddenFieldOverride)
			{
				string value = "";

				object unknownCtl = c;
				if (null != unknownCtl)
				{
					if (unknownCtl is System.Web.UI.ITextControl)
						value = (unknownCtl as System.Web.UI.ITextControl).Text;
					else if (unknownCtl is System.Web.UI.WebControls.Label)
						value = (unknownCtl as System.Web.UI.WebControls.Label).Text;
					else if (unknownCtl is System.Web.UI.HtmlControls.HtmlInputHidden)
					{
						hiddenFieldOverride = true;
						value = (unknownCtl as System.Web.UI.HtmlControls.HtmlInputHidden).Value;
					}
					else if (unknownCtl is System.Web.UI.HtmlControls.HtmlGenericControl)
						value = (unknownCtl as System.Web.UI.HtmlControls.HtmlGenericControl).InnerText;
					else if (unknownCtl is System.Web.UI.HtmlControls.HtmlContainerControl)
						value = (unknownCtl as System.Web.UI.HtmlControls.HtmlContainerControl).InnerText;
					else if (unknownCtl is System.Web.UI.IAttributeAccessor)
						value = (unknownCtl as System.Web.UI.IAttributeAccessor).GetAttribute("value");
				}

				// remove white space chars
				return FilterSpaces(value);
			}

			public static string FilterSpaces(string value)
			{
				if (null == value)
					return "";

				System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("\\s|&nbsp;");
				value = regex.Replace(value, "");

				return value;
			}

			void SetCol(int row, Columns col, int? value)
			{
				if (value.HasValue && null != value)
				{
					SetCol(row, col, Convert.ToString(value.Value));
				}
			}

			void SetCol(int row, Columns col, string value)
			{
				foreach (var c in gv.Rows[row].Cells[(int)col].Controls)
				{
					object unknownCtl = c;
					if (null != unknownCtl)
					{
						if (unknownCtl is System.Web.UI.ITextControl)
							(unknownCtl as System.Web.UI.ITextControl).Text = value;
						else if (unknownCtl is System.Web.UI.WebControls.Label)
							(unknownCtl as System.Web.UI.WebControls.Label).Text = value;
						else if (unknownCtl is System.Web.UI.HtmlControls.HtmlGenericControl)
							(unknownCtl as System.Web.UI.HtmlControls.HtmlGenericControl).InnerText = value;
						else if (unknownCtl is System.Web.UI.HtmlControls.HtmlContainerControl)
							(unknownCtl as System.Web.UI.HtmlControls.HtmlContainerControl).InnerText = value;
						else if (unknownCtl is System.Web.UI.IAttributeAccessor)
							(unknownCtl as System.Web.UI.IAttributeAccessor).SetAttribute("value", value);
					}
				}
			}

			int ToInt(string value, int default_ = 0)
			{
				if (value != "")
				{
					int intValue = 0;
					if (value.Contains("."))
						intValue = Convert.ToInt32(value.Split('.')[0].ToString());
					else
						intValue = Convert.ToInt32(value);

					return intValue;
				}
				else
					return default_;
			}
		}



		class UpdateOptions
		{
			public bool UpdateDayNumber = true;
			public bool NewRecord = true;
			public bool ActualsVisible = false;
			public bool ActualHasAValue = false;
			//public bool RowDisabled = false;
			public EventActualRoomblocks eventWrapper = null;
		}
		private UpdateOptions defaultUpdateOptions = new UpdateOptions();

		public void SaveActualsInPostSession(bool remove, bool top)
		{
			string value;

			Dictionary<string, string> myTable = (Dictionary<string, string>)RoomBlocks2.Grid.ThisIsMe.
				ViewState["tableData"];

			GridViewData g = new GridViewData(this.GridView1);
			int adjustedRow = 0;
			for (int row = 0; row < this.GridView1.Rows.Count 
				/*&& adjustedRow < this.GridView1.Rows.Count*/; //if chopped at the bottom it will be left out.
				row++)
			{
				adjustedRow = row;
				if (remove && top)
				{
					if (row == 0)
					{
						myTable[String.Format("{0}{1}", RoomBlocks2.Grid.Heading(Columns.ActualPercentOfPeak), row)] = "";
						myTable[String.Format("{0}{1}", RoomBlocks2.Grid.Heading(Columns.ActualBlock), row)] = "";
						continue; //lop off the top most, and zero prev. index=0
					}
					adjustedRow = row - 1;
				}
				else if (remove && !top)
				{
					adjustedRow = row;
				}
				else if (!remove && top)
				{
					if (row == 0)
					{//make sure the new rec. at idx is blank
						// and then go on to save record zero now at idx 1
						myTable[String.Format("{0}{1}", RoomBlocks2.Grid.Heading(Columns.ActualPercentOfPeak), row)] = "";
						myTable[String.Format("{0}{1}", RoomBlocks2.Grid.Heading(Columns.ActualBlock), row)] = "";
					}
					adjustedRow = row + 1;
				}
				else if (!remove && !top)
				{
					adjustedRow = row;//nothing is changed
					if (row == this.GridView1.Rows.Count-1)
					{//make sure the new rec at the bottom is blank.
						myTable[String.Format("{0}{1}", RoomBlocks2.Grid.Heading(Columns.ActualPercentOfPeak), row+1)] = "";
						myTable[String.Format("{0}{1}", RoomBlocks2.Grid.Heading(Columns.ActualBlock), row+1)] = "";
					}
				}

				value = "";
				int? intValue = g.ActualPercentOfPeak(row);
				if (intValue.HasValue && intValue != null)
					value = intValue.ToString();
				myTable[String.Format("{0}{1}", RoomBlocks2.Grid.Heading(Columns.ActualPercentOfPeak), adjustedRow)] = value;

				intValue = g.ActualBlock(row);
				if (intValue.HasValue && intValue != null)
					value = intValue.ToString();
				myTable[String.Format("{0}{1}", RoomBlocks2.Grid.Heading(Columns.ActualBlock), adjustedRow)] = value;
			}

			RoomBlocks2.Grid.ThisIsMe.ViewState["tableData"] = myTable;
		}

		private bool UpdateSaveRowsToRoomPattern(GridView gv, UpdateOptions updateOptions)
		{
			GridViewData g = new GridViewData(gv, updateOptions.eventWrapper.PersistantData);

			if (null == updateOptions)
				updateOptions = new UpdateOptions(); //default

			updateOptions.ActualsVisible = ActualsVisible(gv);

			int changes = 0;

			//Calculate which ActualBlocks rows may need to be saved.
			int firstActualRowToBeSaved, lastActualRowToBeSaved;
			new EventActualRoomblocks.ActualRowsToBeSaved(updateOptions.eventWrapper)
				.FirstLastRows(out firstActualRowToBeSaved, out lastActualRowToBeSaved);

			int dayNumber = 1; // One based

			for (int row = 0; row < gv.Rows.Count; row++)
			{
				string RoomPatternGuid = null;
				//if (dtretrieve.Rows.Count >= row + 1)
				//	RoomPatternGuid = dtretrieve.Rows[row]["RoomGUID"].ToString();
				
				RoomPatternGuid = g.RoomGuid(row);
				updateOptions.NewRecord = (null == RoomPatternGuid || "" == RoomPatternGuid);

				// Allow for removal of Actual dates beyond Duration that have become
				// irrelevant i.e. cleared out.
				DateTime day = Convert.ToDateTime(gv.Rows[row].Cells[(int)Columns.Date].Text);
				if (updateOptions.eventWrapper.Bcmc_Actualized
					//&& !updateOptions.eventWrapper.WithinPlannedDuration(day) this can happen for definite status
					&& (row < firstActualRowToBeSaved || row > lastActualRowToBeSaved)
					)
				{
					// If we have a record here in the database
					// The change in prior/post values has indicated to delete this.
					if (!updateOptions.NewRecord)
					{
						logger.Info(String.Format("Deleting {0} first:{1} last{2} (UpdateSaveRowsToRoomPattern)", 
										row, firstActualRowToBeSaved, lastActualRowToBeSaved));
						_service.Delete(New_roompattern.EntityLogicalName,
							new Guid(RoomPatternGuid));
					}
					continue; //do not save this row, and del if exists.
								// Only for Actualized and beyond extreme Actual date values
				}

				New_roompattern roompattern = new New_roompattern();
				if (!updateOptions.NewRecord)
					roompattern.New_roompatternId = new Guid(RoomPatternGuid);

				roompattern.bcmc_User = this.Username;

				roompattern.Bcmc_OriginalpercentofPeak = g.OriginalPercentPeak(row);
				roompattern.Bcmc_OriginalRoomBlock = g.OriginalRoomBlock(row);

				// Disabled Records Current_RoomBlock is set to zero. 
				// i.e. Definite status with a changed Duration
				roompattern.New_PercentofPeak = !gv.Rows[row].Enabled ? 0 : g.PercentofPeak(row);
				roompattern.New_RoomBlock = !gv.Rows[row].Enabled ? 0 : g.CurrentBlock(row);

				if (updateOptions.ActualsVisible)
				{
					roompattern.Bcmc_ActualPercentOfPeak = g.ActualPercentOfPeak(row);
					roompattern.Bcmc_ActualBlock = g.ActualBlock(row);

					if (roompattern.Bcmc_ActualBlock.HasValue
						&& null != roompattern.Bcmc_ActualBlock)
					{
						updateOptions.ActualHasAValue = true;
					}

				}

				roompattern.New_DayNumber = dayNumber++;

				if (updateOptions.NewRecord)
				{
					//roompattern.New_DayNumber = row + 1;		//Convert.ToInt32(gv.Rows[row].Cells[(int)GridColumnsFormLoad.DayNumber].Text);
					roompattern.Bcmc_Date = day;
					roompattern.New_name = roompattern.Bcmc_Date.Value.DayOfWeek.ToString();		//gv.Rows[row].Cells[(int)GridColumnsFormLoad.DayOfWeek].Text;
					Guid guid = Create(roompattern); //CreateRoomPatternRec(roompattern); //Save

					//cannot be relied upon to breaking for not visible cols
					// simply do a reload
					//gv.Rows[row].Cells[(int)Columns.RoomBlockId].Text = guid.ToString(); 

					if (Guid.Empty != guid)
						changes++;
				}
				else
				{
					Guid guid = Update(roompattern); //CreateRoomPatternRec(roompattern); //Save

					//cannot be relied upon to breaking for not visible cols
					// simply do a reload
					//gv.Rows[row].Cells[(int)Columns.RoomBlockId].Text = guid.ToString();  //its possible that create occured in update? or once upon a time it was...

					if (Guid.Empty != guid)
						changes++;
				}
			}
			return changes > 0;
		}

		private string UpdateRoomblockRecords_fromGrid(EventActualRoomblocks eventWrapper/*, EventStatusCodes eventStatusCode = EventStatusCodes.NotDefinite*/)
		{
			try
			{
				GridView gv = eventWrapper.GV;

				UpdateOptions updateOptions = new UpdateOptions();
				updateOptions.eventWrapper = eventWrapper;

				if (gv.Rows.Count > 0)
				{
					UpdateSaveRowsToRoomPattern(gv, updateOptions);

					// Update an attribute retrieved via RetrieveAttributeRequest
					String jsCode = eventWrapper.RecalculateTotalNightsAndPeak();

					//Reload Grid After Save - capture any db triggered updates
					eventWrapper.LoadRoomPatternGrid(AllowRoomBlocksDelete.False); //EventStatusCodes.Definite != eventStatusCode,

					eventWrapper.UpdateMode();

					// Updatestatus is when btnSave.Text == "Update"
					/*if ((1 == Updating && "gvFormLoad" == gv.ID)
						|| "gvRoomBlock" == gv.ID
						|| ((int)EventStatusCodes.Definite).ToString() == eventStatusCode)*/
					{
						/*Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript",
							jsCode + "alert('Successfully updated');",
							true);*/

						return jsCode + "notifyUpdateSuccessfull = true;";//"alert('Successfully updated');";
					}
				}
			}
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
			catch (Exception ex)
			{
				logger.Info(String.Format("LastSuccessLog, UpdateRoomblock_fromSpecificGrid(: {0}", lastSuccessLogMsg));
				logger.Error("1251 UpdateRoomblock_fromSpecificGrid " + ex);
			}

			return "";
		}

		int ExtractGridRowHidden_Int(GridView gv, int row, string ctlName, int defaultInt = 0)
		{
			HiddenField ctl = (HiddenField)gv.Rows[row].FindControl(ctlName);
			string value = ctl.Value;
			int valueInt = defaultInt;
			if (null != ctl)
			{
				if ("" != ctl.Value)
				{
					if (value.Contains("."))
						valueInt = Convert.ToInt32(value.Split('.')[0].ToString());
					else
						valueInt = Convert.ToInt32(value);
				}
			}
			return valueInt;
		}
		int ExtractGridRowCtrl_Int(GridView gv, int row, string ctlName, string msg = "", int default_ = 0)
		{
			string value = ExtractGridRowCtrl_Text(gv, row, ctlName, msg);
			if (value != "")
			{
				int intValue = 0;
				if (value.Contains("."))
					intValue = Convert.ToInt32(value.Split('.')[0].ToString());
				else
					intValue = Convert.ToInt32(value);

				return intValue;
			}
			else
				return default_;
		}

		string ExtractGridRowText_Textbox(GridView gv, int row, string ctlName, string msg = "", string default_ = "")
		{
			TextBox ctl = (TextBox)gv.Rows[row].FindControl(ctlName);
			string value = default_;
			if (null != ctl)
			{
				value = ctl.Text;
			}
			lastSuccessLogMsg = String.Format(msg + "roompattern.{0}={1}", ctlName, value);
			return value;
		}

		string ExtractGridRowCtrl_Text(GridView gv, int row, string ctlName, string msg = "", string default_ = "")
		{
			string value = default_;

			object unknownCtl = gv.Rows[row].FindControl(ctlName);
			if (null != unknownCtl)
			{
				if (unknownCtl is System.Web.UI.ITextControl)
					value = (unknownCtl as System.Web.UI.ITextControl).Text;
				else if (unknownCtl is System.Web.UI.WebControls.Label)
					value = (unknownCtl as System.Web.UI.WebControls.Label).Text;
				else if (unknownCtl is System.Web.UI.HtmlControls.HtmlGenericControl)
					value = (unknownCtl as System.Web.UI.HtmlControls.HtmlGenericControl).InnerText;
				else if (unknownCtl is System.Web.UI.HtmlControls.HtmlContainerControl)
					value = (unknownCtl as System.Web.UI.HtmlControls.HtmlContainerControl).InnerText;
				else if (unknownCtl is System.Web.UI.IAttributeAccessor)
					value = (unknownCtl as System.Web.UI.IAttributeAccessor).GetAttribute("value");
			}
			/*
			System.Web.UI.IAttributeAccessor
			System.Web.UI.WebControls.Label
			//System.Web.UI.HtmlControls.HtmlGenericControl
			System.Web.UI.HtmlControls.HtmlContainerControl
			//System.Web.UI.ITextControl 
				ctl =
					(System.Web.UI.HtmlControls.HtmlContainerControl)
					//(System.Web.UI.ITextControl)
					gv.Rows[row].FindControl(ctlName);
			string value = default_;
			if (null != ctl)
			{
				//value = ctl.Text;
				value = ctl.InnerText;
			}*/
			lastSuccessLogMsg = String.Format(msg + "roompattern.{0}={1}", ctlName, value);
			return value;
		}
	}
}




