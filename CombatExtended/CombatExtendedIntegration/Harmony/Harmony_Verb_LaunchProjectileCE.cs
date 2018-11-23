using CombatExtended;
using FrontierDevelopments.General;
using FrontierDevelopments.Shields;
using FrontierDevelopments.Shields.Harmony;
using Harmony;
using Verse;

namespace FrontierDevelopments.CombatExtended.Harmony
{
    public class Harmony_Verb_LaunchProjectileCE : Harmony_Verb
    {
        private static bool ShieldBlocks(Thing caster, Verb verb, IntVec3 origin, LocalTargetInfo target, ref string report)
        {
            if (!verb.verbProps.requireLineOfSight) return false;
            if (uncheckedTypes.Exists(a => a.IsInstanceOfType(verb))) return false;
            var shielded = caster.Map.GetComponent<ShieldManager>().Shielded(Common.ToVector3(origin), Common.ToVector3(target.Cell), caster.Faction);
            if (shielded) report = "Blocked by shield";
            return shielded;
        }
        
        [HarmonyPatch(typeof(Verb_LaunchProjectileCE), nameof(Verb_LaunchProjectileCE.CanHitTargetFrom), new []{ typeof(IntVec3), typeof(LocalTargetInfo), typeof(string) })]
        static class Patch_CanHitTargetFrom
        {
//            [HarmonyTranspiler]
//            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
//            {
//                // TODO implement
//                return instructions;
//            }

            [HarmonyPostfix]
            static bool Postfix(Verb_LaunchProjectileCE __instance, IntVec3 root, LocalTargetInfo targ, ref string report)
            {
                var result = ShieldBlocks(__instance.Shooter, __instance, root, targ, ref report);
                Log.Message("shield blocks = " + result + ", because " + report);
                return result;
            }
        }

        
    }
}