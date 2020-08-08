using PathLengthChecker;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PathLengthCheckerGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private DateTime _timePathSearchingStarted = DateTime.MinValue;
		private CancellationTokenSource _searchCancellationTokenSource = new CancellationTokenSource();

		public MainWindow()
		{
			InitializeComponent();
			this.DataContext = this;

			// Set the default type for the combo boxes.
			cmbTypesToInclude.SelectedValue = FileSystemTypes.All;
			cmbSearchStrategy.SelectedValue = SearchStrategies.Fast;

			SetWindowTitle();
		}

		private void SetWindowTitle()
		{
			this.Title = "Path Length Checker v" + Assembly.GetEntryAssembly().GetName().Version.ToString(3) + " - Written by Daniel Schroeder";
		}

		public BindingList<PathInfo> Paths
		{
			get { return (BindingList<PathInfo>)GetValue(PathsProperty); }
			set { SetValue(PathsProperty, value); }
		}
		public static readonly DependencyProperty PathsProperty = DependencyProperty.Register("Paths", typeof(BindingList<PathInfo>), typeof(MainWindow), new UIPropertyMetadata(new BindingList<PathInfo>()));

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
			this.Paths = new BindingList<PathInfo>();
			txtNumberOfPaths.Text = string.Empty;
			txtMinAndMaxPathLengths.Text = string.Empty;
			dgPaths.ItemsSource = null;	// Break the data binding as it kills performance to load in all the paths while it's searching.

			// Mark the time that we started searching.
			_timePathSearchingStarted = DateTime.Now;

			await GetPaths(txtRootDirectory.Text.Trim(), txtReplaceRootDirectory.Text.Trim(), txtSearchPattern.Text, _searchCancellationTokenSource.Token);

			// Display the results.
			dgPaths.ItemsSource = Paths;

			// Display the shortest and longest path lengths.
			int shortestPathLength = Paths.Count > 0 ? Paths.Min(p => p.Length) : 0;
			int longestPathLength = Paths.Count > 0 ? Paths.Max(p => p.Length) : 0;
			txtMinAndMaxPathLengths.Text = string.Format("Shortest Path: {0}, Longest Path: {1} characters", shortestPathLength, longestPathLength);

			// Restore the search button.
			btnGetPathLengths.IsEnabled = true;
			btnGetPathLengths.Visibility = Visibility.Visible;
			btnCancelGetPathLengths.IsEnabled = false;
			btnCancelGetPathLengths.Visibility = Visibility.Collapsed;
		}

		/// <summary>
		/// Gets the paths and displays them on the UI.
		/// </summary>
		private async Task GetPaths(string rootDirectory, string rootDirectoryReplacement, string searchPattern, CancellationToken cancellationToken)
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
				MaximumPathLength = maxPathLength,
				FileSystemSearchStrategy = (SearchStrategies)cmbSearchStrategy.SelectedValue == SearchStrategies.Fast ? FileSystemSearchStrategies.SystemIo : FileSystemSearchStrategies.AlphaFs
			};


			// Get the new paths.
			await Task.Run(() =>
			{
				try
				{
					foreach (var pathItem in PathLengthChecker.PathLengthChecker.GetPathsWithLengths(searchOptions, cancellationToken))
					{
						AddPathToList(pathItem);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show($"An error occurred while retrieving paths:{Environment.NewLine}{Environment.NewLine}{ex.Message}{Environment.NewLine}{Environment.NewLine}'Access is denied' and other errors can be skipped by choosing the 'Safe' Strategy.", "Error Occurred");
					Debug.WriteLine(ex.ToString());
				}
			}, cancellationToken);
		}

		public delegate void PathInfoConsumer(PathInfo pathItem);
		public void AddPathToList(PathInfo pathItem)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(new PathInfoConsumer(AddPathToList), new object[] { pathItem });
			}
			else
			{
				this.Paths.Add(pathItem);

				// Display the number of paths found.
				var timeSinceSearchingStarted = DateTime.Now - _timePathSearchingStarted;
				var text = $"{Paths.Count} paths found in {timeSinceSearchingStarted.ToString(@"mm\:ss\.f")}";

				if (_searchCancellationTokenSource.IsCancellationRequested)
				{
					text += " - Cancelled";
				}

				this.txtNumberOfPaths.Text = text;
			}
		}

		/// <summary>
		/// Handles the Click event of the btnCopyToClipboardWithoutLengths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnCopyToClipboardWithoutLengths_Click(object sender, RoutedEventArgs e)
		{
			var text = new StringBuilder();
			foreach (var path in Paths)
			{
				text.AppendLine(path.Path);
			}
			SetClipboardText(text.ToString());
		}

		/// <summary>
		/// Handles the Click event of the btnCopyToClipboard control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnCopyToClipboard_Click(object sender, RoutedEventArgs e)
		{
			var text = new StringBuilder();
			foreach (var path in Paths)
			{
				text.AppendLine(path.ToString());
			}
			SetClipboardText(text.ToString());
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
				MessageBox.Show($"The file/directory '{SelectedPath.Path}' does not appear to exist anymore, so it cannot be opened.", "Cannot open directory");
			}
			else
			{
				Process.Start(directoryPath);
			}
		}
	}
}
