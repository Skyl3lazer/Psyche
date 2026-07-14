using UnityEngine;
using Verse;

namespace Psyche
{
    public class Hediff_PsycheScar : HediffWithComps
    {
        private float peakSeverity;
        private int uiKey;

        public float PeakSeverity => Mathf.Max(peakSeverity, Severity);

        public override string LabelBase => PsycheUtility.BandedLabel(this);

        public override string LabelInBrackets => string.Empty;

        public override int UIGroupKey => uiKey != 0 ? uiKey : (uiKey = PsycheUtility.NextUiGroupKey());

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
        }
    }
}
