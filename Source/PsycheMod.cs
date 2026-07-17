using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Psyche
{
    public class PsycheSettings : ModSettings
    {
        public bool attemptTherapyWithoutBestMedicine = true;
        public Dictionary<string, float> overrides = new Dictionary<string, float>();

        public bool IsModified(TuningEntry e) => overrides.ContainsKey(e.key);

        public void Set(TuningEntry e, float value)
        {
            value = Mathf.Clamp(value, e.min, e.max);
            if (e.integer)
                value = Mathf.Round(value);
            e.set(value);
            if (Mathf.Approximately(value, e.def))
                overrides.Remove(e.key);
            else
                overrides[e.key] = value;
        }

        public void Reset(TuningEntry e)
        {
            e.set(e.def);
            overrides.Remove(e.key);
        }

        public void ResetAll()
        {
            foreach (TuningEntry e in PsycheTuningRegistry.Entries)
                e.set(e.def);
            overrides.Clear();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref attemptTherapyWithoutBestMedicine, "attemptTherapyWithoutBestMedicine", true);
            Scribe_Collections.Look(ref overrides, "tuningOverrides", LookMode.Value, LookMode.Value);
            if (overrides == null)
                overrides = new Dictionary<string, float>();
        }
    }

    public class PsycheMod : Mod
    {
        private static PsycheSettings settings = null!;

        private static readonly Color ModifiedColor = new Color(1f, 0.85f, 0.4f);
        private Vector2 scroll;
        private bool advancedExpanded;
        private float contentHeight = 2000f;
        private readonly Dictionary<string, string> buffers = new Dictionary<string, string>();

        public PsycheMod(ModContentPack content)
            : base(content)
        {
            PsycheTuningRegistry.EnsureBuilt();
            settings = GetSettings<PsycheSettings>();
            PsycheTuningRegistry.ApplyOverrides(settings.overrides);
        }

        public static PsycheSettings Settings => settings ??= new PsycheSettings();

        public override string SettingsCategory() => "Psyche";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            float width = inRect.width - 20f;
            Rect view = new Rect(0f, 0f, width, contentHeight);
            Widgets.BeginScrollView(inRect, ref scroll, view);
            float y = 0f;

            Rect resetAll = new Rect(width - 200f, y, 200f, 30f);
            if (Widgets.ButtonText(resetAll, "Psyche_Setting_ResetAll".Translate(), active: settings.overrides.Count > 0))
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Psyche_Setting_ResetAllConfirm".Translate(), ResetAll));
            y += 40f;

            foreach (string group in PsycheTuningRegistry.GroupOrder)
                y = DrawGroup(width, y, group, advanced: false);

            Rect advHeader = new Rect(0f, y, width, 30f);
            string advKey = advancedExpanded ? "Psyche_Setting_AdvancedHide" : "Psyche_Setting_AdvancedShow";
            if (Widgets.ButtonText(advHeader, advKey.Translate()))
                advancedExpanded = !advancedExpanded;
            y += 40f;

            if (advancedExpanded)
                foreach (string group in PsycheTuningRegistry.GroupOrder)
                    y = DrawGroup(width, y, group, advanced: true);

            contentHeight = y;
            Widgets.EndScrollView();
        }

        private float DrawGroup(float width, float y, string group, bool advanced)
        {
            bool hasBool = !advanced && group == PsycheTuningRegistry.G.Therapy;
            List<TuningEntry> rows = new List<TuningEntry>();
            foreach (TuningEntry e in PsycheTuningRegistry.Entries)
                if (e.group == group && e.advanced == advanced)
                    rows.Add(e);
            if (rows.Count == 0 && !hasBool)
                return y;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, y, width, 34f), ("Psyche_Group_" + group).Translate());
            Text.Font = GameFont.Small;
            y += 36f;

            if (hasBool)
            {
                Rect boolRect = new Rect(0f, y, width, 28f);
                TooltipHandler.TipRegion(boolRect, "Psyche_Setting_AttemptWithoutBestMedicine_Tip".Translate());
                Widgets.CheckboxLabeled(boolRect,
                    "Psyche_Setting_AttemptWithoutBestMedicine".Translate(),
                    ref settings.attemptTherapyWithoutBestMedicine);
                y += 30f;
            }

            foreach (TuningEntry e in rows)
                y = DrawRow(width, y, e);

            return y + 6f;
        }

        private float DrawRow(float width, float y, TuningEntry e)
        {
            const float rowH = 30f;
            bool modified = settings.IsModified(e);
            float right = width;

            if (modified)
            {
                Rect resetRect = new Rect(right - 24f, y + 3f, 24f, 24f);
                if (Widgets.ButtonImage(resetRect, TexButton.CurveResetTex))
                {
                    settings.Reset(e);
                    buffers.Remove(e.key);
                }
                TooltipHandler.TipRegion(resetRect, "Psyche_Setting_ResetField".Translate());
                right -= 30f;
            }

            float labelW = width * 0.45f;
            Rect labelRect = new Rect(0f, y, labelW, rowH);
            if (modified)
                GUI.color = ModifiedColor;
            Widgets.Label(labelRect, LabelFor(e));
            GUI.color = Color.white;
            string tip = TipFor(e);
            if (!tip.NullOrEmpty())
                TooltipHandler.TipRegion(labelRect, tip);

            Rect widget = new Rect(labelW + 8f, y + 4f, right - labelW - 16f, rowH - 8f);
            float cur = e.get();
            if (e.slider)
            {
                float nv = Widgets.HorizontalSlider(widget, cur, e.min, e.max, middleAlignment: true,
                    label: FormatVal(e, cur), leftAlignedLabel: null, rightAlignedLabel: null,
                    roundTo: e.integer ? 1f : -1f);
                if (!Mathf.Approximately(nv, cur))
                    settings.Set(e, nv);
            }
            else
            {
                string buffer = buffers.TryGetValue(e.key, out string b) ? b : cur.ToString();
                float val = cur;
                Widgets.TextFieldNumeric(widget, ref val, ref buffer, e.min, e.max);
                buffers[e.key] = buffer;
                if (!Mathf.Approximately(val, cur))
                    settings.Set(e, val);
            }

            return y + rowH + 2f;
        }

        private void ResetAll()
        {
            settings.ResetAll();
            buffers.Clear();
        }

        private static string LabelFor(TuningEntry e)
        {
            string label = ("Psyche_Tune_" + e.BaseName).Translate();
            return e.arrayIndex < 0 ? label : label + " [" + (e.arrayIndex + 1) + "]";
        }

        private static string TipFor(TuningEntry e)
        {
            string tipKey = "Psyche_Tune_" + e.BaseName + "_Tip";
            string tip = tipKey.CanTranslate() ? tipKey.Translate().ToString() : "";
            if (e.restart)
                tip = (tip + " " + "Psyche_Setting_RestartNote".Translate()).Trim();
            return tip;
        }

        private static string FormatVal(TuningEntry e, float v)
            => v.ToString(e.integer ? "0" : "0.###");
    }
}
