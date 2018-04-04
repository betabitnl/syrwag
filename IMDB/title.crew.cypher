// Files must be downloaded from https://datasets.imdbws.com and decompressed.
// For copyright, terms and conditions on use of the data see http://www.imdb.com/interfaces/.


USING PERIODIC COMMIT 500 
LOAD CSV WITH HEADERS FROM "file:///title.crew.tsv"  AS line FIELDTERMINATOR '\t'
WITH line
MATCH (t:Title {titleId: line.tconst})
WITH t, line, split(CASE 
                    WHEN line.directors  = '\\N' THEN null 
                    WHEN line.directors  = '' THEN null 
                    ELSE line.directors  
                END, ",") AS directors
UNWIND directors AS director
MATCH (d:Name {nameId: director})
WITH t, d, line
MERGE (d)-[:DIRECTED]->(t)
WITH t, line, split(CASE 
                    WHEN line.writers  = '\\N' THEN null 
                    WHEN line.writers  = '' THEN null 
                    ELSE line.writers  
                END, ",") AS writers
UNWIND writers AS writer
MATCH (w:Name {nameId: writer})
WITH t, w
MERGE (w)-[:WROTE]->(t);
