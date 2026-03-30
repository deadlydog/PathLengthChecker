using PathLengthChecker;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PathLengthCheckerGUI.ViewModels
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		private readonly AppSettings _settings;
		private DateTime _timePathSearchingStarted = DateTime.MinValue;
		private CancellationTokenSource _searchCancellationTokenSource = new CancellationTokenSource();

		public event PropertyChangedEventHandler? PropertyChanged;

		public MainWindowViewModel()
		{
			_settings = AppSettings.Load();
			_rootDirectory = _settings.RootDirectory;
			_searchPattern = _settings.SearchPattern;
			_includeSubdirectories = _settings.IncludeSubdirectories;
			_selectedFileSystemType = _settings.FileSystemTypesToInclude;
			_minPathLength = _settings.MinPathLength;
			_maxPathLength = _settings.MaxPathLength;
			_replaceRootDirectory = _settings.ReplaceRootDirectory;
			_rootDirectoryReplacement = _settings.RootDirectoryReplacementText;
			_urlEncodePaths = _settings.UrlEncodePaths;
		}

		public AppSettings Settings => _settings;

		// ── Search options ────────────────────────────────────────────────

		private string _rootDirectory = string.Empty;
		public string RootDirectory
		{
			get => _rootDirectory;
			set { _rootDirectory = value; OnPropertyChanged(); }
		}

		private string _searchPattern = string.Empty;
		public string SearchPattern
		{
			get => _searchPattern;
			set { _searchPattern = value; OnPropertyChanged(); }
		}

		private bool _includeSubdirectories = true;
		public bool IncludeSubdirectories
		{
			get => _includeSubdirectories;
			set { _includeSubdirectories = value; OnPropertyChanged(); }
		}

		private FileSystemTypes _selectedFileSystemType = FileSystemTypes.All;
		public FileSystemTypes SelectedFileSystemType
		{
			get => _selectedFileSystemType;
			set { _selectedFileSystemType = value; OnPropertyChanged(); }
		}

		public IReadOnlyList<FileSystemTypes> FileSystemTypeOptions { get; } =
			Enum.GetValues<FileSystemTypes>().ToList();

		private int _minPathLength = 0;
		public int MinPathLength
		{
			get => _minPathLength;
			set { _minPathLength = value; OnPropertyChanged(); }
		}

		private int _maxPathLength = PathLengthSearchOptions.MaximumPathLengthMaxValue;
		public int MaxPathLength
		{
			get => _maxPathLength;
			set { _maxPathLength = value; OnPropertyChanged(); }
		}

		private bool _replaceRootDirectory = false;
		public bool ReplaceRootDirectory
		{
			get => _replaceRootDirectory;
			set { _replaceRootDirectory = value; OnPropertyChanged(); }
		}

		private string _rootDirectoryReplacement = string.Empty;
		public string RootDirectoryReplacement
		{
			get => _rootDirectoryReplacement;
			set { _rootDirectoryReplacement = value; OnPropertyChanged(); }
		}

		private bool _urlEncodePaths = false;
		public bool UrlEncodePaths
		{
			get => _urlEncodePaths;
			set { _urlEncodePaths = value; OnPropertyChanged(); }
		}

		// ── UI state ──────────────────────────────────────────────────────

		private bool _isSearching = false;
		public bool IsSearching
		{
			get => _isSearching;
			set { _isSearching = value; OnPropertyChanged(); }
		}

		private string _numberOfPathsText = string.Empty;
		public string NumberOfPathsText
		{
			get => _numberOfPathsText;
			set { _numberOfPathsText = value; OnPropertyChanged(); }
		}

		private string _minAndMaxPathLengthsText = string.Empty;
		public string MinAndMaxPathLengthsText
		{
			get => _minAndMaxPathLengthsText;
			set { _minAndMaxPathLengthsText = value; OnPropertyChanged(); }
		}

		// ── Results ───────────────────────────────────────────────────────

		private ObservableCollection<PathInfo> _paths = new ObservableCollection<PathInfo>();
		public ObservableCollection<PathInfo> Paths
		{
			get => _paths;
			set { _paths = value; OnPropertyChanged(); }
		}

		private PathInfo _selectedPath = new PathInfo();
		public PathInfo SelectedPath
		{
			get => _selectedPath;
			set { _selectedPath = value; OnPropertyChanged(); }
		}

		// ── Search logic ──────────────────────────────────────────────────

		public void BeginSearch()
		{
			_searchCancellationTokenSource = new CancellationTokenSource();
			IsSearching = true;
			NumberOfPathsText = string.Empty;
			MinAndMaxPathLengthsText = string.Empty;
			Paths.Clear();

			_timePathSearchingStarted = DateTime.Now;
			NumberOfPathsText = $"Started searching at {_timePathSearchingStarted:h:mm:ss tt}...";
		}

		public void CancelSearch()
		{
			if (!_searchCancellationTokenSource.IsCancellationRequested)
				_searchCancellationTokenSource.Cancel();
		}

		public CancellationToken GetSearchCancellationToken() => _searchCancellationTokenSource.Token;

		/// <summary>
		/// Validates the root directory and runs the path search. Returns null if validation fails.
		/// </summary>
		public async Task<ObservableCollection<PathInfo>?> GetPathsAsync(
			string rootDirectory, string rootDirectoryReplacement, string searchPattern,
			CancellationToken cancellationToken)
		{
			try
			{
				rootDirectory = Path.GetFullPath(rootDirectory);
			}
			catch
			{
				return null;
			}

			if (!Directory.Exists(rootDirectory))
				return null;

			string? replacement = ReplaceRootDirectory && !string.IsNullOrEmpty(rootDirectoryReplacement)
				? rootDirectoryReplacement
				: null;

			var searchOptions = new PathLengthSearchOptions
			{
				RootDirectory = rootDirectory,
				SearchPattern = searchPattern,
				SearchOption = IncludeSubdirectories
					? System.IO.SearchOption.AllDirectories
					: System.IO.SearchOption.TopDirectoryOnly,
				TypesToGet = SelectedFileSystemType,
				RootDirectoryReplacement = replacement,
				UrlEncodePaths = UrlEncodePaths,
				MinimumPathLength = MinPathLength,
				MaximumPathLength = MaxPathLength
			};

			var newPaths = await Task.Run(() =>
			{
				var paths = PathLengthChecker.PathLengthChecker.GetPathsWithLengths(searchOptions, cancellationToken);
				return new ObservableCollection<PathInfo>(paths.ToList());
			}, cancellationToken);

			return newPaths;
		}

		public void FinishSearch(ObservableCollection<PathInfo> newPaths)
		{
			// Clear and re-add to preserve any column sort order in the DataGrid.
			Paths.Clear();
			foreach (var path in newPaths)
				Paths.Add(path);

			DisplayResultsMetadata();
			IsSearching = false;
		}

		private void DisplayResultsMetadata()
		{
			var elapsed = DateTime.Now - _timePathSearchingStarted;
			var text = $"{Paths.Count} paths found in {elapsed:mm\\:ss\\.f}";

			if (_searchCancellationTokenSource.IsCancellationRequested)
				text += " - Search Cancelled";

			NumberOfPathsText = text;

			int shortest = Paths.Count > 0 ? Paths.Min(p => p.Length) : 0;
			int longest = Paths.Count > 0 ? Paths.Max(p => p.Length) : 0;
			MinAndMaxPathLengthsText = $"Shortest Path: {shortest}, Longest Path: {longest} characters";
		}

		// ── Settings persistence ──────────────────────────────────────────

		public void SaveToSettings(double windowWidth, double windowHeight,
			double windowLeft, double windowTop, Avalonia.Controls.WindowState windowState)
		{
			_settings.RootDirectory = RootDirectory;
			_settings.SearchPattern = SearchPattern;
			_settings.IncludeSubdirectories = IncludeSubdirectories;
			_settings.FileSystemTypesToInclude = SelectedFileSystemType;
			_settings.MinPathLength = MinPathLength;
			_settings.MaxPathLength = MaxPathLength;
			_settings.ReplaceRootDirectory = ReplaceRootDirectory;
			_settings.RootDirectoryReplacementText = RootDirectoryReplacement;
			_settings.UrlEncodePaths = UrlEncodePaths;
			_settings.WindowWidth = windowWidth;
			_settings.WindowHeight = windowHeight;
			_settings.WindowLeftPosition = windowLeft;
			_settings.WindowTopPosition = windowTop;
			_settings.WindowState = windowState;
			_settings.Save();
		}

		public void ResetToDefaults()
		{
			RootDirectory = string.Empty;
			SearchPattern = string.Empty;
			MinPathLength = 0;
			MaxPathLength = PathLengthSearchOptions.MaximumPathLengthMaxValue;
			IncludeSubdirectories = true;
			SelectedFileSystemType = FileSystemTypes.All;
			RootDirectoryReplacement = string.Empty;
			ReplaceRootDirectory = false;
			UrlEncodePaths = false;
		}

		// ── Clipboard helpers ─────────────────────────────────────────────

		public string GetPathsAsString(bool includeLength)
		{
			var text = new StringBuilder();
			foreach (var path in Paths)
			{
				text.AppendLine(includeLength ? $"{path.Length}: {path.Path}" : path.Path);
			}
			return text.ToString().Trim();
		}

		public string GetPathsAsCsvString(bool includeLength)
		{
			var text = new StringBuilder();
			text.AppendLine(includeLength ? "Length,\"Path\"" : "\"Path\"");
			foreach (var path in Paths)
			{
				text.AppendLine(includeLength ? $"{path.Length},\"{path.Path}\"" : $"\"{path.Path}\"");
			}
			return text.ToString().Trim();
		}

		protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
