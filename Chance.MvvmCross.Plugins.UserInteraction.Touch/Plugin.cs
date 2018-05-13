using System;
using MvvmCross;
using MvvmCross.Plugin;

namespace Chance.MvvmCross.Plugins.UserInteraction.Touch
{
	public class Plugin : IMvxPlugin
	{
		public void Load() 
		{
			Mvx.RegisterType<IUserInteraction, UserInteraction>();
		}
	}
}

