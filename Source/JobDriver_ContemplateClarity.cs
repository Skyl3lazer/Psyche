using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psyche
{
    public class JobDriver_ContemplateClarity : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => !PsycheClarityWindows.HasWindow(pawn));

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

                bool success = PsycheClarityWindows.ResolveAttempt(pawn, window, PsycheClarityWindows.SoloQuality(pawn));
                if (success)
                {
                    Messages.Message("Psyche_ClarityFormed".Translate(pawn.LabelShort, pawn), pawn, MessageTypeDefOf.PositiveEvent, false);
                }
                else
                {
                    Messages.Message("Psyche_ClarityFailed".Translate(pawn.LabelShort, pawn), pawn, MessageTypeDefOf.NeutralEvent, false);
                }
            };
            yield return resolve;
        }
    }
}
