using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SimpleFixes {
    public partial class NoMessySpawns {
        /// <summary>
        /// Encapsulates a potential spawn displacement discriminator
        /// </summary>
        /// <remarks>Will always return <em>no</em> objects/fail for priority spawns. <see cref="Filter"/>/<see cref="Prioritize"/>)</remarks>
        internal class SuppressedEntry {
            private readonly DisplaceSuppressionDiscriminatorPredicate _discriminator;

            public SuppressedEntry(DisplaceSuppressionDiscriminatorPredicate discriminator) {
                _discriminator = discriminator;
            }

            public bool ShouldDisplace(IntVec3 cell, Map map, bool respawningAfterLoad) => _discriminator(cell, map, respawningAfterLoad);

            public virtual IEnumerable<Thing> Filter(List<Thing> things) => Enumerable.Empty<Thing>();

            public virtual bool Prioritize(Thing t, Map m) => false;
        }

        /// <summary>
        /// <para>Encapsulates a potential spawn displacement discriminator with priority spawning.</para>
        /// <para><see cref="Filter"/> will return all elements of type <typeparamref name="T"/> as potential candidates to be checked via <see cref="Prioritize"/>.</para>
        /// </summary>
        private class SuppressedEntry<T> : SuppressedEntry where T : Thing {
            private readonly SpawnPrioritizationPredicate<T> _prioritization;

            public SuppressedEntry(DisplaceSuppressionDiscriminatorPredicate discriminator, SpawnPrioritizationPredicate<T> prioritization) : base(discriminator) {
                _prioritization = prioritization;
            }

            public override IEnumerable<Thing> Filter(List<Thing> things)  => things.OfType<T>().Cast<Thing>();

            public override bool Prioritize(Thing t, Map m) => _prioritization((T) t, m);
        }
    }
}