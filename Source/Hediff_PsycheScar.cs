using UnityEngine;
using Verse;

namespace Psyche
{
    public class Hediff_PsycheScar : HediffWithComps
    {
        private static int nextUiKey;

        private float peakSeverity;
        private int uiKey;

        public float PeakSeverity => Mathf.Max(peakSeverity, Severity);

        public override string LabelBase => PsycheClarityWindows.BandLabel(Severity) + " " + def.label;

        public override int UIGroupKey => uiKey != 0 ? uiKey : (uiKey = ++nextUiKey);

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
