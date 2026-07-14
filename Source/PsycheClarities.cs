using Verse;

namespace Psyche
{
    public static class PsycheClarities
    {
        public static void TryForm(Pawn pawn, float boonMagnitude)
        {
            FormDirect(pawn, boonMagnitude * PsycheTuning.ClarityScale);
        }

        public static void FormDirect(Pawn pawn, float size)
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

            Hediff clarity = HediffMaker.MakeHediff(PsycheDefOf.Psyche_Clarity, pawn, brain);
            clarity.Severity = size;
            pawn.health.AddHediff(clarity, brain);
            pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }
    }
}
