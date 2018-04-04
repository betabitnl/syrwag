// Files must be downloaded from https://datasets.imdbws.com and decompressed.
// For copyright, terms and conditions on use of the data see http://www.imdb.com/interfaces/.

CREATE INDEX ON :Name(nameId);
//---###---
CREATE INDEX ON :Name(name);
//---###---
CREATE CONSTRAINT ON (name:Name) ASSERT name.nameId IS UNIQUE
//---###---
USING PERIODIC COMMIT 500 
LOAD CSV WITH HEADERS FROM "file:///name.basics.tsv"  AS line FIELDTERMINATOR '\t'
WITH line
CREATE (n:Name { 
        nameId: line.nconst, 
        name: line.primaryName,
        birthYear: toInteger(line.birthYear),
        deathYear: toInteger(line.deathYear)
        })
WITH n, line, split(CASE 
                    WHEN line.primaryProfession  = '\\N' THEN null 
                    WHEN line.primaryProfession  = '' THEN null 
                    ELSE line.primaryProfession  
                END, ",") AS primaryProfessions 
UNWIND primaryProfessions AS primaryProfession
MERGE (p:Profession {name: primaryProfession})
MERGE (n)-[:HAS_PRIMARY_PROFESSION]->(p)
WITH n, p, split(CASE 
                    WHEN line.knownForTitles = '\\N' THEN null 
                    ELSE line.knownForTitles  
                END, ",") AS knownForTitles  
UNWIND knownForTitles AS knownForTitle
MATCH (t:Title {titleId: knownForTitle})
WITH t, p
MERGE (p)-[:KNOWN_FOR]->(t)
