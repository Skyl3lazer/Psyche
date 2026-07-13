using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public class Need_Psyche : Need
    {
        private float innateMbt;
        private bool innateCached;

        private const float RegenPerDay = 0.6f;

        public Need_Psyche(Pawn pawn)
            : base(pawn)
        {
        }

        public float InnateMbt => innateMbt;

        public bool InnateCached => innateCached;

        public float BaseMaxHealth => (1f - innateMbt) * PsycheUtility.HealthScale;

        public float EffectiveMaxHealth => BaseMaxHealth;

        public float ThresholdOffset => innateCached ? (1f - CurLevel) * (1f - innateMbt) : 0f;

        public override float MaxLevel
        {
            get
            {
                float baseMax = BaseMaxHealth;
                return baseMax <= 0f ? 1f : EffectiveMaxHealth / baseMax;
            }
        }

        public override void SetInitialLevel()
        {
            CurLevel = 1f;
            CacheInnate();
        }

        public override void NeedInterval()
        {
            if (CurLevel < MaxLevel)
            {
                CurLevel = Mathf.Min(MaxLevel, CurLevel + (RegenPerDay * 150f / 60000f));
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref innateMbt, "innateMbt", 0f);
            Scribe_Values.Look(ref innateCached, "innateCached", false);
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
