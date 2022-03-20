using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public static void ShowError(IErrorCollector collector, bool clearCollector=true)
		{
			if (App.MainWindow == null)
			{
				return;
			}

			var errorPopup = App.MainWindow.ErrorPopup;
			errorPopup.Message.Content = "ERROR"+collector.ErrorsToString();
			errorPopup.IsActive = true;

			if (clearCollector)
			{
				collector.Clear();
			}

		}


		public static void ShowError(string error)
		{
			if (App.MainWindow == null)
			{
				return;
			}

			var errorPopup = App.MainWindow.ErrorPopup;
			errorPopup.Message.Content ="Error:"+error;
			errorPopup.IsActive = true;

		}

	}
}
