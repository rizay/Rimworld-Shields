using System;
using System.Reflection;
using CombatExtended;
using FrontierDevelopments.General;
using FrontierDevelopments.Shields.Module.RimworldModule;
using Harmony;
using UnityEngine;
using Verse;

namespace FrontierDevelopments.Shields.Handlers
{
    public class Harmony_ProjectileCE : Harmony_Projectile
    {
        private static readonly bool Enabled = true;
        
        private static readonly PropertyInfo DestinationProperty = typeof(ProjectileCE).GetProperty("Destination", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo OriginField = typeof(ProjectileCE).GetField("origin", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly PropertyInfo FTicksProperty = typeof(ProjectileCE).GetProperty("fTicks", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo StartingTicksToImpactProperty = typeof(ProjectileCE).GetProperty("StartingTicksToImpact", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo LauncherField = typeof(ProjectileCE).GetField("launcher", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo GetHeightAtTicksMethod = typeof(ProjectileCE).GetMethod("GetHeightAtTicks", BindingFlags.NonPublic | BindingFlags.Instance);

        static Harmony_ProjectileCE()
        {
            if (DestinationProperty == null)
            {
                Enabled = false;
                Log.Error("FrontierDevelopments Shields :: ProjectileCE handler reflection error on property ProjectileCE.Destination");
            }
            if (OriginField == null)
            {
                Enabled = false;
                Log.Error("FrontierDevelopments Shields :: ProjectileCE handler reflection error on field ProjectileCE.origin");
            }
            if (FTicksProperty == null)
            {
                Enabled = false;
                Log.Error("FrontierDevelopments Shields :: ProjectileCE handler reflection error on property ProjectileCE.fTicks");
            }
            if (StartingTicksToImpactProperty == null)
            {
                Enabled = false;
                Log.Error("FrontierDevelopments Shields :: ProjectileCE handler reflection error on property ProjectileCE.startingTicksToImpact");
            }
            if (LauncherField == null)
            {
                Enabled = false;
                Log.Error("FrontierDevelopments Shields :: ProjectileCE handler reflection error on property ProjectileCE.launcher");
            }
            if (GetHeightAtTicksMethod == null)
            {
                Enabled = false;
                Log.Error("FrontierDevelopments Shields :: ProjectileCE handler reflection error on method ProjectileCE.GetHeightAtTicks");
            }

            Log.Message("FrontierDevelopments Shields :: ProjectileCE handler " + (Enabled ? "enabled" : "disabled due to errors"));
        }
        
        [HarmonyPatch(typeof(ProjectileCE), "Tick")]
        static class Patch_ProjectileCE_Tick
        {
            static bool Prefix(ProjectileCE __instance)
            {
                if (!Enabled) return true;
                
                var projectile = __instance;
                
                var flightTicks = (float)FTicksProperty.GetValue(projectile, null);
                var startingTicksToImpact = (float)StartingTicksToImpactProperty.GetValue(projectile, null);
            
                var origin = (Vector2)OriginField.GetValue(projectile);
                var destination = (Vector2) DestinationProperty.GetValue(projectile, null);
                var position3 = Common.ToVector3(Vector2.Lerp(origin, destination, flightTicks / startingTicksToImpact), projectile.Height);
                var origin3 = Common.ToVector3(origin);
                var destination3 = Common.ToVector3(destination);

                try
                {
                    var nextTick = flightTicks + 1;
                    var nextHeight = (float) GetHeightAtTicksMethod.Invoke(projectile, new object[] {(int) nextTick});
                    var nextPosition =
                        Common.ToVector3(Vector2.Lerp(origin, destination, nextTick / startingTicksToImpact),
                            nextHeight);

                    var impactPoint = TryBlockProjectileCE(
                        projectile,
                        position3,
                        nextPosition,
                        (int) (Mathf.CeilToInt(startingTicksToImpact) - flightTicks),
                        origin3);
                    
                    if (impactPoint != null)
                    {
                        // TODO simulator mortar fragments instead
                        if(ShouldImpact(projectile)) TryExplode(projectile, impactPoint.Value);
                        projectile.Destroy();
                        return false;
                    }
                }
                catch (InvalidOperationException) {}
                return true;
            }
        }

        private static bool ShouldImpact(ProjectileCE projectile)
        {
            
            if (projectile.def.projectile.flyOverhead) return false;
            var type = projectile.GetType();
            return typeof(ProjectileCE_Explosive).IsAssignableFrom(type);
        }

        private static Vector3? TryBlockProjectileCE(
            ProjectileCE projectile,
            Vector3 currentPosition,
            Vector3 nextPosition,
            int ticksToImpact,
            Vector3 origin)
        {
            return TryBlock(
                projectile,
                currentPosition,
                nextPosition,
                ticksToImpact,
                origin,
                // TODO might be able to calculate the exact path with 3d CE projectiles
                
                projectile.def.projectile.flyOverhead,
                // TODO calculate secondary damage to shield
                projectile.def.projectile.GetDamageAmount(1f));
        }

        private static void TryExplode(ProjectileCE projectile, Vector3 point)
        {
            var launcher = (Thing) LauncherField.GetValue(projectile);
            Log.Message("try explode " + projectile.Label + ", has comp = " + (projectile.GetComp<CompExplosiveCE>() != null) + ", launcher not null = " + (launcher != null));
            projectile.GetComp<CompExplosiveCE>()?.
                Explode(
                    (Thing)LauncherField.GetValue(projectile), 
                    new Vector3(point.x, 0, point.y), 
                    projectile.Map);
        }
    }
}