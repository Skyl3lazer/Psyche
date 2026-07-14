using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Psyche
{
    [HarmonyPatch(typeof(PawnRitualRoleSelectionWidget), "ExtraTipContents")]
    public static class Patch_RitualCounselorTip
    {
        private static readonly AccessTools.FieldRef<object, Precept_Ritual> RitualField =
            AccessTools.FieldRefAccess<Precept_Ritual>(typeof(PawnRitualRoleSelectionWidget), "ritual");

        private static readonly AccessTools.FieldRef<object, RitualRoleAssignments> AssignmentsField =
            AccessTools.FieldRefAccess<RitualRoleAssignments>(typeof(PawnRitualRoleSelectionWidget), "ritualAssignments");

        public static void Postfix(PawnRitualRoleSelectionWidget __instance, Pawn pawn, ref string __result)
        {
            Precept_Ritual ritual = RitualField(__instance);
            if (ritual?.def == null || ritual.def.defName != "Psyche_ClarityContemplation")
            {
                return;
            }

            Pawn seeker = AssignmentsField(__instance)?.FirstAssignedPawn("seeker");
            if (seeker == null || pawn == seeker)
            {
                return;
            }

            RitualOutcomeComp_PsycheCounselor? comp = ritual.outcomeEffect?.def?.comps?
                .OfType<RitualOutcomeComp_PsycheCounselor>().FirstOrDefault();
            if (comp?.curve == null)
            {
                return;
            }

            float offset = comp.curve.Evaluate(PsycheClarityRitual.CounselorScore(pawn, seeker));
            if (offset <= 0f)
            {
                return;
            }

            string line = "  - " + "Psyche_AsCounselor".Translate("+" + offset.ToStringPercent());
            __result = string.IsNullOrEmpty(__result) ? line : __result + "\n" + line;
        }
    }
}
