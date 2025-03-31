using System;
#if true
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using log4net;
using log4net.Appender;
using System.Globalization;
namespace RoomBlock
{
    public class LoggerClass
    {
#region Using Directives
        static ILog logWriter = LogManager.GetLogger(typeof(LoggerClass));
        private static RollingFileAppender rollingFileAppender = new RollingFileAppender();
        private static ILog logger = LoggerClass.Logger();
        RollingFileAppender rfa = new RollingFileAppender();
		#endregion

#region Constructor
        public LoggerClass()
        {
            try
            {
                string logFilePath = AppDomain.CurrentDomain.BaseDirectory.ToString() + "BCMC Room block Log\\"; ;
                AppSettingsReader appReader = new AppSettingsReader();
                int maxRollbackSize = (int)appReader.GetValue("maxRollbackSize", typeof(int));
                string maximumFileSize = (string)appReader.GetValue("maximumFileSize", typeof(string));
                string datePattern = (string)appReader.GetValue("datePattern", typeof(string));
                string conversionPattern = (string)appReader.GetValue("conversionPattern", typeof(string));
                string level = (string)appReader.GetValue("level", typeof(string));
                string LogFileName = (string)appReader.GetValue("LogFileName", typeof(string));
                LoggerClass.ConfigureLog4net(logFilePath, maxRollbackSize, maximumFileSize, datePattern, conversionPattern, level, LogFileName);
                WriteVersionInfo(maxRollbackSize, logFilePath, datePattern, conversionPattern, LogFileName, level, maximumFileSize);
            }
            catch (Exception generalException)
            {
                throw new Exception(generalException.ToString());
            }
        }
		#endregion

        /// <summary>
        /// Get the Logger object 
        /// </summary>
        /// <returns>Logger object</returns>
#region Logger
        public static ILog Logger()
        {
            try
            {
                if (logWriter != null)
                    return logWriter;
                else
                {
                    logWriter = LogManager.GetLogger(typeof(LoggerClass));
                    return logWriter;
                }
            }
            catch (Exception generalException)
            {
                throw new Exception(Convert.ToString("Error Occured in Looger ", CultureInfo.CurrentCulture) + generalException.ToString());
            }

        }
		#endregion

        /// <summary>
        /// To Configure Logger
        /// </summary>
        /// <param name="LogFilePath">Log File Path</param>
        /// <param name="maxRollBackSize">Maximum RollBack Size</param>
        /// <param name="maximumFileSize">Maximum File Size</param>
        /// <param name="datePattern">Date Pattern</param>
        /// <param name="conversionPattern">Conversion Pattern</param>
        /// <param name="level">level</param>
#region Configuring Log4Net
        public static void ConfigureLog4net(string logFilePath, int maxRollbackSize, string maximumFileSize,
            string datePattern, string conversionPattern, string level, string LogFileName)
        {
            if (logFilePath == null)
            {
                throw new ArgumentNullException(Convert.ToString("Input parameter is Null", CultureInfo.CurrentCulture));

            }
            if (maximumFileSize == null)
            {
                throw new ArgumentNullException(Convert.ToString("Input parameter is Null", CultureInfo.CurrentCulture));

            }
            if (datePattern == null)
            {
                throw new ArgumentNullException(Convert.ToString("Input parameter is Null", CultureInfo.CurrentCulture));

            }
            if (conversionPattern == null)
            {
                throw new ArgumentNullException(Convert.ToString("Input parameter is Null", CultureInfo.CurrentCulture));
            }
            if (level == null)
            {
                throw new ArgumentNullException(Convert.ToString("Input parameter is Null", CultureInfo.CurrentCulture));
            }
            try
            {
                if (LogManager.GetRepository().Configured == false)
                {
                    rollingFileAppender.File = logFilePath + LogFileName + DateTime.Now.Year.ToString("d4", CultureInfo.CurrentCulture) + DateTime.Now.Month.ToString("d2", CultureInfo.CurrentCulture) + DateTime.Now.Day.ToString("d2", CultureInfo.CurrentCulture) +
                        Convert.ToString("_", CultureInfo.CurrentCulture) + DateTime.Now.Hour.ToString("d2") + DateTime.Now.Minute.ToString("d2", CultureInfo.CurrentCulture) + DateTime.Now.Second.ToString("d2", CultureInfo.CurrentCulture) + Convert.ToString("_", CultureInfo.CurrentCulture) + DateTime.Now.Millisecond.ToString("d4", CultureInfo.CurrentCulture) + ".log";
                    rollingFileAppender.StaticLogFileName = true;
                    rollingFileAppender.CountDirection = 4;
                    rollingFileAppender.AppendToFile = true;
                    rollingFileAppender.MaxSizeRollBackups = maxRollbackSize;
                    rollingFileAppender.MaximumFileSize = maximumFileSize;
                    rollingFileAppender.RollingStyle = RollingFileAppender.RollingMode.Size;
                    setThreshold(level);
                    // rollingFileAppender.Threshold = log4net.Core.level.Debug;    

                    rollingFileAppender.DatePattern = datePattern;
                    log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout();
                    layout.ConversionPattern = conversionPattern;
                    layout.ActivateOptions();

                    rollingFileAppender.Layout = layout;
                    rollingFileAppender.ActivateOptions();
                    log4net.Config.BasicConfigurator.Configure(rollingFileAppender);
                    // AppConstants.IsLoggerConfigured = true;
                }
            }
            catch (Exception generalException)
            {
                throw new Exception(Convert.ToString("Exception occurred in Configurelog4net :", CultureInfo.CurrentCulture) + generalException.ToString());
            }

        }

