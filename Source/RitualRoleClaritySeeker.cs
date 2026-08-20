using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Psyche
{
    public class RitualRoleClaritySeeker : RitualRoleColonist
    {
        public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual? ritual = null, RitualRoleAssignments? assignments = null, Precept_Ritual? precept = null, bool skipReason = false)
        {
            if (!base.AppliesToPawn(p, out reason, selectedTarget, ritual, assignments, precept, skipReason))
            {
                return false;
            }

            if (PsycheUtility.IsTracked(p) && PsycheClarityWindows.HasWindow(p))
            {
                return true;
            }

            if (!skipReason)
            {
                reason = "Psyche_SeekClarity_RoleNoWindow".Translate(p.LabelShort, p);
            }

            return false;
        }
    }
}
