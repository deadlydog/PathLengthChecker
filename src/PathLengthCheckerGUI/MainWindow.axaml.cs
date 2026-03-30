using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PathLengthChecker;
using PathLengthCheckerGUI.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace PathLengthCheckerGUI
{
	public partial class MainWindow : Window
	{
		public MainWindowViewModel ViewModel { get; }

		public MainWindow()
		{
			ViewModel = new MainWindowViewModel();
			DataContext = ViewModel;

			InitializeComponent();

			// Restore window size and position from saved settings.
			var settings = ViewModel.Settings;
			Width = settings.WindowWidth;
			Height = settings.WindowHeight;
			if (settings.WindowLeftPosition != 0 || settings.WindowTopPosition != 0)
				Position = new PixelPoint((int)settings.WindowLeftPosition, (int)settings.WindowTopPosition);
			WindowState = settings.WindowState;

			Title = $"Path Length Checker v{Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3)} - Written by Daniel Schroeder";
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			ViewModel.SaveToSettings(Width, Height, Position.X, Position.Y, WindowState);
		}

		// ── Folder browse buttons ─────────────────────────────────────────

		private async void BtnBrowseForRootDirectory_Click(object? sender, RoutedEventArgs e)
		{
			var folder = await PickFolderAsync("Select the directory whose path lengths you want to check...");
			if (folder != null)
				ViewModel.RootDirectory = folder;
		}

		private async void BtnBrowseForReplaceRootDirectory_Click(object? sender, RoutedEventArgs e)
		{
			var folder = await PickFolderAsync("Select the directory to replace the Starting Directory with in the returned paths...");
			if (folder != null)
				ViewModel.RootDirectoryReplacement = folder;
		}

		private async Task<string?> PickFolderAsync(string title)
		{
			var topLevel = TopLevel.GetTopLevel(this);
			if (topLevel == null) return null;

			var results = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
			{
				Title = title,
				AllowMultiple = false
			});

			return results.Count > 0 ? results[0].Path.LocalPath : null;
		}

		// ── Search ────────────────────────────────────────────────────────

		private async void BtnGetPathLengths_Click(object? sender, RoutedEventArgs e)
		{
			await SearchForPaths();
		}

		public async Task SearchForPaths()
		{
			ViewModel.BeginSearch();

			try
			{
				var newPaths = await ViewModel.GetPathsAsync(
					ViewModel.RootDirectory.Trim(),
					ViewModel.RootDirectoryReplacement.Trim(),
					ViewModel.SearchPattern,
					ViewModel.GetSearchCancellationToken());

				if (newPaths == null)
				{
					await MessageBoxHelper.ShowAsync(this, "Invalid Starting Directory",
						$"The Starting Directory \"{ViewModel.RootDirectory.Trim()}\" does not exist. Please specify a valid directory.");
					ViewModel.IsSearching = false;
					ViewModel.NumberOfPathsText = string.Empty;
					return;
				}

				ViewModel.FinishSearch(newPaths);
			}
			catch (OperationCanceledException)
			{
				ViewModel.FinishSearch(new System.Collections.ObjectModel.ObservableCollection<PathInfo>());
			}
			catch (Exception ex)
			{
				await MessageBoxHelper.ShowAsync(this, "Error Occurred",
					$"An error occurred while retrieving paths:{Environment.NewLine}{Environment.NewLine}{ex.Message}");
				Debug.WriteLine(ex.ToString());
				ViewModel.IsSearching = false;
			}
		}

		private void BtnCancelGetPathLengths_Click(object? sender, RoutedEventArgs e)
		{
			ViewModel.CancelSearch();
		}

		// ── Reset ─────────────────────────────────────────────────────────

		private void BtnResetAllOptions_Click(object? sender, RoutedEventArgs e)
		{
			ViewModel.ResetToDefaults();
		}

		// ── Clipboard ─────────────────────────────────────────────────────

		private async void SplitBtnCopyToClipboard_Click(object? sender, RoutedEventArgs e)
		{
			await SetClipboardTextAsync(ViewModel.GetPathsAsString(includeLength: true));
		}

		private async void BtnCopyToClipboardWithoutLengths_Click(object? sender, RoutedEventArgs e)
		{
			await SetClipboardTextAsync(ViewModel.GetPathsAsString(includeLength: false));
		}

		private async void BtnCopyToClipboardAsCsv_Click(object? sender, RoutedEventArgs e)
		{
			await SetClipboardTextAsync(ViewModel.GetPathsAsCsvString(includeLength: true));
		}

		private async void BtnCopyToClipboardWithoutLengthsAsCsv_Click(object? sender, RoutedEventArgs e)
		{
			await SetClipboardTextAsync(ViewModel.GetPathsAsCsvString(includeLength: false));
		}

		private async Task SetClipboardTextAsync(string text)
		{
			try
			{
				var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
				if (clipboard != null)
					await clipboard.SetTextAsync(text);
			}
			catch (Exception ex)
			{
				await MessageBoxHelper.ShowAsync(this, "Error Occurred Copying To Clipboard",
					$"An error occurred while copying text to the clipboard:{Environment.NewLine}{Environment.NewLine}{ex.Message}");
			}
		}

		// ── Context menu ──────────────────────────────────────────────────

		private async void MenuItem_OpenDirectoryInFileExplorer_Click(object? sender, RoutedEventArgs e)
		{
			var selectedPath = ViewModel.SelectedPath.Path;
			if (string.IsNullOrEmpty(selectedPath))
				return;

			string? directoryPath = null;
			if (Directory.Exists(selectedPath))
				directoryPath = selectedPath;
			else if (File.Exists(selectedPath))
				directoryPath = Directory.GetParent(selectedPath)?.FullName;

			if (string.IsNullOrWhiteSpace(directoryPath))
			{
				await MessageBoxHelper.ShowAsync(this, "Cannot Open Directory",
					$"The following directory either does not exist, you don't have permissions to access it, or its path is too long:{Environment.NewLine}{Environment.NewLine}{selectedPath}");
				return;
			}

			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = directoryPath,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				await MessageBoxHelper.ShowAsync(this, "Cannot Open Directory",
					$"Failed to open directory:{Environment.NewLine}{Environment.NewLine}{ex.Message}");
			}
		}

		// ── SetUIControlsFromSearchOptions ────────────────────────────────

		/// <summary>
		/// Sets the ViewModel state from the provided search options (used for CLI arg handling).
		/// </summary>
		public void SetUIControlsFromSearchOptions(PathLengthSearchOptions searchOptions)
		{
			ViewModel.RootDirectory = searchOptions.RootDirectory ?? string.Empty;
			ViewModel.SearchPattern = searchOptions.SearchPattern ?? string.Empty;
			ViewModel.IncludeSubdirectories = searchOptions.SearchOption == System.IO.SearchOption.AllDirectories;
			ViewModel.SelectedFileSystemType = searchOptions.TypesToGet;
			ViewModel.MinPathLength = searchOptions.MinimumPathLength;
			ViewModel.MaxPathLength = searchOptions.MaximumPathLength;

			if (!string.IsNullOrEmpty(searchOptions.RootDirectoryReplacement))
			{
				ViewModel.RootDirectoryReplacement = searchOptions.RootDirectoryReplacement;
				ViewModel.ReplaceRootDirectory = true;
			}

			ViewModel.UrlEncodePaths = searchOptions.UrlEncodePaths;
		}
	}
}
