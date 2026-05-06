using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    static class AxisAllignedChecker
    {
        const float epsilon = 1e-4f;

        public static bool IsRotationAxisAligned(Transform t)
        {
            // localToWorldMatrix の3列がそれぞれワールド軸に整列しているかをチェック
            // (スケールは長さに出るだけで軸整列性には影響しないので、列の「非ゼロ成分が1つだけ」で判定)
            var m = t.localToWorldMatrix;
            return IsSingleAxis(m.GetColumn(0))
                && IsSingleAxis(m.GetColumn(1))
                && IsSingleAxis(m.GetColumn(2));
        }

        static bool IsSingleAxis(Vector4 v)
        {
            // 列の長さに対する相対比較。lossyScaleが100でも0.001でも安定。
            float len = new Vector3(v.x, v.y, v.z).magnitude;
            if (len < 1e-6f) return false; // 退化
            float threshold = len * epsilon;

            int nonZero = 0;
            if (Mathf.Abs(v.x) > threshold) nonZero++;
            if (Mathf.Abs(v.y) > threshold) nonZero++;
            if (Mathf.Abs(v.z) > threshold) nonZero++;
            return nonZero == 1;
        }

        // Udon的には命令数が少ないこっちの方が？

        public static bool IsRotationAxisAligned2(Transform t)
        {
            return IsAxisAligned(t.right) && IsAxisAligned(t.up);
        }

        const float sqrEpsilon = 1e-8f; // 0.006° (≈ 1e-4 rad)

        static bool IsAxisAligned(Vector3 v)
        {
            var small = 0;
            var x = v.x;
            var y = v.y;
            var z = v.z;
            if (x * x < sqrEpsilon) small++;
            if (y * y < sqrEpsilon) small++;
            if (z * z < sqrEpsilon) small++;
            return small == 2; // 2成分が小さい = 1成分だけ大きい
        }
    }
}
