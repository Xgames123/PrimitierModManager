using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using System.Diagnostics;

namespace PrimitierModManager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static MainWindow MainWindow = null;
		

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			var collector = new ErrorCollector();

			var commandlineArgs = Environment.GetCommandLineArgs();


			if (commandlineArgs.Length > 1)
			{
				if (File.Exists(commandlineArgs[1]))
				{
					ModManager.AddMod(commandlineArgs[1], collector);
					return;
				}

			}


			ConfigFile.Load(collector);


			var IsAlreadyRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location)).Count() > 1;

			if (IsAlreadyRunning)
			{
				App.Current.Shutdown();
				return;
			}

			new MainWindow();
			App.MainWindow.Show();

			PopupManager.ShowErrorPopupWriteToFile(collector);

			Setup.CheckForUpdates(App.Current.MainWindow.Dispatcher);


		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			if (ConfigFile.Config != null)
			{
				ConfigFile.Save();
			}


		}
	}
}
