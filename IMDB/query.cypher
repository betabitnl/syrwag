// An example query.
match (t:Movie)<-[r:WROTE]-(w)
    where t.title contains($title) 
    return t, r, w
