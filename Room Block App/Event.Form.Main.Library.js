/*******************************************************************/
/******************* SYSTEM FUNCTIONS START ************************/
function GlobalVariables() {
	// Place all global variables in this section
	CRM_FORM_TYPE_CREATE   = 1;
	CRM_FORM_TYPE_UPDATE   = 2;
	CRM_FORM_TYPE_READ     = 3;
	CRM_FORM_TYPE_DISABLED = 4;
	CRM_FORM_TYPE_QUICK    = 5;
	CRM_FORM_TYPE_BULK     = 6;
	
	// Used to save the values when the form is loaded.
	_arrivalDate = null;
	_departureDate = null;
	_salesprocessupdated = false;

}

function SystemLoad() {
	/* Load system functions such as events, program initiation etc. */

	OnClickEvents();
	DefaultSettings();

}

function OnClickEvents() {
	RegisterOnClickEvent("bcmc_reportactualized","EventActualized_Onchange");
	//RegisterOnDoubleClickEvent("fieldname","functionname");
}

function DefaultSettings() {
	/* Use this section to default any value, field status etc. as it should be when the form first loads.
	   For example, you may want to hide a system field when the form loads. This is a good place to do this.
	 */

}
/******************* SYSTEM FUNCTIONS END **************************/
/*******************************************************************/
/******************* ONCHANGE FUNCTIONS START **********************/

function EventName_OnChange() {
	PopulateEventName();
}

function Customer_OnChange() {
	PopulateEventName();
}

function Year_OnChange() {
	PopulateEventName();
}

function EventLost_OnChange() {
	if (cwGetValue("bcmc_eventlost") == true) {
		var recordId=Xrm.Page.data.entity.getId();
		var orgName=Xrm.Page.context.getOrgUniqueName();
		var url= 'http://crm-app-01:8092/HotelPicker/CloseEvent.aspx?id=' + recordId + '&' + 'orgname=' + orgName;
		window.showModalDialog(url,'','dialogHeight:620px;dialogWidth:600px;center:yes');
	}	
}

function ResponseDate_OnChange() {

	/* get the date from the original field of type date */
	var created= Xrm.Page.getAttribute("bcmc_hotelleadresponsedate").getValue();

	/* define a blank variable for later use */
	var textdate='';

	/* get the month number from the date and add 1 because getMonth returns 0 to 11 for the month number */
	var intMonth=parseInt(created.getMonth())+1;

	/* set the new text date field to the appropriate format. This example uses mm/dd/yyyy */
	Xrm.Page.getAttribute("bcmc_hotelresponsedatecopy").setValue(textdate.concat(intMonth.toString(),'/',created.getDate(),'/',created.getFullYear()) );
}	

function Status_OnChange() {
	var ro = false;
	var disp = true;
	if (Xrm.Page.getAttribute("statuscode").getValue() != 1) { // Prospect
		if (!(UserHasRole("BCMC System Admin"))) {
			ro = true;
			disp = false;
		}	
	}
	cwSetReadOnly("new_arrivaldate", ro);
	cwSetReadOnly("new_departuredate", ro);
	cwSetReadOnly("bcmc_eventyear", ro);
	//setVisibility("Tab","RoomBlockDaysTab",disp); Will do this using enhancement to Iframe instead.
}

function SalesProcessUpdate_OnChange() {
	_salesprocessupdated = true;
}


function EventActualized_Onchange() {

	var a = Xrm.Page.getAttribute("bcmc_reportactualized").getValue();
	if (a == true) {
		
		var f = Xrm.Page.getAttribute("bcmc_actualizeddayspriorevent");
		if (f.getValue() == null) {
			f.setValue(0);
			//SetActualizedDates();
		}		
		var t = Xrm.Page.getAttribute("bcmc_actualizeddayspostevent");
		if (t.getValue() == null) {
			t.setValue(0);
			SetActualizedDates();
		} 
		
		var origRooms = Xrm.Page.getAttribute("bcmc_originalhotelroomnights");
		var origPeak  = Xrm.Page.getAttribute("bcmc_originalpeakhotelroomnights");
		if (origRooms.getValue() == null) {
			origRooms.setValue(Xrm.Page.getAttribute("new_hotelroomnights").getValue());
			ForceSubmit("bcmc_originalhotelroomnights",true);
			origPeak.setValue(Xrm.Page.getAttribute("new_peakhotelroomnights").getValue());
			ForceSubmit("bcmc_originalpeakhotelroomnights",true);
			
		}	
		setVisibility("Section","Actualized Info",true);
		setVisibility("Control","new_hotelroomnights",false);
		setVisibility("Control","new_peakhotelroomnights",false);
		
		/*
		var preArrival = Xrm.Page.getAttribute("bcmc_actualizeddayspriorevent");
		if (preArrival.getValue() != null) {
			setVisibility("Control","bcmc_actualizedarrivaldate",true);	
			setVisibility("Control","bcmc_actualizeddeparturedate",true);	
		}
		*/

	} else {
		/*
		var f = Xrm.Page.getAttribute("bcmc_actualizeddayspriorevent");
		if (f.getValue() == null) {
			f.setValue(0);
		}		
		var t = Xrm.Page.getAttribute("bcmc_actualizeddayspostevent");
		if (t.getValue() == null) {
			t.setValue(0);
		}
		*/
		//Xrm.Page.getAttribute("bcmc_originalhotelroomnights").setValue(null);
		//Xrm.Page.getAttribute("bcmc_originalpeakhotelroomnights").setValue(null);
		setVisibility("Section","Actualized Info",false);
		setVisibility("Control","new_hotelroomnights",true);
		setVisibility("Control","new_peakhotelroomnights",true);
		

	}		
	



	
}

