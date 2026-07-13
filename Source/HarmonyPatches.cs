using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Psyche
{
    [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
    public static class Patch_ShouldHaveNeed
    {
        private static readonly FieldInfo PawnField = AccessTools.Field(typeof(Pawn_NeedsTracker), "pawn");

        public static void Postfix(Pawn_NeedsTracker __instance, NeedDef __0, ref bool __result)
        {
            if (!__result || __0 != PsycheDefOf.Psyche_Core || PawnField == null)
            {
                return;
            }

            Pawn? pawn = PawnField.GetValue(__instance) as Pawn;
            if (pawn == null || !PsycheUtility.IsTracked(pawn))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
    public static class Patch_SetFaction
    {
        public static void Postfix(Pawn __instance)
        {
            if (__instance.needs != null && __instance.RaceProps.Humanlike)
            {
                __instance.needs.AddOrRemoveNeedsAsAppropriate();
            }
        }
    }
}
