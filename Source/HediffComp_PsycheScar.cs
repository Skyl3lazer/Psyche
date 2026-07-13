using RimWorld;
using Verse;

namespace Psyche
{
    public class HediffCompProperties_PsycheScar : HediffCompProperties
    {
        public HediffCompProperties_PsycheScar()
        {
            compClass = typeof(HediffComp_PsycheScar);
        }
    }

    public class HediffComp_PsycheScar : HediffComp
    {
        public override string? CompTipStringExtra
        {
            get
            {
                Need_Psyche? need = Pawn.needs?.TryGetNeed<Need_Psyche>();
                if (need == null || need.BaseMaxHealth <= 0f)
                {
                    return null;
                }

                float coef = parent.def.GetModExtension<PsycheHealthOffsetExtension>()?.maxHealthPerSeverity ?? -1f;
                float reduction = (parent.Severity * -coef) / need.BaseMaxHealth;
                float mbt = reduction * (1f - need.InnateMbt);
                return "Maximum psyche: -" + reduction.ToStringPercent() + "\nBreak threshold: +" + mbt.ToStringPercent();
            }
        }
    }
}
