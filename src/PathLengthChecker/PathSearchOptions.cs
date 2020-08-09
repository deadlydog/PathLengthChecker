using SearchOption = System.IO.SearchOption;

namespace PathLengthChecker
{
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
	}
}
