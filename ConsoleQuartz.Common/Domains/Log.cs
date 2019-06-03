
using ConsoleQuartz.Common;
using NLog;
using System;
using System.Text;

namespace ConsoleQuartz.Common
{
	public class Log : ILog
	{
		private int spaceCount;

		private int indentLvl;

		public int IndentLevel
		{
			get
			{
				return indentLvl;
			}
			set
			{
				indentLvl = value;
			}
		}

		public int IndentSize
		{
			get
			{
				return spaceCount;
			}
			set
			{
				spaceCount = value;
			}
		}

		public void Flush()
		{
			TraceUtil.Instance.Flush();
		}

		public void Write(string message)
		{
			TraceUtil.Instance.TraceMsgByLevel(NLog.LogLevel.Info, DecorateMsg(message));
		}

		public void Write(string message, params object[] args)
		{
			message = GetFormattedString(message, args);
			TraceUtil.Instance.TraceMsgByLevel(NLog.LogLevel.Info, DecorateMsg(message));
		}

		public void WriteIf(bool condition, string message)
		{
			if (condition)
			{
				Write(message);
			}
		}

		public void WriteIf(bool condition, string message, params object[] args)
		{
			if (condition)
			{
				Write(message, args);
			}
		}

		public void WriteError(string message)
		{
			TraceUtil.Instance.TraceMsgByLevel(NLog.LogLevel.Error, DecorateMsg(message));
		}

		public void WriteError(string message, params object[] args)
		{
			message = GetFormattedString(message, args);
			TraceUtil.Instance.TraceMsgByLevel(NLog.LogLevel.Error, DecorateMsg(message));
		}

		public void WriteErrorIf(bool condition, string message)
		{
			if (condition)
			{
				WriteError(message);
			}
		}

		public void WriteErrorIf(bool condition, string message, params object[] args)
		{
			if (condition)
			{
				WriteError(message, args);
			}
		}

		public void WriteWarning(string message)
		{
			TraceUtil.Instance.TraceMsgByLevel(NLog.LogLevel.Warn, DecorateMsg(message));
		}

		public void WriteWarning(string message, params object[] args)
		{
			message = GetFormattedString(message, args);
			TraceUtil.Instance.TraceMsgByLevel(NLog.LogLevel.Warn, DecorateMsg(message));
		}

		public void WriteWarningIf(bool condition, string message)
		{
			if (condition)
			{
				WriteWarning(message);
			}
		}

		public void WriteWarningIf(bool condition, string message, params object[] args)
		{
			if (condition)
			{
				WriteWarning(message, args);
			}
		}

		public void Indent()
		{
			if (indentLvl != 2147483647)
			{
				indentLvl++;
			}
		}

		public void Unindent()
		{
			if (indentLvl != 0)
			{
				indentLvl--;
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

		private string DecorateMsg(string message)
		{
			return string.Empty.PadLeft(indentLvl * spaceCount) + message;
		}
	}
}
