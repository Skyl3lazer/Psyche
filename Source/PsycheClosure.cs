using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public static class PsycheClosure
    {
        public static bool InstallingArtificial;

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

                Messages.Message(
                    (bittersweet ? "Psyche_BurialClosureBittersweet" : "Psyche_BurialClosure")
                        .Translate(griever.LabelShort, deceased.LabelShort).CapitalizeFirst(),
                    griever,
                    MessageTypeDefOf.PositiveEvent,
                    false);
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

        public static void OnPartReplacedArtificial(Pawn pawn, int partIndex, bool betterThanNatural)
        {
            if (pawn == null)
            {
                return;
            }

            TraitSet? traits = pawn.story?.traits;
            if (traits != null && traits.HasTrait(PsycheDefOf.Transhumanist))
            {
                CloseForPart(pawn, partIndex, "Psyche_LimbReplacedClosure_Transhuman");
            }
            else if (traits != null && traits.HasTrait(PsycheDefOf.BodyPurist))
            {
                // A machine part is no peace for a body purist - only a return to flesh closes their loss.
            }
            else if (betterThanNatural)
            {
                CloseForPart(pawn, partIndex, "Psyche_LimbReplacedClosure");
            }
        }

        public static void OnPartRestoredNatural(Pawn pawn, int partIndex)
        {
            if (pawn != null && pawn.story?.traits?.HasTrait(PsycheDefOf.BodyPurist) == true)
            {
                CloseForPart(pawn, partIndex, "Psyche_LimbReplacedClosure_Purist");
            }
        }

        private static void CloseForPart(Pawn pawn, int partIndex, string messageKey)
        {
            bool closedAny = false;

            List<Thought_Memory>? memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
            if (memories != null)
            {
                for (int i = 0; i < memories.Count; i++)
                {
                    if (memories[i] is Thought_Psychlet pt && pt.HasLostPart(partIndex))
                    {
                        int before = pt.LostPartCount;
                        pt.RemoveLostPart(partIndex);
                        pt.Close(PsycheTuning.LimbReplacedQuality / Mathf.Max(1, before));
                        closedAny = true;
                    }
                }
            }

            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                if (hediffs[i] is Hediff_PsycheScar scar && scar.HasLostPart(partIndex))
                {
                    int before = scar.LostPartCount;
                    scar.RemoveLostPart(partIndex);
                    scar.Severity *= (float)(before - 1) / before;
                    closedAny = true;

                    if (scar.LostPartCount == 0 || scar.Severity <= 0f)
                    {
                        pawn.health.RemoveHediff(scar);
                        if (Rand.Chance(PsycheTuning.LimbReplacedClarityChance))
                        {
                            PsycheClarities.FormDirect(pawn, PsycheTuning.ClosureClaritySize, PsycheDefOf.Psyche_Clarity_MadeWhole);
                        }
                    }
                }
            }

            if (!closedAny)
            {
                return;
            }

            pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();

            BodyPartRecord part = pawn.RaceProps.body.GetPartAtIndex(partIndex);
            if (part != null)
            {
                Messages.Message(
                    messageKey.Translate(pawn.LabelShort, part.LabelShort).CapitalizeFirst(),
                    pawn,
                    MessageTypeDefOf.PositiveEvent,
                    false);
            }
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
                bool griefClosed = false;
                bool maimClosed = false;

                List<Thought_Memory>? memories = griever.needs?.mood?.thoughts?.memories?.Memories;
                if (memories != null)
                {
                    for (int i = 0; i < memories.Count; i++)
                    {
                        if (memories[i] is Thought_Psychlet pt && !pt.IsBoon && pt.KillerId == killerId)
                        {
                            pt.Close(PsycheTuning.RevengeQuality);
                            if (pt.otherPawn != null)
                            {
                                griefClosed = true;
                            }
                            else
                            {
                                maimClosed = true;
                            }
                        }
                    }
                }

                // Grief scars heal on revenge; a maiming's phantom scar does not - killing them never regrew the limb.
                List<Hediff> hediffs = griever.health.hediffSet.hediffs;
                for (int i = hediffs.Count - 1; i >= 0; i--)
                {
                    if (hediffs[i] is Hediff_PsycheScar scar && scar.KillerId == killerId && scar.SubjectId != 0)
                    {
                        RevengeHealScar(griever, scar);
                        griefClosed = true;
                    }
                }

                if (griefClosed || maimClosed)
                {
                    griever.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
                }

                if (griefClosed)
                {
                    Messages.Message(
                        "Psyche_RevengeClosure".Translate(griever.LabelShort, killer.LabelShort).CapitalizeFirst(),
                        griever,
                        MessageTypeDefOf.PositiveEvent,
                        false);
                }

                if (maimClosed)
                {
                    Messages.Message(
                        "Psyche_MaimRevengeClosure".Translate(griever.LabelShort, killer.LabelShort).CapitalizeFirst(),
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

        private static IEnumerable<Pawn> TrackedColonists() => PsycheUtility.TrackedColonists();

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
