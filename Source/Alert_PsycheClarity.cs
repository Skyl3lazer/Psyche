using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Psyche
{
    public class Alert_PsycheClarity : Alert
    {
        private readonly List<Pawn> culprits = new List<Pawn>();

        public Alert_PsycheClarity()
        {
            defaultLabel = "Psyche_Alert_SeekClarity".Translate();
            defaultPriority = AlertPriority.Medium;
        }

        private List<Pawn> Culprits()
        {
            culprits.Clear();
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                foreach (Pawn pawn in maps[i].mapPawns.FreeColonists)
                {
                    if (PsycheClarityWindows.EligibleForAlert(pawn))
                    {
                        culprits.Add(pawn);
                    }
                }
            }

            return culprits;
        }

        public override TaggedString GetExplanation() =>
            "Psyche_Alert_SeekClarity_Desc".Translate(PsycheTuning.ClarityGlitterCost);

        public override AlertReport GetReport() => AlertReport.CulpritsAre(Culprits());
    }
}
