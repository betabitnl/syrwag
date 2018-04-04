// Files must be downloaded from https://datasets.imdbws.com and decompressed.
// For copyright, terms and conditions on use of the data see http://www.imdb.com/interfaces/.


USING PERIODIC COMMIT 500 
LOAD CSV WITH HEADERS FROM "file:///title.ratings.tsv"  AS line FIELDTERMINATOR '\t'
WITH line
MATCH (t:Title {titleId: line.tconst})
WITH t, line
MERGE (r:Rating { average: toFloat(line.averageRating), votes: toInteger(line.numVotes)})
MERGE (t)-[:HAS_RATING]->(r);
