using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PathLengthChecker;

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
		}

		public List<PathInfo> Paths
		{
			get { return (List<PathInfo>)GetValue(PathsProperty); }
			set { SetValue(PathsProperty, value); }
		}
		public static readonly DependencyProperty PathsProperty = DependencyProperty.Register("Paths", typeof(List<PathInfo>), typeof(MainWindow), new UIPropertyMetadata(new List<PathInfo>()));

		/// <summary>
		/// Handles the Click event of the btnBrowseForRootDirectory control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void btnBrowseForRootDirectory_Click(object sender, RoutedEventArgs e)
		{
			// Setup the prompt
			var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
			folderDialog.Description = "Select the directory that contains the paths whose length you want to check...";
			folderDialog.ShowNewFolderButton = false;

			// If the user selected a folder, put it in the Root Directory text box.
			if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				txtRootDirectory.Text = folderDialog.SelectedPath;
		}

		/// <summary>
		/// Handles the Click event of the btnBrowseForReplaceRootDirectory control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void btnBrowseForReplaceRootDirectory_Click(object sender, RoutedEventArgs e)
		{
			// Setup the prompt
			var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
			folderDialog.Description = "Select the directory that you want to use to replace the root directory in the returned paths...";
			folderDialog.ShowNewFolderButton = false;

			// If the user selected a folder, put it in the Replace Root Directory text box.
			if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				txtReplaceRootDirectory.Text = folderDialog.SelectedPath;
		}

		/// <summary>
		/// Handles the Click event of the btnGetPathLengths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void btnGetPathLengths_Click(object sender, RoutedEventArgs e)
		{
			GetPaths();
		}

		/// <summary>
		/// Gets the paths and displays them on the UI.
		/// </summary>
		private void GetPaths()
		{
			var rootDirectory = txtRootDirectory.Text.Trim();
			var rootDirectoryReplacement = txtReplaceRootDirectory.Text.Trim();
			var searchPattern = txtSearchPattern.Text;
			int minPathLength = numMinPathLength.Value ?? 0;
			int maxPathLength = numMaxPathLength.Value ?? 999999;

			if (!Directory.Exists(rootDirectory))
			{
				MessageBox.Show(string.Format("The Root Directory \"{0}\" does not exist. Please specify a valid directory.", rootDirectory), "Invalid Root Directory");
				return;
			}

			// Clear any previous paths out.
			this.Paths = new List<PathInfo>();
			txtNumberOfPaths.Text = string.Empty;
			txtMinAndMaxPathLengths.Text = string.Empty;

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

			try
			{
				// Get and display the new paths.
				Paths = PathLengthChecker.PathLengthChecker.GetPathsWithLengths(searchOptions).ToList();

				// Display the number of paths found.
				txtNumberOfPaths.Text = Paths.Count + " Paths Found";

				// Display the shortest and longest path lengths.
				int shortestPathLength = Paths.Count > 0 ? Paths.Min(p => p.Length) : 0;
				int longestPathLength = Paths.Count > 0 ? Paths.Max(p => p.Length) : 0;
				txtMinAndMaxPathLengths.Text = string.Format("Shortest Path: {0}, Longest Path: {1} characters", shortestPathLength, longestPathLength);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error Occurred");
			}
		}

		/// <summary>
		/// Handles the Click event of the btnCopyToClipboardWithoutLengths control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void btnCopyToClipboardWithoutLengths_Click(object sender, RoutedEventArgs e)
		{
			var text = new StringBuilder();
			foreach (var path in Paths)
			{
				text.AppendLine(path.Path);
			}
			Clipboard.SetText(text.ToString());
		}

		/// <summary>
		/// Handles the Click event of the btnCopyToClipboard control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void btnCopyToClipboard_Click(object sender, RoutedEventArgs e)
		{
			var text = new StringBuilder();
			foreach (var path in Paths)
			{
				text.AppendLine(path.ToString());
			}
			Clipboard.SetText(text.ToString());
		}
	}
}
