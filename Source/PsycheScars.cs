using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Psyche
{
    public static class PsycheScars
    {
        public static void TryForm(Pawn pawn, float magnitude, float upkeep, float treatment, float medication, float closure, int subjectId, int killerId, HediffDef? markDef = null, IEnumerable<int>? lostParts = null)
        {
            float chance = Mathf.Clamp(
                PsycheTuning.ScarBaseChance
                    - (upkeep * PsycheTuning.UpkeepChanceReduction)
                    - (treatment * PsycheTuning.TreatmentChanceReduction)
                    - (medication * PsycheTuning.MedicationChanceReduction)
                    - (closure * PsycheTuning.ClosureChanceReduction),
                PsycheTuning.ScarFloorChance,
                PsycheTuning.ScarBaseChance);

            if (!Rand.Chance(chance))
            {
                return;
            }

            float quality = Mathf.Clamp01(((upkeep + treatment) / 2f) + (closure * PsycheTuning.ClosureSizeQualityBonus));
            float sizeWeight = Mathf.Lerp(PsycheTuning.ScarSizeHeavy, PsycheTuning.ScarSizeLight, quality);
            float size = magnitude * PsycheTuning.ScarScale * sizeWeight;

            BodyPartRecord? brain = pawn.health.hediffSet.GetBrain();
            if (brain == null)
            {
                return;
            }

            HediffDef def = markDef ?? (subjectId != 0 ? PsycheDefOf.Psyche_Scar_Grief : PsycheDefOf.Psyche_Scar);
            Hediff scar = HediffMaker.MakeHediff(def, pawn, brain);
            scar.Severity = size;
            if (scar is Hediff_PsycheScar psycheScar)
            {
                psycheScar.NotePeak();
                psycheScar.TryGetComp<HediffComp_GriefOrigin>()?.SetOrigin(subjectId, killerId);
                psycheScar.TryGetComp<HediffComp_LimbLoss>()?.SetLostParts(lostParts);
            }

            pawn.health.AddHediff(scar, brain);
            pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }
    }
}
