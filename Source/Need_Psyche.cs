using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Psyche
{
    public class Need_Psyche : Need
    {
        private float innateMbt;
        private bool innateCached;
        private int lastCounseledTick = -1;

        public Need_Psyche(Pawn pawn)
            : base(pawn)
        {
        }

        public bool CanReceiveCounseling =>
            lastCounseledTick < 0 || Find.TickManager.TicksGame - lastCounseledTick >= PsycheTuning.CounselingCooldownTicks;

        public void NotifyCounseled()
        {
            lastCounseledTick = Find.TickManager.TicksGame;
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

        public void Recompute()
        {
            float baseMax = BaseMaxHealth;
            if (baseMax <= 0f)
            {
                CurLevel = 1f;
                return;
            }

            CurLevel = (EffectiveMaxHealth - SumPsycheThoughtDamage()) / baseMax;
        }

        public override void SetInitialLevel()
        {
            CacheInnate();
            CurLevel = MaxLevel;
        }

        public override void NeedInterval()
        {
            Recompute();
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
            Scribe_Values.Look(ref lastCounseledTick, "lastCounseledTick", -1);
        }

        private float SumPsycheThoughtDamage()
        {
            List<Thought_Memory>? memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
            if (memories == null)
            {
                return 0f;
            }

            float sum = 0f;
            for (int i = 0; i < memories.Count; i++)
            {
                if (memories[i] is Thought_Psyche pt)
                {
                    sum += pt.CurrentPsycheDamage;
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
