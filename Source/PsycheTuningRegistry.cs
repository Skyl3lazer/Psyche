using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Psyche
{
    public sealed class TuningEntry
    {
        public string key = "";
        public string group = "";
        public Func<float> get = null!;
        public Action<float> set = null!;
        public float def;
        public float min;
        public float max;
        public bool advanced;
        public bool slider;
        public bool integer;
        public bool restart;
        public int arrayIndex = -1;

        public string BaseName => arrayIndex < 0 ? key : key.Substring(0, key.IndexOf('.'));
    }

    public static class PsycheTuningRegistry
    {
        public static class G
        {
            public const string Wound = "WoundConversion";
            public const string ScarChance = "ScarChance";
            public const string ScarSeverity = "ScarSeverity";
            public const string Clarity = "Clarity";
            public const string Therapy = "Therapy";
            public const string Medication = "Medication";
            public const string Closure = "Closure";
            public const string Revenge = "Revenge";
            public const string Limb = "LimbReplaced";
            public const string Ideology = "IdeologyClosures";
            public const string OwnTriggers = "OwnTriggers";
            public const string Injury = "InjuryTreatment";
            public const string Timing = "TherapyTiming";
            public const string Repair = "RepairTiers";
            public const string Seeking = "ClaritySeeking";
            public const string Pyromania = "Pyromania";
        }

        public static readonly string[] GroupOrder =
        {
            G.Wound, G.ScarChance, G.ScarSeverity, G.Clarity, G.Therapy, G.Medication,
            G.Closure, G.Revenge, G.Limb, G.Ideology, G.OwnTriggers, G.Injury,
            G.Timing, G.Repair, G.Seeking, G.Pyromania,
        };

        private const float AdvancedMax = 999999f;

        private static List<TuningEntry>? entries;
        public static List<TuningEntry> Entries => entries ??= Build();

        public static void EnsureBuilt() => _ = Entries;

        public static void ApplyOverrides(Dictionary<string, float> overrides)
        {
            foreach (TuningEntry e in Entries)
                e.set(overrides != null && overrides.TryGetValue(e.key, out float v) ? v : e.def);
        }

        private static List<TuningEntry> build = null!;

        private static List<TuningEntry> Build()
        {
            build = new List<TuningEntry>();

            P("WoundScale", G.Wound, 0f, 3f);
            P("QualifyingMoodThreshold", G.Wound, 0f, 20f, restart: true);
            P("QualifyingDurationDays", G.Wound, 0f, 15f, restart: true);

            P("ScarBaseChance", G.ScarChance, 0f, 1f);
            P("ScarFloorChance", G.ScarChance, 0f, 1f);
            P("UpkeepChanceReduction", G.ScarChance, 0f, 1f);
            P("TreatmentChanceReduction", G.ScarChance, 0f, 1f);

            P("ScarScale", G.ScarSeverity, 0f, 2f);
            P("ScarSizeHeavy", G.ScarSeverity, 0f, 2f);
            P("ScarSizeLight", G.ScarSeverity, 0f, 2f);

            P("BoonDecayFloor", G.Clarity, 0f, 1f);
            P("ClarityChanceCap", G.Clarity, 0f, 1f);
            P("ClarityScale", G.Clarity, 0f, 2f);
            P("MaxHealthFloorFrac", G.Clarity, 0f, 1f);

            P("TreatmentPerSessionBase", G.Therapy, 0f, 1f);
            P("HealFracBase", G.Therapy, 0f, 1f);

            P("MedicationChanceReduction", G.Medication, 0f, 1f);
            P("MedicationPerDose", G.Medication, 0f, 1f);

            P("ClosureChanceReduction", G.Closure, 0f, 1f);
            P("ClosureSizeQualityBonus", G.Closure, 0f, 1f);
            P("ClosureHealFrac", G.Closure, 0f, 1f);
            P("BurialBaseQuality", G.Closure, 0f, 1f);
            P("BittersweetAgeFrac", G.Closure, 0f, 1f);
            P("ClarityPayoffChancePerQuality", G.Closure, 0f, 1f);

            P("RevengeQuality", G.Revenge, 0f, 1f);
            P("RevengeClarityChance", G.Revenge, 0f, 1f);

            P("LimbReplacedQuality", G.Limb, 0f, 1f);
            P("LimbReplacedClarityChance", G.Limb, 0f, 1f);

            P("CharityClosureQualityEssential", G.Ideology, 0f, 1f);
            P("CharityClosureQualityImportant", G.Ideology, 0f, 1f);
            P("CharityClosureQualityWorthwhile", G.Ideology, 0f, 1f);
            P("CaptivityClosureQuality", G.Ideology, 0f, 1f);

            A("ConvertedStackLimit", G.Wound);

            A("ClarityMinDurationDays", G.Clarity);
            A("ClarityChancePerMagnitude", G.Clarity);
            A("ClarityCapHP", G.Clarity);
            A("ClarityMinIntensity", G.Clarity);

            A("TreatmentPerSocialLevel", G.Therapy);
            A("HealFracPerSocialLevel", G.Therapy);

            A("ClosureClaritySize", G.Closure);

            A("ClosureScarHeal", G.Revenge);

            A("OwnTriggerIntensityCap", G.OwnTriggers);
            A("GraveWoundRelationOpinion", G.OwnTriggers);
            A("LifeThreateningBleedRate", G.OwnTriggers);
            A("SavedAllyBloodScaleMin", G.OwnTriggers);
            A("SavedAllyBloodScaleMax", G.OwnTriggers);

            A("InjuryTreatMagnitudeThreshold", G.Injury);
            A("InjuryHealedEpsilon", G.Injury);
            A("InjuryTreatCooldownTicks", G.Injury);

            A("TherapySessionTicks", G.Timing);
            A("SeekWaitTicks", G.Timing);
            A("CounselingCooldownTicks", G.Timing);
            A("SeekWanderRadius", G.Timing);
            A("SeekWanderIntervalMin", G.Timing);
            A("SeekWanderIntervalMax", G.Timing);

            Arr("TierRepairReach", G.Repair);
            Arr("TierRepairMagnitude", G.Repair);
            Arr("TierQualityBase", G.Repair);
            Arr("TierMedicinePotencyCap", G.Repair);
            A("RepairZeroPoint", G.Repair);
            A("RepairQualityPerSocial", G.Repair);
            A("RepairQualityRandomSpread", G.Repair);
            A("RepairMedicineFloorPerPotency", G.Repair);
            A("RepairBackfireScale", G.Repair);

            A("ClarityWindowDeepThreshold", G.Seeking);
            A("ClaritySeekSoloQualityBase", G.Seeking);
            A("ClaritySeekSoloQualityPerSocial", G.Seeking);
            A("ClarityFromWindowScale", G.Seeking);
            A("ClarityGlitterCost", G.Seeking);
            A("ContemplationTicks", G.Seeking);

            Arr("PyromaniaFireFactorByStage", G.Pyromania);

            List<TuningEntry> result = build;
            build = null!;
            return result;
        }

        private static FieldInfo Field(string name)
        {
            FieldInfo? fi = typeof(PsycheTuning).GetField(name, BindingFlags.Public | BindingFlags.Static);
            if (fi == null)
                throw new ArgumentException("PsycheTuning has no field " + name);
            return fi;
        }

        private static void P(string name, string group, float min, float max, bool restart = false)
            => Add(name, group, min, max, advanced: false, slider: true, restart);

        private static void A(string name, string group)
            => Add(name, group, 0f, AdvancedMax, advanced: true, slider: false, restart: false);

        private static void Add(string name, string group, float min, float max, bool advanced, bool slider, bool restart)
        {
            FieldInfo fi = Field(name);
            bool isInt = fi.FieldType == typeof(int);
            TuningEntry e = new TuningEntry
            {
                key = name,
                group = group,
                min = min,
                max = max,
                advanced = advanced,
                slider = slider,
                integer = isInt,
                restart = restart,
                get = () => isInt ? (int)fi.GetValue(null) : (float)fi.GetValue(null),
                set = v => fi.SetValue(null, isInt ? (object)Mathf.RoundToInt(v) : v),
            };
            e.def = e.get();
            build.Add(e);
        }

        private static void Arr(string name, string group)
        {
            FieldInfo fi = Field(name);
            float[] arr = (float[])fi.GetValue(null);
            for (int i = 0; i < arr.Length; i++)
            {
                int idx = i;
                TuningEntry e = new TuningEntry
                {
                    key = name + "." + idx,
                    group = group,
                    min = 0f,
                    max = AdvancedMax,
                    advanced = true,
                    slider = false,
                    integer = false,
                    restart = false,
                    arrayIndex = idx,
                    get = () => ((float[])fi.GetValue(null))[idx],
                    set = v => ((float[])fi.GetValue(null))[idx] = v,
                };
                e.def = e.get();
                build.Add(e);
            }
        }
    }
}
