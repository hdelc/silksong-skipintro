using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace SkipIntro;

public static class SkipIntroPatcher
{
    public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

    public static void Patch(AssemblyDefinition assembly)
    {
        var startManager = assembly.MainModule.GetType("StartManager");
        // Find generated nested class for StartManager#Start IEnumerator method.
        var startMethodClass = startManager.NestedTypes.First((td) => td.Name.Contains("<Start>"));
        var moveNextMethod = startMethodClass.FindMethod("MoveNext");
        var showIntroSequence = startMethodClass.Fields.First((fd) => fd.Name.Contains("<showIntroSequence>"));
        var il = new ILContext(moveNextMethod);
        ILCursor cursor = new ILCursor(il);
        // Go to `showIntroSequence = true`
        if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdcI4(1),
                x => x.MatchStfld(showIntroSequence)
            ))
        {
            // Change to `showIntroSequence = false`
            cursor.Index += 1;
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_I4_0);
        }
    }
}