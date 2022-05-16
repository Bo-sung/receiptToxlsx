using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class CardOptionSlot : UIView
    {
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelValue;
        [SerializeField] UILabelHelper labelMax;
        [SerializeField] UIProgressBar gauge;

        protected override void OnLocalize()
        {
        }        

        public void Set(string title, string value, string maxValue, float gaugeValue, Color32 color)
        {
            labelTitle.Text = title;
            labelValue.Text = value;
            labelMax.Text = maxValue;
            gauge.value = gaugeValue;
            gauge.foregroundWidget.color = color;
        }
    }
}