using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PathLengthChecker
{
	class Program
	{
		/// <summary>
		/// Application entry point.
		/// </summary>
		/// <param name="args">The command line arguments.</param>
		static void Main(string[] args)
		{
			try
			{
				// Fill the search options from the provided command line arguments.
				var searchOptions = ArgumentParser.ParseArgs(args);

				if (string.Equals(searchOptions.OutputType, "MinLength", StringComparison.InvariantCultureIgnoreCase) ||
					string.Equals(searchOptions.OutputType, "MaxLength", StringComparison.InvariantCultureIgnoreCase))
				{
					// Do the search.
					var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, System.Threading.CancellationToken.None);

					// Output the desired information (Min Path, Max Path, or All Paths).
					if (string.Equals(searchOptions.OutputType, "MinLength", StringComparison.InvariantCultureIgnoreCase))
						Console.WriteLine(paths.Min(p => p.Length));
					else if (string.Equals(searchOptions.OutputType, "MaxLength", StringComparison.InvariantCultureIgnoreCase))
						Console.WriteLine(paths.Max(p => p.Length));
				}
				else
				{
					var paths = PathLengthChecker.GetPathsWithLengthsAsString(searchOptions, System.Threading.CancellationToken.None);
					Console.WriteLine(paths);
				}
			}
			catch (Exception ex)
			{
				// Write any errors that occurred.
				Console.Error.WriteLine(ex);
				Console.Error.WriteLine();
				Console.Error.WriteLine(ArgumentParser.ArgumentUsage);
			}
			finally
			{
				// If we're debugging this through visual studio, wait for input before closing so we can see the output.
				if (Debugger.IsAttached)
					Console.ReadKey();
			}
		}


		
	}
}
