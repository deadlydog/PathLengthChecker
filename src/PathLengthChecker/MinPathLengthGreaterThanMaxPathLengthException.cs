using System;

namespace PathLengthChecker
{
	/// <summary>
	/// Exception thrown when the Minimum Path Length search option is greater than the Maximum Path Length search option.
	/// </summary>
	public class MinPathLengthGreaterThanMaxPathLengthException : ArgumentException
	{
		public MinPathLengthGreaterThanMaxPathLengthException()
			: base("MinimumPathLength can not be greater than the MaximumPathLength.")
		{ }
	}
}
