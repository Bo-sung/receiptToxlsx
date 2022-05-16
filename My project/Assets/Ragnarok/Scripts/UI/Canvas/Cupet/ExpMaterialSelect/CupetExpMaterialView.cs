using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UICupetExpMaterialSelect"/>
    /// </summary>
    public class CupetExpMaterialView : LevelUpMaterialSelectView
    {
        [SerializeField] UICupetProfile cupetProfile;

        protected override void Awake()
        {
            base.Awake();

            startDisplayLevel = 1; // 큐펫 레벨은 1부터 보여짐
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelNoData.LocalKey = LocalizeKey._9102; // 보유한 강화 재료가 없습니다.
            labelNotice.LocalKey = LocalizeKey._9103; // 강화에 필요한 재료를 선택하세요.
        }

        public void SetCupet(CupetModel cupetModel, int curPoint, int[] maxPoints)
        {
            cupetProfile.SetData(cupetModel);
            SetPoint(curPoint, maxPoints);
        }
    }
}