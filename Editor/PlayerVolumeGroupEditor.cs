using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    [CustomEditor(typeof(PlayerVolumeGroup), true)]
    public class PlayerVolumeGroupEditor : PlayerVolumeSettingEditor
    {
        const string OverridesParentName = "Overrides";

        SerializedProperty _fallbackToNextGroup;
        SerializedProperty _overrides;

        protected override void OnEnable()
        {
            base.OnEnable();
            _overrides = serializedObject.FindProperty(nameof(PlayerVolumeGroup._overrides));
            _fallbackToNextGroup = serializedObject.FindProperty(nameof(PlayerVolumeGroup._fallbackToNextGroup));
        }

        public override void Draw()
        {
            DrawFallbackToNextGroup();
            DrawOverrides();

            DrawHeader("Default Setting");
            using (new EditorGUI.IndentLevelScope())
            {
                base.Draw();
            }
        }

        protected override HashSet<string> KnownProperties
        {
            get
            {
                if (_knownProperties == null)
                {
                    _knownProperties = new HashSet<string>(base.KnownProperties);
                    _knownProperties.Add(nameof(PlayerVolumeGroup._fallbackToNextGroup));
                    _knownProperties.Add(nameof(PlayerVolumeGroup._overrides));
                }
                return _knownProperties;
            }
        }

        static HashSet<string> _knownProperties = null;

        void DrawFallbackToNextGroup()
        {
            EditorGUILayout.PropertyField(_fallbackToNextGroup);
        }

        void DrawOverrides()
        {
            EditorGUILayout.PropertyField(_overrides, true);
            if (GUILayout.Button("Fill Overrides"))
            {
                FillOverrides();
            }
        }

        void FillOverrides()
        {
            var foundSettings = (target as PlayerVolumeGroup).GetComponentsInChildren<PlayerVolumeSettingByGroup>();

            var foundGroups = FindObjectsByType<PlayerVolumeGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var existingGroups = new HashSet<PlayerVolumeGroup>();
            var noGroupSettings = new List<int>();
            var noGroupSettingExists = false;
            for (var i = 0; i < _overrides.arraySize; i++)
            {
                var s = _overrides.GetArrayElementAtIndex(i).objectReferenceValue as PlayerVolumeSettingByGroup;
                if (s != null)
                {
                    if (s._group != null)
                    {
                        existingGroups.Add(s._group);
                        if (s.name != s._group.name)
                        {
                            Undo.RecordObject(s.gameObject, "Adjust Setting name");
                            s.gameObject.name = s._group.name;
                        }
                    }
                    else
                    {
                        noGroupSettings.Add(i);
                        noGroupSettingExists = true;
                    }
                }
                else
                {
                    noGroupSettings.Add(i);
                }
            }

            bool destroySettingObjects = false;

            if (noGroupSettingExists)
            {
                destroySettingObjects = EditorUtility.DisplayDialog(
                    "Fill Overrides",
                    "Some settings in the overrides do not have a group assigned. Do you want to delete these settings?",
                    "Yes",
                    "No"
                    );
            }

            noGroupSettings.Reverse();
            foreach (var index in noGroupSettings)
            {
                var s = _overrides.GetArrayElementAtIndex(index).objectReferenceValue as PlayerVolumeSettingByGroup;
                _overrides.DeleteArrayElementAtIndex(index);
                if (s != null && destroySettingObjects)
                {
                    Undo.DestroyObjectImmediate(s.gameObject);
                }
            }

            var newGroups = new HashSet<PlayerVolumeGroup>(foundGroups);
            newGroups.ExceptWith(existingGroups);
            foreach (var group in newGroups)
            {
                var setting = foundSettings.FirstOrDefault(s => s._group == group);
                if (setting == null)
                {
                    setting = CreateSetting(group);
                }
                _overrides.InsertArrayElementAtIndex(_overrides.arraySize);
                _overrides.GetArrayElementAtIndex(_overrides.arraySize - 1).objectReferenceValue = setting;
            }
        }

        PlayerVolumeSettingByGroup CreateSetting(PlayerVolumeGroup group)
        {
            var root = (target as PlayerVolumeGroup).transform;
            var parent = root.Find(OverridesParentName);
            if (parent == null)
            {
                parent = new GameObject(OverridesParentName).transform;
                parent.SetParent(root, false);
                Undo.RegisterCreatedObjectUndo(parent.gameObject, "Create Overrides Parent");
            }
            var setting = new GameObject(group.name).AddComponent<PlayerVolumeSettingByGroup>();
            setting._group = group;
            setting.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(setting.gameObject, "Create Setting");
            return setting;
        }
    }
}
