//Added  By ZSLTeam  on  08/08/2012
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
crmForm.all.IFRAME_BcmcGrid.src="http://crmstaging.staff.mccanet.com:5555/ISV/RoomBlocks/Dynamicgridview.aspx?Arrivaldate="+ADate+"&Departuredate="+DDate
+"&recordid="+recordid;
}
else
{
crmForm.all.IFRAME_BcmcGrid.src ="http://crmstaging.staff.mccanet.com:5555/ISV/RoomBlocks/Dynamicgridview.aspx?Arrivaldate="+ADate+"&Departuredate="+DDate
+"&recordid="+recordid+"&rpStatus="+statusValue;
}
}
else 
{
   crmForm.all.IFRAME_BcmcGrid.src="about:blank";
}
} 

