using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimitierModManager
{

	public interface IErrorCollector
	{
		public bool HasErrors { get; }

		public void LogError(string message);

		public void LogError(string message, Exception exception)
		{
			LogError(message);
		}

		public void Clear();

		public string ErrorsToString();

		public string ErrorsToVerboseString() 
		{ 
			return ErrorsToString(); 
		}
	}



	public class ErrorCollector : IErrorCollector
	{

		private StringBuilder _errorsStringBuilder = new StringBuilder();
		private StringBuilder _verboseStringBuilder = new StringBuilder();
		public bool HasErrors { get; private set; } = false;

		public void LogError(string message)
		{
			HasErrors = true;
			_errorsStringBuilder.AppendLine(message);

			_verboseStringBuilder.AppendLine();
			_verboseStringBuilder.AppendLine("Message:");
			_verboseStringBuilder.AppendLine(message);
			_verboseStringBuilder.AppendLine("InternalException: not given");
		}
		public void LogError(string message, Exception exception)
		{
			HasErrors = true;

			_errorsStringBuilder.AppendLine(message);

			_verboseStringBuilder.AppendLine();
			_verboseStringBuilder.AppendLine("Message:");
			_verboseStringBuilder.AppendLine(message);
			_verboseStringBuilder.AppendLine("InternalException:");
			_verboseStringBuilder.AppendLine(exception.ToString());

		}

		public void Clear()
		{
			HasErrors = false;
			_errorsStringBuilder.Clear();
			_verboseStringBuilder.Clear();
		}


		public string ErrorsToString()
		{
			return _errorsStringBuilder.ToString();
		}

		public string ErrorsToVerboseString()
		{
			return _verboseStringBuilder.ToString();
		}


		public static IErrorCollector Discard = new DiscardCollector();

	}

	public class DiscardCollector : IErrorCollector
	{
		public bool HasErrors { get; } = false;

		public void LogError(string message) { }

		public void Clear() { }

		public string ErrorsToString() { return String.Empty; }
	}

}
