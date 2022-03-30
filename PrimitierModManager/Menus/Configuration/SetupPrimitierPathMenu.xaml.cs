﻿using MaterialDesignThemes.Wpf;
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

		private void ResetDragDrop()
		{
			DropTarget.AllowDrop = true;
			DropTargetText.Text = "Drag Primiter.exe into here";
			ButtonProgressAssist.SetIsIndicatorVisible(DropTarget, false);
		}

		private void SetDragDrop(string filePath)
		{
			
			DropTarget.AllowDrop = false;
			DropTargetText.Text = "Loading";
			ButtonProgressAssist.SetIsIndicatorVisible(DropTarget, true);

			Task.Factory.StartNew(() => Setup.SetupPrimitierExe(filePath, Dispatcher))
			.ContinueWith(t =>
			{
				Dispatcher.Invoke(() =>
				{
					if (Setup.SetupPrimitierExeError != "")
					{
						ResetDragDrop();
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
			App.MainWindow.SwitchMenu(0);

		}

	}
}
