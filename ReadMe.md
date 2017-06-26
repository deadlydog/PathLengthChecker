# Path Length Checker Description

Path Length Checker is a stand-alone app that allows you to specify a root directory, and it gives you back a list of all paths (i.e. files and directories) in that root directory and their lengths. It includes features such as pattern matching and min/max length constraints, as well as the ability to specify a string that should replace the root directory in the results brought back, allowing you to quickly see path lengths if you were to move the files/folders to another location.

Download it from [the Releases page][GitHubReleasesPage].

To run the Path Length Checker using the GUI, just run the PathLengthCheckerGUI.exe. The PathLengthChecker.exe is the command-line alternative to the GUI.

![](docs/Images/PathLengthChecker.png)

There is currently [a bug](https://pathlengthchecker.codeplex.com/workitem/1156) with the Path Length Checker tool where it is not able to process paths in Windows restricted directories (e.g. C:\Documents and Settings).  In the meantime you can use [this PowerShell script](http://blog.danskingdom.com/powershell-script-to-check-path-lengths/) to get those path lengths.


<!-- Links -->
[GitHubReleasesPage]: https://github.com/deadlydog/PathLengthChecker/releases
