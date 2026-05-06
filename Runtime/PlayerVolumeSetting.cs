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

        const int SetCount = 10;

        public static bool[] CreateApplySet()
        {
            return new bool[SetCount];
        }

        public static bool[] _ApplyVolumes(VRCPlayerApi player, PlayerVolumeSetting[] settings, bool[] set)
        {
            if (!set[0]) foreach (var setting in settings) if (setting._SetVoiceGain(player)) { set[0] = true; break; }
            if (!set[1]) foreach (var setting in settings) if (setting._SetVoiceDistanceNear(player)) { set[1] = true; break; }
            if (!set[2]) foreach (var setting in settings) if (setting._SetVoiceDistanceFar(player)) { set[2] = true; break; }
            if (!set[3]) foreach (var setting in settings) if (setting._SetVoiceVolumetricRadius(player)) { set[3] = true; break; }
            if (!set[4]) foreach (var setting in settings) if (setting._SetVoiceLowpass(player)) { set[4] = true; break; }
            if (!set[5]) foreach (var setting in settings) if (setting._SetAvatarAudioGain(player)) { set[5] = true; break; }
            if (!set[6]) foreach (var setting in settings) if (setting._SetAvatarAudioDistanceNear(player)) { set[6] = true; break; }
            if (!set[7]) foreach (var setting in settings) if (setting._SetAvatarAudioDistanceFar(player)) { set[7] = true; break; }
            if (!set[8]) foreach (var setting in settings) if (setting._SetAvatarAudioVolumetricRadius(player)) { set[8] = true; break; }
            if (!set[9]) foreach (var setting in settings) if (setting._SetAvatarAudioForceSpatial(player)) { set[9] = true; break; }
            return set;
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
