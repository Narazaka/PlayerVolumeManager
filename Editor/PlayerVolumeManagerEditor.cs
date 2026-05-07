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
            DrawHeader("Listen Default");
            using (new EditorGUI.IndentLevelScope())
            {
                base.Draw();
            }
            DrawConsistencyWarning();
        }

        void DrawConsistencyWarning()
        {
            var manager = target as PlayerVolumeManager;
            var groups = new List<PlayerVolumeGroup>();
            for (var i = 0; i < _groups.arraySize; i++)
            {
                var g = _groups.GetArrayElementAtIndex(i).objectReferenceValue as PlayerVolumeGroup;
                if (g != null && g._matchWhenListener) groups.Add(g);
            }

            var orderedProps = new List<string>();
            var propToReasons = new Dictionary<string, List<string>>();
            void Add(string prop, string reason)
            {
                if (!propToReasons.TryGetValue(prop, out var list))
                {
                    list = new List<string>();
                    propToReasons[prop] = list;
                    orderedProps.Add(prop);
                }
                list.Add(reason);
            }

            foreach (var group in groups)
            {
                foreach (var prop in PlayerVolumeSettingGUI.FindMissingFallback(group, manager))
                {
                    Add(prop, group.name);
                }
            }
            foreach (var (prop, needFallbackGroups) in PlayerVolumeSettingGUI.FindGroupMixedFields(groups, manager))
            {
                foreach (var groupName in needFallbackGroups)
                {
                    Add(prop, $"{groupName} の Listen Default");
                }
            }

            if (orderedProps.Count == 0) return;

            var sb = new System.Text.StringBuilder();
            foreach (var prop in orderedProps)
            {
                sb.AppendLine($"  - {prop}: {string.Join(", ", propToReasons[prop])}");
            }
            EditorGUILayout.HelpBox(
                "以下の項目は設定の有無が揃っておらず、フォールバック値が決まらない箇所があります。Manager または下記の各箇所に値を設定してください:\n" + sb.ToString(),
                MessageType.Warning);
        }

        protected override HashSet<string> KnownProperties
        {
            get
            {
                if (_knownProperties == null)
                {
                    _knownProperties = new HashSet<string>(base.KnownProperties);
                    _knownProperties.Add(nameof(_groups));
                    _knownProperties.Add(nameof(_debugLog));
                }
                return _knownProperties;
            }
        }

        static HashSet<string> _knownProperties = null;

        void DrawGroups()
        {
            EditorGUILayout.PropertyField(_groups, true);
            if (GUILayout.Button("Detect Groups"))
            {
                DetectGroups();
            }
        }

        void DrawDebugLog()
        {
            EditorGUILayout.PropertyField(_debugLog);
        }

        void DetectGroups()
        {
            var foundGroups = FindObjectsByType<PlayerVolumeGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = _groups.arraySize - 1; i >= 0; i--)
            {
                if (_groups.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    _groups.DeleteArrayElementAtIndex(i);
                }
            }
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
