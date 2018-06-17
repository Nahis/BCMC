<%@ control language="C#" autoeventwireup="true" 
CodeFile="HotelGrid.ascx.cs" Inherits="Controls_HotelGrid"
%>
<%--inherits="Controls_HotelGrid, App_Web_ctjc33lj" 
--%>
<%@ Register TagPrefix="ms" TagName="EntityGrid" Src="~/Controls/EntityGrid.ascx" %>

<ms:EntityGrid ID="hotelGrid" runat="server" Style="width:95%;" />
