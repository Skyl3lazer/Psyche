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

            foreach (Thought_Psychlet pt in PsycheThoughts(p))
            {
                pt.age = pt.DurationTicks + 1;
                memories.RemoveMemory(pt);
            }
        }

        [DebugAction("Psyche", "Add test scar", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddScar(Pawn p)
        {
            BodyPartRecord? brain = p.health.hediffSet.GetBrain();
            if (brain == null)
            {
                return;
            }

            Hediff scar = HediffMaker.MakeHediff(PsycheDefOf.Psyche_Scar, p, brain);
            scar.Severity = 10f;
            (scar as Hediff_PsycheScar)?.NotePeak();
            p.health.AddHediff(scar, brain);
            p.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }

        [DebugAction("Psyche", "Add deep test scar (sev 5)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddDeepScar(Pawn p)
        {
            BodyPartRecord? brain = p.health.hediffSet.GetBrain();
            if (brain == null)
            {
                return;
            }

            Hediff scar = HediffMaker.MakeHediff(PsycheDefOf.Psyche_Scar, p, brain);
            scar.Severity = 5f;
            (scar as Hediff_PsycheScar)?.NotePeak();
            p.health.AddHediff(scar, brain);
            p.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }

        [DebugAction("Psyche", "Add haunted scar (named, sev 7)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddHauntedScar(Pawn p)
        {
            BodyPartRecord? brain = p.health.hediffSet.GetBrain();
            if (brain == null)
            {
                return;
            }

            Hediff scar = HediffMaker.MakeHediff(PsycheDefOf.Psyche_Scar_Haunted, p, brain);
            scar.Severity = 7f;
            (scar as Hediff_PsycheScar)?.NotePeak();
            p.health.AddHediff(scar, brain);
            p.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }

        [DebugAction("Psyche", "Add hard-won clarity (named, sev 7)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddHardWonClarity(Pawn p)
        {
            BodyPartRecord? brain = p.health.hediffSet.GetBrain();
            if (brain == null)
            {
                return;
            }

            Hediff clarity = HediffMaker.MakeHediff(PsycheDefOf.Psyche_Clarity_HardWon, p, brain);
            clarity.Severity = 7f;
            p.health.AddHediff(clarity, brain);
            p.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }

        [DebugAction("Psyche", "Apply closure to open wounds (q=1)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ApplyClosure(Pawn p)
        {
            foreach (Thought_Psychlet pt in PsycheThoughts(p))
            {
                pt.Close(1f);
            }

            p.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }

        [DebugAction("Psyche", "Medicate open wounds (one dose)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void MedicateWounds(Pawn p)
        {
            foreach (Thought_Psychlet pt in PsycheThoughts(p))
            {
                pt.Medicate(1f);
            }
        }

        [DebugAction("Psyche", "Trade worst scar (addiction shortcut)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TradeWorstScar(Pawn p)
        {
            PsycheAddictionShortcut.ReduceWorstScar(p);
        }

        [DebugAction("Psyche", "Reconcile trait marks", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ReconcileTraitMarks(Pawn p)
        {
            PsycheTraitMarks.Reconcile(p);
        }

        [DebugAction("Psyche", "Add clarity window (sev 6)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddClarityWindow(Pawn p)
        {
            MemoryThoughtHandler? memories = p.needs?.mood?.thoughts?.memories;
            if (memories == null)
            {
                return;
            }

            Thought_ClarityWindow window = (Thought_ClarityWindow)ThoughtMaker.MakeThought(PsycheDefOf.Psyche_ClarityWindow);
            window.healedSeverity = 6f;
            memories.TryGainMemory(window);
        }

        [DebugAction("Psyche", "Resolve clarity window (quality 1)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ResolveClarityWindow(Pawn p)
        {
            Thought_ClarityWindow? window = PsycheClarityWindows.WorstWindow(p);
            if (window != null)
            {
                PsycheClarityWindows.ResolveAttempt(p, window, 1f);
            }
        }

        [DebugAction("Psyche", "Add test boon", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddBoon(Pawn p)
        {
            if (p.needs?.TryGetNeed<Need_Psyche>() == null)
            {
                return;
            }

            p.needs.mood?.thoughts?.memories?.TryGainMemory(PsycheDefOf.Psyche_TestBoon);
        }

        [DebugAction("Psyche", "Add test clarity", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddClarity(Pawn p)
        {
            BodyPartRecord? brain = p.health.hediffSet.GetBrain();
            if (brain == null)
            {
                return;
            }

            Hediff clarity = HediffMaker.MakeHediff(PsycheDefOf.Psyche_Clarity, p, brain);
            clarity.Severity = 10f;
            p.health.AddHediff(clarity, brain);
            p.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }

        [DebugAction("Psyche", "Repair worst scar (one session)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RepairSession(Pawn p)
        {
            Hediff_PsycheScar? scar = PsycheRepair.WorstReducibleScar(p);
            if (scar == null)
            {
                return;
            }

            PsycheRepair.ApplyRepairSession(p, p, scar);
            p.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }

        [DebugAction("Psyche", "Successful therapy (one session)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SuccessfulRepairSession(Pawn p)
        {
            Hediff_PsycheScar? scar = PsycheRepair.WorstReducibleScar(p);
            if (scar == null)
            {
                return;
            }

            PsycheRepair.ApplyQuality(p, scar, 1f, PsycheRepair.BestResearchedTier());
            p.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }

        [DebugAction("Psyche", "Treat worst injury (one session)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TreatInjury(Pawn p)
        {
            Thought_Psychlet? worst = null;
            foreach (Thought_Psychlet pt in PsycheThoughts(p))
            {
                if (pt.CurrentPsycheDamage > 0f && (worst == null || pt.CurrentPsycheDamage > worst.CurrentPsycheDamage))
                {
                    worst = pt;
                }
            }

            if (worst == null)
            {
                return;
            }

            worst.Treat(p.skills?.GetSkill(SkillDefOf.Social)?.Level ?? 0);
            p.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
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

            List<Thought_Psychlet> thoughts = PsycheThoughts(p);
            List<Hediff> scars = Scars(p);

            Log.Message(string.Format(
                "{0} psyche: M0={1:0.###} baseMax={2:0.#} effMax={3:0.#} cur={4:P0} max={5:P0} offset={6:+0.###;-0.###;0} modelMBT={7:0.###} statMBT={8:0.###} | psycheThoughts={9} (dmg {10:0.#}) scars={11} (sev {12:0.#})",
                p.LabelShort, need.InnateMbt, need.BaseMaxHealth, need.EffectiveMaxHealth, need.CurLevel, need.MaxLevel,
                need.ThresholdOffset, modelMbt, statMbt, thoughts.Count, thoughts.Sum(t => t.CurrentPsycheDamage), scars.Count, scars.Sum(h => h.Severity)));
        }

        private static List<Thought_Psychlet> PsycheThoughts(Pawn p)
        {
            List<Thought_Psychlet> result = new List<Thought_Psychlet>();
            List<Thought_Memory>? memories = p.needs?.mood?.thoughts?.memories?.Memories;
            if (memories == null)
            {
                return result;
            }

            for (int i = 0; i < memories.Count; i++)
            {
                if (memories[i] is Thought_Psychlet pt)
                {
                    result.Add(pt);
                }
            }

            return result;
        }

        private static List<Hediff> Scars(Pawn p) =>
            p.health.hediffSet.hediffs.Where(h => h is Hediff_PsycheScar).ToList();
    }
}
