using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    [CustomPropertyDrawer(typeof(InlinePlayerVolumeSettingByGroupAttribute))]
    public class InlinePlayerVolumeSettingByGroupDrawer : PropertyDrawer
    {
        sealed class Entry
        {
            public SerializedObject SerializedObject;
            public PlayerVolumeSettingGUI.Properties Properties;
        }

        static readonly Dictionary<int, Entry> _cache = new Dictionary<int, Entry>();
        static readonly Regex _arrayIndexRegex = new Regex(@"\.Array\.data\[(\d+)\]$");

        static GUIContent FoldoutLabel(PlayerVolumeGroup fromGroup, SerializedProperty property, GUIContent fallback)
        {
            if (fromGroup == null) return fallback;
            var owner = property.serializedObject.targetObject as PlayerVolumeGroup;
            var text = fromGroup == owner ? "(Same Group)" : fromGroup.name;
            return new GUIContent(text, fallback.tooltip);
        }

        static Entry GetEntry(PlayerVolumeSettingByGroup target)
        {
            var id = target.GetInstanceID();
            if (_cache.TryGetValue(id, out var entry) &&
                entry.SerializedObject != null &&
                entry.SerializedObject.targetObject == target)
            {
                return entry;
            }
            var so = new SerializedObject(target);
            entry = new Entry
            {
                SerializedObject = so,
                Properties = PlayerVolumeSettingGUI.Properties.Create(so),
            };
            _cache[id] = entry;
            return entry;
        }

        static int ExtractArrayIndex(string propertyPath)
        {
            var m = _arrayIndexRegex.Match(propertyPath);
            return m.Success ? int.Parse(m.Groups[1].Value) : -1;
        }

        static readonly GUIContent OverrideLabel = new GUIContent("Override");

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            var line = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded) return line;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            // header line + setting line + body
            return line + spacing + line + spacing + PlayerVolumeSettingGUI.GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            using (new EditorGUI.PropertyScope(position, label, property))
            {
                var line = EditorGUIUtility.singleLineHeight;
                var spacing = EditorGUIUtility.standardVerticalSpacing;

                var headerRect = new Rect(position.x, position.y, position.width, line);
                var foldoutRect = headerRect;
                foldoutRect.width = EditorGUIUtility.labelWidth;
                var objectRect = headerRect;
                objectRect.xMin = headerRect.x + EditorGUIUtility.labelWidth;

                var fromGroup = property.objectReferenceValue as PlayerVolumeGroup;

                {
                    var expanded = property.isExpanded;
                    var newExpanded = EditorGUI.Foldout(foldoutRect, expanded, FoldoutLabel(fromGroup, property, label), true);
                    if (newExpanded != expanded) property.isExpanded = newExpanded;
                }

                EditorGUI.ObjectField(objectRect, property, typeof(PlayerVolumeGroup), GUIContent.none);

                if (!property.isExpanded) return;

                var index = ExtractArrayIndex(property.propertyPath);
                if (index < 0) return;

                var ownerSO = property.serializedObject;
                var listenOverridesProp = ownerSO.FindProperty(nameof(PlayerVolumeGroup._listenOverrides));
                if (listenOverridesProp == null) return;
                while (listenOverridesProp.arraySize <= index)
                {
                    listenOverridesProp.InsertArrayElementAtIndex(listenOverridesProp.arraySize);
                    listenOverridesProp.GetArrayElementAtIndex(listenOverridesProp.arraySize - 1).objectReferenceValue = null;
                }
                var settingProp = listenOverridesProp.GetArrayElementAtIndex(index);

                var settingRect = new Rect(position.x, headerRect.yMax + spacing, position.width, line);
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUI.PropertyField(settingRect, settingProp, OverrideLabel);
                }
                var setting = settingProp.objectReferenceValue as PlayerVolumeSettingByGroup;

                var bodyRect = new Rect(
                    position.x,
                    settingRect.yMax + spacing,
                    position.width,
                    PlayerVolumeSettingGUI.GetHeight());

                var groupProperties = PlayerVolumeSettingGUI.Properties.Create(ownerSO);
                groupProperties.Fallback = PlayerVolumeSettingGUI.GetManagerFallbackProperties();

                using (new EditorGUI.IndentLevelScope())
                {
                    if (setting == null)
                    {
                        var placeholder = new PlayerVolumeSettingGUI.Properties { Fallback = groupProperties };
                        PlayerVolumeSettingGUI.Draw(bodyRect, placeholder);
                        var activated = PlayerVolumeSettingGUI.ConsumePlaceholderActivated();
                        if (activated != PlayerVolumeSettingGUI.FieldFlag.None)
                        {
                            var owner = ownerSO.targetObject as PlayerVolumeGroup;
                            var newSetting = PlayerVolumeSettingGUI.CreateListenOverrideSetting(owner, fromGroup);
                            settingProp.objectReferenceValue = newSetting;
                            ownerSO.ApplyModifiedProperties();

                            var settingSO = new SerializedObject(newSetting);
                            PlayerVolumeSettingGUI.ApplyActivatedFields(settingSO, activated, groupProperties);
                            settingSO.ApplyModifiedProperties();
                        }
                    }
                    else
                    {
                        var entry = GetEntry(setting);
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
}
