using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathLengthChecker
{
	/// <summary>
	/// Holds info about a path.
	/// </summary>
	public struct PathInfo
	{
		public string Path { get; set; }
		public int Length
		{
			get { return this.Path.Length; }	
		}

		public override string ToString()
		{
			return Length + ": " + Path;
		}
	}

	/// <summary>
	/// Options used when retrieving paths with their lengths.
	/// </summary>
	public class PathLengthSearchOptions : PathSearchOptions
	{
		/// <summary>
		/// The Minimum Length that a Path must have to be included in the search results.
		/// Specify a value of -1 to ignore the minimum path length.
		/// </summary>
		public int MinimumPathLength = -1;

		/// <summary>
		/// The Maximum Length that a Path must have to be included in the search results.
		/// Specify a value of -1 to ignore the maximum path length.
		/// </summary>
		public int MaximumPathLength = -1;
	}

	/// <summary>
	/// Exception thrown when the Minimum Path Length search option is greater than the Maximum Path Length search option.
	/// </summary>
	public class MinPathLengthGreaterThanMaxPathLengthException : ArgumentException
	{
		public MinPathLengthGreaterThanMaxPathLengthException()
			: base("MinimumPathLength can not be greater than the MaximumPathLength.")
		{ }
	} 

	/// <summary>
	/// Class used to retrieve file system objects in a given path along with their path lengths.
	/// </summary>
	public static class PathLengthChecker
	{
		public static IEnumerable<string> GetPaths(PathLengthSearchOptions options)
		{
			// Make sure valid lengths were supplied
			if (options.MinimumPathLength > options.MaximumPathLength && options.MinimumPathLength >= 0 && options.MaximumPathLength >= 0)
				throw new MinPathLengthGreaterThanMaxPathLengthException();

			// Get the paths.
			var paths = PathRetriever.GetPaths(options);

			// Filter out paths that don't match the Minimum Path Length
			foreach (var path in paths.Where(path => path.Length >= options.MinimumPathLength && 
				(options.MaximumPathLength < 0 || path.Length <= options.MaximumPathLength)))
			{
				yield return path;
			}
		}

		/// <summary>
		/// Gets the paths with lengths.
		/// </summary>
		/// <param name="options">The options.</param>
		public static IEnumerable<PathInfo> GetPathsWithLengths(PathLengthSearchOptions options)
		{
			foreach (var path in GetPaths(options))
			{
				yield return new PathInfo() { Path = path };
			}
		}

		/// <summary>
		/// Gets all of the paths, along with their lengths, as a string.
		/// </summary>
		/// <param name="options">The options.</param>
		public static string GetPathsWithLengthsAsString(PathLengthSearchOptions options)
		{
			var text = new StringBuilder();
			foreach (var path in GetPathsWithLengths(options))
			{
				text.AppendLine(path.ToString());
			}
			return text.ToString();
		}
	}
}
