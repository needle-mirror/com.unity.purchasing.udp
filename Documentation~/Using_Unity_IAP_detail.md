# Using Unity IAP

If you want to use UDP with Unity IAP instead of the UDP package, [set up Unity IAP](https://docs.unity3d.com/Manual/UnityIAPSettingUp.html). 

**Note**: If you choose to implement UDP with Unity IAP (instead of using the UDP package) then implement via Unity IAP only. Make sure you [Don't mix the implementations](Do_not_mix_the_implementations.md).

Once you have implemented your game’s in-app purchases with Unity IAP, take the following steps to set up UDP with Unity IAP.

### Set UDP as build target

In the Unity Editor, to choose UDP as the target platform for the game to build, select Unity **IAP > Android > Target Unity Distribution Portal (UDP)**

![img](images/image_10.png)

### Fill in the IAP Catalog

To understand how the IAP Catalog works in the UDP context, see [Notion of IAP Catalog](Before_you_begin_know_this.md).

To configure the IAP catalog, select **Window > Unity IAP > IAP Catalog** and for each IAP product fill in the following fields:

- **ID**, the unique identifier of the IAP product
- **Type**, consumable or non-consumable
- **Title**, the name of the IAP product
- **Description**, a short description of the IAP product
- **Price**, the price of the IAP product (in USD). This field is found directly under the **Unity Distribution Portal Configuration** section

![img](images/image_11.png)

To save your IAP product, click **Sync to UDP.**

> **Note**: remember to **Sync to UDP** <u>every individual IAP product</u> that you add to the catalog under the **UDP Configuration** section, using the button immediately below the price field:

 

![img](images/image_12.png)

 

> To ensure your IAP Catalog is properly saved, see [Save / Sync / Push your IAP Catalog](Save_Sync_Push_your_IAP_Catalog.md).

**Note**: if you choose not to use the Codeless way of implementing Unity IAP, you have to manually create and sync your IAP Catalog on the UDP Console. For more information, see [Editing in-app purchases](Editing_your_game_information_on_the_UDP_console.md).

