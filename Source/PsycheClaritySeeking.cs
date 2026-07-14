using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Psyche
{
    [StaticConstructorOnStartup]
    public static class PsycheClaritySeeking
    {
        private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Commands/Draft", false) ?? BaseContent.BadTex;

        public static Command_Action? GizmoFor(Pawn pawn)
        {
            if (!PsycheUtility.IsTracked(pawn) || !pawn.IsColonistPlayerControlled)
            {
                return null;
            }

            Thought_ClarityWindow? window = PsycheClarityWindows.WorstWindow(pawn);
            if (window == null)
            {
                return null;
            }

            Command_Action command = new Command_Action
            {
                defaultLabel = "Psyche_SeekClarity_Label".Translate(window.ScarBandLabel),
                defaultDesc = "Psyche_SeekClarity_Desc".Translate(PsycheTuning.ClarityGlitterCost),
                icon = Icon,
                action = () => Begin(pawn),
            };

            if (!PsycheClarityWindows.CanAfford(pawn))
            {
                command.Disable("Psyche_SeekClarity_NoGlitter".Translate(PsycheTuning.ClarityGlitterCost));
            }

            return command;
        }

        public static Command_Action? DevGizmoFor(Pawn pawn)
        {
            if (!Prefs.DevMode || !PsycheUtility.IsTracked(pawn) || !pawn.IsColonistPlayerControlled)
            {
                return null;
            }

            if (PsycheClarityWindows.WorstWindow(pawn) == null)
            {
                return null;
            }

            return new Command_Action
            {
                defaultLabel = "DEV: Seek clarity (solo)",
                defaultDesc = "Development only. Runs the solo contemplation directly, bypassing the ritual.",
                icon = Icon,
                action = () => BeginSolo(pawn),
            };
        }

        public static void Begin(Pawn pawn)
        {
            if (PsycheClarityRitual.TryBegin(pawn))
            {
                return;
            }

            BeginSolo(pawn);
        }

        public static void BeginSolo(Pawn pawn)
        {
            IntVec3 spot = PsycheTherapy.PickRendezvous(pawn);
            if (!spot.IsValid)
            {
                spot = pawn.Position;
            }

            Thing? glitter = PsycheClarityWindows.FindGlitterStack(pawn);
            Job job = JobMaker.MakeJob(PsycheDefOf.Psyche_ContemplateClarity, spot, glitter);
            job.count = PsycheTuning.ClarityGlitterCost;
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }
    }
}
