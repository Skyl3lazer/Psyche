using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Psyche
{
    [StaticConstructorOnStartup]
    public static class PsycheThoughtClassification
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

            ("AteHumanMeat_", "Psyche_Scar_Defilement"),
            ("ButcheredHuman_", "Psyche_Scar_Defilement"),
        };

        private static readonly Dictionary<string, string> ScarFamilyExact = new Dictionary<string, string>
        {
            { "Stillbirth", "Psyche_Scar_Bereavement" },
            { "Miscarried", "Psyche_Scar_Bereavement" },
            { "PartnerMiscarried", "Psyche_Scar_Bereavement" },

            { "BondedAnimalDied", "Psyche_Scar_SeveredBond" },
            { "BondedAnimalLost", "Psyche_Scar_SeveredBond" },
            { "BondedAnimalReleased", "Psyche_Scar_SeveredBond" },
            { "ConnectedTreeDied", "Psyche_Scar_SeveredBond" },
            { "DryadDied", "Psyche_Scar_SeveredBond" },
            { "PsychicBondTorn", "Psyche_Scar_SeveredBond" },
            { "TameVeneratedAnimalDied", "Psyche_Scar_SeveredBond" },

            { "MySonLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyDaughterLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyHusbandLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyWifeLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyFianceLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyFianceeLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyLoverLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyBrotherLost", "Psyche_Scar_UnresolvedLoss" },
            { "MySisterLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyGrandchildLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyFatherLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyMotherLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyNieceLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyNephewLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyHalfSiblingLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyAuntLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyUncleLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyGrandparentLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyCousinLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyKinLost", "Psyche_Scar_UnresolvedLoss" },
            { "MyBirthMotherLost", "Psyche_Scar_UnresolvedLoss" },
            { "ColonistLost", "Psyche_Scar_UnresolvedLoss" },
            { "PawnWithGoodOpinionLost", "Psyche_Scar_UnresolvedLoss" },
            { "FailedToRescueRelative", "Psyche_Scar_UnresolvedLoss" },
            { "OtherTravelerArrested", "Psyche_Scar_UnresolvedLoss" },
            { "OtherTravelerLeftBehind", "Psyche_Scar_UnresolvedLoss" },

            { "MyOrganHarvested", "Psyche_Scar_Violation" },
            { "KnowColonistOrganHarvested", "Psyche_Scar_Violation" },
            { "KnowGuestOrganHarvested", "Psyche_Scar_Violation" },
            { "XenogermHarvested_Prisoner", "Psyche_Scar_Violation" },
            { "PregnancyTerminated", "Psyche_Scar_Violation" },
            { "FedOn", "Psyche_Scar_Violation" },
            { "OtherTravelerSurgicallyViolated", "Psyche_Scar_Violation" },

            { "WasImprisoned", "Psyche_Scar_Captivity" },
            { "WasEnslaved", "Psyche_Scar_Captivity" },
            { "TrialConvicted", "Psyche_Scar_Captivity" },
            { "TrialFailed", "Psyche_Scar_Captivity" },

            { "AteHumanlikeMeatDirect", "Psyche_Scar_Defilement" },
            { "AteHumanlikeMeatAsIngredient", "Psyche_Scar_Defilement" },
            { "AteCorpse", "Psyche_Scar_Defilement" },
            { "ButcheredHumanlikeCorpse", "Psyche_Scar_Defilement" },
            { "KnowButcheredHumanlikeCorpse", "Psyche_Scar_Defilement" },
        };

        private static readonly Dictionary<ThoughtDef, HediffDef> ScarByDef = new Dictionary<ThoughtDef, HediffDef>();

        static PsycheThoughtClassification()
        {
            foreach (ThoughtDef def in DefDatabase<ThoughtDef>.AllDefsListForReading)
            {
                string? scarName = ResolveScarName(def.defName);
                if (scarName == null)
                {
                    continue;
                }

                HediffDef? scar = DefDatabase<HediffDef>.GetNamedSilentFail(scarName);
                if (scar != null)
                {
                    ScarByDef[def] = scar;
                }
            }
        }

        private static string? ResolveScarName(string defName)
        {
            if (ScarFamilyExact.TryGetValue(defName, out string exact))
            {
                return exact;
            }

            for (int i = 0; i < ScarFamilyPrefixes.Length; i++)
            {
                if (defName.StartsWith(ScarFamilyPrefixes[i].prefix, System.StringComparison.Ordinal))
                {
                    return ScarFamilyPrefixes[i].scarDefName;
                }
            }

            return null;
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
