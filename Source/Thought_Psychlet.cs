using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public enum InjuryTreatState
    {
        NotApplicable,
        EligibleNow,
        OnCooldown,
        TooMinor,
        Faded,
    }

    public class Thought_Psychlet : Thought_Memory
    {
        private float signedMagnitude;
        private float initialUnit;
        private bool scalingIntensity;
        private LimbLossData? limbLoss;
        private bool captured;
        private float mitigationSum;
        private int mitigationSamples;
        private float treatmentLevel;
        private float medicationLevel;
        private float closureLevel;
        private float closureHeal;
        private float healPulse;
        private int lastTreatedTick = -1;
        private int killerId;
        private bool rolled;
        private ThoughtDef? sourceDef;
        private int sourceStageIndex;

        public ThoughtDef DisplayDef => sourceDef ?? def;

        public void SetSource(ThoughtDef source, int stageIndex)
        {
            sourceDef = source;
            sourceStageIndex = stageIndex;
        }

        private ThoughtStage DisplayStage
        {
            get
            {
                ThoughtDef d = DisplayDef;
                int idx = sourceDef != null ? sourceStageIndex : CurStageIndex;
                return d.stages[Mathf.Clamp(idx, 0, d.stages.Count - 1)];
            }
        }

        public override string LabelCap =>
            otherPawn != null
                ? DisplayStage.label.Formatted(otherPawn.LabelShort, otherPawn).CapitalizeFirst()
                : DisplayStage.label.CapitalizeFirst();

        public override string Description =>
            otherPawn != null
                ? DisplayStage.description.Formatted(otherPawn.LabelShort, otherPawn).Resolve()
                : DisplayStage.description;

        public override int DurationTicks => Mathf.RoundToInt(DisplayDef.durationDays * 60000f);

        public override bool GroupsWith(Thought other)
        {
            if (other is Thought_Psychlet o && (sourceDef != null || o.sourceDef != null))
            {
                return sourceDef == o.sourceDef && base.GroupsWith(other);
            }

            return base.GroupsWith(other);
        }

        public override float MoodOffset()
        {
            return PsycheUtility.HasPsyche(pawn) ? 0f : base.MoodOffset();
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

        public float SortValue => IsBoon ? CurrentBoonBonus : -CurrentPsycheDamage;

        public bool CanBeTreated => TreatState == InjuryTreatState.EligibleNow;

        public int TicksUntilExpiry => Mathf.Max(0, DurationTicks - age);

        public bool MarkPossible =>
            IsBoon
                ? DisplayDef.GetModExtension<PsycheThoughtExtension>()?.exemptFromClarities != true
                    && DisplayDef.durationDays >= PsycheTuning.ClarityMinDurationDays
                    && RawMagnitude >= PsycheTuning.ClarityMinIntensity
                : true;

        public float TreatmentLevel => Mathf.Clamp01(treatmentLevel);

        public int TicksUntilTreatable =>
            lastTreatedTick < 0 ? 0 : Mathf.Max(0, lastTreatedTick + PsycheTuning.InjuryTreatCooldownTicks - Find.TickManager.TicksGame);

        public InjuryTreatState TreatState
        {
            get
            {
                if (IsBoon)
                {
                    return InjuryTreatState.NotApplicable;
                }

                if (RawMagnitude < PsycheTuning.InjuryTreatMagnitudeThreshold)
                {
                    return InjuryTreatState.TooMinor;
                }

                if (CurrentPsycheDamage <= PsycheTuning.InjuryHealedEpsilon)
                {
                    return InjuryTreatState.Faded;
                }

                return TicksUntilTreatable > 0 ? InjuryTreatState.OnCooldown : InjuryTreatState.EligibleNow;
            }
        }

        public void EnsureCaptured()
        {
            if (!captured)
            {
                signedMagnitude = base.MoodOffset() * PsycheTuning.WoundScale;
                initialUnit = Mathf.Abs(signedMagnitude);
                // This path only fires for a carrier sized off vanilla's mood, never an authored unit.
                scalingIntensity = true;
                captured = true;
            }
        }

        public void InitMagnitude(float signed, bool scaling)
        {
            signedMagnitude = signed;
            initialUnit = Mathf.Abs(signed);
            scalingIntensity = scaling;
            captured = true;
        }

        private float InitialUnit => initialUnit > 0f ? initialUnit : Mathf.Abs(signedMagnitude);

        private float StackCap =>
            scalingIntensity
                ? PsycheTuning.OwnTriggerIntensityCap
                : Mathf.Min(InitialUnit * PsycheTuning.StackCapFactor, PsycheTuning.OwnTriggerIntensityCap);

        // Clamping the stored magnitude rather than the read value is what makes decay restart from
        // the capped size instead of coasting down to it.
        public void Intensify(float signedDelta)
        {
            EnsureCaptured();
            float cap = StackCap;
            signedMagnitude = Mathf.Clamp(signedMagnitude + signedDelta, -cap, cap);
            age = 0;
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

        public float ClosureHeal => closureHeal;

        public void Close(float quality)
        {
            if (IsBoon)
            {
                return;
            }

            closureLevel = Mathf.Max(closureLevel, quality);
            float heal = RawMagnitude * quality * PsycheTuning.ClosureHealFrac;
            closureHeal += heal;
            healPulse += heal;
        }

        public int KillerId => killerId;

        public int LostPartCount => limbLoss?.Count ?? 0;

        public bool HasLostPart(int partIndex) => limbLoss?.Has(partIndex) ?? false;

        public void AddLostPart(int partIndex) => (limbLoss ??= new LimbLossData()).Add(partIndex);

        public bool RemoveLostPart(int partIndex) => limbLoss?.Remove(partIndex) ?? false;

        public void StampKiller(int id)
        {
            if (!IsBoon)
            {
                killerId = id;
            }
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

            PsycheThoughtExtension? ext = DisplayDef.GetModExtension<PsycheThoughtExtension>();

            if (IsBoon)
            {
                if (MarkPossible)
                {
                    float chance = Mathf.Min(RawMagnitude * PsycheTuning.ClarityChancePerMagnitude, PsycheTuning.ClarityChanceCap);
                    if (Rand.Chance(chance))
                    {
                        PsycheClarities.TryForm(pawn, RawMagnitude, ext?.clarityDef);
                    }
                }

                return;
            }

            float upkeep = mitigationSamples > 0 ? mitigationSum / mitigationSamples : 0.5f;
            HediffDef? scarDef = PsycheThoughtClassification.ResolveScarDef(DisplayDef);
            PsycheScars.TryForm(pawn, RawMagnitude, upkeep, Mathf.Clamp01(treatmentLevel), Mathf.Clamp01(medicationLevel), Mathf.Clamp01(closureLevel), otherPawn?.thingIDNumber ?? 0, killerId, scarDef, limbLoss?.Parts);
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
            Scribe_Values.Look(ref initialUnit, "psyche_initialUnit", 0f);
            Scribe_Values.Look(ref scalingIntensity, "psyche_scalingIntensity", false);
            Scribe_Values.Look(ref captured, "psyche_captured", false);
            Scribe_Values.Look(ref mitigationSum, "psyche_mitigationSum", 0f);
            Scribe_Values.Look(ref mitigationSamples, "psyche_mitigationSamples", 0);
            Scribe_Values.Look(ref treatmentLevel, "psyche_treatmentLevel", 0f);
            Scribe_Values.Look(ref medicationLevel, "psyche_medicationLevel", 0f);
            Scribe_Values.Look(ref closureLevel, "psyche_closureLevel", 0f);
            Scribe_Values.Look(ref closureHeal, "psyche_closureHeal", 0f);
            Scribe_Values.Look(ref killerId, "psyche_killerId", 0);
            Scribe_Values.Look(ref healPulse, "psyche_healPulse", 0f);
            Scribe_Values.Look(ref lastTreatedTick, "psyche_lastTreatedTick", -1);
            Scribe_Values.Look(ref rolled, "psyche_rolled", false);
            Scribe_Defs.Look(ref sourceDef, "psyche_sourceDef");
            Scribe_Values.Look(ref sourceStageIndex, "psyche_sourceStageIndex", 0);
            Scribe_Deep.Look(ref limbLoss, "psyche_limbLoss");
        }
    }
}
