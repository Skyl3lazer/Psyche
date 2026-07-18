using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Psyche
{
    public class JobDriver_SeekTherapy : JobDriver
    {
        private int nextWanderTick;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);

            IntVec3 anchor = job.GetTarget(TargetIndex.A).Cell;
            Toil wait = ToilMaker.MakeToil();
            wait.defaultCompleteMode = ToilCompleteMode.Delay;
            wait.defaultDuration = PsycheTuning.SeekWaitTicks;
            wait.AddFailCondition(() => !PsycheTherapy.NeedsCare(pawn) || !PsycheTherapy.IsClaimed(pawn));
            wait.tickAction = () =>
            {
                if (CounselorInSession())
                {
                    pawn.pather.StopDead();
                    return;
                }

                if (!pawn.pather.Moving && Find.TickManager.TicksGame >= nextWanderTick)
                {
                    IntVec3 dest = CellFinder.RandomClosewalkCellNear(anchor, pawn.Map, PsycheTuning.SeekWanderRadius,
                        c => c.Standable(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.None));
                    if (dest.IsValid && dest != pawn.Position)
                    {
                        pawn.pather.StartPath(dest, PathEndMode.OnCell);
                    }

                    nextWanderTick = Find.TickManager.TicksGame + Rand.Range(PsycheTuning.SeekWanderIntervalMin, PsycheTuning.SeekWanderIntervalMax);
                }
            };
            yield return wait;
        }

        private bool CounselorInSession()
        {
            Map map = pawn.Map;
            foreach (IntVec3 c in CellRect.CenteredOn(pawn.Position, 1))
            {
                if (!c.InBounds(map))
                {
                    continue;
                }

                List<Thing> things = c.GetThingList(map);
                for (int i = 0; i < things.Count; i++)
                {
                    if (things[i] is Pawn other && other != pawn
                        && other.CurJobDef == PsycheDefOf.Psyche_AdministerTherapy
                        && other.CurJob?.GetTarget(TargetIndex.A).Thing == pawn)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
