using System;
using System.Collections;
using System.Configuration;
using System.Data;

using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using System.Collections.Generic;
using System.Text;
using System.Net;
using log4net;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Data.Services.Common;
using Xrm; //XrmEarlyBinding;	
#region Debugging Notes
//using System.Linq;

// TestData
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
#endregion

namespace RoomBlock1
{
    public partial class Grid : System.Web.UI.Page
	{
#region "Variable Declaration"

		private static ILog logger = LogManager.GetLogger(typeof(Grid));

		private static bool bSHOW_ROOM_GUID = false; //always false, for debugging presumably

		//ytodo titles Actuals with sub-headings tr colspan="2" tr


		public static OrganizationService _service;

#endregion //"Variable Declaration"

		/*private DataTable dtretrieve
		{
			get
			{
				return (this.ViewState["dtretrieve"] != null ? 
					(DataTable)this.ViewState["dtretrieve"] : 
					new DataTable());
			}
			set
			{
				this.ViewState["dtretrieve"] = value;
			}
		}*/

		// Backup/Snapshot of dtretrieve at initialization.
		// Or intial empty grid at creation
		/*private DataTable dtRoomBlock
		{
			get
			{
				return (this.ViewState["dtRoomBlock"] != null ? 
					(DataTable)this.ViewState["dtRoomBlock"] : 
					new DataTable());
			}
			set
			{
				this.ViewState["dtRoomBlock"] = value;
			}
		}*/

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
				string eventid = Request.QueryString["recordid"].ToString();
				//eventid = eventid.Replace("{", "").Replace("}", "");
				return eventid;
			}
		}

		public string GetUrlEventId
		{get{
			return Request.QueryString["recordid"].ToString().Replace("{", "").Replace("}", "");
		}}

		public string GetUrlEventIdWithClosures
		{get{
			return Request.QueryString["recordid"].ToString();
		}}

		public bool InvalidEventId
		{get{
			return (HttpContext.Current.Request.QueryString["recordid"] == null);
		}}

		private bool EventStatusIsActualizedOrDefinite
		{
			get { return EventStatusCode == RoomBlocks2.Grid.EventStatusCodes.Definite.ToString(); } 
		}

		private string Username
		{
			get { return (string)this.ViewState["username"]; }
			set { this.ViewState["username"] = value; }
		}

		public DataTable InitDataTable(GridView gv)
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(RoomBlocks2.Grid.Heading(RoomBlocks2.Grid.Columns.DayNumber));
			dt.Columns.Add(RoomBlocks2.Grid.Heading(RoomBlocks2.Grid.Columns.DayOfWeek));
			dt.Columns.Add(RoomBlocks2.Grid.Heading(RoomBlocks2.Grid.Columns.Date));
			dt.Columns.Add(RoomBlocks2.Grid.Heading(RoomBlocks2.Grid.Columns.OriginalPercentOfPeak));
			dt.Columns.Add(RoomBlocks2.Grid.Heading(RoomBlocks2.Grid.Columns.OriginalBlock));
			dt.Columns.Add(RoomBlocks2.Grid.Heading(RoomBlocks2.Grid.Columns.CurrentPercentOfPeak));
			dt.Columns.Add(RoomBlocks2.Grid.Heading(RoomBlocks2.Grid.Columns.CurrentBlock));
			dt.Columns.Add(RoomBlocks2.Grid.Heading(RoomBlocks2.Grid.Columns.RoomBlockId));
			dt.Columns.Add(RoomBlocks2.Grid.Heading(RoomBlocks2.Grid.Columns.ActualPercentOfPeak));
			dt.Columns.Add(RoomBlocks2.Grid.Heading(RoomBlocks2.Grid.Columns.ActualBlock));

			return dt;
		}

#if false
		public void Build_EmptyRoomBlock_Datatable()
		{
			try
			{
				if (InvalidEventId)
					return;

				string eventid = Request.QueryString["recordid"].ToString();

				// Get the dates from the event.
				var cols = new ColumnSet(new String[] { 
					"new_arrivaldate", 
					"new_departuredate" });
				var evt = (Opportunity)_service.Retrieve(
					"opportunity", new Guid(eventid), cols);

				DateTime arrivalDate = evt.New_arrivaldate ?? DateTime.MinValue;
				DateTime departureDate = evt.New_departuredate ?? DateTime.MinValue;

				// ensure whole days
				arrivalDate = new DateTime(arrivalDate.Year, arrivalDate.Month, arrivalDate.Day);
				departureDate = new DateTime(departureDate.Year, departureDate.Month, departureDate.Day);

				eventid = eventid.Replace("{", "").Replace("}", "");

				DataTable dt = //InitRoomBlockDataTable(); ytodo cleanup
					InitDataTable(GridView2);


				for (int i = 0; i < departureDate.Subtract(arrivalDate).TotalDays; i++)
				{
					DataRow dr = dt.NewRow();
					dr["DayNumber"] = (i + 1).ToString();
					dr["DayofWeek"] = arrivalDate.AddDays(i).DayOfWeek.ToString();
					dr["Date"] = arrivalDate.AddDays(i).ToShortDateString();
					//dr["GUIDID"] = eventid; //ytodo cleanup
					dt.Rows.Add(dr);
				}

				dtRoomBlock = dt;
			}
			catch (Exception ex)
			{
				logger.Error("Build_EmptyRoomBlock_Datatable " + ex.ToString());
			}
		}
#endif
#if false
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bool rpStatusFlag = false;
                string eventid = string.Empty;
                try
                {
					log4net.Config.XmlConfigurator.Configure();

					_service = CrmServiceManager.GetCrmService();

					this.Username = Request.QueryString["username"].ToString();

                    if (HttpContext.Current.Request.QueryString["status"] == null)
                    {
                        if (HttpContext.Current.Request.QueryString["recordid"] != null)
                        {
                            eventid = Request.QueryString["recordid"].ToString();
                            ViewState["_eventID"] = eventid;
							logger.Info("Page_Load: evenit= " + eventid);

							// Save eventid on page
							((HiddenField)form1.FindControl("hdnfdEventID")).Value = eventid;

							EventStatusCode = "";
							EventStatusCode = GetEventStatusCode(_service, eventid);
							logger.Info("20 Page_Load: EventStatusCode= " + EventStatusCode);

							// Receiving rpStatus signals State is Active.
                            if (HttpContext.Current.Request.QueryString["rpStatus"] != null)
                            {
                                rpStatusFlag = true;
                                UpdateStatusActiveAndUpdateUserForAllRoomPatternDays(_service, eventid);
                            }
                            else
                                rpStatusFlag = false;

							if (EventStatusCode != string.Empty 
								&& EventStatusIsActualizedOrDefinite)
							{
								LoadGridsFromRoomPatternRecordsOrInitEmptyGrid(_service); //ytodo combine with RetrieveBasedOnStatus function
								btnsave.Text = "Update";
								btnlead.Enabled = true;
							}
							else
							{
								RetrieveBasedOnStatus(_service, rpStatusFlag, 0);
							}

							if (EventStatusIsActualizedOrDefinite)
							{
								//CheckBox_TogglePreAndPostWeek.Style["visibility"] = "visible";
								/*Opportunity o = new Opportunity(); 
								//ytodo remove
								 */
							}
						}
                        else
                        {
							// No eventid provided.
                            btnsave.Visible = false;
                            btnlead.Visible = false;
                        }
                    }
                    else
                    {
						// status urlParm provided
						// signal delete old records (on condition)
                        RetrieveBasedOnStatus(_service, true, 1/*possibly delete*/);
                        btnsave.Text = "Save";
                        btnlead.Enabled = false;
                    }
                }
				catch (System.Web.Services.Protocols.SoapException ex)
				{
					logger.Error(ex.Detail.InnerText, ex);
				}
                catch (Exception ex)
                {
					logger.Error("Page_Load", ex);
                }
                finally //ytodo why is this in finally!!?
                {
                    string _eventID = Convert.ToString(ViewState["_eventID"]);
                    if (_eventID != null && _eventID != string.Empty)
                    {
                        EntityCollection entityCollection = 
							GetRoomPatternRecordsOrderedByDate(_eventID, _service);

						// Roompatterns records and HAVE Dates
						if (entityCollection.Entities.Count > 0 && ViewState["DisplayDateisNULL"] == null) 
                        {
                            btnsave.Text = "Update";
                            btnlead.Enabled = true;
                        }
						// Roompatterns records and NULL Dates
						else if (ViewState["DisplayDateisNULL"] != null)
                        {
                            btnsave.Enabled = false;
                            btnlead.Enabled = false;
                        }
						// NO Roompatterns records
						else
                        {
                            btnsave.Text = "Save";
                            btnlead.Enabled = false;
                            ResetPeakRoomNightsIfNoRoomblocks(_eventID);
                        }
                    }

					//ytodo for a single load or functionCall actuals EventQuery should be performed only once!!
					EventActualRoomblocks ActualRoomBlockLogicAndEventData = new EventActualRoomblocks(this);
					if (ActualRoomBlockLogicAndEventData.Bcmc_Actualized)
						btnlead.Enabled = false;
                }
            }
        }
