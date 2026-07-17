using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    [StaticConstructorOnStartup]
    public static class PsycheThoughtSetup
    {
        private static readonly HashSet<ThoughtDef> Registered = new HashSet<ThoughtDef>();

        static PsycheThoughtSetup()
        {
            foreach (ThoughtDef def in DefDatabase<ThoughtDef>.AllDefsListForReading)
            {
                if (QualifiesAsSource(def))
                {
                    Registered.Add(def);
                }
            }
        }

        public static bool IsRegisteredSource(ThoughtDef def)
        {
            return def != null && Registered.Contains(def);
        }

        private static bool QualifiesAsSource(ThoughtDef def)
        {
            if (def == null || !def.IsMemory)
            {
                return false;
            }

            // Exclude our own psychlet carriers (companions, own-triggers) - they are not sources.
            if (!typeof(Thought_Memory).IsAssignableFrom(def.ThoughtClass) || typeof(Thought_Psychlet).IsAssignableFrom(def.ThoughtClass))
            {
                return false;
            }

            // Worker-driven thoughts are situational, never gained as memories, so no companion forms.
            if (def.workerClass != null)
            {
                return false;
            }

            if (PsycheThoughtClassification.IsDenylisted(def))
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
            float best = 0f;
            for (int i = 0; i < def.stages.Count; i++)
            {
                ThoughtStage? stage = def.stages[i];
                if (stage == null)
                {
                    continue;
                }

                if (stage.baseMoodEffect < worst)
                {
                    worst = stage.baseMoodEffect;
                }

                if (stage.baseMoodEffect > best)
                {
                    best = stage.baseMoodEffect;
                }
            }

            bool qualifiesNegative = worst < 0f
                && (def.durationDays >= PsycheTuning.QualifyingDurationDays || Mathf.Abs(worst) >= PsycheTuning.QualifyingMoodThreshold);
            bool qualifiesPositive = best > 0f
                && (def.durationDays >= PsycheTuning.QualifyingDurationDays || best >= PsycheTuning.QualifyingMoodThreshold);

            return qualifiesNegative || qualifiesPositive;
        }
    }
}
