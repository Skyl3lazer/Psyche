using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psyche
{
    public static class PsycheTherapy
    {
        public static Hediff? WorstScar(Pawn pawn)
        {
            Hediff? worst = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff h = hediffs[i];
                if (h.def == PsycheDefOf.Psyche_Scar && h.Severity > 0f && (worst == null || h.Severity > worst.Severity))
                {
                    worst = h;
                }
            }

            return worst;
        }

        public static bool HasRepairableScar(Pawn pawn) => PsycheRepair.HasReducibleScar(pawn);

        public static bool CanReceiveCounseling(Pawn pawn) =>
            pawn.needs?.TryGetNeed<Need_Psyche>()?.CanReceiveCounseling ?? false;

        public static bool RepairMedicineSatisfied(Pawn patient)
        {
            int tier = PsycheRepair.BestResearchedTier();
            if (!PsycheMedicine.MedicinePossible(patient, tier) || PsycheMod.Settings.attemptTherapyWithoutBestMedicine)
            {
                return true;
            }

            return PsycheMedicine.AnyUsableOnMap(patient, tier);
        }

        public static bool RepairAvailable(Pawn pawn) =>
            HasRepairableScar(pawn) && CanReceiveCounseling(pawn) && RepairMedicineSatisfied(pawn);

        public static Thought_Psychlet? WorstTreatableInjury(Pawn pawn)
        {
            List<Thought_Memory>? memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
            if (memories == null)
            {
                return null;
            }

            Thought_Psychlet? worst = null;
            for (int i = 0; i < memories.Count; i++)
            {
                if (memories[i] is Thought_Psychlet pt && pt.CanBeTreated && (worst == null || pt.CurrentPsycheDamage > worst.CurrentPsycheDamage))
                {
                    worst = pt;
                }
            }

            return worst;
        }

        public static bool HasTreatableInjury(Pawn pawn) => WorstTreatableInjury(pawn) != null;

        public static bool NeedsCare(Pawn pawn) =>
            HasTreatableInjury(pawn) || RepairAvailable(pawn);

        public static bool IsClaimed(Pawn patient) =>
            patient?.Map != null && patient.Map.reservationManager.OnlyReservationsForJobDef(patient, PsycheDefOf.Psyche_AdministerTherapy, requireAtLeastOne: true);

        public static bool IsTherapyCandidate(Pawn patient)
        {
            if (patient == null || !PsycheUtility.IsTracked(patient) || !NeedsCare(patient))
            {
                return false;
            }

            return patient.CurJob == null || patient.CurJob.def.suspendable;
        }

        public static float SeverityScore(Pawn patient)
        {
            Thought_Psychlet? injury = WorstTreatableInjury(patient);
            if (injury != null)
            {
                return 1000f + injury.CurrentPsycheDamage;
            }

            return PsycheRepair.WorstReducibleScar(patient)?.Severity ?? 0f;
        }

        public static void ApplySession(Pawn counselor, Pawn patient)
        {
            Thought_Psychlet? injury = WorstTreatableInjury(patient);
            if (injury != null)
            {
                injury.Treat(SocialLevel(counselor));
                patient.needs?.TryGetNeed<Need_Psyche>()?.Recompute();
                return;
            }

            RepairScar(counselor, patient);
        }

        public static void RepairScar(Pawn counselor, Pawn patient)
        {
            if (!CanReceiveCounseling(patient))
            {
                return;
            }

            Hediff_PsycheScar? scar = PsycheRepair.WorstReducibleScar(patient);
            if (scar == null)
            {
                return;
            }

            PsycheRepair.ApplyRepairSession(counselor, patient, scar);

            Need_Psyche? need = patient.needs?.TryGetNeed<Need_Psyche>();
            need?.Recompute();
            need?.NotifyCounseled();
        }

        public static IntVec3 PickRendezvous(Pawn pawn)
        {
            if (pawn?.Map == null)
            {
                return IntVec3.Invalid;
            }

            List<IntVec3> candidates = new List<IntVec3>();

            Building_Bed? ownedBed = pawn.ownership?.OwnedBed;
            if (ownedBed != null && ownedBed.Spawned)
            {
                candidates.Add(ownedBed.Position);
            }

            foreach (Building_Bed bed in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>())
            {
                if (bed.Medical)
                {
                    candidates.Add(bed.Position);
                    break;
                }
            }

            MeditationSpotAndFocus med = MeditationUtility.FindMeditationSpot(pawn);
            if (med.IsValid)
            {
                candidates.Add(med.spot.Cell);
            }

            List<IntVec3> valid = new List<IntVec3>();
            for (int i = 0; i < candidates.Count; i++)
            {
                IntVec3 c = candidates[i];
                if (c.Standable(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.None))
                {
                    valid.Add(c);
                }
            }

            return valid.Count > 0 ? valid.RandomElement() : IntVec3.Invalid;
        }

        private static int SocialLevel(Pawn pawn) => pawn.skills?.GetSkill(SkillDefOf.Social)?.Level ?? 0;
    }
}
