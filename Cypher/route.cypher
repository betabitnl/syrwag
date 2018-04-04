//  Create the routes
CREATE 
    (A:Place {name: "A"}),
    (B:Place {name: "B"}),
    (C:Place {name: "C"}),
    (D:Place {name: "D"})

CREATE (A)-[:CONNECTS_TO {name: "A->B 1", cost: 1}]->(B)
CREATE (A)-[:CONNECTS_TO {name: "A->B 2", cost: 2}]->(B)
CREATE (A)-[:CONNECTS_TO {name: "A->C 1", cost: 3}]->(C)
CREATE (A)-[:CONNECTS_TO {name: "A->C 2", cost: 15}]->(C)
CREATE (A)-[:CONNECTS_TO {name: "A->D", cost: 10}]->(D)
CREATE (B)-[:CONNECTS_TO {name: "B->D", cost: 5}]->(D)
CREATE (C)-[:CONNECTS_TO {name: "C->D", cost: 2}]->(D)

match (p:Place) return p

// Update the costs of a route. 
MATCH (:Place {name: "A"})-[r:CONNECTS_TO {name: 'A->C 2'}]->(:Place {name: "C"})
SET r.cost = 12
RETURN r

// Remove the costs property from of a route.
MATCH (:Place {name: "A"})-[r:CONNECTS_TO]->(:Place {name: "C"})
REMOVE r.costs

//  Find the cheapest route between A and D.
MATCH p = (a:Place {name: "A"})-[:CONNECTS_TO*]->(n:Place {name: "D"})
RETURN p AS shortestPath, reduce(cost=0, r in relationships(p) | cost + r.cost) AS totalCost
ORDER BY totalCost ASC
LIMIT 1

// Find the shortest path between A and D.
MATCH  (s:Place {name: "A"})-[:CONNECTS_TO*1..4]->(f:Place {name: 'D'}), p = shortestPath((s)-[*]-(f))
RETURN p

// Find the shortest path between B and C.
MATCH  (s:Place {name: "B"})-[:CONNECTS_TO*1..4]->(f:Place {name: "C"}), p = shortestPath((s)-[*]->(f))
RETURN p

// Extend the model to make the routes from D to B and C possible.
MATCH (B:Place {name: "B"})
MATCH (C:Place {name: "C"})
MATCH (D:Place {name: "D"})
CREATE (D)-[:CONNECTS_TO {Cost: 5}]->(B)
CREATE (D)-[:CONNECTS_TO {Cost: 2}]->(C)
