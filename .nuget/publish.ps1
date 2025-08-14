#------------------------------[ HELP INFO ]-------------------------------

<#
.SYNOPSIS
Publishes the release version of the NuGet package to the NuGet Gallery.

.DESCRIPTION
Use this script to publish the latest release build of the library.

.INPUTS
None.

.OUTPUTS
None.

.EXAMPLE
.\publish.ps1
Execute script to publish the release build.
#>

#------------------------[ COMMAND-LINE SWITCHES ]-------------------------

[CmdletBinding(DefaultParameterSetName="default")]
param (
)

#------------------------[ CONFIGURABLE VARIABLES ]------------------------

# TODO: ADD SCRIPT VARIABLES THAT CAN BE OVERWRITTEN VIA THE CONFIG FILE.

#----------------------[ NON-CONFIGURABLE VARIABLES ]----------------------

# TODO: DEFINE SCRIPT VARIABLES THAT CANNOT BE OVERWRITTEN VIA THE CONFIG FILE.

#------------------------------[ CONSTANTS ]-------------------------------

$AssemblyId = "Retry"
$AssemblyName = "DotNetExtras." + $AssemblyId
$SourceBaseDir = Split-Path -Path $PSScriptRoot -Parent
$FolderPath = Join-Path $SourceBaseDir "${AssemblyId}Lib\bin\Release"
$AssemblyFolderPath = Join-Path $FolderPath "net8.0"
$AssemblyPath  = Join-Path $AssemblyFolderPath "${AssemblyName}.dll"
$Source = "https://api.nuget.org/v3/index.json"

#--------------------------------------------------------------------------
# GetAssemblyVersion
#   Returns version of the assembly file.
function GetAssemblyVersion {
    [CmdletBinding()]
    param(
        $assemblyPath
    )

    $version = [System.Reflection.AssemblyName]::GetAssemblyName($assemblyPath).Version
    
    $major = $version.Major
    $minor = $version.Minor
    $build = $version.Build
    
    return "$major.$minor.$build"
}

#---------------------------------[ MAIN ]---------------------------------
# We will trap errors in the try-catch blocks.
$ErrorActionPreference = 'Stop'

# Make sure we have no pending errors.
$Error.Clear()

# MAIN SCRIPT LOGIC
$version = GetAssemblyVersion $AssemblyPath

$Nupkg = Join-Path $FolderPath "$AssemblyName.$version.nupkg"

Write-Host "Publishing $Nupkg to NuGet Gallery..."
nuget push $Nupkg -Source $Source
Write-Host "Done."
# THE END
#--------------------------------------------------------------------------