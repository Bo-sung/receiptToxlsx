using UnityEngine;

namespace Ragnarok
{
    public class CharacterActor : UnitActor
    {
        private GameGraphicSettings graphicSettings;

        protected CharacterEntity characterEntity;

        CharacterAI ai; // 인공지능 관리
        protected CharacterAnimator animator; // 애니메이션 관리
        protected CharacterAppearance appearance; // 외형 관리
        ObjectPicking picking; // 터치 관리

        private bool isShadowCharacterState;
        private bool isNoneWeapon;

        protected override void Awake()
        {
            base.Awake();

            graphicSettings = GameGraphicSettings.Instance;
        }

        public override void OnCreate(IPooledDespawner despawner, string poolID)
        {
            base.OnCreate(despawner, poolID);

            ai = AI as CharacterAI;
            animator = Animator as CharacterAnimator;
            appearance = Appearance as CharacterAppearance;

            picking = GetComponent<ObjectPicking>();
            picking.SetTargetLayer(Layer.CUBE, Layer.ENEMY); // Cube를 포함한 TargetLayer
        }

        public override void OnReady(UnitEntity entity)
        {
            base.OnReady(entity);

            characterEntity = entity as CharacterEntity;

            picking.enabled = IsPickingEnable(); // Picking 설정
            UpdateStyle();
        }

        protected virtual bool IsPickingEnable()
        {
            return false;
        }

        public override void AddEvent()
        {
            base.AddEvent();

            characterEntity.Character.OnChangedJob += ChangeJob; // 전직 이벤트
            characterEntity.Character.OnChangedGender += UpdateStyle; // 성별 변경

            // 더미 캐릭터일 경우에는 스타일이 바뀔 수 있음
            if (characterEntity.Character is DummyCharacterModel)
            {
                DummyCharacterModel characterModel = characterEntity.Character as DummyCharacterModel;
                characterModel.OnChangedStyle += SetStyle;
                characterModel.OnChangedWeapon += UpdateCostumeWihtWeapon;
                characterModel.OnChangeCostumeEvent += UpdateCostumeWihtWeapon;
            }

            if (characterEntity.Inventory != null)
            {
                characterEntity.Inventory.OnUpdateItem += UpdateCostumeWihtWeapon;
                characterEntity.Inventory.OnChangeCostume += UpdateCostumeWihtWeapon;
            }

            if (characterEntity.Trade != null)
            {
                characterEntity.Trade.OnUpdatePrivateStoreState += UpdatePrivateStoreSelling;
            }

            graphicSettings.OnShadowState += OnShadowState;
        }

        public override void RemoveEvent()
        {
            base.RemoveEvent();

            characterEntity.Character.OnChangedJob -= ChangeJob; // 전직 이벤트
            characterEntity.Character.OnChangedGender -= UpdateStyle; // 성별 변경

            // 더미 캐릭터일 경우에는 스타일이 바뀔 수 있음
            if (characterEntity.Character is DummyCharacterModel)
            {
                DummyCharacterModel characterModel = characterEntity.Character as DummyCharacterModel;
                characterModel.OnChangedStyle -= SetStyle;
                characterModel.OnChangedWeapon -= UpdateCostumeWihtWeapon;
                characterModel.OnChangeCostumeEvent -= UpdateCostumeWihtWeapon;
            }

            if (characterEntity.Inventory != null)
            {
                characterEntity.Inventory.OnUpdateItem -= UpdateCostumeWihtWeapon;
                characterEntity.Inventory.OnChangeCostume -= UpdateCostumeWihtWeapon;
            }

            if (characterEntity.Trade != null)
            {
                characterEntity.Trade.OnUpdatePrivateStoreState -= UpdatePrivateStoreSelling;
            }

            graphicSettings.OnShadowState -= OnShadowState;
        }

        void OnShadowState()
        {
            if (isShadowCharacterState == IsShadowCharacterState())
                return;

            appearance.Release(); // 기존 외형 제거
            UpdateStyle();
        }

        /// <summary>
        /// 외형 변경
        /// </summary>
        public void SetStyle(Gender gender, Job job)
        {
            bool isUI = characterEntity.type == UnitEntityType.UI; // UI 용이 아닐때 아이템 이펙트 체크
            appearance.SetIsUI(isUI);

            bool isPrivateStoreSelling = characterEntity.Trade ? characterEntity.Trade.SellingState == PrivateStoreSellingState.SELLING : false;
            isShadowCharacterState = IsShadowCharacterState();
            appearance.Initialize(job, gender, isPrivateStoreSelling, isShadowCharacterState);

            UpdateCostumeWihtWeapon(); // 머리, 얼굴, 망토 코스튬 세팅 후 무기(무기코스튬 세팅)

            // 주의: 외형 변경 후 Animator 처리한다
            // (외형이 변경되면 GameObject의 Animation이 변경되기 때문에)
            animator.SetGender(gender);

            if (isPrivateStoreSelling)
                LookFront();
        }

        protected virtual bool IsShadowCharacterState()
        {
            if (Entity == null)
                return false;

            if (Entity.type != UnitEntityType.MultiPlayer)
                return false;

            return graphicSettings.CurrentShadowState == ShadowMultiPlayerQuality.Shadow || graphicSettings.CurrentShadowState == ShadowMultiPlayerQuality.HalfShadow;
        }

