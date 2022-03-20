using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PrimitierModManager
{
	public static class PrimitierLauncher
	{
		public static void LaunchWithSelectedMods()
		{
			var collector = new ErrorCollector();

			if (ConfigFile.Config == null && !ConfigFile.Load(collector))
			{
				return;
			}

			CleanMelonModsDirectory(collector);
		

			foreach (var mod in ModManager.ActiveMods)
			{
				ExtractModFiles(mod, collector);
			}

			CopyProxyDlls(collector);

			if (collector.HasErrors)
			{
				PopupManager.ShowErrorPopupWriteToFile(collector);
			}

			

			Process.Start(Path.Combine(ConfigFile.Config.PrimitierInstallPath, "Primitier.exe"));

			
		}

		private static void CleanMelonModsDirectory(IErrorCollector collector)
		{
			
			if (ConfigFile.Config == null && !ConfigFile.Load(collector))
			{
				return;
			}

			string melonModsDir = Path.Combine(ConfigFile.Config.PrimitierInstallPath, "Mods");

			foreach (var fileSystemEntry in Directory.GetFileSystemEntries(melonModsDir))
			{
				try
				{
					if (File.Exists(fileSystemEntry))
					{
						File.Delete(fileSystemEntry);
					}
					else
					{
						Directory.Delete(fileSystemEntry, true);
					}
				}catch(Exception e)
				{
					collector.LogError($"Can not delete '{Path.GetFileName(fileSystemEntry)}'");
				}

			}


		}


		private static void CopyProxyDlls(IErrorCollector collector)
		{
			if (ConfigFile.Config == null && !ConfigFile.Load(collector))
			{
				return;
			}

			var proxyDllsPath = Path.Combine(ConfigFile.Config.PrimitierInstallPath, "MelonLoader", "Managed");
			var melonModsDir = Path.Combine(ConfigFile.Config.PrimitierInstallPath, "Mods");

			foreach (var proxyDll in Directory.GetFiles(proxyDllsPath))
			{
				try
				{
					File.Copy(proxyDll, Path.Combine(melonModsDir, Path.GetFileName(proxyDll)), true);
				}catch(Exception e)
				{
					collector.LogError($"Could not copy '{Path.GetFileName(proxyDll)}' to '{melonModsDir}'");
					continue;
				}

			}

			try
			{
				File.Copy(Path.Combine(ConfigFile.Config.PrimitierInstallPath, "MelonLoader", "MelonLoader.dll"), Path.Combine(melonModsDir, "MelonLoader.dll"), true);
			}catch(Exception e)
			{
				collector.LogError($"Could not copy MelonLoader.dll to '{melonModsDir}'");
				return;
			}


		}


		private static void ExtractModFiles(Mod mod, IErrorCollector collector)
		{
			if (ConfigFile.Config == null && !ConfigFile.Load(collector))
			{
				return;
			}

			try
			{
				var zip = ZipFile.OpenRead(mod.FileName);

				string melonModsDir = Path.Combine(ConfigFile.Config.PrimitierInstallPath, "Mods");

				zip.ExtractToDirectory(melonModsDir, true);


				zip.Dispose();
			}catch(Exception e)
			{
				collector.LogError($"Can not extract mod '{mod.FileName}'");
			}
		
		}


	}
}
