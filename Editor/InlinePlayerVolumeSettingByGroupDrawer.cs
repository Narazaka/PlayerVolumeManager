using System.Collections.Generic;
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
            public SerializedProperty Group;
            public PlayerVolumeSettingGUI.Properties Properties;
        }

        static readonly Dictionary<int, Entry> _cache = new Dictionary<int, Entry>();

        static GUIContent FoldoutLabel(PlayerVolumeSettingByGroup target, SerializedProperty property, GUIContent fallback)
        {
            if (target == null) return fallback;
            var group = target._from;
            if (group == null) return fallback;
            var owner = property.serializedObject.targetObject as PlayerVolumeGroup;
            var text = group == owner ? "(Same Group)" : group.name;
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
                Group = so.FindProperty(nameof(PlayerVolumeSettingByGroup._from)),
                Properties = PlayerVolumeSettingGUI.Properties.Create(so),
            };
            _cache[id] = entry;
            return entry;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            var line = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded || property.objectReferenceValue == null) return line;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            // header line + group line + body
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

                var target = property.objectReferenceValue as PlayerVolumeSettingByGroup;
                using (new EditorGUI.DisabledScope(target == null))
                {
                    var expanded = property.isExpanded && target != null;
                    var newExpanded = EditorGUI.Foldout(foldoutRect, expanded, FoldoutLabel(target, property, label), true);
                    if (newExpanded != expanded) property.isExpanded = newExpanded;
                }

                EditorGUI.ObjectField(objectRect, property, typeof(PlayerVolumeSettingByGroup), GUIContent.none);

                if (target == null || !property.isExpanded) return;

                using (new EditorGUI.IndentLevelScope())
                {
                    var entry = GetEntry(target);
                    entry.SerializedObject.UpdateIfRequiredOrScript();

                    var groupRect = new Rect(position.x, position.y + line + spacing, position.width, line);
                    EditorGUI.PropertyField(groupRect, entry.Group);

                    // fallback: parent (PlayerVolumeGroup)
                    var groupProperties = PlayerVolumeSettingGUI.Properties.Create(property.serializedObject);
                    // fallback.fallback: parent/parent (PlayerVolumeManager)
                    groupProperties.Fallback = PlayerVolumeSettingGUI.GetManagerFallbackProperties();
                    entry.Properties.Fallback = groupProperties;

                    var bodyRect = new Rect(
                        position.x,
                        groupRect.yMax + spacing,
                        position.width,
                        PlayerVolumeSettingGUI.GetHeight());
                    PlayerVolumeSettingGUI.Draw(bodyRect, entry.Properties);

                    entry.SerializedObject.ApplyModifiedProperties();
                    entry.Properties.Fallback = null;
                }
            }
        }
    }
}