/******************* ONCHANGE FUNCTIONS END ************************/
/*******************************************************************/
/********************* CUSTOM FUNCTIONS START **********************/

function validateDates(executionObj) {
	
	var errMsg = "The arrival and/or departure dates exceed the 7 day window for this " + Xrm.Page.getAttribute("statuscode").getText().toUpperCase() +	" event, please cancel this event and create a new event";
	var msToDays = 86400000;
	var a = Xrm.Page.getAttribute("new_arrivaldate").getValue();
	var d = Xrm.Page.getAttribute("new_departuredate").getValue();
	var results = true;

	if (a == null || d == null)
		return true;

	if (a > d) {
		alert("Arrival and Departure dates are not sequential");
		results = false;
	}

	if (Xrm.Page.getAttribute("statuscode").getValue() != 1) { // Prospect

		if (results && a != null && _arrivalDate != null) {
			var d1 = Date.parse(a);
			var d2 = Date.parse(_arrivalDate);
			var days = (d2 - d1) / msToDays;
			if (days > 7) {
				alert(errMsg);
				results = false;
			}
		}

		if (results && d != null && _departureDate != null) {
			var d1 = Date.parse(d);
			var d2 = Date.parse(_departureDate);
			var days = (d1 - d2) / msToDays;
			if (days > 7) {
				alert(errMsg);
				results = false;
			}
		}
	}	

	if (results)
		return true;

	Xrm.Page.getControl("new_arrivaldate").setFocus();
	//executionObj.getEventArgs().preventDefault();
	return false;
}

function CalculateNet() {
   if (Xrm.Page.getAttribute("new_grosssquarefeet").getValue() == null) {
     Xrm.Page.getAttribute("new_netsquarefeet").setValue(null);
   } else {
         if (Xrm.Page.getAttribute("new_building").getText() == "Hynes")  {
                Xrm.Page.getAttribute("new_netsquarefeet").setValue(Xrm.Page.getAttribute("new_grosssquarefeet").getValue() * .5);
         }else if (Xrm.Page.getAttribute("new_building").getText() == "BCEC") {
                Xrm.Page.getAttribute("new_netsquarefeet").setValue(Xrm.Page.getAttribute("new_grosssquarefeet").getValue() * .66);
        }
   }
}

function PopulateEventName() {
	if (Xrm.Page.getAttribute("customerid").getValue() == null || Xrm.Page.getAttribute("bcmc_eventname").getValue() == null )
		 return;

	var year;
	if (Xrm.Page.getAttribute("bcmc_eventyear").getValue()==null) 
		  year = ''
	else 
	   year = ' - ' + Xrm.Page.getAttribute("bcmc_eventyear").getValue();

	Xrm.Page.getAttribute("name").setValue(Xrm.Page.getAttribute("customerid").getValue()[0].name + ": " +  Xrm.Page.getAttribute("bcmc_eventname").getValue() + year);
	Xrm.Page.getAttribute("name").setSubmitMode("always");
}


function RoomBlockGrid() {
	if (Xrm.Page.getAttribute("new_arrivaldate").getValue() != null &&
	    Xrm.Page.getAttribute("new_departuredate").getValue() != null &&
		Xrm.Page.getAttribute("bcmc_eventyear").getValue() != null 
	   ) 
	{
		setVisibility("Tab","RoomBlockDaysTab",true);
		var recordid=Xrm.Page.data.entity.getId();
		var userName=Xrm.Page.context.getUserName();
		Xrm.Page.getControl("IFRAME_Bcmcgrid").setSrc("http://crm-app-01:8092/RoomBlocks/Dynamicgridview.aspx?recordid="+recordid + '&username=' + userName);
		//alert("http://crm-app-01:8092/RoomBlocks/Dynamicgridview.aspx?recordid="+recordid);
	}	else {
		setVisibility("Tab","RoomBlockDaysTab",false);
	}
	
}

