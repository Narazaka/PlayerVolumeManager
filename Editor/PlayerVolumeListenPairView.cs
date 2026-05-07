using System.Collections.Generic;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    public static class PlayerVolumeListenPairView
    {
        public enum SortMode
        {
            ManagerOrder = 0,
            DetectionOrder = 1,
        }

        public readonly struct Item
        {
            public readonly Object Group;
            public readonly int Key;
            public readonly bool IsOrphan;
            public readonly bool IsOutsideManager;
            public readonly bool IsDuplicate;

            public Item(Object group, int key)
                : this(group, key, false, false, false) { }

            public Item(Object group, int key, bool isOrphan, bool isOutsideManager, bool isDuplicate)
            {
                Group = group;
                Key = key;
                IsOrphan = isOrphan;
                IsOutsideManager = isOutsideManager;
                IsDuplicate = isDuplicate;
            }

            public Item WithFlags(bool isOrphan, bool isOutsideManager, bool isDuplicate)
                => new Item(Group, Key, isOrphan, isOutsideManager, isDuplicate);
        }

        public static IList<Item> Compute<T>(IReadOnlyList<Item> pairs, IReadOnlyList<T> managerGroups, SortMode sortMode)
            where T : Object
        {
            var n = pairs.Count;
            var managerIndex = new Dictionary<Object, int>();
            for (var i = 0; i < managerGroups.Count; i++)
            {
                var g = (Object)managerGroups[i];
                if (g != null && !managerIndex.ContainsKey(g)) managerIndex[g] = i;
            }

            var groupCounts = new Dictionary<Object, int>();
            for (var i = 0; i < n; i++)
            {
                var g = pairs[i].Group;
                if (g == null) continue;
                groupCounts.TryGetValue(g, out var c);
                groupCounts[g] = c + 1;
            }

            var flagged = new List<Item>(n);
            for (var i = 0; i < n; i++)
            {
                var p = pairs[i];
                var isOrphan = p.Group == null;
                var isOutside = !isOrphan && !managerIndex.ContainsKey(p.Group);
                var isDup = !isOrphan && groupCounts[p.Group] > 1;
                flagged.Add(p.WithFlags(isOrphan, isOutside, isDup));
            }

            if (sortMode == SortMode.DetectionOrder) return flagged;

            var inManager = new List<Item>();
            var tail = new List<Item>();
            foreach (var f in flagged)
            {
                if (f.IsOrphan || f.IsOutsideManager) tail.Add(f);
                else inManager.Add(f);
            }
            inManager.Sort((a, b) => managerIndex[a.Group].CompareTo(managerIndex[b.Group]));
            inManager.AddRange(tail);
            return inManager;
        }
    }
}
