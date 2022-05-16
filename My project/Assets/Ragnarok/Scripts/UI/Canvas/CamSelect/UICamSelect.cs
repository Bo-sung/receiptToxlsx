using UnityEngine;

namespace Ragnarok
{
    public sealed class UICamSelect : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIButtonHelper btnCamera;
        [SerializeField] BattleCameraSettings[] cameraSettings;

        CameraController cameraController;
        BattleManager battleManager;
        bool isDifficulty;

        protected override void OnInit()
        {
            cameraController = CameraController.Instance;
            battleManager = BattleManager.Instance;

            if (cameraSettings != null)
            {
                foreach (var item in cameraSettings)
                {
                    item.Initialize();
                }
            }

            EventDelegate.Add(btnCamera.OnClick, OnClickedBtnCamera);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnCamera.OnClick, OnClickedBtnCamera);
        }

        protected override void OnShow(IUIData data = null)
        {
            SetDifficulty(false);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        void OnClickedBtnCamera()
        {
            BattleCameraSettings battleCameraSettings = Find(battleManager.Mode);
            if (battleCameraSettings == null)
                return;

            battleCameraSettings.Next();
            Refresh();
        }

        private void Refresh()
        {
            BattleCameraSettings battleCameraSettings = Find(battleManager.Mode);
            if (battleCameraSettings == null)
            {
                btnCamera.Text = "Cam 0";
                cameraController.HideCustomView();
                return;
            }

            CameraSettings settings = battleCameraSettings.GetCameraSettings();
            if (settings == null)
            {
                btnCamera.Text = "Cam 0";
                cameraController.HideCustomView();
                return;
            }

            btnCamera.Text = settings.name;

            if (settings.isDefault)
            {
                cameraController.HideCustomView();
            }
            else
            {
                cameraController.ShowCustomView();
                cameraController.SetCustomViewRotateX(settings.angleX);
                cameraController.SetCustomViewRotateY(settings.angleY);
                cameraController.SetCustomViewDistance(settings.cameraDistance);
            }
        }

        public void SetDifficulty(bool isDifficulty)
        {
            this.isDifficulty = isDifficulty;
            Refresh();
        }

        private BattleCameraSettings Find(BattleMode mode)
        {
            if (cameraSettings == null)
                return null;

            for (int i = 0; i < cameraSettings.Length; i++)
            {
                if (cameraSettings[i].battleMode == mode && cameraSettings[i].isDifficulty == isDifficulty)
                    return cameraSettings[i];
            }

            return null;
        }

        public override bool Find()
        {
            base.Find();

#if UNITY_EDITOR
            if (cameraSettings == null)
                cameraSettings = new BattleCameraSettings[0];

            if (cameraSettings.Length == 0)
            {
                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.Stage, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = true },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 43 },
                    new CameraSettings { name = "Cam 3", isDefault = false, angleX = 35, angleY = -135, cameraDistance = 45 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.ScenarioMaze, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = false, angleX = 35, angleY = -135, cameraDistance = 43 },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 48 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.MultiMazeLobby, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 43 },
                    new CameraSettings { name = "Cam 2", isDefault = true }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.MultiMaze, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = false, angleX = 35, angleY = -135, cameraDistance = 43 },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 43 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.MultiMaze, isDifficulty: true,
                    new CameraSettings { name = "Cam 1", isDefault = false, angleX = 35, angleY = -135, cameraDistance = 35 },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 40 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.Lobby, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 43 },
                    new CameraSettings { name = "Cam 2", isDefault = true }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.ClickerDungeon, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = true },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 43 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.WorldBoss, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = true },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -135, cameraDistance = 45 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.Defence, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = true },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -135, cameraDistance = 45 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.CentralLab, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = true },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 43 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.SpecialDungeon, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = true },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 43 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.TamingMaze, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = true },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 43 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.MatchMultiMaze, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = false, angleX = 35, angleY = -135, cameraDistance = 35 },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 40 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.GuildLobby, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 43 },
                    new CameraSettings { name = "Cam 2", isDefault = true }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.GuildAttack, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 43 },
                    new CameraSettings { name = "Cam 2", isDefault = true }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.ChristmasMatchMultiMaze, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = false, angleX = 35, angleY = -135, cameraDistance = 35 },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 40 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.EndlessTower, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = true }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.ForestMaze, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 43 },
                    new CameraSettings { name = "Cam 2", isDefault = true }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.TimePatrol, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = true },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 43 },
                    new CameraSettings { name = "Cam 3", isDefault = false, angleX = 35, angleY = -135, cameraDistance = 45 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.DarkMaze, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = false, angleX = 35, angleY = -135, cameraDistance = 35 },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 40 }));

                UnityEditor.ArrayUtility.Add(ref cameraSettings, new BattleCameraSettings(BattleMode.GateMaze, isDifficulty: false,
                    new CameraSettings { name = "Cam 1", isDefault = false, angleX = 35, angleY = -135, cameraDistance = 35 },
                    new CameraSettings { name = "Cam 2", isDefault = false, angleX = 30, angleY = -90, cameraDistance = 40 }));

                NGUITools.SetDirty(this);
            }
#endif
            return true;
        }

        [System.Serializable]
        private class BattleCameraSettings
        {
            private const string CAMERA_INDEX_KEY = "CAMERA_INDEX";

            public BattleMode battleMode;
            public bool isDifficulty;
            public CameraSettings[] settings;
            private int cameraIndex;

            public BattleCameraSettings(BattleMode battleMode, bool isDifficulty, params CameraSettings[] settings)
            {
                this.battleMode = battleMode;
                this.isDifficulty = isDifficulty;
                this.settings = settings;
            }

            public void Initialize()
            {
                cameraIndex = LoadCameraIndex();
                CheckValidCameraIndex();
            }

            public CameraSettings GetCameraSettings()
            {
                if (cameraIndex < 0 || cameraIndex >= settings.Length)
                    return null;

                return settings[cameraIndex];
            }

            public void Next()
            {
                ++cameraIndex;
                CheckValidCameraIndex();
                SaveCameraIndex();
            }

            private void CheckValidCameraIndex()
            {
                if (cameraIndex >= settings.Length)
                    cameraIndex = 0;
            }

            private int LoadCameraIndex()
            {
                string key = string.Concat(CAMERA_INDEX_KEY, "-", battleMode.ToString());
                return PlayerPrefs.GetInt(key, 0);
            }

            private void SaveCameraIndex()
            {
                string key = string.Concat(CAMERA_INDEX_KEY, "-", battleMode.ToString());
                PlayerPrefs.SetInt(key, cameraIndex);
            }
        }

        [System.Serializable]
        private class CameraSettings
        {
            public string name;
            public bool isDefault;
            public int angleX;
            public int angleY;
            public int cameraDistance;
        }
    }
}