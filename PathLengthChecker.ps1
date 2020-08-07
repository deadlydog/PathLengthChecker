<# PathLengthChecker.ps1
	.SYNOPSIS
		Output the length of all files and folders in the given path.

	.EXAMPLE
		PathLengthChecker -PathToScan "C:\example" -OutputFilePath "C:\temp\PathLengths.txt" -WriteToConsole $false -ShowShortPaths $false
#>
[CmdletBinding()]
param
(
	[Parameter(HelpMessage = 'The directory to scan path lengths in. Subdirectories will be scanned as well.')]
	[string] $DirectoryPathToScan = "C:\Temp",

	[Parameter(HelpMessage = 'Only paths this length or longer will be included in the results.')]
	[int] $MinimumPathLengthsToShow = 0,

	[Parameter(HelpMessage = 'If the results should be written to the console or not. Can be slow if there are many results.')]
	[bool] $WriteResultsToConsole = $true,

	[Parameter(HelpMessage = 'If the results should be shown in a Grid View or not once the scanning completes.')]
	[bool] $WriteResultsToGridView = $true,

	[Parameter(HelpMessage = 'If the results should be written to a file or not.')]
	[bool] $WriteResultsToFile = $true,

	[Parameter(HelpMessage = 'The file path to write the results to when $WriteResultsToFile is true.')]
	[string] $ResultsFilePath = "C:\Temp\PathLengths.txt"
)

# Ensure output directory exists
[string] $resultsFileDirectoryPath = Split-Path $ResultsFilePath -Parent
if (!(Test-Path $resultsFileDirectoryPath)) { New-Item $resultsFileDirectoryPath -ItemType Directory }

# Open a new file stream (nice and fast) and write all the paths and their lengths to it.
if ($WriteResultsToFile) { $fileStream = New-Object System.IO.StreamWriter($ResultsFilePath, $false) }

$filePathsAndLengths = [System.Collections.ArrayList]::new()

Get-ChildItem -Path $DirectoryPathToScan -Recurse -Force | Select-Object -Property FullName, @{Name = "FullNameLength"; Expression = { ($_.FullName.Length) } } | Sort-Object -Property FullNameLength -Descending | ForEach-Object {
	$filePath = $_.FullName
	$length = $_.FullNameLength

	# If this path is too short, skip it and move onto the next one.
	if ($length -lt $MinimumPathLengthsToShow) { continue }

	[string] $lineOutput = "$length : $filePath"

	if ($WriteResultsToConsole) { Write-Output $lineOutput }

	if ($WriteResultsToFile) { $fileStream.WriteLine($lineOutput) }

	$filePathsAndLengths.Add($_) > $null
}

if ($WriteResultsToFile) { $fileStream.Close() }

if ($WriteResultsToGridView) { $filePathsAndLengths | Out-GridView -Title "Paths under '$DirectoryPathToScan' longer than '$MinimumPathLengthsToShow'." }
