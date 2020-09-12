using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PathLengthChecker
{
	/// <summary>
	/// Class used to retrieve file system objects in a given path along with their path lengths.
	/// </summary>
	public static class PathLengthChecker
	{
		/// <summary>
		/// Gets the paths with lengths.
		/// </summary>
		/// <param name="options">The options.</param>
		public static IEnumerable<PathInfo> GetPathsWithLengths(PathLengthSearchOptions options, CancellationToken cancellationToken)
		{
			foreach (var path in RetrievePaths(options, cancellationToken))
			{
				yield return new PathInfo() { Path = path };
			}
		}

		/// <summary>
		/// Gets all of the paths, along with their lengths, as a string.
		/// </summary>
		/// <param name="options">The options.</param>
		public static string GetPathsWithLengthsAsString(PathLengthSearchOptions options, CancellationToken cancellationToken)
		{
			var text = new StringBuilder();
			foreach (var path in GetPathsWithLengths(options, cancellationToken))
			{
				text.AppendLine($"{path.Length}: {path.Path}");
			}
			return text.ToString();
		}

		private static IEnumerable<string> RetrievePaths(PathLengthSearchOptions options, CancellationToken cancellationToken)
		{
			// Make sure valid lengths were supplied
			if (options.MinimumPathLength > options.MaximumPathLength && options.MinimumPathLength >= 0 && options.MaximumPathLength >= 0)
				throw new MinPathLengthGreaterThanMaxPathLengthException();

			// Get the paths.
			var paths = PathRetriever.GetPaths(options, cancellationToken);

			// Filter out paths that don't match the Minimum Path Length
			foreach (var path in paths.Where(path => path.Length >= options.MinimumPathLength &&
				(options.MaximumPathLength < 0 || path.Length <= options.MaximumPathLength)))
			{
				yield return path;
			}
		}
	}
}
