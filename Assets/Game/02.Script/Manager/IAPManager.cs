using UnityEngine;
using UnityEngine.Purchasing;
using System;

namespace ThreeMatch.Manager
{
    public class IAPManager : IStoreListener
    {
        public static IAPManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IAPManager();
                    InitializePurchasing();
                }

                return Instance;
            }
        }

        private static IAPManager _instance;
        private static IStoreController storeController; // Unity IAP의 스토어 컨트롤러
        private static IExtensionProvider storeExtensionProvider; // 확장 기능 제공자

        // 상품 ID (IAP Catalog의 상품 ID와 일치해야 합니다)
        public const string PRODUCT_ID_CONSUMABLE = "consumable_product";
        public const string PRODUCT_ID_NONCONSUMABLE = "nonconsumable_product";
        public const string PRODUCT_ID_SUBSCRIPTION = "subscription_product";
        
        // IAP 초기화
        private static void InitializePurchasing()
        {
            if (IsInitialized())
            {
                return;
            }

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // 상품 추가 (ID와 유형을 설정)
            builder.AddProduct(PRODUCT_ID_CONSUMABLE, ProductType.Consumable);
            builder.AddProduct(PRODUCT_ID_NONCONSUMABLE, ProductType.NonConsumable);
            builder.AddProduct(PRODUCT_ID_SUBSCRIPTION, ProductType.Subscription);

            UnityPurchasing.Initialize(_instance, builder);
        }

        private static bool IsInitialized()
        {
            return storeController != null && storeExtensionProvider != null;
        }

        // 구매 요청
        public void BuyConsumable()
        {
            BuyProductID(PRODUCT_ID_CONSUMABLE);
        }

        public void BuyNonConsumable()
        {
            BuyProductID(PRODUCT_ID_NONCONSUMABLE);
        }

        public void BuySubscription()
        {
            BuyProductID(PRODUCT_ID_SUBSCRIPTION);
        }

        private void BuyProductID(string productId)
        {
            if (IsInitialized())
            {
                Product product = storeController.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    storeController.InitiatePurchase(product);
                }
                else
                {
                    Debug.Log("상품을 구매할 수 없습니다.");
                }
            }
            else
            {
                Debug.Log("스토어가 초기화되지 않았습니다.");
            }
        }

        // 구매 실패 시 호출
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {

        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            if (string.Equals(args.purchasedProduct.definition.id, PRODUCT_ID_CONSUMABLE, StringComparison.Ordinal))
            {
                Debug.Log("소모품 구매 완료");
                // 소모품 구매 후 처리
            }
            else if (string.Equals(args.purchasedProduct.definition.id, PRODUCT_ID_NONCONSUMABLE,
                         StringComparison.Ordinal))
            {
                Debug.Log("비소모품 구매 완료");
                // 비소모품 구매 후 처리
            }
            else if (string.Equals(args.purchasedProduct.definition.id, PRODUCT_ID_SUBSCRIPTION,
                         StringComparison.Ordinal))
            {
                Debug.Log("구독 구매 완료");
                // 구독 구매 후 처리
            }
            else
            {
                Debug.Log("알 수 없는 상품을 구매했습니다.");
            }

            return PurchaseProcessingResult.Complete;
        }

        // 구매 실패 시 호출
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log($"구매 실패: {product.definition.storeSpecificId}, 사유: {failureReason}");
        }

        // IStoreListener 구현
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            storeController = controller;
            storeExtensionProvider = extensions;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("스토어 초기화 실패: " + error);
        }
    }
}