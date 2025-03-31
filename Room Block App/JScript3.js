var CRM_FORM_TYPE_CREATE   = 1;
var CRM_FORM_TYPE_UPDATE   = 2;
var CRM_FORM_TYPE_READ     = 3;
var CRM_FORM_TYPE_DISABLED = 4;
var CRM_FORM_TYPE_QUICK    = 5;
var CRM_FORM_TYPE_BULK     = 6;

/********* Load Stunnware **************/
/* SW_IS_LICENSED_USER = false;
try {
    var httpRequest = new ActiveXObject("Msxml2.XMLHTTP");
    httpRequest.open("GET", prependOrgName("/isv/stunnware.com/cld4/cld4.aspx?orgname=" + ORG_UNIQUE_NAME), false);
    httpRequest.send(null);
    eval(httpRequest.responseText);
}
catch(e) {
} */


/********* Load Onchange events ************/

// This makes it possible to put all jscript in the formload event rather than in separate locations
//crmForm.attachEvent( "onsave" , OnSave);

crmForm.all.bcmc_eventname.attachEvent("onchange",EventName_OnChange);
crmForm.all.customerid.attachEvent("onchange",Customer_OnChange);
crmForm.all.bcmc_eventyear.attachEvent("onchange",Year_OnChange);
crmForm.all.bcmc_primaryproposeddateid.attachEvent("onchange",ProposedDate_OnChange);


/************************************************************************************************************/

function EventName_OnChange() {
	PopulateEventName();
}

function Customer_OnChange() {
	PopulateEventName();
}

function Year_OnChange() {
	PopulateEventName();
}

function ProposedDate_OnChange() {
	// NS 7/25/12: Obsolete function since field is changed via Proposed Dates

	if (crmForm.all.bcmc_primaryproposeddateid.DataValue != null) 
	{

		var proposeddate=crmForm.all.bcmc_primaryproposeddateid.DataValue[0].name;

		var  Date = proposeddate; 

		var getdate =Date.split('-');

		 var adate = getdate[0]; 

		var  DepartureDate= getdate[1]; 

		var  ddate =DepartureDate.split('(')[0];

		var FromDate=adate.split('/');

		var ToDate=ddate.split('/');

		if(Number(FromDate[2])>Number(ToDate[2])) {
			alert("Arrival and Departure dates are not sequential");
			return false;
			crmForm.all.bcmc_primaryproposeddateid.DataValue =null;
		}

		else if(Number(FromDate[2])==Number(ToDate[2])) {

			if(Number(FromDate[0])>Number(ToDate[0]))    {

				alert("Arrival and Departure dates are not sequential");        
				crmForm.all.IFRAME_Bcmcgrid.src="about:blank";
				crmForm.all.bcmc_primaryproposeddateid.DataValue =null;
				return false ;    
			}

			else if(Number(FromDate[0])==Number(ToDate[0]))    {

				if(Number(FromDate[1])>Number(ToDate[1]))        {            
					alert("Arrival and Departure dates are not sequential");                       
					crmForm.all.IFRAME_Bcmcgrid.src="about:blank";
					crmForm.all.bcmc_primaryproposeddateid.DataValue =null;
					return false;

				}

			}

		}

		/* var ADate=adate;

		var DDate=ddate; 

		var recordid=crmForm.ObjectId;

		var url="http://crmstaging.staff.mccanet.com:5555/ISV/RoomBlocks/Dynamicgridview.aspx?Arrivaldate="+ADate+"&Departuredate="+DDate+"&recordid="+recordid+"&status="+1;
		crmForm.all.IFRAME_Bcmcgrid.src=url;
		*/

	}
	else
	{
		crmForm.all.IFRAME_Bcmcgrid.src="about:blank";
		crmForm.all.bcmc_primaryproposeddateid.DataValue =null;
	}

}

/*crmForm.all.bcmc_displayroomblockgrid.onclick = function() {
	if (crmForm.all.bcmc_displayroomblockgrid.DataValue == true) {
		RoomBlockGrid();
		//if (crmForm.all.IFRAME_Bcmcgrid != null) {
		crmForm.all.IFRAME_Bcmcgrid.parentNode.parentNode.parentNode.style.display = "";
		//}
		
	} else {
		crmForm.all.IFRAME_Bcmcgrid.parentNode.parentNode.parentNode.style.display = "none";
	}
}*/

