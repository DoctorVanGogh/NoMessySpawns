namespace SimpleFixes.Patches
{

#if false

    class Class1
    {
        public void foo(List<Thing> things) { 


            Console.WriteLine("pre");
            bar(things);
            Console.WriteLine("post");
        }

        private class NonCompilerGeneratedClosure {
            private readonly Map _map;
            private readonly NoMessySpawns.SuppressedEntry _entry;

            public NonCompilerGeneratedClosure(Map map, NoMessySpawns.SuppressedEntry entry) {
                _map = map;
                _entry = entry;
            }

            public bool Foo(Thing t) => _entry.Prioritize(t, _map);
        }

        public void foo2(List<Thing> things, Map map)
        {

            Console.WriteLine("pre");

            foreach (List<Thing> chunk in Map_FinalizeLoading.ChunkBySpawnPriority(things, map)) {
                things = chunk;

                bar(things);
            }

            Console.WriteLine("post");
        }

        void bar(List<Thing> l) {

        }
    }
#endif

    #region IL content of foo2
#if false
 .method public hidebysig instance void 
    foo2(
      class [mscorlib]System.Collections.Generic.List`1<class ['Assembly-CSharp']Verse.Thing> things, 
      class ['Assembly-CSharp']Verse.Map map
    ) cil managed 
  {
    .maxstack 2
    .locals init (
      [0] class [mscorlib]System.Collections.Generic.IEnumerator`1<class [mscorlib]System.Collections.Generic.List`1<class ['Assembly-CSharp']Verse.Thing>> V_0,
      [1] class [mscorlib]System.Collections.Generic.List`1<class ['Assembly-CSharp']Verse.Thing> chunk
    )

    // [34 9 - 34 10]
    IL_0000: nop          

    // [36 13 - 36 38]
    IL_0001: ldstr        "pre"
    IL_0006: call         void [mscorlib]System.Console::WriteLine(string)
    IL_000b: nop          

    // [38 13 - 38 20]
    IL_000c: nop          

    // [38 43 - 38 96]
    IL_000d: ldarg.1      // things
    IL_000e: ldarg.2      // map
    IL_000f: call         class [mscorlib]System.Collections.Generic.IEnumerable`1<class [mscorlib]System.Collections.Generic.List`1<class ['Assembly-CSharp']Verse.Thing>> SimpleFixes.Patches.Map_FinalizeLoading::ChunkBySpawnPriority(class [mscorlib]System.Collections.Generic.List`1<class ['Assembly-CSharp']Verse.Thing>, class ['Assembly-CSharp']Verse.Map)
    IL_0014: callvirt     instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0/*class [mscorlib]System.Collections.Generic.List`1<class ['Assembly-CSharp']Verse.Thing>*/> class [mscorlib]System.Collections.Generic.IEnumerable`1<class [mscorlib]System.Collections.Generic.List`1<class ['Assembly-CSharp']Verse.Thing>>::GetEnumerator()
    IL_0019: stloc.0      // V_0
    .try
    {

      IL_001a: br.s         IL_0030
      // start of loop, entry point: IL_0030

        // [38 22 - 38 39]
        IL_001c: ldloc.0      // V_0
        IL_001d: callvirt     instance !0/*class [mscorlib]System.Collections.Generic.List`1<class ['Assembly-CSharp']Verse.Thing>*/ class [mscorlib]System.Collections.Generic.IEnumerator`1<class [mscorlib]System.Collections.Generic.List`1<class ['Assembly-CSharp']Verse.Thing>>::get_Current()
        IL_0022: stloc.1      // chunk

        // [38 98 - 38 99]
        IL_0023: nop          

        // [39 17 - 39 32]
        IL_0024: ldloc.1      // chunk
        IL_0025: starg.s      things

        // [41 17 - 41 29]
        IL_0027: ldarg.0      // this
        IL_0028: ldarg.1      // things
        IL_0029: call         instance void SimpleFixes.Patches.Class1::bar(class [mscorlib]System.Collections.Generic.List`1<class ['Assembly-CSharp']Verse.Thing>)
        IL_002e: nop          

        // [42 13 - 42 14]
        IL_002f: nop          

        // [38 40 - 38 42]
        IL_0030: ldloc.0      // V_0
        IL_0031: callvirt     instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
        IL_0036: brtrue.s     IL_001c
      // end of loop
      IL_0038: leave.s      IL_0045
    } // end of .try
    finally
    {

      IL_003a: ldloc.0      // V_0
      IL_003b: brfalse.s    IL_0044
      IL_003d: ldloc.0      // V_0
      IL_003e: callvirt     instance void [mscorlib]System.IDisposable::Dispose()
      IL_0043: nop          

      IL_0044: endfinally   
    } // end of finally

    // [44 13 - 44 39]
    IL_0045: ldstr        "post"
    IL_004a: call         void [mscorlib]System.Console::WriteLine(string)
    IL_004f: nop          

    // [45 9 - 45 10]
    IL_0050: ret          

  } // end of method Class1::foo2
#endif
    #endregion
}
