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
	internal sealed class TraceUtil
	{
		private Logger log;

		private static readonly TraceUtil instance = new TraceUtil();

		public static TraceUtil Instance => instance;

		private TraceUtil()
		{
			try
			{
				log = LogManager.GetCurrentClassLogger();
			}
			catch (Exception)
			{
				try
				{
					log = InitLoggerConfiguration();
				}
				catch (Exception ex)
				{
					EventLog.WriteEntry(base.GetType().FullName + ".TraceUtil", ex.Message, EventLogEntryType.Error);
					Trace.WriteLine(ex.Message);
				}
			}
		}

		private Logger InitLoggerConfiguration()
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
				BufferSize = 64,
				ArchiveAboveSize = 2097152,
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
			return LogManager.GetCurrentClassLogger();
		}

		public void TraceMsgByLevel(NLog.LogLevel logLevel, string errorMsg)
		{
			try
			{
				Instance.log.Log(logLevel, errorMsg);
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
				try
				{
					EventLog.WriteEntry(base.GetType().FullName + ".TraceMsgByLevel", ex.Message, EventLogEntryType.Error);
				}
				catch (Exception ex2)
				{
					Trace.WriteLine(ex2.Message);
				}
			}
		}

		public void Flush()
		{
			if (Instance.log != null)
			{
				try
				{
					Instance.log.Factory.Flush();
				}
				catch (Exception ex)
				{
					try
					{
						EventLog.WriteEntry(base.GetType().FullName + ".Flush", ex.Message, EventLogEntryType.Error);
					}
					catch (Exception ex2)
					{
						Trace.WriteLine(ex2.Message);
					}
				}
			}
		}
	}
}
