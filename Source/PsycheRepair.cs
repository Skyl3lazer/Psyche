using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public static class PsycheRepair
    {
        public static int BestResearchedTier()
        {
            if (PsycheDefOf.Psyche_EMDR.IsFinished)
            {
                return 4;
            }

            if (PsycheDefOf.Psyche_CognitiveBehavioralTherapy.IsFinished)
            {
                return 3;
            }

            if (PsycheDefOf.Psyche_Humoralism.IsFinished)
            {
                return 2;
            }

            return 1;
        }

        public static float FloorForTier(int tier, float peak) =>
            Mathf.Max(0f, peak - PsycheTuning.TierRepairReach[tier - 1]);

        public static bool IsReducible(Hediff_PsycheScar scar, int tier) =>
            scar.Severity > FloorForTier(tier, scar.PeakSeverity) + 0.001f;

        public static Hediff_PsycheScar? WorstReducibleScar(Pawn pawn)
        {
            int tier = BestResearchedTier();
            Hediff_PsycheScar? worst = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i] is Hediff_PsycheScar scar && IsReducible(scar, tier) && (worst == null || scar.Severity > worst.Severity))
                {
                    worst = scar;
                }
            }

            return worst;
        }

        public static bool HasReducibleScar(Pawn pawn) => WorstReducibleScar(pawn) != null;

        public static bool ApplyRepairSession(Pawn counselor, Pawn patient, Hediff_PsycheScar scar)
        {
            int tier = BestResearchedTier();
            Thing? medicine = PsycheMedicine.FindBestUsable(counselor, patient, tier);
            float medFloor = PsycheMedicine.QualityFloor(medicine);

            int social = counselor.skills?.GetSkill(SkillDefOf.Social)?.Level ?? 0;
            float skillQuality = PsycheTuning.TierQualityBase[tier - 1]
                + (social * PsycheTuning.RepairQualityPerSocial)
                + Rand.Range(-PsycheTuning.RepairQualityRandomSpread, PsycheTuning.RepairQualityRandomSpread);
            float quality = Mathf.Clamp01(Mathf.Max(medFloor, skillQuality));

            bool cured = ApplyQuality(patient, scar, quality, tier);

            if (medicine != null)
            {
                PsycheMedicine.ConsumeOne(medicine);
            }

            return cured;
        }

        public static bool ApplyQuality(Pawn patient, Hediff_PsycheScar scar, float quality, int tier)
        {
            float magnitude = PsycheTuning.TierRepairMagnitude[tier - 1];
            float zero = PsycheTuning.RepairZeroPoint;

            if (quality > zero)
            {
                float floor = FloorForTier(tier, scar.PeakSeverity);
                if (scar.Severity > floor)
                {
                    float progress = magnitude * ((quality - zero) / (1f - zero));
                    scar.Severity = Mathf.Max(floor, scar.Severity - progress);
                }
            }
            else
            {
                float backfire = magnitude * ((zero - quality) / zero) * PsycheTuning.RepairBackfireScale;
                scar.Severity += backfire;
                scar.NotePeak();
            }

            bool qualifiedForClarity = scar.PeakSeverity >= PsycheTuning.ClarityWindowDeepThreshold;
            bool cured = scar.Severity <= 0f;
            if (cured)
            {
                float peak = scar.PeakSeverity;
                PsycheTraitMarkExtension? traitMark = scar.def.GetModExtension<PsycheTraitMarkExtension>();
                patient.health.RemoveHediff(scar);
                if (traitMark != null)
                {
                    PsycheTraitMarks.RemoveTraitFor(patient, traitMark);
                }
                else if (qualifiedForClarity)
                {
                    PsycheClarityWindows.Open(patient, peak);
                }
            }

            return cured;
        }
    }
}
