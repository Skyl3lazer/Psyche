using System.Collections.Generic;
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
        private List<int> lostPartIndices = new List<int>();

        public float PeakSeverity => Mathf.Max(peakSeverity, Severity);

        public int SubjectId => subjectId;

        public int KillerId => killerId;

        public void SetOrigin(int subject, int killer)
        {
            subjectId = subject;
            killerId = killer;
        }

        public int LostPartCount => lostPartIndices.Count;

        public bool HasLostPart(int partIndex) => lostPartIndices.Contains(partIndex);

        public bool RemoveLostPart(int partIndex) => lostPartIndices.Remove(partIndex);

        public void SetLostParts(IEnumerable<int> parts)
        {
            lostPartIndices.Clear();
            if (parts == null)
            {
                return;
            }

            foreach (int part in parts)
            {
                if (part >= 0 && !lostPartIndices.Contains(part))
                {
                    lostPartIndices.Add(part);
                }
            }
        }

        public override string LabelBase => PsycheUtility.BandedLabel(this);

        public override string LabelInBrackets => string.Empty;

        public override bool Visible => PsycheUtility.HasPsyche(pawn) && base.Visible;

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
            Scribe_Collections.Look(ref lostPartIndices, "psyche_lostPartIndices", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && lostPartIndices == null)
            {
                lostPartIndices = new List<int>();
            }
        }
    }
}
