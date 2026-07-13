using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Psyche
{
    public class Need_Psyche : Need
    {
        private float innateMbt;
        private bool innateCached;

        public Need_Psyche(Pawn pawn)
            : base(pawn)
        {
        }

        public float InnateMbt => innateMbt;

        public bool InnateCached => innateCached;

        public float BaseMaxHealth => (1f - innateMbt) * PsycheUtility.HealthScale;

        public float EffectiveMaxHealth
        {
            get
            {
                float sum = BaseMaxHealth;
                List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
                for (int i = 0; i < hediffs.Count; i++)
                {
                    PsycheHealthOffsetExtension? ext = hediffs[i].def.GetModExtension<PsycheHealthOffsetExtension>();
                    if (ext != null)
                    {
                        sum += hediffs[i].Severity * ext.maxHealthPerSeverity;
                    }
                }

                return sum;
            }
        }

        public float ThresholdOffset => innateCached ? (1f - CurLevel) * (1f - innateMbt) : 0f;

        public override float MaxLevel
        {
            get
            {
                float baseMax = BaseMaxHealth;
                return baseMax <= 0f ? 1f : EffectiveMaxHealth / baseMax;
            }
        }

        public void RecomputeFromHediffs()
        {
            float baseMax = BaseMaxHealth;
            if (baseMax <= 0f)
            {
                CurLevel = 1f;
                return;
            }

            CurLevel = (EffectiveMaxHealth - SumWoundSeverities()) / baseMax;
        }

        public override void SetInitialLevel()
        {
            CacheInnate();
            CurLevel = MaxLevel;
        }

        public override void NeedInterval()
        {
            RecomputeFromHediffs();
        }

        public override string GetTipString()
        {
            return (LabelCap + ": " + CurLevel.ToStringPercent() + " (max " + MaxLevel.ToStringPercent() + ")")
                .Colorize(ColoredText.TipSectionTitleColor) + "\n" + def.description;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref innateMbt, "innateMbt", 0f);
            Scribe_Values.Look(ref innateCached, "innateCached", false);
        }

        private float SumWoundSeverities()
        {
            float sum = 0f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i].def == PsycheDefOf.Psyche_Wound)
                {
                    sum += hediffs[i].Severity;
                }
            }

            return sum;
        }

        private void CacheInnate()
        {
            StatPart_PsycheThreshold.SuppressForInnateRead = true;
            try
            {
                innateMbt = pawn.GetStatValue(StatDefOf.MentalBreakThreshold);
                innateCached = true;
            }
            finally
            {
                StatPart_PsycheThreshold.SuppressForInnateRead = false;
            }
        }
    }
}