#endif

		public void UpdatesDebug(Microsoft.Xrm.Sdk.AttributeCollection attribs)
		{
			if (!EventEntityDataBase.DebugShowSecondGrid)
				return;

			string htmlBody = "<div>{0}</div>";
			//string htmlEl = "<span>{0}</span>";
			string html = "";
			foreach (var x in attribs)
			{
				string item = String.Format("<span>{0}: {1}</span>", x.Key, x.Value);
				html += item;
			}
			this.DbgInfo.InnerHtml += String.Format(htmlBody, html);
		}

		public Guid Create(Entity e) //CreateRoomPatternRec
		{
			UpdatesDebug(e.Attributes);
			if (EventEntityDataBase.DebugDisableSaveToDB)
				return Guid.Empty;
			else
			{
				New_roompattern roompattern = e.ToEntity<New_roompattern>();
				roompattern.New_EventId = new EntityReference(Opportunity.EntityLogicalName, new Guid(hdnfdEventID.Value));
				return _service.Create(roompattern);
			}
		}
		public Guid Update(Opportunity o)
		{
			UpdatesDebug(o.Attributes);
			if (!EventEntityDataBase.DebugDisableSaveToDB)
			{
				_service.Update(o);
				return o.Id;
			}
			return Guid.Empty;
		}

		public Guid Update(New_roompattern roompattern)
		{
			// When additional days are added 
			// pre and prior for actual roomblock mode
			if (null == roompattern.Id
				|| Guid.Empty == roompattern.Id)
				return Create(roompattern);
			else //the usual case
			{
				UpdatesDebug(roompattern.Attributes);
				if (!EventEntityDataBase.DebugDisableSaveToDB)
				{
					_service.Update(roompattern);
					return roompattern.Id;
				}
			}
			return Guid.Empty;
		}

		public string UpdateEventRoomNights(string eventid, int totalRoomBlocks, int peakRoomBlock)
		{
			try
			{
				Opportunity o = new Opportunity();
				o.OpportunityId = new Guid(eventid);
				o.New_HotelRoomNights = totalRoomBlocks;
				o.New_PeakHotelRoomNights = peakRoomBlock;

				Update(o);
			}
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
			catch (Exception ex)
			{
				logger.Error("UpdateEventRoomNights " + ex.ToString());
			}

			// Build the JavaScript code to be used to update the CRM event form.
			return String.Format("updateParent({0}, {1}); ",
							totalRoomBlocks, peakRoomBlock); //ytodo not currently working
		}

		// UpdateEvent_Calc_TotalRoomblocksAndPeak
		public String UpdateEvent_withCalc_TotalRoomblocksAndPeak(GridView gv, string _eventID)
		{
			string ctlId;
			if (gv.Columns[(int)RoomBlocks2.Grid.Columns.ActualBlock].Visible)
				ctlId = RoomBlocks2.Grid.GridCtrlNames.txtActualBlock.ToString();
			else
				ctlId = RoomBlocks2.Grid.GridCtrlNames.txtCBlock.ToString();

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
				logger.InfoFormat("BindPeakRoom: TotalHotelRooms={0} Peak={1} Event={2}",
					_totalRoomBlock, _peakRoomNight, _eventID);

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


#if false
        private void ResetPeakRoomNightsIfNoRoomblocks(string _eventID)
        {
            try
            {
				//logger.Info("70 Dynamicgridview Page: UpdatePeakRoomNights : Started");
				logger.Info("70 Dynamicgridview Page:  UpdatePeakRoomNights Method: Before service execution");

                Entity entity = new Entity("new_roompattern");
                EntityCollection entityCollection = GetRoomPatternRecordsOrderedByDate(_eventID, _service);

                //logger.Info("Dynamicgridview Page:  UpdatePeakRoomNights Method: After service execution");
                logger.Info("80 Dynamicgridview Page: entityCollection.BusinessEntities.Length:: " + entityCollection.Entities.Count);

                if (entityCollection.Entities.Count == 0)
                {
                    Entity oppEvent = new Entity("opportunity");
                    oppEvent["new_hotelroomnights"] = 0;
                    oppEvent["new_peakhotelroomnights"] = 0;
                    oppEvent["opportunityid"] = new Guid(_eventID);
					Update(oppEvent.ToEntity<Opportunity>());
                }
				logger.Info("90 Dynamicgridview Page: UpdatePeakRoomNights : Ended NS00: " + _eventID);
            }
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
            catch (Exception ex)
            {
				logger.Error("Dynamicgridview Page: UpdatePeakRoomNights()", ex);
            }
        }

		/// <summary>
		/// Copy current roomblock and percentage to Originals
		/// Update roompattern records from grid values
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        protected void btnlead_Click(object sender, EventArgs e)
        {
			//if (!ValidateProposedDates())
			//	return;
            SendLeadtoHotel();
        }

        private void SendLeadtoHotel()
        {
            try
            {
                string _eventID = Convert.ToString(ViewState["_eventID"]);

                if (GridView1.Rows.Count > 0)
                {
					//ytodo we're updating twice for no reason here, tidy up.
					if (EventStatusCode == EventStatusCodes.Definite.ToString())
						UpdateRoomblockRecords_fromGrids(_eventID, 0, EventStatusCodes.Definite);
						//UpdateRoomblockDefiniteStatus(_eventID, 0);
					else
                        UpdateRoomblockRecords_fromGrids(_eventID, 0);
                        
					// Copy from CurrentBlock,PercentPeak to Originals
					// for all records
                    foreach (GridViewRow Item in GridView1.Rows)
                    {
                        TextBox txtcurrent = (TextBox)Item.FindControl("txtCBlock");
                        string currentroom = txtcurrent.Text;

						System.Web.UI.WebControls.Label txtoriginal = (System.Web.UI.WebControls.Label)Item.FindControl("lblOblock");
                        txtoriginal.Text = currentroom;

                        HiddenField txt1 = (HiddenField)Item.FindControl("hdnfdValue");
                        string current = txt1.Value;
						System.Web.UI.WebControls.Label originalPeakCtrl = (System.Web.UI.WebControls.Label)Item.FindControl("lblOPeak");
                        originalPeakCtrl.Text = current;
                    }

					// Save all updated recs to roompattern db //ytodo combine with updateProc above...
					UpdateSaveRowsToRoomPattern(GridView1);
					/* ytodo tidy up
                    for (int i = 0; i < gvFormLoad.Rows.Count; i++)
                    {
						//string RoomGUID = gvRoomBlock.Rows[i].Cells[7].Text;
						New_roompattern roompattern = new New_roompattern();
						roompattern.Id = new Guid(dtretrieve.Rows[i]["RoomGUID"].ToString());
						roompattern.Bcmc_OriginalpercentofPeak = ExtractGridRowCtrl_Int(gvFormLoad, i, GridCtrlNames.lblOPeak.ToString());
						roompattern.Bcmc_OriginalRoomBlock = ExtractGridRowCtrl_Int(gvFormLoad, i, GridCtrlNames.lblOblock.ToString()); //ytodo test works for labels
						roompattern.New_PercentofPeak = ExtractGridRowHidden_Int(gvFormLoad, i, GridCtrlNames.hdnfdValue.ToString()); //tytodo test hidden works
						roompattern.New_RoomBlock = ExtractGridRowCtrl_Int(gvFormLoad, i, GridCtrlNames.txtCBlock.ToString());
						if (gvFormLoad.Columns[(int)GridColumnsFormLoad.ActualBlock].Visible)
						{
							roompattern.Bcmc_ActualBlock = ExtractGridRowCtrl_Int(gvFormLoad, i, GridCtrlNames.txtActualBlock.ToString());
							roompattern.Bcmc_ActualPercentOfPeak = ExtractGridRowCtrl_Int(gvFormLoad, i, GridCtrlNames.txtActualPercentOfPeak.ToString());
						}
						roompattern.bcmc_User = this.Username;

                        //_service.Update(roompattern);
						Update(roompattern);
                    }
					*/
                    string myscript = "alert('Successfully completed.');";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);
                    btnlead.Enabled = true;
                }
                else if (GridView2.Rows.Count > 0)
                {
					//ytodo we're updating twice for no reason here, tidy up.
                    if (EventStatusCode != "10")
                        UpdateRoomblockRecords_fromGrids(_eventID, 0);
                    else
                        UpdateRoomblockDefiniteStatus(_eventID, 0);

					// copy current to orig for all recs
                    foreach (GridViewRow Item in GridView2.Rows)
                    {
                        TextBox txtcurrent = (TextBox)Item.FindControl("txtCBlock");
                        string currentroom = txtcurrent.Text;
						// copy current to orig
						System.Web.UI.WebControls.Label txtoriginal = (System.Web.UI.WebControls.Label)Item.FindControl("lblOblock");
                        txtoriginal.Text = currentroom;

                        HiddenField txt1 = (HiddenField)Item.FindControl("hdnfdValue");
                        string current = txt1.Value;
						// copy current to orig
						System.Web.UI.WebControls.Label txt2 = (System.Web.UI.WebControls.Label)Item.FindControl("lblOPeak");
                        txt2.Text = current;
                    }

					UpdateSaveRowsToRoomPattern(GridView2);
					/* ytodo tidy up
                    for (int i = 0; i < gvRoomBlock.Rows.Count; i++)
                    {
						New_roompattern roompattern = new New_roompattern();
						string RoomGUID = //gvRoomBlock.Rows[i].Cells[8].Text;
								 dtretrieve.Rows[i]["RoomGUID"].ToString();
						Guid current_roompatternid = new Guid(RoomGUID);
						roompattern["new_roompatternid"] = current_roompatternid;

						System.Web.UI.WebControls.
							Label txtOriginalpeak = (System.Web.UI.WebControls.Label)gvRoomBlock.Rows[i].FindControl("lblOPeak");
                        string OriginalPeak = txtOriginalpeak.Text;
                        int intOPeak = Convert.ToInt32(OriginalPeak);
                        if ((((System.Web.UI.WebControls.Label)(gvRoomBlock.Rows[i].FindControl("lblOPeak"))).Text != string.Empty) 
							&& ((System.Web.UI.WebControls.Label)(gvRoomBlock.Rows[i].FindControl("lblOPeak"))).Text != null)
                        {
                            if (OriginalPeak.Contains("."))
                            {
                                intOPeak = Convert.ToInt32(OriginalPeak.Split('.')[0].ToString());
                            }
                            else
                            {
                                intOPeak = Convert.ToInt32(OriginalPeak);
                            }
                        }
                        else
                        {
                            intOPeak = 0;
                        }
                        roompattern["bcmc_originalpercentofpeak"] = intOPeak;

                        HiddenField txtCurrentpeak = (HiddenField)gvRoomBlock.Rows[i].FindControl("hdnfdValue");
                        string CurrentPeak = txtCurrentpeak.Value;
                        int intCPeak = 0;
                        if ((((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))).Value != string.Empty) 
							&& ((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))) != null)
                        {
                            if (CurrentPeak.Contains("."))
                            {
                                intCPeak = Convert.ToInt32(CurrentPeak.Split('.')[0].ToString());
                            }
                            else
                            {
                                intCPeak = Convert.ToInt32(CurrentPeak);
                            }
                        }
                        else
                        {
                            intCPeak = 0;
                        }
                        roompattern["new_percentofpeak"] = intCPeak;

						System.Web.UI.WebControls.
                        Label txtOriginalblock = (System.Web.UI.WebControls.Label)gvRoomBlock.Rows[i].FindControl("lblOblock");
                        string Originalblock = txtOriginalblock.Text;
                        int intOBlock = 0;
                        if ((((System.Web.UI.WebControls.Label)(gvRoomBlock.Rows[i].FindControl("lblOblock"))).Text != string.Empty) 
							&& ((System.Web.UI.WebControls.Label)(gvRoomBlock.Rows[i].FindControl("lblOblock"))).Text != null)
                        {
                            if (Originalblock.Contains("."))
                            {
                                intOBlock = Convert.ToInt32(Originalblock.Split('.')[0].ToString());
                            }
                            else
                            {
                                intOBlock = Convert.ToInt32(Originalblock);
                            }
                        }
                        else
                            intOBlock = 0;
                        roompattern["bcmc_originalroomblock"] = intOBlock;

                        TextBox txtcurrentblock = (TextBox)gvRoomBlock.Rows[i].FindControl("txtCBlock");
                        string Currentblock = txtcurrentblock.Text;
                        int intCBlock = 0;
                        if ((((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != string.Empty) && ((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != null)
                        {
                            if (Currentblock.Contains("."))
                            {
                                intCBlock = Convert.ToInt32(Currentblock.Split('.')[0].ToString());
                            }
                            else
                            {
                                intCBlock = Convert.ToInt32(Currentblock);
                            }
                        }
                        else
                        {
                            intCBlock = 0;
                        }
                        roompattern["new_roomblock"] = intCBlock;

						if (gvRoomBlock.Columns[(int)GridColumnsFormLoad.ActualBlock].Visible)
						{
							roompattern.Bcmc_ActualBlock = ExtractGridRowCtrl_Int(gvRoomBlock, i, GridCtrlNames.txtActualBlock.ToString());
							roompattern.Bcmc_ActualPercentOfPeak = ExtractGridRowCtrl_Int(gvRoomBlock, i, GridCtrlNames.txtActualPercentOfPeak.ToString());
						}


						logger.Info(String.Format("150 Update new_roompatternid {0}=={1}; Roomblock={2}", 
							current_roompatternid,
							gvRoomBlock.Rows[i].Cells[8].Text,
							intCBlock));

						roompattern["bcmc_user"] = this.Username;

                        //_service.Update(roompattern);
						Update(roompattern);
                    }
					*/
                    string myscript = "alert('Successfully completed. ');";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);
                    btnlead.Enabled = false;
                }

				EventActualRoomblocks SetupEventActualRoomBlocks = new EventActualRoomblocks(this);
			}
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
            catch (Exception ex)
            {
				logger.Info(String.Format("SendLeadtoHotel(: {0}",lastSuccessLogMsg));
				logger.Error("SendLeadtoHotel()", ex);
            }
        }

        protected void btnsave_Click(object sender, EventArgs e)
        {
            try
            {
                string _eventID = Convert.ToString(ViewState["_eventID"]);
				//string eventId = ((HiddenField)form1.FindControl("hdnfdEventID")).Value;
				//ytodo combine all eventid vars to one.

				logger.InfoFormat("180 btnsave_Click: eventID1={0}", _eventID);
                if (btnsave.Text == "Save")
                {
                    BindSave(_eventID);
                }
                else if (btnsave.Text == "Update")
                {
                    if (EventStatusCode != "10")
                        UpdateRoomblockRecords_fromGrids(_eventID, 1);
                    else
                        UpdateRoomblockDefiniteStatus(_eventID, 1);
                }
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

        protected void gvFormLoad_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            foreach (TableCell cell in e.Row.Cells)
            {
                if (e.Row.RowIndex >= 0)
                {
					string cellStyle = "border-color:Gray;padding-left:8px;";
                    cell.Attributes["Style"] = cellStyle;
					foreach (Control ctl in cell.Controls)
					{
						//if (ctl is HtmlControl)
						//	(ctl as HtmlControl).Attributes["style"] = cellStyle;
					}
                }
            }

			// Add any holidays to the day or week.
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				//ytodo this is not the correct place for this.
				Control ctlActualPercentOfPeak = e.Row.FindControl("txtActualPct");
				if (null != ctlActualPercentOfPeak)
				{
					// This won't in gvRoomBlock only once there is actualized data, i.e. in gvFormLoad
					TextBox txtActualPercentOfPeak = (TextBox)ctlActualPercentOfPeak;
					txtActualPercentOfPeak.Attributes.Add("readonly", "readonly"); //ensure will be in postback
					/*txtActualPercentOfPeak.BorderStyle = BorderStyle.None;
					txtActualPercentOfPeak.BorderWidth = Unit.Pixel(0);*/ //taken care of in css
					txtActualPercentOfPeak.Enabled = false; //prevent focus and caret
				}

				string date = e.Row.Cells[(int)Columns.Date].Text;
				try
				{
					DateTime dt = DateTime.Parse(date);
					String holiday = HelperFunctions.getHoliday(dt, _service, Cache);
					if (String.IsNullOrEmpty(holiday))
						return;

					e.Row.Cells[(int)Columns.DayOfWeek].Text += "<br/>(" + holiday + ")";
					e.Row.Cells[(int)Columns.DayOfWeek].Attributes["Style"] = "background-color:LightYellow;";
					//logger.InfoFormat("gvFormLoad_RowDataBound() - {0} - {1}", e.Row.Cells[1].Text, e.Row.Cells[2].Text);
				}
				catch (Exception ex)
				{
					logger.Error(String.Format("gvFormLoad_RowDataBound date={0}", date));
					// Indication of invalid data introduced during debugging remove...///ytodo remove this code.
					/*
					DataTable dt = (DataTable)gvFormLoad.DataSource;
					dt.Rows.RemoveAt(e.Row.RowIndex);
					 * */
				}
			}
        }

