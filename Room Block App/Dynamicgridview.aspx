<%@ Page Language="C#" AutoEventWireup="true" 
	CodeBehind="Dynamicgridview.aspx.cs" 
	Inherits="RoomBlocks2.Grid" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>RoomBlock Days Page</title>
	
	<style type="text/css">
		 .hidden_boundfld {
			 --display:none;
			 --visibility: collapse;
			 visibility: hidden;
			 display: none;
		 }
		.box-color-and-size {/*not used*/
			font-size:11px;
			background-color: lightgray;
		}
		.gridcell {
			border-color:Gray;
			padding-left:8px;
		}
		.centreInMarginsDebug {
			margin: 3px 3px 3px 3px;
			display: block;
			clear:both;
			--text-align: left;
		}
		.hdnTxtBoxSimlation {
			display:block;
			border: 1px black solid;
			visibility:visible;
			font-size: 11px;
		}
		.block {
			display: block;
			background-repeat:no-repeat;
			--background: ;
		}
	</style>


	<link rel="stylesheet" type="text/css" href="App_Themes/LinesAndReadable/Misc.css" /><%-- generic stuff --%>
	<link rel="stylesheet" type="text/css" href="App_Themes/LinesAndReadable/Tbl-Blue.css" />
	<link rel="stylesheet" type="text/css" href="App_Themes/LinesAndReadable/Statusbar-Blue.css" /><%--mySB--%>
	<link rel="stylesheet" type="text/css" href="App_Themes/LinesAndReadable/ComboActualCmplt-Blue.css" /><%--ActualEntryComplete--%>
	<link rel="stylesheet" type="text/css" href="App_Themes/LinesAndReadable/ActualDayBtns-Blue.css" />

	<style type="text/css">
		#gridContainer {
			--position:relative; 
			width:auto; 
			display:inline-block;
			--margin-top: 14px;
		}
		.grid {
		}
		.gridcell {
			border-color:Gray;
			padding-left:8px;
		}
	</style>

</head>

