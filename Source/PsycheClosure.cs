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

        public static void OnPawnDied(Pawn dead, Pawn killer)
        {
            if (dead == null)
            {
                return;
            }

            if (killer != null)
            {
                StampKiller(dead, killer.thingIDNumber);
            }

            RevengeClosure(dead);
        }

        private static void StampKiller(Pawn deceased, int killerId)
        {
            foreach (Pawn griever in TrackedColonists())
            {
                List<Thought_Memory>? memories = griever.needs?.mood?.thoughts?.memories?.Memories;
                if (memories == null)
                {
                    continue;
                }

                for (int i = 0; i < memories.Count; i++)
                {
                    if (memories[i] is Thought_Psychlet pt && !pt.IsBoon && pt.otherPawn == deceased)
                    {
                        pt.StampKiller(killerId);
                    }
                }
            }
        }

        private static void RevengeClosure(Pawn killer)
        {
            int killerId = killer.thingIDNumber;
            foreach (Pawn griever in TrackedColonists())
            {
                bool any = false;

                List<Thought_Memory>? memories = griever.needs?.mood?.thoughts?.memories?.Memories;
                if (memories != null)
                {
                    for (int i = 0; i < memories.Count; i++)
                    {
                        if (memories[i] is Thought_Psychlet pt && !pt.IsBoon && pt.KillerId == killerId)
                        {
                            pt.Close(PsycheTuning.RevengeQuality);
                            any = true;
                        }
                    }
                }

                List<Hediff> hediffs = griever.health.hediffSet.hediffs;
                for (int i = hediffs.Count - 1; i >= 0; i--)
                {
                    if (hediffs[i] is Hediff_PsycheScar scar && scar.KillerId == killerId)
                    {
                        RevengeHealScar(griever, scar);
                        any = true;
                    }
                }

                if (any)
                {
                    griever.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
                    Messages.Message(
                        "Psyche_RevengeClosure".Translate(griever.LabelShort, killer.LabelShort).CapitalizeFirst(),
                        griever,
                        MessageTypeDefOf.PositiveEvent,
                        false);
                }
            }
        }

        private static void RevengeHealScar(Pawn griever, Hediff_PsycheScar scar)
        {
            scar.Severity = Mathf.Max(0f, scar.Severity - PsycheTuning.ClosureScarHeal);
            if (Rand.Chance(PsycheTuning.RevengeClarityChance))
            {
                PsycheClarities.FormDirect(griever, PsycheTuning.ClosureClaritySize);
            }

            if (scar.Severity <= 0f)
            {
                griever.health.RemoveHediff(scar);
            }
        }

        private static IEnumerable<Pawn> TrackedColonists()
        {
            List<Map> maps = Find.Maps;
            for (int m = 0; m < maps.Count; m++)
            {
                IReadOnlyList<Pawn> pawns = maps[m].mapPawns.AllPawnsSpawned;
                for (int i = 0; i < pawns.Count; i++)
                {
                    if (PsycheUtility.IsTracked(pawns[i]))
                    {
                        yield return pawns[i];
                    }
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
