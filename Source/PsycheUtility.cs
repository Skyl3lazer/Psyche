using RimWorld;
using Verse;

namespace Psyche
{
    public static class PsycheUtility
    {
        public const float HealthScale = 100f;

        private static int nextUiGroupKey;

        public static int NextUiGroupKey() => ++nextUiGroupKey;

        public static bool IsTracked(Pawn pawn)
        {
            return pawn != null && (pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony);
        }

        public static string BandedLabel(Hediff hediff)
        {
            string band = hediff.CurStage?.label;
            if (band.NullOrEmpty())
            {
                return hediff.def.label;
            }

            return hediff.def.label.NullOrEmpty() ? band : band + " " + hediff.def.label;
        }
    }
}
