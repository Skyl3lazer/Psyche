using RimWorld;
using Verse;

namespace Psyche
{
    public class RitualOutcomeComp_PsycheCounselor : RitualOutcomeComp_Quality
    {
        public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
        {
            return PsycheClarityRitual.CounselorScore(ritual.PawnWithRole("counselor"), ritual.PawnWithRole("seeker"));
        }

        public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
        {
            Pawn counselor = assignments.FirstAssignedPawn("counselor");
            Pawn seeker = assignments.FirstAssignedPawn("seeker");
            if (counselor == null || seeker == null)
            {
                return null;
            }

            float score = PsycheClarityRitual.CounselorScore(counselor, seeker);
            float q = curve.Evaluate(score);
            return new QualityFactor
            {
                label = label,
                count = score.ToString("0.#"),
                qualityChange = q > float.Epsilon
                    ? "OutcomeBonusDesc_QualitySingleOffset".Translate(q.ToStringWithSign("0.#%")).Resolve()
                    : " - ",
                positive = q >= 0f,
                quality = q,
                priority = 0f,
            };
        }
    }
}
