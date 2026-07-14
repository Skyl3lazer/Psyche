using UnityEngine;
using Verse;

namespace Psyche
{
    public class PsycheSettings : ModSettings
    {
        public bool attemptTherapyWithoutBestMedicine = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref attemptTherapyWithoutBestMedicine, "attemptTherapyWithoutBestMedicine", true);
        }
    }

    public class PsycheMod : Mod
    {
        private static PsycheSettings settings = null!;

        public PsycheMod(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<PsycheSettings>();
        }

        public static PsycheSettings Settings => settings ??= new PsycheSettings();

        public override string SettingsCategory() => "Psyche";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled(
                "Psyche_Setting_AttemptWithoutBestMedicine".Translate(),
                ref settings.attemptTherapyWithoutBestMedicine,
                "Psyche_Setting_AttemptWithoutBestMedicine_Tip".Translate());
            listing.End();
        }
    }
}
