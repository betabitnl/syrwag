# Library with functions.

function Remove-Folder($folderName)
{
	Write-Verbose "Removing folder '$folderName'."
	if(Test-Path $folderName)
	{
		Remove-Item -Recurse -Force -Path $folderName -ErrorAction SilentlyContinue
		if(Test-Path $folderName)
		{
			Write-Warning "Folder '$folderName' not (completely) removed!"
		}
	}
}

function New-Folder($folderName)
{
	Write-Verbose "Creating folder '$folderName'."
	if(-not (Test-Path $folderName))
	{
		mkdir -Path $folderName | Out-Null
	}
}

# Library with functions around tools packages from NuGet.

$projectRoot = Resolve-Path (Join-path $PSScriptRoot  "..") 
$toolsRoot = Join-path $projectRoot "Tools"
$nugetRoot = Join-Path $toolsRoot "NuGet"
$nugetExe = Join-Path $nugetRoot "NuGet.exe"

function Clear-Tools()
{
	Remove-Folder $toolsRoot
}

function Clear-NuGetExe()
{
	Remove-Folder $nugetRoot
}

function Get-NuGetExe()
{
	$nugetCommand = Get-Command nuget.exe -ErrorAction SilentlyContinue
	if($nugetCommand)
	{
		$script:nugetExe = $nugetCommand.Path
		Write-Verbose "Found NuGet version $($nugetCommand.Version) in path '$nugetExe'."
	}
	if(-not (Test-Path $nugetExe))
	{
		$nugetUri = 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe'
		Write-Verbose "Downloading NuGet latest version from '$nugetUri' to '$nugetExe'."
		New-Folder $toolsRoot
		New-Folder $nugetRoot
		Invoke-WebRequest -Uri $nugetUri -OutFile $nugetExe
	}
	$nugetCommand = Get-Command $nugetExe
}

function Clear-ToolsPackage($packageName)
{
	$packageFolder = Join-Path $toolsRoot $packageName
	Remove-Folder $packageFolder
}

function Get-ToolsPackage($packageName, $version, [Switch] $refresh)
{
	New-Folder $toolsRoot
	$packageFolder = Join-Path $toolsRoot $packageName
	if($refresh.IsPresent)
	{
		Write-Verbose "Refreshing package $packageName with version $version."
		Remove-Folder $packageFolder
	}
	if(-not (Test-Path $packageFolder))
	{
		& $nugetExe install $packageName -OutputDirectory $toolsRoot -ExcludeVersion -Version $version
	}
}

function ConvertTo-Dictionary([hashtable] $HashTable)
{
	$dictionary = New-Object "System.Collections.Generic.Dictionary[[String],[Object]]"

	foreach ($entry in $HashTable.GetEnumerator())
	{
		$dictionary.Add($entry.Key, $entry.Value)
	}

	$dictionary -as [System.Collections.Generic.IDictionary[[String],[Object]]]
}