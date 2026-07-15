namespace Psyche
{
    public static class PsycheTuning
    {
        public const float WoundScale = 1f;
        public const float QualifyingMoodThreshold = 8f;
        public const float QualifyingDurationDays = 5f;
        public const int ConvertedStackLimit = 5;

        public const float ScarBaseChance = 0.50f;
        public const float ScarFloorChance = 0.10f;
        public const float UpkeepChanceReduction = 0.30f;
        public const float TreatmentChanceReduction = 0.30f;

        public const float ScarScale = 0.35f;
        public const float ScarSizeHeavy = 1.0f;
        public const float ScarSizeLight = 0.4f;

        public const float BoonDecayFloor = 0.5f;
        public const float ClarityMinDurationDays = 10f;
        public const float ClarityChancePerMagnitude = 0.005f;
        public const float ClarityChanceCap = 0.5f;
        public const float ClarityScale = 0.35f;
        public const float ClarityCapHP = 30f;
        public const float MaxHealthFloorFrac = 0.1f;

        public const float TreatmentPerSessionBase = 0.25f;
        public const float TreatmentPerSocialLevel = 0.02f;
        public const float HealFracBase = 0.25f;
        public const float HealFracPerSocialLevel = 0.015f;

        public const float MedicationChanceReduction = 0.15f;
        public const float MedicationPerDose = 0.35f;

        public const float ClosureChanceReduction = 0.40f;
        public const float ClosureSizeQualityBonus = 0.5f;
        public const float ClosureHealFrac = 0.5f;
        public const float BurialBaseQuality = 0.35f;
        public const float BittersweetAgeFrac = 0.85f;
        public const float ClarityPayoffChancePerQuality = 0.10f;
        public const float ClosureClaritySize = 2f;

        public const float RevengeQuality = 0.8f;
        public const float ClosureScarHeal = 3f;
        public const float RevengeClarityChance = 0.15f;

        public const float OwnTriggerIntensityCap = 40f;
        public const float ClarityMinIntensity = 10f;
        public const int GraveWoundRelationOpinion = 20;
        public const float LifeThreateningBleedRate = 0.3f;
        public const float SavedAllyBloodScaleMin = 0.2f;
        public const float SavedAllyBloodScaleMax = 1.0f;

        public const float InjuryTreatMagnitudeThreshold = 4f;
        public const float InjuryHealedEpsilon = 0.5f;
        public const int InjuryTreatCooldownTicks = 40000;

        public const int TherapySessionTicks = 600;
        public const int SeekWaitTicks = 1500;
        public const int CounselingCooldownTicks = 60000;

        public static readonly float[] TierRepairReach = { 2f, 4f, 7f, 99999f };
        public static readonly float[] TierRepairMagnitude = { 1.0f, 1.5f, 2.25f, 3.0f };
        public static readonly float[] TierQualityBase = { 0.05f, 0.10f, 0.15f, 0.20f };
        public static readonly float[] TierMedicinePotencyCap = { 0f, 0.75f, 1.25f, 99999f };

        public const float RepairZeroPoint = 0.20f;
        public const float RepairQualityPerSocial = 0.03f;
        public const float RepairQualityRandomSpread = 0.10f;
        public const float RepairMedicineFloorPerPotency = 0.25f;
        public const float RepairBackfireScale = 0.5f;

        public const float ClarityWindowDeepThreshold = 4f;
        public const float ClaritySeekSoloQualityBase = 0.25f;
        public const float ClaritySeekSoloQualityPerSocial = 0.02f;
        public const float ClarityFromWindowScale = 1f;
        public const int ClarityGlitterCost = 3;
        public const int ContemplationTicks = 4000;

        public static readonly float[] PyromaniaFireFactorByStage = { 0.3f, 0.7f, 1.0f };
    }
}
