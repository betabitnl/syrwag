#Requires -Version 6

# This script downloads the IMDB dataset.
# For copyright, terms and conditions on use of the data see http://www.imdb.com/interfaces/.

[cmdletbinding()]
Param()

Set-Location $PSScriptRoot

$datasets = 'name.basics.tsv.gz', 'title.akas.tsv.gz', 'title.basics.tsv.gz', 'title.crew.tsv.gz', 'title.episode.tsv.gz', 'title.principals.tsv.gz', 'title.ratings.tsv.gz'

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

function Expand-GzipFile
{
    Param(
        $infile,
        $outfile = ($infile -replace '\.gz$','')
        )

    $input = New-Object System.IO.FileStream $inFile, Open, Read, Read
    $output = New-Object System.IO.FileStream $outFile, Create, Write, None
    $gzipStream = New-Object System.IO.Compression.GzipStream $input, ([IO.Compression.CompressionMode]::Decompress)

    $buffer = New-Object byte[](1024)
    while($true)
    {
        $read = $gzipstream.Read($buffer, 0, 1024)
        if ($read -le 0){break}
        $output.Write($buffer, 0, $read)
    }

    $gzipStream.Close()
    $output.Close()
    $input.Close()
}

function Download-TsvFile($fileName)
{
    $url = 'https://datasets.imdbws.com'
    $uri = "$url/$fileName"
    Invoke-WebRequest -Uri "$uri" -OutFile $fileName 
}

function Fix-TsvFormat($inFile, $outFile)
{
    $utf8NoBomEncoding = New-Object System.Text.UTF8Encoding($false)
    $sr = New-Object System.IO.StreamReader($inFile, $utf8NoBomEncoding, $true, 16MB)
    $sw = New-Object System.IO.StreamWriter($outFile, $false, $utf8NoBomEncoding, 16MB)
    try 
    {
        $isFirst = $true
        while($line = $sr.ReadLine())
        {
            if($isFirst)
            {
                $sw.WriteLine("$line")
                $isFirst = $false
            }
            else
            {
                $updatedLine = $line -replace '\\"', '"' -replace '"', '""' -replace "`t", "`"`t`""
                $sw.WriteLine("""$updatedLine""")
            }
        }
        $sw.Flush()
    }
    finally
    {
        $sw.Dispose()
        $sr.Dispose()
    }
}

$datasets | % {
    $set = $_
    Write-Verbose "Downloading $set"
    Download-TsvFile $set
    $file = Get-Item $set
    $outFile = "$(Join-Path $file.Directory $file.BaseName).tmp"
    $tsvFile = (Join-Path $file.Directory $file.BaseName)
    Write-Verbose "Decompressing $set to $outFile"
    Expand-GzipFile $file.FullName $outFile
    Write-Verbose "Fixing $outFile to $tsvFile"
    Fix-TsvFormat $outFile $tsvFile
    rm $outFile
}


# Create a small sub-set of titles to limit the amout of data for testing.
Get-Content .\title.basics.tsv | select -first 1 | Out-File -Encoding utf8NoBOM title.tsv
Get-Content .\title.basics.tsv | select -skip 5200 -first 500 | Out-File -Encoding utf8NoBOM title.tsv -Append
Get-Content .\title.basics.tsv | select -skip 52200 -first 500 | Out-File -Encoding utf8NoBOM title.tsv -Append
Get-Content .\title.basics.tsv | ? { $_ -match "`"movie.*Harry Potter and" }  | Out-File -Encoding utf8NoBOM title.tsv -Append

#$titleIds = (cat .\title.tsv | ConvertFrom-Csv -Delimiter "`t").tcons

#Get-Content .\title.ratings.tsv | select -first 1 | Out-File -Encoding utf8NoBOM ratings.tsv
#Get-Content .\title.ratings.tsv | ConvertFrom-Csv -Delimiter "`t" | ? tconst -in $titleIds

