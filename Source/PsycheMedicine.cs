using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psyche
{
    public static class PsycheMedicine
    {
        public static float PotencyOf(ThingDef medDef) =>
            medDef?.GetStatValueAbstract(StatDefOf.MedicalPotency) ?? 0f;

        public static float QualityFloor(Thing? medicine) =>
            medicine == null ? 0f : PotencyOf(medicine.def) * PsycheTuning.RepairMedicineFloorPerPotency;

        public static bool MedicinePossible(Pawn patient, int tier)
        {
            if (PsycheTuning.TierMedicinePotencyCap[tier - 1] <= 0f)
            {
                return false;
            }

            MedicalCareCategory care = patient.playerSettings?.medCare ?? MedicalCareCategory.NoMeds;
            return care != MedicalCareCategory.NoCare && care != MedicalCareCategory.NoMeds;
        }

        public static Thing? FindBestUsable(Pawn counselor, Pawn patient, int tier)
        {
            float tierCap = PsycheTuning.TierMedicinePotencyCap[tier - 1];
            if (tierCap <= 0f || counselor.Map == null)
            {
                return null;
            }

            MedicalCareCategory care = patient.playerSettings?.medCare ?? MedicalCareCategory.NoMeds;

            Thing? best = null;
            float bestPotency = -1f;
            List<Thing> meds = counselor.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine);
            for (int i = 0; i < meds.Count; i++)
            {
                Thing med = meds[i];
                float potency = PotencyOf(med.def);
                if (potency > tierCap || potency <= bestPotency || med.IsForbidden(counselor))
                {
                    continue;
                }

                if (!care.AllowsMedicine(med.def))
                {
                    continue;
                }

                if (!counselor.CanReserveAndReach(med, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, false))
                {
                    continue;
                }

                best = med;
                bestPotency = potency;
            }

            return best;
        }

        public static bool AnyUsableOnMap(Pawn patient, int tier)
        {
            float tierCap = PsycheTuning.TierMedicinePotencyCap[tier - 1];
            if (tierCap <= 0f || patient.Map == null)
            {
                return false;
            }

            MedicalCareCategory care = patient.playerSettings?.medCare ?? MedicalCareCategory.NoMeds;
            List<Thing> meds = patient.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine);
            for (int i = 0; i < meds.Count; i++)
            {
                Thing med = meds[i];
                if (PotencyOf(med.def) <= tierCap && !med.IsForbidden(Faction.OfPlayer) && care.AllowsMedicine(med.def))
                {
                    return true;
                }
            }

            return false;
        }

        public static void ConsumeOne(Thing medicine)
        {
            if (medicine == null || medicine.Destroyed)
            {
                return;
            }

            medicine.SplitOff(1).Destroy();
        }
    }
}
