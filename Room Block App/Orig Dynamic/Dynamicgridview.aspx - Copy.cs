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

namespace RoomBlock
{
    public partial class Dynamicgridview : System.Web.UI.Page
    {
        #region "Variable Declaration"
		static OrganizationService _service = CrmServiceManager.GetCrmService();
        static DataTable dtretrieve = new DataTable();
        static DataTable dtRoomBlock = new DataTable();
        private static ILog logger = LogManager.GetLogger(typeof(Dynamicgridview));
        static string getEventStatus = string.Empty;
        decimal totalRoomBlock = 0M;
        string Totalrooms = string.Empty;
        string PeakRoomNight = string.Empty;
        //static string _eventID = string.Empty;
        #endregion

        #region Pageload

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bool rpStatusFlag = false;
                string getRecordid = string.Empty;
                try
                {
                    //LoggerClass loggerClass = new LoggerClass();
                    logger.Info("Dynamicgridview Page: Page_Load Method: Started");

                    if (HttpContext.Current.Request.QueryString["status"] == null)
                    {
                        if (HttpContext.Current.Request.QueryString["recordid"] != null)
                        {
                            getRecordid = Request.QueryString["recordid"].ToString();
                            ViewState["_eventID"] = getRecordid;
                            if (HttpContext.Current.Request.QueryString["rpStatus"] != null)
                            {
                                rpStatusFlag = true;
                                CheckEventUpdateStatus(_service, getRecordid);
                            }
                            else
                                rpStatusFlag = false;

                            getEventStatus = "";
                            getEventStatus = CheckEventStatus(_service, getRecordid);

                            logger.Info("Dynamicgridview Page: Page_Load Method: getEventStatus = " + getEventStatus);

                            if (getEventStatus != string.Empty && getEventStatus == "10")
                            {
                                RetrieveBasedonStatus(_service);
                                btnsave.Text = "Update";
                                btnlead.Enabled = true;
                            }
                            else
                                Retrieve(_service, rpStatusFlag,0);

                            logger.Info("Dynamicgridview Page: Page_Load Method: End");
                        }
                        else
                        {
                            btnsave.Visible = false;
                            btnlead.Visible = false;
                        }
                    }
                    else
                    {
                        Retrieve(_service, true,1);
                        btnsave.Text = "Save";
                        btnlead.Enabled = false;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Dynamicgridview Page: Page_Load Method: Error" + ex.ToString());
                }
                finally
                {

                    logger.Info("Dynamicgridview Page:  PageLoad Method: Before service execution");
                    string _eventID = Convert.ToString(ViewState["_eventID"]);
                    if (_eventID != null && _eventID != string.Empty)
                    {
                        EntityCollection entityCollection = CheckRoomPatternecords(_eventID, _service);

                        logger.Info("Dynamicgridview Page:  PageLoad Method: After service execution");
                        logger.Info("Dynamicgridview Page: entityCollection.BusinessEntities.Length:: " + entityCollection.BusinessEntities.Length);

                        if (entityCollection.BusinessEntities.Length > 0 && ViewState["DisplayDateisNULL"] == null)
                        {
                            btnsave.Text = "Update";
                            btnlead.Enabled = true;
                        }
                        else if (ViewState["DisplayDateisNULL"] != null)
                        {
                            btnsave.Enabled = false;
                            btnlead.Enabled = false;
                        }
                        else
                        {
                            btnsave.Text = "Save";
                            btnlead.Enabled = false;
                            UpdatePeakRoomNights(_eventID);
                        }
                    }
                }
            }
        }
        #endregion

