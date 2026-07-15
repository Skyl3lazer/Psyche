using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public static class PsycheClosure
    {
        public static void OnBuried(Pawn deceased, Map map)
        {
            if (deceased == null || map == null)
            {
                return;
            }

            bool bittersweet = IsBittersweet(deceased);
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn griever = pawns[i];
                if (!PsycheUtility.IsTracked(griever))
                {
                    continue;
                }

                List<Thought_Memory>? memories = griever.needs?.mood?.thoughts?.memories?.Memories;
                if (memories == null)
                {
                    continue;
                }

                float quality = FuneralQuality(memories);
                bool closedAny = false;
                for (int j = 0; j < memories.Count; j++)
                {
                    if (memories[j] is Thought_Psychlet pt && !pt.IsBoon && pt.otherPawn == deceased)
                    {
                        pt.Close(quality);
                        closedAny = true;
                    }
                }

                if (!closedAny)
                {
                    continue;
                }

                griever.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
                if (bittersweet)
                {
                    GrantBittersweetPayoff(griever, quality);
                }
            }
        }

        public static bool IsBittersweet(Pawn deceased) =>
            deceased.RaceProps != null && deceased.ageTracker != null
            && deceased.ageTracker.AgeBiologicalYears >= deceased.RaceProps.lifeExpectancy * PsycheTuning.BittersweetAgeFrac;

        private static float FuneralQuality(List<Thought_Memory> memories)
        {
            float best = PsycheTuning.BurialBaseQuality;
            for (int i = 0; i < memories.Count; i++)
            {
                float q = QualityFor(memories[i].def.defName);
                if (q > best)
                {
                    best = q;
                }
            }

            return best;
        }

        private static float QualityFor(string defName)
        {
            switch (defName)
            {
                case "HeartwarmingFuneral":
                    return 1f;
                case "GoodFuneral":
                    return 0.7f;
                default:
                    return 0f;
            }
        }

        private static void GrantBittersweetPayoff(Pawn griever, float quality)
        {
            griever.needs?.mood?.thoughts?.memories?.TryGainMemory(PsycheDefOf.Psyche_LongLife);
            if (Rand.Chance(quality * PsycheTuning.ClarityPayoffChancePerQuality))
            {
                PsycheClarities.FormDirect(griever, PsycheTuning.ClosureClaritySize);
            }
        }
    }
}
