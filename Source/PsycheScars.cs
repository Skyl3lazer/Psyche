using UnityEngine;
using Verse;

namespace Psyche
{
    public static class PsycheScars
    {
        public static void TryForm(Pawn pawn, float magnitude, float mitigation)
        {
            float chance = Mathf.Clamp01(magnitude / PsycheTuning.ScarChanceDivisor) * (1f - (mitigation * PsycheTuning.MitigationStrength));
            if (!Rand.Chance(chance))
            {
                return;
            }

            float size = magnitude * PsycheTuning.ScarScale;
            if (mitigation >= PsycheTuning.StrongMitigationThreshold && Rand.Chance(PsycheTuning.ScarShrinkChance))
            {
                size *= PsycheTuning.ScarShrinkFactor;
            }

            BodyPartRecord? brain = pawn.health.hediffSet.GetBrain();
            if (brain == null)
            {
                return;
            }

            Hediff scar = HediffMaker.MakeHediff(PsycheDefOf.Psyche_Scar, pawn, brain);
            scar.Severity = size;
            pawn.health.AddHediff(scar, brain);
            pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }
    }
}
