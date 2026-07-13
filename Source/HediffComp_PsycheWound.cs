using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public class HediffCompProperties_PsycheWound : HediffCompProperties
    {
        public HediffCompProperties_PsycheWound()
        {
            compClass = typeof(HediffComp_PsycheWound);
        }
    }

    public class HediffComp_PsycheWound : HediffComp
    {
        public float initialMagnitude;
        public int healTicks = 1;

        private float mitigationSum;
        private int mitigationSamples;
        private int sampleCounter;
        private bool rolled;

        public override bool CompShouldRemove => rolled;

        public override string? CompTipStringExtra
        {
            get
            {
                Need_Psyche? need = Pawn.needs?.TryGetNeed<Need_Psyche>();
                if (need == null || need.BaseMaxHealth <= 0f)
                {
                    return null;
                }

                float drain = parent.Severity / need.BaseMaxHealth;
                float mbt = drain * (1f - need.InnateMbt);
                return "Psyche drain: -" + drain.ToStringPercent() + "\nBreak threshold: +" + mbt.ToStringPercent();
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (rolled)
            {
                return;
            }

            if (parent.ageTicks >= healTicks)
            {
                Resolve(remove: false);
                return;
            }

            sampleCounter++;
            if (sampleCounter >= 150)
            {
                sampleCounter = 0;
                mitigationSum += SampleAmbientMitigation();
                mitigationSamples++;
            }

            float healed = (float)parent.ageTicks / healTicks;
            parent.Severity = initialMagnitude * Mathf.Clamp01(1f - healed);
        }

        public void ForceFullHeal()
        {
            if (!rolled)
            {
                Resolve(remove: true);
            }
        }

        private void Resolve(bool remove)
        {
            rolled = true;
            parent.Severity = 0f;

            float mitigation = mitigationSamples > 0 ? mitigationSum / mitigationSamples : 0.5f;
            TryFormScar(parent.pawn, initialMagnitude, mitigation);

            if (remove)
            {
                parent.pawn.health.RemoveHediff(parent);
            }

            parent.pawn.needs?.TryGetNeed<Need_Psyche>()?.RecomputeFromHediffs();
        }

        private float SampleAmbientMitigation()
        {
            Pawn pawn = parent.pawn;
            float mood = pawn.needs?.mood?.CurLevelPercentage ?? 0.5f;
            float rest = pawn.needs?.rest?.CurLevelPercentage ?? 0.5f;
            float joy = pawn.needs?.joy?.CurLevelPercentage ?? 0.5f;
            return (mood + rest + joy) / 3f;
        }

        private static void TryFormScar(Pawn pawn, float magnitude, float mitigation)
        {
            float chance = Mathf.Clamp01(magnitude / PsycheTuning.ScarChanceDivisor) * (1f - (mitigation * PsycheTuning.MitigationStrength));
            if (!Rand.Chance(chance))
            {
                return;
            }

            float size = magnitude * PsycheTuning.ScarScale;
            if (mitigation >= PsycheTuning.StrongMitigationThreshold && Rand.Chance(PsycheTuning.ScarShrinkChance))
            {
                size *= PsycheTuning.ScarShrinkFactor;
            }

            BodyPartRecord? brain = pawn.health.hediffSet.GetBrain();
            if (brain == null)
            {
                return;
            }

            Hediff scar = HediffMaker.MakeHediff(PsycheDefOf.Psyche_Scar, pawn, brain);
            scar.Severity = size;
            pawn.health.AddHediff(scar, brain);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref initialMagnitude, "initialMagnitude", 0f);
            Scribe_Values.Look(ref healTicks, "healTicks", 1);
            Scribe_Values.Look(ref mitigationSum, "mitigationSum", 0f);
            Scribe_Values.Look(ref mitigationSamples, "mitigationSamples", 0);
            Scribe_Values.Look(ref sampleCounter, "sampleCounter", 0);
            Scribe_Values.Look(ref rolled, "rolled", false);
        }
    }
}
