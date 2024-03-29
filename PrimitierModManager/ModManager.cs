﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PrimitierModManager
{
	public static class ModManager
	{

		public static List<Mod> LoadedMods = new List<Mod>();
		public static List<Mod> ActiveMods = new List<Mod>();

		public static event Action OnModListsUpdate;

		public static BitmapImage DefaultPMFIcon;
		public static BitmapImage DefaultMelonIcon;

		public static void EnableMod(Mod mod)
		{
			if (ActiveMods.Contains(mod))
			{
				return;
			}
			if (!LoadedMods.Contains(mod))
			{
				return;
			}

			LoadedMods.Remove(mod);
			
			ActiveMods.Add(mod);
			AddToActiveModsConfig(mod.Name);



		   OnModListsUpdate?.Invoke();

		}



		public static void DisableMod(Mod mod)
		{
			if (!ActiveMods.Contains(mod))
			{
				return;
			}

			ActiveMods.Remove(mod);
			RemoveFromActiveModsConfig(mod.Name);

			if (!LoadedMods.Contains(mod))
			{
				LoadedMods.Add(mod);
			}


			OnModListsUpdate?.Invoke();
		}

		private static void AddToActiveModsConfig(string name)
		{
			var sourceArray = ConfigFile.Config.ActiveMods;
			string[] newActiveModHashArray = new string[sourceArray.Length + 1];
			Array.Copy(sourceArray, newActiveModHashArray, sourceArray.Length);
			newActiveModHashArray[sourceArray.Length] = name;
			ConfigFile.Config.ActiveMods = newActiveModHashArray;

		}
		private static void RemoveFromActiveModsConfig(string name)
		{

			string[] newActiveModHashArray = new string[ConfigFile.Config.ActiveMods.Length - 1];
			int ii = 0;
			for (int i = 0; i < ConfigFile.Config.ActiveMods.Length; i++)
			{
				var file = ConfigFile.Config.ActiveMods[i];
				if (file == name)
				{
					continue;
				}
				newActiveModHashArray[ii] = file;

				ii++;
			}
			ConfigFile.Config.ActiveMods = newActiveModHashArray;
		}


		public static void ReloadMods()
		{
			var collector = new ErrorCollector();

			LoadedMods.Clear();
			ActiveMods.Clear();

			if (ConfigFile.Config == null && !ConfigFile.Load(collector))
			{
				LogManager.FlushCollector(collector);
				return;
			}

			
			var modDirFiles = Directory.GetFiles(ConfigFile.PMFModsDirPath);
			List<string> ActiveModsNames = new List<string>(modDirFiles.Length);
			foreach (var modPath in modDirFiles)
			{
				var mod = LoadModFromFile(modPath, collector);
				if (mod != null)
				{

					if (LoadedMods.Contains(mod) || ActiveMods.Contains(mod))
					{
						collector.LogError($"'{mod.DisplayName}' is loaded double");
						continue;
					}
					


					if (ConfigFile.Config.ActiveMods.Contains(mod.Name))
					{
						ActiveMods.Add(mod);
						ActiveModsNames.Add(mod.Name);
						
					}
					else
					{
						LoadedMods.Add(mod);
					}

				}

			}
			ConfigFile.Config.ActiveMods = ActiveModsNames.ToArray();
			ConfigFile.Save();

		
			OnModListsUpdate?.Invoke();

			LogManager.FlushCollector(collector);
		}

		public static ZipArchiveEntry GenerateModJsonFile(ZipArchive zip, string zipFilePath)
		{
			
			var modJsonEntry = zip.CreateEntry("Mod.json");
			var modJsonStream = modJsonEntry.Open();

			var mod = new Mod();
			mod.Name = Path.GetFileNameWithoutExtension(zipFilePath);
			mod.DisplayName = mod.Name;

			mod.Description = "Generated from a .zip mod";
			
			
			mod.FileName = zipFilePath;
			mod.IsGenerated = true;
			mod.InitUI();


			var stringMod = JsonConvert.SerializeObject(mod);

			modJsonStream.Write(Encoding.ASCII.GetBytes(stringMod));

			modJsonStream.Close();

			return modJsonEntry;
		}

		public static void GenerateModJsonFile(string filePath)
		{
			var zipStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite);

			var zip = new ZipArchive(zipStream, ZipArchiveMode.Update);
			GenerateModJsonFile(zip, filePath);

			zip.Dispose();
			zipStream.Close();
		}



		public static void DeleteMod(Mod mod)
		{
			if (!File.Exists(mod.FileName))
			{
				return;
			}

			File.Delete(mod.FileName);


			ReloadMods();
		}
		public static void AddMod(string file, IErrorCollector collector)
		{
			if (!ValidateModFile(file, collector, tryFix:true))
			{
				return;
			}

			try
			{
				File.Copy(file, Path.Combine(ConfigFile.PMFModsDirPath, Path.GetFileName(file)), true);
			}catch (Exception e)
			{
				collector.LogError("Can not copy mod files", e);

			}

			ReloadMods();
		}


		public static Mod? LoadModFromFile(string file, IErrorCollector collector)
		{
			var zipStream = File.Open(file, FileMode.Open, FileAccess.ReadWrite);
			var zip = new ZipArchive(zipStream, ZipArchiveMode.Update);

			bool isModpack = false;
			var modjsonEntry = zip.GetEntry("Mod.json");
			if (modjsonEntry == null)
			{
				modjsonEntry = zip.GetEntry("Modpack.json");
				isModpack = true;
			}
			if (modjsonEntry == null)
			{
				modjsonEntry = GenerateModJsonFile(zip, file);
			}

			Mod mod = null;
			var bytes = ZipHelper.ReadEntryZipBytes(modjsonEntry);
			try
			{
				mod = JsonConvert.DeserializeObject<Mod>(Encoding.ASCII.GetString(bytes));
			}
			catch (Exception e)
			{
				collector.LogError($"Can not load mod '{file}' invalid Mod.json", e);
				return null;
			}

			
			var iconEntry = zip.GetEntry("Icon.png");
			if (iconEntry != null)
			{
				var stream = iconEntry.Open();
				mod.Image = LoadFromStream(stream);

				stream.Close();
			}
			else
			{
				if (ZipHelper.IsPMFMod(zip) || isModpack)
				{
					if (DefaultPMFIcon == null)
					{
						DefaultPMFIcon = new BitmapImage(new Uri("/Assets/Images/PMFIcon.png", UriKind.Relative));
					}

					mod.Image = DefaultPMFIcon;
				}
				else
				{
					if (DefaultMelonIcon == null)
					{
						DefaultMelonIcon = new BitmapImage(new Uri("/Assets/Images/MelonLoaderIcon.png", UriKind.Relative));
					}

					mod.Image = DefaultMelonIcon;
				}

				
				
				
			}
			
			zip.Dispose();
			zipStream.Close();

			if (isModpack)
			{
				mod.DisplayName += " (Modpack)";
			}

			mod.IsModpack = isModpack;
			mod.Name = Path.GetFileNameWithoutExtension(file);
			mod.FileName = file;
			mod.InitUI();
			return mod;
		}



		private static BitmapImage LoadFromStream(Stream stream)
		{
			var image = new BitmapImage();

			image.BeginInit();
			image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.UriSource = null;
			image.StreamSource = stream;
			image.EndInit();

			image.Freeze();
			return image;
		}



		private static bool ValidateModFile(string file, IErrorCollector collector, bool tryFix=false)
		{
			if (!File.Exists(file))
			{
				collector.LogError($"Can not find mod '{file}'");

				return false;
			}

			if (Path.GetExtension(file) != ".pmfm" && Path.GetExtension(file) != ".zip")
			{
				collector.LogError($"Mod '{file}' is not the right file type");

				return false;
			}

			ZipArchive zip=null;
			FileStream zipStream=null;
			try
			{
				zipStream = File.Open(file, FileMode.Open, FileAccess.ReadWrite);
				zip = new ZipArchive(zipStream, ZipArchiveMode.Update, true);
			}catch(Exception e)
			{
				collector.LogError($"Can't read '{file}'", e);
				return false;
			}
			
			if (zip.GetEntry("Mod.json") == null)
			{
				if (tryFix)
				{
					GenerateModJsonFile(zip, file);
				}
				else
				{
					collector.LogError($"Mod '{file}' doesn't contain a Mod.json file");
					return false;
				}

				
			}



			zipStream?.Close();
			return true;
		}


		
	}
}
