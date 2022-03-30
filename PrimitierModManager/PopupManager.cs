using MaterialDesignThemes.Wpf;
using PrimitierModManager.Dialogs;
using System;
using System.Collections.Generic;

using System.Windows;
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




		public static void ShowErrorPopup(string message)
		{
			if (App.MainWindow == null)
			{
				var result = MessageBox.Show(message, "Error", MessageBoxButton.OK);
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
