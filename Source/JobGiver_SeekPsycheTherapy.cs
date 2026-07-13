using Verse;
using Verse.AI;

namespace Psyche
{
    public class JobGiver_SeekPsycheTherapy : ThinkNode_JobGiver
    {
        protected override Job? TryGiveJob(Pawn pawn)
        {
            if (!PsycheUtility.IsTracked(pawn) || !PsycheTherapy.HasRepairableScar(pawn) || !PsycheTherapy.CanReceiveCounseling(pawn))
            {
                return null;
            }

            IntVec3 spot = PsycheTherapy.PickRendezvous(pawn);
            if (!spot.IsValid)
            {
                return null;
            }

            return JobMaker.MakeJob(PsycheDefOf.Psyche_SeekTherapy, spot);
        }
    }
}
