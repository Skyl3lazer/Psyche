using RimWorld;
using Verse;

namespace Psyche
{
    public class Thought_ClarityWindow : Thought_Memory
    {
        public float healedSeverity;
        public HediffDef? upgradeClarityDef;

        public override float MoodOffset() => 0f;

        public string ScarBandLabel => PsycheClarityWindows.BandLabel(healedSeverity);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref healedSeverity, "psyche_healedSeverity", 0f);
            Scribe_Defs.Look(ref upgradeClarityDef, "psyche_upgradeClarityDef");
        }
    }
}
