using UnityEngine;
using Verse;

namespace Psyche
{
    public static class PsycheWounds
    {
        public static void Apply(Pawn pawn, float magnitude, float sourceDurationDays)
        {
            BodyPartRecord? brain = pawn.health.hediffSet.GetBrain();
            if (brain == null)
            {
                return;
            }

            Hediff hediff = HediffMaker.MakeHediff(PsycheDefOf.Psyche_Wound, pawn, brain);
            hediff.Severity = magnitude;

            HediffComp_PsycheWound? comp = hediff.TryGetComp<HediffComp_PsycheWound>();
            if (comp != null)
            {
                comp.initialMagnitude = magnitude;
                comp.healTicks = Mathf.Max(PsycheTuning.MinHealTicks, Mathf.RoundToInt(sourceDurationDays * 60000f * PsycheTuning.HealTimeFactor));
            }

            pawn.health.AddHediff(hediff, brain);
            pawn.needs?.TryGetNeed<Need_Psyche>()?.RecomputeFromHediffs();
        }
    }
}
