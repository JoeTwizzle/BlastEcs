# General Overview

## Type system


### 1. EcsHandle
* Stored as ulong
* Two kinds
    * Entity EcsHandle
    * Pair EcsHandle
    * Differentiated by 8-bit flags
    * Bits 57-64: Flags -> Byte
        * Flags Bit 1: IsPair 
        * Flags Bit 2: IsTagRelation
* Entity EcsHandle
    | Bits      | Usage         | Data type |
    |:--------: | --------      | :--------:|
    | 0-16      | Generation    | Short     |
    | 16-24     | Unused        | Byte      |
    | 24-32     | WorldId       | Byte      | 
    | 32-56     | Id            | UInt      | 
    | 55-64     | Flags         | Byte      | 
* Pair EcsHandle
    | Bits      | Usage         | Data type |
    |:--------: | --------      | :--------:|
    | 0-24      | Target        | UInt      |
    | 24-32     | WorldId       | Byte      | 
    | 32-56     | Id            | UInt      | 
    | 56-64     | Flags         | Byte      | 

### 2. Archetypes
Archetypes store entites with the same set of components.
Archetypes map entities to their index in the Table which stores component data.

### 3. Tables
Stores component data in a per component dense array.
Each entity is assigned an index in each array of components.

### 4. Registry
An identifier is created **for every type** known to the ECS. <br/>
**If a struct is not a tag**, then a component of type **EcsComponent** is added to the identifier. <br/>
The presence of this component indicates, that the **type is associated with a struct storing data**.

#### When new EcsHandles are created:

When an unregistered struct is **added to, or removed from** an Entity. <br/>
When an unregistered struct is **accessed from, or checked on** an Entity. <br/>
When an unregistered Pair is **added to, or removed from** an Entity. <br/>
**For each unregistered constituent type** in a Pair.<br/>

**FIXME: Should removing not create a new handle?**

<img src="BlastEcs layout.png"/>

**Diagram illustrating the different conceptual kinds of EcsHandle types and their properties.**

## Interacting with entities

EcsHandles can have any number of EcsHandles attached. <br/>
EcsHandles can have intrinsic data associated, if they do they are Components, else they are Tags.<br/>
Entity is another word for an EcsHandle with no intrinsic data associated. <br/>

EcsHandles can be attached to an existing entity with the ``Add<T>()`` function.<br/>
EcsHandles can be removed from an existing entity with the ``Remove<T>()`` function.<br/>

If the type ``T`` implements the ``ITag`` interface, then the added EcsHandle is considered a Tag.<br/>

EcsHandles consisting of Pair EcsHandles can be attached to an existing entity with the following functions.
```cs
AddPair<TKind, TTarget>()
AddRelation<TKind>(EcsHandle Target)
AddRelation(EcsHandle Kind, EcsHandle Target)
```
EcsHandles consisting of Pair EcsHandles can be removed from an existing entity with the following functions.
```cs
RemovePair<TKind, TTarget>()
RemoveRelation<TKind>(EcsHandle Target)
RemoveRelation(EcsHandle Kind, EcsHandle Target)
```
If the type ``TKind`` or ``TTarget`` implements the ``ITagRelation`` interface, then a Pair containing this EcsHandle is considered a Tag.<br/>

If the EcsHandles ``Kind`` or ``Target`` have intrinsic data with the ``ITagRelation`` interface, then a Pair containing this EcsHandle is considered a Tag.<br/>

## Lifetimes

An EcsHandle is considered 'Alive' after its creation using ``CreateEntity(...)`` or ``CreatePair(...)``.

An EcsHandle is considered 'Dead' after its destruction using ``DestroyEntity(...)``.

If an EcsHandle ``B`` is attached to another EcsHandle ``A``, either by itself, as a Tag or Component, or as part of a Pair, then the EcsHandle ``B`` is Removed from EcsHandle ``A`` upon EcsHandle ``B`` being destroyed.

**FIXME: Allow custom behaviour, such as destroying any entities with the EcsHandle, or Replacing it, etc..**

## Constraints (WIP)

A constraint may be applied to a specific EcsHandle ``A``. 

Ideas for constraints: <br/>

When to apply the constraint:<br/>
``OnCreate``<br/>
``OnAdd``<br/>
``OnRemove``<br/>
``OnDestroy``<br/>

Action to perform: <br/>
``AddComponent(EcsHandle)``<br/>
``RemoveComponent(EcsHandle)``<br/>
``RequireComponent(EcsHandle)``<br/>