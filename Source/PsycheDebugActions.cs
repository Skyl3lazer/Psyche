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
        [DebugAction("Psyche", "Add wound (20)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddWound20(Pawn p) => AddWound(p, 20f, 6f);

        [DebugAction("Psyche", "Add wound (40)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddWound40(Pawn p) => AddWound(p, 40f, 12f);

        [DebugAction("Psyche", "Force-heal wounds (roll scars)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ForceHeal(Pawn p)
        {
            if (p.needs?.TryGetNeed<Need_Psyche>() == null)
            {
                return;
            }

            foreach (Hediff h in Wounds(p))
            {
                h.TryGetComp<HediffComp_PsycheWound>()?.ForceFullHeal();
            }
        }

        [DebugAction("Psyche", "Clear wounds", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ClearWounds(Pawn p) => RemoveAll(p, Wounds(p));

        [DebugAction("Psyche", "Clear scars", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ClearScars(Pawn p) => RemoveAll(p, Scars(p));

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

            List<Hediff> wounds = Wounds(p);
            List<Hediff> scars = Scars(p);

            Log.Message(string.Format(
                "{0} psyche: M0={1:0.###} baseMax={2:0.#} effMax={3:0.#} cur={4:P0} max={5:P0} offset={6:+0.###;-0.###;0} modelMBT={7:0.###} statMBT={8:0.###} | wounds={9} (sev {10:0.#}) scars={11} (sev {12:0.#})",
                p.LabelShort, need.InnateMbt, need.BaseMaxHealth, need.EffectiveMaxHealth, need.CurLevel, need.MaxLevel,
                need.ThresholdOffset, modelMbt, statMbt, wounds.Count, wounds.Sum(h => h.Severity), scars.Count, scars.Sum(h => h.Severity)));
        }

        private static void AddWound(Pawn p, float magnitude, float days)
        {
            if (p.needs?.TryGetNeed<Need_Psyche>() == null)
            {
                return;
            }

            PsycheWounds.Apply(p, magnitude, days);
        }

        private static void RemoveAll(Pawn p, List<Hediff> hediffs)
        {
            foreach (Hediff h in hediffs)
            {
                p.health.RemoveHediff(h);
            }

            p.needs?.TryGetNeed<Need_Psyche>()?.RecomputeFromHediffs();
        }

        private static List<Hediff> Wounds(Pawn p) =>
            p.health.hediffSet.hediffs.Where(h => h.def == PsycheDefOf.Psyche_Wound).ToList();

        private static List<Hediff> Scars(Pawn p) =>
            p.health.hediffSet.hediffs.Where(h => h.def == PsycheDefOf.Psyche_Scar).ToList();
    }
}
