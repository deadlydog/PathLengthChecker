using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Threading.Tasks;

namespace PathLengthCheckerGUI
{
	/// <summary>
	/// Simple cross-platform message box built entirely on Avalonia primitives.
	/// </summary>
	public static class MessageBoxHelper
	{
		public static async Task ShowAsync(Window owner, string title, string message)
		{
			var dialog = new Window
			{
				Title = title,
				MinWidth = 400,
				MaxWidth = 600,
				SizeToContent = SizeToContent.Height,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
				CanResize = false,
				ShowInTaskbar = false,
			};

			var okButton = new Button
			{
				Content = "OK",
				HorizontalAlignment = HorizontalAlignment.Right,
				MinWidth = 80,
				IsDefault = true,
			};
			okButton.Click += (_, _) => dialog.Close();

			dialog.Content = new StackPanel
			{
				Margin = new Thickness(20),
				Spacing = 16,
				Children =
				{
					new TextBlock
					{
						Text = message,
						TextWrapping = TextWrapping.Wrap,
					},
					okButton,
				}
			};

			await dialog.ShowDialog(owner);
		}
	}
}
