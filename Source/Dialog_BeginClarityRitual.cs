using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Psyche
{
    public class Dialog_BeginClarityRitual : Dialog_BeginRitual
    {
        private readonly Pawn? preferredCounselor;
        private readonly Precept_Ritual ritualRef;

        public Dialog_BeginClarityRitual(
            Pawn? preferredCounselor,
            string ritualLabel,
            Precept_Ritual ritual,
            TargetInfo target,
            Map map,
            Dialog_BeginRitual.ActionCallback action,
            Pawn organizer,
            RitualObligation obligation,
            Dialog_BeginRitual.PawnFilter filter,
            string okButtonText,
            List<Pawn> requiredPawns,
            Dictionary<string, Pawn> forcedForRole,
            RitualOutcomeEffectDef outcome,
            List<string> extraInfoText,
            Pawn selectedPawn)
            : base(ritualLabel, ritual, target, map, action, organizer, obligation, filter, okButtonText, requiredPawns, forcedForRole, outcome, extraInfoText, selectedPawn)
        {
            this.preferredCounselor = preferredCounselor;
            ritualRef = ritual;
        }

        public override void PostOpen()
        {
            base.PostOpen();

            if (preferredCounselor == null)
            {
                return;
            }

            RitualRole? role = ritualRef?.behavior?.def?.roles?.FirstOrDefault(r => r.id == "counselor");
            if (role == null || assignments.FirstAssignedPawn("counselor") == preferredCounselor)
            {
                return;
            }

            foreach (Pawn occupant in assignments.AssignedPawns(role).ToList())
            {
                assignments.TryUnassignAnyRole(occupant);
            }

            assignments.TryAssign(preferredCounselor, role, out _);
        }
    }
}
