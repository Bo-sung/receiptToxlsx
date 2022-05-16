/*using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace Ragnarok
{   
    public class PurchaseResponse
    {
        public bool isSuccess;
        public Product product;
        public PurchaseFailureReason error;
        public string purchaseData;
        public string signature;

        public PurchaseResponse(bool isSuccess, Product product, string purchaseData = "", string signature = "", PurchaseFailureReason error = PurchaseFailureReason.Unknown)
        {
            this.isSuccess = isSuccess;
            this.product = product;
            this.purchaseData = purchaseData;
            this.signature = signature;
            this.error = error;
        }
    }

    public class IAP : Singleton<IAP>, IStoreListener
    {
        private IStoreController m_StoreController = null;
        private IExtensionProvider m_StoreExtensionProvider = null;

        private PurchaseResponse response = null;
        private bool purchaseInProgress = false;
        public Action<PurchaseResponse> OnPurchaseResult; // 결제 결과 이벤트

        protected override void OnTitle()
        {
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public async Task Initialized(IEnumerable<ObscuredString> productIds)
        {
            response = null;
            if (null == m_StoreController)
            {
                ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());                
                foreach (var item in productIds)
                {
                    builder.AddProduct(item, ProductType.Consumable);
                }
                UnityPurchasing.Initialize(this, builder);
            }
            await new WaitUntil(IsInitialized);
        }

        /// <summary>
        /// 초기화 여부
        /// </summary>
        /// <returns></returns>
        private bool IsInitialized()
        {
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        /// <summary>
        /// 초기화 성공
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="extensions"></param>
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("인앱 초기화 성공");
            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
        }

        /// <summary>
        /// 초기화 실패
        /// </summary>
        /// <param name="error"></param>
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError(error);
        }

        public Product GetProduct(string productId)
        {
            if (!IsInitialized())
                return null;

            return m_StoreController.products.WithID(productId);
        }

        /// <summary>
        /// 구매
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="payload"></param>
        public async Task<PurchaseResponse> Purchase(string productId, string payload = "")
        {
            response = null;
            if (IsInitialized())
            {
                Product product = GetProduct(productId);
                if (product != null && product.availableToPurchase)
                {
                    purchaseInProgress = true;
                    m_StoreController.InitiatePurchase(product, payload);
                    await new WaitUntil(IsReceived);
                }
            }
            return response;
        }

        /// <summary>
        /// 구매 응답
        /// </summary>
        /// <returns></returns>
        private bool IsReceived()
        {
            return response != null;
        }

        /// <summary>
        /// 구매 실패
        /// </summary>
        /// <param name="i"></param>
        /// <param name="p"></param>
        public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
        {
            purchaseInProgress = false;
            Debug.LogError(string.Format($"Id={i.definition.id}, PurchaseFailureReason={p}"));
            response = new PurchaseResponse(false, i, error: p);
            OnPurchaseResult?.Invoke(response);
        }

        /// <summary>
        /// 구매 성공
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            Product i = e.purchasedProduct;

            var wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(i.receipt);
            if (wrapper == null)
                throw new InvalidReceiptDataException();

            var payload = (string)wrapper ["Payload"];

            if (Application.platform == RuntimePlatform.Android)
            {                
                var details = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
                var json = (string)details["json"];
                var signature = (string)details["signature"];
                response = new PurchaseResponse(true, i, json, signature);                
            }
            else if(Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                response = new PurchaseResponse(true, i, payload, payload);            
            }

            // 결제 실패 후 재구매 루틴
            if(!purchaseInProgress)
            {
                // TODO 재구매 불가능 아이템일경우 바로 소모시켜야 함
                ShopData shopData = ShopDataManager.Instance.GetByProductId(i.definition.id);
                if(shopData != null)
                {

                }
            }
           
            purchaseInProgress = false;
            OnPurchaseResult?.Invoke(response);
            return PurchaseProcessingResult.Pending;
        }

        /// <summary>
        /// 구매완료 후 호출 
        /// 구매아이템 소모
        /// </summary>
        /// <param name="p"></param>
        public void ConfirmPendingPurchase(Product p)
        {
            UI.HideIndeicator();
            m_StoreController.ConfirmPendingPurchase(p);
        }
    }
}
*/
