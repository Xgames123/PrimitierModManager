using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace PrimitierModManager.Menus.Configuration
{
	/// <summary>
	/// Interaction logic for SetupPrimitierPathMenu.xaml
	/// </summary>
	public partial class SetupPrimitierPathMenu : UserControl
	{

		private static string? _draggedFile;

		public SetupPrimitierPathMenu()
		{
			InitializeComponent();

			if (ConfigFile.Config == null && !ConfigFile.Load())
			{
				return;
			}
			if(!string.IsNullOrEmpty(ConfigFile.Config.PrimitierInstallPath))
			{
				SetDragDrop(System.IO.Path.Combine(ConfigFile.Config.PrimitierInstallPath, "Primitier.exe"));
			}


		}

		private void SetDragDrop(string? filePath)
		{
			DropTarget.AllowDrop = true;
			_draggedFile = filePath;

			if (_draggedFile != null)
			{
				DropTargetText.Text = "Primitier path is valid click on next to continue";
				Next.IsEnabled = true;
			}
			else
			{
				DropTargetText.Text = "Drag Primiter.exe into here";
				Next.IsEnabled = false;
			}

			
		}


		private void OnDrop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop))
				return;

			var fileDropData = (string[])e.Data.GetData(DataFormats.FileDrop);

			if (fileDropData == null || fileDropData.Length == 0 || fileDropData[0] == null)
			{
				return;
			}

			SetDragDrop(fileDropData[0]);

		}

		private void NextButtonClick(object sender, RoutedEventArgs e)
		{
			ButtonProgressAssist.SetIsIndicatorVisible(DropTarget, true);
			Task.Factory.StartNew(() => Setup.SetupPrimitierExe(_draggedFile, Dispatcher))
			.ContinueWith(t =>
			{
				Dispatcher.Invoke(() =>
				{
					if (Setup.SetupPrimitierExeError != "")
					{
						SetDragDrop(null);
						PopupManager.ShowErrorPopup(Setup.SetupPrimitierExeError);
					}
					else
					{
						Next.IsEnabled = true;
						DropTargetText.Text = "Done";
						ButtonProgressAssist.SetIsIndicatorVisible(DropTarget, false);
					}

				});


			});

			App.MainWindow.SwitchMenu(0);

		}

	}
}