        #region Events
        private void UpdatePeakRoomNights(string _eventID)
        {
            try
            {
                logger.Info("Dynamicgridview Page: UpdatePeakRoomNights : Started");
                logger.Info("Dynamicgridview Page:  UpdatePeakRoomNights Method: Before service 

                Entity entity = new Entity("new_roompattern");
execution");
                EntityCollection entityCollection = CheckRoomPatternecords(_eventID, _service);

                logger.Info("Dynamicgridview Page:  UpdatePeakRoomNights Method: After service execution");
                logger.Info("Dynamicgridview Page: entityCollection.BusinessEntities.Length:: " + entityCollection.Entities.Count);

                if (entityCollection.Entities.Count == 0)
                {
                    Entity oppEvent = new Entity("opportunity");
                    oppEvent["new_hotelroomnights"] = 0;
                    oppEvent["new_peakhotelroomnights"] = 0;
                    oppEvent["opportunityid"] = new Guid(_eventID);
                    _service.Update(oppEvent);
                }
                logger.Info("Dynamicgridview Page: UpdatePeakRoomNights : Ended");
            }
            catch (Exception ex)
            {
                logger.Error("Dynamicgridview Page: UpdatePeakRoomNights : Error" + ex.ToString());
            }
        }

        protected void btnlead_Click(object sender, EventArgs e)
        {
			if (!ValidateProposedDates())
				return;
            SendLeadtoHotel();
        }

        private void SendLeadtoHotel()
        {
            try
            {
                string _eventID = Convert.ToString(ViewState["_eventID"]);

                logger.Info("Dynamicgridview Page:  SendLeadtoHotel Method: Started");

                if (gvFormLoad.Rows.Count > 0)
                {
                    if (getEventStatus != "10")
                        UpdateRoomblock(_eventID,0);
                    else
                        UpdateRoomblockStatus(_eventID,0);

                    foreach (GridViewRow Item in gvFormLoad.Rows)
                    {
                        TextBox txtcurrent = (TextBox)Item.FindControl("txtCBlock");
                        string currentroom = txtcurrent.Text;

						System.Web.UI.WebControls.
                        Label txtoriginal = (System.Web.UI.WebControls.Label)Item.FindControl("lblOblock");
                        txtoriginal.Text = currentroom;

                        HiddenField txt1 = (HiddenField)Item.FindControl("hdnfdValue");
                        string current = txt1.Value;
						System.Web.UI.WebControls.
                        Label txt2 = (System.Web.UI.WebControls.Label)Item.FindControl("lblOPeak");
                        txt2.Text = current;
                    }

                    for (int i = 0; i < gvFormLoad.Rows.Count; i++)
                    {
                        // Create the contact object.
                        Entity roompattern = new Entity("new_roompattern");

                        // Set the contact object properties to be updated.
						System.Web.UI.WebControls.
                        Label txtOriginalpeak = (System.Web.UI.WebControls.Label)gvFormLoad.Rows[i].FindControl("lblOPeak");
                        string OriginalPeak = txtOriginalpeak.Text;
                        int intOPeak = 0;

                        if ((((System.Web.UI.WebControls.Label)
								(gvFormLoad.Rows[i].FindControl("lblOPeak"))).Text != string.Empty) 
							&& ((System.Web.UI.WebControls.Label)
								(gvFormLoad.Rows[i].FindControl("lblOPeak"))).Text != null)
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

                        HiddenField txtCurrentpeak = (HiddenField)gvFormLoad.Rows[i].FindControl("hdnfdValue");
                        string CurrentPeak = txtCurrentpeak.Value;
                        int intCPeak = 0;

                        if ((((HiddenField)(gvFormLoad.Rows[i].FindControl("hdnfdValue"))).Value != string.Empty) 
							&& ((HiddenField)(gvFormLoad.Rows[i].FindControl("hdnfdValue"))) != null)
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

                        logger.Info("------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method------------------------------------------------");
                        logger.Info("roompattern.bcmc_originalpercentofpeak  = " + 
							(roompattern["bcmc_originalpercentofpeak"] as int).ToString() + 
							"- roompattern.new_percentofpeak " + 
							(roompattern["new_percentofpeak"] as int).ToString());
                        logger.Info("-----------------------------------Dynamicgridview Page:  SendLeadtoHotel Method-------------------------------------------------------");

						System.Web.UI.WebControls.
                        Label txtOriginalblock = (System.Web.UI.WebControls.Label)
							gvFormLoad.Rows[i].FindControl("lblOblock");
                        string Originalblock = txtOriginalblock.Text;
                        int intOBlock = 0;

                        if ((((System.Web.UI.WebControls.Label)
								(gvFormLoad.Rows[i].FindControl("lblOblock"))).Text != string.Empty) 
							&& ((System.Web.UI.WebControls.Label)
								(gvFormLoad.Rows[i].FindControl("lblOblock"))).Text != null)
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
                        {
                            intOBlock = 0;
                        }
                        roompattern["bcmc_originalroomblock"] = intOBlock;

                        TextBox txtcurrentblock = (TextBox)gvFormLoad.Rows[i].FindControl("txtCBlock");
                        string Currentblock = txtcurrentblock.Text;
                        CrmNumber intCBlock = new CrmNumber();
                        if ((((TextBox)(gvFormLoad.Rows[i].FindControl("txtCBlock"))).Text != string.Empty) && ((TextBox)(gvFormLoad.Rows[i].FindControl("txtCBlock"))).Text != null)
                        {
                            if (Currentblock.Contains("."))
                            {
                                intCBlock.Value = Convert.ToInt32(Currentblock.Split('.')[0].ToString());
                            }
                            else
                            {
                                intCBlock.Value = Convert.ToInt32(Currentblock);
                            }
                        }
                        else
                        {
                            intCBlock.Value = 0;
                        }
                        roompattern.new_roomblock = intCBlock;

                        logger.Info("-----------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method----------------------------------------------");
                        logger.Info("roompattern.bcmc_originalroomblock   = " + roompattern.bcmc_originalroomblock.Value + "-  roompattern.new_roomblock " + roompattern.new_roomblock.Value);
                        logger.Info("-----------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method---------------------------------------------------");

                        // The contactid is a key that references the ID of the contact to be updated.
                        roompattern.new_roompatternid = new RoomBlock.CrmService.Key();
                        // The contactid.Value is the GUID of the record to be changed.

                        roompattern.new_roompatternid.Value = new Guid(dtretrieve.Rows[i]["RoomGUID"].ToString());

                        // Update the contact.
                        _service.Update(roompattern);

                        logger.Info("Dynamicgridview Page:  SendLeadtoHotel Method:Completed Successfully");
                    }

                    string myscript = "alert('Successfully completed.');";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);
                    btnlead.Enabled = true;

                }
                else if (gvRoomBlock.Rows.Count > 0)
                {

                    if (getEventStatus != "10")
                        UpdateRoomblock(_eventID,0);
                    else
                        UpdateRoomblockStatus(_eventID,0);

                    foreach (GridViewRow Item in gvRoomBlock.Rows)
                    {
                        TextBox txtcurrent = (TextBox)Item.FindControl("txtCBlock");
                        string currentroom = txtcurrent.Text;

                        Label txtoriginal = (Label)Item.FindControl("lblOblock");
                        txtoriginal.Text = currentroom;

                        HiddenField txt1 = (HiddenField)Item.FindControl("hdnfdValue");
                        string current = txt1.Value;
                        Label txt2 = (Label)Item.FindControl("lblOPeak");
                        txt2.Text = current;
                    }


                    for (int i = 0; i < gvRoomBlock.Rows.Count; i++)
                    {
                        // Create the contact object.
                        new_roompattern roompattern = new new_roompattern();

                        Label txtOriginalpeak = (Label)gvRoomBlock.Rows[i].FindControl("lblOPeak");
                        string OriginalPeak = txtOriginalpeak.Text;

                        CrmNumber intOPeak = new CrmNumber();
                        intOPeak.Value = Convert.ToInt32(OriginalPeak);

                        if ((((Label)(gvRoomBlock.Rows[i].FindControl("lblOPeak"))).Text != string.Empty) && ((Label)(gvRoomBlock.Rows[i].FindControl("lblOPeak"))).Text != null)
                        {
                            if (OriginalPeak.Contains("."))
                            {
                                intOPeak.Value = Convert.ToInt32(OriginalPeak.Split('.')[0].ToString());
                            }
                            else
                            {
                                intOPeak.Value = Convert.ToInt32(OriginalPeak);
                            }
                        }
                        else
                        {
                            intOPeak.Value = 0;
                        }
                        roompattern.bcmc_originalpercentofpeak = intOPeak;


                        HiddenField txtCurrentpeak = (HiddenField)gvRoomBlock.Rows[i].FindControl("hdnfdValue");
                        string CurrentPeak = txtCurrentpeak.Value;
                        CrmNumber intCPeak = new CrmNumber();
                        if ((((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))).Value != string.Empty) && ((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))) != null)
                        {
                            if (CurrentPeak.Contains("."))
                            {
                                intCPeak.Value = Convert.ToInt32(CurrentPeak.Split('.')[0].ToString());
                            }
                            else
                            {
                                intCPeak.Value = Convert.ToInt32(CurrentPeak);
                            }
                        }
                        else
                        {
                            intCPeak.Value = 0;
                        }
                        roompattern.new_percentofpeak = intCPeak;

                        logger.Info("---------------------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method----------------------------------------------------------");
                        logger.Info("roompattern.bcmc_originalpercentofpeak     = " + roompattern.bcmc_originalpercentofpeak.Value + "-    roompattern.new_percentofpeak  " + roompattern.new_percentofpeak.Value);
                        logger.Info("---------------------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method----------------------------------------");

                        Label txtOriginalblock = (Label)gvRoomBlock.Rows[i].FindControl("lblOblock");
                        string Originalblock = txtOriginalblock.Text;

                        CrmNumber intOBlock = new CrmNumber();

                        if ((((Label)(gvRoomBlock.Rows[i].FindControl("lblOblock"))).Text != string.Empty) && ((Label)(gvRoomBlock.Rows[i].FindControl("lblOblock"))).Text != null)
                        {
                            if (Originalblock.Contains("."))
                            {
                                intOBlock.Value = Convert.ToInt32(Originalblock.Split('.')[0].ToString());
                            }
                            else
                            {
                                intOBlock.Value = Convert.ToInt32(Originalblock);
                            }
                        }
                        else
                        {
                            intOBlock.Value = 0;
                        }
                        roompattern.bcmc_originalroomblock = intOBlock;

                        TextBox txtcurrentblock = (TextBox)gvRoomBlock.Rows[i].FindControl("txtCBlock");
                        string Currentblock = txtcurrentblock.Text;
                        CrmNumber intCBlock = new CrmNumber();
                        if ((((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != string.Empty) && ((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != null)
                        {
                            if (Currentblock.Contains("."))
                            {
                                intCBlock.Value = Convert.ToInt32(Currentblock.Split('.')[0].ToString());
                            }
                            else
                            {
                                intCBlock.Value = Convert.ToInt32(Currentblock);
                            }
                        }
                        else
                        {
                            intCBlock.Value = 0;
                        }
                        roompattern.new_roomblock = intCBlock;

                        logger.Info("--------------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method------------------------------------------------");
                        logger.Info("roompattern.bcmc_originalroomblock    = " + roompattern.bcmc_originalroomblock.Value + "-   roompattern.new_roomblock  " + roompattern.new_roomblock.Value);
                        logger.Info("---------------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method-------------------------------------------------");



                        // The contactid is a key that references the ID of the contact to be updated.
                        roompattern.new_roompatternid = new RoomBlock.CrmService.Key();
                        // The contactid.Value is the GUID of the record to be changed.

                        roompattern.new_roompatternid.Value = new Guid(dtretrieve.Rows[i]["RoomGUID"].ToString());

                        // Update the contact.
                        logger.Info("Dynamicgridview Page:  SendLeadtoHotel Method: Updating the record roompattern.new_roompatternid.Value " + gvRoomBlock.Rows[i].Cells[8].Text);
                        // Update the contact.
                        _service.Update(roompattern);

                        logger.Info("Dynamicgridview Page:  SendLeadtoHotel Method: Completed Successfully");
                    }
                    string myscript = "alert('Successfully completed. ');";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);
                    btnlead.Enabled = false;

                }

                logger.Info("Dynamicgridview Page: SendLeadtoHotel : End");
            }
            catch (Exception ex)
            {
                logger.Error("Dynamicgridview Page: SendLeadtoHotel Method: Error" + ex.ToString());
            }

        }
        //private void SendLeadtoHotel()
        //{
        //    try
        //    {
        //        logger.Info("Dynamicgridview Page:  SendLeadtoHotel Method: Started");
        //        if (gvFormLoad.Rows.Count > 0)
        //        {
        //            foreach (GridViewRow Item in gvFormLoad.Rows)
        //            {
        //                TextBox txtcurrent = (TextBox)Item.FindControl("txtCBlock");
        //                string currentroom = txtcurrent.Text;

        //                Label txtoriginal = (Label)Item.FindControl("lblOblock");
        //                txtoriginal.Text = currentroom;

        //                HiddenField txt1 = (HiddenField)Item.FindControl("hdnfdValue");
        //                string current = txt1.Value;
        //                Label txt2 = (Label)Item.FindControl("lblOPeak");
        //                txt2.Text = current;
        //            }

        //            for (int i = 0; i < gvFormLoad.Rows.Count; i++)
        //            {
        //                // Create the contact object.
        //                new_roompattern roompattern = new new_roompattern();

        //                // Set the contact object properties to be updated.
        //                Label txtOriginalpeak = (Label)gvFormLoad.Rows[i].FindControl("lblOPeak");
        //                string OriginalPeak = txtOriginalpeak.Text;
        //                CrmNumber intOPeak = new CrmNumber();

        //                if ((((Label)(gvFormLoad.Rows[i].FindControl("lblOPeak"))).Text != string.Empty) && ((Label)(gvFormLoad.Rows[i].FindControl("lblOPeak"))).Text != null)
        //                {
        //                    if (OriginalPeak.Contains("."))
        //                    {
        //                        intOPeak.Value = Convert.ToInt32(OriginalPeak.Split('.')[0].ToString());
        //                    }
        //                    else
        //                    {
        //                        intOPeak.Value = Convert.ToInt32(OriginalPeak);
        //                    }

        //                }
        //                else
        //                {
        //                    intOPeak.Value = 0;
        //                }
        //                roompattern.bcmc_originalpercentofpeak = intOPeak;

        //                HiddenField txtCurrentpeak = (HiddenField)gvFormLoad.Rows[i].FindControl("hdnfdValue");
        //                string CurrentPeak = txtCurrentpeak.Value;
        //                CrmNumber intCPeak = new CrmNumber();

        //                if ((((HiddenField)(gvFormLoad.Rows[i].FindControl("hdnfdValue"))).Value != string.Empty) && ((HiddenField)(gvFormLoad.Rows[i].FindControl("hdnfdValue"))) != null)
        //                {
        //                    if (CurrentPeak.Contains("."))
        //                    {
        //                        intCPeak.Value = Convert.ToInt32(CurrentPeak.Split('.')[0].ToString());
        //                    }
        //                    else
        //                    {
        //                        intCPeak.Value = Convert.ToInt32(CurrentPeak);
        //                    }
        //                }
        //                else
        //                {
        //                    intCPeak.Value = 0;
        //                }

        //                roompattern.new_percentofpeak = intCPeak;

        //                logger.Info("------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method------------------------------------------------");
        //                logger.Info("roompattern.bcmc_originalpercentofpeak  = " + roompattern.bcmc_originalpercentofpeak.Value + "- roompattern.new_percentofpeak " + roompattern.new_percentofpeak.Value);
        //                logger.Info("-----------------------------------Dynamicgridview Page:  SendLeadtoHotel Method-------------------------------------------------------");


        //                Label txtOriginalblock = (Label)gvFormLoad.Rows[i].FindControl("lblOblock");
        //                string Originalblock = txtOriginalblock.Text;
        //                CrmNumber intOBlock = new CrmNumber();

        //                if ((((Label)(gvFormLoad.Rows[i].FindControl("lblOblock"))).Text != string.Empty) && ((Label)(gvFormLoad.Rows[i].FindControl("lblOblock"))).Text != null)
        //                {
        //                    if (Originalblock.Contains("."))
        //                    {
        //                        intOBlock.Value = Convert.ToInt32(Originalblock.Split('.')[0].ToString());
        //                    }
        //                    else
        //                    {
        //                        intOBlock.Value = Convert.ToInt32(Originalblock);
        //                    }
        //                }
        //                else
        //                {
        //                    intOBlock.Value = 0;
        //                }
        //                roompattern.bcmc_originalroomblock = intOBlock;

        //                TextBox txtcurrentblock = (TextBox)gvFormLoad.Rows[i].FindControl("txtCBlock");
        //                string Currentblock = txtcurrentblock.Text;
        //                CrmNumber intCBlock = new CrmNumber();
        //                if ((((TextBox)(gvFormLoad.Rows[i].FindControl("txtCBlock"))).Text != string.Empty) && ((TextBox)(gvFormLoad.Rows[i].FindControl("txtCBlock"))).Text != null)
        //                {
        //                    if (Currentblock.Contains("."))
        //                    {
        //                        intCBlock.Value = Convert.ToInt32(Currentblock.Split('.')[0].ToString());
        //                    }
        //                    else
        //                    {
        //                        intCBlock.Value = Convert.ToInt32(Currentblock);
        //                    }
        //                }
        //                else
        //                {
        //                    intCBlock.Value = 0;
        //                }
        //                roompattern.new_roomblock = intCBlock;

        //                logger.Info("-----------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method----------------------------------------------");
        //                logger.Info("roompattern.bcmc_originalroomblock   = " + roompattern.bcmc_originalroomblock.Value + "-  roompattern.new_roomblock " + roompattern.new_roomblock.Value);
        //                logger.Info("-----------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method---------------------------------------------------");

        //                // The contactid is a key that references the ID of the contact to be updated.
        //                roompattern.new_roompatternid = new RoomBlock.CrmService.Key();
        //                // The contactid.Value is the GUID of the record to be changed.

        //                roompattern.new_roompatternid.Value = new Guid(dtretrieve.Rows[i]["RoomGUID"].ToString());

        //                // Update the contact.
        //                _service.Update(roompattern);

        //                logger.Info("Dynamicgridview Page:  SendLeadtoHotel Method:Completed Successfully");
        //            }

        //            string myscript = "alert('Successfully completed.');";
        //            Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);
        //            btnlead.Enabled = true ;

        //        }
        //        else if (gvRoomBlock.Rows.Count > 0)
        //        {

        //            foreach (GridViewRow Item in gvRoomBlock.Rows)
        //            {
        //                TextBox txtcurrent = (TextBox)Item.FindControl("txtCBlock");
        //                string currentroom = txtcurrent.Text;

        //                Label txtoriginal = (Label)Item.FindControl("lblOblock");
        //                txtoriginal.Text = currentroom;

        //                HiddenField txt1 = (HiddenField)Item.FindControl("hdnfdValue");
        //                string current = txt1.Value;
        //                Label txt2 = (Label)Item.FindControl("lblOPeak");
        //                txt2.Text = current;
        //            }


        //            for (int i = 0; i < gvRoomBlock.Rows.Count; i++)
        //            {
        //                // Create the contact object.
        //                new_roompattern roompattern = new new_roompattern();

        //                Label txtOriginalpeak = (Label)gvRoomBlock.Rows[i].FindControl("lblOPeak");
        //                string OriginalPeak = txtOriginalpeak.Text;

        //                CrmNumber intOPeak = new CrmNumber();
        //                intOPeak.Value = Convert.ToInt32(OriginalPeak);

        //                if ((((Label)(gvRoomBlock.Rows[i].FindControl("lblOPeak"))).Text != string.Empty) && ((Label)(gvRoomBlock.Rows[i].FindControl("lblOPeak"))).Text != null)
        //                {
        //                    if (OriginalPeak.Contains("."))
        //                    {
        //                        intOPeak.Value = Convert.ToInt32(OriginalPeak.Split('.')[0].ToString());
        //                    }
        //                    else
        //                    {
        //                        intOPeak.Value = Convert.ToInt32(OriginalPeak);
        //                    }
        //                }
        //                else
        //                {
        //                    intOPeak.Value = 0;
        //                }
        //                roompattern.bcmc_originalpercentofpeak = intOPeak;


        //                HiddenField txtCurrentpeak = (HiddenField)gvRoomBlock.Rows[i].FindControl("hdnfdValue");
        //                string CurrentPeak = txtCurrentpeak.Value;
        //                CrmNumber intCPeak = new CrmNumber();
        //                if ((((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))).Value != string.Empty) && ((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))) != null)
        //                {
        //                    if (CurrentPeak.Contains("."))
        //                    {
        //                        intCPeak.Value = Convert.ToInt32(CurrentPeak.Split('.')[0].ToString());
        //                    }
        //                    else
        //                    {
        //                        intCPeak.Value = Convert.ToInt32(CurrentPeak);
        //                    }
        //                }
        //                else
        //                {
        //                    intCPeak.Value = 0;
        //                }
        //                roompattern.new_percentofpeak = intCPeak;

        //                logger.Info("---------------------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method----------------------------------------------------------");
        //                logger.Info("roompattern.bcmc_originalpercentofpeak     = " + roompattern.bcmc_originalpercentofpeak.Value + "-    roompattern.new_percentofpeak  " + roompattern.new_percentofpeak.Value);
        //                logger.Info("---------------------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method----------------------------------------");

        //                Label txtOriginalblock = (Label)gvRoomBlock.Rows[i].FindControl("lblOblock");
        //                string Originalblock = txtOriginalblock.Text;

        //                CrmNumber intOBlock = new CrmNumber();

        //                if ((((Label)(gvRoomBlock.Rows[i].FindControl("lblOblock"))).Text != string.Empty) && ((Label)(gvRoomBlock.Rows[i].FindControl("lblOblock"))).Text != null)
        //                {
        //                    if (Originalblock.Contains("."))
        //                    {
        //                        intOBlock.Value = Convert.ToInt32(Originalblock.Split('.')[0].ToString());
        //                    }
        //                    else
        //                    {
        //                        intOBlock.Value = Convert.ToInt32(Originalblock);
        //                    }
        //                }
        //                else
        //                {
        //                    intOBlock.Value = 0;
        //                }
        //                roompattern.bcmc_originalroomblock = intOBlock;

        //                TextBox txtcurrentblock = (TextBox)gvRoomBlock.Rows[i].FindControl("txtCBlock");
        //                string Currentblock = txtcurrentblock.Text;
        //                CrmNumber intCBlock = new CrmNumber();
        //                if ((((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != string.Empty) && ((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != null)
        //                {
        //                    if (Currentblock.Contains("."))
        //                    {
        //                        intCBlock.Value = Convert.ToInt32(Currentblock.Split('.')[0].ToString());
        //                    }
        //                    else
        //                    {
        //                        intCBlock.Value = Convert.ToInt32(Currentblock);
        //                    }
        //                }
        //                else
        //                {
        //                    intCBlock.Value = 0;
        //                }
        //                roompattern.new_roomblock = intCBlock;

        //                logger.Info("--------------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method------------------------------------------------");
        //                logger.Info("roompattern.bcmc_originalroomblock    = " + roompattern.bcmc_originalroomblock.Value + "-   roompattern.new_roomblock  " + roompattern.new_roomblock.Value);
        //                logger.Info("---------------------------------------------Dynamicgridview Page:  SendLeadtoHotel Method-------------------------------------------------");



        //                // The contactid is a key that references the ID of the contact to be updated.
        //                roompattern.new_roompatternid = new RoomBlock.CrmService.Key();
        //                // The contactid.Value is the GUID of the record to be changed.

        //                roompattern.new_roompatternid.Value = new Guid(dtretrieve.Rows[i]["RoomGUID"].ToString());

        //                // Update the contact.
        //                logger.Info("Dynamicgridview Page:  SendLeadtoHotel Method: Updating the record roompattern.new_roompatternid.Value " + gvRoomBlock.Rows[i].Cells[8].Text);
        //                // Update the contact.
        //                _service.Update(roompattern);

        //                logger.Info("Dynamicgridview Page:  SendLeadtoHotel Method: Completed Successfully");
        //            }
        //            string myscript = "alert('Successfully completed. ');";
        //            Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);
        //            btnlead.Enabled = false;

        //        }

        //        logger.Info("Dynamicgridview Page: SendLeadtoHotel : End");
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Dynamicgridview Page: SendLeadtoHotel Method: Error" + ex.ToString());
        //    }

        //}

        protected void btnsave_Click(object sender, EventArgs e)
        {
            try
            {
                logger.Info("Dynamicgridview Page: SaveClick : Started");

				if (!ValidateProposedDates())
					return;

                string _eventID = Convert.ToString(ViewState["_eventID"]);

                logger.Info("Dynamicgridview Page: Save Method : _eventID:" + _eventID);
                if (btnsave.Text == "Save")
                {
                    BindSave(_eventID);
                }
                else if (btnsave.Text == "Update")
                {
                    //if (getEventStatus == "1")
                    //{
                    //    btnsave.Text = "Save";
                    //    DeleteRoomblock(_eventID);
                    //    BindSave(_eventID);

                    //}
                    //else
                    //{
                    //    UpdateRoomblock(_eventID, 1);

                    //}

                    if (getEventStatus != "10")
                        UpdateRoomblock(_eventID, 1);
                    else
                        UpdateRoomblockStatus(_eventID, 1);
                }
                logger.Info("Dynamicgridview Page: SaveClick : End");
            }
            catch (Exception ex)
            {
                logger.Error("Dynamicgridview Page: SaveClick : Error" + ex.ToString());
            }

        }

        protected void gvFormLoad_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            foreach (TableCell cell in e.Row.Cells)
            {
                if (e.Row.RowIndex >= 0)
                {
                    cell.Attributes["Style"] = "border-color:Gray;padding-left:8px;";
                }
            }

        }

