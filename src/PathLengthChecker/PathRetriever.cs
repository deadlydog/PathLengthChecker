using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PathLengthChecker
{
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
				throw new DirectoryNotFoundException($"The specified root directory '{searchOptions.RootDirectory}' does not exist. Please provide a valid directory.");
			}

			// If no Search Pattern was provided, then find everything.
			if (string.IsNullOrEmpty(searchOptions.SearchPattern))
				searchOptions.SearchPattern = "*";

			var enumerationOptions = new EnumerationOptions
			{
				RecurseSubdirectories = searchOptions.SearchOption == SearchOption.AllDirectories,
				IgnoreInaccessible = true,
				AttributesToSkip = FileAttributes.ReparsePoint,
				MatchCasing = MatchCasing.PlatformDefault,
			};

			IEnumerable<string> paths;
			switch (searchOptions.TypesToGet)
			{
				case FileSystemTypes.Files:
					paths = Directory.EnumerateFiles(searchOptions.RootDirectory, searchOptions.SearchPattern, enumerationOptions);
					break;
				case FileSystemTypes.Directories:
					paths = Directory.EnumerateDirectories(searchOptions.RootDirectory, searchOptions.SearchPattern, enumerationOptions);
					break;
				default:
					paths = Directory.EnumerateFileSystemEntries(searchOptions.RootDirectory, searchOptions.SearchPattern, enumerationOptions);
					break;
			}

			// Return each of the paths, replacing the Root Directory if specified to do so.
			foreach (var path in paths)
			{
				// If we've been asked to stop searching, just return.
				if (cancellationToken.IsCancellationRequested)
					yield break;

				var transformedPath = path;

				if (searchOptions.RootDirectoryReplacement != null)
					transformedPath = transformedPath.Replace(searchOptions.RootDirectory, searchOptions.RootDirectoryReplacement);

				if (searchOptions.UrlEncodePaths)
					transformedPath = Uri.EscapeDataString(transformedPath);

				yield return transformedPath;
			}
		}
	}
}
