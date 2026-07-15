using Verse;

namespace Psyche
{
    public static class PsycheClarities
    {
        public static void TryForm(Pawn pawn, float boonMagnitude, HediffDef? markDef = null)
        {
            FormDirect(pawn, boonMagnitude * PsycheTuning.ClarityScale, markDef);
        }

        public static void FormDirect(Pawn pawn, float size, HediffDef? markDef = null)
        {
            if (size <= 0f)
            {
                return;
            }

            BodyPartRecord? brain = pawn.health.hediffSet.GetBrain();
            if (brain == null)
            {
                return;
            }

            Hediff clarity = HediffMaker.MakeHediff(markDef ?? PsycheDefOf.Psyche_Clarity, pawn, brain);
            clarity.Severity = size;
            pawn.health.AddHediff(clarity, brain);
            pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }
    }
}
