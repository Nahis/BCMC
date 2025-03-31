/* Library:

	cwGetAttr(attr);
	
	cwGetValue("fieldname");
	
	cwGetDate("fieldname", "/", "mmddyyyy")
	
	cwSetValue("fieldname", value);
	
	cwGetLookup("fieldname");
	
	cwSetLookupValue("fieldname", entityData.AttId.Id, entityData.AttId.Name, "account") 
	
	cwSetReadOnly("fieldname", true);
	
	cwSetReq("fieldname", true);
	
	RegisterOnClickEvent("attribname","functionname");

	RegisterOnDoubleClickEvent("attribname","functionname");

	setVisibility("Tab","MPMFunds",false);

	setVisibility("Section","BV5Section",false);

	setVisibility("Control","tickersymbol",false);

	setVisibility("Navigation","navLink{76b3c241-c9e4-ca18-9244-832797535061}",false);

	retrieveEntityById(entity,entityid[0].id,fields,actionFunction) - Usage Sample:
		
				//must have JSON and JQuery web resources loaded
				function retrieveSample() {

					//Update next 3 lines. NB: Field names in select are case sensitive and must adhere to the schema name
					var entity = "Product";
					var entityid = Xrm.Page.getAttribute("productid").getValue();
					var fields = "?$select=ProductId,DefaultUoMId,ProductNumber"
					
					if (entityid != null) {
						entityData = retrieveEntityById(entity,entityid[0].id,fields,actionFunction);
					}
				}

				function actionFunction(entityData) {
					if (entityData != null) {  
						Xrm.Page.getAttribute("address1_stateorprovince").setValue(entityData.snt_State);
						Xrm.Page.getAttribute("address1_city").setValue(entityData.snt_City);
						Xrm.Page.getAttribute("address1_postalcode").setValue(entityData.snt_name);
						Xrm.Page.getAttribute("address1_postalcode").setValue(entityData.snt_num.Value);
						Xrm.Page.getAttribute("address1_postalcode").setValue(entityData.snt_entityid.Id);
					}	
				}
				
	function retrieveEntityByIdSync(entity,entityid,fields)    - same as above except syncronous - usage sample:

				var e1 = retrieveEntityByIdSync("SystemUser",Xrm.Page.context.getUserId(),"?$select=FirstName,LastName");
				alert(e1.FirstName);
				alert(e1.LastName);	
	
				
	function retrieveMultiple(entity,filter,fields,functionToCall) - Usage Sample:

				//must have JSON and JQuery web resources loaded (not tested)
				function retrieveSample() {

					//Update next 5 lines. NB: Field names in select are case sensitive and must adhere to the schema name
					var entity = "mpm_contact_systemuser";
					var entityid = Xrm.Page.getAttribute("contactid").getValue();
					var systemuserid = Xrm.Page.context.getUserId();
					var filter = "contactid eq guid'" + entityid + "'" + " and "+  "systemuserid eq guid'" + systemuserid[0].id + "'";
					var fields = "?$select=systemuserid";
					
					if (entityid != null) {
						entityData = retrieveMultiple(entity,filter,fields,actionFunction);
					}
				}

				function actionFunction(entityData) {
					for( i=0; i< entityData.length; i++)  {     
						var entity = entityData[i];     
						var accountNumberAttribute = entity.AccountNumber;              
					}
				}	

	function retrieveMultipleSync(entity,filter,fields)  - same as above except syncronous - usage sample:
	
				var entity = "snt_contact_systemuser";
				var entityid = Xrm.Page.data.entity.getId();
				var filter = "contactid eq guid'" + entityid + "'" + " and "+  "systemuserid eq guid'" + Xrm.Page.context.getUserId() + "'";
				var fields = "?$select=systemuserid";
					
				var entityData = retrieveMultipleSync(entity,filter,fields);
				if (entityData.length > 0)
					return false;
				else 
					return true;
	
				
	RetrieveOptionSetLabel("entity","controlsetfieldname",entityData.controlsetfieldname.Value,"targetassignfieldname");
				
	UserHasRole("RoleName");
	
	ForceSubmit("snt_name", true);
	

*/

function cwGetAttr(attr) {

	try {
		return Xrm.Page.getAttribute(attr);
	} catch (e) {
		alert("cwGetAttr: " + e.message);
	}	
}

function cwGetValue(attr) {

	try {
		return Xrm.Page.getAttribute(attr).getValue();
	} catch (e) {
		alert("cwGetValue: " + e.message);
	}	
}

