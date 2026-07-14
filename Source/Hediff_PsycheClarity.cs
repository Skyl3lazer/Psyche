using Verse;

namespace Psyche
{
    public class Hediff_PsycheClarity : HediffWithComps
    {
        private static int nextUiKey;

        private int uiKey;

        public override string LabelBase => PsycheClarityWindows.ClarityBandLabel(Severity) + " " + def.label;

        public override int UIGroupKey => uiKey != 0 ? uiKey : (uiKey = ++nextUiKey);

        public override bool TryMergeWith(Hediff other) => false;
    }
}
