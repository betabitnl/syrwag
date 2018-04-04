# Parts of the code based on https://glennsarti.github.io/blog/using-neo4j-dotnet-client-in-ps/

. $PSScriptRoot\lib.ps1

Get-NuGetExe
Get-ToolsPackage Neo4j.Driver 1.5.2
$neo4jRoot = Join-Path $toolsRoot "Neo4j.Driver\lib\netstandard1.3"
Add-Type -Path "$neo4jRoot\Neo4j.Driver.dll"

function Invoke-CypherQuery([string] $Query, [hashtable] $Parameters = $null, [PSCredential] $Credential = $null, $Uri = "bolt://localhost:7687")
{
	try 
	{
		if( $Credential)
		{
			$authToken = [Neo4j.Driver.V1.AuthTokens]::Basic($Credential.UserName, $Credential.GetNetworkCredential().Password)
			$dbDriver = [Neo4j.Driver.V1.GraphDatabase]::Driver($Uri, $authToken)
		}
		else
		{
			$dbDriver = [Neo4j.Driver.V1.GraphDatabase]::Driver($Uri)
		}
		$session = $dbDriver.Session()
		if($Parameters)
		{
			$dictionary = ConvertTo-Dictionary $Parameters
			$result = $session.Run($Query, $dictionary)
		}
		else 
		{
			$result = $session.Run($Query, $dictionary)
		}
		@{ Records = ($result | % { $_ }); Summary = $result.Summary }
	}
	finally 
	{
		$session.Dispose()
		$session = $null
		$dbDriver.Dispose()
		$dbDriver = $null
	}
}

function Invoke-CypherQueryFromFile([string] $Path, [hashtable] $Parameters = $null, [PSCredential] $Credential = $null, $Uri = "bolt://localhost:7687")
{
	(Get-Content $Path -Raw) -split '//---###---' | % { 
		Invoke-CypherQuery -Query $_ -Parameters $Parameters -Credential $Credential -Uri $Uri
	}
}
