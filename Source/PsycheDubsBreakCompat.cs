using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Psyche
{
    public static class PsycheDubsBreakCompat
    {
        private static FieldInfo? settingsField;
        private static FieldInfo? minorLimitField;
        private static FieldInfo? majorLimitField;
        private static FieldInfo? extremeLimitField;

        // True once the postfix is wired into Dubs Break Mod; the test kit gates its checks on this.
        public static bool Patched { get; private set; }

        public static void Apply(Harmony harmony)
        {
            Type? patchType = AccessTools.TypeByName("DubsBreakMod.Harmony_CurrentPossibleMoodBreaks");
            MethodInfo? target = patchType != null ? AccessTools.Method(patchType, "DetermineHighestIntensity") : null;
            if (target == null)
            {
                return;
            }

            Type? modType = AccessTools.TypeByName("DubsBreakMod.MentalManagementMod");
            Type? settingsType = AccessTools.TypeByName("DubsBreakMod.Settings");
            settingsField = modType != null ? AccessTools.Field(modType, "Settings") : null;
            minorLimitField = settingsType != null ? AccessTools.Field(settingsType, "MinorLimit") : null;
            majorLimitField = settingsType != null ? AccessTools.Field(settingsType, "MajorLimit") : null;
            extremeLimitField = settingsType != null ? AccessTools.Field(settingsType, "ExtremeLimit") : null;

            if (settingsField == null || minorLimitField == null || majorLimitField == null || extremeLimitField == null)
            {
                Log.Warning("[Psyche] Dubs Break Mod detected but its break-tier limits could not be read; psychlets will not raise break severity.");
                return;
            }

            harmony.Patch(target, postfix: new HarmonyMethod(typeof(PsycheDubsBreakCompat), nameof(RaiseForPsychlets)));
            Patched = true;
        }

        // Dubs Break Mod caps severity at the worst single mood thought; a psychlet carries zero mood, so an active injury must raise the tier itself.
        public static void RaiseForPsychlets(ThoughtHandler __1, ref MentalBreakIntensity __result)
        {
            Pawn? pawn = __1?.pawn;
            if (settingsField == null || pawn == null || !PsycheUtility.HasPsyche(pawn))
            {
                return;
            }

            List<Thought_Memory>? memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
            if (memories == null || memories.Count == 0)
            {
                return;
            }

            object? settings = settingsField!.GetValue(null);
            if (settings == null)
            {
                return;
            }

            int minor = (int)minorLimitField!.GetValue(settings);
            int major = (int)majorLimitField!.GetValue(settings);
            int extreme = (int)extremeLimitField!.GetValue(settings);

            int highest = (int)__result;
            for (int i = 0; i < memories.Count; i++)
            {
                if (memories[i] is not Thought_Psychlet injury || injury.IsBoon)
                {
                    continue;
                }

                float signed = -injury.CurrentPsycheDamage;
                int tier = signed <= extreme ? 3 : signed <= major ? 2 : signed <= minor ? 1 : 0;
                if (tier > highest)
                {
                    highest = tier;
                }
            }

            if (highest > (int)__result)
            {
                __result = (MentalBreakIntensity)highest;
            }
        }
    }
}
