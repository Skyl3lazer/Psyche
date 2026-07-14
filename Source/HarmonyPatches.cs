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

    [HarmonyPatch(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new[] { typeof(Thought_Memory), typeof(Pawn) })]
    public static class Patch_TryGainMemory
    {
        public static void Postfix(MemoryThoughtHandler __instance, Thought_Memory newThought)
        {
            if (!(newThought is Thought_Psychlet pt) || !__instance.Memories.Contains(newThought))
            {
                return;
            }

            pt.EnsureCaptured();
            __instance.pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.RemoveMemory))]
    public static class Patch_RemoveMemory
    {
        public static void Postfix(MemoryThoughtHandler __instance, Thought_Memory th)
        {
            if (th is Thought_Psychlet pt && pt.ShouldDiscard)
            {
                pt.RollOnExpiry();
                __instance.pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
            }
        }
    }
}
