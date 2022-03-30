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
				if (commandlineArgs[1] == "cleanup")
				{
					Setup.Uninstall(App.Current.Dispatcher, collector, false);
					LogManager.FlushCollector(collector);
					App.Current.Shutdown();
					return;
				}

				if (File.Exists(commandlineArgs[1]))
				{
					ModManager.AddMod(commandlineArgs[1], collector);
					LogManager.FlushCollector(collector);
					App.Current.Shutdown();
					return;
				}

			}


			ConfigFile.Load(collector);


			var IsAlreadyRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location)).Count() > 1;

			if (IsAlreadyRunning)
			{
				LogManager.FlushCollector(collector);
				App.Current.Shutdown();
				return;
			}

			new MainWindow();
			App.MainWindow.Show();

			LogManager.FlushCollector(collector);

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
