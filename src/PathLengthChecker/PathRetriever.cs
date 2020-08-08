using Alphaleonis.Win32.Filesystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SearchOption = System.IO.SearchOption;

namespace PathLengthChecker
{
	/// <summary>
	/// The type of Paths that should be included.
	/// </summary>
	[Flags]
	public enum FileSystemTypes
	{
		Files = 1,
		Directories = 2,
		All = Files | Directories
	}

	public enum FileSystemSearchStrategies
	{
		/// <summary>
		/// Uses the default .Net implementation.
		/// Very fast, but haults when it encounters a directory it doesn't have permissions to go into.
		/// </summary>
		Fast = 1,

		/// <summary>
		/// Uses 3rd party AlphaFS implementation.
		/// Safer in that it is able to enumerate over all directories, but slower.
		/// </summary>
		Safe = 2
	}

	/// <summary>
	/// Options used when retrieving paths.
	/// </summary>
	public class PathSearchOptions
	{
		/// <summary>
		/// The root directory to search in.
		/// </summary>
		public string RootDirectory = string.Empty;

		/// <summary>
		/// The search pattern to match against.
		/// </summary>
		public string SearchPattern = "*";

		/// <summary>
		/// Specifies if we should search subdirectories or not.
		/// </summary>
		public SearchOption SearchOption = SearchOption.AllDirectories;

		/// <summary>
		/// Specifies if we should look for Files, Directories, or both.
		/// </summary>
		public FileSystemTypes TypesToGet = FileSystemTypes.All;

		/// <summary>
		/// The directory that the root directory should be replaced with in the found paths.
		/// Specify null to leave the original paths unmodified.
		/// </summary>
		public string RootDirectoryReplacement = null;

		/// <summary>
		/// Which implementation to use for searching the file system.
		/// </summary>
		public FileSystemSearchStrategies FileSystemSearchStrategy = FileSystemSearchStrategies.Fast;
	}

	/// <summary>
	/// Class used to retrieve file system objects in a given path.
	/// </summary>
	public static class PathRetriever
	{
		/// <summary>
		/// Gets the paths.
		/// </summary>
		/// <param name="searchOptions">The search options to use.</param>
		public static IEnumerable<string> GetPaths(PathSearchOptions searchOptions, CancellationToken cancellationToken)
		{
			if (!Directory.Exists(searchOptions.RootDirectory))
			{
				throw new System.IO.DirectoryNotFoundException($"The specified root directory '{searchOptions.RootDirectory}' does not exist. Please provide a valid directory.");
			}

			// If no Search Pattern was provided, then find everything.
			if (string.IsNullOrEmpty(searchOptions.SearchPattern))
				searchOptions.SearchPattern = "*";

			// Get the paths according to the search parameters
			IEnumerable<string> paths = Enumerable.Empty<string>();
			switch (searchOptions.FileSystemSearchStrategy)
			{
				case FileSystemSearchStrategies.Safe:
					paths = GetPathsUsingAlphaFs(searchOptions);
					break;

				default:
				case FileSystemSearchStrategies.Fast:
					paths = GetPathsUsingSystemIo(searchOptions);
					break;
			}

			// Return each of the paths, replacing the Root Directory if specified to do so.
			foreach (var path in paths)
			{
				// If we've been asked to stop searching, just return.
				if (cancellationToken.IsCancellationRequested)
					yield break;

				if (searchOptions.RootDirectoryReplacement == null)
					yield return path;
				else
					yield return path.Replace(searchOptions.RootDirectory, searchOptions.RootDirectoryReplacement);
			}
		}

		private static IEnumerable<string> GetPathsUsingAlphaFs(PathSearchOptions searchOptions)
		{
			DirectoryEnumerationOptions options = (DirectoryEnumerationOptions)searchOptions.TypesToGet |
				DirectoryEnumerationOptions.ContinueOnException | DirectoryEnumerationOptions.SkipReparsePoints;

			if (searchOptions.SearchOption == SearchOption.AllDirectories)
				options |= DirectoryEnumerationOptions.Recursive;

			var paths = Directory.EnumerateFileSystemEntries(searchOptions.RootDirectory, searchOptions.SearchPattern, options);
			return paths;
		}

		private static IEnumerable<string> GetPathsUsingSystemIo(PathSearchOptions searchOptions)
		{
			// Get the paths according to the search parameters
			var paths = Enumerable.Empty<string>();

			switch (searchOptions.TypesToGet)
			{
				default:
				case FileSystemTypes.All:
					paths = Directory.GetFileSystemEntries(searchOptions.RootDirectory, searchOptions.SearchPattern, searchOptions.SearchOption);
					break;

				case FileSystemTypes.Directories:
					paths = Directory.GetDirectories(searchOptions.RootDirectory, searchOptions.SearchPattern, searchOptions.SearchOption);
					break;

				case FileSystemTypes.Files:
					paths = Directory.GetFiles(searchOptions.RootDirectory, searchOptions.SearchPattern, searchOptions.SearchOption);
					break;
			}

			return paths;
		}
	}
}
