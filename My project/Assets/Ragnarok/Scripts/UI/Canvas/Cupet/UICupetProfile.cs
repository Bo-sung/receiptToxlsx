using UnityEngine;

namespace Ragnarok
{
    public class UICupetProfile : UIInfo<CupetModel>
    {
        [SerializeField] UITextureHelper thumbnail;
        [SerializeField] GameObject equipRibbon;
        [SerializeField] UILabelHelper labelEquip;
        [SerializeField] UILabelHelper labelCover;
        [SerializeField] UIGridHelper rate;
        [SerializeField] UIGraySprite iconCupetType;
        [SerializeField] UILabelHelper labelLevel;

        protected override void OnLocalize()
        {
            base.OnLocalize();

            if (labelEquip)
                labelEquip.LocalKey = LocalizeKey._5010; // E

            if (labelCover)
                labelCover.LocalKey = LocalizeKey._5025; // 출전 중
        }

        protected override void Refresh()
        {
            if (IsInvalid())
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            if (thumbnail)
                thumbnail.SetMonster(info.ThumbnailName);

            if (iconCupetType)
                iconCupetType.spriteName = info.CupetType.GetIconName();

            // 보유중인 큐펫 데이터
            if (info.IsInPossession)
            {
                if (thumbnail)
                    thumbnail.Mode = UIGraySprite.SpriteMode.None;

                if (iconCupetType)
                    iconCupetType.Mode = UIGraySprite.SpriteMode.None;

                if (equipRibbon)
                    equipRibbon.SetActive(info.IsEquip);

                if (rate)
                {
                    rate.SetActive(true);
                    rate.SetValue(info.Rank);
                }

                if (labelLevel)
                {
                    labelLevel.Text = string.Concat("Lv. ", info.Level.ToString());
                    labelLevel.SetActive(true);
                }
            }
            else
            {
                if (thumbnail)
                    thumbnail.Mode = UIGraySprite.SpriteMode.Grayscale;

                if (iconCupetType)
                    iconCupetType.Mode = UIGraySprite.SpriteMode.Grayscale;

                if (equipRibbon)
                    equipRibbon.SetActive(false);

                if (rate)
                    rate.SetActive(false);

                if (labelLevel)
                    labelLevel.SetActive(false);
            }
        }
    }
}