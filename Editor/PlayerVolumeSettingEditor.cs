using UnityEditor;
using UdonSharpEditor;
using System.Collections.Generic;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    [CustomEditor(typeof(PlayerVolumeSetting), true)]
    public class PlayerVolumeSettingEditor : UnityEditor.Editor
    {
        PlayerVolumeSettingGUI.Properties _properties;

        protected virtual void OnEnable()
        {
            _properties = PlayerVolumeSettingGUI.Properties.Create(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.UpdateIfRequiredOrScript();
            DrawAll();
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawAll()
        {
            var iterator = serializedObject.GetIterator();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                if (!KnownProperties.Contains(iterator.propertyPath))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }

                enterChildren = false;
            }

            Draw();
        }

        public virtual void Draw()
        {
            PlayerVolumeSettingGUI.DrawModeDropdownLayout();
            DrawVolumeSetting();
        }

        protected void DrawVolumeSetting()
        {
            _properties.Fallback = BuildFallback();
            try
            {
                var rect = EditorGUILayout.GetControlRect(false, PlayerVolumeSettingGUI.GetHeight());
                PlayerVolumeSettingGUI.Draw(rect, _properties);
            }
            finally
            {
                _properties.Fallback = null;
            }
        }

        private protected virtual PlayerVolumeSettingGUI.Properties BuildFallback() => null;

        protected virtual HashSet<string> KnownProperties => _knownProperties;

        static HashSet<string> _knownProperties = new HashSet<string>
        {
            "m_Script",
            nameof(PlayerVolumeSetting._voiceGain),
            nameof(PlayerVolumeSetting._voiceDistanceNear),
            nameof(PlayerVolumeSetting._voiceDistanceFar),
            nameof(PlayerVolumeSetting._voiceVolumetricRadius),
            nameof(PlayerVolumeSetting._enableVoiceLowpass),
            nameof(PlayerVolumeSetting._voiceLowpass),
            nameof(PlayerVolumeSetting._avatarAudioGain),
            nameof(PlayerVolumeSetting._avatarAudioDistanceNear),
            nameof(PlayerVolumeSetting._avatarAudioDistanceFar),
            nameof(PlayerVolumeSetting._avatarAudioVolumetricRadius),
            nameof(PlayerVolumeSetting._enableAvatarAudioForceSpatial),
            nameof(PlayerVolumeSetting._avatarAudioForceSpatial),
        };

        protected void DrawHeader(string label) => EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
    }
}
