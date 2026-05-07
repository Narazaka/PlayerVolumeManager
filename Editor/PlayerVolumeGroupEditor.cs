using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    [CustomEditor(typeof(PlayerVolumeGroup), true)]
    public class PlayerVolumeGroupEditor : PlayerVolumeSettingEditor
    {
        SerializedProperty _matchWhenListener;
        SerializedProperty _matchWhenSpeaker;
        SerializedProperty _fallbackToNextGroup;

        readonly Dictionary<PlayerVolumeListenPair, SerializedObject> _pairSerializedObjects
            = new Dictionary<PlayerVolumeListenPair, SerializedObject>();

        protected override void OnEnable()
        {
            base.OnEnable();
            _matchWhenListener = serializedObject.FindProperty(nameof(PlayerVolumeGroup._matchWhenListener));
            _matchWhenSpeaker = serializedObject.FindProperty(nameof(PlayerVolumeGroup._matchWhenSpeaker));
            _fallbackToNextGroup = serializedObject.FindProperty(nameof(PlayerVolumeGroup._fallbackToNextGroup));
        }

        void OnDisable()
        {
            _pairSerializedObjects.Clear();
        }

        public override void Draw()
        {
            DrawMisc();
            if (_matchWhenListener.boolValue)
            {
                DrawHeader(T.ListenFromGroupOverride);
                DrawDetectButton();
                PlayerVolumeSettingGUI.DrawModeDropdownLayout();
                DrawSortModeToggle();

                DrawListenPairs();

                DrawHeader(T.ListenDefault);
                using (new EditorGUI.IndentLevelScope())
                {
                    base.DrawVolumeSetting();
                }

                DrawConsistencyWarning();
            }
        }

        void DrawDetectButton()
        {
            var owner = (PlayerVolumeGroup)target;
            if (GUILayout.Button(T.Detect))
            {
                var pairs = owner.GetComponentsInChildren<PlayerVolumeListenPair>(true);
                DetectPairs(owner, pairs, GetManagerGroups());
            }
        }

        void DrawMisc()
        {
            EditorGUILayout.PropertyField(_matchWhenListener, T.MatchWhenListener.GUIContent);
            EditorGUILayout.PropertyField(_matchWhenSpeaker, T.MatchWhenSpeaker.GUIContent);
            EditorGUILayout.PropertyField(_fallbackToNextGroup, T.FallbackToNextGroup.GUIContent);
        }

        void DrawSortModeToggle()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(T.Sort.GUIContent);
                var current = PlayerVolumeSettingGUI.GetPairSortMode();
                var managerOn = GUILayout.Toggle(current == PlayerVolumeListenPairView.SortMode.ManagerOrder, T.SortModeManager, EditorStyles.miniButtonLeft);
                var detectionOn = GUILayout.Toggle(current == PlayerVolumeListenPairView.SortMode.DetectionOrder, T.SortModeDetection, EditorStyles.miniButtonRight);
                if (managerOn && current != PlayerVolumeListenPairView.SortMode.ManagerOrder)
                    PlayerVolumeSettingGUI.SetPairSortMode(PlayerVolumeListenPairView.SortMode.ManagerOrder);
                else if (detectionOn && current != PlayerVolumeListenPairView.SortMode.DetectionOrder)
                    PlayerVolumeSettingGUI.SetPairSortMode(PlayerVolumeListenPairView.SortMode.DetectionOrder);
            }
        }

        void DrawListenPairs()
        {
            var owner = (PlayerVolumeGroup)target;
            var pairs = owner.GetComponentsInChildren<PlayerVolumeListenPair>(true);
            var managerGroups = GetManagerGroups();

            // View 用 Item に変換
            var items = new List<PlayerVolumeListenPairView.Item>(pairs.Length);
            for (var i = 0; i < pairs.Length; i++)
            {
                items.Add(new PlayerVolumeListenPairView.Item(pairs[i]._group, key: i));
            }
            var view = PlayerVolumeListenPairView.Compute(items, managerGroups, PlayerVolumeSettingGUI.GetPairSortMode());

            // 各 Pair 行 (sort 結果順)
            foreach (var item in view)
            {
                var pair = pairs[item.Key];
                if (pair == null) continue;
                var isSelf = pair._group != null && pair._group == owner;

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    DrawPairHeaderRow(pair, item, isSelf);

                    if (item.IsOrphan)
                        EditorGUILayout.HelpBox(T.OrphanWarning, MessageType.Warning);
                    if (item.IsOutsideManager)
                        EditorGUILayout.HelpBox(T.OutsideWarning, MessageType.Warning);
                    if (item.IsDuplicate)
                        EditorGUILayout.HelpBox(T.DuplicateWarning, MessageType.Warning);

                    var pairSerializedObject = GetOrCreatePairSerializedObject(pair);
                    pairSerializedObject.UpdateIfRequiredOrScript();
                    PlayerVolumeListenPairEditor.DrawInline(pairSerializedObject, pair);
                    pairSerializedObject.ApplyModifiedProperties();
                }
            }

            // 削除予定の cache 要素をクリーンアップ
            CleanupStalePairSerializedObjects(pairs);
        }

        static class T
        {
            public static readonly istring MatchWhenListener = new istring(
                "Match When Listener", "リスナー時にマッチ",
                "Considered when matching as listener (= local processing).",
                "リスナーとしての判定時に考慮される（ローカルでの処理時に考慮される）");
            public static readonly istring MatchWhenSpeaker = new istring(
                "Match When Speaker", "スピーカー時にマッチ",
                "Considered when matching as speaker (= remote processing).",
                "スピーカーとしての判定時に考慮される（リモートでの処理時に考慮される）");
            public static readonly istring FallbackToNextGroup = new istring(
                "Fallback To Next Group", "次のグループにフォールバック",
                "For items not overridden by this group, defer to the next group instead of the Manager.",
                "このグループでオーバーライドされない項目を、Managerではなく次のグループにまかせる");

            public static readonly istring ListenFromGroupOverride = new istring("Listen from Group Override", "グループ別 Listen オーバーライド");
            public static readonly istring Detect = new istring("Detect", "検出");
            public static readonly istring Sort = new istring("(Sort)", "(並び順)");
            public static readonly istring SortModeManager = new istring("Manager", "Manager 順");
            public static readonly istring SortModeDetection = new istring("Detection", "検出順");
            public static readonly istring ListenDefault = new istring("Listen Default", "Listen 既定値");
            public static readonly istring SelfBadge = new istring("Self", "Self");
            public static readonly istring OrphanWarning = new istring(
                "Group is not assigned.",
                "group が未設定です。");
            public static readonly istring OutsideWarning = new istring(
                "This group is not registered in Manager._groups.",
                "この group は Manager._groups に登録されていません。");
            public static readonly istring DuplicateWarning = new istring(
                "The same group is assigned to multiple Pairs.",
                "同じ group が複数の Pair に設定されています。");
            public static readonly istring SelfBadgeTooltip = new istring(
                "This pair targets the owning group itself (= override applied when the group itself matches).",
                "このグループ自身を override 対象にしています (= 自分が match したときに自身の値で上書き)");
            public static readonly istring GroupConsistencyHeader = new istring(
                "These fields have override values, but no fallback exists in this group's Listen Default or in the Manager:\n",
                "以下の項目は override で値が設定されていますが、このグループの Listen 既定値にも Manager にもフォールバック値がありません:\n");
        }

        static readonly Color SelfBadgeColor = new Color(0.30f, 0.55f, 0.85f);

        static GUIStyle _badgeStyle;
        static GUIStyle BadgeStyle
        {
            get
            {
                if (_badgeStyle == null)
                {
                    _badgeStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        normal = { textColor = Color.white },
                        padding = new RectOffset(6, 6, 1, 1),
                        fixedHeight = EditorGUIUtility.singleLineHeight,
                    };
                }
                return _badgeStyle;
            }
        }

        static void DrawBadge(string text, Color color, string tooltip = null)
        {
            var content = new GUIContent(text, tooltip);
            var size = BadgeStyle.CalcSize(content);
            var rect = GUILayoutUtility.GetRect(size.x, EditorGUIUtility.singleLineHeight, GUILayout.Width(size.x));
            EditorGUI.DrawRect(rect, color);
            GUI.Label(rect, content, BadgeStyle);
        }

        void DrawPairHeaderRow(PlayerVolumeListenPair pair, PlayerVolumeListenPairView.Item item, bool isSelf)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (isSelf)
                    DrawBadge(T.SelfBadge, SelfBadgeColor, T.SelfBadgeTooltip);
                GUILayout.Label(pair.gameObject.name, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("×", GUILayout.Width(24)))
                {
                    Undo.DestroyObjectImmediate(pair.gameObject);
                    GUIUtility.ExitGUI();
                }
            }
        }

        SerializedObject GetOrCreatePairSerializedObject(PlayerVolumeListenPair pair)
        {
            if (_pairSerializedObjects.TryGetValue(pair, out var existing) && existing != null && existing.targetObject == pair) return existing;
            var pairSerializedObject = new SerializedObject(pair);
            _pairSerializedObjects[pair] = pairSerializedObject;
            return pairSerializedObject;
        }

        void CleanupStalePairSerializedObjects(PlayerVolumeListenPair[] currentPairs)
        {
            var keep = new HashSet<PlayerVolumeListenPair>(currentPairs);
            var toRemove = new List<PlayerVolumeListenPair>();
            foreach (var kv in _pairSerializedObjects)
            {
                if (kv.Key == null || !keep.Contains(kv.Key))
                {
                    toRemove.Add(kv.Key);
                }
            }
            foreach (var k in toRemove) _pairSerializedObjects.Remove(k);
        }

        void DetectPairs(PlayerVolumeGroup owner, PlayerVolumeListenPair[] existing, IReadOnlyList<PlayerVolumeGroup> managerGroups)
        {
            var existingGroups = new List<PlayerVolumeGroup>(existing.Length);
            for (var i = 0; i < existing.Length; i++) existingGroups.Add(existing[i]._group);
            var missing = PlayerVolumeListenPairDetect.Compute(existingGroups, managerGroups);

            // wrapper 確保
            var wrapper = owner.transform.Find(PlayerVolumeSettingGUI.ListenOverridesParentName);
            if (wrapper == null)
            {
                var wrapperGO = new GameObject(PlayerVolumeSettingGUI.ListenOverridesParentName);
                Undo.RegisterCreatedObjectUndo(wrapperGO, "Create Listen Overrides Wrapper");
                wrapperGO.transform.SetParent(owner.transform, false);
                wrapper = wrapperGO.transform;
            }

            foreach (var g in missing)
            {
                var pairGO = new GameObject(g != null ? g.name : "Pair");
                Undo.RegisterCreatedObjectUndo(pairGO, "Create Listen Pair");
                pairGO.transform.SetParent(wrapper, false);
                var pair = pairGO.AddComponent<PlayerVolumeListenPair>();
                pair._group = g;
            }
        }

        static IReadOnlyList<PlayerVolumeGroup> GetManagerGroups()
        {
            var manager = PlayerVolumeSettingGUI.GetManager();
            if (manager == null) return new PlayerVolumeGroup[0];
            var so = new SerializedObject(manager);
            var prop = so.FindProperty("_groups");
            if (prop == null) return new PlayerVolumeGroup[0];
            var n = prop.arraySize;
            var arr = new PlayerVolumeGroup[n];
            for (var i = 0; i < n; i++) arr[i] = prop.GetArrayElementAtIndex(i).objectReferenceValue as PlayerVolumeGroup;
            return arr;
        }

        void DrawConsistencyWarning()
        {
            var owner = (PlayerVolumeGroup)target;
            var pairs = owner.GetComponentsInChildren<PlayerVolumeListenPair>(true);
            var overrides = new List<PlayerVolumeSettingByGroup>(pairs.Length);
            for (var i = 0; i < pairs.Length; i++) overrides.Add(pairs[i]._setting);

            var missing = PlayerVolumeSettingGUI.FindMissingFallback(owner, overrides, PlayerVolumeSettingGUI.GetManager());
            if (missing.Count == 0) return;
            var sb = new System.Text.StringBuilder();
            foreach (var name in missing) sb.AppendLine($"  - {name}");
            EditorGUILayout.HelpBox(T.GroupConsistencyHeader + sb.ToString(), MessageType.Warning);
        }

        private protected override PlayerVolumeSettingGUI.Properties BuildFallback() =>
            PlayerVolumeSettingGUI.GetManagerFallbackProperties();

        protected override HashSet<string> KnownProperties
        {
            get
            {
                if (_knownProperties == null)
                {
                    _knownProperties = new HashSet<string>(base.KnownProperties);
                    _knownProperties.Add(nameof(PlayerVolumeGroup._matchWhenListener));
                    _knownProperties.Add(nameof(PlayerVolumeGroup._matchWhenSpeaker));
                    _knownProperties.Add(nameof(PlayerVolumeGroup._fallbackToNextGroup));
                }
                return _knownProperties;
            }
        }

        static HashSet<string> _knownProperties = null;
    }
}
