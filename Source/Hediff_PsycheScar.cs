using UnityEngine;
using Verse;

namespace Psyche
{
    public class Hediff_PsycheScar : HediffWithComps
    {
        private float peakSeverity;
        private int uiKey;
        private int subjectId;
        private int killerId;

        public float PeakSeverity => Mathf.Max(peakSeverity, Severity);

        public int SubjectId => subjectId;

        public int KillerId => killerId;

        public void SetOrigin(int subject, int killer)
        {
            subjectId = subject;
            killerId = killer;
        }

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
            Scribe_Values.Look(ref subjectId, "psyche_subjectId", 0);
            Scribe_Values.Look(ref killerId, "psyche_killerId", 0);
        }
    }
}
