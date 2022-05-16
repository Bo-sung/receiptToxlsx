using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public class UIManager : GameObjectSingleton<UIManager>
    {
        private IUIContainer UIContainer;

        public Action OnLocalize { get; set; }

        /// <summary>
        /// UI Show 이벤트
        /// </summary>
        public Action<ICanvas> OnUIShow { get; set; }

        /// <summary>
        /// UI Close 이벤트
        /// </summary>
        public Action<ICanvas> OnUIClose { get; set; }

        /// <summary>
        /// 사용중인 모든 UI 목록
        /// </summary>
        private Dictionary<string, ICanvas> allCanvas = new Dictionary<string, ICanvas>();

        /// <summary>
        /// 백버튼 반응 UI 목록
        /// </summary>
        [SerializeField] private List<UICanvas> backList = new List<UICanvas>();
        [SerializeField] private List<string> backNameList = new List<string>();
        [SerializeField] private Dictionary<string, IUIData> backListData = new Dictionary<string, IUIData>();
        [SerializeField] private Transform mRoot;
        [SerializeField] private UIPanel hudPanel;
        [SerializeField] private UIPanel uiIndicator;
        [SerializeField] private GameObject indcatorBack;
        [SerializeField] private Transform layerUI;
        [SerializeField] private Transform layerChatting;
        [SerializeField] private Transform layerPopup;
        [SerializeField] private Transform layerExceptForCharZoom;
        [SerializeField] private Transform layerEmpty;
        [SerializeField] private Camera cachedCamera;
        [SerializeField] private Camera anchorCamera;
        [SerializeField] private UISafeAreaResizeViewport safeAreaResizeViewport;

        public Transform HUDRoot => hudPanel.cachedTransform;

        public event Action OnResizeSafeArea;
        public Vector3 SafeAreaOffset { get; private set; } = Vector3.zero;
        public float SafeAreaScale { get; private set; } = 1f;

        protected override void Awake()
        {
            base.Awake();

            if (safeAreaResizeViewport)
                safeAreaResizeViewport.OnResizeScreen += OnResizeSafeAreaScreen;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (safeAreaResizeViewport)
                safeAreaResizeViewport.OnResizeScreen -= OnResizeSafeAreaScreen;
        }

        protected override void OnTitle()
        {
            CloseAll(); // 모든 UI 제거
            HideIndicator(); // 인디케이터 숨김
        }

        public UICanvas GetUI(string behaviourName)
        {
            if (allCanvas.TryGetValue(behaviourName, out ICanvas canvas))
            {
                return canvas as UICanvas;
            }
            return null;
        }

        public T GetUI<T>() where T : UICanvas
        {
            return GetUI(typeof(T).Name) as T;
        }

        /// <summary>
        /// UI 생성
        /// 프리팹이름과 클래스이름이 동일해야 한다. 
        /// </summary>
        public void Show<T>(IUIData data, bool isSkipAni) where T : UICanvas
        {
            Show(typeof(T).Name, data, isSkipAni);
        }

        public void Show(string canvasName, IUIData data, bool isSkipAni)
        {
            // 백키로 파괴된 UI 생성시에 사용
            if (backListData.ContainsKey(canvasName))
                backListData.Remove(canvasName);

            backListData.Add(canvasName, data);

            ShowCanvas(canvasName, data, isSkipAni);
        }

        private void ShowCanvas(string behaviourName, IUIData data, bool isSkipAni)
        {
            if (!allCanvas.TryGetValue(behaviourName, out ICanvas canvas))
            {
                GameObject uiPrefab = GetUIPrefab(behaviourName);
                canvas = NGUITools.AddChild(mRoot.gameObject, uiPrefab).GetComponent<ICanvas>();
                if (canvas.layer == Layer.UI)
                {
                    canvas.Transform.SetParent(layerUI, worldPositionStays: false);
                    NGUITools.SetLayer(canvas.Transform.gameObject, Layer.UI);
                }
                else if (canvas.layer == Layer.UI_Chatting)
                {
                    canvas.Transform.SetParent(layerChatting, worldPositionStays: false);
                    NGUITools.SetLayer(canvas.Transform.gameObject, Layer.UI_Chatting);
                }
                else if (canvas.layer == Layer.UI_Popup)
                {
                    canvas.Transform.SetParent(layerPopup, worldPositionStays: false);
                    NGUITools.SetLayer(canvas.Transform.gameObject, Layer.UI_Popup);
                }
                else if (canvas.layer == Layer.UI_ExceptForCharZoom)
                {
                    canvas.Transform.SetParent(layerExceptForCharZoom, worldPositionStays: false);
                    NGUITools.SetLayer(canvas.Transform.gameObject, Layer.UI_ExceptForCharZoom);
                }
                else if (canvas.layer == Layer.UI_Empty)
                {
                    canvas.Transform.SetParent(layerEmpty, worldPositionStays: false);
                    NGUITools.SetLayer(canvas.Transform.gameObject, Layer.UI_Empty);
                }
                canvas.Init();

                allCanvas.Add(behaviourName, canvas);
            }

            if (canvas.HasFlag(UIType.Back))
            {
                if (!backList.Contains(canvas))
                {
                    var cvs = canvas as UICanvas;
                    backList.Add(cvs);
                    backNameList.Add(cvs.CanvasName);
                }
            }

            if (canvas.HasFlag(UIType.Single))
            {
                foreach (var item in allCanvas.Values.ToList())
                {
                    if (canvas.Equals(item)) continue;
                    if (!item.HasFlag(UIType.Single)) continue;
                    if (item.HasFlag(UIType.Fixed)) continue;
                    if (backList.Contains(item))
                    {
                        // Reactivation 타입은 backList에서 제거하지 않음
                        if (!item.HasFlag(UIType.Reactivation))
                        {
                            var itm = item as UICanvas;
                            backList.Remove(itm);
                            backNameList.Remove(itm.CanvasName);
                        }
                    }

                    if (!item.IsVisible) continue;

                    CloseCanvas(item, isSkipAni: true, false);
                }
            }

            // OnShow 에서 바로 Hide 되는 경우 (UI 이용 불가능한 상태) Escape 가 꺼지지 않기 때문에
            // Escape 를 먼저 켜주고 OnShow 호출
            if (canvas.HasFlag(UIType.Single))
                Show<UIEscape>(null, false);

            OnUIShow?.Invoke(canvas);
            canvas.Show(data, isSkipAni);
        }

        /// <summary>
        /// UI 강제로 파괴
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isSkipAni"></param>
        /// <returns></returns>
        public bool Destroy<T>(bool isSkipAni = true) where T : UICanvas
        {
            return Close(typeof(T).Name, isSkipAni: isSkipAni, isDestroy: true);
        }

        /// <summary>
        /// UI 닫기
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public bool Close<T>(bool isSkipAni = false, bool isDestroy = false) where T : UICanvas
        {
            return Close(typeof(T).Name, isSkipAni, isDestroy: isDestroy);
        }

        public bool Close(string behaviourName, bool isSkipAni = false, bool isDestroy = false)
        {
            if (!allCanvas.TryGetValue(behaviourName, out ICanvas canvas))
                return false;

            if (backList.Contains(canvas))
            {
                var cvs = canvas as UICanvas;
                backList.Remove(cvs);
                backNameList.Remove(cvs.CanvasName);
            }

            if (!canvas.IsVisible)
                return false;

            CloseCanvas(canvas, isSkipAni, isDestroy);

            return true;
        }

        private void CloseCanvas(ICanvas canvas, bool isSkipAni, bool isDestroy)
        {
            if (canvas.HasFlag(UIType.Single))
                Close<UIEscape>(false, false);

            OnUIClose?.Invoke(canvas);

            if (isDestroy)
            {
                canvas.Close(isSkipAni);
                allCanvas.Remove(canvas.CanvasName);
                return;
            }

            if (canvas.HasFlag(UIType.Hide))
            {
                canvas.Hide(isSkipAni);
            }
            else if (canvas.HasFlag(UIType.Destroy))
            {
                canvas.Close(isSkipAni);
                allCanvas.Remove(canvas.CanvasName);
            }
        }

        /// <summary>
        /// 모든 UI 삭제
        /// </summary>
        public void CloseAll()
        {
            foreach (var canvas in allCanvas.Values.ToList())
            {
                canvas.Close(true);
            }
            allCanvas.Clear();
            backList.Clear();
            backNameList.Clear();
            backListData.Clear();
        }

        GameObject GetUIPrefab(string assetName)
        {
            if (UIContainer == null)
            {
                UIContainer = AssetManager.Instance;
            }

            GameObject go = UIContainer.GetUI(assetName);

            return go;
        }

        public void Localize()
        {
            UIRoot.Broadcast(nameof(OnLocalize));
            OnLocalize?.Invoke();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Escape())
                    return;

                var entry = BattleManager.Instance.GetCurrentEntry();

                if (entry == null)
                    UI.ShowExitPopup();
                else if (entry.IsAllReady)
                    entry.OnBack();
            }            
        }

        public bool Escape()
        {
            if (Tutorial.isInProgress)
            {
                UI.ShowToastPopup(LocalizeKey._26030.ToText()); // 튜토리얼 중에는 이용할 수 없습니다.
                return true;
            }

            if (backList.Count > 0)
            {
                backList[backList.Count - 1].Back();

                // 리스트의 마지막 값이 Reactivation 타입일 때, Hide 타입은 Show
                if (backList.Count > 0)
                {
                    var ui = backList[backList.Count - 1];

                    if (ui == null)
                    {
                        var name = backNameList[backNameList.Count - 1];

                        // UI가 Destroy타입일 경우 삭제하고 새로 생성
                        backList.RemoveAt(backList.Count - 1);
                        backNameList.RemoveAt(backNameList.Count - 1);

                        ShowCanvas(name, backListData[name], false);
                    }
                    else
                    {
                        if (backList[backList.Count - 1].HasFlag(UIType.Reactivation))
                        {
                            if (backList[backList.Count - 1].HasFlag(UIType.Hide))
                            {
                                backList[backList.Count - 1].Show();
                            }

                            Show<UIEscape>(null, false);
                        }
                    }
                }
                return true;
            }

            return false;
        }

        #region HUD

        [Obsolete("Use 'UI.ShowHUD' instead")]
        public void ShowHUD()
        {
            SetActiveHUD(true);
        }

        [Obsolete("Use 'UI.HideHUD' instead")]
        public void HideHUD()
        {
            SetActiveHUD(false);
        }

        public void SetActiveHUD(bool isActive)
        {
            hudPanel.alpha = isActive ? 1f : 0f;
        }

        #endregion

        #region Indicator

        public void ShowIndicator(float delay)
        {
            Debug.Log($"<color=#FFFF64>ShowIndicator</color>");
            Timing.KillCoroutines(gameObject);
            indcatorBack.SetActive(false);
            uiIndicator.alpha = 1f;
            Timing.RunCoroutine(YieldShowIndicator(), gameObject);

            if (delay > 0f)
                Timing.RunCoroutine(YieldDelayToIntro(delay), Segment.Update, gameObject);
        }

        public void HideIndicator()
        {
            Debug.Log($"<color=#64D2D2>HideIndicator</color>");
            Timing.KillCoroutines(gameObject);
            indcatorBack.SetActive(false);
            Timing.RunCoroutine(YieldHideIndicator(), gameObject);
        }

        /// <summary>
        /// 확성기 수신 켜기
        /// </summary>
        public void ShowLoudSpeaker()
        {
            UI.Show<UILoudSpeakerView>();
        }

        /// <summary>
        /// 확성기 수신 끄기
        /// </summary>
        public void HideLoudSpeaker()
        {
            UI.Close<UILoudSpeakerView>();
        }

        private IEnumerator<float> YieldShowIndicator()
        {
            yield return Timing.WaitForSeconds(1f);
            indcatorBack.SetActive(true);
        }

        private IEnumerator<float> YieldHideIndicator()
        {
            yield return Timing.WaitForOneFrame;
            uiIndicator.alpha = 0f;
        }

        /// <summary>
        /// 일정시간 딜레이 후 타이틀 화면으로 이동
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> YieldDelayToIntro(float delay)
        {
            yield return Timing.WaitForSeconds(delay);

#if UNITY_EDITOR
            Debug.LogError($"인디케이더 딜레이 초과 = {delay}");
#endif
            if (!Application.isEditor)
            {
                ShowToIntroPopup();
            }
        }

        async void ShowToIntroPopup()
        {
            Tutorial.Abort(); // 튜토리얼 강제 중지

            await UI.ConfirmPopupAsync(LocalizeKey._524.ToText()); // 네트워크 연결이 원활하지 않습니다.\n타이틀로 돌아갑니다.
            SceneLoader.LoadIntro(); // 타이틀 화면으로 이동
            UI.HideIndicator();
        }

