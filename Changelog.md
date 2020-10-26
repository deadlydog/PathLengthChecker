# Changelog

## v1.8.0 - October 25, 2020

Features:

- Use monospaced font for paths shown in the grid.
- Remember user's search option settings between sessions.
- Added button to reset search options to their default values.
- Remember the window's position and size between sessions.

## v1.7.0 - October 25, 2020

Features:

- `PathLengthCheckerGUI.exe` now supports the same command line arguments as `PathLengthChecker.exe`, allowing the GUI application to be launched externally and begin running immediately.
Special thanks to [@mwanchap](https://github.com/mwanchap) for the PR! 😊

## v1.6.0 - October 6, 2020

Features:

- Drag-and-drop a directory onto the `PathLengthCheckerGUI.exe` to automatically start the application and have it search that directory.
Special thanks to [@mwanchap](https://github.com/mwanchap) for the PR! 😊

## v1.5.0 - September 12, 2020

Features:

- Use a Split Button for all Copy To Clipboard commands.
- Add 2 new buttons to allow copying paths to clipboard as CSV.

## v1.4.0 - August 21, 2020

Features:

- Allow sorting by columns on the GUI's grid.

Fixes:

- Fix date formatting to not mix 12-hour and 24-hour time format.

## v1.3.30 - August 9, 2020

Features:

- Ability to cancel long-running searches.
- Right-click context menu added to allow quickly opening a path's directory.
- Grid now shows row numbers.
- Display time searching started and how long it took to complete.
- Tooltips added and updated.

Fixes:

- Continue searching even if an inaccessible directory is found.

Breaking Changes:

- Minimum required .Net Framework changed from 4.0 to 4.5.2.

## v1.2.0 - May 25, 2018

Features:

- GUI is now resizable.

## v1.1.1 - June 26, 2014

Features:

- Improved search performance.
- Added better error handling.

This is the same release as on [the old CodePlex website](https://archive.codeplex.com/?p=pathlengthchecker).

## v1.0.0 - February 10, 2012

Initial release.
