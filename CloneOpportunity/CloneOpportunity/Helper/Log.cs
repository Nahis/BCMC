using System;
using System.Diagnostics;

//***************************************************
//© 2007 PnP Solutions Pty Ltd.  All rights reserved.
//***************************************************

	/// <summary>
	/// Summary description for logging.
	/// </summary>
	public class log
	{
		/// <summary>
		/// Writes the input message to the server event log
		/// </summary>
		/// <param name="strMessage">the message</param>
		public static void logError( string strMessage )
		{
			try
			{
				EventLog objLog;
				objLog = new EventLog();
				objLog.Source = "MSCRM";
				objLog.WriteEntry( strMessage, EventLogEntryType.Error );
			}
			catch (Exception ex){
                throw ex;
            }
		}


        /// <summary>
        /// Writes the input message to the server event log
        /// </summary>
        /// <param name="strMessage">the message</param>
        public static void logInfo(string strMessage)
        {
            try
            {
                EventLog objLog;
                objLog = new EventLog();
                objLog.Source = "MSCRM";
                objLog.WriteEntry(strMessage, EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
	}

