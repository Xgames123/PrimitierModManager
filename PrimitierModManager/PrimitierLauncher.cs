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
				if (mod.IsModpack)
				{
					ExtractModpackFiles(mod, collector);
				}
				else
				{
					ExtractModFiles(mod, collector);
				}
				
			}

			CopyProxyDlls(collector);

			if (collector.HasErrors)
			{
				LogManager.FlushCollector(collector);
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

			if (!Directory.Exists(melonModsDir))
			{
				collector.LogError("MelonMods directory not found. This could be because Melonloader is not installed properly or the primitier path is not setup properly");
				return;
			}

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

			if (!Directory.Exists(proxyDllsPath))
			{
				collector.LogError("Proxy dll path not found. This could be because Melonloader is not installed properly or the primitier path is not setup properly");
				return;
			}

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


		private static void ExtractModpackFiles(Mod modpack, IErrorCollector collector)
		{
			if (ConfigFile.Config == null && !ConfigFile.Load(collector))
			{
				return;
			}

			try
			{
				var zip = ZipFile.OpenRead(modpack.FileName);


				foreach (var entry in zip.Entries)
				{
					if (entry.Name.EndsWith(".pmfm") || entry.Name.EndsWith(".zip"))
					{
						ExtractModFiles(entry, collector);
					}

				} 


				zip.Dispose();
			}
			catch (Exception e)
			{
				collector.LogError($"Can not extract modpack '{modpack.FileName}'");
			}

		}

		private static void ExtractModFiles(ZipArchiveEntry entry, IErrorCollector collector)
		{
			if (entry == null)
			{
				return;
			}

			var zipStream = entry.Open();
			var zip = new ZipArchive(zipStream, ZipArchiveMode.Read, true);

			ExtractModFiles(zip, entry.FullName, collector);

			zip.Dispose();
			zipStream.Close();

		}


		private static void ExtractModFiles(ZipArchive modzip, string modFileName, IErrorCollector collector)
		{

			try
			{
				string melonModsDir = Path.Combine(ConfigFile.Config.PrimitierInstallPath, "Mods");

				if (modzip.GetEntry("Mod.json") == null)
				{
					return;
				}

				modzip.ExtractToDirectory(melonModsDir, true);
			}
			catch (Exception e)
			{
				collector.LogError($"Can not extract mod '{modFileName}'");
			}

			

		}


		private static void ExtractModFiles(Mod mod, IErrorCollector collector)
		{
			if (mod.IsModpack)
			{
				ExtractModpackFiles(mod, collector);
				return;
			}

			if (ConfigFile.Config == null && !ConfigFile.Load(collector))
			{
				return;
			}
			var zip = ZipFile.OpenRead(mod.FileName);

			ExtractModFiles(zip, mod.FileName, collector);

			zip.Dispose();

		}


	}
}
