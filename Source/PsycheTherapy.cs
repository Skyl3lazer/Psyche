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
            patient.Map.reservationManager.OnlyReservationsForJobDef(patient, PsycheDefOf.Psyche_AdministerTherapy, requireAtLeastOne: true);

        // Off by default is vanilla's doing: enabled non-exclusive interactions start as an empty list.
        public static bool TherapyPermitted(Pawn patient) =>
            !patient.IsPrisonerOfColony || (patient.guest?.IsInteractionEnabled(PsycheDefOf.Psyche_GiveTherapy) ?? false);

        // Reserving the patient for a session locks out the doctor, so anything a medic owes them wins.
        public static bool MedicalCarePending(Pawn patient) =>
            HealthAIUtility.WantsToBeRescued(patient)
            || HealthAIUtility.ShouldBeTendedNowByPlayer(patient)
            || patient.health.hediffSet.InLabor()
            || HealthAIUtility.ShouldHaveSurgeryDoneNow(patient);

        public static bool IsTherapyCandidate(Pawn patient)
        {
            if (patient == null || !PsycheUtility.IsTracked(patient) || !TherapyPermitted(patient) || !NeedsCare(patient))
            {
                return false;
            }

            if (MedicalCarePending(patient))
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
            Building_Bed? occupied = pawn.CurrentBed();
            if (occupied != null && RendezvousUsable(pawn, occupied.Position))
            {
                return occupied.Position;
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
                if (RendezvousUsable(pawn, candidates[i]))
                {
                    valid.Add(candidates[i]);
                }
            }

            return valid.Count > 0 ? valid.RandomElement() : IntVec3.Invalid;
        }

        public static bool RendezvousUsable(Pawn patient, IntVec3 cell) =>
            cell.IsValid && patient.Map != null && cell.InBounds(patient.Map) && cell.Standable(patient.Map)
            && patient.CanReach(cell, PathEndMode.OnCell, Danger.None);

        // Memoized so the patient and the counselor walk to the same cell across separate think-tree passes.
        public static IntVec3 EnsureRendezvous(Pawn patient)
        {
            Need_Psyche? need = patient.needs?.TryGetNeed<Need_Psyche>();
            if (need == null)
            {
                return IntVec3.Invalid;
            }

            if (RendezvousUsable(patient, need.TherapyRendezvous))
            {
                return need.TherapyRendezvous;
            }

            need.TherapyRendezvous = PickRendezvous(patient);
            return need.TherapyRendezvous;
        }

        public static IntVec3 CurrentRendezvous(Pawn patient) =>
            patient.needs?.TryGetNeed<Need_Psyche>()?.TherapyRendezvous ?? IntVec3.Invalid;

        public static void ClearRendezvous(Pawn? patient)
        {
            Need_Psyche? need = patient?.needs?.TryGetNeed<Need_Psyche>();
            if (need != null)
            {
                need.TherapyRendezvous = IntVec3.Invalid;
            }
        }

        public static Pawn? Counselor(Pawn patient)
        {
            if (patient.Map == null)
            {
                return null;
            }

            List<ReservationManager.Reservation> reservations = patient.Map.reservationManager.ReservationsReadOnly;
            for (int i = 0; i < reservations.Count; i++)
            {
                ReservationManager.Reservation r = reservations[i];
                if (r.Target.Thing == patient && r.Job?.def == PsycheDefOf.Psyche_AdministerTherapy)
                {
                    return r.Claimant;
                }
            }

            return null;
        }

        // The session may only start where the patient chose to be, never on the move.
        public static bool AtRendezvous(Pawn patient)
        {
            if (!patient.Spawned)
            {
                return false;
            }

            if (patient.InBed())
            {
                return true;
            }

            IntVec3 spot = CurrentRendezvous(patient);
            if (!spot.IsValid || !patient.Position.InHorDistOf(spot, PsycheTuning.SeekWanderRadius + 1))
            {
                return false;
            }

            // Proximity alone is met while the patient is still walking past on the way in.
            return patient.jobs?.curDriver is JobDriver_SeekTherapy seek && seek.ReachedRendezvous;
        }

        private static int SocialLevel(Pawn pawn) => pawn.skills?.GetSkill(SkillDefOf.Social)?.Level ?? 0;
    }
}
