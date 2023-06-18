# SQL Generator from Interface Function Names via Dispatch Proxies
## Features
- Generate save<object> functions that generates insert queries from unmarshalled object's values, ignoring the primary key
- Generate delete<object> queries with optionak WHERE clauses, generated from the interface function name
- Generate findAll / find<WHERE> queries with optional IEnumerable<T> return type or just <T>, based on function definition.
- Unmarshall result sets into objects
