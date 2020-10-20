using PathLengthChecker;
using System;
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

			// Set the default type for the combo boxes.
			cmbTypesToInclude.SelectedValue = FileSystemTypes.All;

			SetWindowTitle();
		}

		private void SetWindowTitle()
		{
			this.Title = "Path Length Checker v" + Assembly.GetEntryAssembly().GetName().Version.ToString(3) + " - Written by Daniel Schroeder";
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
			Paths = new ObservableCollection<PathInfo>();
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
			int maxPathLength = numMaxPathLength.Value ?? 999999;

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
				MinimumPathLength = minPathLength,
				MaximumPathLength = maxPathLength
			};

			// Get the paths in a background task so we don't lock the UI.
			Paths = await Task.Run(() =>
			{
				var paths = PathLengthChecker.PathLengthChecker.GetPathsWithLengths(searchOptions, cancellationToken);
				return new ObservableCollection<PathInfo>(paths.ToList());
			}, cancellationToken);
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
			var text = GetPathsAsString(includeLength: true);
			SetClipboardText(text);
		}

		private void btnCopyToClipboardWithoutLengths_Click(object sender, RoutedEventArgs e)
		{
			var text = GetPathsAsString(includeLength: false);
			SetClipboardText(text);
			CloseCopyToClipboardSplitButtonDropDown();
		}

		private void btnCopyToClipboardAsCsv_Click(object sender, RoutedEventArgs e)
		{
			var text = GetPathsAsCsvString(includeLength: true);
			SetClipboardText(text);
			CloseCopyToClipboardSplitButtonDropDown();
		}
		
		private void btnCopyToClipboardWithoutLengthsAsCsv_Click(object sender, RoutedEventArgs e)
		{
			var text = GetPathsAsCsvString(includeLength: false);
			SetClipboardText(text);
			CloseCopyToClipboardSplitButtonDropDown();
		}

		private void CloseCopyToClipboardSplitButtonDropDown()
		{
			splitbtnCopyToClipboard.IsOpen = false;
		}

		private string GetPathsAsString(bool includeLength)
		{
			var text = new StringBuilder();
			foreach (var path in Paths)
			{
				var item = includeLength ? $"{path.Length}: {path.Path}" : path.Path;
				text.AppendLine(item);
			}
			return text.ToString();
		}

		private string GetPathsAsCsvString(bool includeLength)
		{
			var text = new StringBuilder();
			foreach (var path in Paths)
			{
				var item = includeLength ? $"{path.Length},{path.Path}" : path.Path;
				text.Append(item + ",");
			}
			return text.ToString().TrimEnd(',');
		}

		/// <summary>
		/// Handles threading issues and swallows exceptions.
		/// </summary>
		/// <param name="text">A string for the clipboard</param>
		private static void SetClipboardText(string text)
		{
			try
			{
				Thread thread = new Thread(() => Clipboard.SetText(text));
				thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
				thread.Start();
				thread.Join();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred while copying text to the clipboard:{Environment.NewLine}{Environment.NewLine}{ex.Message}", "Error Occurred");
				Debug.WriteLine(ex.ToString());
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

			numMinPathLength.Value = argSearchOptions.MinimumPathLength;
			numMaxPathLength.Value = argSearchOptions.MaximumPathLength;
		}
	}
}
