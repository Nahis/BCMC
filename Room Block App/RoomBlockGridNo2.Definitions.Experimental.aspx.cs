using System.Data;
using System.Reflection;
//using System.Reflection.TypeInfo;

namespace RoomBlocks2
{
	public partial class Grid : System.Web.UI.Page
	{
#if false

		int[][,] jaggedArray4 = new int[3][,] 
		{
			new int[,] { {1,3}, {5,7} },
			new int[,] { {0,2}, {4,6}, {8,10} },
			new int[,] { {11,22}, {99,88}, {0,9} } 
		};

		//Tuple<Columns, string>
		string[][] x = 
		{
			new string[] { "", "" },
			new string[] { "", "" },
		};
		System.Collections.Generic.Dictionary<Columns, System.Tuple<string, string>> x1 = new System.Collections.Generic.Dictionary<Columns, System.Tuple<string, string>>
		{
			{Columns.ActualBlock, new System.Tuple<string, string>("het", "dll") },
		};
		string x(Columns c, int i) { return x1[c].Item1; }

		System.Collections.Generic.Dictionary<Columns, dynamic> x2 = new System.Collections.Generic.Dictionary<Columns, dynamic>
		{
			{Columns.ActualBlock, new { id = 1, title="title", attrib="new_roomblock"} },
			{Columns.CurrentBlock, new { id = 1, title="Current Block", attrib="new_roomblock" } },
		};
		string x22(Columns c, int i) { return x2[c].title; }

		class t
		{
			public const dynamic ActualBlock = new { order = 1, title = "Current Block", attrib = "new_roomblock" };


			public static DataTable InitDataTable_()
			{
				DataTable dt = new DataTable();
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

				MemberInfo[] members = typeof(t).GetMembers(
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.);

PropertyInfo[] properties = typeof(t).GetProperties();
foreach (PropertyInfo property in properties)
{
    property.GetValue(t, );
}
	
				foreach (MemberInfo member in members)
				{
					//typeof(t).GetMember(
					member.
					ReflectionExtensions.call(member.Name);
				}

				return dt;
			}
		}
		string x22(Columns c, int i) { return t.ActualBlock.title + t.ActualBlock.ToString(); }


		static class ReflectionExtensions
		{
			public static object call(this object o, string methodName, params object[] args)
			{
				var mi = o.GetType().GetMethod(methodName,
					System.Reflection.BindingFlags.NonPublic |
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.Public);
				if (mi != null)
				{
					return mi.Invoke(o, args);
				}
				return null;
			}

			public static object staticCall(string methodName, params object[] args)
			{
				var mi = o.GetType().GetMethod(methodName,
					System.Reflection.BindingFlags.NonPublic |
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.Public);
				if (mi != null)
				{
					return mi.Invoke(o, args);
				}
				return null;
			}
		}
	}

#endif
	}
}