//#region UserdefinedMethods
		private enum DeleteAllRoomBlockRecordsOnChangeStatus
		{
			False = 0,
			True = 1
		};
        private void RetrieveBasedOnStatus(OrganizationService _service, bool rpFlag/*ytodo checkout*/, int DeleteAllRoomBlockRecordsOnChangeStatus)
        {
            try
            {
				if (HttpContext.Current.Request.QueryString["recordid"] == null)
                {
					btnsave.Visible = false;
					btnlead.Visible = false;
					return;
				}

                string getRecordid = Request.QueryString["recordid"].ToString();
                string _eventID = getRecordid.Replace("{", "").Replace("}", "");
				ViewState["_eventID"] = _eventID;

				// Get the dates from the event.
				EventActualRoomblocks ActualRoomBlocksAndEventData = new EventActualRoomblocks(this, true);

				DateTime dtArrivalDate = ActualRoomBlocksAndEventData.arrivalDate;
				DateTime dtdeparture = ActualRoomBlocksAndEventData.departureDate;

				try
                {
                    EntityCollection entityCollection = GetRoomPatternRecordsOrderedByDate(_eventID, _service);
                    Hashtable hshTableRoomPattern = new Hashtable();
                    if (entityCollection.Entities.Count > 0)
                    {
                        for (int j = 0; j < entityCollection.Entities.Count; j++)
                        {
                            New_roompattern roompattern = (New_roompattern)entityCollection.Entities[j];
                            if (roompattern.Bcmc_Date.HasValue)
                                hshTableRoomPattern.Add("bcmc_date" + j, roompattern.Bcmc_Date.Value.ToShortDateString());
                            else
                                hshTableRoomPattern.Add("bcmc_date" + j, "01/01/0001");
                        }

                        //if (hshTableRoomPattern.Count > 0) //this HAS TO have values as its based on the entityCollection
                        {
                            string roomblock_ArrivalDate_uniqueFromHash = hshTableRoomPattern["bcmc_date0"].ToString();
                              
							// Have roompattern records been initialized
							// i.e. do they have Dates?
                            if (roomblock_ArrivalDate_uniqueFromHash != "01/01/0001")
                            {
								dtdeparture = dtdeparture.AddDays(-1); //ytodo check this out, does it need to be here. origRetrieve( needs it!! maybe other proc NO!

								// If actualized there should not be anymore date range changes, only pre and prior values may change.
								if (ActualRoomBlocksAndEventData.Bcmc_Actualized
									|| (!ActualRoomBlocksAndEventData.Bcmc_Actualized && ActualRoomBlocksAndEventData.Debugging) //allow entry in debugging mode even though its not a relevant state
									// If Event.arrival and departure dates
									// same as first and last dates in new_roompattern records
									|| (hshTableRoomPattern["bcmc_date0"].ToString() == dtArrivalDate.ToShortDateString()
										&& dtdeparture.ToShortDateString() == 
											hshTableRoomPattern["bcmc_date" + (hshTableRoomPattern.Count - 1).ToString()].ToString()))
                                {
									//ytodo combine with other code which is the same....!
									dtretrieve = InitDataTable(GridView1);

									for (int i = 0; i < entityCollection.Entities.Count; i++)
                                    {
										DataRow drretrieves = dtretrieve.NewRow();
										New_roompattern entity = (New_roompattern)entityCollection.Entities[i];

										drretrieves["RoomGUID"] = (entity["new_roompatternid"] as Guid?).Value.ToString();

										if (entity["new_daynumber"] != null)
											drretrieves["DayNumber"] = entity["new_daynumber"].ToString();
										else
											drretrieves["DayNumber"] = "0";
										if (entity["new_name"] != null)
										{
											drretrieves["DayofWeek"] = entity["new_name"];
										}
										if (entity["bcmc_date"] != null)
										{
											drretrieves["Date"] = (entity["bcmc_date"] as DateTime?).Value.ToShortDateString();
										}
										if (entity.Bcmc_OriginalpercentofPeak.HasValue)
										{
											if (entity["bcmc_originalpercentofpeak"] != null)
											{
												drretrieves["Original % of Peak"] = entity.Bcmc_OriginalpercentofPeak;
											}
										}
										if (entity.New_PercentofPeak != null)
										{
											drretrieves["Current % of Peak"] = entity.New_PercentofPeak.Value;
										}
										if (entity.Bcmc_OriginalRoomBlock != null)
										{
											drretrieves["Original Block"] = entity.Bcmc_OriginalRoomBlock.Value;
										}
										if (entity.New_RoomBlock != null)
										{
											drretrieves["Current Block"] = entity.New_RoomBlock.Value;
										}
										if (entity.Bcmc_ActualPercentOfPeak != null)
										{
											drretrieves["Actual % of Peak"] = entity.Bcmc_ActualPercentOfPeak.Value;
										}
										if (entity.Bcmc_ActualBlock != null)
										{
											drretrieves["Actual Block"] = entity.Bcmc_ActualBlock.Value;
										}

										dtretrieve.Rows.Add(drretrieves);
                                    }
                                    GridView1.DataSource = dtretrieve;
                                    GridView1.DataBind();
									GridView1.Columns[(int)Columns.RoomBlockId].Visible = EventEntityDataBase.DebugShowSecondGrid || bSHOW_ROOM_GUID ? true : false;
									GridView1.Columns[(int)Columns.DayNumber].Visible = EventEntityDataBase.DebugShowSecondGrid ? true : false;
                                    GridView1.Visible = true;
                                    if (GridView1.Rows.Count > 0)
                                    {
                                        DataTable dt = new DataTable();
                                        GridView2.DataSource = dt;
                                        GridView2.DataBind();
										GridView2.Visible = EventEntityDataBase.DebugShowSecondGrid ? true : false;
                                    }
                                    btnsave.Text = "Update";
                                    btnlead.Enabled = true;
                                }
                                else
                                {
                                    if (DeleteAllRoomBlockRecordsOnChangeStatus == 1)
                                        DeleteRoomblock(_eventID);
                                    InitNewEmptyRoomBlockGrid();
                                }
                            }
                            else
                            {
                                DisplayNullDateRecords(entityCollection);
                            }
                        }
                    }
                    else
                    {
                        InitNewEmptyRoomBlockGrid();
                    }

					ActualRoomBlocksAndEventData.AddAdditionalActualRows();
                }
				catch (System.Web.Services.Protocols.SoapException ex)
				{
					logger.Error(ex.Detail.InnerText, ex);
				}
                catch (Exception ex)
                {
                    logger.Error("Dynamicgridview Page: Retrieve Roomblock  Method: Error" + ex.ToString());
                }
            }
			catch (System.Web.Services.Protocols.SoapException ex)
            {
				logger.Error(ex.Detail.InnerText, ex);
            }
            catch (Exception ex)
            {
                logger.Error("Roomblock:" + ex.ToString());
			}
        }

        private void DisplayNullDateRecords(EntityCollection entityCollection)
        {
            try
            {
				dtretrieve = InitDataTable(GridView1);

                for (int i = 0; i < entityCollection.Entities.Count; i++)
                {
                    DataRow drretrieves = dtretrieve.NewRow();

					New_roompattern entity = new New_roompattern();
					entity = (New_roompattern)entityCollection.Entities[i];

                    drretrieves["RoomGUID"] = entity.New_roompatternId.Value.ToString();

                    if (entity.New_DayNumber != null)
                    {
                        drretrieves["DayNumber"] = entity.New_DayNumber.Value;
                    }
                    else
                    {
                        drretrieves["DayNumber"] = "0";
                    }
                    if (entity.New_name != null)
                    {
                        drretrieves["DayofWeek"] = entity.New_name;
                    }
                    if (entity.Bcmc_Date != null)
                    {
                        drretrieves["Date"] = entity.Bcmc_Date.Value.ToShortDateString();
                    }
                    if (entity.Bcmc_OriginalpercentofPeak != null)
                    {
                        drretrieves["Original % of Peak"] = entity.Bcmc_OriginalpercentofPeak.Value;
                    }
                    if (entity.New_PercentofPeak != null)
                    {
                        drretrieves["Current % of Peak"] = entity.New_PercentofPeak.Value;
                    }
                    if (entity.Bcmc_OriginalRoomBlock != null)
                    {
                        drretrieves["Original Block"] = entity.Bcmc_OriginalRoomBlock.Value;
                    }
                    if (entity.New_RoomBlock != null)
                    {
                        drretrieves["Current Block"] = entity.New_RoomBlock.Value;
                    }
					if (entity.Bcmc_ActualPercentOfPeak != null)
					{
						drretrieves["Actual % of Peak"] = entity.Bcmc_ActualPercentOfPeak.Value;
					}
					//if (entity.Bcmc_ActualBlock.HasValue) //ytodo tidy up
					//{
						if (entity.Bcmc_ActualBlock != null)
						{
							drretrieves["Actual Block"] = entity.Bcmc_ActualBlock.Value;
						}
					//}

					dtretrieve.Rows.Add(drretrieves);
                }
                GridView1.DataSource = dtretrieve;
                GridView1.DataBind();
				GridView1.Columns[(int)Columns.RoomBlockId].Visible = EventEntityDataBase.DebugShowSecondGrid || bSHOW_ROOM_GUID ? true : false;
				GridView1.Columns[(int)Columns.DayNumber].Visible = EventEntityDataBase.DebugShowSecondGrid ? true : false; ;
                GridView1.Visible = true;
                if (GridView1.Rows.Count > 0)
                {
                    GridView2.DataSource = new DataTable();
                    GridView2.DataBind();
					GridView2.Visible = EventEntityDataBase.DebugShowSecondGrid ? true : false;
                }

                ViewState["DisplayDateisNULL"] = "DisplayDateisNULL";
            }
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
            catch (Exception ex)
            {
                logger.Error("DisplayNullDateRecords " + ex.ToString()); 
            }             
        }

        private void DeleteRoomblock(string eventid)
        {
            try
            {
				/* select id from new_roompattern
				 * where new_eventid = eventid
				 * 
				 * and delete them all
				 */ 
                string Eventid = eventid;

                ColumnSet columnSet = new ColumnSet(new string[] { "new_roompatternid" });

                ConditionExpression condition = new ConditionExpression();
                condition.AttributeName = "new_eventid";
                condition.Operator = ConditionOperator.Equal;
                condition.Values.Add(Eventid);

                FilterExpression filter = new FilterExpression();
                filter.Conditions.Add(condition);

                QueryExpression query = new QueryExpression();
                query.ColumnSet = columnSet;
                query.Criteria = filter;
                query.EntityName = New_roompattern.EntityLogicalName;

                EntityCollection entityCollection = _service.RetrieveMultiple(query);
                if (entityCollection.Entities.Count > 0)
                {
                    for (int j = 0; j < entityCollection.Entities.Count; j++)
                    {
                        New_roompattern entity = (New_roompattern)entityCollection.Entities[j];
                        if (entity != null)
                        {
							_service.Delete(New_roompattern.EntityLogicalName, new Guid(entity.New_roompatternId.Value.ToString()));
                        }
                    }
                }
            }
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
            catch (Exception ex)
            {
                logger.Error("DeleteRoomblock " + ex.ToString());
            }
        }

        private void InitNewEmptyRoomBlockGrid()
        {
            try
            {
				Build_EmptyRoomBlock_Datatable();
				GridView2.DataSource = dtRoomBlock;
                GridView2.DataBind();
				GridView2.Columns[(int)GridColumnsRoomBlock.DayNumber].Visible = EventEntityDataBase.DebugShowSecondGrid ? true : false;
				//gvRoomBlock.Columns[(int)GridColumnsRoomBlock.EventId].Visible = EventEntityDataBase.DebugShowSecondGrid || bSHOW_ROOM_GUID ? true : false; //ytodo cleanup
				GridView2.Columns[(int)GridColumnsRoomBlock.RoomBlockId].Visible = EventEntityDataBase.DebugShowSecondGrid || bSHOW_ROOM_GUID ? true : false;
                if (GridView1.Rows.Count > 0)
                {
					GridView1.DataSource = new DataTable();
                    GridView1.DataBind();
					GridView1.Visible = EventEntityDataBase.DebugShowSecondGrid ? true : false;
                }
            }
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
            }
            catch (Exception ex)
            {
                logger.Error("RoomBlockGrid" + ex.ToString());
            }
			finally
			{
			}
        }

		private void LoadGridsFromRoomPatternRecordsOrInitEmptyGrid(OrganizationService _service, 
			bool formloadWithExistingRecords = false,
			bool rpFlag = false, 
			int DeleteAllRoomBlockRecordsOnChangeStatus = 0)
        {
            try
            {
				if (HttpContext.Current.Request.QueryString["recordid"] == null)
                {
					btnsave.Visible = false;
					btnlead.Visible = false;
					if (formloadWithExistingRecords) return;
				}

				EventActualRoomblocks ActualRoomBlocksAndEventData = new EventActualRoomblocks(this, true);


                string getRecordid = Request.QueryString["recordid"].ToString();
                string _eventID = getRecordid.Replace("{", "").Replace("}", "");
                ViewState["_eventID"] = _eventID;

				/* ytodo tidy up
				// Get event dates.
				var cols = new ColumnSet(new String[] { "new_arrivaldate", "new_departuredate" });
				var evt = (Opportunity)_service.Retrieve("opportunity", new Guid(_eventID), cols);
				DateTime dtArrivalDate = evt.New_arrivaldate ?? DateTime.MinValue;
				DateTime dtDepartureDate = evt.New_departuredate ?? DateTime.MinValue;
                // ensure whole days
                dtArrivalDate = new DateTime(dtArrivalDate.Year, dtArrivalDate.Month, dtArrivalDate.Day);
				dtDepartureDate = new DateTime(dtDepartureDate.Year, dtDepartureDate.Month, dtDepartureDate.Day);
				*/

				// Get event dates.
				DateTime dtArrivalDate = ActualRoomBlocksAndEventData.arrivalDate;
				DateTime dtDepartureDate = ActualRoomBlocksAndEventData.departureDate;

				try
                {
                    EntityCollection entityCollection = GetRoomPatternRecordsOrderedByDate(_eventID, _service);
                    Hashtable hshTableRoomPattern = new Hashtable();
                    if (entityCollection.Entities.Count > 0)
                    {
						//Detect date range changes
						// Fill in existing dates from new_roompattern records
						// into hash i.e. all dates will be unqiue
						for (int j = 0; j < entityCollection.Entities.Count; j++)
                        {
                            New_roompattern new_roompatten = (New_roompattern)entityCollection.Entities[j];
                            if (new_roompatten.Bcmc_Date != null)
                                hshTableRoomPattern.Add("bcmc_date" + j, new_roompatten.Bcmc_Date.Value.ToShortDateString());
                            else
                                hshTableRoomPattern.Add("bcmc_date" + j, "01/01/0001");
                        }

                        //if (hshTableRoomPattern.Count > 0) //this HAS TO have values as its based on the entityCollection above
                        {
							// Date of first & last 
							// new_roompattern record
                            string FirstRoomPatternRecordDate = hshTableRoomPattern["bcmc_date0"].ToString(); //"bcmc_date" + j
                            string LastRoomPatternRecordDate = hshTableRoomPattern["bcmc_date" + (hshTableRoomPattern.Count - 1).ToString()].ToString();

							bool AreRoompatternRecordsInitialized = (FirstRoomPatternRecordDate != "01/01/0001"); // i.e. do they have Dates?

                            if ((true && !formloadWithExistingRecords) || (AreRoompatternRecordsInitialized && formloadWithExistingRecords))
                            {
								if (formloadWithExistingRecords) dtDepartureDate = dtDepartureDate.AddDays(-1);
									
								bool EventArrivalDepartureSameAsFirstLastRoomPatterns =
									// If actualized there should not be anymore date range changes, only pre and prior values may change.
									ActualRoomBlocksAndEventData.Bcmc_Actualized 
									|| (!ActualRoomBlocksAndEventData.Bcmc_Actualized && ActualRoomBlocksAndEventData.Debugging) //allow entry in debugging mode even though its not a relevant state
									// If Event.arrival and departure dates
									// same as first and last dates in new_roompattern records
									|| (hshTableRoomPattern["bcmc_date0"].ToString() == dtArrivalDate.ToShortDateString()
										 && dtDepartureDate.ToShortDateString() ==
											hshTableRoomPattern["bcmc_date" + (hshTableRoomPattern.Count - 1).ToString()].ToString());
								
                                if ((true && !formloadWithExistingRecords) || (formloadWithExistingRecords && EventArrivalDepartureSameAsFirstLastRoomPatterns))
                                {
									// Create Datatable fields /Grid Headers
									if (formloadWithExistingRecords)
										dtretrieve = InitDataTable(GridView1); //InitFormLoadDataTable(); ytodo cleanup
									else
										dtretrieve = InitDataTable(GridView2); //InitRoomBlockDataTable(); ytodo cleanup

									// Grid data rows from new_roompattern records
                                    for (int i = 0; i < entityCollection.Entities.Count; i++)
                                    {
                                        DataRow drretrieves = dtretrieve.NewRow();
                                        New_roompattern new_roompatten = (New_roompattern)entityCollection.Entities[i];

                                        drretrieves["RoomGUID"] = new_roompatten.New_roompatternId.Value.ToString();
                                        //if (!formloadWithExistingRecords) drretrieves["GUIDID"] = _eventID.ToString(); //ytodo cleanup

                                        if (new_roompatten.New_DayNumber != null)
                                            drretrieves["DayNumber"] = new_roompatten.New_DayNumber.Value;
                                        else
                                            drretrieves["DayNumber"] = "0";

                                        if (new_roompatten.New_name != null)
                                            drretrieves["DayofWeek"] = new_roompatten.New_name;

                                        if (new_roompatten.Bcmc_Date != null)
                                            drretrieves["Date"] = new_roompatten.Bcmc_Date.Value.ToShortDateString();

										if (new_roompatten.Bcmc_OriginalpercentofPeak.HasValue)
										{
											if (new_roompatten.Bcmc_OriginalpercentofPeak != null)
											{
												drretrieves["Original % of Peak"] = new_roompatten.Bcmc_OriginalpercentofPeak.Value;

												if (!formloadWithExistingRecords)
												{
													btnsave.Text = "Update";
													btnlead.Enabled = true;
												}
											}
										}

                                        if (new_roompatten.Bcmc_OriginalRoomBlock != null)
                                        {
											drretrieves["Original Block"] = new_roompatten.Bcmc_OriginalRoomBlock.Value;

											if (!formloadWithExistingRecords)
											{
                                            btnsave.Text = "Update";
                                            btnlead.Enabled = true;
											}
                                        }

                                        if (new_roompatten.New_PercentofPeak != null)
                                        {
                                            drretrieves["Current % of Peak"] = new_roompatten.New_PercentofPeak.Value;

											if (!formloadWithExistingRecords)
											{
                                            btnsave.Text = "Update";
                                            btnlead.Enabled = true;
											}
                                        }

										if (new_roompatten.New_RoomBlock != null)
                                        {
											drretrieves["Current Block"] = new_roompatten.New_RoomBlock.Value;
                                        }
										if (new_roompatten.Bcmc_ActualPercentOfPeak.HasValue)
										{
											if (new_roompatten.Bcmc_ActualPercentOfPeak != null)
											{
												drretrieves["Actual % of Peak"] = new_roompatten.Bcmc_ActualPercentOfPeak.Value;

												if (!formloadWithExistingRecords)
												{
													btnsave.Text = "Update";
													btnlead.Enabled = true;
												}
											}
										}
										if (new_roompatten.Bcmc_ActualBlock.HasValue)
										{
											if (new_roompatten.Bcmc_ActualBlock != null)
											{
												drretrieves["Actual Block"] = new_roompatten.Bcmc_ActualBlock.Value;

												if (!formloadWithExistingRecords)
												{
													btnsave.Text = "Update";
													btnlead.Enabled = true;
												}
											}
										}

                                        dtretrieve.Rows.Add(drretrieves);
                                    }

									// Update grid containing data from roomblock db 
									// for current date range in roomblocks.
									GridView1.DataSource = dtretrieve;
                                    GridView1.DataBind();

									if (!formloadWithExistingRecords) 
									{
										EntityCollection NullDateRoomPatternDays = GetNullDateRoomPatternDays(_eventID, _service);
										bool RoomBlockHasNullDates = !(NullDateRoomPatternDays.Entities.Count == 0);

										//Hashtable hshDateCollections = new Hashtable(); //not used - removed //yair

										// Reset CurrentBlock & %ofPeak if outside Arrival-Departure Range
										if (! RoomBlockHasNullDates)
										{
											// reset roomblock days not in the event range to 0

											// Compare date range between that in event
											// to that fetched from roomblocks.
											// Initialize missing roomblock days/date to 0
											for (int i = 0; i < dtretrieve.Rows.Count; i++) //roomblocks records
											{
												string dayDate = dtretrieve.Rows[i]["Date"].ToString();
												string roomID = dtretrieve.Rows[i]["RoomGUID"].ToString();
												//Added this code for check the existing records in crm, 
												//if those records does not having the date and has null means, we check the record and update it.

												//if (! RoomBlockHasNullDates) no point in this check, its done above.
												//{
													// If a new date out of original range has been added
													// initialize roomblock to blank/0
													if (! IsDateInRange(dtArrivalDate, dtDepartureDate, dayDate))
													{
														dtretrieve.Rows[i]["Current Block"] = "";
														dtretrieve.Rows[i]["Current % of Peak"] = "";

														/*New_roompattern roomEvent = new New_roompattern();
														roomEvent.New_roompatternId = new Guid(roomID);
														roomEvent.New_RoomBlock = (int)0;
														roomEvent.New_PercentofPeak = 0;
														//Commented by ZSL Team on July 27th 2012
														//  _service.Update(roomEvent);
														*/
													}
												//}
												/*else
												{
													RoomBlockHasNullDates = true;
													//hshDateCollections = GetHashOfDatesBetweenArrivalAndDeparture(
													//						dtArrivalDate.ToShortDateString(), 
													//						dtDepartureDate.ToShortDateString());
													break;
												}*/
											}
										}

										// backup snapshot of current datatable? does this keep a full copy?
										dtRoomBlock = dtretrieve;

										// Update grid containing data from roomblock db 
										// for current date range in roomblocks.
										// with days not in the event range reset to 0
										GridView1.DataSource = dtretrieve;
										GridView1.DataBind();

										for (int i = 0; i < dtretrieve.Rows.Count; i++)
										{
											string recordDate = dtretrieve.Rows[i]["Date"].ToString();
											//string roomID = dtretrieve.Rows[i]["RoomGUID"].ToString();

											if (! RoomBlockHasNullDates 
												&& ! IsDateInRange(dtArrivalDate, dtDepartureDate, recordDate))
											{
												GridView1.Rows[i].Enabled = false;
											}
										}
									}
									GridView1.Columns[(int)Columns.RoomBlockId].Visible = EventEntityDataBase.DebugShowSecondGrid || bSHOW_ROOM_GUID ? true : false;
									GridView1.Columns[(int)Columns.DayNumber].Visible = EventEntityDataBase.DebugShowSecondGrid || bSHOW_ROOM_GUID ? true : false;
                                    GridView1.Visible = true;

                                    if (GridView1.Rows.Count > 0)
                                    {
                                        DataTable dt = new DataTable();
                                        GridView2.DataSource = dt;
                                        GridView2.DataBind();
										GridView2.Visible = EventEntityDataBase.DebugShowSecondGrid ? true : false;
                                    }
									if (formloadWithExistingRecords)
									{
										btnsave.Text = "Update";
										btnlead.Enabled = true;
									}
                                }
								else if (formloadWithExistingRecords)
								{
                                    if (DeleteAllRoomBlockRecordsOnChangeStatus == 1)
                                        DeleteRoomblock(_eventID);
                                    InitNewEmptyRoomBlockGrid();
								}
                            }
							else if (formloadWithExistingRecords)
							{
                                DisplayNullDateRecords(entityCollection);
							}
                        }
                    }
                    else
                    {
                        InitNewEmptyRoomBlockGrid();
                    }
                }
				catch (System.Web.Services.Protocols.SoapException ex)
				{
					logger.Error(ex.Detail.InnerText, ex);
				}
                catch (Exception ex)
                {
					LogLastSuccessMsg("RetrieveBasedonStatus(0" + (formloadWithExistingRecords ? " otherFn" : ""));
					logger.Error("720 RetrieveBasedonStatus" + ex.ToString());
                }

				ActualRoomBlocksAndEventData.AddAdditionalActualRows();
			}
			catch (System.Web.Services.Protocols.SoapException ex)
            {
				logger.Error(ex.Detail.InnerText, ex);
            }
			catch (Exception ex)
			{
				LogLastSuccessMsg("RetrieveBasedonStatus(1");
				logger.Error("721 RetrieveBasedonStatus" + ex.ToString());
            }
        }

		private void LogLastSuccessMsg(string functionName)
		{
			logger.Info(String.Format("lastSuccessMsg, {0}: {1}", functionName, lastSuccessLogMsg));
		}

        private Hashtable GetHashOfDatesBetweenArrivalAndDeparture(string _arrivaldate, string __departuredate)
        {
            Hashtable hshDateCollections = new Hashtable();
            try
            {
				logger.Info("750 Dynamicgridview Page:  ProcessDateIsNull   Method: Started");
                DateTime departureDate = Convert.ToDateTime((Convert.ToDateTime(__departuredate)).ToString("MM/dd/yyyy"));
				//logger.Info("CheckDateExists Method: ProcessDateIsNull:Departure Date: " + departureDate);
                DateTime arrivalDate = Convert.ToDateTime((Convert.ToDateTime(_arrivaldate)).ToString("MM/dd/yyyy"));
				//logger.Info("CheckDateExists Method: ProcessDateIsNull:Arrival Date: " + departureDate);

                TimeSpan dtTime = departureDate.Subtract(arrivalDate);

                for (int i = 0; i < Math.Floor(dtTime.TotalDays); i++)
                {
                    hshDateCollections.Add(i, arrivalDate.AddDays(i).ToString("MM/dd/yyyy"));
                }
				//logger.Info("ProcessDateIsNull Method: compared dates Failed. No Same dates found.");
            }
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
            catch (Exception ex)
            {
                logger.Error("ProcessDateIsNull Method: Error" + ex.ToString());
                // throw;
            }
            return hshDateCollections;
        }

		private bool IsDateInRange(DateTime arrivalDate, DateTime departureDate, string in_Date)
        {
            try
            {
				// Remove Time portion of dates, for accurate timespan
				arrivalDate = Convert.ToDateTime(arrivalDate.ToString("MM/dd/yyyy"));      //arrivalDate.ToShortDateString()
				departureDate = Convert.ToDateTime(departureDate.ToString("MM/dd/yyyy"));
				TimeSpan dateSpan = departureDate.Subtract(arrivalDate);

				// Get comparison date into correct format for exact comparisons
                string date = Convert.ToDateTime(in_Date).ToString("MM/dd/yyyy");

				for (int i = 0; i < Math.Floor(dateSpan.TotalDays); i++)
                {
                    if (date == arrivalDate.AddDays(i).ToString("MM/dd/yyyy"))
                    {
                        return true;
                    }
                }
            }
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
            catch (Exception ex)
            {
                logger.Error("CheckDateExists Method: Error" + ex.ToString());
                // throw;
            }
            return false;
        }

        private string GetEventStatusCode(OrganizationService _service, string eventid)
        {
            string _EventStatusCode = "";
            try
            {
				/* select bcmc_eventname, statuscode
				 * from opportunity as event
				 * where opportunityid = eventid
				 * 
				 * expect only one result of course
				 * return statuscode
				 */
				ColumnSet columnSet = new ColumnSet(new string[] { "bcmc_eventname", "statuscode" });

                ConditionExpression condition_new_eventid = new ConditionExpression();
                condition_new_eventid.AttributeName = "opportunityid";
                condition_new_eventid.Operator = ConditionOperator.Equal;
                condition_new_eventid.Values.Add(eventid);

                FilterExpression filter_Eventid = new FilterExpression();
                filter_Eventid.Conditions.Add(condition_new_eventid);

                // Create the query.
                QueryExpression query = new QueryExpression();

                // Set the properties of the query.
                query.ColumnSet = columnSet;
                query.Criteria = filter_Eventid;
                query.EntityName = Opportunity.EntityLogicalName;

                EntityCollection entityCollection = _service.RetrieveMultiple(query);
                Hashtable hshTableOpportunity = new Hashtable();
                if (entityCollection.Entities.Count > 0)
                {
                    for (int j = 0; j < entityCollection.Entities.Count; j++)
                    {
                        Opportunity opportunity = (Opportunity)entityCollection.Entities[j];
                        _EventStatusCode = opportunity.StatusCode.Value.ToString();
                    }
                }
            }
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
            catch (Exception ex)
            {
				logger.Error("GetEventStatusCode Method" + ex.ToString());
            }

            return _EventStatusCode;
        }

		private EntityCollection GetRoomPatternRecordsOrderedByDate(string _eventID, OrganizationService _service)
        {
			/* select from new_roompattern
			 * where new_eventid = eventid
			 * order by bcmc_date asc
			 */  
			ConditionExpression condition_new_eventid = new ConditionExpression();
            condition_new_eventid.AttributeName = "new_eventid";
            condition_new_eventid.Operator = ConditionOperator.Equal;
            condition_new_eventid.Values.Add(_eventID);

            OrderExpression ordeErp = new OrderExpression();
            ordeErp.AttributeName = "bcmc_date";
            ordeErp.OrderType = OrderType.Ascending;

            FilterExpression filter_Eventid = new FilterExpression();
            filter_Eventid.Conditions.Add(condition_new_eventid);

            QueryExpression query = new QueryExpression();
            query.ColumnSet.AddColumns(new string[] { 
				"new_daynumber", 
				"new_name", 
				"bcmc_date", 
				"new_percentofpeak", 
				"bcmc_originalpercentofpeak", 
				"bcmc_originalroomblock", 
				"new_roomblock",
				"bcmc_actualpercentofpeak",
				"bcmc_actualblock" });
            query.Criteria = filter_Eventid;
            query.Orders.Add(ordeErp);
            query.EntityName = New_roompattern.EntityLogicalName;

            EntityCollection entityCollection = _service.RetrieveMultiple(query);
            return entityCollection;
        }

        private EntityCollection GetNullDateRoomPatternDays(string _eventID, OrganizationService _service)
        {
			/* select ... from new_roompattern
			 * where new_eventid = eventid
			 *   and bcmc_date is NULL
			 */
			ColumnSet columnSet = new ColumnSet(new string[] { 
				"new_roompatternid"
				/*"new_daynumber", 
				"new_name", 
				"bcmc_date", 
				"new_percentofpeak", 
				"bcmc_originalpercentofpeak", 
				"bcmc_originalroomblock", 
				"new_roomblock"*//*, 
				"bcmc_actualblock"*/ }); //ytododev no point in fetching all those fields
            ConditionExpression condition_new_eventid = new ConditionExpression();
            condition_new_eventid.AttributeName = "new_eventid";
            condition_new_eventid.Operator = ConditionOperator.Equal;
            condition_new_eventid.Values.Add(_eventID);

            ConditionExpression condition_date = new ConditionExpression();
            condition_date.AttributeName = "bcmc_date";
            condition_date.Operator = ConditionOperator.Null;

            FilterExpression filter_Eventid = new FilterExpression();
            filter_Eventid.FilterOperator = LogicalOperator.And;
            filter_Eventid.Conditions.AddRange( new ConditionExpression[] { condition_new_eventid, condition_date });

            QueryExpression query = new QueryExpression();
            query.ColumnSet = columnSet;
            query.Criteria = filter_Eventid;
            query.EntityName = New_roompattern.EntityLogicalName;

			EntityCollection entityCollection = _service.RetrieveMultiple(query);
            return entityCollection;
        }

        private void BindSave(string _eventID)
        {
            try
            {
				bool changes = UpdateSaveRowsToRoomPattern(GridView2);
#if false
				// ytodo tidy up
                Guid crmguid = Guid.Empty;

                for (int i = 0; i < gvRoomBlock.Rows.Count; i++)
                {
					int daynumber = i + 1;//Convert.ToInt32(dtRoomBlock.Rows[i]["DayNumber"]);
                    string new_week = dtRoomBlock.Rows[i]["DayofWeek"].ToString();

                    DateTime roomblockdate = Convert.ToDateTime(dtRoomBlock.Rows[i]["Date"]);

					int new_originalpeak = 0;
					System.Web.UI.WebControls.Label lblOriginalPeak = (System.Web.UI.WebControls.Label)(gvRoomBlock.Rows[i].FindControl("lblOPeak"));
                    if (null != lblOriginalPeak.Text && lblOriginalPeak.Text != string.Empty)
                    {
						if (lblOriginalPeak.Text.Contains("."))
							new_originalpeak = Convert.ToInt32(lblOriginalPeak.Text.Split('.')[0].ToString());
                        else
							new_originalpeak = Convert.ToInt32(lblOriginalPeak.Text);
                    }

                    int new_Original = 0;
					System.Web.UI.WebControls.Label labelOriginalBlock = (System.Web.UI.WebControls.Label)(gvRoomBlock.Rows[i].FindControl("lblOblock"));
                    if (null != labelOriginalBlock.Text && labelOriginalBlock.Text != string.Empty)
                    {
                        new_Original = Convert.ToInt32(labelOriginalBlock.Text);
                    }

                    int new_percentofpeak = 0;
					HiddenField hdnFld = (HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue")); //ytododev why is this peak a hidden fld?
                    if (null != hdnFld.Value && hdnFld.Value != string.Empty)
                    {
                        if (hdnFld.Value.Contains("."))
                            new_percentofpeak = Convert.ToInt32(hdnFld.Value.Split('.')[0].ToString());
                        else
                            new_percentofpeak = Convert.ToInt32(hdnFld.Value);
                    }

                    int new_Current = 0;
					TextBox txtBoxCurrent = (TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"));
                    if (null != txtBoxCurrent.Text && txtBoxCurrent.Text != string.Empty)
                    {
						new_Current = Convert.ToInt32(txtBoxCurrent.Text);
                    }

                    EntityReference new_eventid = 
						new EntityReference("opportunity", new Guid(_eventID));

                    Entity myroomblock = new Entity(New_roompattern.EntityLogicalName);
					myroomblock["new_daynumber"] = daynumber;
					myroomblock["new_name"] = new_week;
					myroomblock["bcmc_date"] = roomblockdate;
					myroomblock["bcmc_originalpercentofpeak"] = new_originalpeak;
					myroomblock["bcmc_originalroomblock"] = new_Original;
					myroomblock["new_percentofpeak"] = new_percentofpeak;
					myroomblock["new_roomblock"] = new_Current;
					myroomblock["new_eventid"] = new_eventid;
					myroomblock["bcmc_user"] = this.Username;
					if (ActualsVisible(gvRoomBlock))//ytododev check this works
					{
						SetEntityAttribute_FromGridRow(myroomblock, "bcmc_actualblock", "txtActualBlock", gvRoomBlock, i);
						SetEntityAttribute_FromGridRow(myroomblock, "bcmc_actualpercentofpeak", "txtActualPercentOfPeak", gvRoomBlock, i);
					}

					//crmguid = _service.Create(myroomblock);
					crmguid = Create(myroomblock);
					logger.Info("990 BindSave new_roomblockid= " + crmguid.ToString());

					dtRoomBlock.Rows[i]["RoomGUID"] = crmguid.ToString();

					/* Not necessary as Sync is done in 'Retrieve' below
					// Sync corresponding datatable to refresh gridview with new guids.
                    dtRoomBlock.Rows[i]["RoomGUID"] = crmguid.ToString();
					dtRoomBlock.Rows[i]["DayNumber"] = daynumber.ToString();
					dtRoomBlock.Rows[i]["DayofWeek"] = new_week;
					dtRoomBlock.Rows[i]["Date"] = roomblockdate.ToShortDateString();
					dtRoomBlock.Rows[i]["Original % of Peak"] = new_originalpeak.ToString();
					dtRoomBlock.Rows[i]["Current % of Peak"] = new_percentofpeak.ToString();
					dtRoomBlock.Rows[i]["Original Block"] = new_Original.ToString();
					dtRoomBlock.Rows[i]["Current Block"] = new_Current.ToString();
					//GUIDID
					*/ 
				}
				/* Not necessary as Sync is done in 'Retrieve' below
				// Sync Grid, with new guids
				gvRoomBlock.DataSource = dtRoomBlock;
				gvRoomBlock.DataBind();
				*/
#endif

				String jsCode = UpdateEvent_withCalc_TotalRoomblocksAndPeak(GridView2, _eventID);

                if (changes)//crmguid != Guid.Empty)
                {
					string myscript = jsCode + "alert('Successfully saved ');";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);
                    btnsave.Text = "Update";
                    btnlead.Enabled = true;
                }
                RetrieveBasedOnStatus(_service, true, 0);
                btnlead.Enabled = true;
            }
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
            catch (Exception ex)
            {
                logger.Error("SaveBind: " + ex.ToString());
            }
        }

		class UpdateOptions
		{
			public bool UpdateDayNumber = false;
			public bool NewRecord = true;
			public bool ActualsVisible = false;
			public bool ActualHasAValue = false;
			public bool RowDisabled = false;
		} 
		private UpdateOptions defaultUpdateOptions = new UpdateOptions();
		
		/* ytodo cleanup
		private string GetFieldS(GridView gv, int row, int col)
		{
			return dtretrieve.Rows[row].ItemArray[col].ToString();
		}
		private int GetFieldI(GridView gv, int row, int col)
		{
			string value = GetFieldS(gv, row, col);
			if ("" == value)
				value = "0";
			else
			{
				if (value.Contains("."))
					value = value.Split('.')[0].ToString();
			}
			return Convert.ToInt32(value);
		}*/
		private bool FL(GridView gv)
		{
			return gv.ID == "gvFormLoad";
		}

		private bool UpdateSaveRowsToRoomPattern(GridView gv, UpdateOptions updateOptions = null)
		{
			if (null == updateOptions)
				updateOptions = new UpdateOptions(); //default

			updateOptions.ActualsVisible = ActualsVisible(gv);
			int actualsDayNumberOffsetFromFirstValue = 0;

			int changes = 0;

			for (int row = 0; row < gv.Rows.Count; row++)
			{
				/*string RoomGUID = ("gvFormLoad" == gv.ID) 
						? gv.Rows[i].Cells[(int)GridColumnsFormLoad.RoomBlockId].Text
						: gv.Rows[i].Cells[(int)GridColumnsRoomBlock.RoomBlockId].Text; //ytodo //this is used for definite
					* //ytodo remove eventually
				*/
				string RoomPatternGuid = null;
				if (dtretrieve.Rows.Count >= row+1) 
					RoomPatternGuid = dtretrieve.Rows[row]["RoomGUID"].ToString();
				updateOptions.NewRecord = (null == RoomPatternGuid || "" == RoomPatternGuid);

				New_roompattern roompattern = new New_roompattern();
				if (!updateOptions.NewRecord)
					roompattern.New_roompatternId = new Guid(RoomPatternGuid);

				roompattern.bcmc_User = this.Username;

				roompattern.Bcmc_OriginalpercentofPeak = ExtractGridRowCtrl_Int(gv, row, "lblOPeak");
				roompattern.Bcmc_OriginalRoomBlock = ExtractGridRowCtrl_Int(gv, row, "lblOblock");

				// Disabled gvFormLoad Records Current_RoomBlock is set to zero.
				roompattern.New_PercentofPeak = updateOptions.RowDisabled ? 0 : ExtractGridRowHidden_Int(gv, row, "hdnfdValue");
				roompattern.New_RoomBlock = updateOptions.RowDisabled ? 0 : ExtractGridRowCtrl_Int(gv, row, "txtCBlock");

				if (updateOptions.ActualsVisible)
				{
					string actualBlock = ExtractGridRowCtrl_Text(gv, row, GridCtrlNames.txtActualBlock.ToString());
					if (!updateOptions.ActualHasAValue && null != actualBlock && "" != actualBlock)
						updateOptions.ActualHasAValue = true;

					roompattern.Bcmc_ActualPercentOfPeak = ExtractGridRowCtrl_Int(gv, row, GridCtrlNames.txtActualPercentOfPeak.ToString()); //ytodo tidy up
					roompattern.Bcmc_ActualPercentOfPeak = ExtractGridRowHidden_Int(gv, row, GridCtrlNames.hdnActualPercentOfPeak.ToString());
					roompattern.Bcmc_ActualPercentOfPeak = ExtractGridRowCtrl_Int(gv, row, GridCtrlNames.txtActualPct.ToString());
					roompattern.Bcmc_ActualBlock = ExtractGridRowCtrl_Int(gv, row, GridCtrlNames.txtActualBlock.ToString());
				}

				if (updateOptions.ActualsVisible && !updateOptions.ActualHasAValue)
					actualsDayNumberOffsetFromFirstValue++;

				//if (updateOptions.UpdateDayNumber) //not relevant anymore.
				{
					roompattern.New_DayNumber = row - actualsDayNumberOffsetFromFirstValue + 1; //ytodo why is this needed only for gvFormLoad?
				}

				if (updateOptions.ActualsVisible && !updateOptions.ActualHasAValue)
					continue; // This is not yet a real record

				if (updateOptions.NewRecord)
				{
					//roompattern.New_DayNumber = row + 1;		//Convert.ToInt32(gv.Rows[row].Cells[(int)GridColumnsFormLoad.DayNumber].Text);
					roompattern.Bcmc_Date = Convert.ToDateTime(gv.Rows[row].Cells[(int)Columns.Date].Text);
					roompattern.New_name = roompattern.Bcmc_Date.Value.DayOfWeek.ToString();		//gv.Rows[row].Cells[(int)GridColumnsFormLoad.DayOfWeek].Text;
					Guid guid = Create(roompattern); //CreateRoomPatternRec(roompattern); //Save
					gv.Rows[row].Cells[(int)Columns.RoomBlockId].Text = guid.ToString();
					if (Guid.Empty != guid)
						changes++;
				}
				else
				{
					Guid guid = Update(roompattern); //CreateRoomPatternRec(roompattern); //Save
					gv.Rows[row].Cells[(int)Columns.RoomBlockId].Text = guid.ToString(); // its possible that create occured in update? or once upon a time it was...
					if (Guid.Empty != guid)
						changes++;
				}
			}
			return changes > 0;
		}

		private void UpdateRoomblockRecords_fromGrid(
			string _eventID, int Updating, 
			GridView gv, 
			EventStatusCodes eventStatusCode = EventStatusCodes.NotDefinite)
		{
			try
			{
				UpdateOptions updateOptions = new UpdateOptions();
				updateOptions.UpdateDayNumber = (EventStatusCodes.Definite == eventStatusCode
													&& "gvFormLoad" == gv.ID);
				if (gv.Rows.Count > 0)
				{
					UpdateSaveRowsToRoomPattern(gv, updateOptions);

					// Update an attribute retrieved via RetrieveAttributeRequest
					String jsCode = UpdateEvent_withCalc_TotalRoomblocksAndPeak(gv, _eventID);

					// Updatestatus is when btnSave.Text == "Update"
					if ((1 == Updating && "gvFormLoad" == gv.ID)
						|| "gvRoomBlock" == gv.ID
						|| EventStatusCodes.Definite == eventStatusCode )
					{
						Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", 
							jsCode + "alert('Successfully updated');",
							true);
					}

					LoadGridsFromRoomPatternRecordsOrInitEmptyGrid(_service, EventStatusCodes.Definite != eventStatusCode,
						true, (int)DeleteAllRoomBlockRecordsOnChangeStatus.False);
					//ytodo definite call not below
					RetrieveBasedOnStatus(_service, true, (int)DeleteAllRoomBlockRecordsOnChangeStatus.False);
					btnlead.Enabled = true;
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
		}

		private void UpdateRoomblockRecords_fromGrids(string _eventID, int Updating, 
			EventStatusCodes eventStatusCode = EventStatusCodes.NotDefinite)
        {
            try
            {
                if (GridView1.Rows.Count > 0)
					UpdateRoomblockRecords_fromGrid(_eventID, Updating, GridView1, eventStatusCode);
                else if (GridView2.Rows.Count > 0)
					UpdateRoomblockRecords_fromGrid(_eventID, Updating, GridView2, eventStatusCode);
			}
			catch (System.Web.Services.Protocols.SoapException ex)
            {
				logger.Error(ex.Detail.InnerText, ex);
			}
			catch (Exception ex)
            {
				logger.Info(String.Format("LastSuccessLog, UpdateRoomblock(: {0}", lastSuccessLogMsg));
				logger.Error("1251 UpdateRoomblock " + ex.ToString());
            }
		}

		#region Original UpdateStatus
		private void UpdateRoomblockDefiniteStatus(string _eventID, int Updatestatus)
		{
			try
			{
				if (GridView1.Rows.Count > 0)
				{
					UpdateSaveRowsToRoomPattern(GridView1);
					/* ytodo tidy up
					for (int i = 0; i < gvFormLoad.Rows.Count; i++)
					{
						// Create the RoomPattern object.
						New_roompattern roompattern = new New_roompattern();
						string RoomGUID = //gvFormLoad.Rows[i].Cells[7].Text; 
							dtretrieve.Rows[i]["RoomGUID"].ToString();
						roompattern.New_roompatternId = new Guid(RoomGUID);

						HiddenField txtcurrentpercent = (HiddenField)gvFormLoad.Rows[i].FindControl("hdnfdValue");
						string CPeak = txtcurrentpercent.Value;
						int intCPeak = 0;
						if ((((HiddenField)(gvFormLoad.Rows[i].FindControl("hdnfdValue"))).Value != string.Empty) 
							&& ((HiddenField)(gvFormLoad.Rows[i].FindControl("hdnfdValue"))) != null)
						{
							if (CPeak.Contains("."))
							{
								intCPeak = Convert.ToInt32(CPeak.Split('.')[0].ToString());
							}
							else
							{
								intCPeak = Convert.ToInt32(CPeak);
							}
						}
						roompattern.New_PercentofPeak = intCPeak;

						string CRoom = "0";
						int intCRoom = 0;
						if (gvFormLoad.Rows[i].Enabled == true)
						{
							TextBox txtcurrentroom = (TextBox)gvFormLoad.Rows[i].FindControl("txtCBlock");
							CRoom = txtcurrentroom.Text;
							if ((((TextBox)(gvFormLoad.Rows[i].FindControl("txtCBlock"))).Text != string.Empty) && ((TextBox)(gvFormLoad.Rows[i].FindControl("txtCBlock"))).Text != null)
							{
								if (CRoom.Contains("."))
								{
									intCRoom = Convert.ToInt32(CRoom.Split('.')[0].ToString());
								}
								else
								{
									intCRoom = Convert.ToInt32(CRoom);
								}
							}
							roompattern.New_RoomBlock = intCRoom;

							if (ActualsVisible(gvFormLoad))
							{
								roompattern.Bcmc_ActualBlock = ExtractGridRowCtrl_Int(gvFormLoad, i, GridCtrlNames.txtActualBlock.ToString());
								roompattern.Bcmc_ActualPercentOfPeak = ExtractGridRowCtrl_Int(gvFormLoad, i, GridCtrlNames.txtActualPercentOfPeak.ToString());
							}
						}
						else
						{
							//For Diasbled Records Current RoomBlock value set to zero  .
							intCRoom = 0;
							roompattern.New_RoomBlock = intCRoom;
						}

						int intdaynumber = i + 1;
						roompattern.New_DayNumber = intdaynumber;
						roompattern["bcmc_user"] = this.Username;

						//_service.Update(roompattern);
						Update(roompattern);
					}
					*/

					String jsCode = UpdateEvent_withCalc_TotalRoomblocksAndPeak(GridView1, _eventID);

					// Update Button Pressed, (as opposed to save)
					if (Updatestatus == 1)
					{
						string myscript = jsCode + "alert('Successfully updated ');";
						Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);
					}

					LoadGridsFromRoomPatternRecordsOrInitEmptyGrid(_service);
					btnlead.Enabled = true;
				}
				else if (GridView2.Rows.Count > 0)
				{
					UpdateSaveRowsToRoomPattern(GridView2);
					/* ytodo tidy up
					for (int i = 0; i < gvRoomBlock.Rows.Count; i++)
					{
						New_roompattern roompattern = new New_roompattern();
						string RoomGUID = gvRoomBlock.Rows[i].Cells[8].Text;
						logger.Info("UpdateRoomblockStatus Updating New_roompatternId=" + RoomGUID);
						roompattern.New_roompatternId = new Guid(RoomGUID);

						HiddenField txtcurrentpercent = (HiddenField)gvRoomBlock.Rows[i].FindControl("hdnfdValue");
						string CPeak = txtcurrentpercent.Value;

						int intCPeak = 0;
						if ((((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))).Value != string.Empty) && ((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))) != null)
						{
							if (CPeak.Contains("."))
							{
								intCPeak = Convert.ToInt32(CPeak.Split('.')[0].ToString());
							}
							else
							{
								intCPeak = Convert.ToInt32(CPeak);
							}
						}
						roompattern.New_PercentofPeak = intCPeak;

						TextBox txtcurrentroom = (TextBox)gvRoomBlock.Rows[i].FindControl("txtCBlock");
						string CRoom = txtcurrentroom.Text;
						int intCRoom = 0;
						if ((((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != string.Empty) 
							&& ((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != null)
						{
							if (CRoom.Contains("."))
							{
								intCRoom = Convert.ToInt32(CRoom.Split('.')[0].ToString());
							}
							else
							{
								intCRoom = Convert.ToInt32(CRoom);
							}
						}
						roompattern.New_RoomBlock = intCRoom;

						if (gvRoomBlock.Columns[(int)GridColumnsFormLoad.ActualBlock].Visible)
						{
							roompattern.Bcmc_ActualBlock = ExtractGridRowCtrl_Int(gvRoomBlock, i, GridCtrlNames.txtActualBlock.ToString());
							roompattern.Bcmc_ActualPercentOfPeak = ExtractGridRowCtrl_Int(gvRoomBlock, i, GridCtrlNames.txtActualPercentOfPeak.ToString());
						}

						roompattern["bcmc_user"] = this.Username;

						//_service.Update(roompattern);
						Update(roompattern);
                    }
					*/


					String jsCode = UpdateEvent_withCalc_TotalRoomblocksAndPeak(GridView2, _eventID);

					string myscript = jsCode + "alert('Successfully updated ');";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);

                    LoadGridsFromRoomPatternRecordsOrInitEmptyGrid(_service);
                    btnlead.Enabled = true;
                }
            }
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
            catch (Exception ex)
            {
				logger.Info(String.Format("UpdateRoomblockStatus(: {0}", lastSuccessLogMsg));
                logger.Error("UpdateRoomblockStatus" + ex.ToString());
            }
        }

