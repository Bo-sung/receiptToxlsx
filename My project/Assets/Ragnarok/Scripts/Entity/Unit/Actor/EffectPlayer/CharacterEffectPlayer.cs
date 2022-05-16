using UnityEngine;

namespace Ragnarok
{
    public class CharacterEffectPlayer : UnitEffectPlayer
    {
        protected const float CONTROLLER_ASSIST_SPEED = 20f;

        private GameGraphicSettings graphicSettings;
        protected CharacterEntity entity;

        protected HudCharacterName hudName;
        protected PoolObject controllerAssist;
        protected UIController uiController;
        protected HudCharacterInfo hudCharacterInfo;
        protected LobbyPrivateStoreBalloon storeBalloon;

        protected override bool IsCharacter => true;

        /// <summary>
        /// 공격 시 카메라 흔들림 사용 여부
        /// </summary>
        protected bool hasAttackImpulse = false;

        protected override void Awake()
        {
            base.Awake();

            graphicSettings = GameGraphicSettings.Instance;
        }

        public override void OnReady(UnitEntity entity)
        {
            base.OnReady(entity);

            this.entity = entity as CharacterEntity;
        }

        public override void OnRelease()
        {
            base.OnRelease();

            if (hudName)
            {
                hudName.OnUnitInfo = null;
                hudName.Release();
                hudName = null;
            }

            if (controllerAssist)
            {
                RemoveControllerAssist();
                controllerAssist.Release();
                controllerAssist = null;
            }

            if (hudCharacterInfo)
            {
                RemoveUnitInfo();
                hudCharacterInfo.Release();
                hudCharacterInfo = null;
            }

            if (storeBalloon)
            {
                RemoveStoreBalloon();
                storeBalloon.Release();
                storeBalloon = null;
            }
        }

        public override void AddEvent()
        {
            base.AddEvent();

            entity.Character.OnUpdateLevel += OnUpdateLevel; // 레벨 이벤트
            entity.Character.OnUpdateJobLevel += OnUpdateJobLevel; // 직업레벨 이벤트
            entity.Character.OnChangedName += OnChangedName; // 이름 변경

            if (entity.Guild)
            {
                entity.Guild.OnUpdateGuildState += OnUpdateGuildState; // 길드 상태 변경
            }

            if (entity.Trade)
            {
                entity.Trade.OnUpdatePrivateStoreState += OnUpdatePrivateStoreState;
            }

            if (entity.Inventory)
            {
                entity.Inventory.OnUpdateItem += OnChangeCostume;
                entity.Inventory.OnChangeCostume += OnChangeCostume;
            }

            graphicSettings.OnShadowState += OnShadowState;
        }

        public override void RemoveEvent()
        {
            base.RemoveEvent();

            entity.Character.OnUpdateLevel -= OnUpdateLevel; // 레벨 이벤트
            entity.Character.OnUpdateJobLevel -= OnUpdateJobLevel; // 직업레벨 이벤트
            entity.Character.OnChangedName -= OnChangedName; // 이름 변경

            if (entity.Guild)
            {
                entity.Guild.OnUpdateGuildState -= OnUpdateGuildState; // 길드 상태 변경
            }

            if (entity.Trade)
            {
                entity.Trade.OnUpdatePrivateStoreState -= OnUpdatePrivateStoreState;
            }

            if (entity.Inventory)
            {
                entity.Inventory.OnUpdateItem -= OnChangeCostume;
                entity.Inventory.OnChangeCostume -= OnChangeCostume;
            }

            graphicSettings.OnShadowState -= OnShadowState;
        }

        void OnShadowState()
        {
            isNonSkillEffectState = IsNonSkillEffectCharacterState();
        }

        protected override void OnAttacked(UnitEntity target, SkillType skillType, int skillId, ElementType elementType, bool hasDamage, bool isChainableSkill)
        {
            base.OnAttacked(target, skillType, skillId, elementType, hasDamage, isChainableSkill);

            if (isChainableSkill)
                return;

            if (hasDamage && hasAttackImpulse)
                GenerateImpulse(BattlePoolManager.ImpulseType.NormalAttack); // 일반 공격 충격파
        }

        /// <summary>
        /// 공격 시 카메라흔들림 사용 여부 설정
        /// </summary>
        public void SetAttackImpulse(bool isActive)
        {
            this.hasAttackImpulse = isActive;
        }

        /// <summary>
        /// 레벨업 이펙트 출력
        /// </summary>
        private void OnUpdateLevel(int dummy)
        {
            PlayLevelUpEffect();
        }

        /// <summary>
        /// 전직 이펙트 출력
        /// </summary>
        private void OnUpdateJobLevel(int dummy)
        {
            PlayJobLevelUpEffect();
            ShowName(); // 이름에 직업 레벨 표시
        }

        void OnChangedName(string name)
        {
            ShowName();
        }

        void OnUpdateGuildState()
        {
            ShowName();
        }

        void OnChangeCostume()
        {
            ShowName();
        }

