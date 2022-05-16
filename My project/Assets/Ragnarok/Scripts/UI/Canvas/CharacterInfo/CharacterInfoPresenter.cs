using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICharacterInfo"/>
    /// </summary>
    public sealed class CharacterInfoPresenter : ViewPresenter, UICostumeInfoSlot.Impl
    {
        public interface IView
        {
            void ShowEquippedItemSlot();

            UILabelHelper GetAPLabel();
            UIPlayTween GetUpValuePlay();
            UILabelHelper GetUpValueLabel();
            ContentAbility GetAbilitySubcanvas();

            void SetStrongerEquipmentNotice();
        }

        private readonly IView view;
        private readonly InventoryModel inventoryModel;
        private readonly StatusModel statusModel;

        private ContentAbilityPresenter CAPresenter = null;
        UIPlayTween playUpValue;
        UILabelHelper labUpValue;
        UILabelHelper labAP;

        // 전투력 증감 연출을 위한 변수 (시작전투력, 애니메이션용 수치, 최종전투력)
        int prevAP, viewAP, curAP;
        bool isPlayingAPAnimation;

        public CharacterInfoPresenter(IView view)
        {
            this.view = view;

            inventoryModel = Entity.player.Inventory;
            statusModel = Entity.player.Status;
        }

        void Initialize()
        {
            //	AP 이벤트를 위한 초기화
            prevAP = viewAP = 0;
            curAP = 0;

            if (CAPresenter == null)
            {
                CAPresenter = view.GetAbilitySubcanvas()?.GetPresenter();
            }

            playUpValue = playUpValue ?? view.GetUpValuePlay();
            labUpValue = labUpValue ?? view.GetUpValueLabel();
            labAP = labAP ?? view.GetAPLabel();

            isPlayingAPAnimation = false;
        }

        public override void AddEvent()
        {
            Initialize();

            labUpValue?.SetActive(false);
            CAPresenter.OnChangeAP += OnIncreaseAP;
            inventoryModel.OnUpdateItem += view.ShowEquippedItemSlot;
            inventoryModel.OnChangeCostume += view.ShowEquippedItemSlot;
            statusModel.OnUpdateBasicStatus += OnUpdateAP;
            Entity.player.OnReloadStatus += OnReloadStatus;

            // 더 좋은 장비 탐색을 위한 이벤트
            inventoryModel.EquipmentItemAttackPowerInfo.OnAttackPowerTableUpdate += OnUpdateEquipmentAttackPower;
        }

        public override void RemoveEvent()
        {
            ContentAbility CA = view.GetAbilitySubcanvas();
            ContentAbilityPresenter CAPresenter = CA.GetPresenter();
            CAPresenter.OnChangeAP -= OnIncreaseAP;
            isPlayingAPAnimation = false;

            inventoryModel.OnUpdateItem -= view.ShowEquippedItemSlot;
            inventoryModel.OnChangeCostume -= view.ShowEquippedItemSlot;
            statusModel.OnUpdateBasicStatus -= OnUpdateAP;
            Entity.player.OnReloadStatus -= OnReloadStatus;

            // 더 좋은 장비 탐색을 위한 이벤트
            inventoryModel.EquipmentItemAttackPowerInfo.OnAttackPowerTableUpdate -= OnUpdateEquipmentAttackPower;
        }

        public void OnUpdateAP()
        {
            var sb = StringBuilderPool.Get();
            sb.Append(LocalizeKey._4047.ToText()); // 전투력 :
            sb.Append(" ");
            sb.Append(Entity.player.GetTotalAttackPower());
            labAP.Text = sb.Release();
        }

        void OnIncreaseAP(int prev, int cur)
        {
            curAP = cur;
            if (!isPlayingAPAnimation)
            {
                viewAP = prevAP = prev;
                IncreaseAPDynamically();
            }
        }

        async void IncreaseAPDynamically()
        {
            isPlayingAPAnimation = true;

            int diffValue = 0;
            float viewAPFloat = viewAP;
            var sb = StringBuilderPool.Get();
            while (viewAP < curAP && isPlayingAPAnimation)
            {
                float addVal = (curAP - prevAP) * 0.025f;
                if (addVal < 1f)
                    addVal = 1f;
                viewAPFloat += addVal;
                viewAP = (int)viewAPFloat;
                if (viewAP > curAP)
                    viewAP = curAP;

                // UpValue 업데이트
                int upValue = viewAP - prevAP;
                int colorValue = (int)((1f - (float)(viewAP - prevAP) / (float)(curAP - prevAP)) * 255f);
                string hexValue = colorValue.ToString("X2");
                sb.Clear();
                sb.Append("[");
                sb.Append(hexValue);
                sb.Append("FF");
                sb.Append(hexValue);
                sb.Append("]+");
                sb.Append(upValue);

                labUpValue.SetActive(true);
                labUpValue.Text = sb.ToString();
                playUpValue.Play();

                // 전투력 업데이트
                sb.Clear();
                sb.Append(LocalizeKey._4047.ToText()); // 전투력 :
                sb.Append(" ");
                sb.Append(viewAP);
                labAP.Text = sb.ToString();

                await Awaiters.Seconds(0.04f);
            }

            sb.Release();
            isPlayingAPAnimation = false;
        }

        /// <summary>
        /// 플레이 중인 캐릭터엔터티 반환
        /// </summary>
        public CharacterEntity GetPlayer()
        {
            return Entity.player;
        }

        /// <summary>
        /// 장착중인 장비
        /// </summary>
        public ItemInfo GetEquippedItem(ItemEquipmentSlotType type)
        {
            return inventoryModel.itemList.Find(a => a.EquippedSlotType == type);
        }

        /// <summary>
        /// 장착할 수 있는 장비가 있는지 (장비가 없을 때만)
        /// </summary>
        public bool CanEquip(ItemEquipmentSlotType slotType)
        {
            if (GetEquippedItem(slotType) != null)
                return false;

            return inventoryModel.HasStrongerEquipmentInSlotType(slotType);
        }

        public bool EquipableCostume()
        {
            return inventoryModel.EquipableCostume();
        }

        /// <summary>
        /// 장착할 수 있는 코스튬이 있는지 (장비가 없을 때만)
        /// </summary>
        public bool EquipableCostume(ItemEquipmentSlotType slotType)
        {
            if (GetEquippedItem(slotType) != null)
                return false;

            return inventoryModel.EquipableCostume(slotType);
        }

        private void OnReloadStatus()
        {
            OnUpdateAP();
        }

        /// <summary>
        /// 장비 전투력 업데이트 신호 -> 더 강한 장비 발생 시, 해당 슬롯에 Notice 띄우기.
        /// </summary>
        private void OnUpdateEquipmentAttackPower()
        {
            view.SetStrongerEquipmentNotice();
        }

        public bool HasStrongerEquipment()
        {
            return inventoryModel.HasStrongerEquipment();
        }

        public bool HasStrongerShadowEquipment()
        {
            return inventoryModel.HasStrongerShadowEquipment();
        }

        /// <summary>
        /// 해당 슬롯타입의 더 강한 장비 존재 여부
        /// </summary>
        public bool HasStrongerEquipment(ItemEquipmentSlotType slotType)
        {
            return inventoryModel.HasStrongerEquipmentInSlotType(slotType);
        }

        void UICostumeInfoSlot.Impl.OnSelect(ItemInfo item)
        {
            UI.Show<UICostumeInfo>().Set(item.ItemNo);
        }

        bool UICostumeInfoSlot.Impl.IsDisassemble(long itemNo)
        {
            return default;
        }

        TweenAlpha UICostumeInfoSlot.Impl.TweenAlpha => default;
    }
}