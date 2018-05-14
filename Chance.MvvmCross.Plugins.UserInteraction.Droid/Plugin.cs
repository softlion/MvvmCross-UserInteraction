using System;
using MvvmCross;
using MvvmCross.Plugin;

namespace Vapolia.MvvmCross.UserInteraction.Droid
{
	public class Plugin : IMvxPlugin
	{
		public void Load() 
		{
			Mvx.RegisterType<IUserInteraction, UserInteraction>();
		}
	}
}

