using RimWorld;
using Verse;

namespace Psyche
{
    public class PsycheHealthOffsetExtension : DefModExtension
    {
        public float maxHealthPerSeverity;
    }

    public class PsycheThoughtExtension : DefModExtension
    {
        public bool exemptFromWounds;
    }

    public class PsycheTraitMarkExtension : DefModExtension
    {
        public TraitDef trait = null!;
        public int degree;
        public float seedSeverity = 8f;
    }
}
