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
        public PlayerVolumeSettingByGroup[] _listenOverrides = new PlayerVolumeSettingByGroup[0];
        PlayerVolumeGroup[] froms;

        public virtual bool _ContainsPlayer(VRCPlayerApi player)
        {
            var local = player.isLocal;
            return (local && _matchWhenListener) || (!local && _matchWhenSpeaker);
        }

        protected virtual void OnEnable()
        {
            var len = _listenOverrides.Length;
            froms = new PlayerVolumeGroup[len];
            for (var i = 0; i < len; i++)
            {
                froms[i] = _listenOverrides[i] == null ? null : _listenOverrides[i]._from;
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
                settings[i] = settingIndex == -1 ? (PlayerVolumeSetting)this : (PlayerVolumeSetting)_listenOverrides[settingIndex];
            }
            settings[len] = this;
            settings[len + 1] = parent;
            return _ApplyVolumes(player, settings, set);
        }
    }
}
