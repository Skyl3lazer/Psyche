namespace Psyche
{
    // Mutable (not const) so the options page can adjust them at runtime.
    public static class PsycheTuning
    {
        public static float WoundScale = 1f;
        public static float QualifyingMoodThreshold = 8f;
        public static float QualifyingDurationDays = 5f;
        public static int ConvertedStackLimit = 5;

        public static float ScarBaseChance = 0.50f;
        public static float ScarFloorChance = 0.10f;
        public static float UpkeepChanceReduction = 0.30f;
        public static float TreatmentChanceReduction = 0.30f;

        public static float ScarScale = 0.35f;
        public static float ScarSizeHeavy = 1.0f;
        public static float ScarSizeLight = 0.4f;

        public static float BoonDecayFloor = 0.5f;
        public static float ClarityMinDurationDays = 10f;
        public static float ClarityChancePerMagnitude = 0.005f;
        public static float ClarityChanceCap = 0.5f;
        public static float ClarityScale = 0.35f;
        public static float ClarityCapHP = 30f;
        public static float MaxHealthFloorFrac = 0.1f;

        public static float TreatmentPerSessionBase = 0.25f;
        public static float TreatmentPerSocialLevel = 0.02f;
        public static float HealFracBase = 0.25f;
        public static float HealFracPerSocialLevel = 0.015f;

        public static float MedicationChanceReduction = 0.15f;
        public static float MedicationPerDose = 0.35f;

        public static float ClosureChanceReduction = 0.40f;
        public static float ClosureSizeQualityBonus = 0.5f;
        public static float ClosureHealFrac = 0.5f;
        public static float BurialBaseQuality = 0.35f;
        public static float BittersweetAgeFrac = 0.85f;
        public static float ClarityPayoffChancePerQuality = 0.10f;
        public static float ClosureClaritySize = 2f;

        public static float RevengeQuality = 0.8f;
        public static float ClosureScarHeal = 3f;
        public static float RevengeClarityChance = 0.15f;

        public static float LimbReplacedQuality = 0.8f;
        public static float LimbReplacedClarityChance = 0.15f;

        public static float CharityClosureQualityEssential = 0.8f;
        public static float CharityClosureQualityImportant = 0.5f;
        public static float CharityClosureQualityWorthwhile = 0.3f;
        public static float CaptivityClosureQuality = 0.7f;

        public static float OwnTriggerIntensityCap = 40f;
        public static float StackCapFactor = 1.5f;
        public static float ClarityMinIntensity = 10f;
        public static int GraveWoundRelationOpinion = 20;
        public static float LifeThreateningBleedRate = 0.3f;
        public static float SavedAllyBloodScaleMin = 0.2f;
        public static float SavedAllyBloodScaleMax = 1.0f;
        public static float SavedPrisonerFactor = 0.4f;

        public static float InjuryTreatMagnitudeThreshold = 4f;
        public static float InjuryHealedEpsilon = 0.5f;
        public static int InjuryTreatCooldownTicks = 40000;

        public static int TherapySessionTicks = 600;
        public static int SeekWaitTicks = 1500;
        public static int TherapyRendezvousWaitTicks = 2500;
        public static int CounselingCooldownTicks = 60000;
        public static int SeekWanderRadius = 3;
        public static int SeekWanderIntervalMin = 60;
        public static int SeekWanderIntervalMax = 180;

        public static float[] TierRepairReach = { 2f, 4f, 7f, 99999f };
        public static float[] TierRepairMagnitude = { 1.0f, 1.5f, 2.25f, 3.0f };
        public static float[] TierQualityBase = { 0.05f, 0.10f, 0.15f, 0.20f };
        public static float[] TierMedicinePotencyCap = { 0f, 0.75f, 1.25f, 99999f };

        public static float RepairZeroPoint = 0.20f;
        public static float RepairQualityPerSocial = 0.03f;
        public static float RepairQualityRandomSpread = 0.10f;
        public static float RepairMedicineFloorPerPotency = 0.25f;
        public static float RepairBackfireScale = 0.5f;

        public static float ClarityWindowDeepThreshold = 4f;
        public static float ClaritySeekSoloQualityBase = 0.25f;
        public static float ClaritySeekSoloQualityPerSocial = 0.02f;
        public static float ClarityFromWindowScale = 1f;
        public static int ClarityGlitterCost = 3;
        public static int ContemplationTicks = 4000;

        public static float[] PyromaniaFireFactorByStage = { 0.3f, 0.7f, 1.0f };
    }
}