        protected virtual bool IsNonSkillEffectCharacterState()
        {
            if (entity == null)
                return false;

            if (entity.type != UnitEntityType.MultiPlayer)
                return false;

            return graphicSettings.CurrentShadowState == ShadowMultiPlayerQuality.Shadow;
        }

        public override void ShowName()
        {
            if (hudName == null)
                hudName = hudPool.SpawnCharacterName(CachedTransform);

            hudName.Initialize(entity.GetName(), entity.Character.JobLevel, entity.type);
            hudName.Initialize(GetGuildName(), GetTitleName());
        }

        protected string GetGuildName()
        {
            if (entity.Guild == null)
                return string.Empty;

            if (entity.Guild.HaveGuild)
                return string.Empty;

            string guildName = entity.Guild.GuildName;
            if (string.IsNullOrEmpty(guildName))
                return string.Empty;

            return StringBuilderPool.Get()
                .Append("[")
                .Append(guildName)
                .Append("]")
                .Release();
        }

        protected string GetTitleName()
        {
            string titleName = entity.GetTitleName();

            if (string.IsNullOrEmpty(titleName))
                return string.Empty;

            return titleName;
        }

        public void ShowControllerAssist(UIController uiController)
        {
            RemoveControllerAssist();  // 이전 컨트롤러 이벤트 제거

            if (controllerAssist is null)
            {
                controllerAssist = battlePool.SpawnControllerAssist(actor.CachedTransform);
            }

            this.uiController = uiController;
            this.uiController.OnStart += OnControllerStart;
            this.uiController.OnDrag += OnControllerDrag;
            this.uiController.OnReset += OnControllerReset;

            controllerAssist.Hide();
        }

        protected void OnControllerStart()
        {
            if (actor.Entity.IsDie)
            {
                controllerAssist.Hide();
                return;
            }

            controllerAssist.Show();
            controllerAssist.CachedTransform.localPosition = Vector3.zero;
        }

        protected void OnControllerDrag(Vector2 pos)
        {
            bool isDie = (actor.Entity.IsDie);
            bool isZero = (pos == Vector2.zero);

            if (isDie || isZero)
            {
                controllerAssist.Hide();
                return;
            }

            controllerAssist.CachedTransform.localPosition = Vector3.forward * uiController.GetDragDistance() * CONTROLLER_ASSIST_SPEED;
        }

        protected void OnControllerReset()
        {
            controllerAssist.Hide();
        }

        protected void RemoveControllerAssist()
        {
            if (controllerAssist is null || uiController is null)
                return;

            this.uiController.OnStart -= OnControllerStart;
            this.uiController.OnDrag -= OnControllerDrag;
            this.uiController.OnReset -= OnControllerReset;
        }

        public override void SetUnitInfo()
        {
            RemoveUnitInfo();

            if (hudCharacterInfo is null)
            {
                hudCharacterInfo = hudPool.SpawnCharacterInfo(actor.CachedTransform);
            }

            hudCharacterInfo.OnSelect += OnSelectUnitInfo;
            hudCharacterInfo.OnChat += OnChatUnitInfo;

            hudCharacterInfo.Hide();
        }

        void OnSelectUnitInfo()
        {
            OnInfo?.Invoke(actor);
        }

        void OnChatUnitInfo()
        {
            OnChat?.Invoke(actor);
        }

        protected void RemoveUnitInfo()
        {
            if (hudCharacterInfo is null)
                return;

            hudCharacterInfo.OnSelect -= OnSelectUnitInfo;
            hudCharacterInfo.OnChat -= OnChatUnitInfo;
        }

        public override void ShowBattleHUD()
        {
            if (hudCharacterInfo is null)
                return;

            hudCharacterInfo.Show();
        }

        public override void HideBattleHUD()
        {
            if (hudCharacterInfo is null)
                return;

            hudCharacterInfo.Hide();
        }

        public override void SetStoreBalloon()
        {
            if (entity.Trade == null)
                return;

            RemoveStoreBalloon();

            if (storeBalloon is null)
            {
                storeBalloon = hudPool.SpawnLobbyPrivateStoreBalloon(actor.CachedTransform);
            }

            storeBalloon.OnSelect += OnSelectStoreBalloon;

            if (entity.Trade.SellingState == PrivateStoreSellingState.SELLING)
            {
                storeBalloon.SetComment(entity.Trade.StallName);
                storeBalloon.Show();
            }
            else
            {
                storeBalloon.Hide();
            }
        }

        void OnSelectStoreBalloon()
        {
            OnStore?.Invoke(actor);
        }

        void RemoveStoreBalloon()
        {
            if (storeBalloon is null)
                return;

            storeBalloon.OnSelect -= OnSelectStoreBalloon;
        }

        protected virtual void OnUpdatePrivateStoreState()
        {
            if (entity.Trade == null)
                return;

            if (storeBalloon is null)
                return;

            if (entity.Trade.SellingState == PrivateStoreSellingState.SELLING)
            {
                storeBalloon.SetComment(entity.Trade.StallName);
                storeBalloon.Show();
            }
            else
            {
                storeBalloon.Hide();
            }
        }

        public override void ShowRebirthEffect()
        {
            battlePool.SpawnRebirth(CachedTransform.position);
        }
    }
}