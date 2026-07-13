using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public static class PsycheDebugActions
    {
        [DebugAction("Psyche", "Add test psyche thought", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddTestThought(Pawn p)
        {
            if (p.needs?.TryGetNeed<Need_Psyche>() == null)
            {
                return;
            }

            p.needs.mood?.thoughts?.memories?.TryGainMemory(PsycheDefOf.Psyche_TestThought);
        }

        [DebugAction("Psyche", "Expire psyche thoughts (roll scars)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ExpireThoughts(Pawn p)
        {
            MemoryThoughtHandler? memories = p.needs?.mood?.thoughts?.memories;
            if (memories == null)
            {
                return;
            }

            foreach (Thought_Psyche pt in PsycheThoughts(p))
            {
                pt.age = pt.DurationTicks + 1;
                memories.RemoveMemory(pt);
            }
        }

        [DebugAction("Psyche", "Clear scars", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ClearScars(Pawn p)
        {
            foreach (Hediff h in Scars(p))
            {
                p.health.RemoveHediff(h);
            }

            p.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }

        [DebugAction("Psyche", "Dump", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Dump(Pawn p)
        {
            Need_Psyche? need = p.needs?.TryGetNeed<Need_Psyche>();
            if (need == null)
            {
                Log.Message(p.LabelShort + ": no psyche need");
                return;
            }

            StatDef stat = StatDefOf.MentalBreakThreshold;
            float modelMbt = Mathf.Clamp(need.InnateMbt + need.ThresholdOffset, stat.minValue, stat.maxValue);
            float statMbt = p.GetStatValue(stat);

            List<Thought_Psyche> thoughts = PsycheThoughts(p);
            List<Hediff> scars = Scars(p);

            Log.Message(string.Format(
                "{0} psyche: M0={1:0.###} baseMax={2:0.#} effMax={3:0.#} cur={4:P0} max={5:P0} offset={6:+0.###;-0.###;0} modelMBT={7:0.###} statMBT={8:0.###} | psycheThoughts={9} (dmg {10:0.#}) scars={11} (sev {12:0.#})",
                p.LabelShort, need.InnateMbt, need.BaseMaxHealth, need.EffectiveMaxHealth, need.CurLevel, need.MaxLevel,
                need.ThresholdOffset, modelMbt, statMbt, thoughts.Count, thoughts.Sum(t => t.CurrentPsycheDamage), scars.Count, scars.Sum(h => h.Severity)));
        }

        private static List<Thought_Psyche> PsycheThoughts(Pawn p)
        {
            List<Thought_Psyche> result = new List<Thought_Psyche>();
            List<Thought_Memory>? memories = p.needs?.mood?.thoughts?.memories?.Memories;
            if (memories == null)
            {
                return result;
            }

            for (int i = 0; i < memories.Count; i++)
            {
                if (memories[i] is Thought_Psyche pt)
                {
                    result.Add(pt);
                }
            }

            return result;
        }

        private static List<Hediff> Scars(Pawn p) =>
            p.health.hediffSet.hediffs.Where(h => h.def == PsycheDefOf.Psyche_Scar).ToList();
    }
}
