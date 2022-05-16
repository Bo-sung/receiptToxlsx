using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="GuildBattleInfoView"/>
    /// <seealso cref="GuildBattleReadyView"/>
    /// </summary>
    public class UITurretInfo : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] CupetElement[] cupets;

        public event System.Action<ICupetModel> OnSelectCupet;

        void Awake()
        {
            for (int i = 0; i < cupets.Length; i++)
            {
                cupets[i].OnSelectBtnCupet += OnSelectBtnCupet;
            }
        }

        void OnDestroy()
        {
            for (int i = 0; i < cupets.Length; i++)
            {
                cupets[i].OnSelectBtnCupet -= OnSelectBtnCupet;
            }
        }

        void OnSelectBtnCupet(ICupetModel cupet)
        {
            OnSelectCupet?.Invoke(cupet);
        }

        public void SetCupet(CupetModel[] cupetModels)
        {
            for (int i = 0; i < cupets.Length; i++)
            {
                bool isValid = cupetModels != null && i < cupetModels.Length;
                cupets[i].Set(isValid ? cupetModels[i] : null);
            }
        }

        bool IInspectorFinder.Find()
        {
            cupets = GetComponentsInChildren<CupetElement>();
            return true;
        }
    }
}