using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class Harmony_Explosion
    {
        protected static List<string> BlockedDamageTypes = new List<string>();

        public static void BlockType(string defName)
        {
            BlockedDamageTypes.Add(defName);
        }
        
        private static bool TryBlock(Explosion explosion, IntVec3 position)
        {
            if (explosion?.damType?.defName == null) return false;
            foreach (var type in BlockedDamageTypes)
            {
                if (type == explosion.damType.defName)
                {
                    return explosion.Map.GetComponent<ShieldManager>().Block(
                        explosion.TrueCenter(),
                        Common.ToVector3(position),
                        explosion.damAmount);
                }
            }
            return false;
        }
        
        [HarmonyPatch(typeof(Explosion), "AffectCell")]
        static class Patch_AffectCell
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var patchPhase = 0;
                var shieldTestLabel = il.DefineLabel();
                
                foreach (var instruction in instructions)
                {
                    switch (patchPhase)
                    {
                        // search for Map::InBounds
                        case 0:
                        {
                            if (instruction.opcode == OpCodes.Call 
                                && instruction.operand as MethodInfo == AccessTools.Method(typeof(GenGrid), nameof(GenGrid.InBounds), new []{ typeof(IntVec3), typeof(Map) }))
                            {
                                patchPhase = 1;
                            }
                            break;
                        }
                        
                        // replace the label for map being in bounds to the shield check
                        case 1:
                        {
                            if (instruction.opcode == OpCodes.Brtrue)
                            {
                                instruction.operand = shieldTestLabel;
                                patchPhase = 2;
                            }
                            break;
                        }
                        
                        // search for return after Map::InBounds
                        case 2:
                        {
                            if (instruction.opcode == OpCodes.Ret)
                            {
                                patchPhase = 3;
                            }
                            break;
                        }

                        // insert check
                        case 3:
                        {
                            var continueLabel = il.DefineLabel();
                            instruction.labels.Add(continueLabel);

                            yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = new List<Label>(new[] { shieldTestLabel })};
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_Explosion), nameof(TryBlock)));
                            yield return new CodeInstruction(OpCodes.Brfalse, continueLabel);
                            yield return new CodeInstruction(OpCodes.Ret);

                            patchPhase = -1;
                            break;
                        }
                    }
                    
                    yield return instruction;
                }
            }
        }
    }
}
