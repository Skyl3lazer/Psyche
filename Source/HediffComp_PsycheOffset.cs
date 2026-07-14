using RimWorld;
using Verse;

namespace Psyche
{
    public class HediffCompProperties_PsycheOffset : HediffCompProperties
    {
        public HediffCompProperties_PsycheOffset()
        {
            compClass = typeof(HediffComp_PsycheOffset);
        }
    }

    public class HediffComp_PsycheOffset : HediffComp
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

                float coef = parent.def.GetModExtension<PsycheHealthOffsetExtension>()?.maxHealthPerSeverity ?? 0f;
                float deltaFraction = (parent.Severity * coef) / need.BaseMaxHealth;
                float thresholdChange = -deltaFraction * (1f - need.InnateMbt);
                return "Maximum psyche: " + Signed(deltaFraction) + "\nBreak threshold: " + Signed(thresholdChange);
            }
        }

        private static string Signed(float fraction) => (fraction > 0f ? "+" : string.Empty) + fraction.ToStringPercent();
    }
}
