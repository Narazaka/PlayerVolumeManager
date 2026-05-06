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
        const int LineCount = 13;

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
            return EditorGUIUtility.singleLineHeight * LineCount
                 + EditorGUIUtility.standardVerticalSpacing * (LineCount - 1);
        }

        public static void Draw(Rect rect, Properties p)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var step = line + EditorGUIUtility.standardVerticalSpacing;
            var r = new Rect(rect.x, rect.y, rect.width, line);

            DrawHeader(r, "Player Volume Settings"); r.y += step;
            DrawFloat(r, Labels.VoiceGain, p.VoiceGain, DefaultVoiceGain, MaxVoiceGain); r.y += step;
            DrawFloat(r, Labels.VoiceDistanceNear, p.VoiceDistanceNear, DefaultVoiceDistanceNear, MaxVoiceDistanceNear); r.y += step;
            DrawFloat(r, Labels.VoiceDistanceFar, p.VoiceDistanceFar, DefaultVoiceDistanceFar, MaxVoiceDistanceFar); r.y += step;
            DrawFloat(r, Labels.VoiceVolumetricRadius, p.VoiceVolumetricRadius, DefaultVoiceVolumetricRadius, MaxVoiceVolumetricRadius); r.y += step;
            DrawBool(r, Labels.VoiceLowPass, p.VoiceLowpass, p.EnableVoiceLowpass, DefaultVoiceLowpass); r.y += step;
            r.y += step;
            DrawHeader(r, "Avatar Audio Settings"); r.y += step;
            DrawFloat(r, Labels.AvatarAudioGain, p.AvatarAudioGain, DefaultAvatarAudioGain, MaxAvatarAudioGain); r.y += step;
            DrawFloat(r, Labels.AvatarAudioDistanceNear, p.AvatarAudioDistanceNear, DefaultAvatarAudioDistanceNear, MaxAvatarAudioDistanceNear); r.y += step;
            DrawFloat(r, Labels.AvatarAudioDistanceFar, p.AvatarAudioDistanceFar, DefaultAvatarAudioDistanceFar, MaxAvatarAudioDistanceFar); r.y += step;
            DrawFloat(r, Labels.AvatarAudioVolumetricRadius, p.AvatarAudioVolumetricRadius, DefaultAvatarAudioVolumetricRadius, MaxAvatarAudioVolumetricRadius); r.y += step;
            DrawBool(r, Labels.AvatarAudioForceSpatial, p.AvatarAudioForceSpatial, p.EnableAvatarAudioForceSpatial, DefaultAvatarAudioForceSpatial);
        }

        static void DrawHeader(Rect rect, string label)
        {
            EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
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