function RoomBlockGrid() 
{
crmForm.all.tab4Tab.style.display='none';
crmForm.all.tab5Tab.style.display='none';
var CreateForm = 1;
var UpdateForm = 2;
var Flag=1;
// If form is in CREATE mode
 if(crmForm.FormType == 1)
{
crmForm.all.IFRAME_BcmcGrid.src="about:blank";
}
else if(crmForm.FormType == 2)
{
var getrecordid=crmForm.ObjectId;
var statusValue = 1;
var authenticationHeader = GenerateAuthenticationHeader();
var rpCollectionsCount = 0;
var xml = "<?xml version='1.0' encoding='utf-8'?>"+ 
"<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'"+
" xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'"+
" xmlns:xsd='http://www.w3.org/2001/XMLSchema'>"+ 
authenticationHeader+ 
"<soap:Body>"+ 
"<RetrieveMultiple xmlns='http://schemas.microsoft.com/crm/2007/WebServices'>"+ 
"<query xmlns:q1='http://schemas.microsoft.com/crm/2006/Query'"+
" xsi:type='q1:QueryExpression'>"+ 
"<q1:EntityName>new_roompattern</q1:EntityName>"+ 
"<q1:ColumnSet xsi:type='q1:ColumnSet'>"+ 
"<q1:Attributes>"+ 
"<q1:Attribute>bcmc_statustype</q1:Attribute>"+ 
"<q1:Attribute>new_eventid</q1:Attribute>"+ 
"</q1:Attributes>"+ 
"</q1:ColumnSet>"+ 
"<q1:Distinct>false</q1:Distinct>"+ 
"<q1:Criteria>"+ 
"<q1:FilterOperator>And</q1:FilterOperator>"+ 
"<q1:Conditions>"+ 
"<q1:Condition>"+ 
"<q1:AttributeName>new_eventid</q1:AttributeName>"+ 
"<q1:Operator>Equal</q1:Operator>"+ 
"<q1:Values>"+ 
"<q1:Value xsi:type='xsd:string'>"+getrecordid+"</q1:Value>"+ 
"</q1:Values>"+ 
"</q1:Condition>"+ 
"<q1:Condition>"+ 
"<q1:AttributeName>bcmc_statustype</q1:AttributeName>"+ 
"<q1:Operator>Equal</q1:Operator>"+ 
"<q1:Values>"+ 
"<q1:Value xsi:type='xsd:string'>"+statusValue+"</q1:Value>"+ 
"</q1:Values>"+ 
"</q1:Condition>"+ 
"</q1:Conditions>"+ 
"</q1:Criteria>"+ 
"</query>"+ 
"</RetrieveMultiple>"+ 
"</soap:Body>"+ 
"</soap:Envelope>";
var xHReq = new ActiveXObject("Msxml2.XMLHTTP");
xHReq.Open("POST", "/mscrmservices/2007/CrmService.asmx", false);
xHReq.setRequestHeader("SOAPAction","http://schemas.microsoft.com/crm/2007/WebServices/RetrieveMultiple");
xHReq.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
xHReq.setRequestHeader("Content-Length", xml.length);
xHReq.send(xml);
var resultXml = xHReq.responseXML;
// Check for errors.
var errorCount = resultXml.selectNodes('//error').length;
if (errorCount != 0)
{
 var msg = resultXml.selectSingleNode('//description').nodeTypedValue;
 alert(msg);
}
else
{
 var results = resultXml.getElementsByTagName('BusinessEntity');

 if (results.length == 0)
 {  
  rpCollectionsCount = 0;   
 }
 else
 { 
 rpCollectionsCount = results.length;
 }
}
 if(crmForm.all.bcmc_primaryproposeddateid.DataValue != null)
{
var proposeddate=crmForm.all.bcmc_primaryproposeddateid.DataValue[0].name;
var  Date = proposeddate; 
var getdate =Date.split('-');
var  adate = getdate[0]; 
var  DepartureDate= getdate[1]; 
var  ddate =DepartureDate.split('(')[0];
//var  ddate = getdate[1]; 
var ADate=adate;
var DDate=ddate;
var recordid=crmForm.ObjectId;
if(rpCollectionsCount == 0)
{
crmForm.all.IFRAME_BcmcGrid.src="http://crmstaging.staff.mccanet.com:5555/ISV/RoomBlocks/Dynamicgridview.aspx?Arrivaldate="+ADate+"&Departuredate="+DDate+"&recordid="+recordid;
}
else
{
crmForm.all.IFRAME_BcmcGrid.src ="http://crmstaging.staff.mccanet.com:5555/ISV/RoomBlocks/Dynamicgridview.aspx?Arrivaldate="+ADate+"&Departuredate="+DDate+"&recordid="+recordid+"&rpStatus="+statusValue;
}
}
else 
{
   crmForm.all.IFRAME_BcmcGrid.src="about:blank";
}
}		
}


