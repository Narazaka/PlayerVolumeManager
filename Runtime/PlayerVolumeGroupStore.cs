using VRC.SDKBase;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    public static class PlayerVolumeGroupStore
    {
        const string TagPlayerVolumeGroup = "Narazaka.VRChat.PlayerVolumeManager.PlayerVolumeGroup";

        public static string GetPlayerVolumeGroupIndex(VRCPlayerApi player)
        {
            return player.GetPlayerTag(TagPlayerVolumeGroup);
        }

        public static void SetPlayerVolumeGroupIndex(VRCPlayerApi player, string groupIndex)
        {
            player.SetPlayerTag(TagPlayerVolumeGroup, groupIndex);
        }

        /// <returns>true if changed</returns>
        public static bool SetPlayerVolumeGroupIndexIfChanged(VRCPlayerApi player, string groupIndex)
        {
            if (GetPlayerVolumeGroupIndex(player) != groupIndex)
            {
                SetPlayerVolumeGroupIndex(player, groupIndex);
                return true;
            }
            return false;
        }
    }
}
