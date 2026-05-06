using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    [CustomEditor(typeof(PlayerVolumeManager))]
    public class PlayerVolumeManagerEditor : PlayerVolumeSettingEditor
    {
        SerializedProperty _groups;
        SerializedProperty _debugLog;

        protected override void OnEnable()
        {
            base.OnEnable();
            _groups = serializedObject.FindProperty(nameof(_groups));
            _debugLog = serializedObject.FindProperty(nameof(_debugLog));
        }

        public override void Draw()
        {
            DrawGroups();
            DrawDebugLog();
            DrawHeader("Default Setting");
            using (new EditorGUI.IndentLevelScope())
            {
                base.Draw();
            }
        }

        protected void DrawGroups()
        {
            EditorGUILayout.PropertyField(_groups, true);
            if (GUILayout.Button("Detect Groups"))
            {
                DetectGroups();
            }
        }

        protected void DrawDebugLog()
        {
            EditorGUILayout.PropertyField(_debugLog);
        }

        void DetectGroups()
        {
            var foundGroups = FindObjectsByType<PlayerVolumeGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var existingGroups = new HashSet<PlayerVolumeGroup>();
            for (var i = 0; i < _groups.arraySize; i++)
            {
                var group = _groups.GetArrayElementAtIndex(i).objectReferenceValue as PlayerVolumeGroup;
                if (group != null)
                {
                    existingGroups.Add(group);
                }
            }
            var newGroups = new HashSet<PlayerVolumeGroup>(foundGroups);
            newGroups.ExceptWith(existingGroups);
            foreach (var group in newGroups)
            {
                _groups.InsertArrayElementAtIndex(_groups.arraySize);
                _groups.GetArrayElementAtIndex(_groups.arraySize - 1).objectReferenceValue = group;
            }
        }
    }
}
