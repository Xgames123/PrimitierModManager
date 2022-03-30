using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PrimitierModManager
{
	public static class LogManager
	{

		public static string? LogsPath;
		private static bool s_logsPathCreated=false;

		public static void TryCreateLogsPath()
		{
			if (s_logsPathCreated)
			{
				return;
			}
			try
			{
				ConfigFile.RebuildDirectorySturcture(ErrorCollector.Discard);
				LogsPath = Path.Combine(ConfigFile.PMFDirPath, "Logs");
				Directory.CreateDirectory(LogsPath);
				s_logsPathCreated = true;
			}
			catch (Exception)
			{
				LogsPath = "Logs";
				Directory.CreateDirectory(LogsPath);
			}
		}


		public static void FlushCollector(IErrorCollector collector, bool clearCollector =true)
		{
			if (!collector.HasErrors)
			{
				return;
			}

			PopupManager.ShowErrorPopup(collector.ErrorsToString());


			TryCreateLogsPath();

			File.AppendAllText(Path.Combine(LogsPath, $"{DateTime.UtcNow.Minute}-{DateTime.UtcNow.Day}-{DateTime.UtcNow.Month}-{DateTime.UtcNow.Year}.log"), collector.ErrorsToVerboseString());


			if (clearCollector)
			{
				collector.Clear();
			}

		}

	}
}
