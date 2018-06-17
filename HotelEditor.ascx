<%@ control language="C#" autoeventwireup="true" 
CodeFile="HotelEditor.ascx.cs" Inherits="Controls_HotelEditor"
%>
<%-- inherits="Controls_HotelEditor, App_Web_ctjc33lj" 
--%>
<%@ Register TagPrefix="ms" TagName="EntityEditor" Src="~/Controls/EntityEditor.ascx" %>
<ms:EntityEditor ID="hotelEditor" runat="server" />
<table class="msa-EntityGrid-GridTable" border="0">
    <tr class="msa-EntityGrid-GridRow">
       <%-- <td align="left" style="padding-left: 48%">
            <asp:Button ID="updateButton" Text="<%$ Resources:eService, entityeditor_savebutton %>"
                CssClass="msa-Global-Button" OnClientClick="return VerifyInput('account') && VerifyInput('contact');"
                 runat="server" />
        </td>--%>
    </tr>
</table>