#endregion //Original UpdateStatus

        private void UpdateStatusActiveAndUpdateUserForAllRoomPatternDays(OrganizationService _service, string getRecordid)
        {
            EntityCollection roomBlockDaysForEvent = GetRoomPatternWithAscendingDates(getRecordid, _service); //ytodo specify only needed fields

            for (int i = 0; i < roomBlockDaysForEvent.Entities.Count; i++)
            {
				try
				{
					Entity entity = roomBlockDaysForEvent.Entities[i];

					New_roompattern roompattern = new New_roompattern();
					roompattern.Id = entity.Id;
					roompattern.Bcmc_statustype = (int)New_roompatternState.Active; //0
					roompattern.bcmc_User = this.Username;
					Update(roompattern);
				}
				catch (Exception ex)
				{
					logger.Error(String.Format("UpdateStatusActiveAndUserForAllRoomPatternDays {0}", roomBlockDaysForEvent.Entities[i].Id), ex);
				}
            }
        }

        private EntityCollection GetRoomPatternWithAscendingDates(string eventGUID, OrganizationService _service)
        {
			EntityCollection _businessEntitiesRP = null;
			try
			{
				/* select * from new_roompattern
				 * where new_eventid = eventGUID
				 * order by bcmc_date asc
				 */
				ConditionExpression conditionRP = new ConditionExpression();
				conditionRP.AttributeName = "new_eventid";
				conditionRP.Operator = ConditionOperator.Equal;
				conditionRP.Values.Add(eventGUID.ToString());

				FilterExpression filterRP = new FilterExpression();
				filterRP.FilterOperator = LogicalOperator.And;
				filterRP.Conditions.Add(conditionRP);

				OrderExpression orderExpr = new OrderExpression();
				orderExpr.AttributeName = "bcmc_date";
				orderExpr.OrderType = OrderType.Ascending;

				QueryExpression queryRP = new QueryExpression();
				queryRP.EntityName = "new_roompattern";
				queryRP.ColumnSet = new ColumnSet(true);
				queryRP.Criteria = filterRP;
				queryRP.Orders.Add(orderExpr);

				_businessEntitiesRP = _service.RetrieveMultiple(queryRP);
			}
			catch (Exception ex)
			{
				logger.Error("GetRoomPatternWithAscendingDates", ex);
			}

            return _businessEntitiesRP;
        }

		/// <summary>
		/// Display an alert box when the page completes posting back.
		/// </summary>
		/// <param name="message"></param>
		protected void Alert(String message)
		{
			logger.InfoFormat("Enter Alert({0})", message);
			string myscript = String.Format("alert('{0}');", message);
			Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);
		}