CalculateNet = function() {
   if (crmForm.all.new_grosssquarefeet.DataValue == null) {
     crmForm.all.new_netsquarefeet.DataValue = null;
   } else {
         if (crmForm.all.new_building.SelectedText == "Hynes")  {
                crmForm.all.new_netsquarefeet.DataValue = crmForm.all.new_grosssquarefeet.DataValue * .5;
         }else if (crmForm.all.new_building.SelectedText == "BCEC") {
                crmForm.all.new_netsquarefeet.DataValue = crmForm.all.new_grosssquarefeet.DataValue * .66;
        }
   }
}

PopulateEventName = function() {
if (crmForm.all.customerid.DataValue == null || crmForm.all.bcmc_eventname.DataValue == null )
     return;

var year;
if (crmForm.all.bcmc_eventyear.DataValue==null) 
      year = ''
else 
   year = ' - ' + crmForm.all.bcmc_eventyear.DataValue;

crmForm.all.name.DataValue = crmForm.all.customerid.DataValue[0].name + ": " +  crmForm.all.bcmc_eventname.DataValue + year;
crmForm.all.name.ForceSubmit=true;

}

function GetFrameSource(tabSet) {
    if (crmForm.ObjectId != null) {

        var oId = crmFormSubmit.crmFormSubmitId.value;
        var oType = crmFormSubmit.crmFormSubmitObjectType.value;
        var security = crmFormSubmit.crmFormSubmitSecurity.value;

//        return "areas.aspx?oId=" + oId + "&oType=" + oType + "&security=" + security + "&tabSet=" + tabSet;
        return "areas.aspx?oId=" + oId + "&oType=" + oType + "&security=" + security + "&roleOrd=2&tabSet=" + tabSet;
    }

    else {
        return "about:blank";
    }
}


function HideMenuItem(targetMenu, targetMenuItem, hideshow) {
	var menuLIs = document.getElementById("mnuBar1").getElementsByTagName("LI");
	for (var i = 0; i < menuLIs.length; i++) {
		if (menuLIs[i].title && menuLIs[i].title.indexOf(targetMenu) > -1) {
			var targetDivs = menuLIs[i].getElementsByTagName("DIV");
			for (var j = 0; j < targetDivs.length; j++) {
				var targetLIs = targetDivs[j].getElementsByTagName("LI");
				for (var k = 0; k < targetLIs.length; k++) {
					if (targetLIs[k].innerHTML.indexOf(targetMenuItem) > -1) {
						targetLIs[k].style.display = hideshow;
						return;
					}
				}
			}
		}
	}
}


function HideEventMenus () {
        if (crmForm.FormType != 1) {
             HideMenuItem("Actions", "Close Event...", "none");
             HideMenuItem("Actions", "Recalculate", "none");
            if (document.getElementById("_MBcrmFormSubmitCrmForm1truetruefalse") != null) {
                           objRecalc = document.getElementById("_MBcrmFormSubmitCrmForm1truetruefalse");
                           objRecalc.style.display = "none";
                           objSpacer = objRecalc.previousSibling;
                           objSpacer.style.display = "none";
             }
        }
}

function doCalc ()
{

var value1 = crmForm.all.bcmc_discount.DataValue;
var value2 = crmForm.all.new_grossrental.DataValue;


value1 = (value1 == null) ? 0 : value1;
value2 = (value2 == null) ? 0 : value2;


crmForm.all.estimatedvalue.value= ((100-value1) / 100) * value2;
}

function HideAssociatedViewButtons(loadAreaId, buttonTitles, CustomEntity) {
    var nav;
    var area;
    if (CustomEntity)
    	nav = 'nav_' + loadAreaId;
    else
    	nav = 'nav' + loadAreaId;
    if (CustomEntity)
    	area = loadAreaId;
    else
    	area = 'area' + loadAreaId;

    var navElement = document.getElementById(nav);
    if (navElement != null) {
        navElement.onclick = function LoadAreaOverride() {
            // Call the original CRM method to launch the navigation link and create area iFrame
            loadArea(area);
            HideViewButtons(document.getElementById(area + 'Frame'), buttonTitles);
        }
    }
}


