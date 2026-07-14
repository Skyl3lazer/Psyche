using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public static class PsycheClarityWindows
    {
        public static string BandLabel(float severity)
        {
            if (severity < 3f)
            {
                return "faint";
            }

            return severity < 6f ? "deep" : "severe";
        }

        public static bool CapabilityResearched => PsycheDefOf.Psyche_EMDR.IsFinished;

        public static void Open(Pawn pawn, float healedSeverity)
        {
            if (!CapabilityResearched)
            {
                return;
            }

            MemoryThoughtHandler? memories = pawn.needs?.mood?.thoughts?.memories;
            if (memories == null)
            {
                return;
            }

            Thought_ClarityWindow window = (Thought_ClarityWindow)ThoughtMaker.MakeThought(PsycheDefOf.Psyche_ClarityWindow);
            window.healedSeverity = healedSeverity;
            memories.TryGainMemory(window);
        }

        public static List<Thought_ClarityWindow> Windows(Pawn pawn)
        {
            List<Thought_ClarityWindow> result = new List<Thought_ClarityWindow>();
            List<Thought_Memory>? memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
            if (memories == null)
            {
                return result;
            }

            for (int i = 0; i < memories.Count; i++)
            {
                if (memories[i] is Thought_ClarityWindow w)
                {
                    result.Add(w);
                }
            }

            return result;
        }

        public static bool HasWindow(Pawn pawn) => Windows(pawn).Count > 0;

        public static Thought_ClarityWindow? WorstWindow(Pawn pawn)
        {
            Thought_ClarityWindow? worst = null;
            List<Thought_ClarityWindow> windows = Windows(pawn);
            for (int i = 0; i < windows.Count; i++)
            {
                if (worst == null || windows[i].healedSeverity > worst.healedSeverity)
                {
                    worst = windows[i];
                }
            }

            return worst;
        }

        public static bool EligibleForAlert(Pawn pawn) =>
            PsycheUtility.IsTracked(pawn) && CapabilityResearched && HasWindow(pawn);

        public static int GlitterAvailable(Pawn pawn)
        {
            if (pawn.Map == null)
            {
                return 0;
            }

            int count = 0;
            List<Thing> meds = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.MedicineUltratech);
            for (int i = 0; i < meds.Count; i++)
            {
                Thing med = meds[i];
                if (!med.IsForbidden(pawn) && pawn.CanReach(med, Verse.AI.PathEndMode.ClosestTouch, Danger.Deadly))
                {
                    count += med.stackCount;
                }
            }

            return count;
        }

        public static bool CanAfford(Pawn pawn) => GlitterAvailable(pawn) >= PsycheTuning.ClarityGlitterCost;

        public static float SoloQuality(Pawn pawn)
        {
            int social = pawn.skills?.GetSkill(SkillDefOf.Social)?.Level ?? 0;
            return Mathf.Clamp01(
                PsycheTuning.ClaritySeekSoloQualityBase
                + (social * PsycheTuning.ClaritySeekSoloQualityPerSocial)
                + Rand.Range(-PsycheTuning.RepairQualityRandomSpread, PsycheTuning.RepairQualityRandomSpread));
        }

        public static bool ResolveAttempt(Pawn pawn, Thought_ClarityWindow window, float quality)
        {
            float chance = Mathf.Clamp01(quality);
            bool success = Rand.Chance(chance);
            if (success)
            {
                ConsumeGlitter(pawn, PsycheTuning.ClarityGlitterCost);
                float size = window.healedSeverity * chance * PsycheTuning.ClarityFromWindowScale;
                PsycheClarities.FormDirect(pawn, size);
            }

            pawn.needs?.mood?.thoughts?.memories?.RemoveMemory(window);
            pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
            return success;
        }

        private static void ConsumeGlitter(Pawn pawn, int count)
        {
            if (pawn.Map == null)
            {
                return;
            }

            int remaining = count;
            List<Thing> meds = new List<Thing>(pawn.Map.listerThings.ThingsOfDef(ThingDefOf.MedicineUltratech));
            for (int i = 0; i < meds.Count && remaining > 0; i++)
            {
                Thing med = meds[i];
                if (med.IsForbidden(pawn) || !pawn.CanReach(med, Verse.AI.PathEndMode.ClosestTouch, Danger.Deadly))
                {
                    continue;
                }

                int take = Mathf.Min(remaining, med.stackCount);
                med.SplitOff(take).Destroy();
                remaining -= take;
            }
        }
    }
}
