// Files must be downloaded from https://datasets.imdbws.com and decompressed.
// For copyright, terms and conditions on use of the data see http://www.imdb.com/interfaces/.

// Some queries:
match (g:Genre)<--(t) return collect(distinct(g.name))
match (g:Genre)<--(t) return g, count(g) as total order by total desc
match (g:Genre)<--(t) return count(g)
match (t:Movie) where t.title =~ 'Harry Potter.*' return t order by t.startYear
match (t:Movie) where t.title contains("Harry Potter") return t