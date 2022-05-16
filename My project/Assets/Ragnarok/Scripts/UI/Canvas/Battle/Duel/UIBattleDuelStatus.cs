using UnityEngine;

namespace Ragnarok.View.BattleLeague
{
    public class UIBattleDuelStatus : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper iconRank;
        [SerializeField] UILabelHelper labelLevel;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UIPlayerStatus playerStatus;

        public void ResetData()
        {
            playerStatus.ResetData();
        }

        public void SetData(string iconRankName, CharacterEntity entity)
        {
            iconRank.SetRankTier(iconRankName);
            labelLevel.Text = string.Concat("Lv. ", entity.Character.JobLevel.ToString());
            labelName.Text = entity.GetName();

            playerStatus.SetData(entity);
        }

        public void SetAgents(CharacterEntity[] agents)
        {
            playerStatus.SetAgents(agents);
        }
    }
}