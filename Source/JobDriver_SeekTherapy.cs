using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Psyche
{
    public class JobDriver_SeekTherapy : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);

            Toil wait = Toils_General.Wait(PsycheTuning.SeekWaitTicks);
            wait.AddFailCondition(() => !PsycheTherapy.NeedsCare(pawn));
            yield return wait;
        }
    }
}