function cwSetValue(attr, val) {
	try {
		Xrm.Page.getAttribute(attr).setValue(val);
	} catch (e) {
		alert("cwSetValue: attr/val = " + attr + "/" + val + "\n\n" + e.message);
	}		
}

function cwGetLookup(attr) {

	try {
		var a = Xrm.Page.getAttribute(attr);
		if (a.getValue() != null) {
			return a.getValue()[0];  // .id and .name 
		} else {
			return null;
		}
	} catch (e) {
		alert("cwGetLookup: attr = " + attr + "\n\n" + e.message);
	}		
	
}

function cwSetLookupValue(attr, id, name, entity) {
	try {
		var lookupData = new Array();
		var lookupItem= new Object();
		
		if (id != null) {
			lookupItem.id = id;
			lookupItem.name = name;
			lookupItem.typename = entity;
			lookupData[0] = lookupItem;
			Xrm.Page.getAttribute(attr).setValue(lookupData);
		} else {
			Xrm.Page.getAttribute(attr).setValue(null);
		}
	} catch (e) {
		alert("cwSetLookupValue: attr/name = " + attr + "/" + name + "\n\n" + e.message);
	}		
}			

function cwGetDate(attr, separator, type) {
	try {
		var d = Xrm.Page.getAttribute(attr).getValue();
		var ret = null;
		if (d != null) {
			if (type == "mmddyyyy") {
				var curr_date = d.getDate();
				var curr_month = d.getMonth();
				curr_month++;  // getMonth() considers Jan month 0, need to add 1
				var curr_year = d.getFullYear();
				ret = curr_month + separator + curr_date + separator + curr_year;
			}	
		}
		return ret;
	} catch (e) {
		alert("cwGetDate: attr = " + attr + "\n\n" + e.message);
	}		
}

function cwSetReadOnly(attr, ro) {
	try {
		Xrm.Page.getControl(attr).setDisabled(ro);
	} catch (e) {
		alert("cwSetReadOnly: attr = " + attr + "\n\n" + e.message);
	}		
}

function cwSetReq(attr, req) {
	try {
		var level = "none";
		if (req == true)
			level = "required";
		
		Xrm.Page.getAttribute(attr).setRequiredLevel(level);
	} catch (e) {
		alert("cwSetReq: " + e.message);
	}		
		
}	

function RegisterOnClickEvent(attr, fn) {
	var e = document.getElementById(attr);  
    var f = "var click=function() { " +
              "var a = Xrm.Page.data.entity.attributes.get(attr); " +
              "a.setValue(!a.getValue()); " + fn + "(); " + 
              " };";			
  
    eval(f);  
 
    // Attach to click event  
    e.attachEvent("onclick", click, false);  
}

function RegisterOnDoubleClickEvent(attr, fn) {
	var e = document.getElementById(attr);  
    var f = "var doubleclick=function() { " +  
              fn + "(); " +  
			" };";  
  
    eval(f);  
 
    // Attach to double click event  
    e.attachEvent("ondblclick", doubleclick, false);  
}


function setVisibility(objecttype,objectname,show) {  
    switch (objecttype.toLowerCase().charAt(0)) {  
        //tab  
        case 't': setTabVisibility(objectname,show);  
            break;  
        //section  
        case 's': setSectionVisibility(objectname,show);  
            break;  
        //control  
        case 'c': setControlVisibility(objectname,show);  
            break;  
        //navigation  
        case 'n': setNavigationVisibility(objectname,show);  
            break;  
    }  
}  
  
function setNavigationVisibility(navitemname,show) {  
    var navitem = Xrm.Page.ui.navigation.items.get(navitemname);  
    if (navitem == null)  
    {  
        return;  
    }  
    navitem.setVisible(show);  
}  
  
function setTabVisibility(tabname,show) {  
    var tab = Xrm.Page.ui.tabs.get(tabname);  
    if (tab == null)  
    {  
        return;  
    }  
    tab.setVisible(show);  
}  
  
function setSectionVisibility(sectionname,show) {  
    var tabs = Xrm.Page.ui.tabs.get();  
    for(var i in tabs) {  
        var tab = tabs[i];  
        var section = tab.sections.get(sectionname);  
		if (section != null) {  
            section.setVisible(show);  
            return;  
        }  		
    }  
}  
  
function setControlVisibility(controlname,show) {  
    var control = Xrm.Page.ui.controls.get(controlname);  
    if (control == null)  
    {  
        return;  
    }  
    control.setVisible(show);  
}  

