using UnityEditor;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    internal static class PlayerVolumeSettingGUI
    {
        const float DefaultVoiceGain = 15f;
        const float DefaultVoiceDistanceNear = 0f;
        const float DefaultVoiceDistanceFar = 25f;
        const float DefaultVoiceVolumetricRadius = 0f;
        const bool DefaultVoiceLowpass = true;
        const float DefaultAvatarAudioGain = 10f;
        const float DefaultAvatarAudioDistanceNear = 0f;
        const float DefaultAvatarAudioDistanceFar = 40f;
        const float DefaultAvatarAudioVolumetricRadius = 0f;
        const bool DefaultAvatarAudioForceSpatial = false;

        const float MaxVoiceGain = 24f;
        const float MaxVoiceDistanceNear = 1000000f;
        const float MaxVoiceDistanceFar = 1000000f;
        const float MaxVoiceVolumetricRadius = 1000f;
        const float MaxAvatarAudioGain = 10f;
        const float MaxAvatarAudioDistanceNear = 1000000f;
        const float MaxAvatarAudioDistanceFar = 1000000f;
        const float MaxAvatarAudioVolumetricRadius = 1000f;

        // Header(1) + Voice(5) + Header(1) + AvatarAudio(5)
        const int FullLineCount = 12;
        // Header + toggle row + value row, twice
        const int CompactLineCount = 6;

        const string ModePrefKey = "Narazaka.VRChat.PlayerVolumeManager.InspectorMode";
        const string FilterMaskPrefKey = "Narazaka.VRChat.PlayerVolumeManager.InspectorFilterMask";

        public enum DisplayMode
        {
            Full = 0,
            Compact = 1,
            Filter = 2,
        }

        [System.Flags]
        public enum FieldFlag
        {
            None = 0,
            VoiceGain = 1 << 0,
            VoiceDistanceNear = 1 << 1,
            VoiceDistanceFar = 1 << 2,
            VoiceVolumetricRadius = 1 << 3,
            VoiceLowpass = 1 << 4,
            AvatarAudioGain = 1 << 5,
            AvatarAudioDistanceNear = 1 << 6,
            AvatarAudioDistanceFar = 1 << 7,
            AvatarAudioVolumetricRadius = 1 << 8,
            AvatarAudioForceSpatial = 1 << 9,
        }

        const FieldFlag VoiceMask =
            FieldFlag.VoiceGain | FieldFlag.VoiceDistanceNear | FieldFlag.VoiceDistanceFar
            | FieldFlag.VoiceVolumetricRadius | FieldFlag.VoiceLowpass;
        const FieldFlag AvatarMask =
            FieldFlag.AvatarAudioGain | FieldFlag.AvatarAudioDistanceNear | FieldFlag.AvatarAudioDistanceFar
            | FieldFlag.AvatarAudioVolumetricRadius | FieldFlag.AvatarAudioForceSpatial;

        public static DisplayMode GetMode() => (DisplayMode)EditorPrefs.GetInt(ModePrefKey, 0);
        public static void SetMode(DisplayMode value) => EditorPrefs.SetInt(ModePrefKey, (int)value);
        public static FieldFlag GetFilterMask() => (FieldFlag)EditorPrefs.GetInt(FilterMaskPrefKey, (int)FieldFlag.VoiceGain);
        public static void SetFilterMask(FieldFlag value) => EditorPrefs.SetInt(FilterMaskPrefKey, (int)value);

        public sealed class Properties
        {
            public SerializedProperty VoiceGain;
            public SerializedProperty VoiceDistanceNear;
            public SerializedProperty VoiceDistanceFar;
            public SerializedProperty VoiceVolumetricRadius;
            public SerializedProperty EnableVoiceLowpass;
            public SerializedProperty VoiceLowpass;
            public SerializedProperty AvatarAudioGain;
            public SerializedProperty AvatarAudioDistanceNear;
            public SerializedProperty AvatarAudioDistanceFar;
            public SerializedProperty AvatarAudioVolumetricRadius;
            public SerializedProperty EnableAvatarAudioForceSpatial;
            public SerializedProperty AvatarAudioForceSpatial;

            public static Properties Create(SerializedObject so) => new Properties
            {
                VoiceGain = so.FindProperty(nameof(PlayerVolumeSetting._voiceGain)),
                VoiceDistanceNear = so.FindProperty(nameof(PlayerVolumeSetting._voiceDistanceNear)),
                VoiceDistanceFar = so.FindProperty(nameof(PlayerVolumeSetting._voiceDistanceFar)),
                VoiceVolumetricRadius = so.FindProperty(nameof(PlayerVolumeSetting._voiceVolumetricRadius)),
                EnableVoiceLowpass = so.FindProperty(nameof(PlayerVolumeSetting._enableVoiceLowpass)),
                VoiceLowpass = so.FindProperty(nameof(PlayerVolumeSetting._voiceLowpass)),
                AvatarAudioGain = so.FindProperty(nameof(PlayerVolumeSetting._avatarAudioGain)),
                AvatarAudioDistanceNear = so.FindProperty(nameof(PlayerVolumeSetting._avatarAudioDistanceNear)),
                AvatarAudioDistanceFar = so.FindProperty(nameof(PlayerVolumeSetting._avatarAudioDistanceFar)),
                AvatarAudioVolumetricRadius = so.FindProperty(nameof(PlayerVolumeSetting._avatarAudioVolumetricRadius)),
                EnableAvatarAudioForceSpatial = so.FindProperty(nameof(PlayerVolumeSetting._enableAvatarAudioForceSpatial)),
                AvatarAudioForceSpatial = so.FindProperty(nameof(PlayerVolumeSetting._avatarAudioForceSpatial)),
            };
        }

        sealed class Labels
        {
            public static GUIContent VoiceGain = new GUIContent("Gain");
            public static GUIContent VoiceDistanceNear = new GUIContent("Near");
            public static GUIContent VoiceDistanceFar = new GUIContent("Far");
            public static GUIContent VoiceVolumetricRadius = new GUIContent("Volumetric Radius");
            public static GUIContent VoiceLowPass = new GUIContent("Lowpass");
            public static GUIContent AvatarAudioGain = new GUIContent("Gain");
            public static GUIContent AvatarAudioDistanceNear = new GUIContent("Near");
            public static GUIContent AvatarAudioDistanceFar = new GUIContent("Far");
            public static GUIContent AvatarAudioVolumetricRadius = new GUIContent("Volumetric Radius");
            public static GUIContent AvatarAudioForceSpatial = new GUIContent("Force Spatial");
        }

        public static float GetHeight()
        {
            int lines;
            switch (GetMode())
            {
                case DisplayMode.Full: lines = FullLineCount; break;
                case DisplayMode.Compact: lines = CompactLineCount; break;
                default: lines = GetFilterLineCount(GetFilterMask()); break;
            }
            return EditorGUIUtility.singleLineHeight * lines
                 + EditorGUIUtility.standardVerticalSpacing * (lines - 1);
        }

        static int GetFilterLineCount(FieldFlag mask)
        {
            var voiceCount = CountBits((int)(mask & VoiceMask));
            var avatarCount = CountBits((int)(mask & AvatarMask));
            var lines = 0;
            if (voiceCount > 0) lines += 1 + voiceCount;
            if (avatarCount > 0) lines += 1 + avatarCount;
            return lines == 0 ? 1 : lines;
        }

        public static void Draw(Rect rect, Properties p)
        {
            switch (GetMode())
            {
                case DisplayMode.Full: DrawFull(rect, p); break;
                case DisplayMode.Compact: DrawCompact(rect, p); break;
                default: DrawFilter(rect, p); break;
            }
        }

        static readonly GUIContent ModeLabel = new GUIContent("(Listen Setting Filter)");

        public static void DrawModeDropdownLayout()
        {
            var rect = EditorGUILayout.GetControlRect();
            var dropdownRect = EditorGUI.PrefixLabel(rect, ModeLabel);
            var content = new GUIContent(GetCurrentModeLabel());
            if (EditorGUI.DropdownButton(dropdownRect, content, FocusType.Keyboard))
            {
                ShowModeMenu(dropdownRect);
            }
        }

        static void DrawFull(Rect rect, Properties p)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var step = line + EditorGUIUtility.standardVerticalSpacing;
            var r = new Rect(rect.x, rect.y, rect.width, line);

            DrawHeader(r, "Player Volume"); r.y += step;
            DrawFloat(r, Labels.VoiceGain, p.VoiceGain, DefaultVoiceGain, MaxVoiceGain); r.y += step;
            DrawFloat(r, Labels.VoiceDistanceNear, p.VoiceDistanceNear, DefaultVoiceDistanceNear, MaxVoiceDistanceNear); r.y += step;
            DrawFloat(r, Labels.VoiceDistanceFar, p.VoiceDistanceFar, DefaultVoiceDistanceFar, MaxVoiceDistanceFar); r.y += step;
            DrawFloat(r, Labels.VoiceVolumetricRadius, p.VoiceVolumetricRadius, DefaultVoiceVolumetricRadius, MaxVoiceVolumetricRadius); r.y += step;
            DrawBool(r, Labels.VoiceLowPass, p.VoiceLowpass, p.EnableVoiceLowpass, DefaultVoiceLowpass); r.y += step;
            DrawHeader(r, "Avatar Audio"); r.y += step;
            DrawFloat(r, Labels.AvatarAudioGain, p.AvatarAudioGain, DefaultAvatarAudioGain, MaxAvatarAudioGain); r.y += step;
            DrawFloat(r, Labels.AvatarAudioDistanceNear, p.AvatarAudioDistanceNear, DefaultAvatarAudioDistanceNear, MaxAvatarAudioDistanceNear); r.y += step;
            DrawFloat(r, Labels.AvatarAudioDistanceFar, p.AvatarAudioDistanceFar, DefaultAvatarAudioDistanceFar, MaxAvatarAudioDistanceFar); r.y += step;
            DrawFloat(r, Labels.AvatarAudioVolumetricRadius, p.AvatarAudioVolumetricRadius, DefaultAvatarAudioVolumetricRadius, MaxAvatarAudioVolumetricRadius); r.y += step;
            DrawBool(r, Labels.AvatarAudioForceSpatial, p.AvatarAudioForceSpatial, p.EnableAvatarAudioForceSpatial, DefaultAvatarAudioForceSpatial);
        }

        static void DrawCompact(Rect rect, Properties p)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var step = line + spacing;
            var sectionHeight = line + spacing + line;
            var r = new Rect(rect.x, rect.y, rect.width, line);

            DrawHeader(r, "Player Volume"); r.y += step;
            var voiceCols = SplitHorizontal(new Rect(r.x, r.y, r.width, sectionHeight), 5);
            DrawCompactFloat(voiceCols[0], Labels.VoiceGain, p.VoiceGain, DefaultVoiceGain, MaxVoiceGain);
            DrawCompactFloat(voiceCols[1], Labels.VoiceDistanceNear, p.VoiceDistanceNear, DefaultVoiceDistanceNear, MaxVoiceDistanceNear);
            DrawCompactFloat(voiceCols[2], Labels.VoiceDistanceFar, p.VoiceDistanceFar, DefaultVoiceDistanceFar, MaxVoiceDistanceFar);
            DrawCompactFloat(voiceCols[3], Labels.VoiceVolumetricRadius, p.VoiceVolumetricRadius, DefaultVoiceVolumetricRadius, MaxVoiceVolumetricRadius);
            DrawCompactBool(voiceCols[4], Labels.VoiceLowPass, p.VoiceLowpass, p.EnableVoiceLowpass, DefaultVoiceLowpass);
            r.y += sectionHeight + spacing;

            DrawHeader(r, "Avatar Audio"); r.y += step;
            var avatarCols = SplitHorizontal(new Rect(r.x, r.y, r.width, sectionHeight), 5);
            DrawCompactFloat(avatarCols[0], Labels.AvatarAudioGain, p.AvatarAudioGain, DefaultAvatarAudioGain, MaxAvatarAudioGain);
            DrawCompactFloat(avatarCols[1], Labels.AvatarAudioDistanceNear, p.AvatarAudioDistanceNear, DefaultAvatarAudioDistanceNear, MaxAvatarAudioDistanceNear);
            DrawCompactFloat(avatarCols[2], Labels.AvatarAudioDistanceFar, p.AvatarAudioDistanceFar, DefaultAvatarAudioDistanceFar, MaxAvatarAudioDistanceFar);
            DrawCompactFloat(avatarCols[3], Labels.AvatarAudioVolumetricRadius, p.AvatarAudioVolumetricRadius, DefaultAvatarAudioVolumetricRadius, MaxAvatarAudioVolumetricRadius);
            DrawCompactBool(avatarCols[4], Labels.AvatarAudioForceSpatial, p.AvatarAudioForceSpatial, p.EnableAvatarAudioForceSpatial, DefaultAvatarAudioForceSpatial);
        }

        static void DrawFilter(Rect rect, Properties p)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var step = line + EditorGUIUtility.standardVerticalSpacing;
            var r = new Rect(rect.x, rect.y, rect.width, line);
            var mask = GetFilterMask();
            var hasVoice = (mask & VoiceMask) != 0;
            var hasAvatar = (mask & AvatarMask) != 0;

            if (hasVoice)
            {
                DrawHeader(r, "Player Volume"); r.y += step;
                if ((mask & FieldFlag.VoiceGain) != 0) { DrawFloat(r, Labels.VoiceGain, p.VoiceGain, DefaultVoiceGain, MaxVoiceGain); r.y += step; }
                if ((mask & FieldFlag.VoiceDistanceNear) != 0) { DrawFloat(r, Labels.VoiceDistanceNear, p.VoiceDistanceNear, DefaultVoiceDistanceNear, MaxVoiceDistanceNear); r.y += step; }
                if ((mask & FieldFlag.VoiceDistanceFar) != 0) { DrawFloat(r, Labels.VoiceDistanceFar, p.VoiceDistanceFar, DefaultVoiceDistanceFar, MaxVoiceDistanceFar); r.y += step; }
                if ((mask & FieldFlag.VoiceVolumetricRadius) != 0) { DrawFloat(r, Labels.VoiceVolumetricRadius, p.VoiceVolumetricRadius, DefaultVoiceVolumetricRadius, MaxVoiceVolumetricRadius); r.y += step; }
                if ((mask & FieldFlag.VoiceLowpass) != 0) { DrawBool(r, Labels.VoiceLowPass, p.VoiceLowpass, p.EnableVoiceLowpass, DefaultVoiceLowpass); r.y += step; }
            }
            if (hasAvatar)
            {
                DrawHeader(r, "Avatar Audio"); r.y += step;
                if ((mask & FieldFlag.AvatarAudioGain) != 0) { DrawFloat(r, Labels.AvatarAudioGain, p.AvatarAudioGain, DefaultAvatarAudioGain, MaxAvatarAudioGain); r.y += step; }
                if ((mask & FieldFlag.AvatarAudioDistanceNear) != 0) { DrawFloat(r, Labels.AvatarAudioDistanceNear, p.AvatarAudioDistanceNear, DefaultAvatarAudioDistanceNear, MaxAvatarAudioDistanceNear); r.y += step; }
                if ((mask & FieldFlag.AvatarAudioDistanceFar) != 0) { DrawFloat(r, Labels.AvatarAudioDistanceFar, p.AvatarAudioDistanceFar, DefaultAvatarAudioDistanceFar, MaxAvatarAudioDistanceFar); r.y += step; }
                if ((mask & FieldFlag.AvatarAudioVolumetricRadius) != 0) { DrawFloat(r, Labels.AvatarAudioVolumetricRadius, p.AvatarAudioVolumetricRadius, DefaultAvatarAudioVolumetricRadius, MaxAvatarAudioVolumetricRadius); r.y += step; }
                if ((mask & FieldFlag.AvatarAudioForceSpatial) != 0) { DrawBool(r, Labels.AvatarAudioForceSpatial, p.AvatarAudioForceSpatial, p.EnableAvatarAudioForceSpatial, DefaultAvatarAudioForceSpatial); r.y += step; }
            }
        }

        static void DrawHeader(Rect rect, string label)
        {
            EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
        }

        static void ShowModeMenu(Rect rect)
        {
            var menu = new GenericMenu();
            var mode = GetMode();
            var mask = GetFilterMask();
            menu.AddItem(new GUIContent("Full"), mode == DisplayMode.Full, () => SetMode(DisplayMode.Full));
            menu.AddItem(new GUIContent("Compact"), mode == DisplayMode.Compact, () => SetMode(DisplayMode.Compact));
            menu.AddSeparator("");
            AddFieldItem(menu, mode, mask, FieldFlag.VoiceGain, "Player > Gain");
            AddFieldItem(menu, mode, mask, FieldFlag.VoiceDistanceNear, "Player > Near");
            AddFieldItem(menu, mode, mask, FieldFlag.VoiceDistanceFar, "Player > Far");
            AddFieldItem(menu, mode, mask, FieldFlag.VoiceVolumetricRadius, "Player > Volumetric Radius");
            AddFieldItem(menu, mode, mask, FieldFlag.VoiceLowpass, "Player > Lowpass");
            AddFieldItem(menu, mode, mask, FieldFlag.AvatarAudioGain, "Avatar > Gain");
            AddFieldItem(menu, mode, mask, FieldFlag.AvatarAudioDistanceNear, "Avatar > Near");
            AddFieldItem(menu, mode, mask, FieldFlag.AvatarAudioDistanceFar, "Avatar > Far");
            AddFieldItem(menu, mode, mask, FieldFlag.AvatarAudioVolumetricRadius, "Avatar > Volumetric Radius");
            AddFieldItem(menu, mode, mask, FieldFlag.AvatarAudioForceSpatial, "Avatar > Force Spatial");
            menu.DropDown(rect);
        }

        static void AddFieldItem(GenericMenu menu, DisplayMode mode, FieldFlag mask, FieldFlag flag, string label)
        {
            var on = mode == DisplayMode.Filter ? (mask & flag) != 0 : false;
            menu.AddItem(new GUIContent(label), on, () =>
            {
                if (mode != DisplayMode.Filter)
                {
                    SetFilterMask(flag);
                    SetMode(DisplayMode.Filter);
                }
                else
                {
                    var current = GetFilterMask();
                    var next = (current & flag) != 0 ? current & ~flag : current | flag;
                    if (next == FieldFlag.None) SetMode(DisplayMode.Full);
                    else SetFilterMask(next);
                }
            });
        }

        static string GetCurrentModeLabel()
        {
            switch (GetMode())
            {
                case DisplayMode.Full: return "Full";
                case DisplayMode.Compact: return "Compact";
                default:
                    var mask = GetFilterMask();
                    var count = CountBits((int)mask);
                    if (count == 0) return "Filter";
                    if (count == 1) return GetSingleFieldLabel(mask);
                    return $"Filter ({count})";
            }
        }

        static string GetSingleFieldLabel(FieldFlag flag)
        {
            switch (flag)
            {
                case FieldFlag.VoiceGain: return "Player > Gain";
                case FieldFlag.VoiceDistanceNear: return "Player > Near";
                case FieldFlag.VoiceDistanceFar: return "Player > Far";
                case FieldFlag.VoiceVolumetricRadius: return "Player > Volumetric Radius";
                case FieldFlag.VoiceLowpass: return "Player > Lowpass";
                case FieldFlag.AvatarAudioGain: return "Avatar > Gain";
                case FieldFlag.AvatarAudioDistanceNear: return "Avatar > Near";
                case FieldFlag.AvatarAudioDistanceFar: return "Avatar > Far";
                case FieldFlag.AvatarAudioVolumetricRadius: return "Avatar > Volumetric Radius";
                case FieldFlag.AvatarAudioForceSpatial: return "Avatar > Force Spatial";
                default: return "Filter";
            }
        }

        static int CountBits(int v)
        {
            var c = 0;
            while (v != 0) { c += v & 1; v >>= 1; }
            return c;
        }

        static void DrawFloat(Rect rect, GUIContent label, SerializedProperty property, float defaultValue, float maxValue)
        {
            var toggleRect = rect;
            toggleRect.width = EditorGUIUtility.labelWidth + 20;
            var valueRect = rect;
            valueRect.xMin += toggleRect.width;

            using (new EditorGUI.PropertyScope(rect, label, property))
            {
                bool effective;
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    effective = EditorGUI.ToggleLeft(toggleRect, label, property.floatValue >= 0f);
                    if (check.changed) property.floatValue = effective ? defaultValue : -1f;
                }
                if (effective) EditorGUI.Slider(valueRect, property, 0f, maxValue, GUIContent.none);
            }
        }

        static void DrawBool(Rect rect, GUIContent label, SerializedProperty property, SerializedProperty enableProperty, bool defaultValue)
        {
            var toggleRect = rect;
            toggleRect.width = EditorGUIUtility.labelWidth + 20;
            var valueRect = rect;
            valueRect.xMin += toggleRect.width;

            using (new EditorGUI.PropertyScope(toggleRect, label, enableProperty))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    enableProperty.boolValue = EditorGUI.ToggleLeft(toggleRect, label, enableProperty.boolValue);
                    if (check.changed) property.boolValue = enableProperty.boolValue ? defaultValue : false;
                }
            }
            if (enableProperty.boolValue) EditorGUI.PropertyField(valueRect, property, GUIContent.none);
        }

        static void DrawCompactFloat(Rect rect, GUIContent label, SerializedProperty property, float defaultValue, float maxValue)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var step = line + EditorGUIUtility.standardVerticalSpacing;
            var toggleRect = new Rect(rect.x, rect.y, rect.width, line);
            var valueRect = new Rect(rect.x, rect.y + step, rect.width, line);

            using (new EditorGUI.PropertyScope(rect, label, property))
            {
                bool effective;
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    effective = EditorGUI.ToggleLeft(toggleRect, label, property.floatValue >= 0f);
                    if (check.changed) property.floatValue = effective ? defaultValue : -1f;
                }
                if (!effective) return;
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var v = EditorGUI.FloatField(valueRect, property.floatValue);
                    if (check.changed) property.floatValue = Mathf.Clamp(v, 0f, maxValue);
                }
            }
        }

        static void DrawCompactBool(Rect rect, GUIContent label, SerializedProperty property, SerializedProperty enableProperty, bool defaultValue)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var step = line + EditorGUIUtility.standardVerticalSpacing;
            var toggleRect = new Rect(rect.x, rect.y, rect.width, line);
            var valueRect = new Rect(rect.x, rect.y + step, rect.width, line);

            using (new EditorGUI.PropertyScope(toggleRect, label, enableProperty))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    enableProperty.boolValue = EditorGUI.ToggleLeft(toggleRect, label, enableProperty.boolValue);
                    if (check.changed) property.boolValue = enableProperty.boolValue ? defaultValue : false;
                }
            }
            if (enableProperty.boolValue) EditorGUI.PropertyField(valueRect, property, GUIContent.none);
        }

        static Rect[] SplitHorizontal(Rect rect, int columns)
        {
            var rects = new Rect[columns];
            var width = rect.width / columns;
            for (var i = 0; i < columns; i++)
            {
                rects[i] = new Rect(rect.x + width * i, rect.y, width, rect.height);
            }
            return rects;
        }
    }
}
