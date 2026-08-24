using RimWorld;
using Verse;
using Verse.AI;

namespace Psyche
{
    public class JobGiver_SeekPsycheTherapy : ThinkNode_JobGiver
    {
        protected override Job? TryGiveJob(Pawn pawn)
        {
            if (pawn.Map == null || !PsycheUtility.IsTracked(pawn) || !PsycheTherapy.TherapyPermitted(pawn)
                || !PsycheTherapy.NeedsCare(pawn) || !PsycheTherapy.IsClaimed(pawn) || NeedsComeFirst(pawn))
            {
                return null;
            }

            IntVec3 spot = PsycheTherapy.EnsureRendezvous(pawn);
            if (!spot.IsValid)
            {
                return null;
            }

            return JobMaker.MakeJob(PsycheDefOf.Psyche_SeekTherapy, spot);
        }

        // Sitting above the main behavior core means declining what that core would have handled first.
        private static bool NeedsComeFirst(Pawn pawn)
        {
            if (pawn.needs?.rest != null && pawn.needs.rest.CurCategory >= RestCategory.Tired)
            {
                return true;
            }

            return pawn.needs?.food != null && pawn.needs.food.CurCategory >= HungerCategory.Hungry;
        }
    }
}
