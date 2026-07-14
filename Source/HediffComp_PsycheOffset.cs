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
                float delta = parent.Severity * coef;
                float thresholdChange = -(delta / need.BaseMaxHealth) * (1f - need.InnateMbt);
                return "Maximum psyche: " + SignedValue(delta) + "\nBreak threshold: " + SignedPercent(thresholdChange);
            }
        }

        private static string SignedValue(float value) => (value > 0f ? "+" : string.Empty) + value.ToString("0.#");

        private static string SignedPercent(float fraction) => (fraction > 0f ? "+" : string.Empty) + fraction.ToStringPercent();
    }
}
