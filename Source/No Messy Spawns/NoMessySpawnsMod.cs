using System;
using System.Reflection;
using Harmony;
using Verse;

namespace SimpleFixes
{
    public class NoMessySpawnsMod : Mod {
        public NoMessySpawnsMod(ModContentPack content) : base(content) {
            try
            {                
                HarmonyInstance instance = HarmonyInstance.Create(@"nomessyspawns.patches");
                instance.PatchAll(Assembly.GetExecutingAssembly());
                Log.Message($"No Messy Spawns {typeof(NoMessySpawnsMod).Assembly.GetName().Version} - Harmony patches successful");
            }
            catch (Exception ex)
            {
                Log.Error("No Messy Spawns :: Caught exception: " + ex);
            }
        }
    }


}
