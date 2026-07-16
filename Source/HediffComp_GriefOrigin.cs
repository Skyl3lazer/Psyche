using Verse;

namespace Psyche
{
    public class HediffCompProperties_GriefOrigin : HediffCompProperties
    {
        public HediffCompProperties_GriefOrigin()
        {
            compClass = typeof(HediffComp_GriefOrigin);
        }
    }

    public class HediffComp_GriefOrigin : HediffComp
    {
        private GriefOriginData data = new GriefOriginData();

        public int SubjectId => data.SubjectId;

        public int KillerId => data.KillerId;

        public void SetOrigin(int subject, int killer) => data.Set(subject, killer);

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref data, "psyche_griefOrigin");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && data == null)
            {
                data = new GriefOriginData();
            }
        }
    }
}
