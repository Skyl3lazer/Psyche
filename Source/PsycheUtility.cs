using System.Collections.Generic;
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

        public static IEnumerable<Pawn> TrackedColonists()
        {
            List<Map> maps = Find.Maps;
            for (int m = 0; m < maps.Count; m++)
            {
                IReadOnlyList<Pawn> pawns = maps[m].mapPawns.AllPawnsSpawned;
                for (int i = 0; i < pawns.Count; i++)
                {
                    if (IsTracked(pawns[i]))
                    {
                        yield return pawns[i];
                    }
                }
            }
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
