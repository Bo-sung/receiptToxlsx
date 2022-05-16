using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <see cref="ContentAbility"/>
	public sealed class ContentAbilityPresenter : ViewPresenter
    {
        public interface IView
        {
            void ShowStatus(CharacterEntity player, CharacterEntity preview);
            void ShowStatusPoint(int point);
            void SetAutoStat(bool isAutoStat);

            UIWidget GetStatusArea();
            UIButtonHelper GetBtnStrUpgrade();
            bool IsClickedBtnStrLevelUp();
            UIButtonHelper GetBtnApplyStatus();
        }

        private readonly IView view;
        private CharacterEntity player;
        private ClonePlayerEntity preview;
        private CharacterModel characterModel;
        private StatusModel statusModel;

        public Action<int, int> OnChangeAP;

        // 사용한 스탯 포인트(프리뷰용)
        public int UseStatPoint
        {
            get
            {
                int total = 0;
                total += (preview.Status.BasicStr - player.Status.BasicStr);
                total += (preview.Status.BasicAgi - player.Status.BasicAgi);
                total += (preview.Status.BasicVit - player.Status.BasicVit);
                total += (preview.Status.BasicInt - player.Status.BasicInt);
                total += (preview.Status.BasicDex - player.Status.BasicDex);
                total += (preview.Status.BasicLuk - player.Status.BasicLuk);
                return total;
            }
        }

        // 남은 스탯 포인트(프리뷰용)
        public int RemainStatPoint
        {
            get
            {
                if (player.Status.StatPoint > preview.Status.StatPoint)
                {
                    return player.Status.StatPoint - UseStatPoint;
                }
                return preview.Status.StatPoint - UseStatPoint;
            }
        }

        // 사용한 스탯 포인트 있는가
        public bool HaveUseStatPoint
        {
            get
            {

                return (6 < player.Status.BasicStr +
                            player.Status.BasicAgi +
                            player.Status.BasicVit +
                            player.Status.BasicInt +
                            player.Status.BasicDex +
                            player.Status.BasicLuk);
            }
        }

        public bool IsPreview;

        public ContentAbilityPresenter(IView view)
        {
            this.view = view;
        }

        public CharacterEntity GetPlayer() => player;

        public CharacterEntity GetPreview()
        {
            return preview;
        }

        public override void AddEvent()
        {

        }

        public override void RemoveEvent()
        {
            ClearPlayer();
        }

        public void SetPlayer(CharacterEntity charaEntity)
        {
            ClearPlayer(); // 기존 플레이어 제거
            player = charaEntity;

            characterModel = player.Character;
            statusModel = player.Status;

            if (player.type == UnitEntityType.Player)
            {
                player.OnReloadStatus += OnReloadStatus;
                statusModel.OnUpdateStatPoint += ShowStatusPoint;
                characterModel.OnRebirth += OnRebirth;
                statusModel.OnAutoStat += SetAutoStat;
            }

            InitClonePlayer();
            ShowStatus();
        }

        private void ClearPlayer()
        {
            if (player == null)
                return;

            if (player.type == UnitEntityType.Player)
            {
                player.OnReloadStatus -= OnReloadStatus;
                statusModel.OnUpdateStatPoint -= ShowStatusPoint;
                characterModel.OnRebirth -= OnRebirth;
                statusModel.OnAutoStat -= SetAutoStat;
            }

            player = null;
        }

        void OnRebirth()
        {
            CancelPreviewStatPoint();
        }

        private void InitClonePlayer()
        {
            if (preview == null)
                preview = Entity.Factory.CreateClonePlayer();

            preview.Initialize(player, IsPreview);
        }

        public void ReleaseClonePlayer()
        {
            IsPreview = false;

            if (preview != null)
            {
                preview.Release();
                preview = null;
            }
        }

        public void AddStr(short add)
        {
            if (RemainStatPoint == 0) return;

            if (add > RemainStatPoint)
                add = (short)RemainStatPoint;

            if (add > player.Status.MaxStatus(BasicStatusType.Str) - preview.Status.BasicStr)
                add = (short)(player.Status.MaxStatus(BasicStatusType.Str) - preview.Status.BasicStr);

            IsPreview = true;
            preview.Status.AddStatusStr(add);
            ShowStatus();
            ShowStatusPoint();
        }

        public void AddAgi(short add)
        {
            if (RemainStatPoint == 0) return;

            if (add > RemainStatPoint)
                add = (short)RemainStatPoint;

            if (add > player.Status.MaxStatus(BasicStatusType.Agi) - preview.Status.BasicAgi)
                add = (short)(player.Status.MaxStatus(BasicStatusType.Agi) - preview.Status.BasicAgi);

            IsPreview = true;
            preview.Status.AddStatusAgi(add);
            ShowStatus();
            ShowStatusPoint();
        }

        public void AddVit(short add)
        {
            if (RemainStatPoint == 0) return;

            if (add > RemainStatPoint)
                add = (short)RemainStatPoint;

            if (add > player.Status.MaxStatus(BasicStatusType.Vit) - preview.Status.BasicVit)
                add = (short)(player.Status.MaxStatus(BasicStatusType.Vit) - preview.Status.BasicVit);

            IsPreview = true;
            preview.Status.AddStatusVit(add);
            ShowStatus();
            ShowStatusPoint();
        }

        public void AddInt(short add)
        {
            if (RemainStatPoint == 0) return;

            if (add > RemainStatPoint)
                add = (short)RemainStatPoint;

            if (add > player.Status.MaxStatus(BasicStatusType.Int) - preview.Status.BasicInt)
                add = (short)(player.Status.MaxStatus(BasicStatusType.Int) - preview.Status.BasicInt);

            IsPreview = true;
            preview.Status.AddStatusInt(add);
            ShowStatus();
            ShowStatusPoint();
        }

        public void AddDex(short add)
        {
            if (RemainStatPoint == 0) return;

            if (add > RemainStatPoint)
                add = (short)RemainStatPoint;

            if (add > player.Status.MaxStatus(BasicStatusType.Dex) - preview.Status.BasicDex)
                add = (short)(player.Status.MaxStatus(BasicStatusType.Dex) - preview.Status.BasicDex);

            IsPreview = true;
            preview.Status.AddStatusDex(add);
            ShowStatus();
            ShowStatusPoint();
        }

        public void AddLuk(short add)
        {
            if (RemainStatPoint == 0) return;

            if (add > RemainStatPoint)
                add = (short)RemainStatPoint;

            if (add > player.Status.MaxStatus(BasicStatusType.Luk) - preview.Status.BasicLuk)
                add = (short)(player.Status.MaxStatus(BasicStatusType.Luk) - preview.Status.BasicLuk);

            IsPreview = true;
            preview.Status.AddStatusLuk(add);
            ShowStatus();
            ShowStatusPoint();
        }

        public void AddMaxStat(BasicStatusType type, out CharacterEntity player, out CharacterEntity preview)
        {
            switch (type)
            {
                case BasicStatusType.Str:
                    this.preview.Status.AddStatusStr(1);
                    break;

                case BasicStatusType.Agi:
                    this.preview.Status.AddStatusAgi(1);
                    break;

                case BasicStatusType.Vit:
                    this.preview.Status.AddStatusVit(1);
                    break;

                case BasicStatusType.Int:
                    this.preview.Status.AddStatusInt(1);
                    break;

                case BasicStatusType.Dex:
                    this.preview.Status.AddStatusDex(1);
                    break;

                case BasicStatusType.Luk:
                    this.preview.Status.AddStatusLuk(1);
                    break;
            }

            this.preview.ReloadStatus();

            player = this.player;
            preview = this.preview;
        }

        public void CancelPreviewStatPoint()
        {
            IsPreview = false;
            InitClonePlayer();
            ShowStatus();
            ShowStatusPoint();
        }

        public void ShowStatus()
        {
            InitClonePlayer();

            preview.ReloadStatus();

            view.ShowStatus(player, preview);
        }

        public void ShowStatusPoint()
        {
            InitClonePlayer();

            view.ShowStatusPoint(RemainStatPoint);
        }

        /// <summary>
        /// 캐릭터 스탯포인트 업데이트
        /// </summary>
        public async void RequestStatPointUpdate()
        {
            short str = (short)(preview.Status.BasicStr - player.Status.BasicStr);
            short agi = (short)(preview.Status.BasicAgi - player.Status.BasicAgi);
            short vit = (short)(preview.Status.BasicVit - player.Status.BasicVit);
            short inte = (short)(preview.Status.BasicInt - player.Status.BasicInt);
            short dex = (short)(preview.Status.BasicDex - player.Status.BasicDex);
            short lux = (short)(preview.Status.BasicLuk - player.Status.BasicLuk);

            // 찍은 스탯 있는지 체크
            int total = str + agi + vit + inte + dex + lux;
            if (total == 0) return;

            IsPreview = false;
            bool isSuccess = await statusModel.RequestCharStatPointUpdate(str, agi, vit, inte, dex, lux);
            if (isSuccess)
            {
                InitClonePlayer();
                ShowStatus();
            }
        }

        /// <summary>
        /// 캐릭터 스탯 포인트 초기화
        /// </summary>
        public async Task RequestCharStatPointInit()
        {
            // 1이라도 스탯을 찍었는지 체크
            if (!HaveUseStatPoint)
            {
                Debug.LogError("사용한 스탯 포인트가 없음");
                return;
            }

            IsPreview = false;
            bool isSuccess = await statusModel.RequestCharStatPointInit();
            if (!isSuccess)
                return;

            InitClonePlayer();
            ShowStatus();
        }

        /// <summary>자동 분배</summary>
        public void AutoStat()
        {
            if (RemainStatPoint == 0)
                return;

            InitClonePlayer();

            short[] addStat = player.Status.GetAutoStatGuidePoints(RemainStatPoint);

            IsPreview = true;
            preview.Status.AddStatusStr(addStat[0]);
            preview.Status.AddStatusAgi(addStat[1]);
            preview.Status.AddStatusVit(addStat[2]);
            preview.Status.AddStatusInt(addStat[3]);
            preview.Status.AddStatusDex(addStat[4]);
            preview.Status.AddStatusLuk(addStat[5]);
            preview.ReloadStatus();
            ShowStatus();
            ShowStatusPoint();
        }

        private void OnReloadStatus()
        {
            ShowStatus();
        }

        public void RequestAutoStat()
        {
            statusModel.RequestAutoStat(!statusModel.IsAutoStat).WrapNetworkErrors();
        }

        public void SetAutoStat()
        {
            view.SetAutoStat(statusModel.IsAutoStat);
        }
    }
}