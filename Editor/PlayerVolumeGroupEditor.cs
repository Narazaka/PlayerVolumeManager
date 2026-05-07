using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    [CustomEditor(typeof(PlayerVolumeGroup), true)]
    public class PlayerVolumeGroupEditor : PlayerVolumeSettingEditor
    {
        SerializedProperty _matchWhenListener;
        SerializedProperty _matchWhenSpeaker;
        SerializedProperty _fallbackToNextGroup;
        SerializedProperty _listenFromGroups;
        SerializedProperty _listenOverrides;

        protected override void OnEnable()
        {
            base.OnEnable();
            _matchWhenListener = serializedObject.FindProperty(nameof(PlayerVolumeGroup._matchWhenListener));
            _matchWhenSpeaker = serializedObject.FindProperty(nameof(PlayerVolumeGroup._matchWhenSpeaker));
            _fallbackToNextGroup = serializedObject.FindProperty(nameof(PlayerVolumeGroup._fallbackToNextGroup));
            _listenFromGroups = serializedObject.FindProperty(nameof(PlayerVolumeGroup._listenFromGroups));
            _listenOverrides = serializedObject.FindProperty(nameof(PlayerVolumeGroup._listenOverrides));
        }

        public override void Draw()
        {
            DrawMisc();
            if (_matchWhenListener.boolValue)
            {
                PlayerVolumeSettingGUI.DrawModeDropdownLayout();

                DrawOverrides();

                DrawHeader("Listen Default");
                using (new EditorGUI.IndentLevelScope())
                {
                    base.DrawVolumeSetting();
                }

                DrawConsistencyWarning();
            }
        }

        void DrawConsistencyWarning()
        {
            var missing = PlayerVolumeSettingGUI.FindMissingFallback(target as PlayerVolumeGroup, PlayerVolumeSettingGUI.GetManager());
            if (missing.Count == 0) return;
            var sb = new System.Text.StringBuilder();
            foreach (var name in missing) sb.AppendLine($"  - {name}");
            EditorGUILayout.HelpBox(
                "以下の項目は override で値が設定されていますが、このグループの Listen Default にも Manager にもフォールバック値がありません:\n" + sb.ToString(),
                MessageType.Warning);
        }

        private protected override PlayerVolumeSettingGUI.Properties BuildFallback() =>
            PlayerVolumeSettingGUI.GetManagerFallbackProperties();

        protected override HashSet<string> KnownProperties
        {
            get
            {
                if (_knownProperties == null)
                {
                    _knownProperties = new HashSet<string>(base.KnownProperties);
                    _knownProperties.Add(nameof(PlayerVolumeGroup._matchWhenListener));
                    _knownProperties.Add(nameof(PlayerVolumeGroup._matchWhenSpeaker));
                    _knownProperties.Add(nameof(PlayerVolumeGroup._fallbackToNextGroup));
                    _knownProperties.Add(nameof(PlayerVolumeGroup._listenFromGroups));
                    _knownProperties.Add(nameof(PlayerVolumeGroup._listenOverrides));
                }
                return _knownProperties;
            }
        }

        static HashSet<string> _knownProperties = null;

        void DrawMisc()
        {
            EditorGUILayout.PropertyField(_matchWhenListener);
            EditorGUILayout.PropertyField(_matchWhenSpeaker);
            EditorGUILayout.PropertyField(_fallbackToNextGroup);
        }

        void DrawOverrides()
        {
            // Snapshot before user interaction so we can detect reorder.
            var beforeFromGroups = SnapshotObjectArray(_listenFromGroups);
            var beforeOverrides = SnapshotObjectArray(_listenOverrides);

            EditorGUILayout.PropertyField(_listenFromGroups, true);
            if (GUILayout.Button("Fill Overrides"))
            {
                FillOverrides();
                return;
            }

            var afterFromGroups = SnapshotObjectArray(_listenFromGroups);
            if (!SequenceEqual(beforeFromGroups, afterFromGroups))
            {
                ApplyReorderToOverrides(beforeFromGroups, beforeOverrides, afterFromGroups);
            }
        }

        static Object[] SnapshotObjectArray(SerializedProperty arrayProp)
        {
            var n = arrayProp.arraySize;
            var result = new Object[n];
            for (var i = 0; i < n; i++)
            {
                result[i] = arrayProp.GetArrayElementAtIndex(i).objectReferenceValue;
            }
            return result;
        }

        static bool SequenceEqual(Object[] a, Object[] b)
        {
            if (a.Length != b.Length) return false;
            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        void ApplyReorderToOverrides(Object[] beforeFromGroups, Object[] beforeOverrides, Object[] afterFromGroups)
        {
            var len = afterFromGroups.Length;
            _listenOverrides.arraySize = len;
            var consumed = new bool[beforeFromGroups.Length];
            var matched = new bool[len];
            var settings = new Object[len];

            // Pass 1: same-group reorder.
            // Each after slot consumes a unique before slot whose group matches. Duplicate group
            // references and multiple null/orphan rows are handled because slots are consumed
            // first-come-first-served instead of always falling back to the first IndexOf.
            for (var i = 0; i < len; i++)
            {
                var g = afterFromGroups[i];
                for (var idx = 0; idx < beforeFromGroups.Length; idx++)
                {
                    if (consumed[idx]) continue;
                    if (beforeFromGroups[idx] != g) continue;
                    if (idx < beforeOverrides.Length) settings[i] = beforeOverrides[idx];
                    consumed[idx] = true;
                    matched[i] = true;
                    break;
                }
            }

            // Pass 2: same-index fallback.
            // The slot stayed at index i but its group reference changed (orphan filled /
            // replaced with another group / cleared to null). Inherit the same-index setting
            // so user edits are not silently dropped. New rows appended by "+" land here too
            // but their before-side index is out of range, so they end up with null (placeholder).
            for (var i = 0; i < len; i++)
            {
                if (matched[i]) continue;
                if (i >= beforeFromGroups.Length || consumed[i]) continue;
                if (i < beforeOverrides.Length) settings[i] = beforeOverrides[i];
                consumed[i] = true;
            }

            // Apply
            for (var i = 0; i < len; i++)
            {
                _listenOverrides.GetArrayElementAtIndex(i).objectReferenceValue = settings[i];
            }
        }

        void FillOverrides()
        {
            // Drop entries whose group reference is null (cleanup pass moved here from SyncOverrideArrays).
            for (var i = _listenFromGroups.arraySize - 1; i >= 0; i--)
            {
                if (_listenFromGroups.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    _listenFromGroups.DeleteArrayElementAtIndex(i);
                    if (i < _listenOverrides.arraySize)
                    {
                        _listenOverrides.GetArrayElementAtIndex(i).objectReferenceValue = null;
                        _listenOverrides.DeleteArrayElementAtIndex(i);
                    }
                }
            }

            var ownerGroup = target as PlayerVolumeGroup;

            // Pick up existing settings under ListenOverrides parent (by GameObject name).
            var existingSettings = new Dictionary<string, PlayerVolumeSettingByGroup>();
            var overridesParent = ownerGroup.transform.Find(PlayerVolumeSettingGUI.ListenOverridesParentName);
            if (overridesParent != null)
            {
                foreach (Transform child in overridesParent)
                {
                    var s = child.GetComponent<PlayerVolumeSettingByGroup>();
                    if (s != null) existingSettings[child.name] = s;
                }
            }

            // Snapshot current (group -> setting) mapping.
            var currentMap = new Dictionary<PlayerVolumeGroup, PlayerVolumeSettingByGroup>();
            for (var i = 0; i < _listenFromGroups.arraySize; i++)
            {
                var g = _listenFromGroups.GetArrayElementAtIndex(i).objectReferenceValue as PlayerVolumeGroup;
                if (g == null) continue;
                var s = i < _listenOverrides.arraySize
                    ? _listenOverrides.GetArrayElementAtIndex(i).objectReferenceValue as PlayerVolumeSettingByGroup
                    : null;
                if (s == null) existingSettings.TryGetValue(g.name, out s);
                currentMap[g] = s;
            }

            // Decide ordering: prefer Manager._groups order, then leftovers.
            var orderedGroups = new List<PlayerVolumeGroup>();
            var manager = PlayerVolumeSettingGUI.GetManager();
            if (manager != null)
            {
                var managerSO = new SerializedObject(manager);
                var managerGroupsProp = managerSO.FindProperty("_groups");
                if (managerGroupsProp != null)
                {
                    for (var i = 0; i < managerGroupsProp.arraySize; i++)
                    {
                        var g = managerGroupsProp.GetArrayElementAtIndex(i).objectReferenceValue as PlayerVolumeGroup;
                        if (g != null && !orderedGroups.Contains(g)) orderedGroups.Add(g);
                    }
                }
            }
            else
            {
                foreach (var g in FindObjectsByType<PlayerVolumeGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (g != null && !orderedGroups.Contains(g)) orderedGroups.Add(g);
                }
            }
            // Append entries that exist in current registration but not in Manager (preserve user data).
            foreach (var kv in currentMap)
            {
                if (!orderedGroups.Contains(kv.Key)) orderedGroups.Add(kv.Key);
            }

            // Write back arrays in the new order.
            _listenFromGroups.arraySize = orderedGroups.Count;
            _listenOverrides.arraySize = orderedGroups.Count;
            for (var i = 0; i < orderedGroups.Count; i++)
            {
                var g = orderedGroups[i];
                _listenFromGroups.GetArrayElementAtIndex(i).objectReferenceValue = g;
                if (!currentMap.TryGetValue(g, out var setting))
                {
                    existingSettings.TryGetValue(g.name, out setting);
                }
                _listenOverrides.GetArrayElementAtIndex(i).objectReferenceValue = setting;
            }
        }
    }
}
