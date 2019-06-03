namespace ConsoleQuartz.Common
{
	public interface ILog
	{
		int IndentLevel
		{
			get;
			set;
		}

		int IndentSize
		{
			get;
			set;
		}

		void Flush();

		void Write(string message);

		void Write(string message, params object[] args);

		void WriteIf(bool condition, string message);

		void WriteIf(bool condition, string message, params object[] args);

		void WriteError(string message);

		void WriteError(string message, params object[] args);

		void WriteErrorIf(bool condition, string message);

		void WriteErrorIf(bool condition, string message, params object[] args);

		void WriteWarning(string message);

		void WriteWarning(string message, params object[] args);

		void WriteWarningIf(bool condition, string message);

		void WriteWarningIf(bool condition, string message, params object[] args);

		void Indent();

		void Unindent();
	}
}
