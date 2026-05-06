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
        PlayerVolumeGroup[] froms;

        public abstract bool _ContainsPlayer(VRCPlayerApi player);

        protected virtual void OnEnable()
        {
            var len = _overrides.Length;
            froms = new PlayerVolumeGroup[len];
            for (var i = 0; i < len; i++)
            {
                froms[i] = _overrides[i] == null ? null : _overrides[i]._from;
            }
        }

        public bool[] _ApplyVolumesWithOverride(PlayerVolumeSetting parent, VRCPlayerApi player, PlayerVolumeGroup[] fromGroups, bool[] set)
        {
            if (fromGroups == null || fromGroups.Length == 0)
            {
                return _ApplyVolumes(player, new PlayerVolumeSetting[] {this, parent}, set);
            }

            var len = fromGroups.Length;
            var settings = new PlayerVolumeSetting[len + 2];
            for (var i = 0; i < len; i++)
            {
                var settingIndex = Array.IndexOf(froms, fromGroups[i]);
                settings[i] = settingIndex == -1 ? (PlayerVolumeSetting)this : (PlayerVolumeSetting)_overrides[settingIndex];
            }
            settings[len] = this;
            settings[len + 1] = parent;
            return _ApplyVolumes(player, settings, set);
        }
    }
}
