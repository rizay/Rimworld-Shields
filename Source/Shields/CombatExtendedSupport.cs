using System;
using System.Reflection;
using FrontierDevelopments.Core;
using FrontierDevelopments.Shields.Harmony;
using Harmony;
using Verse;

namespace FrontierDevelopments.Shields
{
    public class CombatExtendedSupport
    {
        public const string CombatExtendedSupportAssembly =
            "Integrations/FrontierDevelopments-Shields-ClimateControl.dll";

        public static void Load(HarmonyInstance harmony)
        {
            const string combatExtendedName = "CombatRealism";
//            var combatExtendedVersion = new Version(1, 0, 0, 0);
        
            try
            {
                var combatExtendedAssembly = Assembly.Load(combatExtendedName);
                if (combatExtendedAssembly != null)
                {
//                    var version = new AssemblyName(combatExtendedAssembly.FullName).Version;
//                    if (version == combatExtendedVersion)
//                    {
                        var assembly = AssemblyUtility.FindModAssembly(Mod.ModName, CombatExtendedSupportAssembly);
                        if (assembly != null)
                        {
                            harmony.PatchAll(assembly);
                            Log.Message("Frontier Developments Shields :: enabled Combat Extended support");
                        }
                        else
                        {
                            Log.Warning("Frontier Developments Shields :: unable to load Combat Extended support assembly");
                        }
//                    }
//                    else
//                    {
//                        Log.Warning("Frontier Developments Shields :: Combat Extended " + version + 
//                                    "is loaded and " + combatExtendedVersion + " is required, not enabling support");
//                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning("Frontier Developments Shields :: exception while loading Combat Extended support: " + e.Message);
            }
        }
    }
}