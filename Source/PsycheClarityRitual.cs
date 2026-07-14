using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Psyche
{
    public static class PsycheClarityRitual
    {
        private static bool resolved;
        private static PreceptDef? preceptDef;
        private static RitualPatternDef? patternDef;

        private static void Resolve()
        {
            if (resolved)
            {
                return;
            }

            resolved = true;
            preceptDef = DefDatabase<PreceptDef>.GetNamedSilentFail("Psyche_ClarityContemplation");
            patternDef = DefDatabase<RitualPatternDef>.GetNamedSilentFail("Psyche_ClarityContemplation");
        }

        public static float CounselorScore(Pawn? counselor, Pawn? seeker)
        {
            if (counselor == null || seeker == null)
            {
                return 0f;
            }

            int social = counselor.skills?.GetSkill(SkillDefOf.Social)?.Level ?? 0;
            int opinion = counselor.relations?.OpinionOf(seeker) ?? 0;
            return social + (Mathf.Max(0, opinion) * 0.1f);
        }

        public static bool TryBegin(Pawn pawn)
        {
            if (!ModsConfig.IdeologyActive || pawn.Map == null)
            {
                return false;
            }

            Resolve();
            if (preceptDef == null || patternDef == null)
            {
                return false;
            }

            try
            {
                Ideo ideo = pawn.Ideo;
                if (ideo == null)
                {
                    return false;
                }

                Precept_Ritual? ritual = ideo.GetPrecept(preceptDef) as Precept_Ritual;
                if (ritual == null)
                {
                    Precept_Ritual made = (Precept_Ritual)PreceptMaker.MakePrecept(preceptDef);
                    ideo.AddPrecept(made, true, null, patternDef);
                    ritual = made;
                }

                if (ritual?.behavior == null)
                {
                    return false;
                }

                IntVec3 spot = PsycheTherapy.PickRendezvous(pawn);
                if (!spot.IsValid)
                {
                    spot = pawn.Position;
                }

                TargetInfo target = new TargetInfo(spot, pawn.Map);

                Pawn? best = null;
                float bestScore = 0f;
                foreach (Pawn c in pawn.Map.mapPawns.FreeColonistsSpawned)
                {
                    if (c == pawn || c.Downed)
                    {
                        continue;
                    }

                    float s = CounselorScore(c, pawn);
                    if (s > bestScore)
                    {
                        bestScore = s;
                        best = c;
                    }
                }

                Dictionary<string, Pawn> forced = new Dictionary<string, Pawn> { { "seeker", pawn } };

                Precept_Ritual ritualLocal = ritual;
                Dialog_BeginRitual.ActionCallback action = delegate (RitualRoleAssignments assignments)
                {
                    ritualLocal.behavior.TryExecuteOn(target, null, ritualLocal, null, assignments, true);
                    return true;
                };

                Dialog_BeginRitual.PawnFilter filter = delegate (Pawn p, bool voluntary, bool allowOtherIdeos)
                {
                    if (p == pawn || p == best)
                    {
                        return true;
                    }

                    if (p.GetLord() != null || !p.RaceProps.Humanlike || p.IsSubhuman)
                    {
                        return false;
                    }

                    return p.relations != null && p.relations.OpinionOf(pawn) > 0;
                };

                List<string> extraInfo = new List<string>();
                if (ritual.outcomeEffect?.def?.extraInfoLines != null)
                {
                    extraInfo.AddRange(ritual.outcomeEffect.def.extraInfoLines);
                }

                Window dialog = new Dialog_BeginClarityRitual(best, ritual.Label.CapitalizeFirst(), ritual, target, pawn.Map, action, null, null, filter, "Begin".Translate(), null, forced, null, extraInfo, pawn);
                Find.WindowStack.Add(dialog);
                return true;
            }
            catch (Exception e)
            {
                Log.Warning("[Psyche] Clarity ritual failed to open; falling back to solo. " + e);
                return false;
            }
        }
    }
}
