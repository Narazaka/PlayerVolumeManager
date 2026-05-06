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
        [SerializeField] bool _debugLog;

        VRCPlayerApi[] players = new VRCPlayerApi[1];
        int playerIndex;
        PlayerVolumeGroup selfGroup;

        void Update()
        {
            var player = selfGroup == null ? Networking.LocalPlayer : players[playerIndex];
            if (Utilities.IsValid(player))
            {
                ProcessPlayer(player);
            }
            playerIndex = (playerIndex + 1) % players.Length;
        }

        void ProcessPlayer(VRCPlayerApi player)
        {
            var groupIndex = DetectPlayerGroup(player);
            var groupIndexString = groupIndex == -1 ? null : groupIndex.ToString();
            var previousGroupIndexString = PlayerVolumeGroupStore.GetPlayerVolumeGroupIndex(player);
            if (previousGroupIndexString != groupIndexString)
            {
                PlayerVolumeGroupStore.SetPlayerVolumeGroupIndex(player, groupIndexString);
                var group = groupIndex < 0 ? null : _groups[groupIndex];

                // logging
                if (_debugLog)
                {
                    var previousGroupIndex = string.IsNullOrEmpty(previousGroupIndexString) ? -1 : int.Parse(previousGroupIndexString);
                    var previousGroup = previousGroupIndex < 0 ? null : _groups[previousGroupIndex];
                    var groupName = group == null ? "None" : group.name;
                    var previousGroupName = previousGroup == null ? "None" : previousGroup.name;
                    Debug.Log($"[PlayerVolumeManager]({player.playerId})<{player.displayName}>{(player.isLocal ? "(SELF)" : "")} [{previousGroupIndexString}]({previousGroupName}) => [{groupIndexString}]({groupName})");
                }

                if (player.isLocal)
                {
                    selfGroup = group;
                    foreach (var p in players)
                    {
                        if (Utilities.IsValid(p))
                        {
                            ApplySuitableVolumes(p);
                        }
                    }
                }
                else
                {
                    ApplySuitableVolumes(player, group);
                }
            }
        }

        void ApplySuitableVolumes(VRCPlayerApi player)
        {
            if (selfGroup == null)
            {
                _ApplyVolumes(player);
            }
            else
            {
                var groupIndexString = PlayerVolumeGroupStore.GetPlayerVolumeGroupIndex(player);
                var groupIndex = string.IsNullOrEmpty(groupIndexString) ? -1 : int.Parse(groupIndexString);
                var group = groupIndex < 0 ? null : _groups[groupIndex];
                selfGroup._ApplyVolumesWithOverride(this, player, group);
            }
        }

        void ApplySuitableVolumes(VRCPlayerApi player, PlayerVolumeGroup group)
        {
            if (selfGroup == null)
            {
                _ApplyVolumes(player);
            }
            else
            {
                selfGroup._ApplyVolumesWithOverride(this, player, group);
            }
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

            ApplySuitableVolumes(player);
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
