﻿using System;
using System.Reflection;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace MetalTracker.Trackers.Z1M1.Dialogs
{
	public class AboutDlg : Dialog
	{
		public AboutDlg()
		{
			XamlReader.Load(this);
		}

		protected void HandleLoad(object sender, EventArgs e)
		{
			var platform = Eto.Platform.Instance;
			var assyVersion = Assembly.GetExecutingAssembly().GetName().Version;
			string versionString = $"{assyVersion.Major}.{assyVersion.Minor}.{assyVersion.Build}";
			this.FindChild<Label>("labelVersion").Text = $"Version {versionString} ({platform.ID})";
		}
	}
}
