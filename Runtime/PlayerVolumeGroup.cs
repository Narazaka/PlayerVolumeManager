using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    public abstract class PlayerVolumeGroup : PlayerVolumeSetting
    {
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

        void _ApplyVolumes(VRCPlayerApi player)
        {
            _SetVoiceGain(player);
            _SetVoiceDistanceNear(player);
            _SetVoiceDistanceFar(player);
            _SetVoiceVolumetricRadius(player);
            _SetVoiceLowpass(player);
            _SetAvatarAudioGain(player);
            _SetAvatarAudioDistanceNear(player);
            _SetAvatarAudioDistanceFar(player);
            _SetAvatarAudioVolumetricRadius(player);
            _SetAvatarAudioForceSpatial(player);
        }

        void _ApplyVolumes(PlayerVolumeSetting parent, VRCPlayerApi player)
        {
            if (parent == null)
            {
                _ApplyVolumes(player);
                return;
            }
            if (!_SetVoiceGain(player)) parent._SetVoiceGain(player);
            if (!_SetVoiceDistanceNear(player)) parent._SetVoiceDistanceNear(player);
            if (!_SetVoiceDistanceFar(player)) parent._SetVoiceDistanceFar(player);
            if (!_SetVoiceVolumetricRadius(player)) parent._SetVoiceVolumetricRadius(player);
            if (!_SetVoiceLowpass(player)) parent._SetVoiceLowpass(player);
            if (!_SetAvatarAudioGain(player)) parent._SetAvatarAudioGain(player);
            if (!_SetAvatarAudioDistanceNear(player)) parent._SetAvatarAudioDistanceNear(player);
            if (!_SetAvatarAudioDistanceFar(player)) parent._SetAvatarAudioDistanceFar(player);
            if (!_SetAvatarAudioVolumetricRadius(player)) parent._SetAvatarAudioVolumetricRadius(player);
            if (!_SetAvatarAudioForceSpatial(player)) parent._SetAvatarAudioForceSpatial(player);
        }

        void _ApplyVolumes(PlayerVolumeSetting parent, VRCPlayerApi player, PlayerVolumeSetting over)
        {
            if (over == null)
            {
                _ApplyVolumes(parent, player);
                return;
            }
            else if (parent == null)
            {
                _ApplyVolumes(player, over);
                return;
            }
            if (!over._SetVoiceGain(player) && !_SetVoiceGain(player)) parent._SetVoiceGain(player);
            if (!over._SetVoiceDistanceNear(player) && !_SetVoiceDistanceNear(player)) parent._SetVoiceDistanceNear(player);
            if (!over._SetVoiceDistanceFar(player) && !_SetVoiceDistanceFar(player)) parent._SetVoiceDistanceFar(player);
            if (!over._SetVoiceVolumetricRadius(player) && !_SetVoiceVolumetricRadius(player)) parent._SetVoiceVolumetricRadius(player);
            if (!over._SetVoiceLowpass(player) && !_SetVoiceLowpass(player)) parent._SetVoiceLowpass(player);
            if (!over._SetAvatarAudioGain(player) && !_SetAvatarAudioGain(player)) parent._SetAvatarAudioGain(player);
            if (!over._SetAvatarAudioDistanceNear(player) && !_SetAvatarAudioDistanceNear(player)) parent._SetAvatarAudioDistanceNear(player);
            if (!over._SetAvatarAudioDistanceFar(player) && !_SetAvatarAudioDistanceFar(player)) parent._SetAvatarAudioDistanceFar(player);
            if (!over._SetAvatarAudioVolumetricRadius(player) && !_SetAvatarAudioVolumetricRadius(player)) parent._SetAvatarAudioVolumetricRadius(player);
            if (!over._SetAvatarAudioForceSpatial(player) && !_SetAvatarAudioForceSpatial(player)) parent._SetAvatarAudioForceSpatial(player);
        }

        void _ApplyVolumes(VRCPlayerApi player, PlayerVolumeSetting over)
        {
            if (over == null)
            {
                _ApplyVolumes(player);
                return;
            }
            if (!over._SetVoiceGain(player)) _SetVoiceGain(player);
            if (!over._SetVoiceDistanceNear(player)) _SetVoiceDistanceNear(player);
            if (!over._SetVoiceDistanceFar(player)) _SetVoiceDistanceFar(player);
            if (!over._SetVoiceVolumetricRadius(player)) _SetVoiceVolumetricRadius(player);
            if (!over._SetVoiceLowpass(player)) _SetVoiceLowpass(player);
            if (!over._SetAvatarAudioGain(player)) _SetAvatarAudioGain(player);
            if (!over._SetAvatarAudioDistanceNear(player)) _SetAvatarAudioDistanceNear(player);
            if (!over._SetAvatarAudioDistanceFar(player)) _SetAvatarAudioDistanceFar(player);
            if (!over._SetAvatarAudioVolumetricRadius(player)) _SetAvatarAudioVolumetricRadius(player);
            if (!over._SetAvatarAudioForceSpatial(player)) _SetAvatarAudioForceSpatial(player);
        }
    }
}