//#endregion //UserdefinedMethods

		/*private void SetRoomPatternActualRoomsField_fromGrid(int row, New_roompattern roompattern, GridView gridview, string msg)
		{
			TextBox txtactualroom = (TextBox)gridview.Rows[row].FindControl("txtActualBlock");
			if (null != txtactualroom)
			{
				string ActualRooms = txtactualroom.Text;
				int intActualRooms = 0;
				if (null != txtactualroom
					&& txtactualroom.Text != string.Empty)
				{
					if (ActualRooms.Contains("."))
						intActualRooms = Convert.ToInt32(ActualRooms.Split('.')[0].ToString());
					else
						intActualRooms = Convert.ToInt32(ActualRooms);
				}
				roompattern.Bcmc_ActualBlock = intActualRooms;
				//roompattern["bcmc_actualblock"] = intActualRooms;
				lastSuccessLogMsg = String.Format(msg + "roompattern.Bcmc_ActualBlock={0}", intActualRooms);
			}
		}
		private void SetRoomPatternActualRoomsField_fromGrid(int row, Entity roompattern, GridView gridview, string msg)
		{
			TextBox txtactualroom = (TextBox)gridview.Rows[row].FindControl("txtActualBlock");
			if (null != txtactualroom)
			{
				string ActualRooms = txtactualroom.Text;
				int intActualRooms = 0;
				if (null != txtactualroom
					&& txtactualroom.Text != string.Empty)
				{
					if (ActualRooms.Contains("."))
						intActualRooms = Convert.ToInt32(ActualRooms.Split('.')[0].ToString());
					else
						intActualRooms = Convert.ToInt32(ActualRooms);
				}
				//roompattern.Bcmc_ActualBlock = intActualRooms;//ytododev
				//roompattern["bcmc_actualblock"] = intActualRooms;
				lastSuccessLogMsg = String.Format(msg + "roompattern.Bcmc_ActualBlock={0}", intActualRooms);
			}
		}*/

		string lastSuccessLogMsg = "";

		private int GetGridViewIntValue(string ctlId, GridView gv, int row)
		{
			ITextControl txtBox = (ITextControl)(gv.Rows[row].FindControl(ctlId));
			if (null != txtBox.Text && txtBox.Text != string.Empty)
			{
				if (txtBox.Text.Contains("."))
					return Convert.ToInt32(txtBox.Text.Split('.')[0].ToString());
				else
					return Convert.ToInt32(txtBox.Text);
			}
			return -1;
		}
		private void SetEntityAttribute_FromGridRow(Entity e, string attribute, string ctlId, GridView gv, int row)
		{
			int value = GetGridViewIntValue(ctlId, gv, row);
			if (-1 != value)
				e[attribute] = value;
		}
		
		/*int ExtractGridRowInt_LabelField(GridView gv, int row, string ctlName, int defaultInt = 0)
		{
			System.Web.UI.WebControls.Label ctl = (System.Web.UI.WebControls.Label)gv.Rows[row].FindControl(ctlName);
			System.Web.UI.ITextControl ctrl = (System.Web.UI.ITextControl)gv.Rows[row].FindControl(ctlName);
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
		}*/
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

			/*TextBox ctl = (TextBox)gv.Rows[row].FindControl(ctlName);
			int valueInt = defaultInt;
			if (null != ctl)
			{
				if (ctl.Text != "")
				{
					string value = ctl.Text;
					if (value.Contains("."))
						valueInt = Convert.ToInt32(value.Split('.')[0].ToString());
					else
						valueInt = Convert.ToInt32(value);
				}
			}
			//lastSuccessLogMsg = String.Format(msg + "roompattern.{0}={1}", ctlName, valueInt);
			return valueInt;*/
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


		bool RowDisabled(GridView gv, int row)
		{
			if ("gvFormLoad" == gv.ID && !gv.Rows[row].Enabled)
				return true;
			return false;
		}

		/*
		bool ColDisabled(GridView gv, int row)
		{
			if (!gv.Rows[row].Enabled)
				return true;
			return false;
		}
		*/ 


		protected void TogglePreAndPostWeek_CheckedChanged(object sender, EventArgs e)
		{
			if (CheckBox_TogglePreAndPostWeek.Checked)
			{
				//add pre/post week to roomblock.
				// add “Day of Week” and “Date” columns

				// don't worry about this...
				// CheckBox_TogglePreAndPostWeek.Style[] disabled
			}
		}

		// for actual and planned
		private void CalculatePercentageOfPeak(string roomblockFld, string percentageOfPeakField)
		{
		}

		//ytodo possibly merge the next 2 functions
		private void Calculate_Total_RoomNights()
		{
			if (CheckBox_TogglePreAndPostWeek.Checked)
			{
				//use actual values
			}
			else
			{
				//use planned values
			}
		}

		private void Calcuate_Total_PeakRoomNights()
		{
			if (CheckBox_TogglePreAndPostWeek.Checked)
			{
				//use actual values
			}
			else
			{
				//use planned values
			}
		}

		// ONLY FOR DEBUGGING
		protected void btnDbugUpdateDateRange_click(object sender, EventArgs e)
		{
			DateTime dtDbgArrive = Convert.ToDateTime(txtDbgArrivalDate.Text);
			DateTime dtDbgDepart = Convert.ToDateTime(txtDbgDepartureDate.Text);

			if (new EventActualRoomblocks(this, true).IsNewDateRange(dtDbgArrive, dtDbgDepart))
			{
				// If Range has been changed set it for Event
				Opportunity o = new Opportunity();
				o.Id = new Guid(EventId);
				o.New_arrivaldate = dtDbgArrive;
				o.New_departuredate = dtDbgDepart;
				Update(o);
			}

			// Reload grid
			LoadGridsFromRoomPatternRecordsOrInitEmptyGrid(_service, (GridView1.Rows.Count > 0 ? true: false));
		}
#endif
	}
}

