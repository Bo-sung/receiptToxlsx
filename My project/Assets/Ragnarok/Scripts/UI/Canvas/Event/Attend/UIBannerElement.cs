using MEC;
using Ragnarok.View;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIBannerElement : UIElement<UIBannerElement.IInput>
    {
        public interface IInput
        {
            string ImageUrl { get; }
            string Name { get; }
            string Description { get; }
            RemainTime RemainTime { get; }
        }

        [SerializeField] UITextureHelper textureBanner;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelHelper labelRemainTime;

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            textureBanner.SetFromUrl(info.ImageUrl);
            labelTitle.Text = info.Name;
            labelDescription.Text = info.Description;
            Timing.RunCoroutineSingleton(YieldRemainTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private IEnumerator<float> YieldRemainTime()
        {
            while (true)
            {
                System.TimeSpan time = info.RemainTime.ToRemainTime().ToTimeSpan();

                if (time.Ticks <= 0)
                    break;

                if(time.Days >= 1000)
                {
                    labelRemainTime.SetActive(false);
                }
                else
                {
                    labelRemainTime.SetActive(true);
                    labelRemainTime.Text = LocalizeKey._11005.ToText()
                        .Replace("{DAY}", time.Days.ToString())
                        .Replace("{HOUR}", time.Hours.ToString())
                        .Replace("{MINUTE}", time.Minutes.ToString());
                }
                yield return Timing.WaitForSeconds(1f);
            }
            labelRemainTime.Text = "00:00:00";
        }
    }
}