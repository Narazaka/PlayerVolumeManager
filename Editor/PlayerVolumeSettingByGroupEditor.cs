using UnityEngine;
using UnityEditor;
using UdonSharpEditor;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    [CustomEditor(typeof(PlayerVolumeSettingByGroup))]
    public class PlayerVolumeSettingByGroupEditor : PlayerVolumeSettingEditor
    {
        SerializedProperty _group;

        protected override void OnEnable()
        {
            base.OnEnable();
            _group = serializedObject.FindProperty(nameof(PlayerVolumeSettingByGroup._group));
        }

        public override void Draw()
        {
            DrawGroup();
            base.Draw();
        }

        protected void DrawGroup()
        {
            EditorGUILayout.PropertyField(_group);
        }
    }
}
