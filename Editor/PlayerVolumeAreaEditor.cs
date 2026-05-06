using UnityEngine;
using UnityEditor;
using UdonSharpEditor;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    [CustomEditor(typeof(PlayerVolumeArea))]
    public class PlayerVolumeAreaEditor : PlayerVolumeGroupEditor
    {
        SerializedProperty _targets;
        SerializedProperty _isStatic;

        protected override void OnEnable()
        {
            base.OnEnable();
            _targets = serializedObject.FindProperty(nameof(_targets));
            _isStatic = serializedObject.FindProperty(nameof(_isStatic));
        }

        public override void Draw()
        {
            EditorGUILayout.PropertyField(_targets, true);
            EditorGUILayout.PropertyField(_isStatic);
            base.Draw();
        }
    }
}
