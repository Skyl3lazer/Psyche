using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Psyche
{
    public class RitualOutcomeEffectWorker_PsycheClarity : RitualOutcomeEffectWorker_FromQuality
    {
        public RitualOutcomeEffectWorker_PsycheClarity()
        {
        }

        public RitualOutcomeEffectWorker_PsycheClarity(RitualOutcomeEffectDef def)
            : base(def)
        {
        }

        protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
        {
            extraOutcomeDesc = null;

            Pawn seeker = jobRitual.assignments?.FirstAssignedPawn("seeker");
            if (seeker == null)
            {
                return;
            }

            Thought_ClarityWindow? window = PsycheClarityWindows.WorstWindow(seeker);
            if (window == null)
            {
                return;
            }

            float quality = GetQuality(jobRitual, 1f);
            bool success = PsycheClarityWindows.ResolveAttempt(seeker, window, quality);
            extraOutcomeDesc = success
                ? "Psyche_ClarityFormed".Translate(seeker.LabelShort, seeker)
                : "Psyche_ClarityFailed".Translate(seeker.LabelShort, seeker);
            letterLookTargets = seeker;
        }
    }
}
