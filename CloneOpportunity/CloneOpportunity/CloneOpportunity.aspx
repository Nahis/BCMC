<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CloneOpportunity.aspx.cs"
    Inherits="CloneOpportunity.CloneOpportunity" EnableViewState="true" 
    EnableEventValidation="false" 
    %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <base target="_self" />
    <title>Clone Opporunity...</title>
    <link rel="stylesheet" type="text/css" href="StyleSheet/CloneOpportunity.css"/>
	<link rel="stylesheet" type="text/css" href="StyleSheet/dialog.css" />
    <script language="javascript" type="text/javascript">

        function ShowProcessing() {
            document.getElementById('dvSpacerForImmediateClone').style.display = 'block'; //keep ok/cancel down
            document.getElementById('divLoading').style.display = 'block';
            document.getElementById('<%= btnCancel.ClientID %>').style.display = "none";
            document.getElementById('<%= btnClone.ClientID %>').style.display = "none";
            return true;
        }

        function winClose() {
            window.opener = null;
            window.close();
            return false;
        }

        //Check/Unchecks child nodes of on check/uncheck of parent node
        function Client_OnTreeNodeChecked() {
            var obj = window.event.srcElement;
            var treeNodeFound = false;
            var checkedState;
            if (obj.tagName == "INPUT" && obj.type == "checkbox") {
                var treeNode = obj;
                checkedState = treeNode.checked;
                do {
                    obj = obj.parentElement;
                } while (obj.tagName != "TABLE")
                var parentTreeLevel = obj.rows[0].cells.length;
                var parentTreeNode = obj.rows[0].cells[0];
                var tables = obj.parentElement.getElementsByTagName("TABLE");
                var numTables = tables.length
                if (numTables >= 1) {
                    for (i = 0; i < numTables; i++) {
                        if (tables[i] == obj) {
                            treeNodeFound = true;
                            i++;
                            if (i == numTables) {
                                return;
                            }
                        }
                        if (treeNodeFound == true) {
                            var childTreeLevel = tables[i].rows[0].cells.length;
                            if (childTreeLevel > parentTreeLevel) {
                                var cell = tables[i].rows[0].cells[childTreeLevel - 1];
                                var inputs = cell.getElementsByTagName("INPUT");
                                inputs[0].checked = checkedState;
                            }
                            else {
                                return;
                            }
                        }
                    }
                }
            }
        }
    </script>

	<style type="text/css">
		input[type=checkbox] {
			cursor: pointer;
		}
	</style>

</head>
<body>

    <div id="divLoading" style="z-index: 1000; position: absolute; text-align: left;
        height: 100%; width: 100%; left: 0px; top: 0px; display: none;">
        <iframe id="frmProcess" frameborder="0" style="position: absolute; top: 0px; left: 0px;
            width: 300px; height: 300px; filter: alpha(opacity=0);"></iframe>
        <div style="position: relative; width: 30px; left: 65px; top: 100px;" id="divProg">
            <br />
            <table border="0" cellpadding="0" cellspacing="0">
                <tr>
                    <td align="center" valign="middle">
                        <img id="imgLoading" src="Images/Cloning.gif" alt="Loading..." runat="server" />
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <form id="form1" runat="server">
  <table style="width:100%; height:100%;" cellspacing="0" cellpadding="8">
		<tr>
			<td class="ms-crm-Dialog-Header">
				<div class="ms-crm-Dialog-Header-Title">
					Clone Opportunity
				</div>
                <div>
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;with Relationships &amp; Event History
                </div>
			</td>
		</tr>
		<tr>
			<td style="border-bottom: 1px solid #999999;" valign="top">
				<div id="divFill">
					  <table id="tblOpportunityClone" width="100%" border="0">
						<%--
                        <tr id="ProgressWindow">
							<td align="center">
								Cloning Opportunity with Relationships &amp; Event History...</td>
						</tr>
                        --%>
						<tr>
							<td align="center">
								<asp:Label ID="lblErrorMessage" runat="server" Text="" CssClass="Errmsg"></asp:Label>
								<div id="dvErrors"  runat="server" style="height: 220px; width: 300px; cursor: pointer;"
									visible="false">
									<asp:DataGrid ID="gvError" runat="server" CellPadding="4" ForeColor="#333333" GridLines="None"
										AutoGenerateColumns="false">
										<Columns>
											<asp:TemplateColumn HeaderText="Errors">
												<ItemTemplate>
													<asp:Label ID="lblErrorDescription" runat="server" Text="<%# Container.DataItem %>"></asp:Label>
												</ItemTemplate>
											</asp:TemplateColumn>
										</Columns>
										<FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
										<EditItemStyle BackColor="#2461BF" />
										<SelectedItemStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
										<PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
										<AlternatingItemStyle BackColor="White" />
										<ItemStyle BackColor="#EFF3FB" />
										<HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
									</asp:DataGrid>
								</div>
							</td>
						</tr>
						<tr>
							<td>
								<div id="dvClone" runat="server" style="height: 220px; width: 300px; display: none;">
									<asp:Label ID="lblDisconnectSettings" runat="server" Text="Please select which event sections should be copied to the new Event."
										CssClass="titleText"></asp:Label>
									<br />
									<asp:TreeView ID="trvClone" runat="server" ShowCheckBoxes="All" ShowLines="True"
										onclick="Client_OnTreeNodeChecked();">
										<NodeStyle CssClass="treeNodeText" />
									</asp:TreeView>
								</div>
                                <div id="dvSpacerForImmediateClone" runat="server" style="height: 220px; width: 300px; display: none;">
								</div>
							</td>
						</tr>
						<tr>
							<td height="5px">
							</td>
						</tr>
						
						<tr>
							<td>
								<asp:HiddenField ID="hdnEventId" runat="server" />
							</td>
						</tr>
					</table>
				</div>
			</td>
		</tr>
		<tr>
            <td align="right" width="100%">
                <asp:Button ID="btnClone" runat="server" Text="Ok" OnClick="btnClone_Click" CssClass="button"
                    OnClientClick="return ShowProcessing();" Width="75px" />
                &nbsp; &nbsp; &nbsp;
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClientClick="winClose();"
                    CssClass="button" Width="75px" />
            </td>
        </tr>
	</table>
    </form>
    <script language="javascript" type="text/javascript">
        function StartClone() {
            __doPostBack('', 'FireCloneOnclick');
            return true;
        }
        var btn = document.getElementById('<%= btnClone.ClientID %>');
        if (btn != null
            && btn.title != "we're in postback") {
            ShowProcessing();
            StartClone();
        }
    </script>
</body>
</html>
