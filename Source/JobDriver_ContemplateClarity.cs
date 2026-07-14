using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psyche
{
    public class JobDriver_ContemplateClarity : JobDriver
    {
        private Thing Glitter => job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            Glitter == null || pawn.Reserve(Glitter, job, 1, job.count, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => !PsycheClarityWindows.HasWindow(pawn));

            if (Glitter != null)
            {
                yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B);
                yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            }

            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);

            Toil contemplate = Toils_General.Wait(PsycheTuning.ContemplationTicks);
            contemplate.WithProgressBarToilDelay(TargetIndex.A);
            contemplate.activeSkill = () => SkillDefOf.Social;
            yield return contemplate;

            Toil resolve = ToilMaker.MakeToil();
            resolve.initAction = () =>
            {
                Thought_ClarityWindow? window = PsycheClarityWindows.WorstWindow(pawn);
                if (window == null)
                {
                    return;
                }

                bool success = PsycheClarityWindows.ResolveAttempt(pawn, window, PsycheClarityWindows.SoloQuality(pawn), consumeFromMap: false);
                if (success)
                {
                    pawn.carryTracker.CarriedThing?.Destroy();
                    Messages.Message("Psyche_ClarityFormed".Translate(pawn.LabelShort, pawn), pawn, MessageTypeDefOf.PositiveEvent, false);
                }
                else
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out Thing _);
                    Messages.Message("Psyche_ClarityFailed".Translate(pawn.LabelShort, pawn), pawn, MessageTypeDefOf.NeutralEvent, false);
                }
            };
            yield return resolve;
        }
    }
}
