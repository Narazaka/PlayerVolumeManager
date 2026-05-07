using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor.Tests
{
    public class PlayerVolumeGroupListenOverridesUtilTests
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

        // -------- ComputeReorderedOverrides ----------

        [Test]
        public void Reorder_NoChange_KeepsSettings()
        {
            var A = NewObj("A");
            var SA = NewObj("SA");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new[] { A }, new[] { SA }, new[] { A });
            CollectionAssert.AreEqual(new[] { SA }, result);
        }

        [Test]
        public void Reorder_SimpleSwap()
        {
            var A = NewObj("A");
            var B = NewObj("B");
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new[] { A, B }, new[] { SA, SB }, new[] { B, A });
            CollectionAssert.AreEqual(new[] { SB, SA }, result);
        }

        [Test]
        public void Reorder_DuplicateGroup_KeepsIndividualSettings()
        {
            var A = NewObj("A");
            var B = NewObj("B");
            var SA = NewObj("SA");
            var SAA = NewObj("SAA");
            var SB = NewObj("SB");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new[] { A, A, B },
                new[] { SA, SAA, SB },
                new[] { A, B, A });
            CollectionAssert.AreEqual(new[] { SA, SB, SAA }, result);
        }

        [Test]
        public void Reorder_GroupToNull_KeepsSetting()
        {
            var A = NewObj("A");
            var SA = NewObj("SA");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new ScriptableObject[] { A }, new[] { SA }, new ScriptableObject[] { null });
            CollectionAssert.AreEqual(new[] { SA }, result);
        }

        [Test]
        public void Reorder_OrphanFilledWithGroup_KeepsSetting()
        {
            var A = NewObj("A");
            var SX = NewObj("SX");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new ScriptableObject[] { null }, new[] { SX }, new[] { A });
            CollectionAssert.AreEqual(new[] { SX }, result);
        }

        [Test]
        public void Reorder_GroupReplacement_KeepsSetting()
        {
            // 注: 同 index で group を別物 (C) に差し替えても、setting (SA) は引き継がれる。
            // ユーザーが group 参照を変えただけで override 編集が消えるのを防ぐ仕様判断。
            // setting を切り替えたい場合は Override フィールドを直接編集する。
            var A = NewObj("A");
            var C = NewObj("C");
            var SA = NewObj("SA");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new[] { A }, new[] { SA }, new[] { C });
            CollectionAssert.AreEqual(new[] { SA }, result);
        }

        [Test]
        public void Reorder_PlusButton_AppendedSlotIsNull()
        {
            // 注: Unity の "+" は末尾要素をコピーするので after は [A, B, B] になるが、
            // 新規追加された 3 番目の slot は SB を共有せず null になる (placeholder 化)。
            // 旧仕様 [SA, SB, SB] は [A, A, B] [SA, SAA, SB] の並び替えで重複 group の
            // setting が壊れる原因になっていたため、consumed 方式に切り替え済み。
            var A = NewObj("A");
            var B = NewObj("B");
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new[] { A, B }, new[] { SA, SB }, new[] { A, B, B });
            CollectionAssert.AreEqual(new ScriptableObject[] { SA, SB, null }, result);
        }

        [Test]
        public void Reorder_FillGroupAtNullSlot_PreservesAllOverrides()
        {
            // [A, null, null] [SA, SB, SC] -> [A, B, null]
            var A = NewObj("A");
            var B = NewObj("B");
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var SC = NewObj("SC");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new ScriptableObject[] { A, null, null },
                new[] { SA, SB, SC },
                new ScriptableObject[] { A, B, null });
            CollectionAssert.AreEqual(new[] { SA, SB, SC }, result);
        }

        [Test]
        public void Reorder_SharedSetting_PreservedAcrossSwap()
        {
            // [A, B, C] [SAB, SAB, SC] -> [B, A, C]
            var A = NewObj("A");
            var B = NewObj("B");
            var C = NewObj("C");
            var SAB = NewObj("SAB");
            var SC = NewObj("SC");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new[] { A, B, C },
                new[] { SAB, SAB, SC },
                new[] { B, A, C });
            CollectionAssert.AreEqual(new[] { SAB, SAB, SC }, result);
        }

        [Test]
        public void Reorder_TailGroupReplaced_KeepsSetting()
        {
            // [A, B, C] [SA, SB, SC] -> [A, B, D]
            // 注: 末尾 group を C → D に差し替えても、同 index の SC は維持される
            // (Reorder_GroupReplacement_KeepsSetting と同じルール)。
            var A = NewObj("A");
            var B = NewObj("B");
            var C = NewObj("C");
            var D = NewObj("D");
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var SC = NewObj("SC");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new[] { A, B, C }, new[] { SA, SB, SC }, new[] { A, B, D });
            CollectionAssert.AreEqual(new[] { SA, SB, SC }, result);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Reorder_DeleteOne_FromTriple(int deleteIndex)
        {
            // [A, B, C] [SA, SB, SC] minus index `deleteIndex`.
            var A = NewObj("A");
            var B = NewObj("B");
            var C = NewObj("C");
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var SC = NewObj("SC");
            var groups = new[] { A, B, C };
            var overrides = new[] { SA, SB, SC };
            var afterList = new List<ScriptableObject>(groups);
            afterList.RemoveAt(deleteIndex);
            var expected = new List<ScriptableObject>(overrides);
            expected.RemoveAt(deleteIndex);
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                groups, overrides, afterList.ToArray());
            CollectionAssert.AreEqual(expected.ToArray(), result);
        }

        [Test]
        public void Reorder_DeleteAll_ReturnsEmpty()
        {
            var A = NewObj("A");
            var SA = NewObj("SA");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new[] { A }, new[] { SA }, new ScriptableObject[0]);
            CollectionAssert.AreEqual(new ScriptableObject[0], result);
        }

        [Test]
        public void Reorder_EmptyToOne_NewNullSlotIsNull()
        {
            // [] -> [null]: Unity's "+" on empty array gives a default(null) entry.
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new ScriptableObject[0], new ScriptableObject[0], new ScriptableObject[] { null });
            CollectionAssert.AreEqual(new ScriptableObject[] { null }, result);
        }

        [Test]
        public void Reorder_OrphanNoChange_KeepsSetting()
        {
            // [null] [SX] -> [null]
            var SX = NewObj("SX");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new ScriptableObject[] { null }, new[] { SX }, new ScriptableObject[] { null });
            CollectionAssert.AreEqual(new[] { SX }, result);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void Reorder_TwoOrphans_FillOne_KeepsBoth(int fillIndex)
        {
            // [null, null] [SA, SB] -> fill one slot with A; both settings should survive.
            var A = NewObj("A");
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var after = new ScriptableObject[] { null, null };
            after[fillIndex] = A;
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new ScriptableObject[] { null, null }, new[] { SA, SB }, after);
            CollectionAssert.AreEqual(new[] { SA, SB }, result);
        }

        [Test]
        public void Reorder_TwoOrphans_DeleteFirstSlot_KeepsFirstOverride()
        {
            // [null, null] [SA, SB] -> [null]
            // 注: UI 上 2 つの null は区別不能なのでどちらが消えたか決定できない。
            // 実装は「未消費の null slot を先頭から消費」するため、結果は [SA] になる
            // (実装定義の挙動)。
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new ScriptableObject[] { null, null }, new[] { SA, SB },
                new ScriptableObject[] { null });
            CollectionAssert.AreEqual(new[] { SA }, result);
        }

        [Test]
        public void Reorder_DuplicatePlusNull_Mixed()
        {
            // [A, null, A] [SA, SX, SA2] -> [A, A, null]
            // 注: 並び替えと slot 消費の組み合わせ。
            //   Pass 1: i=0 A=A 同 index → SA を保持
            //   Pass 2: i=1 A → before の未消費 A (idx=2) を消費 → SA2
            //           i=2 null → before の未消費 null (idx=1) を消費 → SX
            // 結果として「末尾の null が orphan setting (SX) を引き連れて末尾に移動した」形になる。
            var A = NewObj("A");
            var SA = NewObj("SA");
            var SX = NewObj("SX");
            var SA2 = NewObj("SA2");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new ScriptableObject[] { A, null, A },
                new[] { SA, SX, SA2 },
                new ScriptableObject[] { A, A, null });
            CollectionAssert.AreEqual(new[] { SA, SA2, SX }, result);
        }

        [Test]
        public void Reorder_InsertNullAtFront_ShiftsExisting()
        {
            // [A] [SA] -> [null, A]
            var A = NewObj("A");
            var SA = NewObj("SA");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new[] { A }, new[] { SA }, new ScriptableObject[] { null, A });
            CollectionAssert.AreEqual(new ScriptableObject[] { null, SA }, result);
        }

        [Test]
        public void Reorder_DuplicateGroupDelete()
        {
            // [A, A, B] [SA, SAA, SB] -> [A, B]: keep first A and B
            var A = NewObj("A");
            var B = NewObj("B");
            var SA = NewObj("SA");
            var SAA = NewObj("SAA");
            var SB = NewObj("SB");
            var result = PlayerVolumeGroupListenOverridesUtil.ComputeReorderedOverrides(
                new[] { A, A, B }, new[] { SA, SAA, SB }, new[] { A, B });
            CollectionAssert.AreEqual(new[] { SA, SB }, result);
        }

        // -------- ComputeFilledOverrides ----------

        [Test]
        public void Fill_EmptyToManagerOrder_AddsAllAsPlaceholder()
        {
            var A = NewObj("A");
            var B = NewObj("B");
            var (g, o) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                new ScriptableObject[0], new ScriptableObject[0],
                new[] { A, B },
                new Dictionary<string, ScriptableObject>());
            CollectionAssert.AreEqual(new[] { A, B }, g);
            CollectionAssert.AreEqual(new ScriptableObject[] { null, null }, o);
        }

        [Test]
        public void Fill_ReordersByManagerOrder()
        {
            var A = NewObj("A");
            var B = NewObj("B");
            var C = NewObj("C");
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var SC = NewObj("SC");
            var (g, o) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                new[] { B, A, C }, new[] { SB, SA, SC },
                new[] { A, B, C },
                new Dictionary<string, ScriptableObject>());
            CollectionAssert.AreEqual(new[] { A, B, C }, g);
            CollectionAssert.AreEqual(new[] { SA, SB, SC }, o);
        }

        [Test]
        public void Fill_DropsNullGroups()
        {
            var A = NewObj("A");
            var B = NewObj("B");
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var (g, o) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                new ScriptableObject[] { A, null, B },
                new ScriptableObject[] { SA, null, SB },
                new[] { A, B },
                new Dictionary<string, ScriptableObject>());
            CollectionAssert.AreEqual(new[] { A, B }, g);
            CollectionAssert.AreEqual(new[] { SA, SB }, o);
        }

        [Test]
        public void Fill_AdoptsSameNameSettingsForEmptySlots()
        {
            var A = NewObj("A");
            var B = NewObj("B");
            var settingForA = NewObj("A"); // child object whose name matches the group name
            var settingForB = NewObj("B");
            var dict = new Dictionary<string, ScriptableObject>
            {
                ["A"] = settingForA,
                ["B"] = settingForB,
            };
            var (g, o) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                new ScriptableObject[0], new ScriptableObject[0],
                new[] { A, B },
                dict);
            CollectionAssert.AreEqual(new[] { A, B }, g);
            CollectionAssert.AreEqual(new[] { settingForA, settingForB }, o);
        }

        [Test]
        public void Fill_PreservesUserAddedGroupsNotInManager()
        {
            // Manager has [A, B], user added X. Result should append X.
            // 注: Manager._groups に登録されていない group (= ユーザーが手で追加した X) は
            // 削除せず末尾に残す。Detect ボタンが Manager と完全同期せず、ユーザーデータ
            // を破壊しないようにするための設計判断。
            var A = NewObj("A");
            var B = NewObj("B");
            var X = NewObj("X");
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var SX = NewObj("SX");
            var (g, o) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                new[] { A, B, X }, new[] { SA, SB, SX },
                new[] { A, B },
                new Dictionary<string, ScriptableObject>());
            CollectionAssert.AreEqual(new[] { A, B, X }, g);
            CollectionAssert.AreEqual(new[] { SA, SB, SX }, o);
        }

        [Test]
        public void Fill_AdoptsSameNameSettingForExistingEmptyOverride()
        {
            // existing has A with empty override; same-name child setting is offered.
            var A = NewObj("A");
            var settingForA = NewObj("A");
            var dict = new Dictionary<string, ScriptableObject> { ["A"] = settingForA };
            var (g, o) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                new[] { A }, new ScriptableObject[] { null },
                new[] { A },
                dict);
            CollectionAssert.AreEqual(new[] { A }, g);
            CollectionAssert.AreEqual(new[] { settingForA }, o);
        }

        [Test]
        public void Fill_NonNullExistingOverrideNotReplacedByNamedSetting()
        {
            // existing override is already non-null; same-name child setting must NOT replace it.
            // 注: 既存 override が non-null の時は、同名 child setting が見つかっても
            // 上書きしない。ユーザーが手動でアサインした override を尊重する設計判断。
            // null の場合のみ同名採用が発動する (Fill_AdoptsSameNameSettingForExistingEmptyOverride)。
            var A = NewObj("A");
            var SA1 = NewObj("Override");      // user-attached object, name does not match group
            var SA2 = NewObj("A");             // same-name child object, would be picked if SA1 were null
            var dict = new Dictionary<string, ScriptableObject> { ["A"] = SA2 };
            var (g, o) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                new[] { A }, new[] { SA1 },
                new[] { A },
                dict);
            CollectionAssert.AreEqual(new[] { A }, g);
            CollectionAssert.AreEqual(new[] { SA1 }, o);
        }

        [Test]
        public void Fill_ManagerHasGroupsNotInExisting_AddedAsPlaceholder()
        {
            // Manager [A, B, C] but existing has only [B, A]. Result should be [A, B, C].
            var A = NewObj("A");
            var B = NewObj("B");
            var C = NewObj("C");
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var (g, o) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                new[] { B, A }, new[] { SB, SA },
                new[] { A, B, C },
                new Dictionary<string, ScriptableObject>());
            CollectionAssert.AreEqual(new[] { A, B, C }, g);
            CollectionAssert.AreEqual(new ScriptableObject[] { SA, SB, null }, o);
        }

        [Test]
        public void Fill_NullsInManagerGroupsSkipped()
        {
            // managerGroups = [A, null, B] should be treated as [A, B].
            var A = NewObj("A");
            var B = NewObj("B");
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var (g, o) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                new[] { A, B }, new[] { SA, SB },
                new ScriptableObject[] { A, null, B },
                new Dictionary<string, ScriptableObject>());
            CollectionAssert.AreEqual(new[] { A, B }, g);
            CollectionAssert.AreEqual(new[] { SA, SB }, o);
        }

        [Test]
        public void Fill_EmptyManager_KeepsExistingOrder()
        {
            // managerGroups empty → ordering driven solely by existing entries.
            var A = NewObj("A");
            var B = NewObj("B");
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var (g, o) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                new[] { A, B }, new[] { SA, SB },
                new ScriptableObject[0],
                new Dictionary<string, ScriptableObject>());
            CollectionAssert.AreEqual(new[] { A, B }, g);
            CollectionAssert.AreEqual(new[] { SA, SB }, o);
        }

        [Test]
        public void Fill_NullManagerArgument_TreatedAsNoOrderingHint()
        {
            // managerGroups = null is allowed; existing order is preserved.
            var A = NewObj("A");
            var B = NewObj("B");
            var SA = NewObj("SA");
            var SB = NewObj("SB");
            var (g, o) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                new[] { A, B }, new[] { SA, SB },
                null,
                new Dictionary<string, ScriptableObject>());
            CollectionAssert.AreEqual(new[] { A, B }, g);
            CollectionAssert.AreEqual(new[] { SA, SB }, o);
        }

        [Test]
        public void Fill_NullSettingsByName_NoCrashAndKeepsCurrentOnly()
        {
            // settingsByName = null should be tolerated.
            var A = NewObj("A");
            var SA = NewObj("SA");
            var (g, o) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                new[] { A }, new[] { SA },
                new[] { A },
                null);
            CollectionAssert.AreEqual(new[] { A }, g);
            CollectionAssert.AreEqual(new[] { SA }, o);
        }

        [Test]
        public void Fill_DuplicateGroupInExisting_DedupedToOne()
        {
            // [A, A] [SA1, SA2] → currentMap[A] = SA2 (last wins). Result: [A].
            // 注: Detect ボタンは重複 group を 1 つに集約する。Dictionary[group]=setting の
            // 上書きで後勝ち (last-wins) になる実装定義の挙動。Reorder では重複 group の
            // 個別 setting は維持されるが、Fill では「Manager 順に整列 + 重複解消」が
            // 主目的なのでこの挙動を採用している。
            var A = NewObj("A");
            var SA1 = NewObj("SA1");
            var SA2 = NewObj("SA2");
            var (g, o) = PlayerVolumeGroupListenOverridesUtil.ComputeFilledOverrides(
                new[] { A, A }, new[] { SA1, SA2 },
                new[] { A },
                new Dictionary<string, ScriptableObject>());
            CollectionAssert.AreEqual(new[] { A }, g);
            CollectionAssert.AreEqual(new[] { SA2 }, o);
        }
    }
}
