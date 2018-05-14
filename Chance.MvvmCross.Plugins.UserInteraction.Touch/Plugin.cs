using System;
using MvvmCross;
using MvvmCross.Plugin;

namespace Vapolia.MvvmCross.UserInteraction.Touch
{
	public class Plugin : IMvxPlugin
	{
		public void Load() 
		{
			Mvx.RegisterType<IUserInteraction, UserInteraction>();
		}
	}
}

