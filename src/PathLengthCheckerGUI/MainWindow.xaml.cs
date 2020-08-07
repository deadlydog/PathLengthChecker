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

namespace PathLengthCheckerGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
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

		public BindingList<PathInfo> Paths
		{
			get { return (BindingList<PathInfo>)GetValue(PathsProperty); }
			set { SetValue(PathsProperty, value); }
		}
		public static readonly DependencyProperty PathsProperty = DependencyProperty.Register("Paths", typeof(BindingList<PathInfo>), typeof(MainWindow), new UIPropertyMetadata(new BindingList<PathInfo>()));

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
				Description = "Select the directory that you want to use to replace the root directory in the returned paths...",
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
			this.btnGetPathLengths.IsEnabled = false;

			// Clear any previous paths out.
			this.Paths = new BindingList<PathInfo>();
			txtNumberOfPaths.Text = string.Empty;
			txtMinAndMaxPathLengths.Text = string.Empty;

			await GetPaths(txtRootDirectory.Text.Trim(), txtReplaceRootDirectory.Text.Trim(), txtSearchPattern.Text);

			// Display the shortest and longest path lengths.
			int shortestPathLength = Paths.Count > 0 ? Paths.Min(p => p.Length) : 0;
			int longestPathLength = Paths.Count > 0 ? Paths.Max(p => p.Length) : 0;
			txtMinAndMaxPathLengths.Text = string.Format("Shortest Path: {0}, Longest Path: {1} characters", shortestPathLength, longestPathLength);

			this.btnGetPathLengths.IsEnabled = true;
		}

		/// <summary>
		/// Gets the paths and displays them on the UI.
		/// </summary>
		private async Task GetPaths(string rootDirectory, string rootDirectoryReplacement, string searchPattern)
		{
			try
			{
				rootDirectory = Path.GetFullPath(rootDirectory);
			}
			catch
			{
				MessageBox.Show(string.Format("The Root Directory \"{0}\" does not exist. Please specify a valid directory.", txtRootDirectory.Text), "Invalid Root Directory");
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


			// Get the new paths.
			await Task.Run(() =>
			{
				try
				{
					foreach (var pathItem in PathLengthChecker.PathLengthChecker.GetPathsWithLengths(searchOptions))
					{
						AddPathToList(pathItem);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Error Occurred");
				}
			});
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
				this.txtNumberOfPaths.Text = Paths.Count + " Paths Found";
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
				Debug.Print(ex.Message);
			}
		}
	}
}
