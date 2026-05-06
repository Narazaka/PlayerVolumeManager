using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    public abstract class PlayerVolumeSetting : UdonSharpBehaviour
    {
        public float _voiceGain = -1f;
        public float _voiceDistanceNear = -1f;
        public float _voiceDistanceFar = -1f;
        public float _voiceVolumetricRadius = -1f;
        public bool _enableVoiceLowpass = false;
        public bool _voiceLowpass = true;

        public float _avatarAudioGain = -1f;
        public float _avatarAudioDistanceNear = -1f;
        public float _avatarAudioDistanceFar = -1f;
        public float _avatarAudioVolumetricRadius = -1f;
        public bool _enableAvatarAudioForceSpatial = false;
        public bool _avatarAudioForceSpatial = false;

        public void _ApplyVolumes(VRCPlayerApi player)
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

        public void _ApplyVolumes(PlayerVolumeSetting parent, VRCPlayerApi player)
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

        public void _ApplyVolumes(PlayerVolumeSetting parent, VRCPlayerApi player, PlayerVolumeSetting over)
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

        public void _ApplyVolumes(VRCPlayerApi player, PlayerVolumeSetting over)
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


        public bool _SetVoiceGain(VRCPlayerApi player)
        {
            if (_voiceGain < 0f) return false;
            player.SetVoiceGain(_voiceGain);
            return true;
        }

        public bool _SetVoiceDistanceNear(VRCPlayerApi player)
        {
            if (_voiceDistanceNear < 0f) return false;
            player.SetVoiceDistanceNear(_voiceDistanceNear);
            return true;
        }

        public bool _SetVoiceDistanceFar(VRCPlayerApi player)
        {
            if (_voiceDistanceFar < 0f) return false;
            player.SetVoiceDistanceFar(_voiceDistanceFar);
            return true;
        }

        public bool _SetVoiceVolumetricRadius(VRCPlayerApi player)
        {
            if (_voiceVolumetricRadius < 0f) return false;
            player.SetVoiceVolumetricRadius(_voiceVolumetricRadius);
            return true;
        }

        public bool _SetVoiceLowpass(VRCPlayerApi player)
        {
            if (!_enableVoiceLowpass) return false;
            player.SetVoiceLowpass(_voiceLowpass);
            return true;
        }

        public bool _SetAvatarAudioGain(VRCPlayerApi player)
        {
            if (_avatarAudioGain < 0f) return false;
            player.SetAvatarAudioGain(_avatarAudioGain);
            return true;
        }

        public bool _SetAvatarAudioDistanceNear(VRCPlayerApi player)
        {
            if (_avatarAudioDistanceNear < 0f) return false;
            player.SetAvatarAudioNearRadius(_avatarAudioDistanceNear);
            return true;
        }

        public bool _SetAvatarAudioDistanceFar(VRCPlayerApi player)
        {
            if (_avatarAudioDistanceFar < 0f) return false;
            player.SetAvatarAudioFarRadius(_avatarAudioDistanceFar);
            return true;
        }

        public bool _SetAvatarAudioVolumetricRadius(VRCPlayerApi player)
        {
            if (_avatarAudioVolumetricRadius < 0f) return false;
            player.SetAvatarAudioVolumetricRadius(_avatarAudioVolumetricRadius);
            return true;
        }

        public bool _SetAvatarAudioForceSpatial(VRCPlayerApi player)
        {
            if (!_enableAvatarAudioForceSpatial) return false;
            player.SetAvatarAudioForceSpatial(_avatarAudioForceSpatial);
            return true;
        }
    }
}
