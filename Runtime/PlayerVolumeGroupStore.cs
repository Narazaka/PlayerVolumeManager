using UnityEngine;
using VRC.SDKBase;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    public static class PlayerVolumeGroupStore
    {
        const string TagPlayerVolumeGroup = "Narazaka.VRChat.PlayerVolumeManager.PlayerVolumeGroup";

        public static string Get(VRCPlayerApi player)
        {
            return player.GetPlayerTag(TagPlayerVolumeGroup);
        }

        public static void Set(VRCPlayerApi player, string groupIndexes)
        {
            player.SetPlayerTag(TagPlayerVolumeGroup, groupIndexes);
        }

        public static int[] StringToInts(string groupIndexesString)
        {
            if (string.IsNullOrEmpty(groupIndexesString))
            {
                return new int[0];
            }
            var parts = groupIndexesString.Split(',');
            // Debug.Log($"[PlayerVolumeGroupStore::StringToInts] \"{groupIndexesString}\" => [{string.Join(",", parts)}]");
            var groupIndexes = new int[parts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                groupIndexes[i] = int.Parse(parts[i]);
            }
            return groupIndexes;
        }

        public static string IntsToString(int[] groupIndexes)
        {
            if (groupIndexes == null || groupIndexes.Length == 0)
            {
                return null;
            }
            var parts = new string[groupIndexes.Length];
            for (var i = 0; i < groupIndexes.Length; i++)
            {
                parts[i] = groupIndexes[i].ToString();
            }
            return string.Join(",", parts);
        }
    }
}
