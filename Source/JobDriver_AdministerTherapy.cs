using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psyche
{
    public class JobDriver_AdministerTherapy : JobDriver
    {
        private Pawn Patient => (Pawn)job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(Patient, job, 1, -1, null, errorOnFailed))
            {
                return false;
            }

            // A missing rendezvous should time out of the wait, not error the job.
            IntVec3 spot = PsycheTherapy.EnsureRendezvous(Patient);
            job.SetTarget(TargetIndex.B, spot.IsValid ? spot : Patient.Position);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => !PsycheTherapy.IsTherapyCandidate(Patient));

            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.Touch);
            yield return WaitForPatient();

            Toil approach = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            approach.AddFailCondition(() => !PsycheTherapy.AtRendezvous(Patient));
            yield return approach;

            Toil session = Toils_General.WaitWith(TargetIndex.A, PsycheTuning.TherapySessionTicks, useProgressBar: true,
                maintainPosture: true, face: TargetIndex.A);
            session.activeSkill = () => SkillDefOf.Social;
            yield return session;

            Toil apply = ToilMaker.MakeToil();
            apply.initAction = () =>
            {
                PsycheTherapy.ApplySession(pawn, Patient);
                // Clearing this on any job end would re-roll the cell out from under the waiting patient.
                PsycheTherapy.ClearRendezvous(Patient);
            };
            yield return apply;
        }

        // A deadline set in initAction is still zero when the fail conditions first run.
        private Toil WaitForPatient()
        {
            Toil toil = ToilMaker.MakeToil();
            int waited = 0;
            toil.initAction = () =>
            {
                pawn.pather.StopDead();
                waited = 0;
            };
            toil.tickAction = () =>
            {
                if (PsycheTherapy.AtRendezvous(Patient))
                {
                    ReadyForNextToil();
                    return;
                }

                if (++waited > PsycheTuning.TherapyRendezvousWaitTicks)
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.socialMode = RandomSocialMode.Normal;
            return toil;
        }
    }
}
