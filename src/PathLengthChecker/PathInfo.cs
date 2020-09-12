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
	}
}
