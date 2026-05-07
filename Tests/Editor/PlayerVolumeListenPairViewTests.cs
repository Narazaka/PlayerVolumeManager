using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor.Tests
{
    public class PlayerVolumeListenPairViewTests
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

        static PlayerVolumeListenPairView.Item Item(Object g, int key)
            => new PlayerVolumeListenPairView.Item(g, key);

        // ========================================================================
        // Boundary cases
        // ========================================================================

        [Test]
        public void Empty_ReturnsEmpty_BothModes()
        {
            var pairs = new PlayerVolumeListenPairView.Item[0];
            var manager = new ScriptableObject[0];

            var managerResult = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.ManagerOrder);
            var detectionResult = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.DetectionOrder);

            Assert.AreEqual(0, managerResult.Count);
            Assert.AreEqual(0, detectionResult.Count);
        }

        [Test]
        public void EmptyPairs_NonEmptyManager_ReturnsEmpty()
        {
            var A = NewObj("A");
            var manager = new[] { A };
            var pairs = new PlayerVolumeListenPairView.Item[0];

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.ManagerOrder);

            // 注: pair が無い場合、Manager にいくつ group があっても結果は空
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void EmptyManager_AllPairsFlaggedAsOutside()
        {
            var X = NewObj("X");
            var Y = NewObj("Y");
            var manager = new ScriptableObject[0];
            var pairs = new[] { Item(X, 0), Item(Y, 1) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.ManagerOrder);

            // 注: Manager 空のとき、null でない全ての pair は outside 扱い
            Assert.AreEqual(2, result.Count);
            foreach (var item in result)
            {
                Assert.IsFalse(item.IsOrphan);
                Assert.IsTrue(item.IsOutsideManager);
                Assert.IsFalse(item.IsDuplicate);
            }
        }

        // ========================================================================
        // Orphan (null group)
        // ========================================================================

        [Test]
        public void ManagerOrder_NullGroup_GoesToTailWithFlag()
        {
            var A = NewObj("A");
            var manager = new[] { A };
            var pairs = new[] { Item(null, 0), Item(A, 1) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.ManagerOrder);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].Key); // A
            Assert.IsFalse(result[0].IsOrphan);
            Assert.AreEqual(0, result[1].Key); // null は末尾
            Assert.IsTrue(result[1].IsOrphan);
        }

        [Test]
        public void MultipleNulls_AllOrphan_NoneDuplicate()
        {
            var A = NewObj("A");
            var manager = new[] { A };
            var pairs = new[] { Item(null, 0), Item(null, 1), Item(A, 2) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.ManagerOrder);

            // 注: null は groupCounts に登録されないため、null が複数あっても duplicate にはならない
            Assert.AreEqual(3, result.Count);
            // A が先頭 (Manager 順)
            Assert.AreEqual(2, result[0].Key);
            Assert.IsFalse(result[0].IsOrphan);
            // 後ろに null が 2 つ
            Assert.AreEqual(0, result[1].Key);
            Assert.IsTrue(result[1].IsOrphan);
            Assert.IsFalse(result[1].IsDuplicate);
            Assert.AreEqual(1, result[2].Key);
            Assert.IsTrue(result[2].IsOrphan);
            Assert.IsFalse(result[2].IsDuplicate);
        }

        [Test]
        public void DetectionOrder_OnlyNulls_KeepsPosition()
        {
            var manager = new ScriptableObject[0];
            var pairs = new[] { Item(null, 0), Item(null, 1), Item(null, 2) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.DetectionOrder);

            Assert.AreEqual(3, result.Count);
            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, result[i].Key);
                Assert.IsTrue(result[i].IsOrphan);
                Assert.IsFalse(result[i].IsDuplicate);
                Assert.IsFalse(result[i].IsOutsideManager); // 注: orphan のときは outside フラグは false (短絡)
            }
        }

        // ========================================================================
        // Outside manager
        // ========================================================================

        [Test]
        public void ManagerOrder_OutsideManager_GoesToTailWithFlag()
        {
            var A = NewObj("A");
            var X = NewObj("X");
            var manager = new[] { A };
            var pairs = new[] { Item(X, 0), Item(A, 1) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.ManagerOrder);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].Key); // A
            Assert.IsFalse(result[0].IsOutsideManager);
            Assert.AreEqual(0, result[1].Key); // X (outside)
            Assert.IsTrue(result[1].IsOutsideManager);
        }

        [Test]
        public void OutsideAndOrphanMixed_BothInTailInInputOrder()
        {
            var A = NewObj("A");
            var X = NewObj("X");
            var manager = new[] { A };
            var pairs = new[] { Item(X, 0), Item(null, 1), Item(A, 2) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.ManagerOrder);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result[0].Key); // A
            // 注: tail は入力順 (outside の X が先、null が後)
            Assert.AreEqual(0, result[1].Key);
            Assert.IsTrue(result[1].IsOutsideManager);
            Assert.AreEqual(1, result[2].Key);
            Assert.IsTrue(result[2].IsOrphan);
        }

        [Test]
        public void OutsideAndDuplicate_BothFlags()
        {
            var A = NewObj("A");
            var X = NewObj("X");
            var manager = new[] { A };
            var pairs = new[] { Item(X, 0), Item(X, 1), Item(A, 2) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.DetectionOrder);

            // 注: outside と duplicate は同時に立ちうる
            Assert.IsTrue(result[0].IsOutsideManager);
            Assert.IsTrue(result[0].IsDuplicate);
            Assert.IsTrue(result[1].IsOutsideManager);
            Assert.IsTrue(result[1].IsDuplicate);
            Assert.IsFalse(result[2].IsOutsideManager);
            Assert.IsFalse(result[2].IsDuplicate);
        }

        // ========================================================================
        // Duplicate
        // ========================================================================

        [Test]
        public void ManagerOrder_Duplicate_AllFlagged()
        {
            var A = NewObj("A");
            var manager = new[] { A };
            var pairs = new[] { Item(A, 0), Item(A, 1) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.ManagerOrder);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0].IsDuplicate);
            Assert.IsTrue(result[1].IsDuplicate);
        }

        [Test]
        public void Triplicate_AllThreeFlagged()
        {
            var A = NewObj("A");
            var manager = new[] { A };
            var pairs = new[] { Item(A, 0), Item(A, 1), Item(A, 2) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.DetectionOrder);

            // 注: 同じ group が 3 つ以上あっても全てに duplicate フラグ
            Assert.AreEqual(3, result.Count);
            for (var i = 0; i < 3; i++) Assert.IsTrue(result[i].IsDuplicate);
        }

        [Test]
        public void MixedDuplicateAndUnique_OnlyDupFlagged()
        {
            var A = NewObj("A");
            var B = NewObj("B");
            var manager = new[] { A, B };
            var pairs = new[] { Item(A, 0), Item(B, 1), Item(B, 2) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.DetectionOrder);

            // 注: duplicate しているのは B のみ。A は単独なので flag 立たない
            Assert.IsFalse(result[0].IsDuplicate); // A
            Assert.IsTrue(result[1].IsDuplicate);  // B
            Assert.IsTrue(result[2].IsDuplicate);  // B
        }

        // ========================================================================
        // Manager edge cases
        // ========================================================================

        [Test]
        public void ManagerHasNullEntries_IgnoredForLookup()
        {
            var A = NewObj("A");
            var manager = new ScriptableObject[] { null, A, null };
            var pairs = new[] { Item(A, 0), Item(null, 1) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.ManagerOrder);

            Assert.AreEqual(2, result.Count);
            // A は in-manager (Manager 内 null は無視される)
            Assert.AreEqual(0, result[0].Key);
            Assert.IsFalse(result[0].IsOutsideManager);
            // null pair は orphan
            Assert.AreEqual(1, result[1].Key);
            Assert.IsTrue(result[1].IsOrphan);
        }

        [Test]
        public void ManagerHasDuplicates_UsesFirstIndex()
        {
            var A = NewObj("A");
            var B = NewObj("B");
            // 注: Manager 内に A が 2 回。最初の出現位置 (index 0) が sort key になる前提
            var manager = new[] { A, B, A };
            var pairs = new[] { Item(B, 0), Item(A, 1) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.ManagerOrder);

            Assert.AreEqual(2, result.Count);
            // A の sort key は最初の出現位置 0、B は 1
            Assert.AreEqual(1, result[0].Key); // A (index 0)
            Assert.AreEqual(0, result[1].Key); // B (index 1)
            foreach (var item in result)
            {
                Assert.IsFalse(item.IsOutsideManager);
                Assert.IsFalse(item.IsDuplicate);
            }
        }

        // ========================================================================
        // Sort behavior
        // ========================================================================

        [Test]
        public void ManagerOrder_AllInManager_SortedByManagerIndex()
        {
            var A = NewObj("A");
            var B = NewObj("B");
            var C = NewObj("C");
            var manager = new[] { A, B, C };
            var pairs = new[] { Item(C, 0), Item(A, 1), Item(B, 2) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.ManagerOrder);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0].Key); // A
            Assert.AreEqual(2, result[1].Key); // B
            Assert.AreEqual(0, result[2].Key); // C
            foreach (var item in result)
            {
                Assert.IsFalse(item.IsOrphan);
                Assert.IsFalse(item.IsOutsideManager);
                Assert.IsFalse(item.IsDuplicate);
            }
        }

        [Test]
        public void ManagerOrder_TailKeepsInsertionOrder()
        {
            var A = NewObj("A");
            var X = NewObj("X");
            var Y = NewObj("Y");
            var manager = new[] { A };
            // 注: 末尾セクションに来る複数 outside の順序が入力順を保つことを確認
            var pairs = new[] { Item(Y, 0), Item(X, 1), Item(A, 2) };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.ManagerOrder);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result[0].Key); // A (in-manager)
            Assert.AreEqual(0, result[1].Key); // Y (input idx 0、tail 先頭)
            Assert.AreEqual(1, result[2].Key); // X (input idx 1、tail 末尾)
        }

        [Test]
        public void DetectionOrder_KeepsHierarchyOrder_ButFlagsWarnings()
        {
            var A = NewObj("A");
            var X = NewObj("X");
            var manager = new[] { A };
            var pairs = new[]
            {
                Item(X, 0),    // outside
                Item(null, 1), // orphan
                Item(A, 2),
            };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.DetectionOrder);

            Assert.AreEqual(0, result[0].Key);
            Assert.IsTrue(result[0].IsOutsideManager);
            Assert.AreEqual(1, result[1].Key);
            Assert.IsTrue(result[1].IsOrphan);
            Assert.AreEqual(2, result[2].Key);
            Assert.IsFalse(result[2].IsOrphan);
            Assert.IsFalse(result[2].IsOutsideManager);
        }

        [Test]
        public void DetectionOrder_AllFlagsCorrect_ComplexMix()
        {
            var A = NewObj("A");
            var B = NewObj("B");
            var X = NewObj("X");
            var manager = new[] { A, B };
            // A (in, dup), null (orphan), X (outside, dup), A (in, dup), X (outside, dup), B (in)
            var pairs = new[]
            {
                Item(A, 0),
                Item(null, 1),
                Item(X, 2),
                Item(A, 3),
                Item(X, 4),
                Item(B, 5),
            };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.DetectionOrder);

            // 順序維持
            for (var i = 0; i < 6; i++) Assert.AreEqual(i, result[i].Key);

            // A (key=0): in manager, duplicate
            Assert.IsFalse(result[0].IsOrphan);
            Assert.IsFalse(result[0].IsOutsideManager);
            Assert.IsTrue(result[0].IsDuplicate);

            // null (key=1): orphan, not duplicate (null は count されない)
            Assert.IsTrue(result[1].IsOrphan);
            Assert.IsFalse(result[1].IsOutsideManager);
            Assert.IsFalse(result[1].IsDuplicate);

            // X (key=2): outside, duplicate
            Assert.IsFalse(result[2].IsOrphan);
            Assert.IsTrue(result[2].IsOutsideManager);
            Assert.IsTrue(result[2].IsDuplicate);

            // A (key=3): in manager, duplicate
            Assert.IsFalse(result[3].IsOrphan);
            Assert.IsFalse(result[3].IsOutsideManager);
            Assert.IsTrue(result[3].IsDuplicate);

            // X (key=4): outside, duplicate
            Assert.IsTrue(result[4].IsOutsideManager);
            Assert.IsTrue(result[4].IsDuplicate);

            // B (key=5): in manager, single
            Assert.IsFalse(result[5].IsOrphan);
            Assert.IsFalse(result[5].IsOutsideManager);
            Assert.IsFalse(result[5].IsDuplicate);
        }

        [Test]
        public void ManagerOrder_AllFlagsCorrect_ComplexMix_TailSection()
        {
            var A = NewObj("A");
            var B = NewObj("B");
            var X = NewObj("X");
            var manager = new[] { A, B };
            var pairs = new[]
            {
                Item(B, 0),    // in
                Item(X, 1),    // outside (tail)
                Item(A, 2),    // in
                Item(null, 3), // orphan (tail)
                Item(X, 4),    // outside dup (tail)
                Item(A, 5),    // in dup
            };

            var result = PlayerVolumeListenPairView.Compute(pairs, manager, PlayerVolumeListenPairView.SortMode.ManagerOrder);

            Assert.AreEqual(6, result.Count);

            // Manager 順セクション: A 2 つ -> B 1 つ
            // Sort は安定 (List.Sort は不安定だが、同じ Manager index 内なので順序は元の inManager 追加順依存)
            // 注: Manager 順内では A が先 (index 0) → B (index 1)
            // A の 2 つは inManager リスト内で出現順 (key=2 が key=5 より前) になる
            Assert.IsFalse(result[0].IsOrphan);
            Assert.IsFalse(result[0].IsOutsideManager);
            Assert.IsTrue(result[0].IsDuplicate);
            Assert.IsFalse(result[1].IsOrphan);
            Assert.IsFalse(result[1].IsOutsideManager);
            Assert.IsTrue(result[1].IsDuplicate);
            Assert.IsFalse(result[2].IsOrphan);
            Assert.IsFalse(result[2].IsOutsideManager);
            Assert.IsFalse(result[2].IsDuplicate); // B 単独

            // tail: X (key=1, outside dup), null (key=3, orphan), X (key=4, outside dup)
            Assert.IsTrue(result[3].IsOutsideManager);
            Assert.IsTrue(result[3].IsDuplicate);
            Assert.AreEqual(1, result[3].Key);

            Assert.IsTrue(result[4].IsOrphan);
            Assert.IsFalse(result[4].IsDuplicate);
            Assert.AreEqual(3, result[4].Key);

            Assert.IsTrue(result[5].IsOutsideManager);
            Assert.IsTrue(result[5].IsDuplicate);
            Assert.AreEqual(4, result[5].Key);
        }
    }
}
