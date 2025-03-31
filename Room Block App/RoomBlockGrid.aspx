<%@ Page Language="C#" AutoEventWireup="true" 
	CodeBehind=".\RoomBlockGrid.aspx.cs"
    Inherits="RoomBlock1.Grid" 
	EnableViewState="true" %>

<%@ Register Assembly="System.Web.Extensions, Version=1.0.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>RoomBlock Days Page</title>
	
	<style type="text/css">
		 .hidden_boundfld {
			 display:none;
		 }
		input[type='text']:disabled, input[id='txtActualPct']:disabled {
			background-color: #f8f8f8;/*lightergray*/
			color: black;
			border: none;
		}
		.box-color-and-size {/*not used*/
			font-size:11px;
			background-color: lightgray;
		}
		.gridcell {
			border-color:Gray;
			padding-left:8px;
		}
	</style>
</head>
<body>
    <form id="form1" runat="server">

    <script type="text/javascript" language="javascript">

		<%--to be implemented...?! --%>
    	function updateParent(total, peak) {
    		/*parent.crmForm.all.new_hotelroomnights.DataValue = total;
    		parent.crmForm.all.new_peakhotelroomnights.DataValue = peak;
    		parent.crmForm.Save();*/

    		//window.top.opener.parent.Xrm.Page
			/*ytododev disabled for now, to work out later
    		parent.Xrm.Page.getAttribute("new_hotelroomnights").setValue(total);
    		parent.Xrm.Page.getAttribute("new_peakhotelroomnights").setValue(peak);
    		parent.Xrm.Page.data.entity.save();
			*/
    	}
		
    	function getClosestAncestorTag(el,tag) {
    		while (typeof el !== 'undefined') {
    			el = el.parentNode;
    			if (el.tagName.toLowerCase() === tag.toLowerCase()) {
    				return el;
    			}
    		}
    		return undefined;
    	}

    	function CalcAndSetPeak_Actuals(textbox, colnum) {

    		var row = textbox.parentNode.parentNode;
    		var colIndex = row.cells.length - 1; //set to actualBlock Col index DEFAULT
    		if (typeof colnum !== 'undefined')
    			colIndex = colnum;

    		var peak = {};
    		peak.block = 0;
    		peak.percentColIndex = colIndex - 1;
    		peak.roomblocks = [];
    		peak.idx = 0;
    		peak.disabled = true;

			//Calc PeakBlock
    		ActOnCol(getClosestAncestorTag(textbox, 'table').id, colIndex, function(colmnCell, peak) {
    			if (colmnCell.children.length > 0) {
    				actualBlockCtrl = colmnCell.getElementsByTagName('input')[0];
    				if (typeof actualBlockCtrl !== 'undefined' //handle header
    					&& actualBlockCtrl.disabled != true) {
    					peak.disabled = false;
    					var roomblock = Number(actualBlockCtrl.value);
    					if (null == roomblock || '' == roomblock)
    						roomblock = 0;
    					if (peak.block < roomblock)
    						peak.block = roomblock;  
    					peak.roomblocks[peak.idx] = roomblock;
    				}
    				peak.idx++;
				}
    		}, peak);

    		// Set Percentage of Peak
    		if (! peak.disabled) {
    			peak.idx = 0; //reset to recurse the rows

    			ActOnCol(getClosestAncestorTag(textbox, 'table').id, 
					peak.percentColIndex, 
					function(colmnCell, peak) {
						if (colmnCell.children.length > 0) { //children didn't work in ie
							//ytodo fix standardize to 1 ctrl
    						var actualPercentBlockCtrl = colmnCell.getElementsByTagName('input')[0]; //'Current %'  2 ctls, we want the second
    						if (colmnCell.getElementsByTagName('input').length > 1)
    							actualPercentBlockCtrl = colmnCell.getElementsByTagName('input')[1]; //'Current %' 2 ctls, we want the second
    						var actualPercentCtrl2 = colmnCell.getElementsByTagName('label')[0];
    						var actualPcnt3LableSpan = colmnCell.getElementsByTagName('span')[0]; //actuals label

    						var percentOfPeak = (0 == peak.block) // x/0 protection
													? peak.roomblocks[peak.idx]
													: peak.roomblocks[peak.idx] / peak.block * 100;
    						percentOfPeak = Math.round(percentOfPeak, 1);

    						if (typeof actualPercentBlockCtrl !== 'undefined' //handle header
    							/*&& actualPercentBlockCtrl.disabled != true*/) {

    							//actualPercentBlockCtrl.disabled = false;
    							if (0 == percentOfPeak)
    								actualPercentBlockCtrl.innerHTML = '';
    							else {
    								actualPercentBlockCtrl.innerHTML = percentOfPeak;
    								actualPercentBlockCtrl.innerText = percentOfPeak;
    								try { actualPercentBlockCtrl.value = percentOfPeak; } catch (e) { }
    								try { actualPercentBlockCtrl.innerHTML = percentOfPeak; } catch (e) { }
    							}
    							//actualPercentBlockCtrl.disabled = true;
							}
							// special handling for label
    						if (typeof actualPercentCtrl2 !== 'undefined'/* && actualPercentCtrl2.disabled != true*/) {
    							//actualPercentCtrl2.disabled = false;
    							if (0 == percentOfPeak) {
    								actualPercentCtrl2.innerHTML = '';
    								actualPercentCtrl2.value = '';
    								actualPercentCtrl2.innerText = '';
    							}
    							else {
    								actualPercentCtrl2.innerHTML = percentOfPeak;
    								actualPercentCtrl2.value = percentOfPeak;
    								actualPercentCtrl2.innerText = percentOfPeak;
    							}
    							//actualPercentCtrl2.disabled = true;
    						}
    						if (typeof actualPcnt3LableSpan !== 'undefined') { //handle header
    							if (0 == percentOfPeak) {
    								actualPcnt3LableSpan.innerHTML = '';
    								actualPcnt3LableSpan.innerText = '';
    							}
    							else {
    								try { actualPcnt3LableSpan.value = percentOfPeak; } catch (e) {
    									try { actualPcnt3LableSpan.innerText = percentOfPeak; } catch (e) { }
    									try { actualPcnt3LableSpan.innerHTML = percentOfPeak; } catch (e) { }
    								}
    							}
    						}
    						peak.idx++;
						}
					}, 
					peak);
    		}
    	}

    function onlyNumbers(event) {
    	// alert (event.keyCode);
    	var key = event.which || event.keyCode; //multi-browser sup.

        if (key == 46 || key == 37 || key == 47) {
            return false;
        }

        if (key == 110 || key == 111) {
            return false;
        }
        if (key == 191 || key == 111) {
            return true;
        }

        if (key >= 46 && key <= 57) {
            return true;
        }

        else if ((key == 37 || key == 8) || (key == 9 || key == 110)) {
            return true;
        }
        else
            return false;
    }

    function ForeachRowCol(table, colNum, extData, internalData, fn) {
    	for (var i = 0, row; row = table.rows[i]; i++) {
    		//for (var j = 0, col; col = row.cells[j]; j++) {
    		//	if (j == colNum)
    		//{
    		internalData.rownum = i;
    		internalData.colnum = colNum;
    		col = row.cells[colNum];
    		if (typeof col !== "undefined")
    			fn(col, extData, internalData);
    		//}
    		//}  
    	}
    }
    function ActOnCol(tbl, colNum, fn, extData) {
    	var table = document.getElementById(tbl);
    	if (null === table)
    		return;
    	internalData = {};
    	internalData.id = tbl;
    	ForeachRowCol(table, colNum, extData, internalData, fn);
    }

    // internalData {rownum, colnum, id}
    // fn(col, extData, internalData)
    function ActOnColTitled(tbl, colTitle, extData, fn) {
    	internalData = {};
    	internalData.id = tbl;
    	var table = document.getElementById(tbl);
    	if (null === table)
    		return;
    	row = table.rows[0];
    	for (var j = 0, col; col = row.cells[j]; j++) {
    		if (col.innerHTML == colTitle) {
    			ForeachRowCol(table, j, extData, internalData, fn);
    			return;
    		}  
    	}
    }

    //ff doesn't support onfocusout, we use evt capture as bkup instead
    var f = document.getElementById("form1");
    f.addEventListener("focusout", BlurOnFocusOutFF, true, true);
    f.addEventListener("blur", BlurOnFocusOutFF, true, true);
    function BlurOnFocusOutFF(event) {
    	//if (typeof Components == "undefined")
    	//	return;
    	//var target = Components.utils.cloneInto(event.target, target); //allow access to privileged object
    	//
    	if (-1 != event.target.id.indexOf('txtActualBlock')) {
			
    		CalcAndSetPeak_Actuals(document.getElementById(event.target.id)); //bypass security to privileged object
    	}
    	else if (-1 != event.target.id.indexOf('txtCBlock')) {
    		el = document.getElementById(event.target.id); //bypass security to privileged object
    		//CalcAndSetPeak(el);
    		extData = {}; extData.el = el;
    		ActOnColTitled(getClosestAncestorTag(el, 'table').id, 'Current Block', extData, function (col, extData, internalData) {
    			CalcAndSetPeak_Actuals(extData.el, internalData.colnum);
    		});
    	}
    }
    </script>

		<div>
				<%-- debug stuff --%>
				<asp:TextBox ID="txtDbgArrivalDate" runat="server"></asp:TextBox>
				<asp:TextBox ID="txtDbgDepartureDate" runat="server"></asp:TextBox>
				<asp:TextBox ID="txtDbgActualized" runat="server"></asp:TextBox>
				<asp:Button ID="btnDbgUpdateDateRange" runat="server" 
							Text="ChangeRangeToDeleteCurrentRecords" 
							OnClick="btnDbugUpdateDateRange_click" />
				<div id="DbgInfo" runat="server" style="display:none;" />
				<div id="rbTitle" runat="server" style="display:none;">
					gvRoomBlock - No data yet</div>

		</div>
		<div>
                <asp:HiddenField ID="hdnfdEventID" runat="server" Value="" /></td></tr>
		</div>

		<div>
			<asp:GridView ID="GridView1" runat="server" 
						Width="100%" GridLines="Both" AutoGenerateColumns="False"
						CssClass="grid" 
						OnRowDataBound="GridView1_RowDataBound"
						DataKeyNames="DayNumber,DayofWeek,Date">
			<HeaderStyle CssClass="gridHeader" />
			<RowStyle Height="24px" />
			<Columns>
				<asp:BoundField DataField="DayNumber" HeaderText="Day Number" ItemStyle-Width="15%"
								HeaderStyle-Font-Size="11px" HeaderStyle-CssClass="cell" ItemStyle-Font-Size="11px" ItemStyle-CssClass="gridcell"/>
				<asp:BoundField DataField="DayofWeek" HeaderText="Day Of Week" ItemStyle-Width="15%"
								HeaderStyle-Font-Size="11px" HeaderStyle-CssClass="cell" ItemStyle-Font-Size="11px" ItemStyle-CssClass="gridcell"/>
				<asp:BoundField DataField="Date" HeaderText="Date" 
								ItemStyle-Width="7%" HeaderStyle-Font-Size="11px"
								HeaderStyle-CssClass="cell" ItemStyle-Font-Size="11px" ItemStyle-CssClass="gridcell"/>
				<asp:TemplateField HeaderText="Original % of Peak"><HeaderStyle Font-Size="11px" CssClass="cell" />
					<ItemStyle Width="11%" Font-Size="11px" CssClass="gridCell"/><ItemTemplate>
						<asp:Label ID="lblOPeak" runat="server" 
									Text='<%#DataBinder.Eval(Container.DataItem, "Original % of Peak")%>'></asp:Label></ItemTemplate></asp:TemplateField>
				<asp:TemplateField HeaderText="Original Block">
					<ItemTemplate>
						<asp:Label ID="lblOblock" runat="server" 
									Text='<%#DataBinder.Eval(Container.DataItem, "Original Block")%>'></asp:Label></ItemTemplate>
					<ItemStyle Width="11%" Font-Size="11px" />
					<HeaderStyle Font-Size="11px" CssClass="cell" /></asp:TemplateField>
				<asp:TemplateField HeaderText="Current % of Peak">
					<ItemTemplate>
						<asp:label id="lblCPeak" runat="server"
									Text='<%#DataBinder.Eval(Container.DataItem, "Current % of Peak")%>' 
									/></ItemTemplate>
					<ItemStyle Width="11%" Font-Size="11px" />
					<HeaderStyle Font-Size="11px" CssClass="cell" /></asp:TemplateField>
				<asp:TemplateField HeaderText="Current Block">
					<ItemTemplate>
						<asp:TextBox ID="txtCBlock" runat="server" autoComplete="off" 
							Text='<%#DataBinder.Eval(Container.DataItem, "Current Block")%>'
							onfocusout="CalcAndSetPeak_Actuals(this, 6);" 
							onkeypress="return onlyNumbers(event);" 
							onpaste="return false;"
							MaxLength="5"></asp:TextBox></ItemTemplate>
					<ItemStyle Width="11%" Font-Size="11px" />
					<HeaderStyle Font-Size="11px" CssClass="cell" /></asp:TemplateField>
				<asp:BoundField DataField="RoomGUID" HeaderText="Room Guid" 
								ItemStyle-Width="15%" HeaderStyle-Font-Size="11px" ItemStyle-Font-Size="11px"
								HeaderStyle-CssClass="cell"  >
					<ItemStyle CssClass="hidden_boundfld"/></asp:BoundField>
				<asp:TemplateField HeaderText="Actual % of Peak">
					<ItemTemplate>
						<asp:label ID="txtActualPercentOfPeak" runat="server"
									style="display:none;visibility:hidden;"
									Text='<%#DataBinder.Eval(Container.DataItem, "Actual % of Peak")%>' 
									/></ItemTemplate>
					<ItemStyle Width="11%" Font-Size="11px" />
					<HeaderStyle Font-Size="11px" CssClass="cell" /></asp:TemplateField>
				<asp:TemplateField HeaderText="Actualized Block">
					<ItemTemplate>
						<asp:TextBox ID="txtActualBlock" runat="server"
									Text='<%#DataBinder.Eval(Container.DataItem, "Actual Block")%>'
									onfocusout="CalcAndSetPeak_Actuals(this);"
									onkeypress="return onlyNumbers(event);" 
									onpaste="return false;" 
									MaxLength="5" 
									autoComplete="off"></asp:TextBox></ItemTemplate>
					<ItemStyle Width="11%" Font-Size="11px" />
					<HeaderStyle Font-Size="11px" CssClass="cell" /></asp:TemplateField>
			</Columns>
			</asp:GridView>
		</div>
		<div>
			<asp:Button ID="ButtonSave" runat="server" 
						Text="Save" 
						OnClick="btnSave_Clicked" 
						ToolTip="Save" />
			<asp:Button ID="ButtonMoveToOriginal" runat="server" 
						Text="Copy Current Block" 
						OnClick="btnSaveOriginal_Clicked"
						Enabled="false" 
						ToolTip="Copy Currentblock to Originalblock" 
						/>
		</div>
		<div>
			<asp:GridView ID="GridView2" runat="server" 
						Width="100%" GridLines="Both" AutoGenerateColumns="False"
						CssClass="grid" 
						OnRowDataBound="GridView1_RowDataBound"
						DataKeyNames="DayNumber,DayofWeek,Date">
			<HeaderStyle CssClass="gridHeader" />
			<RowStyle Height="24px" />
			<Columns>
				<asp:TemplateField HeaderText="Original % of Peak"><HeaderStyle Font-Size="11px" CssClass="cell" />
					<ItemStyle Width="11%" Font-Size="11px" CssClass="gridCell" /><ItemTemplate>
						<asp:Label ID="lblOPeak" runat="server" 
									Text='<%#DataBinder.Eval(Container.DataItem, "Original % of Peak")%>'></asp:Label></ItemTemplate></asp:TemplateField>
				<asp:TemplateField HeaderText="Original Block"><HeaderStyle Font-Size="11px" CssClass="cell" />
					<ItemStyle Width="11%" Font-Size="11px" CssClass="gridCell" /><ItemTemplate>
						<asp:Label ID="lblOblock" runat="server" 
									Text='<%#DataBinder.Eval(Container.DataItem, "Original Block")%>'></asp:Label></ItemTemplate></asp:TemplateField>
			</Columns>
			</asp:GridView>
		</div>
    </form>
</body>
</html>
