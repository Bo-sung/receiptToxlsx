using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class SelectPropertyPopupPresenter : ViewPresenter
    {
        // <!-- Models --!>

        // <!-- Repositories --!>

        // <!-- Event --!>

        List<ElementType> inverseStrong;
        List<ElementType> inverseWeak;
        List<ElementType> directStrong;
        List<ElementType> directWeak;
        Dictionary<PropertyInfoType, List<ElementType>> infos;

        public SelectPropertyPopupPresenter()
        {
            inverseStrong = new List<ElementType>();
            inverseWeak = new List<ElementType>();
            directStrong = new List<ElementType>();
            directWeak = new List<ElementType>();

            infos = new Dictionary<PropertyInfoType, List<ElementType>>();
            infos.Add(PropertyInfoType.INVERSE_STRONG, null);
            infos.Add(PropertyInfoType.INVERSE_WEAK, null);
            infos.Add(PropertyInfoType.DIRECT_STRONG, null);
            infos.Add(PropertyInfoType.DIRECT_WEAK, null);
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public Dictionary<PropertyInfoType, List<ElementType>> GetPropertyInfos(ElementType mainType)
        {
            inverseStrong.Clear();
            inverseWeak.Clear();
            directStrong.Clear();
            directWeak.Clear();

            var defaultFacter = mainType.GetElementFactor(ElementType.None);

            foreach(ElementType e in System.Enum.GetValues(typeof(ElementType)))
            {
                var inverseFacter = e.GetElementFactor(mainType);
                if (inverseFacter > defaultFacter) inverseStrong.Add(e);
                if (inverseFacter < defaultFacter) inverseWeak.Add(e);

                var directFacter = mainType.GetElementFactor(e);
                if (directFacter > defaultFacter) directStrong.Add(e);
                if (directFacter < defaultFacter) directWeak.Add(e);
            }

            infos[PropertyInfoType.INVERSE_STRONG] = inverseStrong;
            infos[PropertyInfoType.INVERSE_WEAK] = inverseWeak;
            infos[PropertyInfoType.DIRECT_STRONG] = directStrong;
            infos[PropertyInfoType.DIRECT_WEAK] = directWeak;

            return infos;
        }
    }
}