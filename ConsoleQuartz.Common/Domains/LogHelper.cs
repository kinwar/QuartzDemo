#define TRACE
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Diagnostics;
using System.Text;

namespace ConsoleQuartz.Common
{
	public class LogHelper : IDisposable
	{
		private Logger log;

		private LogHelper(Type ty)
		{
			try
			{
				log = LogManager.GetCurrentClassLogger(ty);
			}
			catch (Exception)
			{
				try
				{
					log = InitLoggerConfiguration(ty);
				}
				catch (Exception ex)
				{
					EventLog.WriteEntry(base.GetType().FullName + ".TraceUtil", ex.Message, EventLogEntryType.Error);
					Trace.WriteLine(ex.Message);
				}
			}
		}

		private LogHelper(string loggerName)
		{
			try
			{
				log = LogManager.GetLogger(loggerName);
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry(base.GetType().FullName + ".TraceUtil", ex.Message, EventLogEntryType.Error);
			}
		}

		public static LogHelper GetLogHelper(string loggerName)
		{
			return new LogHelper(loggerName);
		}

		public static LogHelper GetLogHelper(Type ty)
		{
			return new LogHelper(ty);
		}

		public void Dispose()
		{
			if (log != null)
			{
				log = null;
			}
		}

		public void WriteFormatLog(LogLevel level, string msg, params object[] senders)
		{
			switch (level)
			{
			case LogLevel.Fatal:
				WriteMsg(NLog.LogLevel.Fatal, GetFormattedString(msg, senders));
				break;
			case LogLevel.Error:
				WriteMsg(NLog.LogLevel.Error, GetFormattedString(msg, senders));
				break;
			case LogLevel.Warn:
				WriteMsg(NLog.LogLevel.Warn, GetFormattedString(msg, senders));
				break;
			case LogLevel.Info:
				WriteMsg(NLog.LogLevel.Info, GetFormattedString(msg, senders));
				break;
			case LogLevel.Trace:
				WriteMsg(NLog.LogLevel.Trace, GetFormattedString(msg, senders));
				break;
			case LogLevel.Debug:
				WriteMsg(NLog.LogLevel.Debug, GetFormattedString(msg, senders));
				break;
			}
		}

		public void WriteLog(LogLevel level, string msg, params object[] senders)
		{
			if (senders != null && senders.Length != 0)
			{
				msg = msg + " " + string.Join(",", senders);
			}
			switch (level)
			{
			case LogLevel.Fatal:
				WriteMsg(NLog.LogLevel.Fatal, msg);
				break;
			case LogLevel.Error:
				WriteMsg(NLog.LogLevel.Error, msg);
				break;
			case LogLevel.Warn:
				WriteMsg(NLog.LogLevel.Warn, msg);
				break;
			case LogLevel.Info:
				WriteMsg(NLog.LogLevel.Info, msg);
				break;
			case LogLevel.Trace:
				WriteMsg(NLog.LogLevel.Trace, msg);
				break;
			case LogLevel.Debug:
				WriteMsg(NLog.LogLevel.Debug, msg);
				break;
			}
		}

		private string GetFormattedString(string msg, params object[] senders)
		{
			if (senders != null && senders.Length != 0)
			{
				string empty = string.Empty;
				try
				{
					return string.Format(msg, senders);
				}
				catch (Exception ex)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < senders.Length; i++)
					{
						stringBuilder.Append("{" + i + "},");
					}
					return ex.Message + "_" + msg + "_" + string.Format(stringBuilder.ToString(), senders);
				}
			}
			return msg;
		}

		private Logger InitLoggerConfiguration(Type t)
		{
			LoggingConfiguration loggingConfiguration = new LoggingConfiguration();
			FileTarget target = new FileTarget
			{
				ConcurrentWrites = true,
				KeepFileOpen = false,
				CreateDirs = true,
				AutoFlush = true,
				Encoding = Encoding.UTF8,
				OpenFileCacheSize = 1,
				BufferSize = 8,
				ArchiveAboveSize = 5242880,
				FileName = (Layout)"${basedir}/log/${machinename}_${shortdate}.log",
				Layout = (Layout)"${longdate}|${message}",
				Name = "FileTarget",
				CleanupFileName = true
			};
			loggingConfiguration.AddTarget(target);
			LoggingRule item = new LoggingRule("*", NLog.LogLevel.Warn, NLog.LogLevel.Fatal, target);
			loggingConfiguration.LoggingRules.Add(item);
			TraceTarget target2 = new TraceTarget
			{
				Layout = (Layout)"[MC]|${longdate}|${message}",
				Name = "TraceTarget"
			};
			loggingConfiguration.AddTarget(target2);
			LoggingRule item2 = new LoggingRule("*", NLog.LogLevel.Debug, target2);
			loggingConfiguration.LoggingRules.Add(item2);
			LogManager.Configuration = loggingConfiguration;
			return LogManager.GetCurrentClassLogger(t);
		}

		private void WriteMsg(NLog.LogLevel level, string msg)
		{
			if (log == null)
			{
				throw new Exception("No Logger");
			}
			try
			{
				if (level == NLog.LogLevel.Fatal)
				{
					log.Fatal(msg);
				}
				else if (level == NLog.LogLevel.Error)
				{
					log.Error(msg);
				}
				else if (level == NLog.LogLevel.Warn)
				{
					log.Warn(msg);
				}
				else if (level == NLog.LogLevel.Info)
				{
					log.Info(msg);
				}
				else if (level == NLog.LogLevel.Trace)
				{
					log.Trace(msg);
				}
				else if (level == NLog.LogLevel.Debug)
				{
					log.Debug(msg);
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
				try
				{
					EventLog.WriteEntry(base.GetType().FullName + ".wl", ex.Message, EventLogEntryType.Error);
				}
				catch (Exception ex2)
				{
					Trace.WriteLine(ex2.Message);
				}
			}
		}
	}
}
