using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    [StaticConstructorOnStartup]
    public static class PsycheThoughtSetup
    {
        static PsycheThoughtSetup()
        {
            foreach (ThoughtDef def in DefDatabase<ThoughtDef>.AllDefsListForReading)
            {
                if (ShouldConvert(def))
                {
                    def.thoughtClass = typeof(Thought_Psyche);
                    def.stackLimit = Mathf.Max(def.stackLimit, PsycheTuning.ConvertedStackLimit);
                }
            }
        }

        private static bool ShouldConvert(ThoughtDef def)
        {
            if (!def.IsMemory || def.ThoughtClass != typeof(Thought_Memory))
            {
                return false;
            }

            PsycheThoughtExtension? ext = def.GetModExtension<PsycheThoughtExtension>();
            if (ext != null && ext.exemptFromWounds)
            {
                return false;
            }

            if (def.stages == null || def.stages.Count == 0)
            {
                return false;
            }

            float worst = 0f;
            for (int i = 0; i < def.stages.Count; i++)
            {
                ThoughtStage? stage = def.stages[i];
                if (stage != null && stage.baseMoodEffect < worst)
                {
                    worst = stage.baseMoodEffect;
                }
            }

            if (worst >= 0f)
            {
                return false;
            }

            return def.durationDays >= PsycheTuning.QualifyingDurationDays
                || Mathf.Abs(worst) >= PsycheTuning.QualifyingMoodThreshold;
        }
    }
}
