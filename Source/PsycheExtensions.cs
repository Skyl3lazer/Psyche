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
        public HediffDef? scarDef;
        public HediffDef? clarityDef;
        public HediffDef? closesScar;
        public float closureQuality = 0.5f;
    }

    public class PsycheScarExtension : DefModExtension
    {
        public HediffDef? upgradeClarityDef;
    }

    public class PsycheTraitMarkExtension : DefModExtension
    {
        public TraitDef trait = null!;
        public int degree;
        public float seedSeverity = 8f;
    }

    public class PsycheMitigationExtension : DefModExtension
    {
        public bool mitigates = true;
        public float potency = 1f;
    }
}
