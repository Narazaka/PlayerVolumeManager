using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVolumeManager : PlayerVolumeSetting
    {
        [SerializeField] PlayerVolumeGroup[] _groups = new PlayerVolumeGroup[0];

        VRCPlayerApi[] players = new VRCPlayerApi[1];
        int playerIndex;

        void Update()
        {
            var player = players[playerIndex];
            if (!Utilities.IsValid(player))
            {
                return;
            }

            var groupIndex = DetectPlayerGroup(player);
            var groupIndexString = groupIndex.ToString();
            if (PlayerVolumeGroupStore.GetPlayerVolumeGroupIndex(player) != groupIndexString)
            {
                PlayerVolumeGroupStore.SetPlayerVolumeGroupIndex(player, groupIndexString);
                _groups[groupIndex]._ApplyVolumesWithOverride(this, player, groupIndex == -1 ? null : _groups[groupIndex]);
            }

            playerIndex = (playerIndex + 1) % players.Length;
        }

        int DetectPlayerGroup(VRCPlayerApi player)
        {
            var groupIndex = -1;
            var len = _groups.Length;
            for (var i = 0; i < len; i++)
            {
                var group = _groups[i];
                if (group != null && group.isActiveAndEnabled && group._ContainsPlayer(player))
                {
                    groupIndex = i;
                    break;
                }
            }
            return groupIndex;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            players = VRCPlayerApi.GetPlayers(players);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            var newPlayers = VRCPlayerApi.GetPlayers(players);
            var len = newPlayers.Length - 1;
            players = new VRCPlayerApi[len];
            var index = Array.IndexOf(newPlayers, player);
            if (index > 0) Array.Copy(newPlayers, 0, players, 0, index);
            if (index < len) Array.Copy(newPlayers, index + 1, players, index, len - index);
        }
    }
}
