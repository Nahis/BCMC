<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CloneOpportunity.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <base target="_self" />
    <title></title>

    <script language="javascript" type="text/javascript">
    	var opener = window.dialogArguments;
    	/* Handling if called from regular window
		 */

    	var ids = '<%=Request.QueryString["id"] %>';
        var grid = '<%=Request.QueryString["grid"] %>';
        var orgname = '<%=Request.QueryString["orgname"] %>';
        var typename = 'opportunity';

        function ShowProcessing() {
            document.getElementById('divLoading').style.display = 'block';
            return true;
        }

        function getInvocationType() {
        	/*
			if (grid == 1) {
                getSelected();
            }
            else {
                getObjectId();
            }
			*/

            frmPassGuids.action = "CloneOpportunity.aspx?orgname=" + orgname + "&typename=" + typename + "&id="+ids;
            frmPassGuids.method = "post";
            frmPassGuids.submit();
        }

        function getObjectId() {
        	var allGuId = opener.document.crmForm.ObjectId;
            var Id = allGuId.substr(1, 36);
            var tempGuid = Id.replace(/{/g, "");
            allGuId = tempGuid.replace(/}/g, "");
            document.getElementById("<%=hdnallGuids.ClientID %>").value = allGuId;
        }

        function getSelected() {
            allGuId = "";

            allGuId = String(opener);
            if (allGuId == "") {
                alert("No Records Selected\nPlease select any Event(s) for Cloning")
                window.close();
                return false;
            }

            var tempGuid = allGuId.replace(/{/g, "");
            allGuId = tempGuid.replace(/}/g, "");
            //alert(allGuId);
            document.getElementById("<%=hdnallGuids.ClientID %>").value = allGuId;
        }
    </script>
</head>
<body onload="getInvocationType();">
    <form id="frmPassGuids">
    <input id="hdnallGuids" runat="server" type="hidden" />
    
    </form>
</body>
</html>
