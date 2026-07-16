using System.Collections.Generic;
using Verse;

namespace Psyche
{
    public class LimbLossData : IExposable
    {
        private List<int> lostPartIndices = new List<int>();

        public IReadOnlyList<int> Parts => lostPartIndices;

        public int Count => lostPartIndices.Count;

        public bool Has(int partIndex) => lostPartIndices.Contains(partIndex);

        public void Add(int partIndex)
        {
            if (partIndex >= 0 && !lostPartIndices.Contains(partIndex))
            {
                lostPartIndices.Add(partIndex);
            }
        }

        public bool Remove(int partIndex) => lostPartIndices.Remove(partIndex);

        public void Set(IEnumerable<int>? parts)
        {
            lostPartIndices.Clear();
            if (parts == null)
            {
                return;
            }

            foreach (int part in parts)
            {
                Add(part);
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref lostPartIndices, "lostPartIndices", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && lostPartIndices == null)
            {
                lostPartIndices = new List<int>();
            }
        }
    }

    public class GriefOriginData : IExposable
    {
        private int subjectId;
        private int killerId;

        public int SubjectId => subjectId;

        public int KillerId => killerId;

        public void Set(int subject, int killer)
        {
            subjectId = subject;
            killerId = killer;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref subjectId, "subjectId", 0);
            Scribe_Values.Look(ref killerId, "killerId", 0);
        }
    }
}