function retrieveEntityById(entity,entityid,fields,functionToCall) {

    try {
		var context = Xrm.Page.context;
		var serverUrl = context.getServerUrl();
		//var serverUrl = document.location.protocol + "//" + document.location.host + "/" + context.getOrgUniqueName();
		var oDataSelect;
		// build query string
		oDataSelect = "/XRMServices/2011/OrganizationData.svc/" + entity + "Set(guid'" + entityid + "')" + fields + "";

		$.ajax({
		
			type: "GET",
			contentType: "application/json; charset=utf-8",
			datatype: "json",
			url: serverUrl + oDataSelect,
			beforeSend: function (XMLHttpRequest) { XMLHttpRequest.setRequestHeader("Accept", "application/json"); },
			success: function (data, textStatus, XmlHttpRequest) {
				functionToCall(data.d);
			},
			error: function (xmlHttpRequest, textStatus, errorThrown) {
				alert("Status: " + textStatus + "; ErrorThrown: " + errorThrown + "; oData: " + serverUrl + oDataSelect);
			}
		});
	} catch (e) {
		alert("RetrieveEntityByID failed to return results - ensure JSON/Jquery libraries loaded- " + e.message);
	}
		
}

function retrieveEntityByIdSync(entity,entityid,fields) {

    try {
		var context = Xrm.Page.context;
		var serverUrl = context.getServerUrl();  // Note the removal of the "/" in oDataSelect clause
		//var serverUrl = document.location.protocol + "//" + document.location.host + "/" + context.getOrgUniqueName();
		var query = new XMLHttpRequest();
		var oDataSelect = serverUrl + "/XRMServices/2011/OrganizationData.svc/" + entity + "Set(guid'" + entityid + "')" + fields + "";
		query.open('GET', oDataSelect, false);
		query.setRequestHeader("Accept", "application/json");
		query.setRequestHeader("Content-Type", "application/json; charset=utf-8");
		query.send(null);
		return JSON.parse(query.responseText).d;
	} catch (e) {
		alert("RetrieveEntityByIDSync failed to return results - ensure JSON/Jquery libraries loaded- " + e.message);
	}

}

function retrieveMultiple(entity,filter,fields,functionToCall) {

	try {
		var context = Xrm.Page.context;
		var serverUrl = context.getServerUrl();
		//var serverUrl = document.location.protocol + "//" + document.location.host + "/" + context.getOrgUniqueName();
		var oDataSelect;
		// build query string
		oDataSelect = "/XRMServices/2011/OrganizationData.svc/" + entity + "Set"+fields+"&$filter="+filter;

		$.ajax({
		
			type: "GET",
			contentType: "application/json; charset=utf-8",
			datatype: "json",
			url: serverUrl + oDataSelect,
			beforeSend: function (XMLHttpRequest) { XMLHttpRequest.setRequestHeader("Accept", "application/json"); },
			success: function (data, textStatus, XmlHttpRequest) {
				functionToCall(data.d.results);
			},
			error: function (xmlHttpRequest, textStatus, errorThrown) {
				alert("Status: " + textStatus + "; ErrorThrown: " + errorThrown);
			}
		});
	} catch (e) {
		alert("retrieveMultiple failed to return results - ensure JSON/Jquery libraries loaded- " + e.message);
	}		
}

function retrieveMultipleSync(entity,filter,fields) {

    try {
		var context = Xrm.Page.context;
		var serverUrl = context.getServerUrl();
		//var serverUrl = document.location.protocol + "//" + document.location.host + "/" + context.getOrgUniqueName();
		var query = new XMLHttpRequest();
		var oDataSelect = serverUrl + "/XRMServices/2011/OrganizationData.svc/" + entity + "Set"+fields+"&$filter="+filter;
		query.open('GET', oDataSelect, false);
		query.setRequestHeader("Accept", "application/json");
		query.setRequestHeader("Content-Type", "application/json; charset=utf-8");
		query.send(null);
		return JSON.parse(query.responseText).d.results;
	} catch (e) {
		alert("Retrieve multiple failed to return results - ensure JSON/Jquery libraries loaded- " + e.message);
	}

}

