using BetterConsole;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimitierModManager.Commands
{
	public class UninstallCommand : Command
	{
		public override string Name => "Uninstall";

		public override string Discription => "Uninstalls PMF installer";

		public override ArgumentDescriptor[] ArgDescriptors => new ArgumentDescriptor[] { new ArgumentDescriptor("RemoveMods", "If true remove the installed mods", "true") };

		protected override void OnExecute(Argument[] args)
		{
			ConfigFile.Load();

			bool removeMods = true;
			if(!bool.TryParse(args[0].Value, out removeMods))
			{
				removeMods = true;
			}
			Setup.Uninstall(removeMods);


			ConfigFile.Config = null;

			App.Current.Shutdown();

		}
	}
}
