using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    public abstract class PlayerVolumeGroup : PlayerVolumeSetting
    {
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

        public void _ApplyVolumesWithOverride(PlayerVolumeSetting parent, VRCPlayerApi player, PlayerVolumeGroup playerGroup)
        {
            if (playerGroup == null)
            {
                _ApplyVolumes(parent, player);
                return;
            }
            var settingIndex = Array.IndexOf(groups, playerGroup);
            if (settingIndex < 0)
            {
                _ApplyVolumes(parent, player);
                return;
            }
            _ApplyVolumes(parent, player, _overrides[settingIndex]);
        }
    }
}
