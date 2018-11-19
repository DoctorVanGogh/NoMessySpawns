using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Verse;

namespace SimpleFixes
{
    public partial class NoMessySpawns {

        public static NoMessySpawns Instance { get; } = new NoMessySpawns();

        internal SortedList<int, Collection<SuppressedEntry>> _elements = new SortedList<Int32, Collection<SuppressedEntry>>();

        private IDictionary<object, StoredElement> _map = new Dictionary<object, StoredElement>();


        struct StoredElement {
            public int Priority;
            public SuppressedEntry Handler;
        }


        private NoMessySpawns() {

        }


        internal static bool ShouldDisplaceOtherItems(IntVec3 cell, Map map, bool respawningAfterLoad) {
            return !Instance._elements.Any(kvp => kvp.Value.Any(e => !e.ShouldDisplace(cell, map, respawningAfterLoad)));
        }

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
        public object Add(DisplaceSuppressionDiscriminatorPredicate discriminator, int executionPriority = 0) {
            return Add(new SuppressedEntry(discriminator), executionPriority);
        }

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
        public object Add<T>(DisplaceSuppressionDiscriminatorPredicate discriminator, SpawnPrioritizationPredicate<T> prioritySpawnCallback, int executionPriority = 0) where T : Thing {
            return Add(new SuppressedEntry<T>(discriminator, prioritySpawnCallback), executionPriority);
        }

        private object Add(SuppressedEntry e, int priority) {
            var key = new { };

            if (!_elements.TryGetValue(priority, out var bag)) {
                _elements.Add(priority, bag = new Collection<SuppressedEntry>());
            }

            bag.Add(e);
            _map.Add(key, new StoredElement {Handler = e, Priority = priority});

            return key;
        }

        /// <summary>
        /// Allows removing a previously registered spawn displacement suppression handler
        /// </summary>
        /// <param name="token">Previous value from a call to <see cref="Add"/> or <see cref="Add{T}"/>.</param>
        /// <returns><see langword="true" /> if a previous registration had been made for <paramref name="token"/>
        /// and was successfully removed; <see langword="false"/> otherwise.</returns>
        public bool Remove(object token) {
            if (!_map.TryGetValue(token, out var element)) {
                return false;
            }

            var tmp = _elements[element.Priority];
            var result = tmp.Remove(element.Handler);
            if (tmp.Count == 0)
                _elements.Remove(element.Priority);
            return result;

        }
    }

    public delegate bool SpawnPrioritizationPredicate<in T>(T thing, Map map) where T : Thing;

    public delegate bool DisplaceSuppressionDiscriminatorPredicate(IntVec3 position, Map map, bool respawningAfterLoad);

}