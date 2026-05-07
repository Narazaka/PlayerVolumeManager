using System.Collections.Generic;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    /// <summary>
    /// Pure helpers for the (fromGroups, overrides) pair maintained by PlayerVolumeGroup.
    /// Kept independent from SerializedObject/CustomEditor so the matching rules can be unit-tested.
    /// </summary>
    public static class PlayerVolumeGroupListenOverridesUtil
    {
        /// <summary>
        /// Re-aligns the overrides array after the user edited the fromGroups array
        /// (reorder / +/- / replace / clear). Three matching passes (in priority):
        /// 1. same-index AND same-group  → no-move slot keeps its setting unconditionally
        /// 2. different-index, same-group → moved slot follows its setting (reorder)
        /// 3. same-index fallback         → group changed at index i (orphan filled / replaced
        ///                                   / cleared); inherit the same-index setting
        /// </summary>
        public static TSetting[] ComputeReorderedOverrides<TGroup, TSetting>(
            TGroup[] beforeFromGroups,
            TSetting[] beforeOverrides,
            TGroup[] afterFromGroups)
            where TGroup : Object
            where TSetting : Object
        {
            var len = afterFromGroups.Length;
            var consumed = new bool[beforeFromGroups.Length];
            var matched = new bool[len];
            var result = new TSetting[len];

            // Pass 1: same-index AND same-group (no-move).
            for (var i = 0; i < len && i < beforeFromGroups.Length; i++)
            {
                if ((Object)afterFromGroups[i] != (Object)beforeFromGroups[i]) continue;
                if (i < beforeOverrides.Length) result[i] = beforeOverrides[i];
                consumed[i] = true;
                matched[i] = true;
            }

            // Pass 2: different-index, same-group (reorder).
            for (var i = 0; i < len; i++)
            {
                if (matched[i]) continue;
                var g = (Object)afterFromGroups[i];
                for (var idx = 0; idx < beforeFromGroups.Length; idx++)
                {
                    if (consumed[idx]) continue;
                    if ((Object)beforeFromGroups[idx] != g) continue;
                    if (idx < beforeOverrides.Length) result[i] = beforeOverrides[idx];
                    consumed[idx] = true;
                    matched[i] = true;
                    break;
                }
            }

            // Pass 3: same-index fallback.
            for (var i = 0; i < len; i++)
            {
                if (matched[i]) continue;
                if (i >= beforeFromGroups.Length || consumed[i]) continue;
                if (i < beforeOverrides.Length) result[i] = beforeOverrides[i];
                consumed[i] = true;
            }

            return result;
        }

        /// <summary>
        /// Builds (fromGroups, overrides) reordered to follow managerGroups order. Existing
        /// (group → setting) bindings are preserved, null group rows are dropped, empty
        /// override slots adopt same-name objects from <paramref name="settingsByName"/>,
        /// and groups present in the existing array but missing from managerGroups are
        /// appended at the tail (user-added entries).
        /// </summary>
        public static (TGroup[] FromGroups, TSetting[] Overrides) ComputeFilledOverrides<TGroup, TSetting>(
            TGroup[] existingFromGroups,
            TSetting[] existingOverrides,
            TGroup[] managerGroups,
            IReadOnlyDictionary<string, TSetting> settingsByName)
            where TGroup : Object
            where TSetting : Object
        {
            var currentMap = new Dictionary<TGroup, TSetting>();
            for (var i = 0; i < existingFromGroups.Length; i++)
            {
                var g = existingFromGroups[i];
                if ((Object)g == null) continue;
                var s = i < existingOverrides.Length ? existingOverrides[i] : null;
                if ((Object)s == null && settingsByName != null)
                {
                    settingsByName.TryGetValue(g.name, out s);
                }
                currentMap[g] = s;
            }

            var orderedGroups = new List<TGroup>();
            if (managerGroups != null)
            {
                for (var i = 0; i < managerGroups.Length; i++)
                {
                    var g = managerGroups[i];
                    if ((Object)g == null) continue;
                    if (orderedGroups.Contains(g)) continue;
                    orderedGroups.Add(g);
                }
            }
            foreach (var kv in currentMap)
            {
                if (!orderedGroups.Contains(kv.Key)) orderedGroups.Add(kv.Key);
            }

            var newFrom = new TGroup[orderedGroups.Count];
            var newOverrides = new TSetting[orderedGroups.Count];
            for (var i = 0; i < orderedGroups.Count; i++)
            {
                var g = orderedGroups[i];
                newFrom[i] = g;
                if (!currentMap.TryGetValue(g, out var setting))
                {
                    if (settingsByName != null) settingsByName.TryGetValue(g.name, out setting);
                }
                newOverrides[i] = setting;
            }

            return (newFrom, newOverrides);
        }
    }
}
