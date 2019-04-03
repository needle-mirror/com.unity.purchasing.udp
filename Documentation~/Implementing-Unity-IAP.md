## Implementing UDP in-app purchases with Unity IAP

If you want to use UDP with Unity IAP instead of the UDP package, [set up Unity IAP](https://docs.unity3d.com/Manual/UnityIAPSettingUp.html). 

Note: If you choose to implement UDP with Unity IAP instead of the UDP package, implement via Unity IAP only.

When the Unity IAP is enabled, take the following steps to set up UDP with Unity IAP .

Step 1: In the Unity Editor, to choose UDP as the target platform for the game to build, select **Unity IAP** > **Android** > **Target Unity Distribution Portal (UDP)**.

Step 2: To configure the IAP catalog, select **Window** > **Unity IAP** > **IAP Catalog**.

Under the Unity Distribution Portal section, you will find the following fields for each IAP product:

* **Name**, which specifies the name of the product

* **Product ID**, which specifies the unique identifier of the product

* **Type**, either consumable or non-consumable

* **Price**, which specifies the price of the product

* **Description**, which describes the product