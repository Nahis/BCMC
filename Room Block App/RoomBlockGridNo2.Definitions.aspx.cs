namespace RoomBlocks2
{
	public partial class Grid : System.Web.UI.Page
	{
#if true

		//Xrm.OpportunityState
		public enum StateCodes //c.f. OpportunityState
		{
			// Won and lost opportunities are read-only and can't be edited until they are reactivated.
			Open = 0, Won = 1, Lost = 2 //ytodo need to clarify values. equivalent!?... Open,Actualized,Cancelled
			
		}
		// This is OpportunityState...
		/*
		 * c.f. public enum LeadState
		 * http://mostlymscrm.blogspot.com/2012/06/entity-statecodes-and-statuscodes.html
		 * 
		 * regex
		 *	public enum [A-Za-z]*Lead[a-zA-Z]*State
		 * 
		 */

		public enum EventStatusCodes
		{
			Prospect = 1,
			Pending = 9,
			Definite = 10,
			//Actualized = 200, //ytodo c.f. to leadStatus
			//NotDefinite = -1, //ytodo
		}

		/* compare to portal
		 * 
		Xrm.Bcmc_hotelleadState.Active = 0;
		Xrm.Bcmc_hotelleadState.InActive = 1;

		enum Bcmc_HotelLeadStatusCode
		{
			Pending = 1,					//Pending = 1
			AcceptedFirstOptionBasis =  10,	//Accepted 1st Option (Hotel has agreed to hold rooms on a first option basis) = 10
			AcceptedSecondOptionBasis =  5,	//Accepted 2nd Option (Hotel has agreed to hold rooms on a second option basis) = 5
			Declined =  6,					//Declined = 6
			Contracted =  7,				//Contracted = 7
			Actualized =  8,				//Actualized = 8
			Cancelled = 2,					//doesn't exist in reality
		};
		*/

		enum GridColumnsFromJavaScript //ytododev remove
		{
			Date = 0,
			DayOfWeek = 1,
			CityWideBlocks = 2,
			ProposedBlock = 3,
			BellCurve = 4,
			ContractedBlock = 5,
			ContractedBellCurve = 6,
			ActualPercentageOfPeak = 7,
			ActualBellCurve = 7,
			ActualBlock = 8,
			Variance = 9
		};

		public enum Columns
		{
			DayNumber = 0,
			DayOfWeek = 1,
			Date,
			OriginalPercentOfPeak,
			OriginalBlock,
			CurrentPercentOfPeak,
			CurrentBlock,
			//EventId, //7
			RoomBlockId, //7
			ActualPercentOfPeak,
			ActualBlock,
		};

		static string[] Titles = {
			"Day Number",
			"Day Of Week",
			"Date",
			"Original % of Peak",
			"Original Block",
			"Current % of Peak",
			"Current Block",
			"Room Guid", //8
			"Actual % of Peak",
			"Actual Block",
		};

		public enum AllowRoomBlocksDelete
		{
			False = 0,
			True = 1
		};


		static public string Heading(Columns col)
		{
			return Grid.Titles[(int)col];
		}

		static public int TitleIdx(Columns col)
		{
			return (int)col;
		}
		/*protected string Ctrl(Columns col)
		{
			return (int)col;
		}*/

		public enum GridCtrlNames
		{
			lblOPeak,
			hdnfdValue, //Current PercentOfPeak
			lblOblock,
			txtCBlock,
			txtActualPercentOfPeak, //ytodo change to lbl
			hdnActualPercentOfPeak,
			txtActualPct, //readonly txtbox % of peak
			txtActualBlock,
			// ----------------------
			// Miscellaneous controls
			HiddenField22, //ytodo remove if not using.
		}

		public static System.Data.DataTable dtPersistant; //ytodo this needs to be fixed

		static public int DR(System.Data.DataRow dr, Columns col, int? val)
		{
			if (val.HasValue)
			{
				if (val != null)
				{
					dr[Heading(col)] = val.Value;
					return 1;
				}
				//else
				//	dr[Heading(col)] = "0";
			}
			return 0;
		}
		static public int DR(System.Data.DataRow dr, Columns col, string val)
		{
			if (val != null)
			{
				dr[Heading(col)] = val;
				return 1;
			}
			return 0;
		}
		static public int DR(System.Data.DataRow dr, Columns col, System.DateTime? val)
		{
			if (val.HasValue)
			{
				if (val != null)
				{
					dr[Heading(col)] = val.Value.ToShortDateString();
					return 1;
				}
			}
			return 0;
		}

		public static System.Data.DataTable InitDataTable()
		{
			System.Data.DataTable dt = new System.Data.DataTable();
			dt.Columns.Add(Heading(Columns.DayNumber));
			dt.Columns.Add(Heading(Columns.DayOfWeek));
			dt.Columns.Add(Heading(Columns.Date));
			dt.Columns.Add(Heading(Columns.OriginalPercentOfPeak));
			dt.Columns.Add(Heading(Columns.OriginalBlock));
			dt.Columns.Add(Heading(Columns.CurrentPercentOfPeak));
			dt.Columns.Add(Heading(Columns.CurrentBlock));
			dt.Columns.Add(Heading(Columns.RoomBlockId));
			dt.Columns.Add(Heading(Columns.ActualPercentOfPeak));
			dt.Columns.Add(Heading(Columns.ActualBlock));

			return dt;
		}

		//origname: InitNewEmptyRoomBlockGrid()
		public static void InitNewEmptyRoomBlockGrid(EventActualRoomblocks eventWrapper)
		{
			try
			{
				System.Data.DataTable dt = InitDataTable();

				if (!eventWrapper.gridviewClass.InvalidEventId)
				{
					System.DateTime arrivalDate = eventWrapper.arrivalDate;
					for (int i = 0; i < eventWrapper.departureDate.Subtract(eventWrapper.arrivalDate).TotalDays; i++)
					{
						System.Data.DataRow dr = dt.NewRow();
						DR(dr, Columns.DayNumber, (i + 1).ToString());
						DR(dr, Columns.DayOfWeek, arrivalDate.AddDays(i).DayOfWeek.ToString());
						DR(dr, Columns.Date, arrivalDate.AddDays(i));
						dt.Rows.Add(dr);
					}
				}

				System.Web.UI.WebControls.GridView gv = eventWrapper.GV;
				gv.DataSource = dt;
				gv.DataBind();
				//gv.Columns[(int)Columns.DayNumber].Visible = EventEntityDataBase.DebugShowSecondGrid ? true : false;
				//gv.Columns[(int)Columns.RoomBlockId].Visible = EventEntityDataBase.DebugShowSecondGrid || bSHOW_ROOM_GUID ? true : false; //ytodofix
			}
			catch (System.Web.Services.Protocols.SoapException ex)
			{
				logger.Error(ex.Detail.InnerText, ex);
			}
			catch (System.Exception ex)
			{
				logger.Error("RoomBlockGrid" + ex.ToString());
			}
			finally
			{
			}
		}

#endif

	}
}
