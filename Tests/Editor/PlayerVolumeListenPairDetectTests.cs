using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor.Tests
{
    public class PlayerVolumeListenPairDetectTests
    {
        readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _created) Object.DestroyImmediate(o);
            _created.Clear();
        }

        ScriptableObject NewObj(string name)
        {
            var so = ScriptableObject.CreateInstance<ScriptableObject>();
            so.name = name;
            _created.Add(so);
            return so;
        }

        [Test]
        public void Empty_ReturnsAllManagerGroups()
        {
            var A = NewObj("A");
            var B = NewObj("B");
            var manager = new[] { A, B };
            var existing = new ScriptableObject[0];

            var result = PlayerVolumeListenPairDetect.Compute(existing, manager);

            CollectionAssert.AreEqual(new[] { A, B }, result);
        }

        [Test]
        public void Partial_ReturnsOnlyMissing()
        {
            var A = NewObj("A");
            var B = NewObj("B");
            var C = NewObj("C");
            var manager = new[] { A, B, C };
            var existing = new[] { B };

            var result = PlayerVolumeListenPairDetect.Compute(existing, manager);

            // 注: Manager 順を維持して不足分のみ返す
            CollectionAssert.AreEqual(new[] { A, C }, result);
        }

        [Test]
        public void NullExisting_Ignored()
        {
            var A = NewObj("A");
            var manager = new[] { A };
            var existing = new ScriptableObject[] { null };

            var result = PlayerVolumeListenPairDetect.Compute(existing, manager);

            // 注: 既存の null は「Aは未配置」とみなされ、Aを返す
            CollectionAssert.AreEqual(new[] { A }, result);
        }

        [Test]
        public void OutsideManagerInExisting_Ignored()
        {
            var A = NewObj("A");
            var X = NewObj("X");
            var manager = new[] { A };
            var existing = new[] { X };

            var result = PlayerVolumeListenPairDetect.Compute(existing, manager);

            // 注: Manager にない X は判定に影響しない、A は不足として返る
            CollectionAssert.AreEqual(new[] { A }, result);
        }

        [Test]
        public void NullManagerEntry_Skipped()
        {
            var A = NewObj("A");
            var manager = new ScriptableObject[] { null, A };
            var existing = new ScriptableObject[0];

            var result = PlayerVolumeListenPairDetect.Compute(existing, manager);

            // 注: Manager 内の null は無視
            CollectionAssert.AreEqual(new[] { A }, result);
        }
    }
}
