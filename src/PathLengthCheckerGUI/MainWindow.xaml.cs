using PathLengthChecker;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PathLengthCheckerGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		#region Notify Property Changed
		/// <summary>
		/// Inherited event from INotifyPropertyChanged.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Fires the PropertyChanged event of INotifyPropertyChanged with the given property name.
		/// </summary>
		/// <param name="propertyName">The name of the property to fire the event against</param>
		public void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		private DateTime _timePathSearchingStarted = DateTime.MinValue;
		private CancellationTokenSource _searchCancellationTokenSource = new CancellationTokenSource();

		public MainWindow()
		{
			InitializeComponent();
			this.DataContext = this;

			SetWindowTitle();
		}
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			LoadColumnSortDescriptionsFromSettings();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			SaveColumnSortDescriptionsToSettings();
		}

		private void SetWindowTitle()
		{
			this.Title = "Path Length Checker v" + Assembly.GetEntryAssembly().GetName().Version.ToString(3) + " - Written by Daniel Schroeder";
		}

		private void LoadColumnSortDescriptionsFromSettings()
		{
			var previousGridColumnSortDescriptions = Properties.Settings.Default.ResultsGridColumnSortDescriptionCollection ?? SortDescriptionCollection.Empty;
			SetGridColumnSortDescriptions(previousGridColumnSortDescriptions);
		}

		private void SaveColumnSortDescriptionsToSettings()
		{
			var gridColumnSortDescriptions = GetCurrentGridColumnSortDescriptions();
			Properties.Settings.Default.ResultsGridColumnSortDescriptionCollection = gridColumnSortDescriptions;
		}

		public ObservableCollection<PathInfo> Paths
		{
			get => _paths;
			set
			{
				_paths = value;
				NotifyPropertyChanged(nameof(Paths));
			}
		}
		private ObservableCollection<PathInfo> _paths = new ObservableCollection<PathInfo>();

		/// <summary>
		/// This returns the same items as the Paths property, but sorted however the UI happens to be sorted.
		/// </summary>
		private IEnumerable<PathInfo> PathsFromUiDataGrid => dgPaths.Items.Cast<PathInfo>();

		public PathInfo SelectedPath
		{
			get { return (PathInfo)GetValue(SelectedPathProperty); }
			set { SetValue(SelectedPathProperty, value); }
		}
		public static readonly DependencyProperty SelectedPathProperty = DependencyProperty.Register("SelectedPath", typeof(PathInfo), typeof(MainWindow), new UIPropertyMetadata(new PathInfo()));

		/// <summary>
		/// Handles the Click event of the btnBrowseForRootDirectory control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnBrowseForRootDirectory_Click(object sender, RoutedEventArgs e)
		{
			// Setup the prompt
			var folderDialog = new System.Windows.Forms.FolderBrowserDialog
			{
				Description = "Select the directory that contains the paths whose length you want to check...",
				ShowNewFolderButton = false
			};

			// If the user selected a folder, put it in the Root Directory text box.
			if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				txtRootDirectory.Text = folderDialog.SelectedPath;
		}

		/// <summary>
		/// Handles the Click event of the btnBrowseForReplaceRootDirectory control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnBrowseForReplaceRootDirectory_Click(object sender, RoutedEventArgs e)
		{
			// Setup the prompt
			var folderDialog = new System.Windows.Forms.FolderBrowserDialog
			{
				Description = "Select the directory that you want to use to replace the Starting Directory in the returned paths...",
				ShowNewFolderButton = false
			};

			// If the user selected a folder, put it in the Replace Root Directory text box.
			if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				txtReplaceRootDirectory.Text = folderDialog.SelectedPath;
		}

		/// <summary>
		/// Handles the Click event of the btnGetPathLengths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private async void btnGetPathLengths_Click(object sender, RoutedEventArgs e)
		{
			// Show the Cancellation button while we search.
			_searchCancellationTokenSource = new CancellationTokenSource();
			btnGetPathLengths.IsEnabled = false;
			btnGetPathLengths.Visibility = Visibility.Collapsed;
			btnCancelGetPathLengths.IsEnabled = true;
			btnCancelGetPathLengths.Visibility = Visibility.Visible;

			// Clear any previous paths out.
			Paths.Clear();
			txtNumberOfPaths.Text = string.Empty;
			txtMinAndMaxPathLengths.Text = string.Empty;

			RecordAndDisplayTimeSearchStarted();

			// Search for all paths that match the search criteria.
			try
			{
				await BuildSearchOptionsAndGetPaths(txtRootDirectory.Text.Trim(), txtReplaceRootDirectory.Text.Trim(), txtSearchPattern.Text, _searchCancellationTokenSource.Token);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred while retrieving paths:{Environment.NewLine}{Environment.NewLine}{ex.Message}", "Error Occurred");
				Debug.WriteLine(ex.ToString());
			}

			DisplayResultsMetadata();

			// Restore the search button.
			btnGetPathLengths.IsEnabled = true;
			btnGetPathLengths.Visibility = Visibility.Visible;
			btnCancelGetPathLengths.IsEnabled = false;
			btnCancelGetPathLengths.Visibility = Visibility.Collapsed;
		}

		private void RecordAndDisplayTimeSearchStarted()
		{
			_timePathSearchingStarted = DateTime.Now;
			txtNumberOfPaths.Text = $"Started searching at {_timePathSearchingStarted.ToString("h:mm:ss tt")}...";
		}

		/// <summary>
		/// Gets the paths and displays them on the UI.
		/// </summary>
		private async Task BuildSearchOptionsAndGetPaths(string rootDirectory, string rootDirectoryReplacement, string searchPattern, CancellationToken cancellationToken)
		{
			try
			{
				rootDirectory = Path.GetFullPath(rootDirectory);
			}
			catch
			{
				MessageBox.Show($"The Starting Directory \"{rootDirectory}\" does not exist. Please specify a valid directory.", "Invalid Starting Directory");
				return;
			}

			if (!Directory.Exists(rootDirectory))
			{
				MessageBox.Show($"The Starting Directory \"{rootDirectory}\" does not exist. Please specify a valid directory.", "Invalid Starting Directory");
				return;
			}

			int minPathLength = numMinPathLength.Value ?? 0;
			int maxPathLength = numMaxPathLength.Value ?? PathLengthSearchOptions.MaximumPathLengthMaxValue;

			// If we should NOT be replacing the Root Directory text, make sure we don't pass anything in for that parameter.
			if (!(chkReplaceRootDirectory.IsChecked ?? false))
				rootDirectoryReplacement = null;

			// Build the options to search with.
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = rootDirectory,
				SearchPattern = searchPattern,
				SearchOption = (chkIncludeSubdirectories.IsChecked ?? false) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly,
				TypesToGet = (FileSystemTypes)cmbTypesToInclude.SelectedValue,
				RootDirectoryReplacement = rootDirectoryReplacement,
				UrlEncodePaths = (chkUrlEncodePaths.IsChecked ?? false),
				MinimumPathLength = minPathLength,
				MaximumPathLength = maxPathLength
			};

			// Get the paths in a background task so we don't lock the UI.
			var newPaths = await Task.Run(() =>
			{
				var paths = PathLengthChecker.PathLengthChecker.GetPathsWithLengths(searchOptions, cancellationToken);
				return new ObservableCollection<PathInfo>(paths.ToList());
			}, cancellationToken);

			// Assigning Paths to a new ObservableCollection wipes out the column sorting in the CollectionViewSource.
			// Ideally we would just use Paths.Add() to repopulate the list, which would preserve the sorting, but it takes forever when there's a lot of items.
			// So instead we backup the CollectionViewSource sorting before assigning Paths to a new ObservableCollection, and then restore it after.
			var previousColumnSortDescriptions = GetCurrentGridColumnSortDescriptions().ToList();

			Paths = newPaths;

			// Restore the previous column sort directions on the GUI DataGrid.
			SetGridColumnSortDescriptions(previousColumnSortDescriptions);
		}

		private SortDescriptionCollection GetCurrentGridColumnSortDescriptions()
		{
			var collectionView = CollectionViewSource.GetDefaultView(dgPaths.ItemsSource);
			return collectionView.SortDescriptions;
		}

		private void SetGridColumnSortDescriptions(SortDescriptionCollection sortDescriptions)
		{
			SetGridColumnSortDescriptions(sortDescriptions.ToList());
		}

		private void SetGridColumnSortDescriptions(IEnumerable<SortDescription> sortDescriptions)
		{
			var collectionView = CollectionViewSource.GetDefaultView(dgPaths.ItemsSource);
			collectionView.SortDescriptions.Clear();
			sortDescriptions.ToList().ForEach(collectionView.SortDescriptions.Add);

			// We need to manually update the sort direction of each column on the grid to show it's sorting glyph.
			foreach (var column in dgPaths.Columns)
			{
				var columnsSortDescription = sortDescriptions.FirstOrDefault(c => string.Equals(c.PropertyName, column.SortMemberPath));
				if (columnsSortDescription.PropertyName != null)
				{
					column.SortDirection = columnsSortDescription.Direction;
				}
				else
				{
					column.SortDirection = null;
				}
			}
		}

		private void DisplayResultsMetadata()
		{
			// Display the number of paths found.
			var timeSinceSearchingStarted = DateTime.Now - _timePathSearchingStarted;
			var text = $"{Paths.Count} paths found in {timeSinceSearchingStarted.ToString(@"mm\:ss\.f")}";

			// If the user cancelled the search part way through, indicate that.
			if (_searchCancellationTokenSource.IsCancellationRequested)
			{
				text += " - Search Cancelled";
			}

			this.txtNumberOfPaths.Text = text;

			// Display the shortest and longest path lengths.
			int shortestPathLength = Paths.Count > 0 ? Paths.Min(p => p.Length) : 0;
			int longestPathLength = Paths.Count > 0 ? Paths.Max(p => p.Length) : 0;
			txtMinAndMaxPathLengths.Text = string.Format("Shortest Path: {0}, Longest Path: {1} characters", shortestPathLength, longestPathLength);
		}

		private void splitbtnCopyToClipboard_Click(object sender, RoutedEventArgs e)
		{
			var text = GetPathsFromUiDataGridAsString(includeLength: true);
			SetClipboardText(text);
		}

		private void btnCopyToClipboardWithoutLengths_Click(object sender, RoutedEventArgs e)
		{
			var text = GetPathsFromUiDataGridAsString(includeLength: false);
			SetClipboardText(text);
			CloseCopyToClipboardSplitButtonDropDown();
		}

		private void btnCopyToClipboardAsCsv_Click(object sender, RoutedEventArgs e)
		{
			var text = GetPathsFromUiDataGridAsCsvString(includeLength: true);
			SetClipboardText(text);
			CloseCopyToClipboardSplitButtonDropDown();
		}

		private void btnCopyToClipboardWithoutLengthsAsCsv_Click(object sender, RoutedEventArgs e)
		{
			var text = GetPathsFromUiDataGridAsCsvString(includeLength: false);
			SetClipboardText(text);
			CloseCopyToClipboardSplitButtonDropDown();
		}

		private void CloseCopyToClipboardSplitButtonDropDown()
		{
			splitbtnCopyToClipboard.IsOpen = false;
		}

		private string GetPathsFromUiDataGridAsString(bool includeLength)
		{
			var text = new StringBuilder();
			foreach (var path in PathsFromUiDataGrid)
			{
				var item = includeLength ? $"{path.Length}: {path.Path}" : path.Path;
				text.AppendLine(item);
			}
			return text.ToString().Trim();
		}

		private string GetPathsFromUiDataGridAsCsvString(bool includeLength)
		{
			var text = new StringBuilder();

			var header = includeLength ?
					"Length,\"Path\"" :
					"\"Path\"";
			text.AppendLine(header);

			foreach (var path in PathsFromUiDataGrid)
			{
				var item = includeLength ?
					$"{path.Length},\"{path.Path}\"" :
					$"\"{path.Path}\"";
				text.AppendLine(item);
			}
			return text.ToString().Trim();
		}

		/// <summary>
		/// Handles threading issues and swallows exceptions.
		/// </summary>
		/// <param name="text">A string for the clipboard</param>
		private static void SetClipboardText(string text)
		{
			// Copying to the clipboard can be unreliable, so we need to implement retries: https://stackoverflow.com/a/69081/602585
			int maxAttempts = 100;
			int millisecondsBetweenAttempts = 10;
			for (int attempts = 1; attempts <= maxAttempts; attempts++)
			{
				try
				{
					Clipboard.SetText(text);
					return;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.ToString());

					if (attempts == maxAttempts)
					{
						MessageBox.Show($"An error occurred while copying text to the clipboard:{Environment.NewLine}{Environment.NewLine}{ex.Message}", "Error Occurred Copying To Clipboard");
					}
				}
				System.Threading.Thread.Sleep(millisecondsBetweenAttempts);
			}
		}

		private void btnCancelGetPathLengths_Click(object sender, RoutedEventArgs e)
		{
			if (!_searchCancellationTokenSource.IsCancellationRequested)
			{
				_searchCancellationTokenSource.Cancel();
				btnCancelGetPathLengths.IsEnabled = false;
			}
		}

		private void dgPaths_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
		{
			// Show row numbers in the grid.
			e.Row.Header = (e.Row.GetIndex() + 1).ToString();
		}

		private void MenuItem_OpenDirectoryInFileExplorer_Click(object sender, RoutedEventArgs e)
		{
			// Get the directory path, as we can't open a file in File Explorer.
			var directoryPath = string.Empty;
			if (Directory.Exists(SelectedPath.Path))
			{
				directoryPath = SelectedPath.Path;
			}
			else if (File.Exists(SelectedPath.Path))
			{
				directoryPath = Directory.GetParent(SelectedPath.Path).FullName;
			}

			if (string.IsNullOrWhiteSpace(directoryPath))
			{
				MessageBox.Show($"The following directory (or file's directory) either does not anymore, you don't have permissions to access it, or it's path is greater than 260 characters, so it cannot be opened.{Environment.NewLine}{Environment.NewLine}{SelectedPath.Path}", "Cannot Open Directory");
			}
			else
			{
				Process.Start(directoryPath);
			}
		}

		/// <summary>
		/// Sets the state of UI controls based on the provided search options
		/// </summary>
		protected internal void SetUIControlsFromSearchOptions(PathLengthSearchOptions argSearchOptions)
		{
			txtRootDirectory.Text = argSearchOptions.RootDirectory;
			txtSearchPattern.Text = argSearchOptions.SearchPattern;
			chkIncludeSubdirectories.IsChecked = argSearchOptions.SearchOption == SearchOption.AllDirectories;
			cmbTypesToInclude.SelectedValue = argSearchOptions.TypesToGet;

			if (!string.IsNullOrEmpty(argSearchOptions.RootDirectoryReplacement))
			{
				txtReplaceRootDirectory.Text = argSearchOptions.RootDirectoryReplacement;
				chkReplaceRootDirectory.IsChecked = true;
			}
			chkUrlEncodePaths.IsChecked = argSearchOptions.UrlEncodePaths;

			numMinPathLength.Value = argSearchOptions.MinimumPathLength;
			numMaxPathLength.Value = argSearchOptions.MaximumPathLength;
		}

		private void btnResetSearchOptions_Click(object sender, RoutedEventArgs e)
		{
			ResetAllUiSearchOptionsToDefaultValues();
		}

		private void ResetAllUiSearchOptionsToDefaultValues()
		{
			// Values specified here should match the default values in the Properties\Settings.settings file.

			txtRootDirectory.Text = string.Empty;
			txtSearchPattern.Text = string.Empty;

			numMinPathLength.Value = 0;
			numMaxPathLength.Value = PathLengthSearchOptions.MaximumPathLengthMaxValue;

			chkIncludeSubdirectories.IsChecked = true;
			cmbTypesToInclude.SelectedValue = FileSystemTypes.All;

			txtReplaceRootDirectory.Text = string.Empty;
			chkReplaceRootDirectory.IsChecked = false;
			chkUrlEncodePaths.IsChecked = false;
		}
	}
}
