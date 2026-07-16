using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Psyche
{
    public static class PsycheOwnTriggers
    {
        public static void Fire(Pawn pawn, ThoughtDef def)
        {
            Fire(pawn, def, DefaultUnit(def));
        }

        public static void FireWithPart(Pawn pawn, ThoughtDef def, int lostPartIndex, int killerId = 0)
        {
            Fire(pawn, def, DefaultUnit(def), lostPartIndex, killerId);
        }

        private static float DefaultUnit(ThoughtDef def)
        {
            return (def != null && def.stages != null && def.stages.Count > 0 ? def.stages[0].baseMoodEffect : 0f) * PsycheTuning.WoundScale;
        }

        public static void Fire(Pawn pawn, ThoughtDef def, float unit, int lostPartIndex = -1, int killerId = 0)
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
                fresh.InitMagnitude(unit);
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
