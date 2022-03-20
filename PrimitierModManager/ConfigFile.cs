using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimitierModManager
{
	[Serializable]
	public class ConfigFile
	{
		[JsonIgnore] public static ConfigFile? Config;
		[JsonIgnore] public static string PMFDirPath  = null;
		[JsonIgnore] public static string PMFModsDirPath = null;
		[JsonIgnore] public static string ConfigFilePath = null;

		[JsonProperty(Required = Required.Always)]
		public string PrimitierInstallPath;

		[JsonProperty(Required = Required.DisallowNull)]
		public string[] ActiveMods = new string[0];

		public static void RebuildDirectorySturcture(IErrorCollector collector)
		{
			try
			{
				PMFDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal, Environment.SpecialFolderOption.Create), "PrimitierModdingFramework");
				Directory.CreateDirectory(PMFDirPath);

				PMFModsDirPath = Path.Combine(PMFDirPath, "Mods");
				Directory.CreateDirectory(PMFModsDirPath);

				ConfigFilePath = Path.Combine(PMFDirPath, "PMFInstallerConfig.json");
			}
			catch (Exception e)
			{
				collector.LogError("Can not rebuild directory structure", e);
			}
		


		}

		public static bool Load()
		{
			return Load(ErrorCollector.Discard);
		}


		public static bool Load(IErrorCollector collector)
		{
			if (Config != null)
			{
				return true;
			}

			RebuildDirectorySturcture(collector);

			try
			{
				Config = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(ConfigFilePath));
				if (Config != null)
				{
					return true;
				}
				
			}catch(Exception e)
			{
			}
			
			Config = null;
			collector.LogError("Can not load config file");
			
			
			if (App.MainWindow != null)
			{
				App.MainWindow.SwitchMenu(1);
			}

			return false;
		}

		public static void Save()
		{
			Save(ErrorCollector.Discard);
		}


		public static void Save(IErrorCollector collector)
		{
			if (Config == null)
			{
				return;
			}
			try
			{
				File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(Config));
			}
			catch (Exception e)
			{
				collector.LogError("Can not save config file");
			}
			

		}


	}
}
