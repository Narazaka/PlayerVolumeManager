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
            var settings = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                beforeFromGroups, beforeOverrides, afterFromGroups);
            _listenOverrides.arraySize = settings.Length;
            for (var i = 0; i < settings.Length; i++)
            {
                _listenOverrides.GetArrayElementAtIndex(i).objectReferenceValue = settings[i];
            }
        }

        void FillOverrides()
        {
            var ownerGroup = target as PlayerVolumeGroup;

            // Snapshot existing arrays.
            var existingFromGroups = new PlayerVolumeGroup[_listenFromGroups.arraySize];
            for (var i = 0; i < existingFromGroups.Length; i++)
            {
                existingFromGroups[i] = _listenFromGroups.GetArrayElementAtIndex(i).objectReferenceValue as PlayerVolumeGroup;
            }
            var existingOverrides = new PlayerVolumeSettingByGroup[_listenOverrides.arraySize];
            for (var i = 0; i < existingOverrides.Length; i++)
            {
                existingOverrides[i] = _listenOverrides.GetArrayElementAtIndex(i).objectReferenceValue as PlayerVolumeSettingByGroup;
            }

            // Existing settings under the ListenOverrides parent (by GameObject name).
            var settingsByName = new Dictionary<string, PlayerVolumeSettingByGroup>();
            var overridesParent = ownerGroup.transform.Find(PlayerVolumeSettingGUI.ListenOverridesParentName);
            if (overridesParent != null)
            {
                foreach (Transform child in overridesParent)
                {
                    var s = child.GetComponent<PlayerVolumeSettingByGroup>();
                    if (s != null) settingsByName[child.name] = s;
                }
            }

            // Manager order (or scene scan as a fallback when there is no Manager yet).
            PlayerVolumeGroup[] managerGroups;
            var manager = PlayerVolumeSettingGUI.GetManager();
            if (manager != null)
            {
                var managerSO = new SerializedObject(manager);
                var managerGroupsProp = managerSO.FindProperty("_groups");
                if (managerGroupsProp != null)
                {
                    managerGroups = new PlayerVolumeGroup[managerGroupsProp.arraySize];
                    for (var i = 0; i < managerGroups.Length; i++)
                    {
                        managerGroups[i] = managerGroupsProp.GetArrayElementAtIndex(i).objectReferenceValue as PlayerVolumeGroup;
                    }
                }
                else
                {
                    managerGroups = new PlayerVolumeGroup[0];
                }
            }
            else
            {
                managerGroups = FindObjectsByType<PlayerVolumeGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            }

            var (newFromGroups, newOverrides) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                existingFromGroups, existingOverrides, managerGroups, settingsByName);

            _listenFromGroups.arraySize = newFromGroups.Length;
            _listenOverrides.arraySize = newOverrides.Length;
            for (var i = 0; i < newFromGroups.Length; i++)
            {
                _listenFromGroups.GetArrayElementAtIndex(i).objectReferenceValue = newFromGroups[i];
                _listenOverrides.GetArrayElementAtIndex(i).objectReferenceValue = newOverrides[i];
            }
        }
    }
}
