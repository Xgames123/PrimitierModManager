using MaterialDesignThemes.Wpf;
using PrimitierModManager.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PrimitierModManager
{




	public static class PopupManager
	{
		private static UpdateDialog UpdateDialog = null;


		public static void ShowUpdatePopup(Uri link)
		{

			UpdateDialog = new UpdateDialog(link);
			DialogHost.Show(UpdateDialog);
			
		}



		public static void ShowErrorPopupWriteToFile(IErrorCollector collector, bool clearCollector=true)
		{
			if (!collector.HasErrors)
			{
				return;
			}

			ShowErrorPopup(collector.ErrorsToString());

			Directory.CreateDirectory("Logs");

			File.AppendAllText($"Logs/{DateTime.UtcNow.Minute}-{DateTime.UtcNow.Day}-{DateTime.UtcNow.Month}-{DateTime.UtcNow.Year}.log", collector.ErrorsToVerboseString());


			if (clearCollector)
			{
				collector.Clear();
			}

		}


		public static void ShowErrorPopup(string message)
		{
			if (App.MainWindow == null)
			{
				return;
			}

			App.MainWindow.Dispatcher.Invoke(() =>
			{
				var errorDialog = new ErrorDialog(message);
				DialogHost.Show(errorDialog);

			
			});




		}

	}
}
