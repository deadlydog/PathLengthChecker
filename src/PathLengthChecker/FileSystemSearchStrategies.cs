namespace PathLengthChecker
{
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
}
