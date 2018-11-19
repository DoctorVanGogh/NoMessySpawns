using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Harmony.ILCopying;
using Verse;

namespace SimpleFixes.Patches {
    /// <summary>
    /// prioritize spawn so genspawn.spawn splurge suppression patch can return a valid discriminator    
    /// </summary>
    /// <seealso cref="NoMessySpawns.ShouldDisplaceOtherItems"/>
    [HarmonyPatch(typeof(Map), nameof(Map.FinalizeLoading))]
    class Map_FinalizeLoading {
        private static MethodInfo miPre = typeof(BackCompatibility).GetMethod(nameof(BackCompatibility.PreCheckSpawnBackCompatibleThingAfterLoading));
        private static MethodInfo miPost = typeof(BackCompatibility).GetMethod(nameof(BackCompatibility.PostCheckSpawnBackCompatibleThingAfterLoading));

        public static IEnumerable<List<Thing>> ChunkBySpawnPriority(List<Thing> things, Map map) {
            List<Thing> remaining = new List<Thing>(things);

            foreach (var priority in NoMessySpawns.Instance._elements) {

                Trace.Log($"Checking priority {priority.Key}");
                foreach (var entry in priority.Value) {
                    var elements = entry.Filter(remaining)
                                        .Where(t => entry.Prioritize(t, map))
                                        .ToList();

                    Trace.Log($"Got {elements.Count} things to spawn");

                    yield return elements;

                    Trace.Log("Done spawning");

                    foreach (var thing in elements) {
                        remaining.Remove(thing);
                        Trace.Log($"Removed from {thing} from list - {remaining.Count} total remaining...");
                    }
                }
            }

            Trace.Log("Falling back to default spawn");

            yield return remaining;
        }



        /// <summary>
        /// For prototype code/desired IL instructions see file Patches/[Prototype]/Map_FinalizeLoading.cs
        /// </summary>
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen) {
            List<CodeInstruction> instructions = new List<CodeInstruction>(instr);

            var idxAnchorPre = instructions.FindIndex(ci => ci.opcode == OpCodes.Call && ci.operand == miPre);
            if (idxAnchorPre == -1) {
                Log.Warning("Could not find Map_FinalizeLoading transpiler start anchor - not patching.");
                return instructions;
            }

            var idxAnchorPost = instructions.FindIndex(ci => ci.opcode == OpCodes.Call && ci.operand == miPost);
            if (idxAnchorPost == -1) {
                Log.Warning("Could not find Map_FinalizeLoading transpiler end anchor - not patching.");
                return instructions;
            }

            // idxAnchorPost -1, since that is an instance method (with a corresponding ldarg)
            SurroundBlock(instructions, idxAnchorPre + 1, idxAnchorPost - 1, ilGen);

            return instructions;

        }

        private static void SurroundBlock(List<CodeInstruction> instructions, int from, int to, ILGenerator ilGen) {
            LocalBuilder locEnumerator = ilGen.DeclareLocal(typeof(IEnumerator<List<Thing>>));

            Label lbl_Loop_Check = ilGen.DefineLabel();
            Label lbl_Loop_Start = ilGen.DefineLabel();
            Label lbl_Using_Conditional_Fail = ilGen.DefineLabel();
            Label lbl_Using_After = ilGen.DefineLabel();
            Label lbl_Block_After = ilGen.DefineLabel();

            // modify code in *reverse* order to preserve indices

            CodeInstruction ciEnd = instructions[to];
            if (ciEnd.labels == null)
                ciEnd.labels = new List<Label>();

            // check for labels jumping *out* of the block to be surrounded - technically we'd need to not just check for jumps to the *beginning* of our post block code, but that is too generic for now ;)
            var jumpTargets = ciEnd.labels.ToArray();

            var jumpOpCodes = new[] {
                                        OpCodes.Leave,
                                        OpCodes.Leave_S,
                                        OpCodes.Br,
                                        OpCodes.Br_S,
                                        OpCodes.Brtrue,
                                        OpCodes.Brtrue_S,
                                        OpCodes.Brfalse,
                                        OpCodes.Brfalse_S
                                    };
            for (int idx = from; idx < to; idx++) {
                var ci = instructions[idx];
                if (jumpOpCodes.Any(oc => ci.opcode == oc) && jumpTargets.Any(lbl => lbl == (Label) ci.operand)) {
                    Trace.Warning($"Patched jump instruction at {idx} to go to our postfix block instead!");
                    ci.operand = lbl_Block_After;
                }
            }

            ciEnd.labels.Add(lbl_Using_After);

            instructions.InsertRange(
                to,
                new[] {
                          new CodeInstruction(OpCodes.Nop) { labels = new List<Label>{ lbl_Block_After}},

                          // "} (of foreach)" & advance/check of "in" in foreach
                          new CodeInstruction(OpCodes.Ldloc, locEnumerator) {labels = new List<Label> {lbl_Loop_Check}},
                          new CodeInstruction(OpCodes.Call, typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext))),
                          new CodeInstruction(OpCodes.Brtrue, lbl_Loop_Start),

                          new CodeInstruction(OpCodes.Leave, lbl_Using_After),

                          new CodeInstruction(OpCodes.Ldloc, locEnumerator) {blocks = new List<ExceptionBlock> {new ExceptionBlock(ExceptionBlockType.BeginFinallyBlock, null)}},
                          new CodeInstruction(OpCodes.Brfalse, lbl_Using_Conditional_Fail),
                          new CodeInstruction(OpCodes.Ldloc, locEnumerator),
                          new CodeInstruction(OpCodes.Callvirt, typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose))),
                          new CodeInstruction(OpCodes.Endfinally) {blocks = new List<ExceptionBlock> {new ExceptionBlock(ExceptionBlockType.EndExceptionBlock, null)}, labels = new List<Label>{ lbl_Using_Conditional_Fail}}

                      });

            instructions.InsertRange(
                from,
                new[] {
                          // foreach ([list2] in ChunkBySpawnPriority([list2], [this])) { 
                          new CodeInstruction(OpCodes.Ldloc_1),
                          new CodeInstruction(OpCodes.Ldarg_0),
                          new CodeInstruction(OpCodes.Call, typeof(Map_FinalizeLoading).GetMethod(nameof(ChunkBySpawnPriority))),
                          new CodeInstruction(OpCodes.Callvirt, typeof(IEnumerable<List<Thing>>).GetMethod(nameof(IEnumerable.GetEnumerator))),
                          new CodeInstruction(OpCodes.Stloc, locEnumerator), 

                          new CodeInstruction(OpCodes.Br, lbl_Loop_Check) { blocks = new List<ExceptionBlock>{ new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock, null)}}, 

                          new CodeInstruction(OpCodes.Ldloc, locEnumerator) { labels = new List<Label>{ lbl_Loop_Start}}, 
                          new CodeInstruction(OpCodes.Callvirt, typeof(IEnumerator<List<Thing>>).GetProperty(nameof(IEnumerator.Current)).GetGetMethod(true)),
                          new CodeInstruction(OpCodes.Stloc_1)
                      });

        }
    }
}
