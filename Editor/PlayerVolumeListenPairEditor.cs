using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    [CustomEditor(typeof(PlayerVolumeListenPair))]
    public class PlayerVolumeListenPairEditor : UnityEditor.Editor
    {
        static class T
        {
            public static readonly istring Group = new istring("Group", "グループ");
            public static readonly istring Setting = new istring("Setting", "設定");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            serializedObject.UpdateIfRequiredOrScript();
            PlayerVolumeSettingGUI.DrawModeDropdownLayout();
            DrawInline(serializedObject, (PlayerVolumeListenPair)target);
            serializedObject.ApplyModifiedProperties();
        }

        sealed class SettingEntry
        {
            public SerializedObject SerializedObject;
            public PlayerVolumeSettingGUI.Properties Properties;
        }

        static readonly Dictionary<int, SettingEntry> _settingCache = new Dictionary<int, SettingEntry>();

        static SettingEntry GetSettingEntry(PlayerVolumeSettingByGroup setting)
        {
            var id = setting.GetInstanceID();
            if (_settingCache.TryGetValue(id, out var entry) &&
                entry.SerializedObject != null &&
                entry.SerializedObject.targetObject == setting)
            {
                return entry;
            }
            var so = new SerializedObject(setting);
            entry = new SettingEntry
            {
                SerializedObject = so,
                Properties = PlayerVolumeSettingGUI.Properties.Create(so),
            };
            _settingCache[id] = entry;
            return entry;
        }

        public static void DrawInline(SerializedObject pairSerializedObject, PlayerVolumeListenPair pair)
        {
            var groupProp = pairSerializedObject.FindProperty(nameof(PlayerVolumeListenPair._group));
            var settingProp = pairSerializedObject.FindProperty(nameof(PlayerVolumeListenPair._setting));

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(groupProp, T.Group.GUIContent);
                if (check.changed)
                {
                    pairSerializedObject.ApplyModifiedProperties();
                    var newGroup = groupProp.objectReferenceValue as PlayerVolumeGroup;
                    var desiredName = newGroup != null ? newGroup.name : "Pair";
                    if (pair.gameObject.name != desiredName)
                    {
                        Undo.RecordObject(pair.gameObject, "Rename Listen Pair");
                        pair.gameObject.name = desiredName;
                    }
                }
            }

            EditorGUILayout.PropertyField(settingProp, T.Setting.GUIContent);

            var bodyRect = EditorGUILayout.GetControlRect(false, PlayerVolumeSettingGUI.GetHeight());

            var ownerGroup = pair.GetComponentInParent<PlayerVolumeGroup>();
            PlayerVolumeSettingGUI.Properties groupProperties = null;
            if (ownerGroup != null)
            {
                var ownerSerializedObject = new SerializedObject(ownerGroup);
                groupProperties = PlayerVolumeSettingGUI.Properties.Create(ownerSerializedObject);
                groupProperties.Fallback = PlayerVolumeSettingGUI.GetManagerFallbackProperties();
            }

            var setting = settingProp.objectReferenceValue as PlayerVolumeSettingByGroup;
            using (new EditorGUI.IndentLevelScope())
            {
                if (setting == null)
                {
                    var placeholder = new PlayerVolumeSettingGUI.Properties { Fallback = groupProperties };
                    PlayerVolumeSettingGUI.Draw(bodyRect, placeholder);
                    var activated = PlayerVolumeSettingGUI.ConsumePlaceholderActivated();
                    if (activated != PlayerVolumeSettingGUI.FieldFlag.None)
                    {
                        var newSetting = PlayerVolumeSettingGUI.CreateListenOverrideSetting(pair);
                        settingProp.objectReferenceValue = newSetting;
                        pairSerializedObject.ApplyModifiedProperties();

                        var newSettingSerializedObject = new SerializedObject(newSetting);
                        PlayerVolumeSettingGUI.ApplyActivatedFields(newSettingSerializedObject, activated, groupProperties);
                        newSettingSerializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    var entry = GetSettingEntry(setting);
                    entry.SerializedObject.UpdateIfRequiredOrScript();
                    entry.Properties.Fallback = groupProperties;
                    PlayerVolumeSettingGUI.Draw(bodyRect, entry.Properties);
                    entry.SerializedObject.ApplyModifiedProperties();
                    entry.Properties.Fallback = null;
                }
            }
        }
    }
}
