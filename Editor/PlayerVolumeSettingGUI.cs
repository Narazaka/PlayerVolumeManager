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

        // Header(1) + Voice(5) + Spacer(1) + Header(1) + AvatarAudio(5)
        const int FullLineCount = 13;
        // Header + toggle row + value row, twice
        const int CompactLineCount = 6;

        const float CompactToggleWidth = 90f;

        const string CompactPrefKey = "Narazaka.VRChat.PlayerVolumeManager.CompactInspector";

        public static bool IsCompact() => EditorPrefs.GetBool(CompactPrefKey, false);
        public static void SetCompact(bool value) => EditorPrefs.SetBool(CompactPrefKey, value);

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
            var lines = IsCompact() ? CompactLineCount : FullLineCount;
            return EditorGUIUtility.singleLineHeight * lines
                 + EditorGUIUtility.standardVerticalSpacing * (lines - 1);
        }

        public static void Draw(Rect rect, Properties p)
        {
            if (IsCompact()) DrawCompact(rect, p);
            else DrawFull(rect, p);
        }

        static void DrawFull(Rect rect, Properties p)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var step = line + EditorGUIUtility.standardVerticalSpacing;
            var r = new Rect(rect.x, rect.y, rect.width, line);

            DrawHeaderWithCompactToggle(r, "Player Volume"); r.y += step;
            DrawFloat(r, Labels.VoiceGain, p.VoiceGain, DefaultVoiceGain, MaxVoiceGain); r.y += step;
            DrawFloat(r, Labels.VoiceDistanceNear, p.VoiceDistanceNear, DefaultVoiceDistanceNear, MaxVoiceDistanceNear); r.y += step;
            DrawFloat(r, Labels.VoiceDistanceFar, p.VoiceDistanceFar, DefaultVoiceDistanceFar, MaxVoiceDistanceFar); r.y += step;
            DrawFloat(r, Labels.VoiceVolumetricRadius, p.VoiceVolumetricRadius, DefaultVoiceVolumetricRadius, MaxVoiceVolumetricRadius); r.y += step;
            DrawBool(r, Labels.VoiceLowPass, p.VoiceLowpass, p.EnableVoiceLowpass, DefaultVoiceLowpass); r.y += step;
            r.y += step;
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

            DrawHeaderWithCompactToggle(r, "Player Volume"); r.y += step;
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

        static void DrawHeader(Rect rect, string label)
        {
            EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
        }

        static void DrawHeaderWithCompactToggle(Rect rect, string label)
        {
            var labelRect = rect; labelRect.width -= CompactToggleWidth;
            EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);

            var toggleRect = rect; toggleRect.xMin = rect.xMax - CompactToggleWidth;
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var compact = EditorGUI.ToggleLeft(toggleRect, "Compact", IsCompact());
                if (check.changed) SetCompact(compact);
            }
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
    }
}
