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

        public const float InjuryTreatMagnitudeThreshold = 4f;
        public const float InjuryHealedEpsilon = 0.5f;
        public const int InjuryTreatCooldownTicks = 40000;

        public const float RepairPerSessionBase = 2f;
        public const float RepairPerSocialLevel = 0.5f;
        public const int TherapySessionTicks = 600;
        public const int SeekWaitTicks = 1500;
        public const int CounselingCooldownTicks = 60000;
    }
}
