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

        public static bool HasRepairableScar(Pawn pawn) => WorstScar(pawn) != null;

        public static bool CanReceiveCounseling(Pawn pawn) =>
            pawn.needs?.TryGetNeed<Need_Psyche>()?.CanReceiveCounseling ?? false;

        public static Thought_Psyche? WorstTreatableInjury(Pawn pawn)
        {
            List<Thought_Memory>? memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
            if (memories == null)
            {
                return null;
            }

            Thought_Psyche? worst = null;
            for (int i = 0; i < memories.Count; i++)
            {
                if (memories[i] is Thought_Psyche pt && pt.CanBeTreated && (worst == null || pt.CurrentPsycheDamage > worst.CurrentPsycheDamage))
                {
                    worst = pt;
                }
            }

            return worst;
        }

        public static bool HasTreatableInjury(Pawn pawn) => WorstTreatableInjury(pawn) != null;

        public static bool NeedsCare(Pawn pawn) =>
            HasTreatableInjury(pawn) || (HasRepairableScar(pawn) && CanReceiveCounseling(pawn));

        public static void ApplySession(Pawn counselor, Pawn patient)
        {
            Thought_Psyche? injury = WorstTreatableInjury(patient);
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

            Hediff? scar = WorstScar(patient);
            if (scar == null)
            {
                return;
            }

            float amount = (PsycheTuning.RepairPerSessionBase + (SocialLevel(counselor) * PsycheTuning.RepairPerSocialLevel)) * Rand.Range(0.75f, 1.25f);
            scar.Severity -= amount;
            if (scar.Severity <= 0f)
            {
                patient.health.RemoveHediff(scar);
            }

            Need_Psyche? need = patient.needs?.TryGetNeed<Need_Psyche>();
            need?.Recompute();
            need?.NotifyCounseled();
        }

        public static IntVec3 PickRendezvous(Pawn pawn)
        {
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
