using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    [CustomEditor(typeof(PlayerVolumeGroup), true)]
    public class PlayerVolumeGroupEditor : PlayerVolumeSettingEditor
    {
        const string ListenOverridesParentName = "ListenOverrides";

        SerializedProperty _matchWhenListener;
        SerializedProperty _matchWhenSpeaker;
        SerializedProperty _fallbackToNextGroup;
        SerializedProperty _listenOverrides;

        protected override void OnEnable()
        {
            base.OnEnable();
            _matchWhenListener = serializedObject.FindProperty(nameof(PlayerVolumeGroup._matchWhenListener));
            _matchWhenSpeaker = serializedObject.FindProperty(nameof(PlayerVolumeGroup._matchWhenSpeaker));
            _listenOverrides = serializedObject.FindProperty(nameof(PlayerVolumeGroup._listenOverrides));
            _fallbackToNextGroup = serializedObject.FindProperty(nameof(PlayerVolumeGroup._fallbackToNextGroup));
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
            EditorGUILayout.PropertyField(_listenOverrides, true);
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
            for (var i = 0; i < _listenOverrides.arraySize; i++)
            {
                var s = _listenOverrides.GetArrayElementAtIndex(i).objectReferenceValue as PlayerVolumeSettingByGroup;
                if (s != null)
                {
                    if (s._from != null)
                    {
                        existingGroups.Add(s._from);
                        if (s.name != s._from.name)
                        {
                            Undo.RecordObject(s.gameObject, "Adjust Setting name");
                            s.gameObject.name = s._from.name;
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
                var s = _listenOverrides.GetArrayElementAtIndex(index).objectReferenceValue as PlayerVolumeSettingByGroup;
                _listenOverrides.DeleteArrayElementAtIndex(index);
                if (s != null && destroySettingObjects)
                {
                    Undo.DestroyObjectImmediate(s.gameObject);
                }
            }

            var newGroups = new HashSet<PlayerVolumeGroup>(foundGroups);
            newGroups.ExceptWith(existingGroups);
            foreach (var group in newGroups)
            {
                var setting = foundSettings.FirstOrDefault(s => s._from == group);
                if (setting == null)
                {
                    setting = CreateSetting(group);
                }
                _listenOverrides.InsertArrayElementAtIndex(_listenOverrides.arraySize);
                _listenOverrides.GetArrayElementAtIndex(_listenOverrides.arraySize - 1).objectReferenceValue = setting;
            }
        }

        PlayerVolumeSettingByGroup CreateSetting(PlayerVolumeGroup group)
        {
            var root = (target as PlayerVolumeGroup).transform;
            var parent = root.Find(ListenOverridesParentName);
            if (parent == null)
            {
                parent = new GameObject(ListenOverridesParentName).transform;
                parent.SetParent(root, false);
                Undo.RegisterCreatedObjectUndo(parent.gameObject, "Create Listen Overrides Parent");
            }
            var setting = new GameObject(group.name).AddComponent<PlayerVolumeSettingByGroup>();
            setting._from = group;
            setting.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(setting.gameObject, "Create Listen Override Setting");
            return setting;
        }
    }
}
