using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    public abstract class PlayerVolumeGroup : PlayerVolumeSetting
    {
        public bool _fallbackToNextGroup;
        [InlinePlayerVolumeSettingByGroup]
        public PlayerVolumeSettingByGroup[] _overrides = new PlayerVolumeSettingByGroup[0];
        PlayerVolumeGroup[] groups;

        public abstract bool _ContainsPlayer(VRCPlayerApi player);

        protected virtual void OnEnable()
        {
            var len = _overrides.Length;
            groups = new PlayerVolumeGroup[len];
            for (var i = 0; i < len; i++)
            {
                groups[i] = _overrides[i] == null ? null : _overrides[i]._group;
            }
        }

        public bool[] _ApplyVolumesWithOverride(PlayerVolumeSetting parent, VRCPlayerApi player, PlayerVolumeGroup[] playerGroups, bool[] set)
        {
            if (playerGroups == null || playerGroups.Length == 0)
            {
                return _ApplyVolumes(player, new PlayerVolumeSetting[] {this, parent}, set);
            }

            var len = playerGroups.Length;
            var settings = new PlayerVolumeSetting[len + 2];
            for (var i = 0; i < len; i++)
            {
                var settingIndex = Array.IndexOf(groups, playerGroups[i]);
                settings[i] = settingIndex == -1 ? (PlayerVolumeSetting)this : (PlayerVolumeSetting)_overrides[settingIndex];
            }
            settings[len] = this;
            settings[len + 1] = parent;
            return _ApplyVolumes(player, settings, set);
        }
    }
}
