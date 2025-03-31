using System;
//using log4net;
/*using Microsoft.Xrm.Client.Services; //OrganizationService
//using Microsoft.Xrm.Sdk;
using System.Data;
using Microsoft.Xrm.Sdk.Query; //ColumnSet
*/
using Microsoft.Xrm.Sdk;
using System.Web.UI;
using System.Web.UI.WebControls;
using Xrm; //XrmEarlyBinding;	
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Query;

namespace RoomBlocks2
{
	public partial class Grid : System.Web.UI.Page
	{
		public void DebugUpdatesToDB_ShowInHtml(Microsoft.Xrm.Sdk.AttributeCollection attribs)
		{
			if (!EventEntityDataBase.DebugShowSecondGrid)
				return;

			string htmlBody = "<div>{0}</div>";
			//string htmlEl = "<span>{0}</span>";
			string html = "";
			foreach (var x in attribs)
			{
				string item = String.Format("<span>{0}: {1}</span>", x.Key, x.Value);
				html += item;
			}
			this.DbgInfo.InnerHtml += String.Format(htmlBody, html);
		}

		public Guid Create(Entity e) //CreateRoomPatternRec
		{
			DebugUpdatesToDB_ShowInHtml(e.Attributes);
			if (EventEntityDataBase.DebugDisableSaveToDB)
				return Guid.Empty;
			else
			{
				if (e is New_roompattern) // a special sevice for roompattern
				{
					New_roompattern roompattern = e.ToEntity<New_roompattern>();
					roompattern.New_EventId = new EntityReference(Opportunity.EntityLogicalName, new Guid(hdnfdEventID.Value));
					return _service.Create(roompattern);
				}
				else
					return _service.Create(e);
			}
		}

		public Guid Update(Opportunity o)
		{
			DebugUpdatesToDB_ShowInHtml(o.Attributes);
			if (!EventEntityDataBase.DebugDisableSaveToDB)
			{
				_service.Update(o);
				return o.Id;
			}
			return Guid.Empty;
		}

		public Guid Update(New_roompattern roompattern)
		{
			// When additional days are added 
			// pre and prior for actual roomblock mode
			if (null == roompattern.Id
				|| Guid.Empty == roompattern.Id)
				return Create(roompattern);
			else //the usual case
			{
				DebugUpdatesToDB_ShowInHtml(roompattern.Attributes);
				if (!EventEntityDataBase.DebugDisableSaveToDB)
				{
					_service.Update(roompattern);
					return roompattern.Id;
				}
			}
			return Guid.Empty;
		}
		
		private void UpdateStatusActiveAndUpdateUserForAllRoomPatternDays(EventActualRoomblocks EventWrapper)
		{
			
			EntityCollection roomBlockDaysForEvent = EventWrapper.roomPattern.RecordsOrderedByDate();

			for (int i = 0; i < roomBlockDaysForEvent.Entities.Count; i++)
			{
				try
				{
					Entity entity = roomBlockDaysForEvent.Entities[i];

					New_roompattern roompattern = new New_roompattern();
					roompattern.Id = entity.Id;
					roompattern.Bcmc_statustype = (int)New_roompatternState.Active; //0
					roompattern.bcmc_User = this.Username;
					Update(roompattern);
				}
				catch (Exception ex)
				{
					logger.Error(String.Format("UpdateStatusActiveAndUserForAllRoomPatternDays {0}", roomBlockDaysForEvent.Entities[i].Id), ex);
				}
			}
		}
	}

	public partial class RoomPattern
	{
		Grid Event;
		//System.Collections.ArrayList roompatternList = null;
		EntityCollection roompatternList = null;
		//System.Collections.Generic.List<New_roompattern> roompatternList = null;
		RoomBlocks2.Grid.EventActualRoomblocks EventWrapper = null;

		public RoomPattern(RoomBlocks2.Grid.EventActualRoomblocks EventWrapper)
		{
			this.EventWrapper = EventWrapper;
		}

		//origname: GetRoomPatternRecordsOrderedByDate(
		public EntityCollection RecordsOrderedByDate(bool refetch=false)
		{
			if (!refetch && null != roompatternList)
				return roompatternList;

			/* select from new_roompattern
			 * where new_eventid = eventid
			 * order by bcmc_date asc
			 */
			
			ConditionExpression condition_new_eventid = new ConditionExpression();
			condition_new_eventid.AttributeName = "new_eventid";
			condition_new_eventid.Operator = ConditionOperator.Equal;
			condition_new_eventid.Values.Add(this.EventWrapper.EventId);

			OrderExpression ordeErp = new OrderExpression();
			ordeErp.AttributeName = "bcmc_date";
			ordeErp.OrderType = OrderType.Ascending;

			FilterExpression filter_Eventid = new FilterExpression();
			filter_Eventid.Conditions.Add(condition_new_eventid);

			QueryExpression query = new QueryExpression();
			query.ColumnSet.AddColumns(new string[] { 
				"new_daynumber", 
				"new_name", //dayOfWeek
				"bcmc_date", 
				"bcmc_originalpercentofpeak", 
				"bcmc_originalroomblock", 
				"new_percentofpeak", 
				"new_roomblock",
				"bcmc_actualpercentofpeak",
				"bcmc_actualblock" 
			});
			query.Criteria = filter_Eventid;
			query.Orders.Add(ordeErp);
			query.EntityName = New_roompattern.EntityLogicalName;

			EntityCollection entityCollection = RoomBlocks2.Grid._service.RetrieveMultiple(query);
			return entityCollection;
		}

