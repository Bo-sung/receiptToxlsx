#if UNITY_EDITOR
using UnityEngine;

namespace Ragnarok.Test
{
    public class TestSecretShop : TestCode
    {
        [TextArea(1, 1)]
        public string Description =      
            "스페이스바 : 비밀상점 목록 변경";

        protected override async void OnMainTest()
        {            
            await Entity.player.ShopModel.RequestSecretShopInit(useResetItem: false);
        }
    }
} 
#endif
