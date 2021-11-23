# Changelog

## vNext

Fixes:

- Fix grammar in message box message (thanks [@tech189](https://github.com/tech189)! [PR #71](https://github.com/deadlydog/PathLengthChecker/pull/71)).

## v1.11.7 - August 26, 2021

Fixes:

- Update Search Pattern tooltip to mention the `*` is a wildcard character.

## v1.11.2 - February 21, 2021

Fixes:

- Fix bug where Starting Directory Replacement wouldn't take effect when also using URL Encoding.

## v1.11.0 - February 20, 2021

Features:

- Add new URL Encode Paths option for the both the GUI and command line app.
- Reset Options button now also clears out grid sorting, since there's no native way to do with using the mouse or keyboard.
- Update UI to group Search Options and Replacement Options separately.

## v1.10.0 - February 20, 2021

Features:

- Preserve UI grid column sorting between searches and sessions.

## v1.9.0 - December 26, 2020

Features:

- Copy to clipboard functions copy paths in the same order they appear in the UI grid.

## v1.8.9 - December 16, 2020

Fixes:

- Remove trailing newlines when copying text to clipboard.
- Follow proper CSV conventions when copying text to clipboard as CSV.
  - Put each item on a newline.
  - Enclose each item in quotes.
  - Include column headers.

## v1.8.5 - December 16, 2020

Fixes:

- Perform retries on copying to clipboard to prevent race condition error.

## v1.8.2 - November 25, 2020

Fixes:

- Prevent race condition that causes copying to the clipboard to crash the application.

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