        #endregion

        #region UserdefinedMethods

        private RoomBlock.CrmService.CrmService getCrmService()
        {
            CrmService.CrmService service = new CrmService.CrmService();
            try
            {
                logger.Info("Dynamicgridview Page:  getSeviceProxy Method: Started");
                // Set up the CRM Service.
                RoomBlock.CrmService.CrmAuthenticationToken token = new RoomBlock.CrmService.CrmAuthenticationToken();
                token.AuthenticationType = 0;
                token.OrganizationName = ConfigurationManager.AppSettings["OrganizationName"].ToString();

                service.Url = ConfigurationManager.AppSettings["CRMService"].ToString();
                service.CrmAuthenticationTokenValue = token;
                service.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["UserID"].ToString(), ConfigurationManager.AppSettings["Password"].ToString(), ConfigurationManager.AppSettings["Domain"].ToString());
                logger.Info("Dynamicgridview Page:  getSeviceProxy Method: CrmService  " + service);


            }
            catch (Exception ex)
            {
                logger.Error("Dynamicgridview Page: getSeviceProxy: Error" + ex.ToString());
            }

            return service;

        }

        private CrmDateTime ConvertToCRMDateTime(DateTime dateTime)
        {
            logger.Info("Dynamicgridview Page:  ConvertToCRMDateTime  Method: Started");
            CrmDateTime crmDateTime = new CrmDateTime();
            crmDateTime.date = dateTime.ToShortDateString();
            crmDateTime.time = dateTime.ToShortTimeString();
            TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
            string sOffset = string.Empty;
            if (offset.Hours < 0)
            {
                sOffset = "-" + (offset.Hours * -1).ToString().PadLeft(2, '0');
            }
            else
            {
                sOffset = "+" + offset.Hours.ToString().PadLeft(2, '0');
            }
            sOffset += offset.Minutes.ToString().PadLeft(2, '0');
            crmDateTime.Value = dateTime.ToString(string.Format("yyyy-MM-ddTHH:mm:ss{0}", sOffset));
            logger.Info("Dynamicgridview Page:ConvertToCRMDateTime crmDateTime: " + crmDateTime.Value);

            logger.Info("Dynamicgridview Page:  ConvertToCRMDateTime  Method: Ended");
            return crmDateTime;
        }

        private void Retrieve(CrmService.CrmService _service, bool rpFlag,int onChangeStatus)
        {
            try
            {
                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: Started");
                if (HttpContext.Current.Request.QueryString["recordid"] != null && HttpContext.Current.Request.QueryString["Arrivaldate"] != null && HttpContext.Current.Request.QueryString["Departuredate"] != null)
                {

                    string getRecordid = Request.QueryString["recordid"].ToString();
                    string _arrivaldate = Convert.ToDateTime(Request.QueryString["Arrivaldate"].ToString()).ToString("MM/dd/yyyy");
                    string __departuredate = Convert.ToDateTime(Request.QueryString["Departuredate"].ToString()).ToString("MM/dd/yyyy");
                    CrmDateTime _arrivaldatedt = ConvertToCRMDateTime(Convert.ToDateTime(_arrivaldate));
                    CrmDateTime _departuredatedt = ConvertToCRMDateTime(Convert.ToDateTime(__departuredate));
                    string _eventID = getRecordid.Replace("{", "").Replace("}", "");

                    ViewState["_eventID"] = _eventID;
                    try
                    {
                        logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: Started");
                        // Define the entity attributes (database table columns) that are to be retrieved.

                        new_roompattern entity = new new_roompattern();

                        BusinessEntityCollection entityCollection = CheckRoomPatternecords(_eventID, _service);
                        logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: After service execution");
                        Hashtable hshTableRoomPattern = new Hashtable();
                        if (entityCollection.BusinessEntities.Length > 0)
                        {
                            logger.Info("Dynamicgridview Page: entityCollection.BusinessEntities.Length:: " + entityCollection.BusinessEntities.Length);

                            for (int j = 0; j < entityCollection.BusinessEntities.Length; j++)
                            {
                                entity = (CrmService.new_roompattern)entityCollection.BusinessEntities[j];
                                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: Retrieve Rooom Pattern Entity Value " + entity);


                                //Added by ZSL Team on 28-06-2012
                                if (entity.bcmc_date != null)
                                    hshTableRoomPattern.Add("bcmc_date" + j, entity.bcmc_date.Value);
                                else
                                    hshTableRoomPattern.Add("bcmc_date" + j, "01/01/0001");

                            }
                            DataRow drretrieve = dtretrieve.NewRow();

                            //Add By ZSL Team 28/05/2012
                            if (hshTableRoomPattern.Count > 0)
                            {
                                string tempArrivalDateRoomPattern = hshTableRoomPattern["bcmc_date0"].ToString();
                              
                                
                                if (tempArrivalDateRoomPattern != "01/01/0001")
                                {
                                    logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: Retrieve Rooom Pattern ArrivalDate " + hshTableRoomPattern["bcmc_date0"].ToString());
                                    string tempDepartureDateRoomPattern = hshTableRoomPattern["bcmc_date" + (hshTableRoomPattern.Count - 1).ToString()].ToString();
                                    logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: Retrieve Rooom Pattern ArrivalDate " + hshTableRoomPattern["bcmc_date" + (hshTableRoomPattern.Count - 1).ToString()].ToString());

                                    #region Bind Grid
                                    string departutedate = __departuredate;
                                    DateTime dtdeparture = Convert.ToDateTime(departutedate);

                                    dtdeparture = dtdeparture.AddDays(-1);
                                    string _Departuredate = Convert.ToString(dtdeparture.ToShortDateString());

                                    DateTime dtTempArrTime = Convert.ToDateTime(tempArrivalDateRoomPattern);
                                    DateTime dtArrivalDate = Convert.ToDateTime(_arrivaldate);


                                    DateTime dtTempDepTime = Convert.ToDateTime(tempDepartureDateRoomPattern);
                                    DateTime dtDeparturedate = Convert.ToDateTime(_Departuredate);

                                    logger.Info("Arrival Date Check dtArrivalDate: + " + dtArrivalDate.ToString());
                                    logger.Info("Arrival Date Check dtTempArrTime: + " + dtTempArrTime.ToString());

                                    int getArrResult = dtTempArrTime.CompareTo(dtArrivalDate);

                                    int getDepResult = dtTempDepTime.CompareTo(dtDeparturedate);

                                    if (getArrResult == 0)
                                    {
                                        if (getDepResult == 0)
                                        {
                                            dtretrieve = new DataTable();
                                            dtretrieve.Columns.Add("DayNumber");
                                            dtretrieve.Columns.Add("DayofWeek");
                                            dtretrieve.Columns.Add("Date");
                                            dtretrieve.Columns.Add("Original % of Peak");
                                            dtretrieve.Columns.Add("Original Block");
                                            dtretrieve.Columns.Add("Current % of Peak");
                                            dtretrieve.Columns.Add("Current Block");
                                            dtretrieve.Columns.Add("RoomGUID");
                                            for (int i = 0; i < entityCollection.BusinessEntities.Length; i++)
                                            {
                                                logger.Info("Dynamicgridview Page: entityCollection.BusinessEntities.Length:: " + entityCollection.BusinessEntities.Length);
                                                DataRow drretrieves = dtretrieve.NewRow();

                                                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: Retrieve Rooom Pattern Entity Name " + entity);
                                                entity = (new_roompattern)entityCollection.BusinessEntities[i];

                                                drretrieves["RoomGUID"] = entity.new_roompatternid.Value.ToString();
                                                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: Retrieve Rooom Pattern Entity - entity new_roompatternid Value.ToString() " + entity.new_roompatternid.Value.ToString());

                                                if (entity.new_daynumber != null)
                                                    if (!entity.new_daynumber.IsNull)
                                                    {
                                                        drretrieves["DayNumber"] = entity.new_daynumber.Value;
                                                    }
                                                    else
                                                    {
                                                        drretrieves["DayNumber"] = "0";
                                                    }
                                                if (entity.new_name != null)
                                                {
                                                    drretrieves["DayofWeek"] = entity.new_name;
                                                }
                                                if (entity.bcmc_date != null)
                                                    if (!entity.bcmc_date.IsNull)
                                                    {
                                                        drretrieves["Date"] = entity.bcmc_date.date;
                                                    }
                                                if (entity.bcmc_originalpercentofpeak != null)
                                                    if (!entity.bcmc_originalpercentofpeak.IsNull)
                                                    {
                                                        drretrieves["Original % of Peak"] = entity.bcmc_originalpercentofpeak.Value;
                                                    }
                                                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: entity.bcmc_originalpercentofpeak.Value  " + entity.bcmc_originalpercentofpeak.Value);

                                                if (entity.new_percentofpeak != null)
                                                    if (!entity.new_percentofpeak.IsNull)
                                                    {
                                                        drretrieves["Current % of Peak"] = entity.new_percentofpeak.Value;
                                                    }
                                                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: entity.new_percentofpeak.Value  " + entity.new_percentofpeak.Value);


                                                if (entity.bcmc_originalroomblock != null)
                                                    if (!entity.bcmc_originalroomblock.IsNull)
                                                    {
                                                        drretrieves["Original Block"] = entity.bcmc_originalroomblock.Value;
                                                    }
                                                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: entity.bcmc_originalroomblock.Value  " + entity.bcmc_originalroomblock.Value);
                                                if (entity.new_roomblock != null)
                                                    if (!entity.new_roomblock.IsNull)
                                                    {
                                                        drretrieves["Current Block"] = entity.new_roomblock.Value;
                                                    }
                                                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: entity.new_roomblock.Value  " + entity.new_roomblock.Value);
                                                dtretrieve.Rows.Add(drretrieves);

                                            } 
                                            gvFormLoad.DataSource = dtretrieve;
                                            gvFormLoad.DataBind();
                                            gvFormLoad.Columns[7].Visible = false;
                                            gvFormLoad.Columns[0].Visible = false;
                                            gvFormLoad.Visible = true;
                                            if (gvFormLoad.Rows.Count > 0)
                                            {
                                                DataTable dt = new DataTable();
                                                gvRoomBlock.DataSource = dt;
                                                gvRoomBlock.DataBind();
                                                gvRoomBlock.Visible = false;
                                            }
                                            btnsave.Text = "Update";
                                            btnlead.Enabled = true;
                                        }
                                        else
                                        {
                                            if (onChangeStatus==1)
                                                DeleteRoomblock(_eventID);
                                            RoomBlockGrid();

                                        }
                                    }
                                    else
                                    {
                                        if (onChangeStatus == 1)
                                            DeleteRoomblock(_eventID);
                                        RoomBlockGrid();
                                    }
                                    #endregion
                                }
                                else
                                {
                                    DisplayNullDateRecords(entityCollection);
                                }
                            }
                        }
                        else
                        {
                            RoomBlockGrid();
                        }
                        logger.Info("Dynamicgridview Page:Retrieve Roomblock Method: End");
                    }

                    catch (Exception ex)
                    {
                        logger.Error("Dynamicgridview Page: Retrieve Roomblock  Method: Error" + ex.ToString());
                    }
                }
                else
                {
                    btnsave.Visible = false;
                    btnlead.Visible = false;
                }

                logger.Info("Dynamicgridview Page:Retrieve Roomblock Method: End");
            }
            catch (Exception ex)
            {
                logger.Error("Dynamicgridview Page: Retrieve Roomblock  Method: Error" + ex.ToString());

            }
        }

        private void DisplayNullDateRecords(BusinessEntityCollection entityCollection)
        {
            try
            {
                logger.Info("Dynamicgridview Page:DisplayNullDateRecords  Method: Started");
                new_roompattern entity = new new_roompattern();
                DataTable dtretrieve = new DataTable();
                dtretrieve.Columns.Add("DayNumber");
                dtretrieve.Columns.Add("DayofWeek");
                dtretrieve.Columns.Add("Date");
                dtretrieve.Columns.Add("Original % of Peak");
                dtretrieve.Columns.Add("Original Block");
                dtretrieve.Columns.Add("Current % of Peak");
                dtretrieve.Columns.Add("Current Block");
                dtretrieve.Columns.Add("RoomGUID");
                for (int i = 0; i < entityCollection.BusinessEntities.Length; i++)
                {
                    logger.Info("Dynamicgridview Page: entityCollection.BusinessEntities.Length:: " + entityCollection.BusinessEntities.Length);
                    DataRow drretrieves = dtretrieve.NewRow();

                    logger.Info("Dynamicgridview Page:  DisplayNullDateRecords Method: Retrieve Rooom Pattern Entity Name " + entity);
                    entity = (new_roompattern)entityCollection.BusinessEntities[i];

                    drretrieves["RoomGUID"] = entity.new_roompatternid.Value.ToString();
                    logger.Info("Dynamicgridview Page: DisplayNullDateRecords Method: Retrieve Rooom Pattern Entity - entity new_roompatternid Value.ToString() " + entity.new_roompatternid.Value.ToString());

                    if (entity.new_daynumber != null)
                        if (!entity.new_daynumber.IsNull)
                        {
                            drretrieves["DayNumber"] = entity.new_daynumber.Value;
                        }
                        else
                        {
                            drretrieves["DayNumber"] = "0";
                        }
                    if (entity.new_name != null)
                    {
                        drretrieves["DayofWeek"] = entity.new_name;
                    }
                    if (entity.bcmc_date != null)
                        if (!entity.bcmc_date.IsNull)
                        {
                            drretrieves["Date"] = entity.bcmc_date.date;
                        }
                    if (entity.bcmc_originalpercentofpeak != null)
                        if (!entity.bcmc_originalpercentofpeak.IsNull)
                        {
                            drretrieves["Original % of Peak"] = entity.bcmc_originalpercentofpeak.Value;
                        }
                    if (entity.new_percentofpeak != null)
                        if (!entity.new_percentofpeak.IsNull)
                        {
                            drretrieves["Current % of Peak"] = entity.new_percentofpeak.Value;
                        }

                    if (entity.bcmc_originalroomblock != null)
                        if (!entity.bcmc_originalroomblock.IsNull)
                        {
                            drretrieves["Original Block"] = entity.bcmc_originalroomblock.Value;
                        }
                    if (entity.new_roomblock != null)
                        if (!entity.new_roomblock.IsNull)
                        {
                            drretrieves["Current Block"] = entity.new_roomblock.Value;
                        }
                    dtretrieve.Rows.Add(drretrieves);

                }
                gvFormLoad.DataSource = dtretrieve;
                gvFormLoad.DataBind();
                gvFormLoad.Columns[7].Visible = false;
                gvFormLoad.Columns[0].Visible = false;
                gvFormLoad.Visible = true;
                if (gvFormLoad.Rows.Count > 0)
                {
                    DataTable dt = new DataTable();
                    gvRoomBlock.DataSource = dt;
                    gvRoomBlock.DataBind();
                    gvRoomBlock.Visible = false;
                }


                ViewState["DisplayDateisNULL"] = "DisplayDateisNULL";
                logger.Info("Dynamicgridview Page:DisplayNullDateRecords Method: End");
            }
            catch (Exception ex)
            {

                logger.Error("Dynamicgridview Page: DisplayNullDateRecords  Method: Error" + ex.ToString()); 
            }             
          
        }

        private void DeleteRoomblock(string eventid)
        {
            try
            {
                logger.Info("Dynamicgridview Page:   DeleteRoomblock Method: Started");
                string Eventid = eventid;

                ColumnSet columnSet = new ColumnSet();

                columnSet.Attributes = new string[] { "new_roompatternid" };
                // Create a retrieve request object.
                ConditionExpression condition = new ConditionExpression();

                condition.AttributeName = "new_eventid";

                condition.Operator = ConditionOperator.Equal;

                condition.Values = new string[] { Eventid };

                logger.Info("Dynamicgridview Page:   DeleteRoomblock Method: new_eventid = " + Eventid);


                FilterExpression filter = new FilterExpression();
                filter.Conditions = new ConditionExpression[] { condition };
                // Create the query.
                QueryExpression query = new QueryExpression();

                // Set the properties of the query.
                query.ColumnSet = columnSet;

                query.Criteria = filter;

                query.EntityName = EntityName.new_roompattern.ToString();


                BusinessEntityCollection entityCollection = _service.RetrieveMultiple(query);

                if (entityCollection.BusinessEntities.Length > 0)
                {
                    logger.Info("Dynamicgridview Page:   DeleteRoomblock Method: Retrieve records count " + entityCollection.BusinessEntities.Length);

                    for (int j = 0; j < entityCollection.BusinessEntities.Length; j++)
                    {
                        CrmService.new_roompattern entity = (CrmService.new_roompattern)entityCollection.BusinessEntities[j];

                        if (entity != null)
                        {
                            logger.Info("Dynamicgridview Page:   DeleteRoomblock Method: Deleting record " + entity.new_roompatternid.Value.ToString());

                            _service.Delete(EntityName.new_roompattern.ToString(), new Guid(entity.new_roompatternid.Value.ToString()));

                            logger.Info("Dynamicgridview Page:   DeleteRoomblock Method: Deleted");
                        }

                        else
                        {
                            logger.Info("Dynamicgridview Page:Delete Roomblock Method: Entity return value  null");

                        }
                    }

                }
                else
                {
                    logger.Info("Dynamicgridview Page:Delete Roomblock Method: Room Block records are not available for this user");

                }

                logger.Info("Dynamicgridview Page:  DeleteRoomblock Method: End");
            }

            catch (Exception ex)
            {
                logger.Error("Dynamicgridview Page: DeleteRoomblock Method: Error" + ex.ToString());

            }

        }

        private void RoomBlockGrid()
        {
            try
            {
                logger.Info("Dynamicgridview Page:  RoomBlockGrid Method: Started");

                if (HttpContext.Current.Request.QueryString["Arrivaldate"] != null && HttpContext.Current.Request.QueryString["Departuredate"] != null && HttpContext.Current.Request.QueryString["recordid"] != null)
                {

                    string _arrivalDate = Convert.ToDateTime(Request.QueryString["Arrivaldate"].ToString()).ToString("MM/dd/yyyy");
                    string _departureDate = Convert.ToDateTime(Request.QueryString["Departuredate"].ToString()).ToString("MM/dd/yyyy");
                    string _recordId = Request.QueryString["recordid"].ToString();

                    logger.Info("Dynamicgridview Page:  RoomBlockGrid Method:  Input Parameter recordid " + _recordId);
                    string eventID = _recordId.Replace("{", "").Replace("}", "");

                    DateTime arrivalDate = Convert.ToDateTime(_arrivalDate);
                    DateTime departureDate = Convert.ToDateTime(_departureDate);

                    dtRoomBlock = new DataTable();

                    dtRoomBlock.Columns.Add("DayNumber");
                    dtRoomBlock.Columns.Add("DayofWeek");
                    dtRoomBlock.Columns.Add("Date");
                    dtRoomBlock.Columns.Add("Original % of Peak");
                    dtRoomBlock.Columns.Add("Original Room Block");
                    dtRoomBlock.Columns.Add("Current % of Peak");
                    dtRoomBlock.Columns.Add("CurrentRoom Block");
                    dtRoomBlock.Columns.Add("GUIDID");
                    dtRoomBlock.Columns.Add("RoomGUID");

                    TimeSpan dtTime = departureDate.Subtract(arrivalDate);

                    for (int i = 0; i < dtTime.TotalDays; i++)
                    {
                        DataRow dr = dtRoomBlock.NewRow();
                        dr["DayNumber"] = (i + 1).ToString();
                        dr["DayofWeek"] = arrivalDate.AddDays(i).DayOfWeek.ToString();
                        dr["Date"] = arrivalDate.AddDays(i).ToShortDateString();
                        dr["GUIDID"] = eventID;
                        dtRoomBlock.Rows.Add(dr);
                    }

                    gvRoomBlock.DataSource = dtRoomBlock;
                    gvRoomBlock.DataBind();
                    gvRoomBlock.Columns[0].Visible = false;
                    gvRoomBlock.Columns[7].Visible = false;
                    gvRoomBlock.Columns[8].Visible = false;
                    logger.Info("Dynamicgridview Page: gvFormLoad.Rows.Count :: " + gvFormLoad.Rows.Count);
                    if (gvFormLoad.Rows.Count > 0)
                    {
                        DataTable dt = new DataTable();
                        gvFormLoad.DataSource = dt;
                        gvFormLoad.DataBind();
                        gvFormLoad.Visible = false;
                    }
                }
                logger.Info("Dynamicgridview Page:  RoomBlockGrid Method: End");
            }
            catch (Exception ex)
            {
                logger.Error("Dynamicgridview Page: RoomBlockGrid Method: Error" + ex.ToString());
            }

        }

        private void RetrieveBasedonStatus(CrmService.CrmService _service)
        {
            try
            {
                logger.Info("Dynamicgridview Page:  RetrieveBasedonStatus Method: Started");
                if (HttpContext.Current.Request.QueryString["recordid"] != null && HttpContext.Current.Request.QueryString["Arrivaldate"] != null && HttpContext.Current.Request.QueryString["Departuredate"] != null)
                {
                    string getRecordid = Request.QueryString["recordid"].ToString();
                    string _arrivaldate = Convert.ToDateTime(Request.QueryString["Arrivaldate"].ToString()).ToString("MM/dd/yyyy");
                    string __departuredate = Convert.ToDateTime(Request.QueryString["Departuredate"].ToString()).ToString("MM/dd/yyyy");
                    CrmDateTime _arrivaldatedt = ConvertToCRMDateTime(Convert.ToDateTime(_arrivaldate));
                    CrmDateTime _departuredatedt = ConvertToCRMDateTime(Convert.ToDateTime(__departuredate));
                    string _eventID = getRecordid.Replace("{", "").Replace("}", "");
                    ViewState["_eventID"] = _eventID;
                    try
                    {
                        logger.Info("Dynamicgridview Page:  RetrieveBasedonStatus Roomblock  Method: Started");

                        // Define the entity attributes (database table columns) that are to be retrieved.

                        new_roompattern entity = new new_roompattern();
                        BusinessEntityCollection entityCollection = CheckRoomPatternecords(_eventID, _service);
                        logger.Info("Dynamicgridview Page:  RetrieveBasedonStatus Roomblock  Method: After service execution");

                        Hashtable hshTableRoomPattern = new Hashtable();

                        if (entityCollection.BusinessEntities.Length > 0)
                        {
                            logger.Info("Dynamicgridview Page: entityCollection.BusinessEntities.Length:: " + entityCollection.BusinessEntities.Length);

                            for (int j = 0; j < entityCollection.BusinessEntities.Length; j++)
                            {

                                entity = (CrmService.new_roompattern)entityCollection.BusinessEntities[j];
                                logger.Info("Dynamicgridview Page:  RetrieveBasedonStatus Roomblock  Method: Retrieve Rooom Pattern Entity Value " + entity);

                                //Added by ZSL Team on 28-06-2012
                                if (entity.bcmc_date != null)
                                    hshTableRoomPattern.Add("bcmc_date" + j, entity.bcmc_date.Value);
                                else
                                    hshTableRoomPattern.Add("bcmc_date" + j, "01/01/0001");

                            }

                            //Add By  ZSL Team 28-05-2012
                            if (hshTableRoomPattern.Count > 0)
                            {
                                string tempArrivalDateRoomPattern = hshTableRoomPattern["bcmc_date0"].ToString();
                                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: RetrieveBasedonStatus Rooom Pattern ArrivalDate " + hshTableRoomPattern["bcmc_date0"].ToString());
                                string tempDepartureDateRoomPattern = hshTableRoomPattern["bcmc_date" + (hshTableRoomPattern.Count - 1).ToString()].ToString();
                                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: RetrieveBasedonStatus Rooom Pattern ArrivalDate " + hshTableRoomPattern["bcmc_date" + (hshTableRoomPattern.Count - 1).ToString()].ToString());

                                #region Bind Grid

                                if (true)
                                {
                                    if (true)
                                    {
                                        dtretrieve = new DataTable();
                                        dtretrieve.Columns.Add("DayNumber");
                                        dtretrieve.Columns.Add("DayofWeek");
                                        dtretrieve.Columns.Add("Date");
                                        dtretrieve.Columns.Add("Original % of Peak");
                                        dtretrieve.Columns.Add("Original Block");
                                        dtretrieve.Columns.Add("Current % of Peak");
                                        dtretrieve.Columns.Add("Current Block");
                                        dtretrieve.Columns.Add("RoomGUID");
                                        dtretrieve.Columns.Add("GUIDID");

                                        for (int i = 0; i < entityCollection.BusinessEntities.Length; i++)
                                        {
                                            logger.Info("Dynamicgridview Page: entityCollection.BusinessEntities.Length:: " + entityCollection.BusinessEntities.Length);
                                            DataRow drretrieves = dtretrieve.NewRow();

                                            logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: RetrieveBasedonStatus Rooom Pattern Entity Name " + entity);
                                            entity = (new_roompattern)entityCollection.BusinessEntities[i];

                                            drretrieves["RoomGUID"] = entity.new_roompatternid.Value.ToString();
                                            drretrieves["GUIDID"] = _eventID.ToString();
                                            logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: RetrieveBasedonStatus Rooom Pattern Entity - entity new_roompatternid Value.ToString() " + entity.new_roompatternid.Value.ToString());

                                            if (entity.new_daynumber != null)
                                                if (!entity.new_daynumber.IsNull)
                                                {
                                                    drretrieves["DayNumber"] = entity.new_daynumber.Value;
                                                }
                                                else
                                                {
                                                    drretrieves["DayNumber"] = "0";
                                                }

                                            if (entity.new_name != null)
                                            {
                                                drretrieves["DayofWeek"] = entity.new_name;
                                            }
                                            if (entity.bcmc_date != null)
                                                if (!entity.bcmc_date.IsNull)
                                                {
                                                    drretrieves["Date"] = entity.bcmc_date.date;
                                                }

                                            logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: RetrieveBasedonStatus Rooom Pattern Entity - entity.bcmc_date.date " + entity.bcmc_date.date);

                                            if (entity.bcmc_originalpercentofpeak != null)
                                            {
                                                if (!entity.bcmc_originalpercentofpeak.IsNull)
                                                {
                                                    drretrieves["Original % of Peak"] = entity.bcmc_originalpercentofpeak.Value;

                                                }
                                                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: RetrieveBasedonStatus Rooom Pattern Entity - entity.bcmc_originalpercentofpeak.Value " + entity.bcmc_originalpercentofpeak.Value);
                                                btnsave.Text = "Update";
                                                btnlead.Enabled = true;
                                            }

                                            if (entity.bcmc_originalroomblock != null)
                                            {
                                                if (!entity.bcmc_originalroomblock.IsNull)
                                                {
                                                    drretrieves["Original Block"] = entity.bcmc_originalroomblock.Value;
                                                }
                                                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: RetrieveBasedonStatus Rooom Pattern Entity - entity.bcmc_originalroomblock.Value " + entity.bcmc_originalroomblock.Value);
                                                btnsave.Text = "Update";
                                                btnlead.Enabled = true;
                                            }


                                            if (entity.new_percentofpeak != null)
                                            {
                                                if (!entity.new_percentofpeak.IsNull)
                                                {
                                                    drretrieves["Current % of Peak"] = entity.new_percentofpeak.Value;
                                                }
                                                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: RetrieveBasedonStatus Rooom Pattern Entity - entity.new_percentofpeak.Value " + entity.new_percentofpeak.Value);
                                                btnsave.Text = "Update";
                                                btnlead.Enabled = true;
                                            }
                                            if (entity.new_roomblock != null)
                                            {
                                                if (!entity.new_roomblock.IsNull)
                                                {
                                                    drretrieves["Current Block"] = entity.new_roomblock.Value;
                                                }
                                                logger.Info("Dynamicgridview Page:  Retrieve Roomblock  Method: RetrieveBasedonStatus Rooom Pattern Entity - entity.new_roomblock.Value " + entity.new_roomblock.Value);
                                                btnsave.Text = "Update";
                                                btnlead.Enabled = true;
                                            }



                                            logger.Info("Dynamicgridview Page:RetrieveBasedonStatus Roomblock Method: Rows Added in ");
                                            dtretrieve.Rows.Add(drretrieves);

                                        }

                                        gvFormLoad.DataSource = dtretrieve;
                                        gvFormLoad.DataBind();

                                        bool dateFLag = false;

                                        Hashtable hshDateCollections = new Hashtable();

                                        for (int i = 0; i < dtretrieve.Rows.Count; i++)
                                        {
                                            string getDate = dtretrieve.Rows[i]["Date"].ToString();

                                            logger.Info("Dynamicgridview Page:RetrieveBasedonStatus Roomblock Method: Rows Added in getDate " + getDate.ToString());

                                            string roomID = dtretrieve.Rows[i]["RoomGUID"].ToString();

                                            logger.Info("Dynamicgridview Page:RetrieveBasedonStatus Roomblock Method: Rows Added in Room ID " + roomID.ToString());


                                            //Added this code for check the existing records in crm, if those records does not having the date and has null means, we check the record and update it.

                                            // Added by ZSL team on 27th July 2012.

                                            BusinessEntityCollection rp_Records_Entities = FindRoomPatternDates(_eventID, _service);

                                            if (rp_Records_Entities.BusinessEntities.Length == 0)
                                            {

                                                if (!CheckDateExists(_arrivaldate, __departuredate, getDate))
                                                {
                                                    logger.Info("RetrieveBasedonStatus Method CheckDateExists Method Checked, Room pattern Entity update process started");
                                                    new_roompattern roomEvent = new new_roompattern();
                                                    roomEvent.new_roompatternid = new Key();
                                                    roomEvent.new_roompatternid.Value = new Guid(roomID);
                                                    CrmNumber currntroomblock = new CrmNumber();
                                                    currntroomblock.Value = 0;
                                                    roomEvent.new_roomblock = currntroomblock;
                                                    dtretrieve.Rows[i]["Current Block"] = "";
                                                    CrmNumber currentpeak = new CrmNumber();
                                                    currentpeak.Value = 0;
                                                    roomEvent.new_percentofpeak = currentpeak;
                                                    dtretrieve.Rows[i]["Current % of Peak"] = "";
                                                    //Commented by ZSL Team on July 27th 2012
                                                    //  _service.Update(roomEvent);
                                                    logger.Info("RetrieveBasedonStatus Method CheckDateExists Method Checked, Room pattern Entity update completed");
                                                }
                                            }
                                            else
                                            {
                                                dateFLag = true;
                                                hshDateCollections = ProcessDateIsNull(_arrivaldate, __departuredate);
                                                break;
                                            }

                                        }
                                        dtRoomBlock = dtretrieve;
                                        gvFormLoad.DataSource = dtretrieve;
                                        gvFormLoad.DataBind();

                                        for (int i = 0; i < dtretrieve.Rows.Count; i++)
                                        {
                                            string getDate = dtretrieve.Rows[i]["Date"].ToString();
                                            string roomID = dtretrieve.Rows[i]["RoomGUID"].ToString();

                                            if (!dateFLag && !CheckDateExists(_arrivaldate, __departuredate, getDate))
                                            {
                                                logger.Info("RetrieveBasedonStatus Method CheckDateExists Method Checked, Gridview Row Disabled on this date " + getDate.ToString());
                                                gvFormLoad.Rows[i].Enabled = false;
                                            }

                                            //Based on the day number update, we ll show the releated valid records in lead link portal. 

                                            //Commented this code by ZSL team on Aug 3rd 2012

                                            //This logic is implemented in Add Room Block plugin

                                            //new_roompattern roomEvent = new new_roompattern();
                                            //roomEvent.new_roompatternid = new Key();

                                            //roomEvent.new_roompatternid.Value = new Guid(roomID);
                                            //CrmNumber intdaynumber = new CrmNumber();
                                            //intdaynumber.Value = i + 1;
                                            //roomEvent.new_daynumber = intdaynumber;


                                            //if (dateFLag)
                                            //{
                                            //    CrmDateTime dateTime = new CrmDateTime();
                                            //    dateTime.Value = hshDateCollections[i].ToString();
                                            //    roomEvent.bcmc_date = dateTime;
                                            //}
                                            //_service.Update(roomEvent);
                                        }



                                        gvFormLoad.Columns[7].Visible = false;
                                        gvFormLoad.Columns[0].Visible = false;
                                        gvFormLoad.Visible = true;

                                        if (gvFormLoad.Rows.Count > 0)
                                        {

                                            DataTable dt = new DataTable();
                                            gvRoomBlock.DataSource = dt;
                                            gvRoomBlock.DataBind();
                                            gvRoomBlock.Visible = false;
                                        }
                                    }
                                }

                                #endregion
                            }

                        }
                        else
                        {
                            RoomBlockGrid();
                        }
                        logger.Info("Dynamicgridview Page:RetrieveBasedonStatus Roomblock Method: End");
                    }

                    catch (Exception ex)
                    {
                        logger.Error("Dynamicgridview Page: RetrieveBasedonStatus Roomblock  Method: Error" + ex.ToString());
                    }
                }
                else
                {
                    btnsave.Visible = false;
                    btnlead.Visible = false;
                }

                logger.Info("Dynamicgridview Page:RetrieveBasedonStatus Roomblock Method: End");
            }
            catch (Exception ex)
            {
                logger.Error("Dynamicgridview Page: RetrieveBasedonStatus Roomblock  Method: Error" + ex.ToString());

            }

        }

        private Hashtable ProcessDateIsNull(string _arrivaldate, string __departuredate)
        {
            Hashtable hshDateCollections = new Hashtable();
            try
            {
                logger.Info("Dynamicgridview Page:  ProcessDateIsNull   Method: Started");
                DateTime departureDate = Convert.ToDateTime((Convert.ToDateTime(__departuredate)).ToString("MM/dd/yyyy"));
                logger.Info("CheckDateExists Method: ProcessDateIsNull:Departure Date: " + departureDate);
                DateTime arrivalDate = Convert.ToDateTime((Convert.ToDateTime(_arrivaldate)).ToString("MM/dd/yyyy"));
                logger.Info("CheckDateExists Method: ProcessDateIsNull:Arrival Date: " + departureDate);

                TimeSpan dtTime = departureDate.Subtract(arrivalDate);

                for (int i = 0; i < dtTime.TotalDays; i++)
                {
                    hshDateCollections.Add(i, arrivalDate.AddDays(i).ToString("MM/dd/yyyy"));
                }
                logger.Info("ProcessDateIsNull Method: compared dates Failed. No Same dates found.");
            }
            catch (Exception ex)
            {
                logger.Error("ProcessDateIsNull Method: Error" + ex.ToString());
                // throw;
            }
            return hshDateCollections;
        }

        private bool CheckDateExists(string _arrivaldate, string __departuredate, string currentArrDate)
        {
            try
            {
                logger.Info("Dynamicgridview Page:  CheckDateExists   Method: Started");

                DateTime departureDate = Convert.ToDateTime((Convert.ToDateTime(__departuredate)).ToString("MM/dd/yyyy"));
                logger.Info("CheckDateExists Method: compared dates Success " + departureDate);
                DateTime arrivalDate = Convert.ToDateTime((Convert.ToDateTime(_arrivaldate)).ToString("MM/dd/yyyy"));
                logger.Info("CheckDateExists Method: compared dates Success " + arrivalDate);
                DateTime getArrDate = Convert.ToDateTime((Convert.ToDateTime(currentArrDate)).ToString("MM/dd/yyyy"));
                logger.Info("CheckDateExists Method: Get process date " + getArrDate);
                TimeSpan dtTime = departureDate.Subtract(arrivalDate);

                for (int i = 0; i < dtTime.TotalDays; i++)
                {
                    if (getArrDate.ToString("MM/dd/yyyy") == arrivalDate.AddDays(i).ToString("MM/dd/yyyy"))
                    {
                        logger.Info("CheckDateExists Method: compared dates Success " + getArrDate);
                        return true;
                    }
                }
                logger.Info("CheckDateExists Method: compared dates Failed. No Same dates found.");
            }
            catch (Exception ex)
            {
                logger.Error("CheckDateExists Method: Error" + ex.ToString());
                // throw;
            }
            return false;
        }

        private string CheckEventStatus(CrmService.CrmService _service, string getRecordid)
        {

            logger.Info("Dynamicgridview Page: Page_Load Method: CheckEventStatus Method Started");
            string getstatusCode = "";
            try
            {
                ColumnSet columnSet = new ColumnSet();

                columnSet.Attributes = new string[] { "bcmc_eventname", "statuscode" };

                ConditionExpression condition_new_eventid = new ConditionExpression();
                condition_new_eventid.AttributeName = "opportunityid";
                condition_new_eventid.Operator = ConditionOperator.Equal;
                condition_new_eventid.Values = new string[] { getRecordid };

                FilterExpression filter_Eventid = new FilterExpression();
                filter_Eventid.Conditions = new ConditionExpression[] { condition_new_eventid };

                // Create the query.
                QueryExpression query = new QueryExpression();

                // Set the properties of the query.
                query.ColumnSet = columnSet;
                query.Criteria = filter_Eventid;
                query.EntityName = EntityName.opportunity.ToString();

                BusinessEntityCollection entityCollection = _service.RetrieveMultiple(query);
                Hashtable hshTableOpportunity = new Hashtable();
                if (entityCollection.BusinessEntities.Length > 0)
                {
                    logger.Info("Dynamicgridview Page: entityCollection.BusinessEntities.Length:: " + entityCollection.BusinessEntities.Length);

                    for (int j = 0; j < entityCollection.BusinessEntities.Length; j++)
                    {
                        CrmService.opportunity opportunity = (CrmService.opportunity)entityCollection.BusinessEntities[j];
                        getstatusCode = opportunity.statuscode.Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {

                logger.Error("CheckEventStatus Method" + ex.ToString());
            }

            return getstatusCode;
        }

        private EntityCollection CheckRoomPatternecords(string _eventID, CrmService.CrmService _service)
        {

            logger.Info("Dynamicgridview Page:  CheckRoomPatternecords Roomblock  Method: Started");

            ColumnSet columnSet = new ColumnSet();

            columnSet.Attributes = new string[] { "new_daynumber", "new_name", "bcmc_date", "new_percentofpeak", "bcmc_originalpercentofpeak", "bcmc_originalroomblock", "new_roomblock" };
            // Create a retrieve request object.
            ConditionExpression condition_new_eventid = new ConditionExpression();
            condition_new_eventid.AttributeName = "new_eventid";
            condition_new_eventid.Operator = ConditionOperator.Equal;
            condition_new_eventid.Values = new string[] { _eventID };
            logger.Info("Dynamicgridview Page:   CheckRoomPatternecords Method: new_eventid = " + _eventID);

            OrderExpression ordeErp = new OrderExpression();
            ordeErp.AttributeName = "bcmc_date";
            ordeErp.OrderType = OrderType.Ascending;

            FilterExpression filter_Eventid = new FilterExpression();
            // Set the properties of the FilterExpression.            
            filter_Eventid.Conditions = new ConditionExpression[] { condition_new_eventid };

            // Create the query.
            QueryExpression query = new QueryExpression();
            // Set the properties of the query.
            query.ColumnSet = columnSet;
            query.Criteria = filter_Eventid;
            query.Orders = new OrderExpression[] { ordeErp };
            query.EntityName = EntityName.new_roompattern.ToString();
            logger.Info("Dynamicgridview Page:  CheckRoomPatternecords Roomblock  Method: Before service execution");
            BusinessEntityCollection entityCollection = _service.RetrieveMultiple(query);
            logger.Info("Dynamicgridview Page:  CheckRoomPatternecords Roomblock  Method: After service execution");

            return entityCollection;
        }

        private BusinessEntityCollection FindRoomPatternDates(string _eventID, CrmService.CrmService _service)
        {

            logger.Info("Dynamicgridview Page:  FindRoomPatternDates   Method: Started");
            ColumnSet columnSet = new ColumnSet();

            columnSet.Attributes = new string[] { "new_daynumber", "new_name", "bcmc_date", "new_percentofpeak", "bcmc_originalpercentofpeak", "bcmc_originalroomblock", "new_roomblock" };
            // Create a retrieve request object.
            ConditionExpression condition_new_eventid = new ConditionExpression();
            condition_new_eventid.AttributeName = "new_eventid";
            condition_new_eventid.Operator = ConditionOperator.Equal;
            condition_new_eventid.Values = new string[] { _eventID };
            logger.Info("Dynamicgridview Page:   FindRoomPatternDates Method: new_eventid = " + _eventID);


            ConditionExpression condition_date = new ConditionExpression();
            condition_date.AttributeName = "bcmc_date";
            condition_date.Operator = ConditionOperator.Null;

            FilterExpression filter_Eventid = new FilterExpression();
            // Set the properties of the FilterExpression.
            filter_Eventid.FilterOperator = LogicalOperator.And;
            filter_Eventid.Conditions = new ConditionExpression[] { condition_new_eventid, condition_date };

            // Create the query.
            QueryExpression query = new QueryExpression();

            // Set the properties of the query.
            query.ColumnSet = columnSet;
            query.Criteria = filter_Eventid;

            query.EntityName = EntityName.new_roompattern.ToString();
            logger.Info("Dynamicgridview Page:  FindRoomPatternDates   Method: Before service execution");
            BusinessEntityCollection entityCollection = _service.RetrieveMultiple(query);
            logger.Info("Dynamicgridview Page:  FindRoomPatternDates   Method: After service execution");
            return entityCollection;
        }

        private void BindSave(string _eventID)
        {

            try
            {
                logger.Info("Dynamicgridview Page:   SaveRoomBlockGrid Method: Started");
                CrmNumber inttotalhotelroom = new CrmNumber();
                CrmNumber intpeakroomnight = new CrmNumber();
                Guid crmguid = Guid.Empty;
                logger.Info("Dynamicgridview Page:   gvRoomBlock.Rows.Count " + gvRoomBlock.Rows.Count);
              
                for (int i = 0; i < gvRoomBlock.Rows.Count; i++)
                {
                    //Instantiate a string property.

                    CrmNumber intdaynumber = new CrmNumber();
                    CrmNumber intoriginalpeak = new CrmNumber();
                    CrmNumber intOriginal = new CrmNumber();
                    CrmNumber intpercentofpeak = new CrmNumber();
                    CrmNumber intCurrent = new CrmNumber();

                    CrmNumberProperty daynumber = new CrmNumberProperty();
                    daynumber.Name = "new_daynumber";
                    intdaynumber.Value = Convert.ToInt32(dtRoomBlock.Rows[i]["DayNumber"]);
                    daynumber.Value = intdaynumber;
                   

                    StringProperty new_week = new StringProperty();
                    new_week.Name = "new_name";
                    new_week.Value = dtRoomBlock.Rows[i]["DayofWeek"].ToString();

                    logger.Info("Dynamicgridview Page:   SaveRoomBlockGrid Method: DayofWeek " + gvRoomBlock.Rows[i].Cells[1].Text);
                    CrmDateTimeProperty roomblockdate = new CrmDateTimeProperty();
                    roomblockdate.Name = "bcmc_date";
                    roomblockdate.Value = new RoomBlock.CrmService.CrmDateTime();
                    roomblockdate.Value.Value = dtRoomBlock.Rows[i]["Date"].ToString();

                    logger.Info("Dynamicgridview Page:   SaveRoomBlockGrid Method: Date " + roomblockdate.Value.Value);
                   
                    CrmNumberProperty new_originalpeak = new CrmNumberProperty();
                    new_originalpeak.Name = "bcmc_originalpercentofpeak";

                    if ((((Label)(gvRoomBlock.Rows[i].FindControl("lblOPeak"))).Text != string.Empty) && ((Label)(gvRoomBlock.Rows[i].FindControl("lblOPeak"))).Text != null)
                    {
                        if (((Label)(gvRoomBlock.Rows[i].FindControl("lblOPeak"))).Text.Contains("."))
                        {
                            intoriginalpeak.Value = Convert.ToInt32(((Label)(gvRoomBlock.Rows[i].FindControl("lblOPeak"))).Text.Split('.')[0].ToString());
                        }
                        else
                            intoriginalpeak.Value = Convert.ToInt32(((Label)(gvRoomBlock.Rows[i].FindControl("lblOPeak"))).Text);
                    }

                    else
                    {
                        intoriginalpeak.Value = 0;
                    }

                    new_originalpeak.Value = intoriginalpeak;
                    logger.Info("#################################   SaveRoomBlockGrid Method:  Original % Of  Peak        #####################################################");
                    logger.Info(" new_originalpeak.Value = " + new_originalpeak.Value);
                    logger.Info("#################################   SaveRoomBlockGrid Method: Original % Of  Peak          ##############################################");


                    CrmNumberProperty new_Original = new CrmNumberProperty();
                    new_Original.Name = "bcmc_originalroomblock";

                    if ((((Label)(gvRoomBlock.Rows[i].FindControl("lblOblock"))).Text != string.Empty) && ((Label)(gvRoomBlock.Rows[i].FindControl("lblOblock"))).Text != null)
                    {
                        intOriginal.Value = Convert.ToInt32(((Label)(gvRoomBlock.Rows[i].FindControl("lblOblock"))).Text);
                    }
                    else
                    {
                        intOriginal.Value = 0;
                    }
                    new_Original.Value = intOriginal;


                    logger.Info("###############################  SaveRoomBlockGrid Method:  Original RoomBlock  #################################################");
                    logger.Info("  new_Original.Value " + new_Original.Value);
                    logger.Info("############################### SaveRoomBlockGrid Method:  Original RoomBlock    ############################################");


                    CrmNumberProperty new_percentofpeak = new CrmNumberProperty();
                    new_percentofpeak.Name = "new_percentofpeak";

                    if ((((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))).Value != string.Empty) && ((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))) != null)
                    {
                        if (((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))).Value.Contains("."))
                        {
                            intpercentofpeak.Value = Convert.ToInt32(((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))).Value.Split('.')[0].ToString());
                        }
                        else
                            intpercentofpeak.Value = Convert.ToInt32(((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))).Value);
                    }
                    else
                    {
                        intpercentofpeak.Value = 0;
                    }
                    new_percentofpeak.Value = intpercentofpeak;

                    logger.Info("##################################  SaveRoomBlockGrid Method:   % Of Peak         #####################################");
                    logger.Info(" new_percentofpeak.Value = " + new_percentofpeak.Value);
                    logger.Info("##################################  SaveRoomBlockGrid Method:  % Of Peak      #######################################");


                    CrmNumberProperty new_Current = new CrmNumberProperty();
                    new_Current.Name = "new_roomblock";

                    if ((((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != string.Empty) && ((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != null)
                    {
                        intCurrent.Value = Convert.ToInt32(((TextBox)gvRoomBlock.Rows[i].FindControl("txtCBlock")).Text);
                    }
                    else
                    {
                        intCurrent.Value = 0;
                    }
                    new_Current.Value = intCurrent;


                    logger.Info("##################################  SaveRoomBlockGrid Method: Current RoomBlock   #########################################");
                    logger.Info("  new_Current.Value " + new_Current.Value);
                    logger.Info("###############################    SaveRoomBlockGrid Method: Current RoomBlock    ##########################################");

                    //Instantiate a lookup property.
                    LookupProperty new_eventid = new LookupProperty();
                    new_eventid.Name = "new_eventid";
                    new_eventid.Value = new Lookup();
                    new_eventid.Value.Value = new Guid(dtRoomBlock.Rows[i]["GUIDID"].ToString());

                    logger.Info("Dynamicgridview Page:   SaveRoomBlockGrid Method:  new_eventid  " + dtRoomBlock.Rows[i]["GUIDID"].ToString());


                    // Create the DynamicEntity object.
                    DynamicEntity myroomblock = new DynamicEntity();

                    // Set the name of the entity type.
                    myroomblock.Name = EntityName.new_roompattern.ToString();
                    myroomblock.Properties = new Property[] { daynumber, new_week, roomblockdate, new_originalpeak, new_Original, new_percentofpeak, new_Current, new_eventid };

                    // Create the target.
                    TargetCreateDynamic targetCreate = new TargetCreateDynamic();
                    targetCreate.Entity = myroomblock;

                    // Create the request object.
                    CreateRequest create = new CreateRequest();

                    // Set the properties of the request object.
                    create.Target = targetCreate;

                    logger.Info("Dynamicgridview Page:  SaveRoomBlockGrid Method: Before create the Roomblocks inside the Roompattern");
                    // Execute the request.
                    CreateResponse created = (CreateResponse)_service.Execute(create);
                    logger.Info("Dynamicgridview Page:  SaveRoomBlockGrid Method: After Created Roomblocks Successfully in Roompattern ");

                    crmguid = created.id;
                    dtRoomBlock.Rows[i]["RoomGUID"] = crmguid.ToString();

                    logger.Info("Dynamicgridview Page:   SaveRoomBlockGrid Method: RoomPatternGuid " + dtRoomBlock.Rows[i]["RoomGUID"].ToString());

                }
                BindPeakroom(gvRoomBlock);

                CrmNumberProperty new_hotelroomnights = new CrmNumberProperty();
                new_hotelroomnights.Name = "new_hotelroomnights";
                if (Totalrooms != null)
                {
                    inttotalhotelroom.Value = Convert.ToInt32(Totalrooms);

                }
                else
                {
                    inttotalhotelroom.Value = 0;
                }

                logger.Info("Dynamicgridview Page:   SaveRoomBlockGrid Method:  inttotalhotelroom " + Totalrooms);
                CrmNumberProperty new_peakhotelroomnights = new CrmNumberProperty();
                new_peakhotelroomnights.Name = "new_peakhotelroomnights";
                //if (lblroom.Text != null && lblroom.Text != string.Empty)
                if (PeakRoomNight != null)
                {
                    intpeakroomnight.Value = Convert.ToInt32(PeakRoomNight);

                }
                else
                {
                    intpeakroomnight.Value = 0;
                }
                logger.Info("Dynamicgridview Page:   SaveRoomBlockGrid Method:    intpeakroomnight " + PeakRoomNight);
                opportunity oppEvent = new opportunity();
                oppEvent.new_hotelroomnights = inttotalhotelroom;
                oppEvent.new_peakhotelroomnights = intpeakroomnight;

                oppEvent.opportunityid = new Key();
                // The contactid.Value is the GUID of the record to be changed.
                oppEvent.opportunityid.Value = new Guid(_eventID);

                logger.Info("Dynamicgridview Page:  SaveRoomBlockGrid Method: Before Update The Total Rooms and PeakRoomNights");
                _service.Update(oppEvent);
                logger.Info("Dynamicgridview Page:  SaveRoomBlockGrid Method:  Updated Successfully ");


                //if (crmguid != Guid.Empty && oppguid != Guid.Empty)
                if (crmguid != Guid.Empty)
                {
                    string myscript = "alert('Successfully saved ');";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);
                    btnsave.Text = "Update";
                    btnlead.Enabled = true;
                }
                Retrieve(_service, true,0);
                btnlead.Enabled = true;
                logger.Info("Dynamicgridview Page:  SaveRoomBlockGrid Method: End");

            }
            catch (Exception ex)
            {

                logger.Error("Dynamicgridview Page: SaveRoomBlockGrid Method: Error" + ex.ToString());
            }

        }

        private void UpdateRoomblock(string _eventID,int Updatestatus)
        {
            try
            {
                logger.Info("Dynamicgridview Page:   UpdateRoomblock Method: Started");
               
                 if (gvFormLoad.Rows.Count > 0)
                {
                    for (int i = 0; i < gvFormLoad.Rows.Count; i++)
                    {
                        // Create the RoomPattern object.
                        new_roompattern roompattern = new new_roompattern();
                        // Set the RoomPattern object properties to be updated.
                        HiddenField txtcurrentpercent = (HiddenField)gvFormLoad.Rows[i].FindControl("hdnfdValue");

                        string CPeak = txtcurrentpercent.Value;

                        CrmNumber intCPeak = new CrmNumber();
                        if ((((HiddenField)(gvFormLoad.Rows[i].FindControl("hdnfdValue"))).Value != string.Empty) && ((HiddenField)(gvFormLoad.Rows[i].FindControl("hdnfdValue"))) != null)
                        {
                            if (CPeak.Contains("."))
                            {
                                intCPeak.Value = Convert.ToInt32(CPeak.Split('.')[0].ToString());
                            }
                            else
                            {
                                intCPeak.Value = Convert.ToInt32(CPeak);
                            }
                        }
                        else
                        {
                            intCPeak.Value = 0;
                        }
                        roompattern.new_percentofpeak = intCPeak;
                        logger.Info("################################  UpdateRoomblock Method:   % Of Peak        ########################################################");
                        logger.Info(" roompattern.new_percentofpeak = " + roompattern.new_percentofpeak.Value);
                        logger.Info("################################## UpdateRoomblock Method:   % Of Peak        #######################################################");


                        TextBox txtcurrentroom = (TextBox)gvFormLoad.Rows[i].FindControl("txtCBlock");
                        string CRoom = txtcurrentroom.Text;
                     
                        CrmNumber intCRoom = new CrmNumber();
                        if ((((TextBox)(gvFormLoad.Rows[i].FindControl("txtCBlock"))).Text != string.Empty) && ((TextBox)(gvFormLoad.Rows[i].FindControl("txtCBlock"))).Text != null)
                        {
                            if (CRoom.Contains("."))
                            {
                                intCRoom.Value = Convert.ToInt32(CRoom.Split('.')[0].ToString());
                            }
                            else
                            {
                                intCRoom.Value = Convert.ToInt32(CRoom);
                            }
                        }
                        else
                        {
                            intCRoom.Value = 0;
                        }
                        roompattern.new_roomblock = intCRoom;

                        logger.Info("#################################  UpdateRoomblock Method: Current RoomBlock   #######################################");
                        logger.Info("  roompattern.new_roomblock " + roompattern.new_roomblock.Value);
                        logger.Info("################################  UpdateRoomblock Method:Current RoomBlock   #########################################");

                        // The RoomPatternid is a key that references the ID of the RoomPattern to be updated.
                        roompattern.new_roompatternid = new RoomBlock.CrmService.Key();
                        // The RoomPatternid.Value is the GUID of the record to be changed.

                        roompattern.new_roompatternid.Value = new Guid(dtretrieve.Rows[i]["RoomGUID"].ToString());
                        logger.Info("Dynamicgridview Page:   UpdateRoomBlockGrid Method: RoomPatternGuid " + dtretrieve.Rows[i]["RoomGUID"].ToString());


                        // Update the RoomPattern.

                        logger.Info("--------------------------------    UpdateRoomblock Method    -------------------------------------------------");
                        logger.Info("roompattern.new_percentofpeak = " + roompattern.new_percentofpeak.Value + "- roompattern.new_roomblock " + roompattern.new_roomblock.Value);
                        logger.Info("----------------------------------  UpdateRoomblock Method  ---------------------------------------------------");
                        
                        logger.Info("Dynamicgridview Page:  UpdateRoomblock Method: Before Update ");                        
                        _service.Update(roompattern);
                        logger.Info("Dynamicgridview Page:  UpdateRoomblock Method: Updated Successfully");

                    }
                    // Update an attribute retrieved via RetrieveAttributeRequest

                    BindPeakroom(gvFormLoad);
                    opportunity Opportunity = new opportunity();
                    CrmNumber totalrooms = new CrmNumber();
                    if (Totalrooms != null)
                    {
                        totalrooms.Value = Convert.ToInt32(Totalrooms);
                    }
                    else
                    {
                        totalrooms.Value = 0;

                    }
                    Opportunity.new_hotelroomnights = totalrooms;

                    CrmNumber Peaknight = new CrmNumber();
                    if (PeakRoomNight != null)
                    {
                        Peaknight.Value = Convert.ToInt32(PeakRoomNight);

                    }
                    else
                    {
                        Peaknight.Value = 0;
                    }
                    Opportunity.new_peakhotelroomnights = Peaknight;

                    Opportunity.opportunityid = new RoomBlock.CrmService.Key();
                    Opportunity.opportunityid.Value = new Guid(_eventID);
                    logger.Info("Dynamicgridview Page:  UpdateRoomblock Method: Before Update The Total Rooms and PeakRoomNights");
                    _service.Update(Opportunity);
                    logger.Info("Dynamicgridview Page:  UpdateRoomblock Method: Updated Successfully");
                    if (Updatestatus == 1)
                    {
                        string myscript = "alert('Successfully updated ');";
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);
                    }
                    Retrieve(_service, true,0);
                    btnlead.Enabled = true;

                }


               //if (gvRoomBlock.Columns.Count == 9)
                else if (gvRoomBlock.Rows.Count > 0)
                {
                    //DeleteRoomblock(_eventID);
                    for (int i = 0; i < gvRoomBlock.Rows.Count; i++)
                    {
                        // Create the RoomPattern object.
                        new_roompattern roompattern = new new_roompattern();
                        // Set the RoomPattern object properties to be updated.
                        HiddenField txtcurrentpercent = (HiddenField)gvRoomBlock.Rows[i].FindControl("hdnfdValue");
                        string CPeak = txtcurrentpercent.Value;

                        CrmNumber intCPeak = new CrmNumber();
                        if ((((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))).Value != string.Empty) && ((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))) != null)
                        {
                            if (CPeak.Contains("."))
                            {
                                intCPeak.Value = Convert.ToInt32(CPeak.Split('.')[0].ToString());
                            }
                            else
                            {
                                intCPeak.Value = Convert.ToInt32(CPeak);
                            }
                        }
                        else
                        {
                            intCPeak.Value = 0;
                        }
                        roompattern.new_percentofpeak = intCPeak;

                        logger.Info("############################### UpdateRoomBlock Method : % Of Peak    ###################################################");
                        logger.Info("roompattern.new_percentofpeak= " + roompattern.new_percentofpeak.Value);
                        logger.Info("############################### UpdateRoomBlock Method : % Of Peak   ###################################################");



                        TextBox txtcurrentroom = (TextBox)gvRoomBlock.Rows[i].FindControl("txtCBlock");
                        string CRoom = txtcurrentroom.Text;

                        
                        CrmNumber intCRoom = new CrmNumber();

                        if ((((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != string.Empty) && ((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != null)
                        {
                            if (CRoom.Contains("."))
                            {
                                intCRoom.Value = Convert.ToInt32(CRoom.Split('.')[0].ToString());
                            }
                            else
                            {
                                intCRoom.Value = Convert.ToInt32(CRoom);
                            }
                        }
                        else
                        {
                            intCRoom.Value = 0;
                        }
                        roompattern.new_roomblock = intCRoom;

                        logger.Info("############################     UpdateRoomBlock Method : Current RoomBlock   ###########################################");
                        logger.Info("roompattern.new_roomblock " + roompattern.new_roomblock.Value);
                        logger.Info("##############################  UpdateRoomBlock Method : Current RoomBlock   #########################################");
                        // The roompatternid is a key that references the ID of the RommPattern to be updated.
                        roompattern.new_roompatternid = new RoomBlock.CrmService.Key();
                        // The roompatternid.Value is the GUID of the record to be changed.    

                        roompattern.new_roompatternid.Value = new Guid(_eventID);
                      
                        // Update the RommPattern.                     
                        logger.Info("-----------------------------------        UpdateRoomBlock Method  ----------------------------------");
                        logger.Info("_eventID = " + _eventID + " roompattern.new_percentofpeak = " + roompattern.new_percentofpeak.Value + "- roompattern.new_roomblock " + roompattern.new_roomblock.Value);
                        logger.Info("-----------------------------------       UpdateRoomBlock Method  -------------------------------");
                       if(_service !=null )
                     
                         logger.Info("Dynamicgridview Page:  UpdateRoomblock Method: Before Update ");                      
                        _service.Update(roompattern);                         
                        logger.Info("Dynamicgridview Page:  UpdateRoomblock Method: Updated Successfully");
                    }
                    if (gvRoomBlock.Rows.Count > 0)
                    {
                        BindPeakroom(gvRoomBlock);
                        opportunity Opportunity = new opportunity();
                        CrmNumber totalrooms = new CrmNumber();
                        if (Totalrooms != null)
                        {
                            totalrooms.Value = Convert.ToInt32(Totalrooms);
                        }
                        else
                        {
                            totalrooms.Value = 0;

                        }
                        Opportunity.new_hotelroomnights = totalrooms;

                        CrmNumber Peaknight = new CrmNumber();
                        if (PeakRoomNight != null)
                        {
                            Peaknight.Value = Convert.ToInt32(PeakRoomNight);

                        }
                        else
                        {
                            Peaknight.Value = 0;
                        }
                        Opportunity.new_peakhotelroomnights = Peaknight;

                        Opportunity.opportunityid = new RoomBlock.CrmService.Key();
                        Opportunity.opportunityid.Value = new Guid(_eventID);
                        logger.Info("Dynamicgridview Page:  UpdateRoomblock Method: Before Update The Total Rooms and PeakRoomNights");
                        _service.Update(Opportunity);
                        logger.Info("Dynamicgridview Page:  UpdateRoomblock Method: Updated Successfully ");
                    }
                    string myscript = "alert('Successfully updated ');";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);

                    Retrieve(_service, true,0);
                    btnlead.Enabled = true;
                }

                logger.Info("Dynamicgridview Page:  UpdateRoomblock Method: End");
            }
            catch (Exception ex)
            {
                logger.Error("Dynamicgridview Page:  UpdateRoomblock Method: Error" + ex.ToString());
            }
        }

        private void UpdateRoomblockStatus(string _eventID, int Updatestatus)
        {
            try
            {
                logger.Info("Dynamicgridview Page:   UpdateRoomblockStatus Method: Started");

                if (gvFormLoad.Rows.Count > 0)
                {
                    for (int i = 0; i < gvFormLoad.Rows.Count; i++)
                    {
                        // Create the RoomPattern object.
                        new_roompattern roompattern = new new_roompattern();

                        // Set the RoomPattern object properties to be updated.

                        HiddenField txtcurrentpercent = (HiddenField)gvFormLoad.Rows[i].FindControl("hdnfdValue");

                        string CPeak = txtcurrentpercent.Value;

                        CrmNumber intCPeak = new CrmNumber();

                        if ((((HiddenField)(gvFormLoad.Rows[i].FindControl("hdnfdValue"))).Value != string.Empty) && ((HiddenField)(gvFormLoad.Rows[i].FindControl("hdnfdValue"))) != null)
                        {

                            if (CPeak.Contains("."))
                            {
                                intCPeak.Value = Convert.ToInt32(CPeak.Split('.')[0].ToString());
                            }
                            else
                            {
                                intCPeak.Value = Convert.ToInt32(CPeak);
                            }
                        }
                        else
                        {
                            intCPeak.Value = 0;
                        }
                        roompattern.new_percentofpeak = intCPeak;
                        logger.Info("############################### UpdateRoomBlockStatus Method: % Of Peak   ##########################################");
                        logger.Info(" roompattern.new_percentofpeak = " + roompattern.new_percentofpeak.Value);
                        logger.Info("############################### UpdateRoomBlockStatus Method: % Of Peak   ########################################");


                        roompattern.new_roompatternid = new RoomBlock.CrmService.Key();
                        // The RoomPatternid.Value is the GUID of the record to be changed.

                        roompattern.new_roompatternid.Value = new Guid(dtretrieve.Rows[i]["RoomGUID"].ToString());

                        string CRoom = "0";

                        CrmNumber intCRoom = new CrmNumber();
                        if (gvFormLoad.Rows[i].Enabled == true)
                        {
                            TextBox txtcurrentroom = (TextBox)gvFormLoad.Rows[i].FindControl("txtCBlock");

                            CRoom = txtcurrentroom.Text;
                           
                           
                            if ((((TextBox)(gvFormLoad.Rows[i].FindControl("txtCBlock"))).Text != string.Empty) && ((TextBox)(gvFormLoad.Rows[i].FindControl("txtCBlock"))).Text != null)
                            {
                                if (CRoom.Contains("."))
                                {
                                    intCRoom.Value = Convert.ToInt32(CRoom.Split('.')[0].ToString());
                                }
                                else
                                {
                                    intCRoom.Value = Convert.ToInt32(CRoom);
                                }
                            }
                            else
                            {
                                intCRoom.Value = 0;
                            }
                            roompattern.new_roomblock = intCRoom;

                            logger.Info("####################   UpdateRoomBlockStatus Method: Current RoomBlock ######################");
                            logger.Info(" roompattern.new_roomblock  " + roompattern.new_roomblock.Value);
                            logger.Info("#################### UpdateRoomBlockStatus Method: Current RoomBlock   #######################");


                            // The RoomPatternid is a key that references the ID of the RoomPattern to be updated.

                            // Update the RoomPattern.

                            logger.Info("-------------------------------------  UpdateRoomBlockStatus Method   ----------------------------------------------");
                            logger.Info("roompattern.new_percentofpeak = " + roompattern.new_percentofpeak.Value + "- roompattern.new_roomblock " + roompattern.new_roomblock.Value);
                            logger.Info("------------------------------------   UpdateRoomBlockStatus Method  ------------------------------------------------");

                        }
                        else
                        {
                            //For Diasbled Records Current RoomBlock value set to zero  .
                            //ADD By ZSL TEAM  ON 16/Aug/2012
                            intCRoom.Value = 0;
                            roompattern.new_roomblock = intCRoom;
                            logger.Info("####################   UpdateRoomBlockStatus Method: Disabled  Current RoomBlock ######################");
                            logger.Info(" roompattern.new_roomblock  " + roompattern.new_roomblock.Value);
                            logger.Info("#################### UpdateRoomBlockStatus Method: Disabled  Current RoomBlock   #######################");
                        
                        }

                        CrmNumber intdaynumber = new CrmNumber();
                        intdaynumber.Value = i + 1;
                        roompattern.new_daynumber = intdaynumber;
                        roompattern.new_daynumber.Value = intdaynumber.Value;
                        logger.Info("Dynamicgridview Page:  UpdateRoomblockStatus Method: Before Update");
                        _service.Update(roompattern);
                        logger.Info("Dynamicgridview Page:  UpdateRoomblockStatus Method: Updated Successfully");

                    }
                    // Update an attribute retrieved via RetrieveAttributeRequest

                    logger.Info("Dynamicgridview Page:   BindPeakroom Method: Started");
                    BindPeakroom(gvFormLoad);
                    opportunity Opportunity = new opportunity();
                    CrmNumber totalrooms = new CrmNumber();
                    if (Totalrooms != null)
                    {
                        totalrooms.Value = Convert.ToInt32(Totalrooms);
                    }
                    else
                    {
                        totalrooms.Value = 0;

                    }
                    Opportunity.new_hotelroomnights = totalrooms;

                    CrmNumber Peaknight = new CrmNumber();
                    if (PeakRoomNight != null)
                    {
                        Peaknight.Value = Convert.ToInt32(PeakRoomNight);

                    }
                    else
                    {
                        Peaknight.Value = 0;
                    }
                    Opportunity.new_peakhotelroomnights = Peaknight;

                    Opportunity.opportunityid = new RoomBlock.CrmService.Key();
                    Opportunity.opportunityid.Value = new Guid(_eventID);
                    logger.Info("Dynamicgridview Page:  UpdateRoomblockStatus Method: Before Update The Total Rooms and PeakRoomNights");
                    _service.Update(Opportunity);
                    logger.Info("Dynamicgridview Page:  UpdateRoomblockStatus Method: Updated Successfully");
                    if (Updatestatus == 1)
                    {
                        string myscript = "alert('Successfully updated ');";
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);
                    }

                    logger.Info("Dynamicgridview Page:   BindPeakroom Method: Ended");
                    RetrieveBasedonStatus(_service);
                    btnlead.Enabled = true;

                }


                else if (gvRoomBlock.Rows.Count > 0)
                {
                    for (int i = 0; i < gvRoomBlock.Rows.Count; i++)
                    {
                        // Create the RoomPattern object.
                        new_roompattern roompattern = new new_roompattern();
                        // Set the RoomPattern object properties to be updated.
                        HiddenField txtcurrentpercent = (HiddenField)gvRoomBlock.Rows[i].FindControl("hdnfdValue");
                        string CPeak = txtcurrentpercent.Value;

                        CrmNumber intCPeak = new CrmNumber();
                        if ((((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))).Value != string.Empty) && ((HiddenField)(gvRoomBlock.Rows[i].FindControl("hdnfdValue"))) != null)
                        {
                            if (CPeak.Contains("."))
                            {
                                intCPeak.Value = Convert.ToInt32(CPeak.Split('.')[0].ToString());
                            }
                            else
                            {
                                intCPeak.Value = Convert.ToInt32(CPeak);
                            }
                        }
                        else
                        {
                            intCPeak.Value = 0;
                        }
                        roompattern.new_percentofpeak = intCPeak;
                        logger.Info("############################ UpdateRoomBlockStatus Method: Current % Of Peak  ########################");
                        logger.Info("  roompattern.new_percentofpeak = " + roompattern.new_percentofpeak.Value);
                        logger.Info("##########################   UpdateRoomBlockStatus Method: Current % Of Peak   #######################");


                        TextBox txtcurrentroom = (TextBox)gvRoomBlock.Rows[i].FindControl("txtCBlock");
                        string CRoom = txtcurrentroom.Text;

                       
                        CrmNumber intCRoom = new CrmNumber();

                        if ((((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != string.Empty) && ((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != null)
                        {
                            if (CRoom.Contains("."))
                            {
                                intCRoom.Value = Convert.ToInt32(CRoom.Split('.')[0].ToString());
                            }
                            else
                            {
                                intCRoom.Value = Convert.ToInt32(CRoom);
                            }
                        }
                        else
                        {
                            intCRoom.Value = 0;
                        }

                        roompattern.new_roomblock = intCRoom;
                        logger.Info("############################ UpdateRoomBlockStatus Method: Current RoomBlock   ###################################");
                        logger.Info("roompattern.new_roomblock  " + roompattern.new_roomblock.Value);
                        logger.Info("##########################  UpdateRoomBlockStatus Method: Current RoomBlock    #################################");
                        // The roompatternid is a key that references the ID of the RommPattern to be updated.
                        roompattern.new_roompatternid = new RoomBlock.CrmService.Key();
                        // The roompatternid.Value is the GUID of the record to be changed.                     

                        logger.Info("UpdateRoomblockStatus   Method event Id:" + _eventID);
                        roompattern.new_roompatternid.Value = new Guid(_eventID);

                        // Update the RommPattern.                     
                        logger.Info("----------------------------------  UpdateRoomblockStatus Method     ------------------------------");
                        logger.Info("roompattern.new_percentofpeak = " + roompattern.new_percentofpeak.Value + "- roompattern.new_roomblock " + roompattern.new_roomblock.Value);
                        logger.Info("----------------------------------   UpdateRoomblockStatus Method  --------------------------------");

                        logger.Info("Dynamicgridview Page:  UpdateRoomblockStatus Method: Before Update");                        
                        _service.Update(roompattern);
                        logger.Info("Dynamicgridview Page:  UpdateRoomblockStatus Method: Updated Successfully");
                    }
                    if (gvRoomBlock.Rows.Count > 0)
                    {
                        BindPeakroom(gvRoomBlock);
                        opportunity Opportunity = new opportunity();
                        CrmNumber totalrooms = new CrmNumber();
                        if (Totalrooms != null)
                        {
                            totalrooms.Value = Convert.ToInt32(Totalrooms);
                        }
                        else
                        {
                            totalrooms.Value = 0;

                        }
                        Opportunity.new_hotelroomnights = totalrooms;

                        CrmNumber Peaknight = new CrmNumber();
                        if (PeakRoomNight != null)
                        {
                            Peaknight.Value = Convert.ToInt32(PeakRoomNight);

                        }
                        else
                        {
                            Peaknight.Value = 0;
                        }
                        Opportunity.new_peakhotelroomnights = Peaknight;

                        Opportunity.opportunityid = new RoomBlock.CrmService.Key();
                        Opportunity.opportunityid.Value = new Guid(_eventID);

                        logger.Info("Dynamicgridview Page:  UpdateRoomblockStatus Method: Before Update The Total Rooms and PeakRoomNights");
                        _service.Update(Opportunity);
                        logger.Info("Dynamicgridview Page:  UpdateRoomblockStatus Method: Updated Successfully ");

                    }
                    string myscript = "alert('Successfully updated ');";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "myscript", myscript, true);

                    RetrieveBasedonStatus(_service);
                    btnlead.Enabled = true;
                }

                logger.Info("Dynamicgridview Page:  UpdateRoomblockStatus Method: End");
            }
            catch (Exception ex)
            {
                logger.Error("Dynamicgridview Page:  UpdateRoomblockStatus Method: Error" + ex.ToString());
            }
        }

        private void BindPeakroom(GridView gvRoomBlock)
        {

            try
            {
                logger.Info("Dynamicgridview Page:   gvRoomBlock.Rows.Count " + gvRoomBlock.Rows.Count);
                logger.Info("Dynamicgridview Page:   BindPeakroom Method: Started");
                int maxValue = -1;
                for (int i = 0; i < gvRoomBlock.Rows.Count; i++)
                {
                    TextBox txtTotalroomblock = new TextBox();

                    if ((((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != string.Empty) && ((TextBox)(gvRoomBlock.Rows[i].FindControl("txtCBlock"))).Text != null)
                    {
                        txtTotalroomblock = (TextBox)gvRoomBlock.Rows[i].FindControl("txtCBlock");
                    }
                    else
                        txtTotalroomblock.Text = "0";

                    decimal totalroomblock = Decimal.Parse(txtTotalroomblock.Text);
                    totalRoomBlock += totalroomblock;
                    string currentroom = txtTotalroomblock.Text;

                    if (Convert.ToInt32(currentroom) > maxValue)
                    {
                        maxValue = Convert.ToInt32(currentroom);
                    }
                }
                Totalrooms = Convert.ToString(totalRoomBlock);

                logger.Info("Dynamicgridview Page:   Total HotelRooms " + Totalrooms);
                PeakRoomNight = Convert.ToString(maxValue);
                logger.Info("Dynamicgridview Page:   Total PeakRoom Nights " + PeakRoomNight);
                logger.Info("Dynamicgridview Page:   BindPeakroom Method: End");
            }
            catch (Exception ex)
            {

                logger.Error("Dynamicgridview Page: BindPeakroom Method: Error" + ex.ToString());
            }
        }

        private void CheckEventUpdateStatus(CrmService.CrmService _service, string getRecordid)
        {
            BusinessEntityCollection _businessEntitiesRP = AssignDayNumberRoomPattern(getRecordid, _service);
            new_roompattern entity = new new_roompattern();
            for (int i = 0; i < _businessEntitiesRP.BusinessEntities.Length; i++)
            {
                BusinessEntity rpEntity = _businessEntitiesRP.BusinessEntities[i];

                entity = (new_roompattern)rpEntity;
                CrmNumber crmNumber = new CrmNumber();
                crmNumber.Value = 0;
                entity.bcmc_statustype = crmNumber;
                logger.Info("Dynamicgridview Page:  CheckEventUpdateStatus: Before Update status");
                _service.Update(entity);
                logger.Info("Dynamicgridview Page:  CheckEventUpdateStatus: After Update Successfully");
            }
        }

        private BusinessEntityCollection AssignDayNumberRoomPattern(string eventGUID, CrmService.CrmService _service)
        {
            ConditionExpression conditionRP = new ConditionExpression();
            conditionRP.AttributeName = "new_eventid";
            conditionRP.Operator = ConditionOperator.Equal;
            conditionRP.Values = new string[] { eventGUID.ToString() };

            FilterExpression filterRP = new FilterExpression();
            filterRP.FilterOperator = LogicalOperator.And;
            filterRP.Conditions = new ConditionExpression[] { conditionRP };

            OrderExpression orderExpr = new OrderExpression();
            orderExpr.AttributeName = "bcmc_date";
            orderExpr.OrderType = OrderType.Ascending;

            QueryExpression queryRP = new QueryExpression();
            queryRP.EntityName = "new_roompattern";
            queryRP.ColumnSet = new AllColumns();
            queryRP.Criteria = filterRP;
            queryRP.Orders = new OrderExpression[] { orderExpr };


            logger.Info("Dynamicgridview Page:  AssignDayNumberRoomPattern  Method: Before service execution");
            BusinessEntityCollection _businessEntitiesRP = (BusinessEntityCollection)_service.RetrieveMultiple(queryRP);
            logger.Info("Dynamicgridview Page:  AssignDayNumberRoomPattern  Method: After service execution");

            return _businessEntitiesRP;
        }

		/// <summary>
		/// Verify that the current proposed dates in CRM match the data currently in the grid.
		/// </summary>
		/// <returns>
		/// Do the current dates match the grid (true/false)
		/// </returns>
		protected bool ValidateProposedDates()
		{
			// Get the arrival and departure date on the form.
			DateTime formArriveDate = Convert.ToDateTime(Request.QueryString["Arrivaldate"].ToString());
			DateTime formDepartDate = Convert.ToDateTime(Request.QueryString["Departuredate"].ToString());
			logger.InfoFormat("formArriveDate={0} formDepartDate={1}", formArriveDate, formDepartDate);

			// Get the arrival and departrue dates from the proposed date record related to the event.
			String eventId = ViewState["_eventID"].ToString();
			var cols = new ColumnSet
			{
				Attributes = new String[] { "bcmc_arrivaldate", "bcmc_departuredate" },
			};

			var qry = new QueryByAttribute
			{
				EntityName = "new_proposeddate",
				ColumnSet = cols,
				Attributes = new String[] { "new_eventid" },
				Values = new object[] { eventId }
			};
			var rsp = _service.RetrieveMultiple(qry);
			if (rsp.BusinessEntities.Length != 1)
			{
				String msg = String.Format("ValidateProposedDates() : Found {0} Proposale Date reocrds were found for Event {0}.  Should always be one.",
					rsp.BusinessEntities.Length, eventId);
				logger.Error(msg);
				Alert(msg);
				return false;
				// TODO: need to post an error.
			}
			var propDate = (new_proposeddate)rsp.BusinessEntities[0];

			DateTime crmArriveDate = Convert.ToDateTime(propDate.bcmc_arrivaldate.date);
			DateTime crmDepartDate = Convert.ToDateTime(propDate.bcmc_departuredate.date);
			logger.InfoFormat("crmArriveDaate={0} crmDepartDate={1}", crmArriveDate, crmDepartDate);
			if ((formArriveDate == crmArriveDate) && (formDepartDate == crmDepartDate))
			{
				return true;
			}
			//  if they don't match, post an error to the user.
			Alert("The Room Block Days grid does not match the Proposed Dates for the event. Please refresh the form before proceeding. If you continue to receive this error message please contact the System Administrator.");
			return false;
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
        #endregion
    }
}



