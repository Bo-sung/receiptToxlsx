using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// UIManager 클래스 래핑
    /// </summary>
    public sealed class UI
    {
        private const float DELAY_TO_INTRO_TIME = 30f; // 일정시간 딜레이 후 타이틀 화면으로 이동하는 시간

        public static void AddEventLocalize(Action action)
        {
            UIManager.Instance.OnLocalize += action;
        }

        public static void RemoveEventLocalize(Action action)
        {
            UIManager.Instance.OnLocalize -= action;
        }

        /// <summary>
        /// UI 켜기
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="isSkipAni">오픈 애니메이션 스킵여부</param>
        /// <returns></returns>
        public static T Show<T>(IUIData data = null, bool isSkipAni = false) where T : UICanvas
        {
            UIManager.Instance.Show<T>(data, isSkipAni);
            return GetUI<T>();
        }

        /// <summary>
        /// UI 켜기
        /// </summary>
        public static void Show(string canvasName, IUIData data = null, bool isSkipAni = false)
        {
            UIManager.Instance.Show(canvasName, data, isSkipAni);
        }

        /// <summary>
        /// UIType.Fixed를 제외한 모든 UI를 제거하고 지정 UI를 킨다.
        /// forceClose가 true 일 경우 UIType.Close 타입으로 실행
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="isSkipAni">종료 애니메이션 스킵여부</param>
        /// <param name="isDestroy">UI 강제 제거 여부</param>
        public static T ShortCut<T>(IUIData data = null, bool isSkipAni = false, bool isDestroy = false) where T : UICanvas
        {
            UIManager.Instance.ShortCut(isDestroy);
            UIManager.Instance.Show<T>(data, isSkipAni);
            return GetUI<T>();
        }

        /// <summary>
        /// UI 켜기
        /// </summary>
        public static void ShortCut(string canvasName, IUIData data = null, bool isSkipAni = false, bool isDestroy = false)
        {
            UIManager.Instance.ShortCut(isDestroy);
            UIManager.Instance.Show(canvasName, data, isSkipAni);
        }

        public static bool Close<T>(bool isSkipAni = false, bool isDestroy = false) where T : UICanvas
        {
            return UIManager.Instance.Close<T>(isSkipAni, isDestroy);
        }

        public static void CloseAll()
        {
            UIManager.Instance.CloseAll();
        }

        public static T GetUI<T>() where T : UICanvas
        {
            return UIManager.Instance.GetUI<T>();
        }

        public static bool Destroy<T>(bool skipAnim = true) where T : UICanvas
        {
            return UIManager.Instance.Destroy<T>(skipAnim);
        }

        /// <summary>
        /// 종료 팝업
        /// </summary>
        public static void ShowExitPopup()
        {
            AsyncShowExitPopup().WrapNetworkErrors();
        }

        private static async Task AsyncShowExitPopup()
        {
            string title = LocalizeKey._3.ToText(); // 게임 종료
            string message;

            // 캐릭터 게임 진입
            if (PlayerEntity.IsJoinGameMap)
            {
                BattleMode curMode = BattleManager.Instance.Mode;
                if (curMode == BattleMode.Stage || curMode == BattleMode.Lobby || curMode == BattleMode.MultiMazeLobby)
                {
                    SharingModel.SharingState state = Entity.player.Sharing.GetSharingState();
                    if (state == SharingModel.SharingState.None) // 셰어링 상태가 아닐 때
                    {
                        // 종료 시 셰어 자동 등록 상태가 아닐 경우
                        if (!Entity.player.Sharing.IsShareExitAutoSetting())
                        {
                            message = LocalizeKey._90179.ToText(); // 게임을 종료하시겠습니까?\n(추가 보상을 위해 셰어 등록을 추천드립니다)
                            string linkDescription = LocalizeKey._90180.ToText(); // [셰어 등록 바로가기]
                            if (await SelectShortCutPopup(title, message, linkDescription, ShowCharacterShareUI, isShowIcon: true))
                                ApplicationQuit();

                            return;
                        }
                    }

                    if (state == SharingModel.SharingState.Sharing) // 셰어링 중일 때
                    {
                        message = LocalizeKey._10318.ToText(); // 게임을 종료하시겠습니까?\n셰어 상태는 유지 됩니다.
                        if (await SelectPopup(title, message, isShowIcon: true))
                            ApplicationQuit();

                        return;
                    }
                }
            }

            // 게임 종료 UI
            message = LocalizeKey._4.ToText();
            if (await SelectPopup(title, message, isShowIcon: true))
                ApplicationQuit();
        }

        private static void ApplicationQuit()
        {
            Application.Quit();

#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
#endif
        }

        private static void ShowCharacterShareUI()
        {
            UICharacterShare ui = Show<UICharacterShare>();
            ui.SetRegisterMode();
        }

        /// <summary>
        /// 확인 팝업
        /// </summary>
        public static void ConfirmPopup(string message, Action action, float timeout = 0f)
        {
            ConfirmPopupAsync(message, action, timeout: timeout).WrapNetworkErrors();
        }

        /// <summary>
        /// 확인 팝업
        /// </summary>
        /// <param name="description"></param>
        public static void ConfirmPopup(int description)
        {
            ConfirmPopup(description.ToText());
        }

        /// <summary>
        /// 확인 팝업
        /// </summary>
        public static void ConfirmPopup(string description)
        {
            Debug.Log($"ShowConfirmPopup : {description}");
            ConfirmPopup(LocalizeKey._5.ToText(), description);
        }

        /// <summary>
        /// 확인 팝업 (큰 팝업)
        /// </summary>
        public static void ConfirmPopupLong(string description)
        {
            Debug.Log($"ShowConfirmPopup : {description}");
            ConfirmPopupLong(LocalizeKey._5.ToText(), description);
        }

        /// <summary>
        /// 확인 팝업
        /// </summary>
        public static void ConfirmPopup(string title, string description)
        {
            ConfirmPopup(title, description, LocalizeKey._1.ToText());
        }

        /// <summary>
        /// 확인 팝업
        /// </summary>
        public static void ConfirmPopup(string title, string description, string confirmText)
        {
            var data = new UIConfirmPopupData()
            {
                title = title,
                description = description,
                confirmText = confirmText,
            };

            Show<UIConfirmPopup>(data);
        }

        /// <summary>
        /// 확인 팝업 (큰 텍스트)
        /// </summary>
        public static void ConfirmPopupLong(string title, string description)
        {
            var data = new UIConfirmPopupData()
            {
                title = title,
                description = description,
                confirmText = LocalizeKey._1.ToText(),
            };

            Show<UIConfirmPopupLong>(data);
        }

        public static void ConfirmPopupLong(string title, string description, string confrimText, Action action = null)
        {
            var data = new UIConfirmPopupData()
            {
                title = title,
                description = description,
                confirmText = confrimText,
            };

            Show<UIConfirmPopupLong>(data).SetAction(action);
        }

        public static async Task ConfirmPopupAsync(string description, Action action = null, float timeout = 0f)
        {
            await ConfirmPopupAsync(LocalizeKey._5.ToText(), description, timeout: timeout);
            action?.Invoke();
        }

        public static async Task ConfirmPopupAsync(string title, string description, float timeout = 0f)
        {
            PopupType popupType = PopupType.None;
            var data = new UIConfirmPopupData()
            {
                title = title,
                description = description,
                timeout = timeout,
                confirmText = LocalizeKey._1.ToText(),
            };

            if (timeout > 0f)
            {
                Show<UIConfirmTimeOutPopup>(data.OnClickedEvent(() =>
                {
                    popupType = PopupType.Confirm;
                }));
            }
            else
            {
                Show<UIConfirmPopup>(data.OnClickedEvent(() =>
                {
                    popupType = PopupType.Confirm;
                }));
            }

            await new WaitUntil(() => popupType != PopupType.None);

            return;
        }

        public static async Task<string> InputPopupAsync(string title, string description, string inputDefault = default, UIInput.KeyboardType keyboardType = UIInput.KeyboardType.Default)
        {
            PopupType popupType = PopupType.None;

            string response = string.Empty;
            Show<UIInputPopup>().Show(title, description, inputDefault, keyboardType, (answer) =>
            {
                response = answer;
                popupType = PopupType.Confirm;
            });

            await new WaitUntil(() => popupType != PopupType.None);

            return response;
        }

        public static async Task<int?> InputPopupAsync(string title, string description, string inputDefault = default, int max = 0)
        {
            PopupType popupType = PopupType.None;

            string response = string.Empty;
            Show<UIInputPopup>().Show(title, description, inputDefault, max, (answer) =>
            {
                response = answer;
                popupType = PopupType.Confirm;
            });

            await new WaitUntil(() => popupType != PopupType.None);

            if (int.TryParse(response, out int result))
                return result;

            return null;
        }

        /// <summary>
        /// 확인 쇼트컷 팝업
        /// </summary>
        public static void ConfirmShortCutPopup(int description, int linkDescription, System.Action linkAction)
        {
            ConfirmShortCutPopup(description.ToText(), linkDescription.ToText(), linkAction);
        }

        /// <summary>
        /// 확인 쇼트컷 팝업
        /// </summary>
        public static void ConfirmShortCutPopup(string description, string linkDescription, System.Action linkAction)
        {
            ConfirmShortCutPopup(LocalizeKey._5.ToText(), description, linkDescription, linkAction);
        }

        /// <summary>
        /// 확인 쇼트컷 팝업
        /// </summary>
        public static void ConfirmShortCutPopup(int title, int description, int linkDescription, System.Action linkAction)
        {
            ConfirmShortCutPopup(title.ToText(), description.ToText(), linkDescription.ToText(), linkAction);
        }

        /// <summary>
        /// 확인 쇼트컷 팝업
        /// </summary>
        public static void ConfirmShortCutPopup(string title, string description, string linkDescription, System.Action linkAction)
        {
            var data = new UIConfirmPopupData()
            {
                title = title,
                description = description,
                confirmText = LocalizeKey._1.ToText(),
            };

            Show<UIConfirmShortCutPopup>(data).SetAction(linkDescription, linkAction);
        }

        public static async Task ConfirmShortCutPopupAsync(string description, string linkDescription, System.Action linkAction)
        {
            await ConfirmShortCutPopupAsync(LocalizeKey._5.ToText(), description, linkDescription, linkAction);
        }

        public static async Task ConfirmShortCutPopupAsync(string title, string description, string linkDescription, System.Action linkAction)
        {
            PopupType popupType = PopupType.None;
            var data = new UIConfirmPopupData()
            {
                title = title,
                description = description,
                confirmText = LocalizeKey._1.ToText(),
            };

            Show<UIConfirmShortCutPopup>(data.OnClickedEvent(() =>
            {
                popupType = PopupType.Confirm;
            })).SetAction(linkDescription, linkAction);

            await new WaitUntil(() => popupType != PopupType.None);

            return;
        }

        /// <summary>
        /// 선택 팝업
        /// </summary>
        public static async Task<bool> SelectPopup(string description, bool isShowIcon = false)
        {
            return await SelectPopup(LocalizeKey._5.ToText(), description, isShowIcon);
        }

        /// <summary>
        /// 선택 팝업
        /// </summary>
        public static async Task<bool> SelectPopup(string title, string description, bool isShowIcon = false)
        {
            bool? result = await SelectPopup(title, description, LocalizeKey._1.ToText(), LocalizeKey._2.ToText(), isShowIcon);

            return result.HasValue && result.Value;
        }

        /// <summary>
        /// 선택 팝업
        /// </summary>
        public static async Task<bool> SelectPopup(string description, string confirmText, string cancelText, bool isShowIcon = false)
        {
            bool? result = await SelectPopup(LocalizeKey._5.ToText(), description, confirmText, cancelText, isShowIcon);

            return result.HasValue && result.Value;
        }

        // 선택 팝업 - 취소, 닫기 분할
        public static async Task<bool?> SelectClosePopup(string description, string confirmText, string cancelText, ConfirmButtonType confirmButtonType = ConfirmButtonType.None)
        {
            return await SelectPopup(LocalizeKey._5.ToText(), description, confirmText, cancelText, false, confirmButtonType);
        }

        /// <summary>
        /// 선택 팝업
        /// </summary>
        public static async Task<bool?> SelectPopup(string title, string description, string confirmText, string cancelText, bool isShowIcon = false, ConfirmButtonType confirmButtonType = ConfirmButtonType.None)
        {
            PopupType popupType = PopupType.None;
            var data = new UISelectPopupData()
            {
                title = title,
                description = description,
                isShowIcon = isShowIcon,
                confirmText = confirmText,
                cancelText = cancelText,
                confirmButtonType = confirmButtonType,
            };

            UI.Show<UISelectPopup>(data.OnClickedEvent((isConfirm) =>
            {
                if (!isConfirm.HasValue)
                {
                    popupType = PopupType.Close;
                }
                else
                {
                    if (isConfirm.Value)
                    {
                        popupType = PopupType.Confirm;
                    }
                    else
                    {
                        popupType = PopupType.Cancel;
                    }
                }
            }));

            await new WaitUntil(() => popupType != PopupType.None);

            switch (popupType)
            {
                case PopupType.Confirm:
                    return true;

                case PopupType.Cancel:
                    return false;

                //case PopupType.Close:
                default:
                    return null;
            }
        }

        public static async Task<bool> SelectShortCutPopup(string description, string linkDescription, System.Action linkAction, bool isShowIcon = false)
        {
            return await SelectShortCutPopup(LocalizeKey._5.ToText(), description, linkDescription, linkAction, isShowIcon);
        }

        /// <summary>
        /// 선택 팝업
        /// </summary>
        public static async Task<bool> SelectShortCutPopup(string title, string description, string linkDescription, System.Action linkAction, bool isShowIcon = false)
        {
            PopupType popupType = PopupType.None;
            var data = new UISelectPopupData()
            {
                title = title,
                description = description,
                isShowIcon = isShowIcon,
                confirmText = LocalizeKey._1.ToText(),
                cancelText = LocalizeKey._2.ToText(),
            };

            UI.Show<UISelectShortCutPopup>(data.OnClickedEvent((isConfirm) =>
            {
                if (isConfirm.HasValue && isConfirm.Value)
                {
                    popupType = PopupType.Confirm;
                }
                else
                {
                    popupType = PopupType.Cancel;
                }
            })).SetAction(linkDescription, linkAction);

            await new WaitUntil(() => popupType != PopupType.None);

            return popupType == PopupType.Confirm;
        }

        /// <summary>
        /// 재화 팝업
        /// </summary>
        public static async Task<bool> CostPopup(CoinType coinTyp, int needCoin, string title, string description)
        {
            PopupType popupType = PopupType.None;
            var data = new UICostPopupData()
            {
                coinType = coinTyp,
                needCoin = needCoin,
                title = title,
                description = description,
                confirmText = LocalizeKey._1.ToText(),
                cancelText = LocalizeKey._2.ToText(),
            };

            UI.Show<UICostPopup>(data.OnClickedEvent((isConfirm) =>
            {
                if (isConfirm.HasValue && isConfirm.Value)
                {
                    popupType = coinTyp.Check(needCoin) ? PopupType.Confirm : PopupType.Cancel;
                }
                else
                {
                    popupType = PopupType.Cancel;
                }
            }));

            await new WaitUntil(() => popupType != PopupType.None);

            return popupType == PopupType.Confirm;
        }

        /// <summary>
        /// 보상 획득 정보
        /// </summary>
        /// <param name="rewards"></param>
        public static void RewardInfo(RewardPacket[] rewards)
        {
            RewardInfo[] infos = ConvertRewardInfo(rewards);

            if (infos.Length == 0)
                return;

            Show<UIRewardInfo>(new UIRewardInfoData(infos));
        }

        public static void ShowIndicator(float delay = DELAY_TO_INTRO_TIME)
        {
            UIManager.Instance.ShowIndicator(delay);
        }

        public static void HideIndicator()
        {
            UIManager.Instance.HideIndicator();
        }

        public static void ShowLoudSpeaker()
        {
            UIManager.Instance.ShowLoudSpeaker();
        }

        public static void HideLoudSpeaker()
        {
            UIManager.Instance.HideLoudSpeaker();
        }

        /// <summary>
        /// 가방 무게 체크
        /// </summary>
        /// <returns></returns>
        public static bool CheckInvenWeight()
        {
            if (Entity.player.Inventory.IsWeightOver)
            {
                ConfirmPopup(LocalizeKey._90025.ToText(), LocalizeKey._90024.ToText()); // 가방 무게가 초과하였습니다.
                return false;
            }
            return true;
        }

        /// <summary>
        /// 가방 무게 체크 (선택) - 던전 진입 전 사용
        /// </summary>
        public static async Task<bool> CheckInvenWightSelect()
        {
            int invenWeight = Entity.player.Inventory.MaxInvenWeight;
            int currentInvenWeight = Entity.player.Inventory.CurrentInvenWeight;
            int remainInvenWight = invenWeight - currentInvenWeight; // 가방 여유 공간

            if (remainInvenWight < 10)
            {
                string title = LocalizeKey._90025.ToText(); // 가방 무게 부족
                string description = LocalizeKey._90026.ToText(); // 가방 무게가 부족하여 드랍되는 아이템을 획득하지 못할 수 있습니다.\n계속 진행하시겠습니까?
                return await SelectPopup(title, description);
            }

            return true;
        }

        public static void ShowToastPopup(string description)
        {
            UI.Show<UIToastPopup>(new ToastPopupData(description));
        }

        public static void ShowConfirmItemPopup(RewardType rewardType, int itemId, int itemCount, int descId)
        {
            ShowConfirmItemPopup(new RewardData(rewardType, itemId, itemCount), descId.ToText());
        }

        public static void ShowConfirmItemPopup(RewardType rewardType, int itemId, int itemCount, string description)
        {
            ShowConfirmItemPopup(new RewardData(rewardType, itemId, itemCount), description);
        }

        public static void ShowConfirmItemPopup(RewardData rewardData, int descId)
        {
            ShowConfirmItemPopup(rewardData, descId.ToText());
        }

        public static void ShowConfirmItemPopup(RewardData rewardData, string description)
        {
            Show<UIConfirmItemPopup>().SetData(rewardData, description);
        }

        public static void RewardToast(RewardPacket[] rewards, bool isExceptGoods = true)
        {
            if (rewards == null)
                return;

            foreach (var rewardPacket in rewards)
            {
                if (isExceptGoods)
                {
                    // 아이템 타입만 토스트팝업 사용
                    if (!rewardPacket.rewardType.ToEnum<RewardType>().IsRewardToast())
                        continue;
                }

                RewardData rewardData = new RewardData(rewardPacket.rewardType, rewardPacket.rewardValue, rewardPacket.rewardCount, rewardPacket.rewardOption);

                string icon_name = rewardData.IconName;
                string description = LocalizeKey._90038.ToText()
                      .Replace("{ItemName}", rewardData.ItemName)
                      .Replace("{Count}", rewardData.Count.ToString());

                UI.ShowRewardToast(icon_name, description);
            }
        }

        public static void ShowRewardToast(string iconName, string description)
        {
            var mode = BattleManager.Instance.GetCurrentEntry().mode;

            if (mode == BattleMode.MatchMultiMaze || mode == BattleMode.MatchMultiMazeBoss || mode == BattleMode.ScenarioMaze || mode == BattleMode.ScenarioMazeBoss || mode == BattleMode.MultiMaze || mode == BattleMode.MultiBossMaze)
                return;

            UI.Show<UIRewardToast>(new RewardToastData(iconName, description));
        }

        public static void ShowItemInfo(ItemInfo info)
        {
            if (info.ItemNo == 0)
            {
                // 보유중이 아닌 아이템
                switch (info.ItemGroupType)
                {
                    case ItemGroupType.Equipment:
                        UI.Show<UIEquipmentInfoSimple>(info);
                        break;
                    case ItemGroupType.Card:
                        UI.Show<UICardInfoSimple>().Show(info);
                        break;
                    case ItemGroupType.ConsumableItem:
                    case ItemGroupType.ProductParts:
                    case ItemGroupType.MonsterPiece:
                        UI.Show<UIPartsInfo>(info);
                        break;
                    case ItemGroupType.Costume:
                        UI.Show<UICostumeInfo>().Set(info);
                        break;
                }
            }
            else
            {
                // 보유중인 아이템
                switch (info.ItemGroupType)
                {
                    case ItemGroupType.Equipment:
                        UI.Show<UIEquipmentInfo>().Set(info.ItemNo);
                        break;
                    case ItemGroupType.Card:
                        UI.Show<UICardInfo>(info);
                        break;
                    case ItemGroupType.ConsumableItem:
                    case ItemGroupType.ProductParts:
                    case ItemGroupType.MonsterPiece:
                        UI.Show<UIPartsInfo>(info);
                        break;
                    case ItemGroupType.Costume:
                        UI.Show<UICostumeInfo>().Set(info.ItemNo);
                        break;
                }
            }
        }

        /// <summary>
        /// 전투력 변동 연출
        /// </summary>
        /// <param name="showAmountOfChange">true: 변화량만 보여줌. false: 최종 전투력의 변화를 보여줌.</param>
        public static void ShowAttackPowerChange(BattleStatusData battleStatusData, bool showAmountOfChange = false)
        {
            UIPowerUpdate.Input uiParam = new UIPowerUpdate.Input(battleStatusData, showAmountOfChange);
            UI.Show<UIPowerUpdate>(uiParam);
        }

        public static void ShowHUD()
        {
            UIManager.Instance.ShowHUD();
        }

        public static void HideHUD()
        {
            UIManager.Instance.HideHUD();
        }

        public static void AddMask(params int[] layers)
        {
            UIManager.Instance.AddMask(layers);
        }

        public static void RemoveMask(params int[] layers)
        {
            UIManager.Instance.RemoveMask(layers);
        }

        public static Camera CurrentCamera => UIManager.Instance.CurrentCamera;
        public static Camera AnchorCamera
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return NGUITools.FindCameraForLayer(Layer.UI);
#endif

                return UIManager.Instance.AnchorCamera;
            }
        }

        /// <summary>
        /// 보상 연출 (재화)
        /// </summary>
        public static void LaunchReward(Vector3 position, RewardType rewardType, int itemValue, UIRewardLauncher.GoodsDestination destination)
        {
            if (rewardType == RewardType.None || itemValue == 0)
                return;

            Show<UIRewardLauncher>().LaunchGoods(position, rewardType, itemValue, destination);
        }

        /// <summary>
        /// 보상 연출 (아이템)
        /// </summary>
        public static void LaunchReward(Vector3 position, string itemIcon, int itemSize, UIRewardLauncher.GoodsDestination destination)
        {
            if (string.IsNullOrEmpty(itemIcon) || itemSize == 0)
                return;

            Show<UIRewardLauncher>().LaunchItem(position, itemIcon, itemSize, destination);
        }

        public static RewardInfo[] ConvertRewardInfo(RewardPacket[] rewards)
        {
            if (rewards == null || rewards.Length == 0)
                return Array.Empty<RewardInfo>();

            // 재화 합치기
            var goodsInfo = rewards.Where(x => x.rewardType != 6 && x.rewardType != 33 && x.rewardValue > 0)
                .GroupBy(x => x.rewardType)
                .Select(group => new RewardInfo(group.Key, group.Sum(x => x.rewardValue), 0))
                .ToList();

            // 아이템 중복 합치기
            var itemInfos = rewards.Where(x => x.rewardType == 6)
                .GroupBy(x => x.rewardValue)
                .Select(group => new RewardInfo(6, group.Key, group.Sum(x => x.rewardCount)));

            // 동행 중복 합치기
            var agentInfos = rewards.Where(x => x.rewardType == 33)
                .GroupBy(x => x.rewardValue)
                .Select(group => new RewardInfo(33, group.Key, group.Sum(x => x.rewardCount)));

            goodsInfo.AddRange(itemInfos);
            goodsInfo.AddRange(agentInfos);

            //var arrayInfo = rewards.Select(x => new RewardInfo(x)).ToArray();
            return goodsInfo.ToArray();
        }

        public static RewardData[] ConvertRewardData(RewardPacket[] rewards)
        {
            return Array.ConvertAll(ConvertRewardInfo(rewards), a => a.data);
        }

        public static void ShowCashShop()
        {
            UI.ShortCut<UIShop>().Set(UIShop.ViewType.Default, ShopTabType.Goods);
        }

        public static void ShowZenyShop()
        {
            UI.ShortCut<UIShop>().Set(UIShop.ViewType.Default, ShopTabType.Goods);
        }

        public static void ShowRoPointShop()
        {
            UI.ShortCut<UIShop>().Set(UIShop.ViewType.Default);
        }
    }
}