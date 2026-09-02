using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Psyche
{
    public class JobDriver_SeekTherapy : JobDriver
    {
        private int nextWanderTick;
        private bool reachedRendezvous;

        public bool ReachedRendezvous => reachedRendezvous;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref reachedRendezvous, "reachedRendezvous", false);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);

            IntVec3 anchor = job.GetTarget(TargetIndex.A).Cell;
            Toil wait = ToilMaker.MakeToil();
            wait.defaultCompleteMode = ToilCompleteMode.Delay;
            wait.defaultDuration = PsycheTuning.SeekWaitTicks;
            wait.AddFailCondition(() => !PsycheTherapy.NeedsCare(pawn) || !PsycheTherapy.IsClaimed(pawn));
            wait.initAction = () => reachedRendezvous = true;
            wait.tickAction = () =>
            {
                if (CounselorArriving())
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

        // Holding still once the counselor is nearby keeps their short walk over from becoming a chase.
        private bool CounselorArriving()
        {
            Pawn? counselor = PsycheTherapy.Counselor(pawn);
            return counselor != null && counselor.Spawned && counselor.Map == pawn.Map
                && counselor.Position.InHorDistOf(pawn.Position, PsycheTuning.SeekWanderRadius + 2);
        }
    }
}
