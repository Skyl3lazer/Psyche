using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psyche
{
    public class JobDriver_AdministerTherapy : JobDriver
    {
        private Pawn Patient => (Pawn)job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            pawn.Reserve(Patient, job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => Patient.CurJobDef != PsycheDefOf.Psyche_SeekTherapy);
            this.FailOn(() => !PsycheTherapy.NeedsCare(Patient));

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil session = Toils_General.Wait(PsycheTuning.TherapySessionTicks, TargetIndex.A);
            session.WithProgressBarToilDelay(TargetIndex.A);
            session.activeSkill = () => SkillDefOf.Social;
            yield return session;

            Toil apply = ToilMaker.MakeToil();
            apply.initAction = () => PsycheTherapy.ApplySession(pawn, Patient);
            yield return apply;
        }
    }
}