<body>
    <form id="form1" runat="server" 
		onsubmit="return (typeof submitted == 'undefined') ? (submitted = true) : !submitted" >

    <script type="text/javascript">

    	function updateParent(total, peak) {
    		<%--ytodo to be implemented...?! --%>
			<%-- also 
					Bcmc_actualpeakhotelroomnights
					Bcmc_actualhotelroomnights
    		/*parent.crmForm.all.new_hotelroomnights.DataValue = total;
    		parent.crmForm.all.new_peakhotelroomnights.DataValue = peak;
    		parent.crmForm.Save();

    		//window.top.opener.parent.Xrm.Page
    		parent.Xrm.Page.getAttribute("new_hotelroomnights").setValue(total);
    		parent.Xrm.Page.getAttribute("new_peakhotelroomnights").setValue(peak);
    		parent.Xrm.Page.data.entity.save();
			*/
			--%>
    	}

    	function getClosestAncestorTag(el, tag) {
    		while (typeof el !== 'undefined') {
    			el = el.parentNode;
    			if (el.tagName.toLowerCase() === tag.toLowerCase()) {
    				return el;
    			}
    		}
    		return undefined;
    	}

    	<%--
    	function InsertDebugElement(elId, parentId, ElementTag, value) {
    		var el = document.getElementById(elId);
    		if (null == el) {
    			var frm = document.getElementById(parentId);
    			var el = document.createElement(ElementTag);
    			el.id = elId;
    			el.type = 'text';
    			el.text = elId;
    			el.innerHTML = elId + ': ' + value;
    			el.innerText = elId + ': ' + value;
    			frm.appendChild(el);
    		} else {
    			el.innerHTML = elId + ': ' + value;
    			el.innerText = elId + ': ' + value;
    		}
    	}
		--%>

    	<%-- This can only be true if the value is NaN
    		http://adripofjavascript.com/blog/drips/the-problem-with-testing-for-nan-in-javascript.html
		--%>
    	function isNaN__(x) {
    		var valueIsNaN = (x !== x);
    		return valueIsNaN;
    	}

    	function CalcAndSetPeak_Actuals(textbox, colnum/*, calcTotalsOnly=false if textbox is null then calcTotalsONLY*/) {

    		/*return;*/

    		var tableid = null;
    		if (null == textbox) {/*Calculate Totals Only*/
    			tableid = document.getElementsByTagName('table')[0].id;
    		}
    		else {
    			var row = textbox.parentNode.parentNode;
    			var colIndex = row.cells.length - 1; //set to actualBlock Col index DEFAULT

    			tableid = getClosestAncestorTag(textbox, 'table').id;
    		}
    		if (typeof tableid == 'undefined'
				|| null == tableid)
    			return; //no date range?

    		if (typeof colnum !== 'undefined')
    			colIndex = colnum;

    		var peak = {};
    		peak.peakblock = 0;
    		peak.percentColIndex = colIndex - 1;
    		peak.roomblocks = [];
    		peak.idx = 0;
    		peak.disabled = true;

    		//Calc PeakBlock
    		ActOnCol(tableid, colIndex, peak, function (colmnCell, peak) {
    			if (colmnCell.children.length > 0) {
    				actualBlockCtrl = colmnCell.getElementsByTagName('input')[0];
    				if (typeof actualBlockCtrl !== 'undefined' //handle header
    					&& actualBlockCtrl.disabled != true) {
    					peak.disabled = false;
    					var roomblock = Number(actualBlockCtrl.value);
    					if (null == roomblock || '' == roomblock)
    						roomblock = 0;
    					if (peak.peakblock < roomblock)
    						peak.peakblock = roomblock;
    					peak.roomblocks[peak.idx] = roomblock;
    				}
    				peak.idx++;
    			} 
    			else if (null == textbox) {/*calc totals*/
    				var roomblock = Number(colCell.textContent);
    				if (null == roomblock || '' == roomblock || isNaN__(roomblock))
    					roomblock = 0;
    				peak.roomblocks[peak.idx] = roomblock;
    				peak.idx++;
				}

    		});


    		// Set totals:
    		TotalRoomBlock = 0;
    		for (block = 0; block < peak.roomblocks.length; block++) {
    			TotalRoomBlock += peak.roomblocks[block];
    		}
    		if (4 == colnum) {
    			totEl = document.getElementById('TB_1');
    		}
    		else if (6 == colnum) {
    			totEl = document.getElementById('TB_2');
    		}
    		else if (9 == colnum) {
    			totEl = document.getElementById('TB_3');
    		}
    		if (null != totEl)
    			totEl.textContent = TotalRoomBlock;

    		if (null == textbox) {/*Calculate Totals Only*/
    			return TotalRoomBlock;
    		}


    		// Set Percentage of Peak
    		if (!peak.disabled) {
    			peak.idx = 0; //reset to recurse the rows

    			ActOnCol(tableid,
					peak.percentColIndex,
					peak,
					function (colmnCell, peak, internalData) {
						if (colmnCell.children.length > 0) {
							if (typeof peak.roomblocks[peak.idx] !== 'undefined') { <%--/*Disabled block won't have a digit & therefore no need for a percantage calc. Or heading I suppose*/--%>
								var percentOfPeak = (0 == peak.peakblock) // x/0 protection
														? peak.roomblocks[peak.idx]
														: peak.roomblocks[peak.idx] / peak.peakblock * 100;
								percentOfPeak = Math.round(percentOfPeak, 1);
								percentOfPeak = 0 == percentOfPeak ? 0 : percentOfPeak;


								<%--
								/*ytodo remove test code*/
								elIdRow = 'row' + internalData.rownum;
								elId = 'row' + internalData.rownum/*peak.idx*/ + 'col' + internalData.colnum;
								InsertDebugElement(elIdRow, 'form1', 'div', elIdRow + 'percentOfPeak: ');
								InsertDebugElement(elId, elIdRow, 'input', percentOfPeak);
								--%>

								<%--
								if (peak.idx < -2) {
									el = document.createElement('div');
									el.innerText = 'before' + peak.idx + colmnCell.innerHTML;
									/*el.style.height = '40px';*/
									document.getElementById('form1').appendChild(el);
								}
								/*c = colmnCell;
								if ('textContent' in colmnCell) {
									c.textContent = percentOfPeak;
								}*/ /*else if ('innerText' in colmnCell) {
									c.innerText = percentOfPeak;*/
								--%>

								inputCtls = colmnCell.getElementsByTagName('input');
								for (ctl = 0; ctl < inputCtls.length; ctl++) {
									inputCtls[ctl].value = percentOfPeak;
								}

								Labels = colmnCell.getElementsByTagName('label');
								for (ctl = 0; ctl < Labels.length; ctl++) {
									c = Labels[ctl];
									document.write('' + percentOfPeak + ' tag=' + c.tagName + ' id=' + c.id + '  class=' + c.className + '<br/>');
									if ('textContent' in c) {
										c.textContent = percentOfPeak;
									} else if ('innerText' in c) {
										c.innerText = percentOfPeak;
									} else {
										c.innerHTML = percentOfPeak;
									}
								}

								Spans = colmnCell.getElementsByTagName('span');
								for (ctl = 0; ctl < Spans.length; ctl++) {
									c = Spans[ctl];
									if ('textContent' in c) {
										c.textContent = percentOfPeak;
									} else if ('innerText' in c) {
										c.innerText = percentOfPeak;
									} else {
										c.innerHTML = percentOfPeak;
									}
								}
								if (peak.idx < -2) {
									el = document.createElement('div');
									el.innerText = 'after' + peak.idx + colmnCell.innerHTML;
									/*el.style.height = '40px';*/
									document.getElementById('form1').appendChild(el);
								}
							} else {
								<%--
								el = document.createElement('div');
								el.innerText = 'this.idx.is.undefined=' + peak.idx;
								/*el.style.height = '40px';*/
								document.getElementById('form1').appendChild(el);
								--%>
							}
							peak.idx++;
						}
					});
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

    	function ForeachRowCol(table, colNum, externalUserData, internalData, fn) {
    		for (var i = 0, row; row = table.rows[i]; i++) {
    			//for (var j = 0, col; col = row.cells[j]; j++) {
    			//	if (j == colNum)
    			//{
    			internalData.rownum = i;
    			internalData.colnum = colNum;
    			colCell = row.cells[colNum];
    			if (typeof colCell !== "undefined")
    				fn(colCell, externalUserData, internalData<%--optional%--%>);   <%--//internalData {id, rownum, colnum}--%>
    			//}
    			//}  
    		}
    	}
    	function ActOnCol(tbl, colNum, extData, fn) {
    		var table = document.getElementById(tbl);
    		if (null === table)
    			return;
    		internalData = {};
    		internalData.id = tbl;
    		ForeachRowCol(table, colNum, extData, internalData, fn);
    	}

		<%--
    	// fn(colCell, externalUserData, internalData)
    	// internalData {id, rownum, colnum}
		--%>
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
			<%--
    		//if (typeof Components == "undefined")
    		//	return;
    		//var target = Components.utils.cloneInto(event.target, target); //allow access to privileged object
    		//
			--%>
    		if (-1 != event.target.id.indexOf('txtActualBlock')) {

    			CalcAndSetPeak_Actuals(document.getElementById(event.target.id)); //bypass security to privileged object
    		}
    		else if (-1 != event.target.id.indexOf('txtCBlock')) {
    			el = document.getElementById(event.target.id); //bypass security to privileged object
    			extData = {};
    			extData.el = el;
    			ActOnColTitled(
					getClosestAncestorTag(el, 'table').id,
					'Current Block',
					extData,
					function (col, extData, internalData) {
    					CalcAndSetPeak_Actuals(extData.el, internalData.colnum);
    			});
    		}
    	}

		
    	function offsetFrom(selector) {
    		element = document.querySelector(selector); //'#GridView1 > tbody:nth-child(1) > tr:nth-child(9) > td:nth-child(3)');

    		if (null == element) {
    			r = {}; //new TextRectangle();
    			r.left = r.top = r.width = 0;
    			return { offset: 0, r: r };
    		}

    		var t = element.tagName + ' ' + element.textContent;

			<%-- 
			Relative to the viewport or relative to the body element
			When the body has a margin, its bounding box will be offset by that number of pixels. Use CSS reset to cancel, or calculate.
			--%>
    		
    		var bodyRect = document.body.getBoundingClientRect(),
				elemRect = element.getBoundingClientRect(),
				offset = elemRect.left - bodyRect.left;
				
    		return {offset : offset, r : elemRect, el: element, bodyRect: bodyRect};
    	}
    	function setPos(id, left, top) {
    		el = document.getElementById(id);
    		if (null == el)
    			return;
			if ('undefined' !== left)
				el.style.left = left+'px';
			if ('undefined' !== top)
				el.style.top = top+'px';
				
			console.log('left,top: '+left+', '+ top);
		}
		function setPosInline(id, id2, left, top) {
			el = document.getElementById(id);
			if (null == el)
				return;
			el2 = document.getElementById(id2);
			r1 = el.getBoundingClientRect();
			console.log(r1);
			rParent = el.parentNode.getBoundingClientRect();
			if (typeof left == 'undefined')
				left = 0;
			setPos(id2, r1.left+r1.width, r1.top-rParent.top-1+ left, top);
		}
		function lineUpFromOffset(idList, left, top) {
			for (var i = 0; i < idList.length; i++) {
				if (0 == i)
					setPos(idList[i], left, top);
				else
					setPosInline(idList[i-1], idList[i]);
			}
		}
		function alignToTblColAndSize(tblColNum, followerId, titleText) {
			headerElem = offsetFrom('tr > th:nth-child(' + tblColNum + ')');
			notFound = 0;
			while (headerElem.el.textContent != titleText) {//innerText
				headerElem.el = headerElem.el.previousSibling;
				if (null == headerElem.el)
					return;
				notFound = notFound + 1;
				if (notFound > 15)
					return;
			}
			grid = document.getElementById('GridView1');
			if (typeof grid == 'undefined' || null == grid)
				grid = document.getElementById('gridContainer');
			gv = grid.getBoundingClientRect();
			btnArray = [];
			followerElem = //document.querySelector('#Totalsbar > columnTotal:nth-child(1)');
				document.getElementById(followerId);
			if (null != followerElem) {
				shortenedLines = 0;
				btnArray.push([followerId]);
				lineUpFromOffset(btnArray, headerElem.r.left - gv.left /*followerElem.parentNode.getBoundingClientRect().left - 1*/ - 1 + shortenedLines/2);
				followerElem.style.width = headerElem.r.width - shortenedLines + "px";
			}
		}

		var notifyUpdateSuccessfull = false;

		function ShowElement(el, show) {
			if (typeof el !== "undefined" && null != el) {
				if (! show) {
					if (null == el.style)
						el.setAttribute('style', 'visibility:hidden;');
					else
						el.style.visibility = 'hidden';
					el.style.display = "none";
				} else {
					if (null == el.style)
						el.setAttribute('style', 'visibility:visible;');
					else
						el.style.visibility = 'visible';
					el.style.display = "inline-block";
				}
			}
		}

		function positionChanged() {

			<%-- /*Line up Add Actual day buttons above and below the grid*/ --%>
			
			var notVisible = false;

			grid = document.getElementById('GridView1');
			if (typeof grid == "undefined" || null == grid) {
				//hide stuff that won't be positioned anyway
				ShowElement(document.getElementById('AddDayButtonsBelow'), false);
				ShowElement(document.getElementById('mySB'), false);

				//el = document.getElementById("loading")
				//if (null != el) {
				//	el.innerHTML = 'No Roomblocks available';
				//}

				notVisible = true;
			}

			addDayBtn = document.getElementById('AddActualDayAbove');
			if (null == addDayBtn) {
				/*Not actualized - so hide totals*/
				TotalsbarContainer = document.getElementsByTagName('InFlowTakeUpSpace_TotalsbarContainer')[0];
				if (null !== TotalsbarContainer) {
					if (null == TotalsbarContainer.style)
						TotalsbarContainer.setAttribute('style', 'visibility:hidden;'); //setAttribute('style', styleString); or IE.style.setAttribute('style', styleString);
					else
						TotalsbarContainer.style.visibility = 'hidden';
					TotalsbarContainer.style.display = "none";
				}
			}

			if (notVisible) {
				//if not visible no positioning necessary.
				return;
			}

			rGrid = grid.getBoundingClientRect();
			/*status bar*/
			//elDim = offsetFrom('#GridView1');

			statusBar = document.getElementById('mySB');
			statusBarRect = statusBar.getBoundingClientRect();
			//belowStatusBar = statusBarRect.height;
			belowStatusBar = rGrid.bottom + statusBarRect.height;

			/*same with as grid*/
			/*statusBar.style.right = elDim.r.right - 1000;
			statusBar.style.width = elDim.r.right - 1000;*/


			elDim = offsetFrom('tr > th:nth-child(3)'); //'tr.cell > th:nth-child(3)'); <%--date column in table--%>

			//offset = elDim.offset + 15;
			offset = elDim.offset + elDim.r.width / 2; /*center over date*/
			offset = offset - 3 - addDayBtn.getBoundingClientRect().width * 2 / 2; /*width of both buttons */

			/*Position top add day buttons*/
			btnArray = [];
			/*if (null != document.getElementById('RemoveActualDayAbove'))
				btnArray.push(['RemoveActualDayAbove']);
			btnArray = btnArray.concat(['AddActualDayAbove', 'TopActualDaysHelpText']);
			lineUpFromOffset(btnArray, offset);
			*/
			btnArray.push(['AddDayButtonsAbove']);
			lineUpFromOffset(btnArray, offset);

			/*set to grid width*/
			TotalsbarContainer = document.getElementsByTagName('InFlowTakeUpSpace_TotalsbarContainer')[0];
			gridwidth = (rGrid.right - rGrid.left) + "px";
			if (null !== TotalsbarContainer) {
				if (null == TotalsbarContainer.style)
					TotalsbarContainer.setAttribute('style', 'width:' + gridwidth); //setAttribute('style', styleString); or IE.style.setAttribute('style', styleString);
				else
					TotalsbarContainer.style.width = gridwidth;
				TotalsbarContainer.style.visibility = 'visible';
			}
			if (null !== statusBar) {
				gridwidth = (rGrid.right - rGrid.left) + 4 + "px";
				if (null == statusBar.style)
					statusBar.setAttribute('style', 'width:' + gridwidth); //setAttribute('style', styleString); or IE.style.setAttribute('style', styleString);
				else
					statusBar.style.width = gridwidth;
			}

			/*Position bottom add day buttons*/
			btnArray = [];
			/*if (null != document.getElementById('RemoveActualDayBelow'))
				btnArray.push(['RemoveActualDayBelow']);
			btnArray = btnArray.concat(['AddActualDayBelow', 'BottomActualDaysHelpText']);
			lineUpFromOffset(btnArray, offset); //, belowStatusBar);*/
			btnArray.push(['AddDayButtonsBelow']);
			lineUpFromOffset(btnArray, offset);

			// actual entry complete yes/no btn
			sbRectHeight = statusBar.getBoundingClientRect().height;
			yesNoCtrlRect = document.getElementById('ActualEntryComplete').getBoundingClientRect();
			if (null !== yesNoCtrlRect) {
				<%--move into statusbar area if statusbar lhs is not showing, otherwise below Save/Copy btns--%>
				statusBarLhs = document.getElementById('mySB_lhs');
				if (null == statusBarLhs /*|| statusBarLhs.style.visibility != 'visible'*/) {
					statusBar.appendChild(document.getElementById('ActualEntryComplete'));
					statusBar.appendChild(document.getElementById('AddDayButtonsBelow'));
					document.getElementById('AddDayButtonsBelow').style.width = "150px"; /*hack*/
				}
				else {
					<%-- position under Update/Copy btns --%>
					//rActualDayAdd = document.getElementById('AddDayButtonsBelow').getBoundingClientRect();
					//rElement = grid.getBoundingClientRect();
					rBody = document.body.getBoundingClientRect();
					btnArray = ['ActualEntryComplete'];
					lineUpFromOffset(btnArray,
						rGrid.right - (yesNoCtrlRect.right - yesNoCtrlRect.left) - rBody.left
						, -21
						//- rActualDayAdd.height
						/*,rGrid.bottom + sbRectHeight*/);
				}

				<%--only visible in actualized mode, Position total cell below appropriate column--%>
				alignToTblColAndSize(5, 'TB_1', 'Original Block');
				alignToTblColAndSize(7, 'TB_2', 'Current Block');
				alignToTblColAndSize(10, 'TB_3', 'Actualized Block');
			}


			//lineUpFromOffset(['RemoveActualDayAbove', 'AddActualDayAbove', 'TopActualDaysHelpText'], offset);
			//lineUpFromOffset(['RemoveActualDayBelow', 'AddActualDayBelow', 'BottomActualDaysHelpText'], offset, belowStatusBar-5);

			/*
			setPos('RemoveActualDayAbove', offset);
			setPosInline('RemoveActualDayAbove', 'AddActualDayAbove'); //,-1);
			setPosInline('AddActualDayAbove', 'TopActualDaysHelpText'); //, -1);

			setPos('AddActualDayBelow', offset);
    		setPosInline('AddActualDayBelow', 'RemoveActualDayBelow');
    		setPosInline('RemoveActualDayBelow', 'BottomActualDaysHelpText');
			*/
    	}

    	// JavaScript
    	function jsUpdateSize() {
    		// Get the dimensions of the viewport
    		var width = window.innerWidth ||
						document.documentElement.clientWidth ||
						document.body.clientWidth;
    		var height = window.innerHeight ||
						 document.documentElement.clientHeight ||
						 document.body.clientHeight;

    		positionChanged();

		<%-- 

			high cross-browser compatibility can use window.pageXOffset and window.pageYOffset 
			instead of window.scrollX and window.scrollY. Scripts without access to window.pageXOffset, 
			window.pageYOffset, window.scrollX and window.scrollY can use:

			// For scrollX
			(((t = document.documentElement) || (t = document.body.parentNode))
			  && typeof t.ScrollLeft == 'number' ? t : document.body).ScrollLeft
			// For scrollY
			(((t = document.documentElement) || (t = document.body.parentNode))
			  && typeof t.ScrollTop == 'number' ? t : document.body).ScrollTop
		--%>

    	};

    	function getTblColFromTitle(tblColNum, titleText) {
    		headerElem = offsetFrom('tr > th:nth-child(' + tblColNum + ')');
    		if (typeof headerElem.el === 'undefined')
    			return null;
    		while (headerElem.el.textContent != titleText) { //innerText 
    			headerElem.el = headerElem.el.previousSibling;
    			if (null == headerElem.el)
    				return null;
    		}
    		return headerElem;
    	}

    	function whenPageFirstLoads() {
    		try {
    			/*extData = {}; //extData.el = el;
				ActOnColTitled(document.getElementsByTagName('table')[0].id, 'Current Block', extData, function (col, extData, internalData) {
					CalcAndSetPeak_Actuals(null, internalData.colnum);
				});*/
    			
    			//var grid = document.getElementById('GridView1');
    			//if (typeof grid == 'undefined' || null == grid) {
    			//	document.getElementById('centreInMarginsDebug').style.visibility = 'hidden';
    			//} else {
    			//	document.getElementById('centreInMarginsDebug').style.visibility = 'visible';
    			//}

    			colHeaderElem = getTblColFromTitle(5, 'Original Block');
    			if (null != colHeaderElem) {
    				CalcAndSetPeak_Actuals(null, colHeaderElem.el.cellIndex);
    			}
    			colHeaderElem = getTblColFromTitle(7, 'Current Block');
    			if (null != colHeaderElem) {
    				CalcAndSetPeak_Actuals(null, colHeaderElem.el.cellIndex);
    			}
    			colHeaderElem = getTblColFromTitle(10, 'Actualized Block');
    			if (null != colHeaderElem) {
    				CalcAndSetPeak_Actuals(null, colHeaderElem.el.cellIndex);
    			}
    		} catch (err)
    		{ /*should ignore fail here, but why ytodo */}

    		jsUpdateSize();

    		if (notifyUpdateSuccessfull) {
    			notifyUpdateSuccessfull = false;
    			alert('Successfully updated');
    		}
		}
		
    	window.onload = whenPageFirstLoads; // When the page first loads
    	window.onresize = jsUpdateSize;     // When the browser changes size
		
    	<%--
    	// jQuery
    	function jqUpdateSize() {
    		// Get the dimensions of the viewport
    		var width = $(window).width();
    		var height = $(window).height();

    		$('#jqWidth').html(width);
    		$('#jqHeight').html(height);
    	};
    	$(document).ready(jqUpdateSize);    // When the page first loads
    	$(window).resize(jqUpdateSize);     // When the browser changes size

		//for learning only!
		var hscroll = (document.all ? document.scrollLeft : window.pageXOffset);
		var vscroll = (document.all ? document.scrollTop : window.pageYOffset);
		--%>

	<%-- 
	//Usage:
    //OnClientClick = "SingleClickOnly('Saving...');"
    //UseSubmitBehavior = "false"
	--%>
   	function SingleClickOnly(msg) {
    		/*if (!Page_IsValid())
				return false;
			*/ 
    		this.disabled = true;
			if ('undefined' !== msg)
    			this.value = msg; //'Saving...';"
    	}
    </script>

	<div class="centreInMarginsDebug">

		<div id="error" runat="server" visible="false"/>

		<div id="dbg" runat="server" DefaultButton="btnDbgUpdateDateRange" visible="false">
				<%-- debug stuff --%>
				<label class="block__">Arrival/Departure dates:
					<asp:TextBox ID="txtDbgArrivalDate" runat="server"></asp:TextBox>
					<asp:TextBox ID="txtDbgDepartureDate" runat="server"></asp:TextBox></label>
				<label class="block">1/0 Actualized on/off; <br/>d0 - delete row 0,<br/> p3 - set prior/post 3;
					<asp:TextBox ID="txtDbgActualized" runat="server"></asp:TextBox></label>
				<label class="block__">Event StatusCode (10 == Definite)
					<asp:TextBox ID="txtDbgStatusCode" runat="server"
								Text=""></asp:TextBox></label>
				<label class="block">Actualized 1/0 on/off<asp:TextBox ID="txtDbgActualized2" runat="server"
													Text="0"></asp:TextBox></label>
				<label class="block__">Actual Prior/Post days
					<asp:TextBox ID="txtDbgExtendDays" runat="server"
								Text="2"></asp:TextBox></label>

				<asp:Button ID="btnDbgUpdateDateRange" runat="server" 
							Text="ChangeRangeToDeleteCurrentRecords" 
							OnClick="btnDbugUpdateDateRange_click" 
							Default/>

				<asp:Button ID="btnDbgTestPluginONLY" runat="server" 
							Text="Test Plugin ONLY" 
							OnClick="btnDbugTestPluginONLY_click" />


				<%-- output window --%>
				<br/>
				<br/>
				<br/>
				<div id="DbgInfo" runat="server" style="display:none;" />
				<div id="rbTitle" runat="server" style="display:none;" />

				<%-- 
					// Not for server control i.e. runat=server
						<div id="xxTesting123" class="<%="txt"+RoomBlocks2.Grid.Columns.DayNumber.ToString() %>" runat="server"
							 />
							<%=this.EventId%>
						<a href="<%= ConfigurationManager.AppSettings["newURL"] %>">Click
					// Server controls:
					// Call Page.DataBind() to bind <%# %> format
						<div id="xxTesting123" class="<%#"txt"+RoomBlocks2.Grid.Columns.DayNumber.ToString() %>" runat="server"
							 />
						<div class=<%# "txt"+RoomBlocks2.Grid.Columns.DayNumber.ToString() %> runat="server"
							 />
				--%>
		</div><%-- dbg ------------------------------------------------------------%>

		<div>
               <asp:HiddenField ID="hdnfdEventID" runat="server" Value="" />
		</div>


		<%-- Add/Remove day buttons ABOVE GRID --%>

		<div id="AddDayButtonsAbove" runat="server" 
				class="add-day-button-container"
				>
			<div class='add-day-button-spacer'></div><!--remove space
			--><div id="AddActualDayAbove" runat="server" 
					class="add-day-button add-day-button-up"
					onclick="javascript:DivClicked(this); return true;" 
					title="Add Day"
    				OnClientClick = "SingleClickOnly();"
    				UseSubmitBehavior = "false"
					>
					</div>
			<div id="RemoveActualDayAbove" runat="server" 
					class="add-day-button add-day-button-down"
					onclick="javascript:DivClicked(this); return true;" 
					title="Remove Day"
    				OnClientClick = "SingleClickOnly();"
    				UseSubmitBehavior = "false"
					>
					</div>
			<div id="TopActualDaysHelpText" runat="server" 
					class="add-day-button add-day-button-text"
					>Add/Remove Day</div></div>
					 
					 
		<div id="gridContainer">		 
			<asp:GridView ID="GridView1" runat="server" 
						GridLines="Both" AutoGenerateColumns="False"
						OnRowDataBound="GridView1_RowDataBound"
						OnPreRender="GridView1_PreRender"
						DataKeyNames="Day Number,Day Of Week,Date">
			<%--<HeaderStyle CssClass="crmGridHeader" /> cell--%>
			<RowStyle Height="24px" />
			<Columns>
				<%--
				<asp:TemplateField>
				  <HeaderTemplate>
					<th colspan="6">Category</th>
					<tr class="gvHeader">
					   <th style="width:0px"></th>
					   <th colspan="3">Hardware</th>                        
					   <th colspan="3">Software</th>
					</tr>
					<tr class="gvHeader">
					  <th></th>
					  <th>S. No.</th>
					  <th>Product</th>
					  <th>Price</th>

					  <th>Product</th>
					  <th>Descript</th>
					  <th>Price</th>
					  <th>Price</th>
					  <th>Price</th>
					  <th>Price</th>
					</tr>
				  </HeaderTemplate> </asp:TemplateField>--%>
				<%-- removed 
					Boundfield HeaderStyle-Font-Size="11px"
					<HeaderStyle Font-Size="11px"
					<ItemStyle Font-Size="11px"
				--%>
				<asp:BoundField DataField="Day Number" HeaderText="Day Number" />
				<asp:BoundField DataField="Day Of Week" HeaderText="Day Of Week" ItemStyle-CssClass="dayOfWeekColumn" />
				<asp:BoundField DataField="Date" HeaderText="Date" />
				<asp:TemplateField HeaderText="Original % of Peak">
					<ItemStyle/><ItemTemplate>
						<asp:Label ID="lblOPeak" runat="server" 
									Text='<%#DataBinder.Eval(Container.DataItem, "Original % of Peak")%>'></asp:Label>
						<input type="hidden" id="hdnOrigPeakPercent" runat="server" 
									value='<%#DataBinder.Eval(Container.DataItem, "Original % of Peak")%>' 
									/>
					                                                             </ItemTemplate></asp:TemplateField>
				<asp:TemplateField HeaderText="Original Block">
					<ItemTemplate>
						<asp:Label ID="lblOblock" runat="server" 
									Text='<%#DataBinder.Eval(Container.DataItem, "Original Block")%>'></asp:Label>
						<input type="hidden" id="hdbOrigBlock" runat="server" 
									value='<%#DataBinder.Eval(Container.DataItem, "Original Block")%>' 
									/>
																							</ItemTemplate>
					<ItemStyle />
					<HeaderStyle /></asp:TemplateField>
				<asp:TemplateField HeaderText="Current % of Peak">
					<ItemTemplate>
						<asp:label id="lblCPeak" runat="server" EnableViewState="true"
									Text='<%#DataBinder.Eval(Container.DataItem, "Current % of Peak")%>' 
									/>
						<input type="hidden" id="hdnCurrentPeak" runat="server" 
									value='<%#DataBinder.Eval(Container.DataItem, "Current % of Peak")%>' 
									/>
						</ItemTemplate>
					<ItemStyle />
					<HeaderStyle /></asp:TemplateField>
				<asp:TemplateField HeaderText="Current Block">
					<ItemTemplate>
						<asp:TextBox ID="txtCBlock" runat="server" 
							Text='<%#DataBinder.Eval(Container.DataItem, "Current Block")%>'
							onfocusout="CalcAndSetPeak_Actuals(this, 6);" 
							onkeypress="return onlyNumbers(event);" 
							onpaste="return false;"
							MaxLength="5"
							autoComplete="off" ></asp:TextBox>
																</ItemTemplate>
					<ItemStyle />
					<HeaderStyle /></asp:TemplateField>
				<asp:BoundField DataField="Room Guid" HeaderText="Room Guid" > <ItemStyle 
								CssClass="hidden_boundfld" /></asp:BoundField>
				<asp:TemplateField HeaderText="Actual % of Peak">
					<ItemTemplate>
						<asp:label ID="txtActualPercentOfPeak" runat="server"
									Text='<%#DataBinder.Eval(Container.DataItem, "Actual % of Peak")%>' 
									/>
						<input type="hidden" id="hdnActualPeak" runat="server" 
									value='<%#DataBinder.Eval(Container.DataItem, "Actual % of Peak")%>' 
									/>
									</ItemTemplate>
					<ItemStyle/>
					<HeaderStyle /></asp:TemplateField>
				<asp:TemplateField HeaderText="Actualized Block">
					<ItemTemplate>
						<asp:TextBox ID="txtActualBlock" runat="server"
									Text='<%#DataBinder.Eval(Container.DataItem, "Actual Block")%>'
									onfocusout="CalcAndSetPeak_Actuals(this);"
									onkeypress="return onlyNumbers(event);" 
									onpaste="return false;" 
									MaxLength="5" 
									autoComplete="off"></asp:TextBox></ItemTemplate>
					<ItemStyle  />
					<HeaderStyle /></asp:TemplateField>
			</Columns>
			</asp:GridView>


			<%-- click event handler --%>

			<asp:Button id="btnEventHandler" runat="server" 
						style="display:none" 
						onclick="btnDummy_EventHandler_OnClick" 
						/>
			<script>
				function DivClicked(sender, parm) {
					//function SingleClickOnly(msg) {
						/*if (!Page_IsValid())
							return false;
						*/
					sender.disabled = true;
					//}

					__doPostBack('zzz', sender.id + ', ' + parm);
					return;
					//return;
					var btnHidden = document.getElementById('<%= btnEventHandler.ClientID %>');
					btnHidden.setAttribute('content', 'test content');
					btnHidden.appendChild(document.createTextNode("test content"));
					//var btnHidden = document.getElementById(sender.id);
					if (btnHidden != null) {
						//btnHidden.click();
						__doPostBack('<%=btnEventHandler.ClientID %>', sender);
					}
				}
				/*function OpenSubTable(bolID, controlID) {
					__doPostBack('UpdatePanelSearch', JSON.stringify({ bolID: bolID, controlID: controlID}();
				}*/
			</script>
				<%-- ytodo cleanup and document
				<a href="" id="Grey" runat="server" onserverclick='btnDummy_EventHandler_OnClick'>Test Text</a></div>
				javascript:__doPostBack(&#39;Grey&#39;,&#39;&#39;)
				javascript:__doPostBack('Grey','')--%>


			<%-- Totals Bar --%>
			<style type="text/css">
				#Totalsbar {
					border-left: solid black 1px;
					border-right: solid black 1px;
					border: solid black 0px;
					padding: 0;
					margin: 0;
					position: relative;
				}
				columnTotal {
					display: inline-block;
					margin: 2px 0 2px;
					padding-left: 4px;
					padding-top: 4px;
					padding-bottom: 0;
					font-weight: bold;
					--width: 70px;
					--height: 30px;
					--visibility: hidden; /*takeupSpaceMakerHidden - now showing lhs title*/
				}
				#Totalsbar > columnTotal {
					position: absolute;
					visibility: visible;
					border: 1px solid black;
					border-top: 2px solid black;
					--border-bottom: 2px solid black;
					padding: 0;
					text-align: center;
					font-weight: normal;
				}
				InFlowTakeUpSpace_TotalsbarContainer {
					display: inline-block;
					--border: solid 4px red;
					--width: 500px;
				}
			</style>
			<InFlowTakeUpSpace_TotalsbarContainer>
				<div id="Totalsbar" class="c-relative-x">
					<columnTotal id="TB_1">123</columnTotal>
					<columnTotal id="TB_2">456</columnTotal>
					<columnTotal id="TB_3">789</columnTotal>
					</div>
				<columnTotal id="InFlowAndTakeUpSpace" >Total Room Nights</columnTotal>
				</InFlowTakeUpSpace_TotalsbarContainer>


			<%-- StatusBar with Save/Update & 'Copy Block' BUTTONS --%>

			<div id="mySB">
				<asp:Button ID="ButtonMoveToOriginal" runat="server" 
							Text="Copy Current Block" 
							OnClick="btnSaveOriginal_Clicked"
							Enabled="false" 
							ToolTip="Copy Currentblock to Originalblock" 
							/>
				<asp:Button ID="ButtonSave" runat="server" name="ButtonSave"
							Text="Save" 
							OnClick="btnSave_Clicked" 
							OnClientClick="/*if (!Page_IsValid()){ return false; }*/ this.disabled = true; this.value = 'Saving...';" 
							UseSubmitBehavior="false"
							ToolTip="Save" 
							/>
				<span id="mySB_lhs" class="crmStyle" runat=server>
					<span >Arrival/Departure: <span id=mySB_AD class="fld-value">2014-07-02 - 2014-07-06</span></span>
					<span> Status: <span class="fld-value">10 (Definite)</span></span>
					<span ><span class="fld-value">ACTUALIZED  </span>
						<div >Prior: <span class="fld-value">2</span></div>
						<div >Post: <span class="fld-value">2</span></div></span>
					<span >Total Room Nights: <span class="fld-value">1508</span></span>
					<span >Peak Room Nights: <span class="fld-value">788</span></span>
					</span>
				</div>

			<%-- 
			<style type="text/css">
				#ytodoOrigStatusBarBkup_Remove {
					background-color: #CE9A31;
				}
				#ytodoOrigStatusBarBkup_Remove > input {
					float:right;
				}
			</style>
			<div id="ytodoOrigStatusBarBkup_Remove">
				<input type="button" text="Btn1" style="width: 30px;"/>
				<input type="button" text="Btn1" style="width: 30px;"/>
					<span class="inlineValue crmStyle-statusbar-separator">Arrival/Departure: 2014-07-02 - 2014-07-06</span>
					<span class="inlineValue crmStyle-statusbar-separator">Status: 10 (Definite)</span>
					<span class="inlineValue crmStyle-statusbar-separator crmStyle-actualized">ACTUALIZED  <span 
						class="crmStyle-actualized">
						<div class="actualized-days">Prior: 2</div>
						<div class="actualized-days">Post: 2</div></span></span>
					<span class="inlineValue crmStyle-actualized">Total Room Nights:  </span>
					<span class="inlineValue statusbar crmStyle-actualized" id="Span1">1508</span>
					<span class="inlineValue crmStyle-actualized">Peak Room Nights:  </span>
					<span class="inlineValue crmStyle-statusbar-separator crmStyle-actualized" id="Span4">788</span>
					</span>
				<span class="row-fill-rest"></span>
				</div>
			--%>
			</div> <%-- gridContainer --%>




		<%-- Add/Remove day buttons Below--%>
		<div id="AddDayButtonsBelow" runat="server" 
				class="add-day-button-container"
				>
			<%-- Note sequence and positioning is by js--%>
			<div class='add-day-button-spacer'></div><!--
			--><div id="RemoveActualDayBelow" runat="server" 
					class="add-day-button add-day-button-up"
					onclick="javascript:DivClicked(this); return true;" 
					title="Remove Day"
					>
					</div>
			<div id="AddActualDayBelow" runat="server" 
					class="add-day-button add-day-button-down"
					onclick="javascript:DivClicked(this); return true;" 
					title="Add Day"
					>
				</div>
			<div id="BottomActualDaysHelpText" runat="server" 
					class="add-day-button add-day-button-text"
					>Add/Remove Day</div></div>


		<%-- Actual Entry Complete: Yes/No Select/Radio --%>	

		<script>
			function UpdateActualEntryComplete(ctrl) {
				ctrl.style.display = 'none';
				ctrl.parentNode.firstChild.textContent = ctrl.parentNode.firstChild.textContent + ' (Saving...)';
				javascript: DivClicked(ctrl, ctrl.value);
			}
		</script>
		<div id="ActualEntryComplete" runat="server"
			class="c-relative followcolumn_" >Actualized Entry Complete<!-- remove space
			--><select name="Actualized Entry Complete" id="ActualEntryCompleted" runat="server" 
				visible="true" 
				onchange="this.disabled = false; UpdateActualEntryComplete(this);"
				UseSubmitBehavior="false">
				<option value="0" selected="selected">No</option>
				<option value="1">Yes</option></select></div>


		
		</div>	<%-- <!-- centreInMarginsDebug 
							Main Container-->--%>



		<%-- debug grid - all records in db grid --%>
			<style type="text/css">
				#GridView_AllRecords {
					background-color: white;
					margin-top: 25px;
				}
			</style>
			<asp:GridView ID="GridView_AllRecords" runat="server" Visible="false"
						Width="100%" GridLines="Both" AutoGenerateColumns="False"
						CssClass="grid" 
						OnRowDataBound="GridView1_RowDataBound"
						OnPreRender="GridView1_PreRender"
						DataKeyNames="Day Number,Day Of Week,Date">
			<HeaderStyle CssClass="gridHeader" />
			<RowStyle Height="24px" />
			<Columns>
				<asp:BoundField DataField="Day Number" HeaderText="Day Number" ItemStyle-Width="15%"
								HeaderStyle-Font-Size="11px" HeaderStyle-CssClass="cell" ItemStyle-Font-Size="11px" ItemStyle-CssClass="gridcell"/>
				<asp:BoundField DataField="Day Of Week" HeaderText="Day Of Week" ItemStyle-Width="15%"
								HeaderStyle-Font-Size="11px" HeaderStyle-CssClass="cell" ItemStyle-Font-Size="11px" ItemStyle-CssClass="gridcell"/>
				<asp:BoundField DataField="Date" HeaderText="Date" 
								ItemStyle-Width="7%" HeaderStyle-Font-Size="11px"
								HeaderStyle-CssClass="cell" ItemStyle-Font-Size="11px" ItemStyle-CssClass="gridcell"/>
				<asp:TemplateField HeaderText="Original % of Peak"><HeaderStyle Font-Size="11px" CssClass="cell" />
					<ItemStyle Width="11%" Font-Size="11px" CssClass="gridCell"/><ItemTemplate>
						<asp:Label ID="lblOPeak" runat="server" 
									Text='<%#DataBinder.Eval(Container.DataItem, "Original % of Peak")%>'></asp:Label>
						<input type="hidden" id="hdnOrigPeakPercent" runat="server" 
									value='<%#DataBinder.Eval(Container.DataItem, "Original % of Peak")%>' 
									/>
					                                                             </ItemTemplate></asp:TemplateField>
				<asp:TemplateField HeaderText="Original Block">
					<ItemTemplate>
						<asp:Label ID="lblOblock" runat="server" 
									Text='<%#DataBinder.Eval(Container.DataItem, "Original Block")%>'></asp:Label>
						<input type="hidden" id="hdbOrigBlock" runat="server" 
									value='<%#DataBinder.Eval(Container.DataItem, "Original Block")%>' 
									/>
																							</ItemTemplate>
					<ItemStyle Width="11%" Font-Size="11px" />
					<HeaderStyle Font-Size="11px" CssClass="cell" /></asp:TemplateField>
				<asp:TemplateField HeaderText="Current % of Peak">
					<ItemTemplate>
						<asp:label id="lblCPeak" runat="server" EnableViewState="true"
									Text='<%#DataBinder.Eval(Container.DataItem, "Current % of Peak")%>' 
									/>
						<input type="hidden" id="hdnCurrentPeak" runat="server" 
									value='<%#DataBinder.Eval(Container.DataItem, "Current % of Peak")%>' 
									/>
						</ItemTemplate>
					<ItemStyle Width="11%" Font-Size="11px" />
					<HeaderStyle Font-Size="11px" CssClass="cell" /></asp:TemplateField>
				<asp:TemplateField HeaderText="Current Block">
					<ItemTemplate>
						<asp:TextBox ID="txtCBlock" runat="server" 
							Text='<%#DataBinder.Eval(Container.DataItem, "Current Block")%>'
							onfocusout=";" 
							onkeypress="return false;" 
							onpaste="return false;"
							MaxLength="5"
							autoComplete="off" ></asp:TextBox>
																</ItemTemplate>
					<ItemStyle Width="11%" Font-Size="11px" />
					<HeaderStyle Font-Size="11px" CssClass="cell" /></asp:TemplateField>
				<asp:BoundField DataField="Room Guid" HeaderText="Room Guid" 
								ItemStyle-Width="15%" HeaderStyle-Font-Size="11px" ItemStyle-Font-Size="11px"
								HeaderStyle-CssClass="cell"  >
					<ItemStyle 
						CssClass="hidden_boundfld__"/></asp:BoundField>
				<asp:TemplateField HeaderText="Actual % of Peak">
					<ItemTemplate>
						<asp:label ID="txtActualPercentOfPeak" runat="server"
									Text='<%#DataBinder.Eval(Container.DataItem, "Actual % of Peak")%>' 
									/>
						<input type="hidden" id="hdnActualPeak" runat="server" 
									value='<%#DataBinder.Eval(Container.DataItem, "Actual % of Peak")%>' 
									/>
																			</ItemTemplate>
					<ItemStyle Width="11%" Font-Size="11px" />
					<HeaderStyle Font-Size="11px" CssClass="cell" /></asp:TemplateField>
				<asp:TemplateField HeaderText="Actualized Block">
					<ItemTemplate>
						<asp:TextBox ID="txtActualBlock" runat="server"
									Text='<%#DataBinder.Eval(Container.DataItem, "Actual Block")%>'
									onfocusout=";"
									onkeypress="return false;" 
									onpaste="return false;" 
									MaxLength="5" 
									autoComplete="off"></asp:TextBox></ItemTemplate>
					<ItemStyle Width="11%" Font-Size="11px" />
					<HeaderStyle Font-Size="11px" CssClass="cell" /></asp:TemplateField>
			</Columns>
			</asp:GridView>




		</div><%-- centreInMarginsDebug--%>

		<div id="tailEnd" runat="server" />
    </form>

	<div id="loading" runat="server" visible="true" 
		style="vertical-align:middle; text-align:center; margin-top: 3em;">Loading...</div>
</body>
</html>
