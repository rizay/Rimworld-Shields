using System.Collections.Generic;
using System.Reflection.Emit;
using CombatExtended;
using FrontierDevelopments.General;
using FrontierDevelopments.Shields;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.CombatExtended.Harmony
{
    public class Harmony_ExplosionCE : Harmony_Explosion
    {
        [HarmonyPatch(typeof(ExplosionCE), "AffectCell")]
        static class Patch_AffectCell
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                // TODO implement
                return instructions;
            }
        }
    }
}