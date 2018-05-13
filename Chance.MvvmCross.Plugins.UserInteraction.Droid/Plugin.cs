using System;
using MvvmCross;
using MvvmCross.Plugin;

namespace Chance.MvvmCross.Plugins.UserInteraction.Droid
{
	public class Plugin : IMvxPlugin
	{
		public void Load() 
		{
			Mvx.RegisterType<IUserInteraction, UserInteraction>();
		}
	}
}

