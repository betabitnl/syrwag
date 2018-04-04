// Create the fingers and their order.
CREATE 
    (T:Finger {name: "Thumb"}),
    (I:Finger {name: "Index finger"}),
    (M:Finger {name: "Middle finger"}),
    (R:Finger {name: "Ring finger"}),
    (P:Finger {name: "Pinky"})

CREATE (T)-[:NEXT_TO]->(I)
CREATE (I)-[:NEXT_TO]->(T)
CREATE (I)-[:NEXT_TO]->(M)
CREATE (M)-[:NEXT_TO]->(R)
CREATE (M)-[:NEXT_TO]->(I)
CREATE (R)-[:NEXT_TO]->(M)
CREATE (R)-[:NEXT_TO]->(P)
CREATE (P)-[:NEXT_TO]->(R)

// Display the full graph.
match(f:Finger) return f

// Starting from the thump, which fingers can be reached in 5 hops.
match (:Finger {name: "Thumb"})-[:NEXT_TO*5]->(f) return f.name

// Drop the two outer ones.
match (f:Finger) where f.name = "Thumb" or f.name = "Pinky" 
optional match (f)-[r:NEXT_TO]-()
delete f, r

// Work on from the previous result.
match (s:Finger)-[:NEXT_TO*3]->(f) where s.name = "Index finger" or s.name = "Ring finger" 
 return distinct f.name as Finger
