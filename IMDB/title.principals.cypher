// Files must be downloaded from https://datasets.imdbws.com and decompressed.
// For copyright, terms and conditions on use of the data see http://www.imdb.com/interfaces/.

// Check correctness of a file:
LOAD CSV WITH HEADERS FROM "file:///title.principals.tsv"  AS line FIELDTERMINATOR '\t'
WITH line
return line.ordering, count(line.ordering);

//---###---
USING PERIODIC COMMIT 500 
LOAD CSV WITH HEADERS FROM "file:///title.principals.tsv"  AS line FIELDTERMINATOR '\t'
WITH line
MATCH (t:Title {titleId: line.tconst})
WITH t, line, split(CASE 
                    WHEN line.principalCast  = '\\N' THEN null 
                    WHEN line.principalCast  = '' THEN null 
                    ELSE line.principalCast  
                END, ",") AS principalCast
UNWIND principalCast AS castMember
MATCH (c:Name {nameId: castMember})
WITH t, c
CREATE (c)-[:CAST_OF]->(t);
