using System.Collections.Generic;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    public static class PlayerVolumeListenPairDetect
    {
        public static IList<T> Compute<T>(IReadOnlyList<T> existingGroups, IReadOnlyList<T> managerGroups)
            where T : Object
        {
            var existing = new HashSet<Object>();
            for (var i = 0; i < existingGroups.Count; i++)
            {
                var g = (Object)existingGroups[i];
                if (g != null) existing.Add(g);
            }

            var result = new List<T>();
            for (var i = 0; i < managerGroups.Count; i++)
            {
                var g = managerGroups[i];
                if ((Object)g == null) continue;
                if (!existing.Contains(g)) result.Add(g);
            }
            return result;
        }
    }
}
