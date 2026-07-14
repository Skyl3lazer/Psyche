using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Psyche
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
    public static class Patch_PawnGetGizmos
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (Gizmo gizmo in __result)
            {
                yield return gizmo;
            }

            Command_Action? seek = PsycheClaritySeeking.GizmoFor(__instance);
            if (seek != null)
            {
                yield return seek;
            }

            Command_Action? devSeek = PsycheClaritySeeking.DevGizmoFor(__instance);
            if (devSeek != null)
            {
                yield return devSeek;
            }
        }
    }

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

            PsycheTraitMarks.Reconcile(__instance);
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public static class Patch_SpawnSetup
    {
        public static void Postfix(Pawn __instance)
        {
            PsycheTraitMarks.Reconcile(__instance);
        }
    }

    [HarmonyPatch(typeof(TraitMentalStateGiver), nameof(TraitMentalStateGiver.CheckGive))]
    public static class Patch_PyromaniaFireChance
    {
        public static bool Prefix(TraitMentalStateGiver __instance, Pawn pawn, ref bool __result)
        {
            if (__instance.traitDegreeData?.randomMentalState?.defName != "FireStartingSpree")
            {
                return true;
            }

            float factor = PsychePyromania.FireFactor(pawn);
            if (factor < 1f && Rand.Value >= factor)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(TraitSet), nameof(TraitSet.GainTrait))]
    public static class Patch_GainTrait
    {
        private static readonly FieldInfo PawnField = AccessTools.Field(typeof(TraitSet), "pawn");

        public static void Postfix(TraitSet __instance)
        {
            if (PawnField?.GetValue(__instance) is Pawn pawn)
            {
                PsycheTraitMarks.Reconcile(pawn);
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
