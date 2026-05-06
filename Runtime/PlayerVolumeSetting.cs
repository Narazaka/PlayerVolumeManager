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
