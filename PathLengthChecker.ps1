<# PathLengthChecker.ps1
    .SYNOPSIS
        Output the length of all files and folders in the given path.

    .EXAMPLE
        PathLengthChecker -PathToScan "C:\example" -OutputFilePath "C:\temp\PathLengths.txt" -WriteToConsole $false -ShowShortPaths $false
#>
[CmdletBinding()]
param (
    [Parameter()]
    # The path to scan lengths for (sub-directories will be scanned as well).
    [String]$PathToScan = "C:\",
    # The results are written to this file.
    [String]$OutputFilePath = "$Env:temp\PathLengths.txt",
    # Writing to the console will be much slower.
    [String]$WriteToConsole = $true,
    # Set to False if you want to only see long paths > 260
    [Boolean]$ShowShortPaths = $true
)



# Ensure output directory exists
$outputFileDirectory = Split-Path $outputFilePath -Parent
if (!(Test-Path $outputFileDirectory)) { New-Item $outputFileDirectory -ItemType Directory }

# Open a new file stream (nice and fast) and write all the paths and their lengths to it.
$stream = New-Object System.IO.StreamWriter($outputFilePath, $false)
Get-ChildItem -Path $pathToScan -Recurse -Force | Select-Object -Property FullName, @{Name="FullNameLength";Expression={($_.FullName.Length)}} | Sort-Object -Property FullNameLength -Descending | ForEach-Object {
    $length = $_.FullNameLength
    if ($showShortPaths -or $length -gt 260){
        $filePath = $_.FullName
        $string = "$length : $filePath"
    
        # Write to the Console.
        if ($writeToConsole) { Write-Host $string }
    
        #Write to the file.
        $stream.WriteLine($string)
    }
}

$stream.Close()