using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Psyche
{
    [StaticConstructorOnStartup]
    public static class PsycheIdeologyClassification
    {
        // Precept thoughts are named <Act><Tier> and <Act>_Know_<Tier>_Mood, so one act prefix maps every tier and witness variant to its family.
        private static readonly (string prefix, string scarDefName)[] ScarFamilyPrefixes =
        {
            ("AteMeat_", "Psyche_Scar_Apostasy"),
            ("AteNonMeat_", "Psyche_Scar_Apostasy"),
            ("AteVeneratedAnimalMeat", "Psyche_Scar_Apostasy"),
            ("InstalledProsthetic_", "Psyche_Scar_Apostasy"),
            ("BioSculpterDespised", "Psyche_Scar_Apostasy"),
            ("GotLovin_", "Psyche_Scar_Apostasy"),
            ("IngestedDrug_", "Psyche_Scar_Apostasy"),
            ("IngestedRecreationalDrug_", "Psyche_Scar_Apostasy"),
            ("IngestedHardDrug_", "Psyche_Scar_Apostasy"),
            ("AdministeredDrug_", "Psyche_Scar_Apostasy"),
            ("AdministeredRecreationalDrug_", "Psyche_Scar_Apostasy"),
            ("AdministeredHardDrug_", "Psyche_Scar_Apostasy"),
            ("Mined_", "Psyche_Scar_Apostasy"),
            ("MineableDestroyed_", "Psyche_Scar_Apostasy"),
            ("CutTree_", "Psyche_Scar_Apostasy"),
            ("InstalledOrgan_", "Psyche_Scar_Apostasy"),
            ("TradedOrgan_", "Psyche_Scar_Apostasy"),
            ("SoldOrgan_", "Psyche_Scar_Apostasy"),

            ("ExecutedPrisoner_", "Psyche_Scar_Bloodguilt"),
            ("ExecutedPrisonerInnocent_", "Psyche_Scar_Bloodguilt"),
            ("ExecutedGuest_", "Psyche_Scar_Bloodguilt"),
            ("ExecutedColonist_", "Psyche_Scar_Bloodguilt"),
            ("InnocentPrisonerDied_", "Psyche_Scar_Bloodguilt"),
            ("EnslavedPrisoner_", "Psyche_Scar_Bloodguilt"),
            ("SoldSlave_", "Psyche_Scar_Bloodguilt"),
            ("SlaughteredAnimal_", "Psyche_Scar_Bloodguilt"),
            ("KilledInnocentAnimal_", "Psyche_Scar_Bloodguilt"),
            ("HarvestedOrgan_", "Psyche_Scar_Bloodguilt"),

            ("CharityRefused_", "Psyche_Scar_Callousness"),

            ("RelicDestroyed", "Psyche_Scar_SacredLoss"),
            ("RelicLost", "Psyche_Scar_SacredLoss"),
            ("IdeoRoleLost", "Psyche_Scar_SacredLoss"),
        };

        private static readonly Dictionary<ThoughtDef, HediffDef> ScarByDef = new Dictionary<ThoughtDef, HediffDef>();

        static PsycheIdeologyClassification()
        {
            foreach (ThoughtDef def in DefDatabase<ThoughtDef>.AllDefsListForReading)
            {
                for (int i = 0; i < ScarFamilyPrefixes.Length; i++)
                {
                    if (!def.defName.StartsWith(ScarFamilyPrefixes[i].prefix, System.StringComparison.Ordinal))
                    {
                        continue;
                    }

                    HediffDef? scar = DefDatabase<HediffDef>.GetNamedSilentFail(ScarFamilyPrefixes[i].scarDefName);
                    if (scar != null)
                    {
                        ScarByDef[def] = scar;
                    }

                    break;
                }
            }
        }

        public static HediffDef? ScarDefFor(ThoughtDef def)
        {
            return def != null && ScarByDef.TryGetValue(def, out HediffDef scar) ? scar : null;
        }

        public static HediffDef? ResolveScarDef(ThoughtDef def)
        {
            return def == null ? null : (def.GetModExtension<PsycheThoughtExtension>()?.scarDef ?? ScarDefFor(def));
        }

        // A good or bad ceremony is an event reaction, not a lasting wound or growth.
        public static bool IsDenylisted(ThoughtDef def)
        {
            return def != null && typeof(Thought_AttendedRitual).IsAssignableFrom(def.ThoughtClass);
        }
    }
}
