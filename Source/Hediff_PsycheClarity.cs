using Verse;

namespace Psyche
{
    public class Hediff_PsycheClarity : HediffWithComps
    {
        private int uiKey;

        public override string LabelBase => PsycheUtility.BandedLabel(this);

        public override string LabelInBrackets => string.Empty;

        public override bool Visible => PsycheUtility.HasPsyche(pawn) && base.Visible;

        public override int UIGroupKey => uiKey != 0 ? uiKey : (uiKey = PsycheUtility.NextUiGroupKey());

        public override bool TryMergeWith(Hediff other) => false;
    }
}