#endregion

        /// <summary>
        /// 바로가기
        /// </summary>
        public void ShortCut(bool forceClose = false)
        {
            // 메인UI 제외하고 전부 끄거나 삭제
            foreach (var canvas in allCanvas.Values.ToList())
            {
                if (canvas.HasFlag(UIType.Fixed))
                    continue;

                if (backList.Contains(canvas))
                {
                    var cvs = canvas as UICanvas;
                    backList.Remove(cvs);
                    backNameList.Remove(cvs.CanvasName);
                }

                if (!canvas.IsVisible)
                    continue;

                CloseCanvas(canvas, isSkipAni: true, forceClose);
            }
            backList.Clear();
            backNameList.Clear();
            backListData.Clear();
        }

        void OnResizeSafeAreaScreen(Vector3 offset, float scale)
        {
            layerUI.localPosition = offset;
            layerChatting.localPosition = offset;
            layerPopup.localPosition = offset;
            layerExceptForCharZoom.localPosition = offset;
            layerEmpty.localPosition = offset;

            Vector3 localScale = Vector3.one * scale;
            layerUI.localScale = localScale;
            layerChatting.localScale = localScale;
            layerPopup.localScale = localScale;
            layerExceptForCharZoom.localScale = localScale;
            layerEmpty.localScale = localScale;

            SafeAreaOffset = offset;
            SafeAreaScale = scale;
            OnResizeSafeArea?.Invoke();
        }

        [Obsolete("Use 'UI.AddMask' instead")]
        public void AddMask(params int[] layers)
        {
            foreach (var layer in layers)
            {
                cachedCamera.cullingMask |= 1 << layer;
            }
        }

        [Obsolete("Use 'UI.RemoveMask' instead")]
        public void RemoveMask(params int[] layers)
        {
            foreach (var layer in layers)
            {
                cachedCamera.cullingMask &= ~(1 << layer);
            }
        }

        [Obsolete("Use 'UI.CurrentCamera' instead")]
        public Camera CurrentCamera => cachedCamera;

        [Obsolete("Use 'UI.AnchorCamera' instead")]
        public Camera AnchorCamera => anchorCamera;
    }
}