using UnityEngine;
using Verse;

namespace Psyche
{
    public static class PsycheScars
    {
        public static void TryForm(Pawn pawn, float magnitude, float upkeep, float treatment, float medication, float closure, int subjectId, int killerId, HediffDef? markDef = null)
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

            Hediff scar = HediffMaker.MakeHediff(markDef ?? PsycheDefOf.Psyche_Scar, pawn, brain);
            scar.Severity = size;
            if (scar is Hediff_PsycheScar psycheScar)
            {
                psycheScar.NotePeak();
                psycheScar.SetOrigin(subjectId, killerId);
            }

            pawn.health.AddHediff(scar, brain);
            pawn.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
        }
    }
}
