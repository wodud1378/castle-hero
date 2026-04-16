using System;
using Cysharp.Threading.Tasks;

namespace CastleHero.Common.InApp
{
    public class IAPManager// : IDetailedStoreListener
    {
        public string GetLocalizedPrice(string productId) => string.Empty;

        // private IStoreController storeController;
        // private IExtensionProvider storeExtensionProvider;
        //
        // private UniTaskCompletionSource<string> purchaseCompletionSource;
        //
        // // IAP 초기화
        // public IAPManager(string[] inAppProducts)
        // {
        //     InitializePurchasing(inAppProducts);
        // }
        //
        // // 현지화된 가격 가져오는 메서드
        // public string GetLocalizedPrice(string productId)
        // {
        //     if (!IsInitialized()) return null;
        //
        //     Product product = storeController.products.WithID(productId);
        //     return product?.metadata.localizedPriceString;
        // }
        //
        // // IAP 초기화 메서드
        // private void InitializePurchasing(string[] inAppProducts)
        // {
        //     if (IsInitialized())
        //         return;
        //
        //     var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        //     foreach (var product in inAppProducts)
        //     {
        //         builder.AddProduct(product, ProductType.Consumable, new IDs
        //         {
        //             { product, GooglePlay.Name },
        //             //{ product, MacAppStore.Name }
        //         });
        //     }
        //
        //     UnityPurchasing.Initialize(this, builder);
        // }
        //
        // public void OnInitializeFailed(InitializationFailureReason error, string message)
        // {
        // }
        //
        // // IAP 초기화 상태 확인
        // private bool IsInitialized()
        // {
        //     return storeController != null && storeExtensionProvider != null;
        // }
        //
        // // 비동기 구매 요청
        // public async UniTask<string> BuyProductAsync(string productId)
        // {
        //     if (!IsInitialized())
        //     {
        //         throw new InvalidOperationException("IAP not initialized.");
        //     }
        //
        //     Product product = storeController.products.WithID(productId);
        //     if (product == null || !product.availableToPurchase)
        //     {
        //         throw new InvalidOperationException("Product not available for purchase.");
        //     }
        //
        //     // UniTaskCompletionSource 생성하여 구매 완료를 기다림
        //     purchaseCompletionSource = new UniTaskCompletionSource<string>();
        //     storeController.InitiatePurchase(product);
        //
        //     // 구매 완료 결과를 반환
        //     return await purchaseCompletionSource.Task;
        // }
        //
        // // 구매 성공 시 호출되는 콜백
        // public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        // {
        //     if (purchaseCompletionSource != null)
        //     {
        //         // 성공 시 UniTaskCompletionSource 완료
        //         purchaseCompletionSource.TrySetResult(args.purchasedProduct.definition.id);
        //     }
        //
        //     return PurchaseProcessingResult.Complete;
        // }
        //
        // public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        // {
        //     if (purchaseCompletionSource != null)
        //     {
        //         // 실패 시 UniTaskCompletionSource를 실패 상태로 설정
        //         purchaseCompletionSource.TrySetException(new Exception($"Purchase failed: {failureReason}"));
        //     }
        // }
        //
        // // IStoreListener 인터페이스 구현 (초기화 완료 시 호출)
        // public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        // {
        //     storeController = controller;
        //     storeExtensionProvider = extensions;
        // }
        //
        // // 구매 실패 시 호출되는 콜백
        // public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        // {
        //     if (purchaseCompletionSource != null)
        //     {
        //         // 실패 시 UniTaskCompletionSource를 실패 상태로 설정
        //         purchaseCompletionSource.TrySetException(new Exception($"Purchase failed: {failureDescription.message}"));
        //     }
        // }
        //
        // // IStoreListener 인터페이스 구현 (초기화 실패 시 호출)
        // public void OnInitializeFailed(InitializationFailureReason error)
        // {
        //     throw new InvalidOperationException($"IAP initialization failed: {error}");
        // }
    }
}