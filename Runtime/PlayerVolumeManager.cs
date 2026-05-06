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
        PlayerVolumeGroup[] selfGroups;

        void Update()
        {
            var player = selfGroups == null ? Networking.LocalPlayer : players[Time.frameCount % players.Length];
            if (Utilities.IsValid(player))
            {
                ProcessPlayer(player);
            }
            playerIndex = (playerIndex + 1) % players.Length;
        }

        void ProcessPlayer(VRCPlayerApi player)
        {
            var groupIndexes = DetectPlayerGroups(player);
            var groupIndexesString = groupIndexes.Length == 0 ? null : PlayerVolumeGroupStore.IntsToString(groupIndexes);
            var previousGroupIndexesString = PlayerVolumeGroupStore.Get(player);
            if (previousGroupIndexesString != groupIndexesString)
            {
                PlayerVolumeGroupStore.Set(player, groupIndexesString);
                var len = groupIndexes.Length;
                var groups = new PlayerVolumeGroup[len];
                for (var i = 0; i < len; i++)
                {
                    groups[i] = _groups[groupIndexes[i]];
                }

                // logging
                if (_debugLog)
                {
                    var previousGroupIndexes = PlayerVolumeGroupStore.StringToInts(previousGroupIndexesString);
                    var lenp = previousGroupIndexes.Length;
                    var previousParts = new string[lenp];
                    for (var i = 0; i < lenp; i++)
                    {
                        var index = previousGroupIndexes[i];
                        var group = _groups[index];
                        previousParts[i] = $"({index}){group.name}";
                    }
                    var parts = new string[len];
                    for (var i = 0; i < len; i++)
                    {
                        var index = groupIndexes[i];
                        var group = groups[i];
                        parts[i] = $"({index}){group.name}";
                    }
                    Debug.Log($"[PlayerVolumeManager::PlayerVolumeManager]({player.playerId})<{player.displayName}>{(player.isLocal ? "(SELF)" : "")} [{string.Join(",", previousParts)}] => [{string.Join(",", parts)}]");
                }

                if (player.isLocal)
                {
                    selfGroups = groups;
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
                    ApplySuitableVolumes(player, groups);
                }
            }
        }

        void ApplySuitableVolumes(VRCPlayerApi player)
        {
            if (selfGroups == null)
            {
                _ApplyVolumes(player, new PlayerVolumeSetting[] { this }, CreateApplySet());
            }
            else
            {
                var groupIndexes = PlayerVolumeGroupStore.StringToInts(PlayerVolumeGroupStore.Get(player));
                var len = groupIndexes.Length;
                var groups = new PlayerVolumeGroup[len];
                for (var i = 0; i < len; i++)
                {
                    groups[i] = _groups[groupIndexes[i]];
                }
                var applySet = CreateApplySet();
                foreach (var sg in selfGroups)
                {
                    applySet = sg._ApplyVolumesWithOverride(this, player, groups, applySet);
                }
            }
        }

        void ApplySuitableVolumes(VRCPlayerApi player, PlayerVolumeGroup[] groups)
        {
            if (selfGroups == null)
            {
                _ApplyVolumes(player, new PlayerVolumeSetting[] { this }, CreateApplySet());
            }
            else
            {
                var applySet = CreateApplySet();
                foreach (var sg in selfGroups)
                {
                    applySet = sg._ApplyVolumesWithOverride(this, player, groups, applySet);
                }
            }
        }

        int[] DetectPlayerGroups(VRCPlayerApi player)
        {
            var len = _groups.Length;
            var groupIndexes = new int[len];
            var matchedGroupCount = 0;
            for (var i = 0; i < len; i++)
            {
                var group = _groups[i];
                if (group != null && group.isActiveAndEnabled && group._ContainsPlayer(player))
                {
                    groupIndexes[matchedGroupCount++] = i;
                    if (!group._fallbackToNextGroup)
                    {
                        break;
                    }
                }
            }
            var matchedGroupIndexes = new int[matchedGroupCount];
            Array.Copy(groupIndexes, matchedGroupIndexes, matchedGroupCount);
            return matchedGroupIndexes;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            players = VRCPlayerApi.GetPlayers();

            ApplySuitableVolumes(player);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            var newPlayers = VRCPlayerApi.GetPlayers();
            var len = newPlayers.Length - 1;
            players = new VRCPlayerApi[len];
            var index = Array.IndexOf(newPlayers, player);
            if (index > 0) Array.Copy(newPlayers, 0, players, 0, index);
            if (index < len) Array.Copy(newPlayers, index + 1, players, index, len - index);
        }
    }
}
