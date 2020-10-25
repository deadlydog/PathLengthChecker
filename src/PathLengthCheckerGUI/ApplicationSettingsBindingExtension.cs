using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PathLengthCheckerGUI
{
	// Binding extension that allows XAML to easily access application settings stored in the Properties\Settings.settings file.
	public class ApplicationSettingsBindingExtension : Binding
	{
		public ApplicationSettingsBindingExtension()
		{
			Initialize();
		}

		public ApplicationSettingsBindingExtension(string path) : base(path)
		{
			Initialize();
		}

		private void Initialize()
		{
			this.Source = PathLengthCheckerGUI.Properties.Settings.Default;
			this.Mode = BindingMode.TwoWay;
		}
	}
}
