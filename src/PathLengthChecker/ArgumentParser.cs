using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathLengthChecker
{
    public class ArgumentParser
    {
        /// <summary>
        /// Parses the specified args array into a PathLengthSearchOptions object instance.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
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
