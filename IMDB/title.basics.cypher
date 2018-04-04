// Files must be downloaded from https://datasets.imdbws.com and decompressed.
// For copyright, terms and conditions on use of the data see http://www.imdb.com/interfaces/.


CREATE CONSTRAINT ON (title:Title) ASSERT title.titleId IS UNIQUE
//---###---
DROP INDEX ON :Title(titleId);
//---###---
CREATE INDEX ON :Title(title);
//---###---

// This example used a short version of title.basics.tsv, as this script needs optimization for 
// large datasets.
USING PERIODIC COMMIT 500 
LOAD CSV WITH HEADERS FROM "file:///title.tsv" AS line FIELDTERMINATOR '\t'
WITH line
MERGE (t:Title { 
        titleId: line.tconst, 
        title: line.primaryTitle,
        originalTitle: line.originalTitle, 
        startYear: toInteger(line.startYear),
        endYear: toInteger(line.endYear),
        runtimeInMinutes: toInteger(line.runtimeMinutes),
        isAdult: CASE 
                    WHEN line.isAdult = 0 THEN false 
                    WHEN line.isAdult = 1 THEN true 
                    ELSE null 
                END
        })
WITH t, line,
CASE WHEN line.titleType = "videoGame" THEN [1] ELSE [] END AS videoGame,
CASE WHEN line.titleType = "movie" THEN [1] ELSE [] END AS movie,
CASE WHEN line.titleType = "tvSeries" THEN [1] ELSE [] END AS tvSeries,
CASE WHEN line.titleType = "tvMiniSeries" THEN [1] ELSE [] END AS tvMiniSeries,
CASE WHEN line.titleType = "short" THEN [1] ELSE [] END AS short,
CASE WHEN line.titleType = "tvSpecial" THEN [1] ELSE [] END AS tvSpecial,
CASE WHEN line.titleType = "video" THEN [1] ELSE [] END AS video,
CASE WHEN line.titleType = "tvShort" THEN [1] ELSE [] END AS tvShort,
CASE WHEN line.titleType = "tvEpisode" THEN [1] ELSE [] END AS tvEpisode,
CASE WHEN line.titleType = "tvMovie" THEN [1] ELSE [] END AS tvMovie
FOREACH (x IN videoGame | SET t:VideoGame)
FOREACH (x IN movie | SET t:Movie)
FOREACH (x IN tvSeries | SET t:TvSeries)
FOREACH (x IN tvMiniSeries | SET t:TvMiniSeries)
FOREACH (x IN short | SET t:Short)
FOREACH (x IN tvSpecial | SET t:TvSpecial)
FOREACH (x IN video | SET t:Video)
FOREACH (x IN tvShort | SET t:TvShort)
FOREACH (x IN tvEpisode | SET t:TvEpisode)
FOREACH (x IN tvMovie | SET t:TvMovie)
WITH t, split(CASE 
                    WHEN line.genres = '\\N' THEN null 
                    ELSE line.genres 
                END, ",") AS genres 
UNWIND genres AS genre
MERGE (g:Genre {name: genre})
MERGE (t)-[:OF_GENRE]->(g);
