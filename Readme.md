# No Messy Spawns

[![RimWorld 1.0](https://img.shields.io/badge/RimWorld-1.0-green.svg?style=popout-square)](http://rimworldgame.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-lightgrey.svg?style=popout-square)](https://github.com/DoctorVanGogh/NoMessySpawns/blob/master/LICENSE)

A shared Rimworld assembly to help eliminate code reuse while suppressing item spawn displacements (which became a thing with B19).

## Usage

`NoMessySpawns.Instance` offers three methods:

```csharp
/// <summary>
/// Adds a potential spawn displacement predicate. 
/// </summary>
/// <param name="discriminator">
/// A spawn displacement predicate. It should return <see langword = "false" /> if no displacement should
/// occur for the set of <c>position</c>/<c>map</c>/<c>respawningAfterLoad</c> arguments.
/// <seealso cref="DisplaceSuppressionDiscriminatorPredicate"/>
/// </param>
/// <param name="executionPriority">
/// Controls execution order of registered predicates. Lower values execute first.
/// </param>
/// <return>
/// A token which can optionally be used to remove this displacement suppression if no longer needed
/// <seealso cref="Remove"/>
/// </return>
public object Add(DisplaceSuppressionDiscriminatorPredicate discriminator,
                  int executionPriority = 0)

/// <summary>
/// Adds a potential spawn displacement predicate.
/// </summary>
/// <param name="discriminator">
/// A spawn displacement predicate. It should return <see langword = "false" /> if no displacement should
/// occur for the set of <c>position</c>/<c>map</c>/<c>respawningAfterLoad</c> arguments.
/// <seealso cref="DisplaceSuppressionDiscriminatorPredicate"/>
/// </param>
/// <param name="prioritySpawnCallback">
/// Allows prioritized spawning of things after map load (if those things need to already exist to allow
/// <paramref name="discriminator"/> to make a valid decision.
/// <typeparamref name="T"/> must derive from <see cref="Thing"/>. Priority spawning will happen for all
/// things which are of type <typeparamref name="T"/> for which <paramref name="prioritySpawnCallback"/>
/// returns <see langword="true" /> for the set of arguments <c>thing</c> and <c>map</c>.
/// <seealso cref="SpawnPrioritizationPredicate{T}"/>
/// </param>
/// <param name="executionPriority">
/// Controls execution order of registered predicates. Lower values execute first.
/// </param>
/// <return>
/// A token which can optionally be used to remove this displacement suppression if no longer needed
/// <seealso cref="Remove"/>
/// </return>
public object Add<T>(DisplaceSuppressionDiscriminatorPredicate discriminator,
                     SpawnPrioritizationPredicate<T> prioritySpawnCallback,
                     int executionPriority = 0) where T : Thing

/// <summary>
/// Allows removing a previously registered spawn displacement suppression handler
/// </summary>
/// <param name="token">Previous value from a call to <see cref="Add"/> or <see cref="Add{T}"/>.</param>
/// <returns><see langword="true" /> if a previous registration had been made for <paramref name="token"/>
/// and was successfully removed; <see langword="false"/> otherwise.</returns>
public bool Remove(object token)
```

## Example

- Reference (and embed in your distriubution) Assembly `0NoMessySpawns.dll`
  (Also requires [Harmony](https://github.com/pardeike/Harmony))
- Add code for _your_ displacement supression to your mod's initializer:

```csharp
    partial class ExtendedStorageMod : Mod {
        public ExtendedStorageMod(ModContentPack content) : base(content)
        {
            try {
                ...
                NoMessySpawns.Instance.Add(
                    (position, map, respawningAfterLoad) => !respawningAfterLoad
                                                            || map?.thingGrid
                                                                   .ThingsListAtFast(position)
                                                                   .OfType<Building_ExtendedStorage>()
                                                                   .Any() != true,
                    (Building_ExtendedStorage b, Map m) => true);

            } catch (Exception ex) {
                Log.Error("Extended Storage :: Caught exception: " + ex);
            }
        }
    }
```
