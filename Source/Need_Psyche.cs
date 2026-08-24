using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Psyche
{
    public class Need_Psyche : Need
    {
        private float innateMbt;
        private bool innateCached;
        private int lastCounseledTick = -1;
        private IntVec3 therapyRendezvous = IntVec3.Invalid;

        public Need_Psyche(Pawn pawn)
            : base(pawn)
        {
        }

        public float InnateMbt => innateMbt;

        public IntVec3 TherapyRendezvous
        {
            get => therapyRendezvous;
            set => therapyRendezvous = value;
        }

        public bool InnateCached => innateCached;

        public float BaseMaxHealth => (1f - innateMbt) * PsycheUtility.HealthScale;

        public bool CanReceiveCounseling =>
            lastCounseledTick < 0 || Find.TickManager.TicksGame - lastCounseledTick >= PsycheTuning.CounselingCooldownTicks;

        public int TicksUntilCounseling =>
            lastCounseledTick < 0 ? 0 : Mathf.Max(0, lastCounseledTick + PsycheTuning.CounselingCooldownTicks - Find.TickManager.TicksGame);

        public float EffectiveMaxHealth
        {
            get
            {
                float baseMax = BaseMaxHealth;
                float sum = baseMax;
                List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
                for (int i = 0; i < hediffs.Count; i++)
                {
                    PsycheHealthOffsetExtension? ext = hediffs[i].def.GetModExtension<PsycheHealthOffsetExtension>();
                    if (ext != null)
                    {
                        sum += hediffs[i].Severity * ext.maxHealthPerSeverity;
                    }
                }

                return Mathf.Clamp(sum, baseMax * PsycheTuning.MaxHealthFloorFrac, baseMax + PsycheTuning.ClarityCapHP);
            }
        }

        public float CurrentPsycheHealth
        {
            get
            {
                float sum = EffectiveMaxHealth;
                List<Thought_Memory>? memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
                if (memories != null)
                {
                    for (int i = 0; i < memories.Count; i++)
                    {
                        if (memories[i] is Thought_Psychlet pt)
                        {
                            sum -= pt.CurrentPsycheDamage;
                            sum += pt.CurrentBoonBonus;
                        }
                    }
                }

                return Mathf.Max(0f, sum);
            }
        }

        public float ThresholdOffset =>
            innateCached ? (1f - (CurrentPsycheHealth / BaseMaxHealth)) * (1f - innateMbt) : 0f;

        public override float MaxLevel
        {
            get
            {
                float baseMax = BaseMaxHealth;
                return baseMax <= 0f ? 1f : EffectiveMaxHealth / baseMax;
            }
        }

        public void NotifyCounseled()
        {
            lastCounseledTick = Find.TickManager.TicksGame;
        }

        public void Recompute()
        {
            float baseMax = BaseMaxHealth;
            CurLevel = baseMax <= 0f ? 1f : CurrentPsycheHealth / baseMax;
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

        public string StatusString()
        {
            float baseMax = BaseMaxHealth;
            float current = CurrentPsycheHealth;
            float pct = baseMax > 0f ? current / baseMax : 0f;
            return LabelCap + ": " + current.ToString("0") + "/" + baseMax.ToString("0") + " (" + pct.ToStringPercent() + ")";
        }

        public override string GetTipString()
        {
            return StatusString().Colorize(ColoredText.TipSectionTitleColor) + "\n" + def.description;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref innateMbt, "innateMbt", 0f);
            Scribe_Values.Look(ref innateCached, "innateCached", false);
            Scribe_Values.Look(ref lastCounseledTick, "lastCounseledTick", -1);
            Scribe_Values.Look(ref therapyRendezvous, "therapyRendezvous", IntVec3.Invalid);
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