		//origname: GetNullDateRoomPatternDays(
		public EntityCollection NullDates()
		{
			/* select ... from new_roompattern
			 * where new_eventid = eventid
			 *   and bcmc_date is NULL
			 */
			ColumnSet columnSet = new ColumnSet(new string[] { 
				"new_roompatternid"
			});
			ConditionExpression condition_new_eventid = new ConditionExpression();
			condition_new_eventid.AttributeName = "new_eventid";
			condition_new_eventid.Operator = ConditionOperator.Equal;
			condition_new_eventid.Values.Add(this.EventWrapper.EventId);

			ConditionExpression condition_date = new ConditionExpression();
			condition_date.AttributeName = "bcmc_date";
			condition_date.Operator = ConditionOperator.Null;

			FilterExpression filter_Eventid = new FilterExpression();
			filter_Eventid.FilterOperator = LogicalOperator.And;
			filter_Eventid.Conditions.AddRange(new ConditionExpression[] { condition_new_eventid, condition_date });

			QueryExpression query = new QueryExpression();
			query.ColumnSet = columnSet;
			query.Criteria = filter_Eventid;
			query.EntityName = New_roompattern.EntityLogicalName;

			EntityCollection entityCollection = RoomBlocks2.Grid._service.RetrieveMultiple(query);
			return entityCollection;
		}
		//public System.Collections.Generic.List<New_roompattern> GetRoomPatternWithAscendingDates()
		//{
		//	try
		//	{
		//		BcmcLinqContext LinqProvider = new BcmcLinqContext(RoomBlocks2.Grid._service);
		//		var roomPatternEntityList = LinqProvider
		//			.New_roompatternSet
		//			.Select(s => new { 
		//				s.Id, 
		//				s.New_EventId,
		//				s.New_name, //dayOfWeek
		//				s.Bcmc_Date, 
		//				s.New_PercentofPeak,
		//				s.New_RoomBlock,
		//				s.Bcmc_OriginalpercentofPeak,
		//				s.Bcmc_OriginalRoomBlock,
		//				s.Bcmc_ActualPercentOfPeak,
		//				s.Bcmc_ActualBlock
		//			})
		//			.Where(rp => rp.New_EventId == new EntityReference(Opportunity.EntityLogicalName, new Guid(this.Event.GetUrlEventId))) //yytodo viewstate
		//			.OrderBy(o => o.Bcmc_Date) //as opposed to OrderByDescending
		//			.ToList()
		//			;
		//		//ytodo specify only needed fields

		//		//this.roompatternList = new EntityCollection();
		//		//foreach (var e in roomPatternEntityList)
		//		//{
		//		//	New_roompattern rp = new New_roompattern();
		//		//	foreach (var x in e.)
		//		//	{
		//		//		rp.Attributes.Add(x);
		//		//	}
		//		//	this.roompatternList.Entities.Add(e);
		//		//}

		//		this.roompatternList = roomPatternEntityList;

		//		return this.roompatternList;
		//	}
		//	catch (Exception ex)
		//	{
		//		RoomBlocks2.Grid.logger.Error("GetRoomPatternWithAscendingDates", ex);
		//	}
		//	return null;
		//}

		public void DeleteRoomblock()
		{
			try
			{
				/* select id from new_roompattern
				 * where new_eventid = eventid
				 * 
				 * and delete them all
				 */

				//ColumnSet columnSet = new ColumnSet(new string[] { "new_roompatternid" });

				//ConditionExpression condition = new ConditionExpression();
				//condition.AttributeName = "new_eventid";
				//condition.Operator = ConditionOperator.Equal;
				//condition.Values.Add(this.EventWrapper.eventid);

				//FilterExpression filter = new FilterExpression();
				//filter.Conditions.Add(condition);

				//QueryExpression query = new QueryExpression();
				//query.ColumnSet = columnSet;
				//query.Criteria = filter;
				//query.EntityName = New_roompattern.EntityLogicalName;

				//EntityCollection entityCollection = RoomBlocks2.Grid._service.RetrieveMultiple(query);

				EntityCollection entityCollection = this.RecordsOrderedByDate();
				if (entityCollection.Entities.Count > 0)
				{
					RoomBlocks2.Grid.logger.Info(String.Format(
						"Deleting All blocks ({0})", 
						entityCollection.Entities.Count));
					for (int j = 0; j < entityCollection.Entities.Count; j++)
					{
						New_roompattern entity = (New_roompattern)entityCollection.Entities[j];
						if (entity != null)
						{
							RoomBlocks2.Grid._service.Delete(New_roompattern.EntityLogicalName, new Guid(entity.New_roompatternId.Value.ToString()));
						}
					}
				}
			}
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				RoomBlocks2.Grid.logger.Error(ex.Detail.InnerText, ex);
			}
			catch (Exception ex)
			{
				RoomBlocks2.Grid.logger.Error("DeleteRoomblock " + ex.ToString());
			}
		}


	}
}