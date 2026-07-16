using System.Collections.Generic;
using Verse;

namespace Psyche
{
    public class HediffCompProperties_LimbLoss : HediffCompProperties
    {
        public HediffCompProperties_LimbLoss()
        {
            compClass = typeof(HediffComp_LimbLoss);
        }
    }

    public class HediffComp_LimbLoss : HediffComp
    {
        private LimbLossData data = new LimbLossData();

        public int LostPartCount => data.Count;

        public bool HasLostPart(int partIndex) => data.Has(partIndex);

        public bool RemoveLostPart(int partIndex) => data.Remove(partIndex);

        public void SetLostParts(IEnumerable<int>? parts) => data.Set(parts);

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref data, "psyche_limbLoss");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && data == null)
            {
                data = new LimbLossData();
            }
        }
    }
}
