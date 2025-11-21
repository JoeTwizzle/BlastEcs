# TODO

*  ## Filters
	* Basic filters 
		* [ ] Finding archetypes matching components and tags
		* [ ] Matching of relation (kind&target)
		* [ ] Matching of relation with Any targets
		* [ ] Matching of relation with Any kind
		* [ ] Allow for excluding specific components
		* [ ] Allow for excluding specific relations (kind&target)
	* Multi source filters
		* [ ] Change filter to operate on sources
		* [ ] Allow matching conditions on different sources
		* [ ] Allow queries to return entities from different archetypes at same time
	* Traversing 
		* [ ] Find entities that are targeted by a relation both up and down
		* [ ] Add non-recursive relationships
		* [ ] Add Component replace relationships (example ChildOf(Target))
*  ## Queries
	* [ ] functions for a filter

* ## Examples
```
// Basic filters
void Test()
{
	var filter = world.Filter<Position, Velocity>()
	.With<ControlledBy>(player1)
	.Without<Paused>()
	.Build();
}

void Test()
{
	var filter = world.Filter<Position, Velocity>()
	.With<ControlledBy>(player1)
	.Without<Paused>()
	.Build();
}

void Test()
{
	var filter = world.Filter<Position, Velocity>()
	.WithPair<HomeIn, Nexus3>()
	.With<ControlledBy>(player1)
	.Without<Paused>()
	.Build();
}
```
