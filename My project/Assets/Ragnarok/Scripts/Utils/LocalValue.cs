using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public static class LocalValue
    {
        private enum Type
        {
            /// <summary>
            /// 리뷰쓰기 동의
            /// </summary>
            AgreedReview,

            /// <summary>
            /// 카메라 흔들림
            /// </summary>
            CameraShake,

            /// <summary>
            /// 스탯 포인트 10배 사용
            /// </summary>
            StatPointTenFold,
        }

        public static void Clear()
        {
            foreach (Type item in System.Enum.GetValues(typeof(Type)))
            {
                string key = item.ToString();
                if (!ObscuredPrefs.HasKey(key))
                    continue;

                ObscuredPrefs.DeleteKey(key);
            }
        }

        /// <summary>
        /// 리뷰쓰기 동의 여부
        /// </summary>
        public static bool IsAgreedReview
        {
            get => ObscuredPrefs.GetBool(nameof(Type.AgreedReview), defaultValue: false);
            set => ObscuredPrefs.SetBool(nameof(Type.AgreedReview), value);
        }

        /// <summary>
        /// 카메라 흔들림 여부
        /// </summary>
        public static bool IsCameraShake
        {
            get => ObscuredPrefs.GetBool(nameof(Type.CameraShake), defaultValue: true);
            set => ObscuredPrefs.SetBool(nameof(Type.CameraShake), value);
        }

        /// <summary>
        /// 스탯 포인트 10배 사용 여부
        /// </summary>
        public static bool IsStatPointTenFold
        {
            get => ObscuredPrefs.GetBool(nameof(Type.StatPointTenFold), defaultValue: false);
            set => ObscuredPrefs.SetBool(nameof(Type.StatPointTenFold), value);
        }
    }
}