function SetActualizedDates() {
	
	var preArrival = Xrm.Page.getAttribute("bcmc_actualizeddayspriorevent");
	if (preArrival.getValue() != null) {
		
		var arrival = Xrm.Page.getAttribute("new_arrivaldate").getValue();
		var actualArrival = new Date(arrival);
		actualArrival.setDate(arrival.getDate() - preArrival.getValue()); 
	
		Xrm.Page.getAttribute("bcmc_actualizedarrivaldate").setValue(actualArrival);
		ForceSubmit("bcmc_actualizedarrivaldate",true);
	}

	var postDepart = Xrm.Page.getAttribute("bcmc_actualizeddayspostevent");
	if (postDepart.getValue() != null) {

		var departure = Xrm.Page.getAttribute("new_departuredate").getValue();
		var actualDeparture = new Date(departure);
		actualDeparture.setDate(departure.getDate() + postDepart.getValue()); 

		ForceSubmit("bcmc_actualizeddeparturedate",true);
		Xrm.Page.getAttribute("bcmc_actualizeddeparturedate").setValue(actualDeparture);

	}
	
}	


/********************* CUSTOM FUNCTIONS END ************************/
/*******************************************************************/
/******************** FORM FUNCTIONS START *************************/

function AllFormLoad() {
	/* Call functions common to read, write, bulk and update forms EXCEPT quick and bulk */
	//Xrm.Page.ui.tabs.get(4).setVisible(false);
	//Xrm.Page.ui.tabs.get(5).setVisible(false);

	//HideEventMenus();
	//attachEvent("onresize",HideEventMenus );

	//Xrm.Page.getControl("IFRAME_EventHistory").setSrc(GetFrameSource("Opportunity_New_EventSites"));

	//HideAssociatedViewButtons('bcmc_opportunity_hotellead', ['Add existing Hotel Lead to this record', 'Add a new Hotel Lead to this record'], true);

	// Save the arrival and departure dates to be used when validating them.
	_arrivalDate = Xrm.Page.getAttribute("new_arrivaldate").getValue();
	_departureDate = Xrm.Page.getAttribute("new_departuredate").getValue();	

	if (cwGetValue("statecode") == 2) //lost
		setVisibility("Control","bcmc_secondarylossreason",true);	
	
}

function CreateFormLoad() {
	AllFormLoad();
	Xrm.Page.getControl("IFRAME_Bcmcgrid").setSrc("about:blank");

    // If "Housing Company" lookup has been defaulted though a mapping
    if(Xrm.Page.getAttribute("bcmc_housingcompanyid").getValue() != null)
    {
        // Clear the default value of the "Housing Company" lookup
        Xrm.Page.getAttribute("bcmc_housingcompanyid").setValue(null);
    }	
	Xrm.Page.getAttribute("ownerid").setValue(null);

}

function UpdateFormLoad() {
	AllFormLoad();
	Status_OnChange();
	EventActualized_Onchange();
	SetActualizedDates();
	setTimeout("RoomBlockGrid()", 1000); 

}

function ReadFormLoad() {
	UpdateFormLoad();
}

function DisabledFormLoad() {
	ReadFormLoad();
}

function QuickFormLoad() {
}

function BulkFormLoad() {
}

function Form_onsave(executionObj) {

    if (_salesprocessupdated) { // prevent autosave
		var eventArgs = executionObj.getEventArgs();
		if (eventArgs.getSaveMode() == 70) {
			eventArgs.preventDefault();
		}
	}	


	if (!validateDates(executionObj)) {
		executionObj.getEventArgs().preventDefault();
		return false;
	}
	
	var arr = Xrm.Page.getAttribute("new_arrivaldate").getValue();
	var dep = Xrm.Page.getAttribute("new_departuredate").getValue();
	if (arr != null && dep == null) {
		alert("If you specify an arrival date you must also specify a departure date");
		Xrm.Page.getControl("new_departuredate").setFocus();
		executionObj.getEventArgs().preventDefault();
		return false;
	}
	if (arr == null && dep != null) {
		alert("If you specify a departure date you must also specify an arrival date");
		Xrm.Page.getControl("new_arrivaldate").setFocus();
		executionObj.getEventArgs().preventDefault();
		return false;
	}
	setTimeout("RoomBlockGrid()", 1000); 
	SetActualizedDates();
	return true;
}




function Form_onload() {

	GlobalVariables();
	SystemLoad();
	
	switch (Xrm.Page.ui.getFormType())
	{
	  case CRM_FORM_TYPE_CREATE:
		CreateFormLoad();
		break;

	  case CRM_FORM_TYPE_UPDATE:
		UpdateFormLoad();
		break;

	  case CRM_FORM_TYPE_READ:
		ReadFormLoad();
		break;

	  case CRM_FORM_TYPE_DISABLED:
		DisabledFormLoad();
		break;

	  case CRM_FORM_TYPE_QUICK:
		QuickFormLoad();
		break;

	  case CRM_FORM_TYPE_BULK:
		BulkFormLoad();
		break;


	}
}

/*********************** FORM FUNCTIONS END ************************/
/*******************************************************************/
