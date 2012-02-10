using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PathLengthChecker
{
	class Program
	{
		// Used to tell what the console app should output.
		private static string _output = string.Empty;

		/// <summary>
		/// Application entry point.
		/// </summary>
		/// <param name="args">The command line arguments.</param>
		static void Main(string[] args)
		{
			try
			{
				// Fill the search options from the provided command line arguments.
				var searchOptions = new PathLengthSearchOptions();
				ParseArgs(args, ref searchOptions);

				if (string.Equals(_output, "MinLength", StringComparison.InvariantCultureIgnoreCase) ||
					string.Equals(_output, "MaxLength", StringComparison.InvariantCultureIgnoreCase))
				{
					// Do the search.
					var paths = PathLengthChecker.GetPathsWithLengths(searchOptions);

					// Output the desired information (Min Path, Max Path, or All Paths).
					if (string.Equals(_output, "MinLength", StringComparison.InvariantCultureIgnoreCase))
						Console.WriteLine(paths.Min(p => p.Length));
					else if (string.Equals(_output, "MaxLength", StringComparison.InvariantCultureIgnoreCase))
						Console.WriteLine(paths.Max(p => p.Length));
				}
				else
				{
					var paths = PathLengthChecker.GetPathsWithLengthsAsString(searchOptions);
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

		private static void ParseArgs(IEnumerable<string> args, ref PathLengthSearchOptions searchOptions)
		{
			foreach (var arg in args)
			{
				// Split the command-line arg on the equals sign.
				var parameter = arg.Split("=".ToCharArray(), 2);
				if (parameter.Count() < 2)
					throw new ArgumentException("All parameters must be of the format 'Parameter=Value'");

				// Assign the Command and Value to temp variables for processing.
				var command = parameter[0];
				var value = parameter[1];

				// Fill in the Search Options based on the Command.
				switch (command)
				{
					default: throw new ArgumentException("Unrecognized command: " + command); break;
					case "RootDirectory": searchOptions.RootDirectory = value; break;
					case "RootDirectoryReplacement": searchOptions.RootDirectoryReplacement = (string.Equals(value, "null", StringComparison.InvariantCultureIgnoreCase)) ? null : value; break;
					case "SearchOption": searchOptions.SearchOption = string.Equals("TopDirectory", value, StringComparison.InvariantCultureIgnoreCase) ? System.IO.SearchOption.TopDirectoryOnly : System.IO.SearchOption.AllDirectories; break;
					case "TypesToInclude":
						FileSystemTypes typesToInclude = FileSystemTypes.All;
						if (string.Equals("OnlyFiles", value, StringComparison.InvariantCultureIgnoreCase))
							typesToInclude = FileSystemTypes.Files;
						else if (string.Equals("OnlyDirectories", value, StringComparison.InvariantCultureIgnoreCase))
							typesToInclude = FileSystemTypes.Directories;
						searchOptions.TypesToGet = typesToInclude;
					break;
					case "SearchPattern": searchOptions.SearchPattern = value; break;
					case "MinLength":
						int minLength = -1;
						if (int.TryParse(value, out minLength))
							searchOptions.MinimumPathLength = minLength; 
					break;
					case "MaxLength":
						int maxLength = -1;
						if (int.TryParse(value, out maxLength))
							searchOptions.MaximumPathLength = maxLength;
					break;
					case "Output": _output = value; break;
				}
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
