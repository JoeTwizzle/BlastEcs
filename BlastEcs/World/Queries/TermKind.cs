namespace BlastEcs.World.Queries;
//Normal/Relation
//- Match
//Named Relation
//- Match
//- KindSource OR TargetSource
//Indirect Up/Self Traversal
//- Match (treat like normal, if false check match on RelationshipToTraverse up until true or empty) 
//- RelationshipToTraverse
//Named Up traversal
//- RelationshipToTraverse
//- TargetSource
//Indirect Down/Self Traversal
//- Match (treat like normal, if false check match on RelationshipToTraverse down until true or empty) 
//- RelationshipToTraverse
enum TermKind
{
    /// <summary>
    /// Matches a ComponentOrRelation
    /// </summary>
    ComponentOrRelation,
    /// <summary>
    /// Matches a Relation, then assign name to set of indeterminate Kind/Target of match
    /// </summary>
    NamedRelation,



}
