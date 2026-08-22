using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public static class PsycheOwnTriggers
    {
        public static void Fire(Pawn pawn, ThoughtDef def)
        {
            Stamp(pawn, def, DefaultUnit(def), scaling: false);
        }

        public static void FireScaled(Pawn pawn, ThoughtDef def, float unit, int lostPartIndex = -1, int killerId = 0)
        {
            Stamp(pawn, def, unit, scaling: true, lostPartIndex, killerId);
        }

        private static float DefaultUnit(ThoughtDef def)
        {
            return (def != null && def.stages != null && def.stages.Count > 0 ? def.stages[0].baseMoodEffect : 0f) * PsycheTuning.WoundScale;
        }

        // Body-part importance priced by what the pawn actually lost the use of, so no per-part table
        // is needed and modded bodies and races price themselves.
        public static float[] CapacitySnapshot(Pawn pawn)
        {
            List<PawnCapacityDef> caps = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
            float[] levels = new float[caps.Count];
            for (int i = 0; i < caps.Count; i++)
            {
                levels[i] = pawn.health.capacities.GetLevel(caps[i]);
            }

            return levels;
        }

        // The authored size is what a total functional loss is worth; a part is worth the share of
        // function it took with it.
        public static float AmputationUnit(Pawn pawn, float[] before)
        {
            List<PawnCapacityDef> caps = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
            float worst = 0f;
            for (int i = 0; i < caps.Count && i < before.Length; i++)
            {
                float drop = before[i] - pawn.health.capacities.GetLevel(caps[i]);
                if (drop > worst)
                {
                    worst = drop;
                }
            }

            return PsycheDefOf.Psyche_OT_Amputation.stages[0].baseMoodEffect * Mathf.Clamp01(worst) * PsycheTuning.WoundScale;
        }

        // Saving a captive is duty, not the personal stake of pulling back a comrade.
        public static float SavedLifeUnit(Pawn patient, float bloodLoss)
        {
            float factor = PsycheTuning.SavedAllyBloodScaleMin
                + ((PsycheTuning.SavedAllyBloodScaleMax - PsycheTuning.SavedAllyBloodScaleMin) * Mathf.Clamp01(bloodLoss));
            if (patient != null && patient.IsPrisonerOfColony)
            {
                factor *= PsycheTuning.SavedPrisonerFactor;
            }

            return PsycheDefOf.Psyche_OT_SavedAlly.stages[0].baseMoodEffect * factor * PsycheTuning.WoundScale;
        }

        private static void Stamp(Pawn pawn, ThoughtDef def, float unit, bool scaling, int lostPartIndex = -1, int killerId = 0)
        {
            if (pawn == null || def == null || !PsycheUtility.IsTracked(pawn))
            {
                return;
            }

            MemoryThoughtHandler? handler = pawn.needs?.mood?.thoughts?.memories;
            if (handler == null)
            {
                return;
            }

            if (unit == 0f)
            {
                return;
            }

            Thought_Psychlet? existing = null;
            List<Thought_Memory> memories = handler.Memories;
            for (int i = 0; i < memories.Count; i++)
            {
                if (memories[i] is Thought_Psychlet pt && pt.def == def && !pt.ShouldDiscard)
                {
                    existing = pt;
                    break;
                }
            }

            Thought_Psychlet target;
            if (existing != null)
            {
                existing.Intensify(unit);
                target = existing;
            }
            else
            {
                Thought_Psychlet fresh = (Thought_Psychlet)ThoughtMaker.MakeThought(def);
                fresh.InitMagnitude(unit, scaling);
                handler.TryGainMemory(fresh);
                target = fresh;
            }

            target.AddLostPart(lostPartIndex);
            if (killerId != 0 && target.KillerId == 0)
            {
                target.StampKiller(killerId);
            }

            pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }

        public static void FireOnRelations(Pawn subject, ThoughtDef def)
        {
            if (subject == null)
            {
                return;
            }

            foreach (Pawn griever in PsycheUtility.TrackedColonists())
            {
                if (griever != subject && griever.relations != null
                    && griever.relations.OpinionOf(subject) >= PsycheTuning.GraveWoundRelationOpinion)
                {
                    Fire(griever, def);
                }
            }
        }
    }
}
