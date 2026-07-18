using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public class Hediff_PsycheScar : HediffWithComps
    {
        private float peakSeverity;
        private int uiKey;

        public bool untreatable;

        public float PeakSeverity => Mathf.Max(peakSeverity, Severity);

        public override string LabelBase => PsycheUtility.BandedLabel(this);

        public override string LabelInBrackets => string.Empty;

        public override bool Visible => PsycheUtility.HasPsyche(pawn) && base.Visible;

        public override int UIGroupKey => uiKey != 0 ? uiKey : (uiKey = PsycheUtility.NextUiGroupKey());

        public override string TipStringExtra
        {
            get
            {
                string baseTip = base.TipStringExtra ?? string.Empty;
                string? repair = RepairTip();
                if (repair.NullOrEmpty())
                {
                    return baseTip;
                }

                return baseTip.NullOrEmpty() ? repair : baseTip.TrimEnd('\n') + "\n" + repair;
            }
        }

        private string? RepairTip()
        {
            if (!PsycheUtility.HasPsyche(pawn))
            {
                return null;
            }

            if (untreatable)
            {
                return "Psyche_Tip_Untreatable".Translate();
            }

            int tier = PsycheRepair.BestResearchedTier();
            if (!PsycheRepair.IsReducible(this, tier))
            {
                return "Psyche_Tip_TierFloor".Translate(NextTierLabel(tier));
            }

            float repaired = PeakSeverity > 0f ? Mathf.Clamp01((PeakSeverity - Severity) / PeakSeverity) : 0f;
            string repairedLine = "Psyche_Tip_Repaired".Translate(repaired.ToStringPercent());

            Need_Psyche? need = pawn.needs?.TryGetNeed<Need_Psyche>();
            string eligibility = need == null || need.CanReceiveCounseling
                ? "Psyche_Tip_ReadyForRepair".Translate()
                : "Psyche_Tip_RepairableAgain".Translate(need.TicksUntilCounseling.ToStringTicksToPeriod());

            return repairedLine + "\n" + eligibility;
        }

        private static string NextTierLabel(int tier)
        {
            switch (tier)
            {
                case 1:
                    return PsycheDefOf.Psyche_Humoralism.label;
                case 2:
                    return PsycheDefOf.Psyche_CognitiveBehavioralTherapy.label;
                default:
                    return PsycheDefOf.Psyche_EMDR.label;
            }
        }

        public override bool TryMergeWith(Hediff other) => false;

        public void NotePeak()
        {
            if (Severity > peakSeverity)
            {
                peakSeverity = Severity;
            }
        }

        public override void PostMake()
        {
            base.PostMake();
            NotePeak();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref peakSeverity, "psyche_peakSeverity", 0f);
            Scribe_Values.Look(ref untreatable, "psyche_untreatable", false);
        }
    }
}
