using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public static class PsycheDebugActions
    {
        [DebugAction("Psyche", "Psyche: damage 10 HP", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DamageTen(Pawn p) => AdjustHealth(p, -10f);

        [DebugAction("Psyche", "Psyche: heal 10 HP", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void HealTen(Pawn p) => AdjustHealth(p, 10f);

        [DebugAction("Psyche", "Psyche: set 100%", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetFull(Pawn p) => SetLevel(p, 1f);

        [DebugAction("Psyche", "Psyche: set 50%", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetHalf(Pawn p) => SetLevel(p, 0.5f);

        [DebugAction("Psyche", "Psyche: set 0%", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetEmpty(Pawn p) => SetLevel(p, 0f);

        [DebugAction("Psyche", "Psyche: dump", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Dump(Pawn p)
        {
            Need_Psyche? need = p.needs?.TryGetNeed<Need_Psyche>();
            if (need == null)
            {
                Log.Message(p.LabelShort + ": no psyche need");
                return;
            }

            float modelMbt = Mathf.Clamp(need.InnateMbt + need.ThresholdOffset, 0.01f, 0.5f);
            float statMbt = p.GetStatValue(StatDefOf.MentalBreakThreshold);
            Log.Message(string.Format(
                "{0} psyche: M0={1:0.###} baseMax={2:0.#} cur={3:P0} max={4:P0} offset={5:+0.###;-0.###;0} modelMBT={6:0.###} statMBT={7:0.###}",
                p.LabelShort, need.InnateMbt, need.BaseMaxHealth, need.CurLevel, need.MaxLevel, need.ThresholdOffset, modelMbt, statMbt));
        }

        private static void AdjustHealth(Pawn p, float hp)
        {
            Need_Psyche? need = p.needs?.TryGetNeed<Need_Psyche>();
            if (need == null)
            {
                return;
            }

            float baseMax = need.BaseMaxHealth;
            if (baseMax <= 0f)
            {
                return;
            }

            need.CurLevel += hp / baseMax;
        }

        private static void SetLevel(Pawn p, float pct)
        {
            Need_Psyche? need = p.needs?.TryGetNeed<Need_Psyche>();
            if (need != null)
            {
                need.CurLevel = pct;
            }
        }
    }
}
