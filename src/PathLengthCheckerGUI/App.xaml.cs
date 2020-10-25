using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PathLengthChecker;

namespace PathLengthCheckerGUI
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App() : base()
		{
			SetupUnhandledExceptionHandling();
			Startup += App_Startup;
			Exit += App_Exit;
		}

		private void App_Exit(object sender, ExitEventArgs e)
		{
			// Save any application settings that were changed when exiting (such as window size and position).
			PathLengthCheckerGUI.Properties.Settings.Default.Save();
		}

		private void App_Startup(object sender, StartupEventArgs e)
		{
			var mainWindow = new MainWindow();

			// If a directory was drag-and-dropped onto the GUI executable, launch with the app searching the given directory.
			if (e.Args.Length == 1 && Directory.Exists(e.Args[0]))
			{
				mainWindow.txtRootDirectory.Text = e.Args[0];
				mainWindow.btnGetPathLengths.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
			}
			else if (e.Args.Length >= 1)
			{
				try
				{
					var searchOptions = PathLengthChecker.ArgumentParser.ParseArgs(e.Args);
					mainWindow.SetUIControlsFromSearchOptions(searchOptions);

					// Only start the search if a root dir was specified
					if (!String.IsNullOrEmpty(searchOptions?.RootDirectory))
					{
						mainWindow.btnGetPathLengths.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
					}
				}
				catch (ArgumentException ex)
				{
					string title = "Incorrect arguments";
					string message = "Incorrectly-formatted arguments were passed to the program.\n\n";
					message += ex.Message + "\n\n" + PathLengthChecker.ArgumentParser.ArgumentUsage;
					MessageBox.Show(message, title);
				}
			}

			mainWindow.Show();
		}

		private void SetupUnhandledExceptionHandling()
		{
			// Catch exceptions from all threads in the AppDomain.
			AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
				ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false);

			// Catch exceptions from each AppDomain that uses a task scheduler for async operations.
			TaskScheduler.UnobservedTaskException += (sender, args) =>
				ShowUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException", false);

			// Catch exceptions from a single specific UI dispatcher thread.
			Dispatcher.UnhandledException += (sender, args) =>
			{
				// If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
				if (!Debugger.IsAttached)
				{
					args.Handled = true;
					ShowUnhandledException(args.Exception, "Dispatcher.UnhandledException", true);
				}
			};

			// Catch exceptions from the main UI dispatcher thread.
			// Typically we only need to catch this OR the Dispatcher.UnhandledException.
			// Handling both can result in the exception getting handled twice.
			//Application.Current.DispatcherUnhandledException += (sender, args) =>
			//{
			//	// If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
			//	if (!Debugger.IsAttached)
			//	{
			//		args.Handled = true;
			//		ShowUnhandledException(args.Exception, "Application.Current.DispatcherUnhandledException", true);
			//	}
			//};
		}

		void ShowUnhandledException(Exception e, string unhandledExceptionType, bool promptUserForShutdown)
		{
			var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
			var messageBoxMessage = $"The following exception occurred:\n\n{e}";
			var messageBoxButtons = MessageBoxButton.OK;

			if (promptUserForShutdown)
			{
				messageBoxMessage += "\n\nNormally the app would die now. Should we let it die?";
				messageBoxButtons = MessageBoxButton.YesNo;
			}

			// Let the user decide if the app should die or not (if applicable).
			if (MessageBox.Show(messageBoxMessage, messageBoxTitle, messageBoxButtons) == MessageBoxResult.Yes)
			{
				Application.Current.Shutdown();
			}
		}
	}
}