        protected override bool CanBeLookTarget(TargetType targetType)
        {
            // 큐펫의 타겟이 될 수 없다.
            switch (targetType)
            {
                case TargetType.AlliesCupet:
                case TargetType.EnemyCupet:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 전직 시 호출
        /// </summary>
        private void ChangeJob(bool isInit)
        {
            if (isInit)
                return;

            EffectPlayer.PlayJobChangeEffect();
            UpdateStyle();
        }

        /// <summary>
        /// 외형 새로고침
        /// </summary>
        private void UpdateStyle()
        {
            SetStyle(characterEntity.Character.Gender, characterEntity.Character.Job);
        }

        /// <summary>
        /// 장착 아이템 변경 시 호출
        /// </summary>
        protected virtual void UpdateWeapon()
        {
            if (characterEntity.IsIgnoreWeapon)
            {
                animator.SetWeaponType(EquipmentClassType.OneHandedSword);
                return;
            }

            // 무기 코스튬 타입 세팅
            string weaponCostumePrefabName = characterEntity.GetCostumePrefabName(ItemEquipmentSlotType.CostumeWeapon);
            string weaponCostumeEffectName = characterEntity.GetEffectName(ItemEquipmentSlotType.CostumeWeapon);

            // 무기 세팅
            EquipmentClassType weaponType = characterEntity.GetWeaponType();
            string weaponPrefabName = characterEntity.GetWeaponPrefabName();
            string weaponEffectName = characterEntity.GetEffectName(ItemEquipmentSlotType.Weapon);
            animator.SetWeaponType(weaponType);

            // 코스튬 체크
            if (!string.IsNullOrEmpty(weaponCostumePrefabName))
            {
                // 같은 무기 타입의 코스튬일때만 표시
                CostumeType weaponCostumeType = characterEntity.GetCostumeWeaponType();
                if (weaponCostumeType.ToWeaponType().Equals(weaponType))
                {
                    weaponPrefabName = weaponCostumePrefabName;
                    weaponEffectName = weaponCostumeEffectName;
                }
            }

            appearance.SetWeaponType(weaponPrefabName, weaponType, weaponEffectName);
        }

        private void UpdateCostumeHat()
        {
            string prefabName = characterEntity.GetCostumePrefabName(ItemEquipmentSlotType.CostumeHat);
            CostumeData costumeData = characterEntity.GetCostumeData();
            string effectName = characterEntity.GetEffectName(ItemEquipmentSlotType.CostumeHat);
            appearance.SetCostumeHat(prefabName, costumeData, effectName); // 모자
        }

        private void UpdateCostumeFace()
        {
            string prefabName = characterEntity.GetCostumePrefabName(ItemEquipmentSlotType.CostumeFace);
            string effectName = characterEntity.GetEffectName(ItemEquipmentSlotType.CostumeFace);
            appearance.SetCostumeFace(prefabName, effectName); // 얼굴
        }

        private void UpdateCostumeCape()
        {
            string prefabName = characterEntity.GetCostumePrefabName(ItemEquipmentSlotType.CostumeCape);
            string effectName = characterEntity.GetEffectName(ItemEquipmentSlotType.CostumeCape);
            appearance.SetCostumeCape(prefabName, effectName); // 망토
        }

        private void UpdateCostumePet()
        {
            string prefabName = characterEntity.GetCostumePrefabName(ItemEquipmentSlotType.CostumePet);
            string effectName = characterEntity.GetEffectName(ItemEquipmentSlotType.CostumePet);
            appearance.SetCostumePet(prefabName, effectName);
        }

        private void UpdateCostumeBoby()
        {
            // 몸 코스튬 타입 세팅
            string prefabName = characterEntity.GetCostumePrefabName(ItemEquipmentSlotType.CostumeBody);
            string effectName = characterEntity.GetEffectName(ItemEquipmentSlotType.CostumeBody);

            // 코스튬 체크
            if (!string.IsNullOrEmpty(prefabName))
            {
                // 몸 코스튬 성별 체크
                CostumeBodyType costumeBodyType = characterEntity.GetCostumeBodyType();
                if (costumeBodyType == CostumeBodyType.Male && characterEntity.GetGender() == Gender.Female)
                {
                    prefabName = string.Empty;
                    effectName = string.Empty;
                }

                if (costumeBodyType == CostumeBodyType.Female && characterEntity.GetGender() == Gender.Male)
                {
                    prefabName = string.Empty;
                    effectName = string.Empty;
                }
            }

            appearance.SetCostumeBody(prefabName, effectName);
        }

        private void UpdateCostumeWihtWeapon()
        {
            UpdateCostumeBoby(); // 몸
            UpdateCostumeHat(); // 모자
            UpdateCostumeFace(); // 얼굴
            UpdateCostumeCape(); // 망토
            UpdateCostumePet(); // 펫
            UpdateWeapon(); // 무기 && 무기 코스튬
        }

        private void UpdatePrivateStoreSelling()
        {
            bool isPrivateStoreSelling = characterEntity.Trade.SellingState == PrivateStoreSellingState.SELLING;
            appearance.SetPrivateStoreSelling(isPrivateStoreSelling);
            // 개인상점 열면 정면 보기
            if (isPrivateStoreSelling)
                LookFront();
        }

        private void LookFront()
        {
            Movement.Look(mainCamera.transform.position);
        }
    }
}