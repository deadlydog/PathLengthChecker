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
				PrintUsage();
			}
			finally
			{
				// If we're debugging this through visual studio, wait for input before closing so we can see the output.
				if (Debugger.IsAttached)
					Console.ReadKey();
			}
		}


		/// <summary>
		/// Prints the acceptable command line arguments.
		/// </summary>
		private static void PrintUsage()
		{
			Console.Error.WriteLine("Parameters and example:");
			Console.Error.WriteLine("RootDirectory= | Path to the directory to search through and list the paths of. Required.");
			Console.Error.WriteLine("RootDirectoryReplacement=[null] | Path to replace the Root Directory with in the returned results. Specify 'null' to not replace the Root Directory. Default is null.");
			Console.Error.WriteLine("SearchOption=[TopDirectory|All] | Specifies whether sub-directories should be searched or not. Default is All.");
			Console.Error.WriteLine("TypesToInclude=[OnlyFiles|OnlyDirectories|All] | Specifies what types of paths should be returned in the results; files, directories, or both. Default is All.");
			Console.Error.WriteLine("SearchPattern= | The pattern to match files against. '*' is a wildcard character. Default is '*' to match against everything.");
			Console.Error.WriteLine("MinLength= | An integer indicating the minimum length that a path must contain in order to be returned in the results. Default is -1 to ignore this flag.");
			Console.Error.WriteLine("MaxLength= | An integer indicating the maximum length that a path may have in order to be returned in the results. Default is -1 to ignore this flag.");
			Console.Error.WriteLine("Output=[MinLength|MaxLength|All] | Indicates if you just want the Min/Max path length to be outputted, or if you want all of the paths to be outputted. Default is All.");
			Console.Error.WriteLine();
			Console.Error.WriteLine("Example: PathLengthChecker.exe RootDirectory=\"C:\\MyDir\" TypesToInclude=OnlyFiles SearchPattern=*FindThis* MinLength=25");
		}
	}
}
