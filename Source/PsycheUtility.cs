using RimWorld;
using Verse;

namespace Psyche
{
    public static class PsycheUtility
    {
        public const float HealthScale = 100f;

        public static bool IsTracked(Pawn pawn)
        {
            return pawn != null && (pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony);
        }
    }
}
