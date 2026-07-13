using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public class Thought_Psyche : Thought_Memory
    {
        private float capturedMagnitude = -1f;
        private float mitigationSum;
        private int mitigationSamples;
        private float treatmentLevel;
        private float healPulse;
        private int lastTreatedTick = -1;
        private bool rolled;

        public override float MoodOffset()
        {
            return PsycheUtility.IsTracked(pawn) ? 0f : base.MoodOffset();
        }

        public float RawMagnitude
        {
            get
            {
                EnsureCaptured();
                return capturedMagnitude;
            }
        }

        public float CurrentPsycheDamage
        {
            get
            {
                int duration = DurationTicks;
                float decayed = duration <= 0 ? RawMagnitude : RawMagnitude * Mathf.Clamp01(1f - ((float)age / duration));
                return Mathf.Max(0f, decayed - healPulse);
            }
        }

        public bool CanBeTreated =>
            RawMagnitude >= PsycheTuning.InjuryTreatMagnitudeThreshold
            && CurrentPsycheDamage > PsycheTuning.InjuryHealedEpsilon
            && (lastTreatedTick < 0 || Find.TickManager.TicksGame - lastTreatedTick >= PsycheTuning.InjuryTreatCooldownTicks);

        public void EnsureCaptured()
        {
            if (capturedMagnitude < 0f)
            {
                capturedMagnitude = Mathf.Abs(base.MoodOffset()) * PsycheTuning.WoundScale;
            }
        }

        public void Treat(int socialLevel)
        {
            treatmentLevel = Mathf.Min(1f, treatmentLevel + PsycheTuning.TreatmentPerSessionBase + (socialLevel * PsycheTuning.TreatmentPerSocialLevel));
            healPulse += RawMagnitude * (PsycheTuning.HealFracBase + (socialLevel * PsycheTuning.HealFracPerSocialLevel));
            lastTreatedTick = Find.TickManager.TicksGame;
        }

        public override void ThoughtInterval()
        {
            base.ThoughtInterval();
            mitigationSum += SampleMitigation();
            mitigationSamples++;
        }

        public void RollScarOnExpiry()
        {
            if (rolled)
            {
                return;
            }

            rolled = true;
            float upkeep = mitigationSamples > 0 ? mitigationSum / mitigationSamples : 0.5f;
            PsycheScars.TryForm(pawn, RawMagnitude, upkeep, Mathf.Clamp01(treatmentLevel));
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
            Scribe_Values.Look(ref capturedMagnitude, "psyche_capturedMagnitude", -1f);
            Scribe_Values.Look(ref mitigationSum, "psyche_mitigationSum", 0f);
            Scribe_Values.Look(ref mitigationSamples, "psyche_mitigationSamples", 0);
            Scribe_Values.Look(ref treatmentLevel, "psyche_treatmentLevel", 0f);
            Scribe_Values.Look(ref healPulse, "psyche_healPulse", 0f);
            Scribe_Values.Look(ref lastTreatedTick, "psyche_lastTreatedTick", -1);
            Scribe_Values.Look(ref rolled, "psyche_rolled", false);
        }
    }
}