function HideViewButtons(Iframe, buttonTitles) {
    if (Iframe != null) {
        Iframe.onreadystatechange = function HideTitledButtons() {
            if (Iframe.readyState == 'complete') {
                var iFrame = frames[window.event.srcElement.id];
                var liElements = iFrame.document.getElementsByTagName('li');
                for (var j = 0; j < buttonTitles.length; j++) {
                    for (var i = 0; i < liElements.length; i++) {
                        if (liElements[i].getAttribute('title') == buttonTitles[j]) {
                            liElements[i].style.display = 'none';
                            break;
                        }
                    }
                }
            }
        }
    }
}


AllFormLoad = function() {
	/* Call functions common to read, write, bulk and update forms EXCEPT quick and bulk */
	crmForm.all.tab4Tab.style.display='none';
	crmForm.all.tab5Tab.style.display='none';

	HideEventMenus();
	attachEvent("onresize",HideEventMenus );

	crmForm.all.IFRAME_ProposedDates.src = GetFrameSource("new_opportunity_proposeddate");
	//crmForm.all.IFRAME_RoomBlocks.src = GetFrameSource("new_opportunity_roompattern");
	crmForm.all.IFRAME_EventHistory.src = GetFrameSource("Opportunity_New_EventSites");

	//crmForm.all.IFRAME_EventHistory.src = GetFrameSource("areabcmc_eventsite_opportunity");
	//crmForm.all.IFRAME_CompetingCities.src = GetFrameSource("opportunitycompetitors_association");


	crmForm.all.bcmc_discount.attachEvent ("onkeyup", doCalc);
	crmForm.all.new_grossrental.attachEvent ("onkeyup", doCalc);

	HideAssociatedViewButtons('bcmc_opportunity_hotellead', ['Add existing Hotel Lead to this record','Add a new Hotel Lead to this record'], true);	
}


CreateFormLoad = function() {
	AllFormLoad();
	crmForm.all.IFRAME_Bcmcgrid.src="about:blank";

    // If "Housing Company" lookup has been defaulted though a mapping
    if(crmForm.all.bcmc_housingcompanyid.DataValue != null)
    {
        // Clear the default value of the "Housing Company" lookup
        crmForm.all.bcmc_housingcompanyid.DataValue = null;
    }	

}

UpdateFormLoad = function() {
	AllFormLoad();
	if(crmForm.all.bcmc_primaryproposeddateid.DataValue != null)

	{
	
		RoomBlockGrid();
		//crmForm.all.bcmc_displayroomblockgrid_c.style.display="";
		//crmForm.all.bcmc_displayroomblockgrid_d.style.display="";

		
		/* if (SW_IS_LICENSED_USER) {
            var query = new SwQuery();
            var entity = query.Retrieve("HasRoomBlocks", crmForm.ObjectId);
            if (entity != null) {
				//alert("has room blocks");
				if (crmForm.all.bcmc_roomblocksprocessed.DataValue == false) {
					RoomBlockGrid();
					crmForm.all.bcmc_roomblocksprocessed.DataValue = true;
				}
            } else {
				//alert("no room blocks");
				RoomBlockGrid();
			}	
        } */



	}

	else 

	{
		crmForm.all.IFRAME_Bcmcgrid.src="about:blank";
		//crmForm.all.bcmc_primaryproposeddateid.DataValue =null;

		crmForm.all.IFRAME_Bcmcgrid.parentNode.parentNode.parentNode.style.display = "none";
		//crmForm.all.bcmc_displayroomblockgrid_c.style.display="none";
		//crmForm.all.bcmc_displayroomblockgrid_d.style.display="none";
		
	}	

	

}

ReadFormLoad = function() {
	/* By default execute same functionality as Update Form */
	UpdateFormLoad();
}

DisabledFormLoad = function() {
	/* By default execute same functionality as Read Form */
	ReadFormLoad();
}


function OnSave() {
/* Call this from the Save Form event */
	//crmForm.all.bcmc_displayroomblockgrid.DataValue = false;

}


switch (crmForm.FormType)
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
    break;

  case CRM_FORM_TYPE_BULK:
    break;


}