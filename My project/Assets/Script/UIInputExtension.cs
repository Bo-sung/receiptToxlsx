using UnityEngine;
using System.Collections.Generic;
using System.Text;

public sealed class UIInputExtension : UIInput
{
    /// <summary>
    /// 주의!! 모비일인 경우에만 사용
    /// </summary>
    public List<EventDelegate> onShow = new List<EventDelegate>();
    bool IsOnShowExecuted = true;
    protected override void Update()
    {
        base.Update();

#if Mobile
            if (mKeyboard != null)
			{
				if(mKeyboard.status == TouchScreenKeyboard.Status.Visible || mKeyboard.active) OnShow();
            }
            if (mKeyboard.status == TouchScreenKeyboard.Status.Visible || mKeyboard.active)
            {                
                Debug.Log($"mKeyboard.status == TouchScreenKeyboard.Status.Visible || mKeyboard.active = {true}");
            }

#endif //Mobile
        if (isSelected && IsOnShowExecuted)
            OnShow();
        else if(!isSelected && IsOnShowExecuted)
            IsOnShowExecuted = true;
    }
    public void OnShow()
    {
        if (current == null && EventDelegate.IsValid(onShow))
        {
            current = this;
            EventDelegate.Execute(onShow);
            current = null;
            IsOnShowExecuted = false;
        }
    }

}