        void WriteVersionInfo(int maxRollbackSize, string logFilePath, string datePattern, string conversionPattern,
            string LogFileName, string level, string maximumFileSize)
        {
            logger.Info("Logger Process Started");
            logger.Info("*********************************************************");
            logger.Info("Name of The Project : " + "MIS Service");
            logger.Info("Release Version : V1.0.000.00");
            logger.Info("Environment OS :" + System.Environment.OSVersion.VersionString);
            logger.Info("Frame work :  " + System.Environment.Version.ToString());
            logger.Info("LogFileName -  " + logFilePath);
            logger.Info("MaxSizeRollBackups - " + maxRollbackSize);
            logger.Info("MaximumFileSize -  " + maximumFileSize);
            logger.Info("DatePattern -    " + datePattern);
            logger.Info("ConversionPattern -   " + conversionPattern);
            logger.Info("Logger Details assigned Successfully");
            logger.Info("End of the Logger Initialising Process");
            logger.Info("*********************************************************");
            logger.Info(Environment.NewLine);
        }
		#endregion

        /// <summary>
        /// To Set Threshold Level
        /// </summary>
        /// <param name="level">level</param>
#region setThreshold
        private static void setThreshold(string level)
        {
            try
            {
                if (level.Trim().Equals("INFO"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Info;
                }
                else if (level.Trim().Equals("DEBUG"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Debug;
                }
                else if (level.Trim().Equals("WARN"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Warn;
                }
                else if (level.Trim().Equals("ERROR"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Error;
                }
                else if (level.Trim().Equals("ALERT"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Alert;
                }
                else if (level.Trim().Equals("CRITICAL"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Critical;
                }
                else if (level.Trim().Equals("EMERGENCY"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Emergency;
                }
                else if (level.Trim().Equals("FATAL"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Fatal;
                }
                else if (level.Trim().Equals("FINE"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Fine;
                }
                else if (level.Trim().Equals("FINER"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Finer;
                }
                else if (level.Trim().Equals("NOTICE"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Notice;
                }
                else if (level.Trim().Equals("OFF"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Off;
                }
                else if (level.Trim().Equals("SEVERE"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Severe;
                }
                else if (level.Trim().Equals("TRACE"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Trace;
                }
                else if (level.Trim().Equals("VERBOSE"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.Verbose;
                }
                else if (level.Trim().Equals("ALL"))
                {
                    rollingFileAppender.Threshold = log4net.Core.Level.All;
                }
            }
            catch (Exception generalException)
            {
                throw new Exception(Convert.ToString("Exception occurred in Configurelog4net :", CultureInfo.CurrentCulture) + generalException.ToString());
            }

        }
		#endregion
    }
}
#endif