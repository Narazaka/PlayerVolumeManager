using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    internal static class PlayerVolumeSettingGUI
    {
        const float DefaultVoiceGain = 15f;
        const float DefaultVoiceDistanceNear = 0f;
        const float DefaultVoiceDistanceFar = 25f;
        const float DefaultVoiceVolumetricRadius = 0f;
        const bool DefaultVoiceLowpass = true;
        const float DefaultAvatarAudioGain = 10f;
        const float DefaultAvatarAudioDistanceNear = 0f;
        const float DefaultAvatarAudioDistanceFar = 40f;
        const float DefaultAvatarAudioVolumetricRadius = 0f;
        const bool DefaultAvatarAudioForceSpatial = false;

        const float MaxVoiceGain = 24f;
        const float MaxVoiceDistanceNear = 1000000f;
        const float MaxVoiceDistanceFar = 1000000f;
        const float MaxVoiceVolumetricRadius = 1000f;
        const float MaxAvatarAudioGain = 10f;
        const float MaxAvatarAudioDistanceNear = 1000000f;
        const float MaxAvatarAudioDistanceFar = 1000000f;
        const float MaxAvatarAudioVolumetricRadius = 1000f;

        // Header(1) + Voice(5) + Header(1) + AvatarAudio(5)
        const int FullLineCount = 12;
        // Header + toggle row + value row, twice
        const int CompactLineCount = 6;

        const string ModePrefKey = "Narazaka.VRChat.PlayerVolumeManager.InspectorMode";
        const string FilterMaskPrefKey = "Narazaka.VRChat.PlayerVolumeManager.InspectorFilterMask";
        const string PairSortModePrefKey = "Narazaka.VRChat.PlayerVolumeManager.PairSortMode";

        public const string ListenOverridesParentName = "ListenOverrides";

        public static PlayerVolumeSettingByGroup CreateListenOverrideSetting(PlayerVolumeListenPair owner)
        {
            var settingName = owner._group != null ? owner._group.name : "ListenOverride";
            var setting = new GameObject(settingName).AddComponent<PlayerVolumeSettingByGroup>();
            setting.transform.SetParent(owner.transform, false);
            Undo.RegisterCreatedObjectUndo(setting.gameObject, "Create Listen Override Setting");
            return setting;
        }

        public enum DisplayMode
        {
            Full = 0,
            Compact = 1,
            Filter = 2,
        }

        [System.Flags]
        public enum FieldFlag
        {
            None = 0,
            VoiceGain = 1 << 0,
            VoiceDistanceNear = 1 << 1,
            VoiceDistanceFar = 1 << 2,
            VoiceVolumetricRadius = 1 << 3,
            VoiceLowpass = 1 << 4,
            AvatarAudioGain = 1 << 5,
            AvatarAudioDistanceNear = 1 << 6,
            AvatarAudioDistanceFar = 1 << 7,
            AvatarAudioVolumetricRadius = 1 << 8,
            AvatarAudioForceSpatial = 1 << 9,
        }

        const FieldFlag VoiceMask =
            FieldFlag.VoiceGain | FieldFlag.VoiceDistanceNear | FieldFlag.VoiceDistanceFar
            | FieldFlag.VoiceVolumetricRadius | FieldFlag.VoiceLowpass;
        const FieldFlag AvatarMask =
            FieldFlag.AvatarAudioGain | FieldFlag.AvatarAudioDistanceNear | FieldFlag.AvatarAudioDistanceFar
            | FieldFlag.AvatarAudioVolumetricRadius | FieldFlag.AvatarAudioForceSpatial;

        // Activate flag for placeholder mode (consumed by Drawer to auto-create a setting object).
        static FieldFlag _placeholderActivated;
        public static FieldFlag ConsumePlaceholderActivated()
        {
            var was = _placeholderActivated;
            _placeholderActivated = FieldFlag.None;
            return was;
        }

        sealed class ManagerEntry
        {
            public PlayerVolumeManager Manager;
            public SerializedObject SerializedObject;
            public Properties Properties;
        }

        static ManagerEntry _managerEntry;

        public static Properties GetManagerFallbackProperties()
        {
            EnsureManagerEntry();
            if (_managerEntry == null) return null;
            _managerEntry.SerializedObject.UpdateIfRequiredOrScript();
            return _managerEntry.Properties;
        }

        public static PlayerVolumeManager GetManager()
        {
            EnsureManagerEntry();
            return _managerEntry?.Manager;
        }

        static void EnsureManagerEntry()
        {
            if (_managerEntry != null && _managerEntry.Manager != null) return;
            var found = UnityEngine.Object.FindObjectsByType<PlayerVolumeManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (found.Length == 0)
            {
                _managerEntry = null;
                return;
            }
            var manager = found[0];
            var so = new SerializedObject(manager);
            _managerEntry = new ManagerEntry
            {
                Manager = manager,
                SerializedObject = so,
                Properties = Properties.Create(so),
            };
        }

        public static List<string> FindMissingFallback(PlayerVolumeGroup group, IReadOnlyList<PlayerVolumeSettingByGroup> overrides, PlayerVolumeSetting manager)
        {
            var missing = new List<string>();
            if (group == null) return missing;
            if (overrides == null || overrides.Count == 0) return missing;

            if (group._voiceGain < 0f && (manager == null || manager._voiceGain < 0f) && AnyOverrideFloat(overrides, ov => ov._voiceGain))
                missing.Add("Player Gain");
            if (group._voiceDistanceNear < 0f && (manager == null || manager._voiceDistanceNear < 0f) && AnyOverrideFloat(overrides, ov => ov._voiceDistanceNear))
                missing.Add("Player Near");
            if (group._voiceDistanceFar < 0f && (manager == null || manager._voiceDistanceFar < 0f) && AnyOverrideFloat(overrides, ov => ov._voiceDistanceFar))
                missing.Add("Player Far");
            if (group._voiceVolumetricRadius < 0f && (manager == null || manager._voiceVolumetricRadius < 0f) && AnyOverrideFloat(overrides, ov => ov._voiceVolumetricRadius))
                missing.Add("Player Volumetric Radius");
            if (!group._enableVoiceLowpass && (manager == null || !manager._enableVoiceLowpass) && AnyOverrideBool(overrides, ov => ov._enableVoiceLowpass))
                missing.Add("Player Lowpass");
            if (group._avatarAudioGain < 0f && (manager == null || manager._avatarAudioGain < 0f) && AnyOverrideFloat(overrides, ov => ov._avatarAudioGain))
                missing.Add("Avatar Gain");
            if (group._avatarAudioDistanceNear < 0f && (manager == null || manager._avatarAudioDistanceNear < 0f) && AnyOverrideFloat(overrides, ov => ov._avatarAudioDistanceNear))
                missing.Add("Avatar Near");
            if (group._avatarAudioDistanceFar < 0f && (manager == null || manager._avatarAudioDistanceFar < 0f) && AnyOverrideFloat(overrides, ov => ov._avatarAudioDistanceFar))
                missing.Add("Avatar Far");
            if (group._avatarAudioVolumetricRadius < 0f && (manager == null || manager._avatarAudioVolumetricRadius < 0f) && AnyOverrideFloat(overrides, ov => ov._avatarAudioVolumetricRadius))
                missing.Add("Avatar Volumetric Radius");
            if (!group._enableAvatarAudioForceSpatial && (manager == null || !manager._enableAvatarAudioForceSpatial) && AnyOverrideBool(overrides, ov => ov._enableAvatarAudioForceSpatial))
                missing.Add("Avatar Force Spatial");
            return missing;
        }

        static bool AnyOverrideFloat(IReadOnlyList<PlayerVolumeSettingByGroup> overrides, Func<PlayerVolumeSettingByGroup, float> getter)
        {
            for (var i = 0; i < overrides.Count; i++)
            {
                var ov = overrides[i];
                if (ov != null && getter(ov) >= 0f) return true;
            }
            return false;
        }

        static bool AnyOverrideBool(IReadOnlyList<PlayerVolumeSettingByGroup> overrides, Func<PlayerVolumeSettingByGroup, bool> getter)
        {
            for (var i = 0; i < overrides.Count; i++)
            {
                var ov = overrides[i];
                if (ov != null && getter(ov)) return true;
            }
            return false;
        }

        public static List<(string Prop, List<string> NeedFallbackGroups)> FindGroupMixedFields(IReadOnlyList<PlayerVolumeGroup> groups, PlayerVolumeSetting manager)
        {
            var result = new List<(string, List<string>)>();
            if (groups == null || groups.Count == 0) return result;

            TryAddMixed(result, "Player Gain", manager == null || manager._voiceGain < 0f, groups, g => g._voiceGain >= 0f);
            TryAddMixed(result, "Player Near", manager == null || manager._voiceDistanceNear < 0f, groups, g => g._voiceDistanceNear >= 0f);
            TryAddMixed(result, "Player Far", manager == null || manager._voiceDistanceFar < 0f, groups, g => g._voiceDistanceFar >= 0f);
            TryAddMixed(result, "Player Volumetric Radius", manager == null || manager._voiceVolumetricRadius < 0f, groups, g => g._voiceVolumetricRadius >= 0f);
            TryAddMixed(result, "Player Lowpass", manager == null || !manager._enableVoiceLowpass, groups, g => g._enableVoiceLowpass);
            TryAddMixed(result, "Avatar Gain", manager == null || manager._avatarAudioGain < 0f, groups, g => g._avatarAudioGain >= 0f);
            TryAddMixed(result, "Avatar Near", manager == null || manager._avatarAudioDistanceNear < 0f, groups, g => g._avatarAudioDistanceNear >= 0f);
            TryAddMixed(result, "Avatar Far", manager == null || manager._avatarAudioDistanceFar < 0f, groups, g => g._avatarAudioDistanceFar >= 0f);
            TryAddMixed(result, "Avatar Volumetric Radius", manager == null || manager._avatarAudioVolumetricRadius < 0f, groups, g => g._avatarAudioVolumetricRadius >= 0f);
            TryAddMixed(result, "Avatar Force Spatial", manager == null || !manager._enableAvatarAudioForceSpatial, groups, g => g._enableAvatarAudioForceSpatial);
            return result;
        }

        static void TryAddMixed(List<(string, List<string>)> result, string prop, bool managerOff, IReadOnlyList<PlayerVolumeGroup> groups, Func<PlayerVolumeGroup, bool> isOn)
        {
            if (!managerOff) return;
            var hasOn = false;
            var offGroups = new List<string>();
            for (var i = 0; i < groups.Count; i++)
            {
                var g = groups[i];
                if (g == null) continue;
                if (isOn(g)) hasOn = true;
                else offGroups.Add(g.name);
            }
            if (hasOn && offGroups.Count > 0)
            {
                result.Add((prop, offGroups));
            }
        }

        public static DisplayMode GetMode() => (DisplayMode)EditorPrefs.GetInt(ModePrefKey, 0);
        public static void SetMode(DisplayMode value) => EditorPrefs.SetInt(ModePrefKey, (int)value);
        public static FieldFlag GetFilterMask() => (FieldFlag)EditorPrefs.GetInt(FilterMaskPrefKey, (int)FieldFlag.VoiceGain);
        public static void SetFilterMask(FieldFlag value) => EditorPrefs.SetInt(FilterMaskPrefKey, (int)value);

        public static PlayerVolumeListenPairView.SortMode GetPairSortMode()
            => (PlayerVolumeListenPairView.SortMode)EditorPrefs.GetInt(PairSortModePrefKey, 0);
        public static void SetPairSortMode(PlayerVolumeListenPairView.SortMode value)
            => EditorPrefs.SetInt(PairSortModePrefKey, (int)value);

        public sealed class Properties
        {
            public Properties Fallback;
            public SerializedProperty VoiceGain;
            public SerializedProperty VoiceDistanceNear;
            public SerializedProperty VoiceDistanceFar;
            public SerializedProperty VoiceVolumetricRadius;
            public SerializedProperty EnableVoiceLowpass;
            public SerializedProperty VoiceLowpass;
            public SerializedProperty AvatarAudioGain;
            public SerializedProperty AvatarAudioDistanceNear;
            public SerializedProperty AvatarAudioDistanceFar;
            public SerializedProperty AvatarAudioVolumetricRadius;
            public SerializedProperty EnableAvatarAudioForceSpatial;
            public SerializedProperty AvatarAudioForceSpatial;

            public static Properties Create(SerializedObject so) => new Properties
            {
                VoiceGain = so.FindProperty(nameof(PlayerVolumeSetting._voiceGain)),
                VoiceDistanceNear = so.FindProperty(nameof(PlayerVolumeSetting._voiceDistanceNear)),
                VoiceDistanceFar = so.FindProperty(nameof(PlayerVolumeSetting._voiceDistanceFar)),
                VoiceVolumetricRadius = so.FindProperty(nameof(PlayerVolumeSetting._voiceVolumetricRadius)),
                EnableVoiceLowpass = so.FindProperty(nameof(PlayerVolumeSetting._enableVoiceLowpass)),
                VoiceLowpass = so.FindProperty(nameof(PlayerVolumeSetting._voiceLowpass)),
                AvatarAudioGain = so.FindProperty(nameof(PlayerVolumeSetting._avatarAudioGain)),
                AvatarAudioDistanceNear = so.FindProperty(nameof(PlayerVolumeSetting._avatarAudioDistanceNear)),
                AvatarAudioDistanceFar = so.FindProperty(nameof(PlayerVolumeSetting._avatarAudioDistanceFar)),
                AvatarAudioVolumetricRadius = so.FindProperty(nameof(PlayerVolumeSetting._avatarAudioVolumetricRadius)),
                EnableAvatarAudioForceSpatial = so.FindProperty(nameof(PlayerVolumeSetting._enableAvatarAudioForceSpatial)),
                AvatarAudioForceSpatial = so.FindProperty(nameof(PlayerVolumeSetting._avatarAudioForceSpatial)),
            };
        }

        sealed class Labels
        {
            public static GUIContent VoiceGain = new GUIContent("Gain");
            public static GUIContent VoiceDistanceNear = new GUIContent("Near");
            public static GUIContent VoiceDistanceFar = new GUIContent("Far");
            public static GUIContent VoiceVolumetricRadius = new GUIContent("Volumetric Radius");
            public static GUIContent VoiceLowPass = new GUIContent("Lowpass");
            public static GUIContent AvatarAudioGain = new GUIContent("Gain");
            public static GUIContent AvatarAudioDistanceNear = new GUIContent("Near");
            public static GUIContent AvatarAudioDistanceFar = new GUIContent("Far");
            public static GUIContent AvatarAudioVolumetricRadius = new GUIContent("Volumetric Radius");
            public static GUIContent AvatarAudioForceSpatial = new GUIContent("Force Spatial");
        }

        // Static getters used to walk the fallback chain without per-call lambda allocations.
        static readonly Func<Properties, SerializedProperty> GetVoiceGain = p => p.VoiceGain;
        static readonly Func<Properties, SerializedProperty> GetVoiceDistanceNear = p => p.VoiceDistanceNear;
        static readonly Func<Properties, SerializedProperty> GetVoiceDistanceFar = p => p.VoiceDistanceFar;
        static readonly Func<Properties, SerializedProperty> GetVoiceVolumetricRadius = p => p.VoiceVolumetricRadius;
        static readonly Func<Properties, SerializedProperty> GetEnableVoiceLowpass = p => p.EnableVoiceLowpass;
        static readonly Func<Properties, SerializedProperty> GetVoiceLowpass = p => p.VoiceLowpass;
        static readonly Func<Properties, SerializedProperty> GetAvatarAudioGain = p => p.AvatarAudioGain;
        static readonly Func<Properties, SerializedProperty> GetAvatarAudioDistanceNear = p => p.AvatarAudioDistanceNear;
        static readonly Func<Properties, SerializedProperty> GetAvatarAudioDistanceFar = p => p.AvatarAudioDistanceFar;
        static readonly Func<Properties, SerializedProperty> GetAvatarAudioVolumetricRadius = p => p.AvatarAudioVolumetricRadius;
        static readonly Func<Properties, SerializedProperty> GetEnableAvatarAudioForceSpatial = p => p.EnableAvatarAudioForceSpatial;
        static readonly Func<Properties, SerializedProperty> GetAvatarAudioForceSpatial = p => p.AvatarAudioForceSpatial;

        static float? ResolveFloat(Properties p, Func<Properties, SerializedProperty> getter)
        {
            while (p != null)
            {
                var prop = getter(p);
                if (prop != null && prop.floatValue >= 0f) return prop.floatValue;
                p = p.Fallback;
            }
            return null;
        }

        static bool? ResolveBool(Properties p,
            Func<Properties, SerializedProperty> enableGetter,
            Func<Properties, SerializedProperty> valueGetter)
        {
            while (p != null)
            {
                var enableProp = enableGetter(p);
                if (enableProp != null && enableProp.boolValue) return valueGetter(p).boolValue;
                p = p.Fallback;
            }
            return null;
        }

        public static float GetHeight()
        {
            int lines;
            switch (GetMode())
            {
                case DisplayMode.Full: lines = FullLineCount; break;
                case DisplayMode.Compact: lines = CompactLineCount; break;
                default: lines = GetFilterLineCount(GetFilterMask()); break;
            }
            return EditorGUIUtility.singleLineHeight * lines
                 + EditorGUIUtility.standardVerticalSpacing * (lines - 1);
        }

        static int GetFilterLineCount(FieldFlag mask)
        {
            var voiceCount = CountBits((int)(mask & VoiceMask));
            var avatarCount = CountBits((int)(mask & AvatarMask));
            var lines = 0;
            if (voiceCount > 0) lines += 1 + voiceCount;
            if (avatarCount > 0) lines += 1 + avatarCount;
            return lines == 0 ? 1 : lines;
        }

        public static void Draw(Rect rect, Properties p)
        {
            switch (GetMode())
            {
                case DisplayMode.Full: DrawFull(rect, p); break;
                case DisplayMode.Compact: DrawCompact(rect, p); break;
                default: DrawFilter(rect, p); break;
            }
        }

        public static void ApplyActivatedFields(SerializedObject so, FieldFlag activated, Properties fallback)
        {
            if ((activated & FieldFlag.VoiceGain) != 0) ApplyFloat(so, nameof(PlayerVolumeSetting._voiceGain), DefaultVoiceGain, ResolveFloat(fallback, GetVoiceGain));
            if ((activated & FieldFlag.VoiceDistanceNear) != 0) ApplyFloat(so, nameof(PlayerVolumeSetting._voiceDistanceNear), DefaultVoiceDistanceNear, ResolveFloat(fallback, GetVoiceDistanceNear));
            if ((activated & FieldFlag.VoiceDistanceFar) != 0) ApplyFloat(so, nameof(PlayerVolumeSetting._voiceDistanceFar), DefaultVoiceDistanceFar, ResolveFloat(fallback, GetVoiceDistanceFar));
            if ((activated & FieldFlag.VoiceVolumetricRadius) != 0) ApplyFloat(so, nameof(PlayerVolumeSetting._voiceVolumetricRadius), DefaultVoiceVolumetricRadius, ResolveFloat(fallback, GetVoiceVolumetricRadius));
            if ((activated & FieldFlag.VoiceLowpass) != 0) ApplyBool(so, nameof(PlayerVolumeSetting._enableVoiceLowpass), nameof(PlayerVolumeSetting._voiceLowpass), DefaultVoiceLowpass, ResolveBool(fallback, GetEnableVoiceLowpass, GetVoiceLowpass));
            if ((activated & FieldFlag.AvatarAudioGain) != 0) ApplyFloat(so, nameof(PlayerVolumeSetting._avatarAudioGain), DefaultAvatarAudioGain, ResolveFloat(fallback, GetAvatarAudioGain));
            if ((activated & FieldFlag.AvatarAudioDistanceNear) != 0) ApplyFloat(so, nameof(PlayerVolumeSetting._avatarAudioDistanceNear), DefaultAvatarAudioDistanceNear, ResolveFloat(fallback, GetAvatarAudioDistanceNear));
            if ((activated & FieldFlag.AvatarAudioDistanceFar) != 0) ApplyFloat(so, nameof(PlayerVolumeSetting._avatarAudioDistanceFar), DefaultAvatarAudioDistanceFar, ResolveFloat(fallback, GetAvatarAudioDistanceFar));
            if ((activated & FieldFlag.AvatarAudioVolumetricRadius) != 0) ApplyFloat(so, nameof(PlayerVolumeSetting._avatarAudioVolumetricRadius), DefaultAvatarAudioVolumetricRadius, ResolveFloat(fallback, GetAvatarAudioVolumetricRadius));
            if ((activated & FieldFlag.AvatarAudioForceSpatial) != 0) ApplyBool(so, nameof(PlayerVolumeSetting._enableAvatarAudioForceSpatial), nameof(PlayerVolumeSetting._avatarAudioForceSpatial), DefaultAvatarAudioForceSpatial, ResolveBool(fallback, GetEnableAvatarAudioForceSpatial, GetAvatarAudioForceSpatial));
        }

        static void ApplyFloat(SerializedObject so, string propertyName, float defaultValue, float? fallback)
        {
            so.FindProperty(propertyName).floatValue = fallback ?? defaultValue;
        }

        static void ApplyBool(SerializedObject so, string enableName, string valueName, bool defaultValue, bool? fallback)
        {
            so.FindProperty(enableName).boolValue = true;
            so.FindProperty(valueName).boolValue = fallback ?? defaultValue;
        }

        static readonly GUIContent ModeLabel = new GUIContent("(Listen Setting Filter)");

        public static void DrawModeDropdownLayout()
        {
            var rect = EditorGUILayout.GetControlRect();
            var dropdownRect = EditorGUI.PrefixLabel(rect, ModeLabel);
            var content = new GUIContent(GetCurrentModeLabel());
            if (EditorGUI.DropdownButton(dropdownRect, content, FocusType.Keyboard))
            {
                ShowModeMenu(dropdownRect);
            }
        }

        static void DrawFull(Rect rect, Properties p)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var step = line + EditorGUIUtility.standardVerticalSpacing;
            var r = new Rect(rect.x, rect.y, rect.width, line);
            var fb = p.Fallback;

            DrawHeader(r, "Player Volume"); r.y += step;
            DrawFloat(r, Labels.VoiceGain, p.VoiceGain, DefaultVoiceGain, MaxVoiceGain, ResolveFloat(fb, GetVoiceGain), FieldFlag.VoiceGain); r.y += step;
            DrawFloat(r, Labels.VoiceDistanceNear, p.VoiceDistanceNear, DefaultVoiceDistanceNear, MaxVoiceDistanceNear, ResolveFloat(fb, GetVoiceDistanceNear), FieldFlag.VoiceDistanceNear); r.y += step;
            DrawFloat(r, Labels.VoiceDistanceFar, p.VoiceDistanceFar, DefaultVoiceDistanceFar, MaxVoiceDistanceFar, ResolveFloat(fb, GetVoiceDistanceFar), FieldFlag.VoiceDistanceFar); r.y += step;
            DrawFloat(r, Labels.VoiceVolumetricRadius, p.VoiceVolumetricRadius, DefaultVoiceVolumetricRadius, MaxVoiceVolumetricRadius, ResolveFloat(fb, GetVoiceVolumetricRadius), FieldFlag.VoiceVolumetricRadius); r.y += step;
            DrawBool(r, Labels.VoiceLowPass, p.VoiceLowpass, p.EnableVoiceLowpass, DefaultVoiceLowpass, ResolveBool(fb, GetEnableVoiceLowpass, GetVoiceLowpass), FieldFlag.VoiceLowpass); r.y += step;
            DrawHeader(r, "Avatar Audio"); r.y += step;
            DrawFloat(r, Labels.AvatarAudioGain, p.AvatarAudioGain, DefaultAvatarAudioGain, MaxAvatarAudioGain, ResolveFloat(fb, GetAvatarAudioGain), FieldFlag.AvatarAudioGain); r.y += step;
            DrawFloat(r, Labels.AvatarAudioDistanceNear, p.AvatarAudioDistanceNear, DefaultAvatarAudioDistanceNear, MaxAvatarAudioDistanceNear, ResolveFloat(fb, GetAvatarAudioDistanceNear), FieldFlag.AvatarAudioDistanceNear); r.y += step;
            DrawFloat(r, Labels.AvatarAudioDistanceFar, p.AvatarAudioDistanceFar, DefaultAvatarAudioDistanceFar, MaxAvatarAudioDistanceFar, ResolveFloat(fb, GetAvatarAudioDistanceFar), FieldFlag.AvatarAudioDistanceFar); r.y += step;
            DrawFloat(r, Labels.AvatarAudioVolumetricRadius, p.AvatarAudioVolumetricRadius, DefaultAvatarAudioVolumetricRadius, MaxAvatarAudioVolumetricRadius, ResolveFloat(fb, GetAvatarAudioVolumetricRadius), FieldFlag.AvatarAudioVolumetricRadius); r.y += step;
            DrawBool(r, Labels.AvatarAudioForceSpatial, p.AvatarAudioForceSpatial, p.EnableAvatarAudioForceSpatial, DefaultAvatarAudioForceSpatial, ResolveBool(fb, GetEnableAvatarAudioForceSpatial, GetAvatarAudioForceSpatial), FieldFlag.AvatarAudioForceSpatial);
        }

        static void DrawCompact(Rect rect, Properties p)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var step = line + spacing;
            var sectionHeight = line + spacing + line;
            var r = new Rect(rect.x, rect.y, rect.width, line);
            var fb = p.Fallback;

            DrawHeader(r, "Player Volume"); r.y += step;
            var voiceCols = SplitHorizontal(new Rect(r.x, r.y, r.width, sectionHeight), 5);
            DrawCompactFloat(voiceCols[0], Labels.VoiceGain, p.VoiceGain, DefaultVoiceGain, MaxVoiceGain, ResolveFloat(fb, GetVoiceGain), FieldFlag.VoiceGain);
            DrawCompactFloat(voiceCols[1], Labels.VoiceDistanceNear, p.VoiceDistanceNear, DefaultVoiceDistanceNear, MaxVoiceDistanceNear, ResolveFloat(fb, GetVoiceDistanceNear), FieldFlag.VoiceDistanceNear);
            DrawCompactFloat(voiceCols[2], Labels.VoiceDistanceFar, p.VoiceDistanceFar, DefaultVoiceDistanceFar, MaxVoiceDistanceFar, ResolveFloat(fb, GetVoiceDistanceFar), FieldFlag.VoiceDistanceFar);
            DrawCompactFloat(voiceCols[3], Labels.VoiceVolumetricRadius, p.VoiceVolumetricRadius, DefaultVoiceVolumetricRadius, MaxVoiceVolumetricRadius, ResolveFloat(fb, GetVoiceVolumetricRadius), FieldFlag.VoiceVolumetricRadius);
            DrawCompactBool(voiceCols[4], Labels.VoiceLowPass, p.VoiceLowpass, p.EnableVoiceLowpass, DefaultVoiceLowpass, ResolveBool(fb, GetEnableVoiceLowpass, GetVoiceLowpass), FieldFlag.VoiceLowpass);
            r.y += sectionHeight + spacing;

            DrawHeader(r, "Avatar Audio"); r.y += step;
            var avatarCols = SplitHorizontal(new Rect(r.x, r.y, r.width, sectionHeight), 5);
            DrawCompactFloat(avatarCols[0], Labels.AvatarAudioGain, p.AvatarAudioGain, DefaultAvatarAudioGain, MaxAvatarAudioGain, ResolveFloat(fb, GetAvatarAudioGain), FieldFlag.AvatarAudioGain);
            DrawCompactFloat(avatarCols[1], Labels.AvatarAudioDistanceNear, p.AvatarAudioDistanceNear, DefaultAvatarAudioDistanceNear, MaxAvatarAudioDistanceNear, ResolveFloat(fb, GetAvatarAudioDistanceNear), FieldFlag.AvatarAudioDistanceNear);
            DrawCompactFloat(avatarCols[2], Labels.AvatarAudioDistanceFar, p.AvatarAudioDistanceFar, DefaultAvatarAudioDistanceFar, MaxAvatarAudioDistanceFar, ResolveFloat(fb, GetAvatarAudioDistanceFar), FieldFlag.AvatarAudioDistanceFar);
            DrawCompactFloat(avatarCols[3], Labels.AvatarAudioVolumetricRadius, p.AvatarAudioVolumetricRadius, DefaultAvatarAudioVolumetricRadius, MaxAvatarAudioVolumetricRadius, ResolveFloat(fb, GetAvatarAudioVolumetricRadius), FieldFlag.AvatarAudioVolumetricRadius);
            DrawCompactBool(avatarCols[4], Labels.AvatarAudioForceSpatial, p.AvatarAudioForceSpatial, p.EnableAvatarAudioForceSpatial, DefaultAvatarAudioForceSpatial, ResolveBool(fb, GetEnableAvatarAudioForceSpatial, GetAvatarAudioForceSpatial), FieldFlag.AvatarAudioForceSpatial);
        }

        static void DrawFilter(Rect rect, Properties p)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var step = line + EditorGUIUtility.standardVerticalSpacing;
            var r = new Rect(rect.x, rect.y, rect.width, line);
            var mask = GetFilterMask();
            var hasVoice = (mask & VoiceMask) != 0;
            var hasAvatar = (mask & AvatarMask) != 0;
            var fb = p.Fallback;

            if (hasVoice)
            {
                DrawHeader(r, "Player Volume"); r.y += step;
                if ((mask & FieldFlag.VoiceGain) != 0) { DrawFloat(r, Labels.VoiceGain, p.VoiceGain, DefaultVoiceGain, MaxVoiceGain, ResolveFloat(fb, GetVoiceGain), FieldFlag.VoiceGain); r.y += step; }
                if ((mask & FieldFlag.VoiceDistanceNear) != 0) { DrawFloat(r, Labels.VoiceDistanceNear, p.VoiceDistanceNear, DefaultVoiceDistanceNear, MaxVoiceDistanceNear, ResolveFloat(fb, GetVoiceDistanceNear), FieldFlag.VoiceDistanceNear); r.y += step; }
                if ((mask & FieldFlag.VoiceDistanceFar) != 0) { DrawFloat(r, Labels.VoiceDistanceFar, p.VoiceDistanceFar, DefaultVoiceDistanceFar, MaxVoiceDistanceFar, ResolveFloat(fb, GetVoiceDistanceFar), FieldFlag.VoiceDistanceFar); r.y += step; }
                if ((mask & FieldFlag.VoiceVolumetricRadius) != 0) { DrawFloat(r, Labels.VoiceVolumetricRadius, p.VoiceVolumetricRadius, DefaultVoiceVolumetricRadius, MaxVoiceVolumetricRadius, ResolveFloat(fb, GetVoiceVolumetricRadius), FieldFlag.VoiceVolumetricRadius); r.y += step; }
                if ((mask & FieldFlag.VoiceLowpass) != 0) { DrawBool(r, Labels.VoiceLowPass, p.VoiceLowpass, p.EnableVoiceLowpass, DefaultVoiceLowpass, ResolveBool(fb, GetEnableVoiceLowpass, GetVoiceLowpass), FieldFlag.VoiceLowpass); r.y += step; }
            }
            if (hasAvatar)
            {
                DrawHeader(r, "Avatar Audio"); r.y += step;
                if ((mask & FieldFlag.AvatarAudioGain) != 0) { DrawFloat(r, Labels.AvatarAudioGain, p.AvatarAudioGain, DefaultAvatarAudioGain, MaxAvatarAudioGain, ResolveFloat(fb, GetAvatarAudioGain), FieldFlag.AvatarAudioGain); r.y += step; }
                if ((mask & FieldFlag.AvatarAudioDistanceNear) != 0) { DrawFloat(r, Labels.AvatarAudioDistanceNear, p.AvatarAudioDistanceNear, DefaultAvatarAudioDistanceNear, MaxAvatarAudioDistanceNear, ResolveFloat(fb, GetAvatarAudioDistanceNear), FieldFlag.AvatarAudioDistanceNear); r.y += step; }
                if ((mask & FieldFlag.AvatarAudioDistanceFar) != 0) { DrawFloat(r, Labels.AvatarAudioDistanceFar, p.AvatarAudioDistanceFar, DefaultAvatarAudioDistanceFar, MaxAvatarAudioDistanceFar, ResolveFloat(fb, GetAvatarAudioDistanceFar), FieldFlag.AvatarAudioDistanceFar); r.y += step; }
                if ((mask & FieldFlag.AvatarAudioVolumetricRadius) != 0) { DrawFloat(r, Labels.AvatarAudioVolumetricRadius, p.AvatarAudioVolumetricRadius, DefaultAvatarAudioVolumetricRadius, MaxAvatarAudioVolumetricRadius, ResolveFloat(fb, GetAvatarAudioVolumetricRadius), FieldFlag.AvatarAudioVolumetricRadius); r.y += step; }
                if ((mask & FieldFlag.AvatarAudioForceSpatial) != 0) { DrawBool(r, Labels.AvatarAudioForceSpatial, p.AvatarAudioForceSpatial, p.EnableAvatarAudioForceSpatial, DefaultAvatarAudioForceSpatial, ResolveBool(fb, GetEnableAvatarAudioForceSpatial, GetAvatarAudioForceSpatial), FieldFlag.AvatarAudioForceSpatial); r.y += step; }
            }
        }

        static void DrawHeader(Rect rect, string label)
        {
            EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
        }

        static void ShowModeMenu(Rect rect)
        {
            var menu = new GenericMenu();
            var mode = GetMode();
            var mask = GetFilterMask();
            menu.AddItem(new GUIContent("Full"), mode == DisplayMode.Full, () => SetMode(DisplayMode.Full));
            menu.AddItem(new GUIContent("Compact"), mode == DisplayMode.Compact, () => SetMode(DisplayMode.Compact));
            menu.AddSeparator("");
            AddFieldItem(menu, mode, mask, FieldFlag.VoiceGain, "Player > Gain");
            AddFieldItem(menu, mode, mask, FieldFlag.VoiceDistanceNear, "Player > Near");
            AddFieldItem(menu, mode, mask, FieldFlag.VoiceDistanceFar, "Player > Far");
            AddFieldItem(menu, mode, mask, FieldFlag.VoiceVolumetricRadius, "Player > Volumetric Radius");
            AddFieldItem(menu, mode, mask, FieldFlag.VoiceLowpass, "Player > Lowpass");
            AddFieldItem(menu, mode, mask, FieldFlag.AvatarAudioGain, "Avatar > Gain");
            AddFieldItem(menu, mode, mask, FieldFlag.AvatarAudioDistanceNear, "Avatar > Near");
            AddFieldItem(menu, mode, mask, FieldFlag.AvatarAudioDistanceFar, "Avatar > Far");
            AddFieldItem(menu, mode, mask, FieldFlag.AvatarAudioVolumetricRadius, "Avatar > Volumetric Radius");
            AddFieldItem(menu, mode, mask, FieldFlag.AvatarAudioForceSpatial, "Avatar > Force Spatial");
            menu.DropDown(rect);
        }

        static void AddFieldItem(GenericMenu menu, DisplayMode mode, FieldFlag mask, FieldFlag flag, string label)
        {
            var on = mode == DisplayMode.Filter ? (mask & flag) != 0 : false;
            menu.AddItem(new GUIContent(label), on, () =>
            {
                if (mode != DisplayMode.Filter)
                {
                    SetFilterMask(flag);
                    SetMode(DisplayMode.Filter);
                }
                else
                {
                    var current = GetFilterMask();
                    var next = (current & flag) != 0 ? current & ~flag : current | flag;
                    if (next == FieldFlag.None) SetMode(DisplayMode.Full);
                    else SetFilterMask(next);
                }
            });
        }

        static string GetCurrentModeLabel()
        {
            switch (GetMode())
            {
                case DisplayMode.Full: return "Full";
                case DisplayMode.Compact: return "Compact";
                default:
                    var mask = GetFilterMask();
                    var count = CountBits((int)mask);
                    if (count == 0) return "Filter";
                    if (count == 1) return GetSingleFieldLabel(mask);
                    return $"Filter ({count})";
            }
        }

        static string GetSingleFieldLabel(FieldFlag flag)
        {
            switch (flag)
            {
                case FieldFlag.VoiceGain: return "Player > Gain";
                case FieldFlag.VoiceDistanceNear: return "Player > Near";
                case FieldFlag.VoiceDistanceFar: return "Player > Far";
                case FieldFlag.VoiceVolumetricRadius: return "Player > Volumetric Radius";
                case FieldFlag.VoiceLowpass: return "Player > Lowpass";
                case FieldFlag.AvatarAudioGain: return "Avatar > Gain";
                case FieldFlag.AvatarAudioDistanceNear: return "Avatar > Near";
                case FieldFlag.AvatarAudioDistanceFar: return "Avatar > Far";
                case FieldFlag.AvatarAudioVolumetricRadius: return "Avatar > Volumetric Radius";
                case FieldFlag.AvatarAudioForceSpatial: return "Avatar > Force Spatial";
                default: return "Filter";
            }
        }

        static int CountBits(int v)
        {
            var c = 0;
            while (v != 0) { c += v & 1; v >>= 1; }
            return c;
        }

        static void DrawFloat(Rect rect, GUIContent label, SerializedProperty property, float defaultValue, float maxValue, float? fallbackValue, FieldFlag flag)
        {
            var toggleRect = rect;
            toggleRect.width = EditorGUIUtility.labelWidth + 20;
            var valueRect = rect;
            valueRect.xMin += toggleRect.width;

            if (property == null)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var on = EditorGUI.ToggleLeft(toggleRect, label, false);
                    if (check.changed && on) _placeholderActivated |= flag;
                }
                if (fallbackValue.HasValue) DrawFallbackFloat(valueRect, fallbackValue.Value, maxValue);
                return;
            }

            using (new EditorGUI.PropertyScope(rect, label, property))
            {
                bool effective;
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    effective = EditorGUI.ToggleLeft(toggleRect, label, property.floatValue >= 0f);
                    if (check.changed) property.floatValue = effective ? (fallbackValue ?? defaultValue) : -1f;
                }
                if (effective) EditorGUI.Slider(valueRect, property, 0f, maxValue, GUIContent.none);
                else if (fallbackValue.HasValue) DrawFallbackFloat(valueRect, fallbackValue.Value, maxValue);
            }
        }

        static void DrawBool(Rect rect, GUIContent label, SerializedProperty property, SerializedProperty enableProperty, bool defaultValue, bool? fallbackValue, FieldFlag flag)
        {
            var toggleRect = rect;
            toggleRect.width = EditorGUIUtility.labelWidth + 20;
            var valueRect = rect;
            valueRect.xMin += toggleRect.width;

            if (property == null || enableProperty == null)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var on = EditorGUI.ToggleLeft(toggleRect, label, false);
                    if (check.changed && on) _placeholderActivated |= flag;
                }
                if (fallbackValue.HasValue) DrawFallbackBool(valueRect, fallbackValue.Value);
                return;
            }

            using (new EditorGUI.PropertyScope(toggleRect, label, enableProperty))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    enableProperty.boolValue = EditorGUI.ToggleLeft(toggleRect, label, enableProperty.boolValue);
                    if (check.changed) property.boolValue = enableProperty.boolValue ? (fallbackValue ?? defaultValue) : false;
                }
            }
            if (enableProperty.boolValue) EditorGUI.PropertyField(valueRect, property, GUIContent.none);
            else if (fallbackValue.HasValue) DrawFallbackBool(valueRect, fallbackValue.Value);
        }

        static void DrawCompactFloat(Rect rect, GUIContent label, SerializedProperty property, float defaultValue, float maxValue, float? fallbackValue, FieldFlag flag)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var step = line + EditorGUIUtility.standardVerticalSpacing;
            var toggleRect = new Rect(rect.x, rect.y, rect.width, line);
            var valueRect = new Rect(rect.x, rect.y + step, rect.width, line);

            if (property == null)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var on = EditorGUI.ToggleLeft(toggleRect, label, false);
                    if (check.changed && on) _placeholderActivated |= flag;
                }
                if (fallbackValue.HasValue) DrawFallbackFloatCompact(valueRect, fallbackValue.Value);
                return;
            }

            using (new EditorGUI.PropertyScope(rect, label, property))
            {
                bool effective;
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    effective = EditorGUI.ToggleLeft(toggleRect, label, property.floatValue >= 0f);
                    if (check.changed) property.floatValue = effective ? (fallbackValue ?? defaultValue) : -1f;
                }
                if (effective)
                {
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        var v = EditorGUI.FloatField(valueRect, property.floatValue);
                        if (check.changed) property.floatValue = Mathf.Clamp(v, 0f, maxValue);
                    }
                }
                else if (fallbackValue.HasValue) DrawFallbackFloatCompact(valueRect, fallbackValue.Value);
            }
        }

        static void DrawCompactBool(Rect rect, GUIContent label, SerializedProperty property, SerializedProperty enableProperty, bool defaultValue, bool? fallbackValue, FieldFlag flag)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var step = line + EditorGUIUtility.standardVerticalSpacing;
            var toggleRect = new Rect(rect.x, rect.y, rect.width, line);
            var valueRect = new Rect(rect.x, rect.y + step, rect.width, line);

            if (property == null || enableProperty == null)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var on = EditorGUI.ToggleLeft(toggleRect, label, false);
                    if (check.changed && on) _placeholderActivated |= flag;
                }
                if (fallbackValue.HasValue) DrawFallbackBool(valueRect, fallbackValue.Value);
                return;
            }

            using (new EditorGUI.PropertyScope(toggleRect, label, enableProperty))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    enableProperty.boolValue = EditorGUI.ToggleLeft(toggleRect, label, enableProperty.boolValue);
                    if (check.changed) property.boolValue = enableProperty.boolValue ? (fallbackValue ?? defaultValue) : false;
                }
            }
            if (enableProperty.boolValue) EditorGUI.PropertyField(valueRect, property, GUIContent.none);
            else if (fallbackValue.HasValue) DrawFallbackBool(valueRect, fallbackValue.Value);
        }

        static void DrawFallbackFloat(Rect valueRect, float value, float maxValue)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.Slider(valueRect, value, 0f, maxValue);
            }
        }

        static void DrawFallbackFloatCompact(Rect valueRect, float value)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.FloatField(valueRect, value);
            }
        }

        static void DrawFallbackBool(Rect valueRect, bool value)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.Toggle(valueRect, value);
            }
        }

        static Rect[] SplitHorizontal(Rect rect, int columns)
        {
            var rects = new Rect[columns];
            var width = rect.width / columns;
            for (var i = 0; i < columns; i++)
            {
                rects[i] = new Rect(rect.x + width * i, rect.y, width, rect.height);
            }
            return rects;
        }
    }
}