function UserHasRole(roleName)
{
    var context = Xrm.Page.context;
	var serverUrl = Xrm.Page.context.getServerUrl();
	//var serverUrl = document.location.protocol + "//" + document.location.host + "/" + context.getOrgUniqueName();

    var oDataEndpointUrl = serverUrl + "/XRMServices/2011/OrganizationData.svc/";
    oDataEndpointUrl += "RoleSet?$top=1&$filter=Name eq '" + roleName + "'";

    var service = GetRequestObject();

    if (service != null)
    {
        service.open("GET", oDataEndpointUrl, false);
        service.setRequestHeader("X-Requested-Width", "XMLHttpRequest");
        service.setRequestHeader("Accept", "application/json, text/javascript, */*");
        service.send(null);

        var requestResults = eval('(' + service.responseText + ')').d;

        if (requestResults != null && requestResults.results.length == 1)
        {
            var role = requestResults.results[0]; 

            var id = role.RoleId;

            var currentUserRoles = Xrm.Page.context.getUserRoles();
			

            for (var i = 0; i < currentUserRoles.length; i++)
            {
                var userRole = currentUserRoles[i];

                if (GuidsAreEqual(userRole, id))
                {
					return true
                }
            }
        }
    }

    return false;
}

function GetRequestObject()
{
    if (window.XMLHttpRequest)
    {
        return new window.XMLHttpRequest;
    }
    else
    {
        try
        {
            return new ActiveXObject("MSXML2.XMLHTTP.3.0");
        }
        catch (ex)
        {
            return null;
        }
    }
}

function GuidsAreEqual(guid1, guid2)
{
    var isEqual = false;

    if (guid1 == null || guid2 == null)
    {
        isEqual = false;
    }
    else
    {
        isEqual = guid1.replace(/[{}]/g, "").toLowerCase() == guid2.replace(/[{}]/g, "").toLowerCase();
    }

    return isEqual;
}


function RetrieveOptionSetLabel(entity,attribute,optionvalue,assignattribute) {
        // Calling Metadata service to get Optionset Label
        SDK.MetaData.RetrieveEntityAsync(SDK.MetaData.EntityFilters.Attributes, entity, null, false, function (entityMetadata) { successRetrieveOptionSetLabel(entityMetadata, attribute, optionvalue, assignattribute); }, errorRetrieveOptionSetLabel);
}

function successRetrieveOptionSetLabel(entityMetadata, attribute, optionvalue, assignattribute) {

	var success = false;
    for (var i = 0; i < entityMetadata.Attributes.length; i++) {
        var AttributeMetadata = entityMetadata.Attributes[i];
        if (success) break;
        if (AttributeMetadata.SchemaName.toLowerCase() == attribute.toLowerCase()) {
            for (var o = 0; o < AttributeMetadata.OptionSet.Options.length; o++) {
                var option = AttributeMetadata.OptionSet.Options[o];
                if (option.OptionMetadata.Value == optionvalue) {
                    Xrm.Page.getAttribute(assignattribute).setValue(option.OptionMetadata.Label.UserLocalizedLabel.Label);
                    success = true;
                    break;
                }
            }
        }

    }


}

function errorRetrieveOptionSetLabel(XmlHttpRequest, textStatus, errorThrown) {
     alert(errorThrown);
}

function ForceSubmit(field, value) {
	try {
		var submitvalue = "never";
		if (value == true)
			submitvalue = "always";
		Xrm.Page.getAttribute(field).setSubmitMode("always");
	} catch (e) {
		alert("ForceSubmit: attr/val = " + field + "/" + value + "\n\n" + e.message);
	}		
}

function IsNumeric(sText) {
   var ValidChars = "0123456789";
   var IsNumber=true;
   var Char;
   
   for (i = 0; i < sText.length && IsNumber == true; i++) 
      { 
      Char = sText.charAt(i); 
      if (ValidChars.indexOf(Char) == -1) 
         {
         IsNumber = false;
         }
      }
   return IsNumber;
   
}

function PhoneNumberValidation(phone,phoneDesc) {
	ret = true;
	var phone1 = Xrm.Page.getAttribute(phone).getValue();
	var phone2 = phone1;
	
	if (phone1 == null)
		return true;
		
	// First trim the phone number
	var stripPhone = phone1.replace(/[^0-9]/g, '');

	if (!IsNumeric(stripPhone)) {
		alert("The " + phoneDesc + " you entered must be a numeric value. Please correct the entry.");
		Xrm.Page.ui.controls.get(phone).setFocus();
		ret = false;
	} else {
	
		if ( stripPhone.length < 10 ) {
			alert("The " + phoneDesc + " you entered must be at 10 digits. Please correct the entry.");
			Xrm.Page.ui.controls.get(phone).setFocus();
			ret = false;
		} else {
			if (stripPhone.length == 10) {
				phone2 = "(" + stripPhone.substring(0,3) + ") " + stripPhone.substring(3,6) + "-" + stripPhone.substring(6,10);
			} else {
				phone2 = stripPhone;
			}
		
		}
	}	
	
	Xrm.Page.getAttribute(phone).setValue(phone2);
	return ret;
}