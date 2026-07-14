using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public class Thought_Psychlet : Thought_Memory
    {
        private float signedMagnitude;
        private bool captured;
        private float mitigationSum;
        private int mitigationSamples;
        private float treatmentLevel;
        private float medicationLevel;
        private float healPulse;
        private int lastTreatedTick = -1;
        private bool rolled;

        public override float MoodOffset()
        {
            return PsycheUtility.IsTracked(pawn) ? 0f : base.MoodOffset();
        }

        public bool IsBoon
        {
            get
            {
                EnsureCaptured();
                return signedMagnitude > 0f;
            }
        }

        public float RawMagnitude
        {
            get
            {
                EnsureCaptured();
                return Mathf.Abs(signedMagnitude);
            }
        }

        public float CurrentPsycheDamage
        {
            get
            {
                if (IsBoon)
                {
                    return 0f;
                }

                int duration = DurationTicks;
                float decayed = duration <= 0 ? RawMagnitude : RawMagnitude * Mathf.Clamp01(1f - ((float)age / duration));
                return Mathf.Max(0f, decayed - healPulse);
            }
        }

        public float CurrentBoonBonus
        {
            get
            {
                if (!IsBoon)
                {
                    return 0f;
                }

                int duration = DurationTicks;
                float effectiveness = duration <= 0 ? 1f : Mathf.Lerp(1f, PsycheTuning.BoonDecayFloor, Mathf.Clamp01((float)age / duration));
                return RawMagnitude * effectiveness;
            }
        }

        public bool CanBeTreated =>
            !IsBoon
            && RawMagnitude >= PsycheTuning.InjuryTreatMagnitudeThreshold
            && CurrentPsycheDamage > PsycheTuning.InjuryHealedEpsilon
            && (lastTreatedTick < 0 || Find.TickManager.TicksGame - lastTreatedTick >= PsycheTuning.InjuryTreatCooldownTicks);

        public void EnsureCaptured()
        {
            if (!captured)
            {
                signedMagnitude = base.MoodOffset() * PsycheTuning.WoundScale;
                captured = true;
            }
        }

        public void Treat(int socialLevel)
        {
            if (IsBoon)
            {
                return;
            }

            treatmentLevel = Mathf.Min(1f, treatmentLevel + PsycheTuning.TreatmentPerSessionBase + (socialLevel * PsycheTuning.TreatmentPerSocialLevel));
            healPulse += RawMagnitude * (PsycheTuning.HealFracBase + (socialLevel * PsycheTuning.HealFracPerSocialLevel));
            lastTreatedTick = Find.TickManager.TicksGame;
        }

        public void Medicate(float potency)
        {
            if (IsBoon)
            {
                return;
            }

            medicationLevel = Mathf.Min(1f, medicationLevel + (PsycheTuning.MedicationPerDose * potency));
        }

        public override void ThoughtInterval()
        {
            base.ThoughtInterval();
            if (!IsBoon)
            {
                mitigationSum += SampleMitigation();
                mitigationSamples++;
            }
        }

        public void RollOnExpiry()
        {
            if (rolled)
            {
                return;
            }

            rolled = true;

            if (IsBoon)
            {
                if (def.durationDays >= PsycheTuning.ClarityMinDurationDays)
                {
                    float chance = Mathf.Min(RawMagnitude * PsycheTuning.ClarityChancePerMagnitude, PsycheTuning.ClarityChanceCap);
                    if (Rand.Chance(chance))
                    {
                        PsycheClarities.TryForm(pawn, RawMagnitude);
                    }
                }

                return;
            }

            float upkeep = mitigationSamples > 0 ? mitigationSum / mitigationSamples : 0.5f;
            PsycheScars.TryForm(pawn, RawMagnitude, upkeep, Mathf.Clamp01(treatmentLevel), Mathf.Clamp01(medicationLevel));
        }

        private float SampleMitigation()
        {
            float mood = pawn.needs?.mood?.CurLevelPercentage ?? 0.5f;
            float rest = pawn.needs?.rest?.CurLevelPercentage ?? 0.5f;
            float joy = pawn.needs?.joy?.CurLevelPercentage ?? 0.5f;
            return (mood + rest + joy) / 3f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref signedMagnitude, "psyche_signedMagnitude", 0f);
            Scribe_Values.Look(ref captured, "psyche_captured", false);
            Scribe_Values.Look(ref mitigationSum, "psyche_mitigationSum", 0f);
            Scribe_Values.Look(ref mitigationSamples, "psyche_mitigationSamples", 0);
            Scribe_Values.Look(ref treatmentLevel, "psyche_treatmentLevel", 0f);
            Scribe_Values.Look(ref medicationLevel, "psyche_medicationLevel", 0f);
            Scribe_Values.Look(ref healPulse, "psyche_healPulse", 0f);
            Scribe_Values.Look(ref lastTreatedTick, "psyche_lastTreatedTick", -1);
            Scribe_Values.Look(ref rolled, "psyche_rolled", false);
        }
    }
}
