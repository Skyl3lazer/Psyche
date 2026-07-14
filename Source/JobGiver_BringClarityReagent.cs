using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Psyche
{
    public class JobGiver_BringClarityReagent : ThinkNode_JobGiver
    {
        protected override Job? TryGiveJob(Pawn pawn)
        {
            if (pawn.Map == null || !(pawn.GetLord()?.LordJob is LordJob_Ritual ritual))
            {
                return null;
            }

            IntVec3 spot = ritual.selectedTarget.Cell;
            int needed = PsycheTuning.ClarityGlitterCost - PsycheClarityWindows.GlitterNear(pawn.Map, spot, 2.9f);
            if (needed <= 0)
            {
                return null;
            }

            Thing? glitter = PsycheClarityWindows.FindGlitterStack(pawn);
            if (glitter == null)
            {
                return null;
            }

            Job job = JobMaker.MakeJob(JobDefOf.HaulToCell, glitter, spot);
            job.count = Mathf.Min(needed, glitter.stackCount);
            job.haulMode = HaulMode.ToCellNonStorage;
            return job;
        }
    }
}
