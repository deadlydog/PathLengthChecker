using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchOption = System.IO.SearchOption;

namespace PathLengthChecker
{
    public class ArgumentParser
    {
        public readonly static string ArgumentUsage =
            "Parameters and example:\n" +
            "RootDirectory= | Path to the directory to search through and list the paths of. Required.\n" +
            "RootDirectoryReplacement=[null] | Path to replace the Root Directory with in the returned results. Specify 'null' to not replace the Root Directory. Default is null.\n" +
            "SearchOption=[TopDirectory|All] | Specifies whether sub-directories should be searched or not. Default is All.\n" +
            "TypesToInclude=[OnlyFiles|OnlyDirectories|All] | Specifies what types of paths should be returned in the results; files, directories, or both. Default is All.\n" +
            "SearchPattern= | The pattern to match files against. '*' is a wildcard character. Default is '*' to match against everything.\n" +
            "MinLength= | An integer indicating the minimum length that a path must contain in order to be returned in the results. Default is -1 to ignore this flag.\n" +
            "MaxLength= | An integer indicating the maximum length that a path may have in order to be returned in the results. Default is -1 to ignore this flag.\n" +
            "Output=[MinLength|MaxLength|All] | Indicates if you just want the Min/Max path length to be outputted, or if you want all of the paths to be outputted. Default is All.\n" +
            "\n" +
            "Example: PathLengthChecker.exe RootDirectory=\"C:\\MyDir\" TypesToInclude=OnlyFiles SearchPattern=*FindThis* MinLength=25";

        /// <summary>
        /// Parses the specified args array into a PathLengthSearchOptions object instance.
        /// </summary>
        public static PathLengthSearchOptions ParseArgs(IEnumerable<string> args)
        {
            var searchOptions = new PathLengthSearchOptions();

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
                    default:
                        throw new ArgumentException("Unrecognized command: " + command);
                    case "RootDirectory":
                        searchOptions.RootDirectory = value;
                        break;
                    case "RootDirectoryReplacement":
                        searchOptions.RootDirectoryReplacement = string.Equals(value, "null", StringComparison.InvariantCultureIgnoreCase) ? null : value;
                        break;
                    case "SearchOption":
                        searchOptions.SearchOption = string.Equals("TopDirectory", value, StringComparison.InvariantCultureIgnoreCase) ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
                        break;
                    case "TypesToInclude":
                        FileSystemTypes typesToInclude = FileSystemTypes.All;
                        if (string.Equals("OnlyFiles", value, StringComparison.InvariantCultureIgnoreCase))
                            typesToInclude = FileSystemTypes.Files;
                        else if (string.Equals("OnlyDirectories", value, StringComparison.InvariantCultureIgnoreCase))
                            typesToInclude = FileSystemTypes.Directories;

                        searchOptions.TypesToGet = typesToInclude;
                        break;
                    case "SearchPattern":
                        searchOptions.SearchPattern = value;
                        break;
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
                    case "Output":
                        searchOptions.OutputType = value;
                        break;
                }
            }

            return searchOptions;
        }

    }
}
