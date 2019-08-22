using System;
using NUnit.Framework;
using UnityEngine.UDP;

public class UDPRuntimeTests
{
    [Test]
    public void PlayModeTestInitializationPasses()
    {
        AppInfo appInfo = new AppInfo();
        StoreService.Initialize(new InitListener(), appInfo);
    }

    [Test]
    public void PlayModeTestPurchaseFails()
    {
        StoreService.ConsumePurchase(new PurchaseInfo(), new PurchaseListener());
    }

    [Test]
    public void PlayModeTestQueryInventoryFails()
    {
        StoreService.ConsumePurchase(new PurchaseInfo(), new PurchaseListener());
    }

    [Test]
    public void PlayModeTestConsumePurchaseFails()
    {
        StoreService.ConsumePurchase(new PurchaseInfo(), new PurchaseListener());
    }

    class PurchaseListener : IPurchaseListener
    {
        public void OnPurchase(PurchaseInfo purchaseInfo)
        {
              Assert.Pass();
        }

        public void OnPurchaseFailed(string message, PurchaseInfo purchaseInfo)
        {
              Assert.Pass();
        }

        public void OnPurchaseRepeated(string productId)
        {
              Assert.Pass();
        }

        public void OnPurchaseConsume(PurchaseInfo purchaseInfo)
        {
              Assert.Pass();
        }

        public void OnPurchaseConsumeFailed(string message, PurchaseInfo purchaseInfo)
        {
              Assert.Pass();
        }

        public void OnQueryInventory(Inventory inventory)
        {
              Assert.Pass();
        }

        public void OnQueryInventoryFailed(string message)
        {
              Assert.Pass();
        }
    }

    class InitListener : IInitListener
    {
        public void OnInitialized(UserInfo userInfo)
        {
            Assert.Pass();
        }

        public void OnInitializeFailed(string message)
        {
            Assert.Fail();
        }
    }
}