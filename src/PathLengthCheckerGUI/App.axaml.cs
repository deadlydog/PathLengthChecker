using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PathLengthChecker;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PathLengthCheckerGUI
{
	public partial class App : Application
	{
		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			SetupUnhandledExceptionHandling();

			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				var mainWindow = new MainWindow();
				desktop.MainWindow = mainWindow;

				// Handle command-line arguments after the window opens so dialogs can be shown.
				var args = desktop.Args ?? Array.Empty<string>();
				mainWindow.Opened += (s, e) => HandleArgs(mainWindow, args);
			}

			base.OnFrameworkInitializationCompleted();
		}

		private static async void HandleArgs(MainWindow mainWindow, string[] args)
		{
			if (args.Length == 1 && Directory.Exists(args[0]))
			{
				mainWindow.ViewModel.RootDirectory = args[0];
				await mainWindow.SearchForPaths();
			}
			else if (args.Length >= 1)
			{
				try
				{
					var searchOptions = ArgumentParser.ParseArgs(args);
					mainWindow.SetUIControlsFromSearchOptions(searchOptions);

					if (!string.IsNullOrEmpty(searchOptions?.RootDirectory))
						await mainWindow.SearchForPaths();
				}
				catch (ArgumentException ex)
				{
					await MessageBoxHelper.ShowAsync(mainWindow, "Incorrect arguments",
						$"Incorrectly-formatted arguments were passed to the program.\n\n{ex.Message}\n\n{ArgumentParser.ArgumentUsage}");
				}
			}
		}

		private void SetupUnhandledExceptionHandling()
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
				ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException");

			TaskScheduler.UnobservedTaskException += (sender, args) =>
				ShowUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException");
		}

		private static async void ShowUnhandledException(Exception? e, string unhandledExceptionType)
		{
			var title = $"Unexpected Error: {unhandledExceptionType}";
			var message = $"The following exception occurred:\n\n{e}";

			if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: Avalonia.Controls.Window mainWindow })
				await MessageBoxHelper.ShowAsync(mainWindow, title, message);
		}
	}
}
