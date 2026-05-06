using UnityEditor;
using UdonSharpEditor;

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
            Draw();
            serializedObject.ApplyModifiedProperties();
        }

        public virtual void Draw() => DrawVolumeSetting();

        public void DrawVolumeSetting()
        {
            var rect = EditorGUILayout.GetControlRect(false, PlayerVolumeSettingGUI.GetHeight());
            PlayerVolumeSettingGUI.Draw(rect, _properties);
        }

        public void DrawHeader(string label) => EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
    }
}
