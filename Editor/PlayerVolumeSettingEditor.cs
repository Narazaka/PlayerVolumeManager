using UnityEngine;
using UnityEditor;
using UdonSharpEditor;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    [CustomEditor(typeof(PlayerVolumeSetting), true)]
    public class PlayerVolumeSettingEditor : UnityEditor.Editor
    {
        SerializedProperty _voiceGain;
        SerializedProperty _voiceDistanceNear;
        SerializedProperty _voiceDistanceFar;
        SerializedProperty _voiceVolumetricRadius;
        SerializedProperty _enableVoiceLowpass;
        SerializedProperty _voiceLowpass;

        SerializedProperty _avatarAudioGain;
        SerializedProperty _avatarAudioDistanceNear;
        SerializedProperty _avatarAudioDistanceFar;
        SerializedProperty _avatarAudioVolumetricRadius;
        SerializedProperty _enableAvatarAudioForceSpatial;
        SerializedProperty _avatarAudioForceSpatial;

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

        protected virtual void OnEnable()
        {
            _voiceGain = serializedObject.FindProperty(nameof(PlayerVolumeSetting._voiceGain));
            _voiceDistanceNear = serializedObject.FindProperty(nameof(PlayerVolumeSetting._voiceDistanceNear));
            _voiceDistanceFar = serializedObject.FindProperty(nameof(PlayerVolumeSetting._voiceDistanceFar));
            _voiceVolumetricRadius = serializedObject.FindProperty(nameof(PlayerVolumeSetting._voiceVolumetricRadius));
            _enableVoiceLowpass = serializedObject.FindProperty(nameof(PlayerVolumeSetting._enableVoiceLowpass));
            _voiceLowpass = serializedObject.FindProperty(nameof(PlayerVolumeSetting._voiceLowpass));
            _avatarAudioGain = serializedObject.FindProperty(nameof(PlayerVolumeSetting._avatarAudioGain));
            _avatarAudioDistanceNear = serializedObject.FindProperty(nameof(PlayerVolumeSetting._avatarAudioDistanceNear));
            _avatarAudioDistanceFar = serializedObject.FindProperty(nameof(PlayerVolumeSetting._avatarAudioDistanceFar));
            _avatarAudioVolumetricRadius = serializedObject.FindProperty(nameof(PlayerVolumeSetting._avatarAudioVolumetricRadius));
            _enableAvatarAudioForceSpatial = serializedObject.FindProperty(nameof(PlayerVolumeSetting._enableAvatarAudioForceSpatial));
            _avatarAudioForceSpatial = serializedObject.FindProperty(nameof(PlayerVolumeSetting._avatarAudioForceSpatial));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.UpdateIfRequiredOrScript();
            Draw();
            serializedObject.ApplyModifiedProperties();
        }

        public virtual void Draw() => DrawVolumeSetting();

        public void DrawVolumeSetting()
        {
            DrawHeader("Player Volume Settings");
            DrawVoiceGain();
            DrawVoiceDistanceNear();
            DrawVoiceDistanceFar();
            DrawVoiceVolumetricRadius();
            DrawVoiceLowpass();
            DrawSpaceLine();
            DrawHeader("Avatar Audio Settings");
            DrawAvatarAudioGain();
            DrawAvatarAudioDistanceNear();
            DrawAvatarAudioDistanceFar();
            DrawAvatarAudioVolumetricRadius();
            DrawAvatarAudioForceSpatial();
        }

        public void DrawHeader(string label) => EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        public void DrawSpaceLine() => EditorGUILayout.Space();

        public void DrawVoiceGain() => DrawFloatProperty(_voiceGain, DefaultVoiceGain, MaxVoiceGain);
        public void DrawVoiceDistanceNear() => DrawFloatProperty(_voiceDistanceNear, DefaultVoiceDistanceNear, MaxVoiceDistanceNear);
        public void DrawVoiceDistanceFar() => DrawFloatProperty(_voiceDistanceFar, DefaultVoiceDistanceFar, MaxVoiceDistanceFar);
        public void DrawVoiceVolumetricRadius() => DrawFloatProperty(_voiceVolumetricRadius, DefaultVoiceVolumetricRadius, MaxVoiceVolumetricRadius);
        public void DrawVoiceLowpass() => DrawBoolProperty(_voiceLowpass, _enableVoiceLowpass, DefaultVoiceLowpass);
        public void DrawAvatarAudioGain() => DrawFloatProperty(_avatarAudioGain, DefaultAvatarAudioGain, MaxAvatarAudioGain);
        public void DrawAvatarAudioDistanceNear() => DrawFloatProperty(_avatarAudioDistanceNear, DefaultAvatarAudioDistanceNear, MaxAvatarAudioDistanceNear);
        public void DrawAvatarAudioDistanceFar() => DrawFloatProperty(_avatarAudioDistanceFar, DefaultAvatarAudioDistanceFar, MaxAvatarAudioDistanceFar);
        public void DrawAvatarAudioVolumetricRadius() => DrawFloatProperty(_avatarAudioVolumetricRadius, DefaultAvatarAudioVolumetricRadius, MaxAvatarAudioVolumetricRadius);
        public void DrawAvatarAudioForceSpatial() => DrawBoolProperty(_avatarAudioForceSpatial, _enableAvatarAudioForceSpatial, DefaultAvatarAudioForceSpatial);

        void DrawFloatProperty(SerializedProperty property, float defaultValue, float maxValue)
        {
            var rect = EditorGUILayout.GetControlRect();
            // Label Toggle FloatSlider
            var effectiveToggleRect = rect;
            effectiveToggleRect.width = EditorGUIUtility.labelWidth + 20;
            var valueRect = rect;
            valueRect.xMin += effectiveToggleRect.width;

            var label = new GUIContent(property.displayName, property.tooltip);
            using (new EditorGUI.PropertyScope(rect, label, property))
            {
                bool effective;
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    effective = EditorGUI.ToggleLeft(effectiveToggleRect, label, property.floatValue >= 0f);
                    if (check.changed)
                    {
                        property.floatValue = effective ? defaultValue : -1f;
                    }
                }
                if (effective) EditorGUI.Slider(valueRect, property, 0f, maxValue, GUIContent.none);
            }
        }

        void DrawBoolProperty(SerializedProperty property, SerializedProperty enableProperty, bool defaultValue)
        {
            var rect = EditorGUILayout.GetControlRect();
            var effectiveToggleRect = rect;
            effectiveToggleRect.width = EditorGUIUtility.labelWidth + 20;
            var valueRect = rect;
            valueRect.xMin += effectiveToggleRect.width;

            var label = new GUIContent(property.displayName, property.tooltip);
            using (new EditorGUI.PropertyScope(effectiveToggleRect, label, enableProperty))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    enableProperty.boolValue = EditorGUI.ToggleLeft(effectiveToggleRect, label, enableProperty.boolValue);
                    if (check.changed)
                    {
                        property.boolValue = enableProperty.boolValue ? defaultValue : false;
                    }
                }
            }
            if (enableProperty.boolValue) EditorGUI.PropertyField(valueRect, property, GUIContent.none);
        }
    }
}
