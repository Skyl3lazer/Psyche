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

        public static void ApplySession(Pawn counselor, Pawn patient)
        {
            Hediff? scar = WorstScar(patient);
            if (scar == null)
            {
                return;
            }

            int social = counselor.skills?.GetSkill(SkillDefOf.Social)?.Level ?? 0;
            float amount = (PsycheTuning.RepairPerSessionBase + (social * PsycheTuning.RepairPerSocialLevel)) * Rand.Range(0.75f, 1.25f);

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
    }
}
