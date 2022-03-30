using PrimitierModManager.MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PrimitierModManager
{
	public static class Setup
	{
		public static string UninstallError = "";

		public static string SetupPrimitierExeError = "";

		public static string MelonInstallUninstallError = "";

		public static void SetupPrimitierExe(string primitierExeFilePath, Dispatcher dispatcher)
		{
			SetupPrimitierExeError = "";

			if (!File.Exists(primitierExeFilePath))
			{
				SetupPrimitierExeError = $"Non existing file dragged '{primitierExeFilePath}'";
				return;
			}

			if (Path.GetFileName(primitierExeFilePath) != "Primitier.exe")
			{
				SetupPrimitierExeError = "The file was not Primitier.exe";
				return;
			}


			if (ConfigFile.Config == null)
			{
				ConfigFile.Config = new ConfigFile();
			}
			ConfigFile.Config.PrimitierInstallPath = Path.GetDirectoryName(primitierExeFilePath);


			InstallMelonLoader(primitierExeFilePath, true, dispatcher);
			if (MelonInstallUninstallError != "")
			{
				SetupPrimitierExeError = "Can not install MelonLoader";
				return;
			}

			Thread.Sleep(2000);

			SetupPrimitierExeError = "";
		}

		private static void InstallMelonLoader(string primitierExePath, bool showPopup, Dispatcher dispatcher)
		{
			MelonInstallUninstallError = "";

			MelonInstaller.Install(Path.GetDirectoryName(primitierExePath), MelonLoaderVersions.V0_5_3, false, false);
			if (!showPopup)
			{
				MelonInstallUninstallError = MelonInstaller.Error;
				return;
			}

			dispatcher.Invoke(() => 
			{
				if (MelonInstaller.Error != "")
				{
					var result = MessageBox.Show($"Fatal error installing MelonLoader:\n{MelonInstaller.Error}\n Retry?", "Fatal error", MessageBoxButton.YesNo);

					if (result == MessageBoxResult.Yes)
					{
						InstallMelonLoader(primitierExePath, showPopup, dispatcher);
					}
					else
					{
						MelonInstallUninstallError = MelonInstaller.Error;
						return;
					}

				}

			});

			

		}

		private static void UninstallMelonLoader(string primitierExePath, bool showPopup, Dispatcher dispatcher)
		{
			MelonInstallUninstallError = "";

			MelonInstaller.Uninstall(Path.GetDirectoryName(primitierExePath));
			
			if (!showPopup)
			{
				MelonInstallUninstallError = MelonInstaller.Error;
				return;
			}

			dispatcher.Invoke(() =>
			{
				if (MelonInstaller.Error != "")
				{
					var result = MessageBox.Show($"Fatal error uninstalling MelonLoader:\n{MelonInstaller.Error}\n Retry?", "Fatal error", MessageBoxButton.YesNo);

					if (result == MessageBoxResult.Yes)
					{
						InstallMelonLoader(primitierExePath, showPopup, dispatcher);
					}
					else
					{
						MelonInstallUninstallError = MelonInstaller.Error;
						return;
					}
				}

			});

		

			

		}

		public static void Uninstall(Dispatcher dispatcher, bool uninstallMods=true)
		{
			UninstallError = "";

			if (ConfigFile.Config == null)
			{
				ConfigFile.Load();
			}
			if (ConfigFile.Config == null)
			{
				UninstallError = "No config file found";
				return;
			}

			UninstallMelonLoader(Path.Combine(ConfigFile.Config.PrimitierInstallPath, "Primitier.exe"), false, dispatcher);
			if (MelonInstallUninstallError != "")
			{
				UninstallError = "Can not uninstall MelonLoader";
				return;
			}

			if (uninstallMods)
			{
				try
				{
					Directory.Delete(ConfigFile.PMFDirPath, true);
				}
				catch (Exception e)
				{
					UninstallError = "Can delete mods folder";
					return;
				}
			}


		}

		public static void CheckForUpdates(Dispatcher dispatcher)
		{

			Task.Factory.StartNew(async () =>
			{
				var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version;
				var latestRelease = await Updater.GetLatestRelease();
				if (latestRelease == null)
				{
					return;
				}

				

				var latestReleaseVersion = new Version(latestRelease.TagName.Substring(1));

				if (currentVersion < latestReleaseVersion)
				{
					var downloadLink = "";
					foreach (var asset in latestRelease.Assets)
					{
						if (asset.Name == "PrimitierModManager.msi")
						{
							downloadLink = asset.BrowserDownloadUrl;
							break;
						}

					}

					dispatcher.Invoke(() =>
					{
						
						PopupManager.ShowUpdatePopup(new Uri(downloadLink));

					});

				}
			});

		}

		


	}
}
