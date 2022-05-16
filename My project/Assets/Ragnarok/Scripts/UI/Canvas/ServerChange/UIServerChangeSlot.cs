using UnityEngine;

namespace Ragnarok.View.ServerChange
{
    public class UIServerChangeSlot : UIView
    {
        [SerializeField] GameObject info;
        [SerializeField] GameObject emptyInfo;
        [SerializeField] GameObject select;

        [SerializeField] UITextureHelper thumbnail;
        [SerializeField] UITextureHelper iconJob;

        [SerializeField] UILabel labelLevel;
        [SerializeField] UILabel labelName;
        [SerializeField] UILabel labelID;
        [SerializeField] UILabel labelLocal;
        [SerializeField] UILabelHelper labelNoData;
        [SerializeField] UILabelHelper labelImpossible;
        [SerializeField] UISprite spriteComplex;

        int serverGroupID;

        public void SetData(ServerInfoPacket serverInfo, bool isSeleted)
        {
            if (!serverInfo.HasCharacter()) // 캐릭터 없음
            {
                emptyInfo.SetActive(true);
                info.SetActive(false);
            }
            else
            {
                emptyInfo.SetActive(false);
                info.SetActive(true);

                var job = serverInfo.Job.ToEnum<Job>();
                var gender = serverInfo.Gender.ToEnum<Gender>();
                thumbnail.Set(serverInfo.ProfileName);
                iconJob.Set(job.GetJobIcon());

                labelLevel.text = LocalizeKey._39003.ToText()
                .Replace(ReplaceKey.LEVEL, serverInfo.JobLevel); // Lv. {LEVEL}
                labelName.text = serverInfo.Name;
                labelID.text = LocalizeKey._33052.ToText() // ID : {VALUE}
                    .Replace(ReplaceKey.VALUE, MathUtils.CidToHexCode(serverInfo.Cid));
            }

            select.SetActive(isSeleted);
            labelLocal.text = serverInfo.ServerNameKey.ToText(); // 해당 서버이름
            serverGroupID = serverInfo.ServerGroupId;

            spriteComplex.spriteName = GetComplexName(serverInfo.Complex);
            labelImpossible.SetActive(!serverInfo.AvailableCreate || serverInfo.State != ServerState.EXTERNAL_OPEN);

            if (serverInfo.State != ServerState.EXTERNAL_OPEN)
            {
                labelImpossible.Text = string.Format("({0})", LocalizeKey._1108.ToText()); // 점검 중
            }
            else
            {
                labelImpossible.Text = string.Format("({0})", LocalizeKey._1107.ToText()); // 생성불가
            }
        }

        public void SetActiveSelect(bool isActive)
        {
            select.SetActive(isActive);
        }

        public int GetServerGroupId()
        {
            return serverGroupID;
        }

        protected override void OnLocalize()
        {
            labelNoData.LocalKey = LocalizeKey._1105; // 캐릭터가 없습니다.
            //labelImpossible.Text = string.Format("({0})", LocalizeKey._1107.ToText()); // 생성불가
        }

        string GetComplexName(ServerComplex complex)
        {
            switch (complex)
            {
                case ServerComplex.SMOOTH:
                    return "Ui_Common_Icon_SeverState_01";
                case ServerComplex.CROWDED:
                    return "Ui_Common_Icon_SeverState_02";
                case ServerComplex.VERY_CROWDED:
                    return "Ui_Common_Icon_SeverState_03";
                default:
                    return "Ui_Common_Icon_SeverState_04";
            }
        }
    }
}