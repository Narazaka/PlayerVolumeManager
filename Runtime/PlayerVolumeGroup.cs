using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    public abstract class PlayerVolumeGroup : PlayerVolumeSetting
    {
        [Tooltip("リスナーとしての判定時に考慮される（ローカルでの処理時に考慮される）")]
        public bool _matchWhenListener = true;
        [Tooltip("スピーカーとしての判定時に考慮される（リモートでの処理時に考慮される）")]
        public bool _matchWhenSpeaker = true;
        [Tooltip("このグループでオーバーライドされない項目を、Managerではなく次のグループにまかせる")]
        public bool _fallbackToNextGroup;

        [InlinePlayerVolumeSettingByGroup]
        public PlayerVolumeGroup[] _listenFromGroups = new PlayerVolumeGroup[0];
        public PlayerVolumeSettingByGroup[] _listenOverrides = new PlayerVolumeSettingByGroup[0];

        public virtual bool _ContainsPlayer(VRCPlayerApi player)
        {
            var local = player.isLocal;
            return (local && _matchWhenListener) || (!local && _matchWhenSpeaker);
        }

        protected virtual void OnEnable()
        {
        }

        public bool[] _ApplyVolumesWithOverride(PlayerVolumeSetting parent, VRCPlayerApi player, PlayerVolumeGroup[] fromGroups, bool[] set)
        {
            if (fromGroups == null || fromGroups.Length == 0)
            {
                return _ApplyVolumes(player, new PlayerVolumeSetting[] { this, parent }, set);
            }

            var len = fromGroups.Length;
            var settings = new PlayerVolumeSetting[len + 2];
            for (var i = 0; i < len; i++)
            {
                var settingIndex = Array.IndexOf(_listenFromGroups, fromGroups[i]);
                settings[i] = settingIndex == -1 || _listenOverrides[settingIndex] == null
                    ? (PlayerVolumeSetting)this
                    : (PlayerVolumeSetting)_listenOverrides[settingIndex];
            }
            settings[len] = this;
            settings[len + 1] = parent;
            return _ApplyVolumes(player, settings, set);
        }
    }
}
