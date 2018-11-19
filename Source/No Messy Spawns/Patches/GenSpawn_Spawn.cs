using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Verse;

namespace SimpleFixes.Patches
{

    /// <summary>
    /// infrastructure to suppress item splurge during load
    /// </summary>
    [HarmonyPatch(typeof(GenSpawn), "Spawn", typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool))]
    internal static class GenSpawn_Spawn
    {
        public static FieldInfo LoadedFullThingsField = typeof(Map).GetField("loadedFullThings", BindingFlags.NonPublic | BindingFlags.Static);

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            bool ldargsSeen = false;
            Label l = il.DefineLabel();
            List<CodeInstruction> instrList = instructions.ToList();
            for (int i = 0; i < instrList.Count; i++)
            {
                if (!ldargsSeen && instrList[i].opcode == OpCodes.Ldarg_S && instrList[i].operand.Equals((byte)4) &&
                    instrList[i].labels.Count > 0 /* Mod compat between other mods that do same patch */)
                {
                    Label jmpLabel = instrList[i].labels[0];
                    instrList[i].labels.Clear();
                    CodeInstruction ins = new CodeInstruction(OpCodes.Ldarg_1)
                                          {
                                              labels = new List<Label> {jmpLabel}
                                          };
                    yield return ins;
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, (byte) 5);
                    yield return new CodeInstruction(OpCodes.Call, typeof(NoMessySpawns).GetMethod(nameof(NoMessySpawns.ShouldDisplaceOtherItems), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
                    yield return new CodeInstruction(OpCodes.Brfalse, l);
                    ldargsSeen = true;
                }

                if (i + 2 < instrList.Count && instrList[i + 2].opcode == OpCodes.Callvirt && instrList[i + 2].operand == typeof(Thing).GetProperty("Rotation").GetSetMethod())
                {
                    instrList[i].labels.Add(l);
                }

                yield return instrList[i];
            }
        }
    }
}