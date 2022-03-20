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

		public void Clear();

		public string ErrorsToString();
	}



	public class ErrorCollector : IErrorCollector
	{

		private StringBuilder _errorsStringBuilder = new StringBuilder();
		public bool HasErrors { get; private set; } = false;

		public void LogError(string message)
		{
			HasErrors = true;
			_errorsStringBuilder.AppendLine(message);
		}

		public void Clear()
		{
			_errorsStringBuilder.Clear();
		}


		public string ErrorsToString()
		{
			return _errorsStringBuilder.ToString();
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
