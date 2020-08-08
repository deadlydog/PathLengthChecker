using System;

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
}
