#Before you begin, know this 

### Notion of IAP Catalog

Very important that you understand how the **IAP Catalog** works on UDP.

The IAP Catalog is an inventory of the IAP Products implemented in your game. For each IAP Product, you define a description, a price, a consumable type, and a Product ID.

Your game, once repacked and published on a store, will query its IAP inventory from the store’s back-end. The store is given your game’s IAP Catalog via UDP. When players make in-game purchases, your game asks the store to confirm the IAP Catalog. UDP must be properly implemented in your game for this step to work smoothly.

With UDP, the IAP Catalog you define in the Unity Editor is synced with the UDP Console (web portal). And when your game is repacked and submitted to a store, the IAP Catalog is synced with the store’s back-end:

![img](images/image_1.png)

The **IAP Catalog on UDP Console** is the keystone.

Unity takes care of the sync between the UDP Console and the store’s back-end.

This part is Unity’s responsibility and has been extensively tested before public preview release.

**It is crucial that the IAP Catalog implemented in the Editor syncs properly with the UDP Console**. That part relies on your implementation and requires that you closely follow the UDP implementation guidance.

A misguided implementation could break the IAP Catalog sync between the game client and the server, and typically result in symptoms in the final game such as:

- The wallet doesn’t appear when invoked
- IAPs are unresponsive

Which means your game is not able to monetize. 

### Two different ways to implement UDP

There are 2 ways to implement UDP in your game:

- **Using Unity IAP**. If your game is already using Unity IAP, it should be the obvious choice
- **Using the UDP Package**. We designed this implementation to be similar to the Google Play In-App Billing implementation, so that if your game was originally wired that way then porting it to UDP should be straightforward

You are free to choose either implementation, but it is important you stick to **ONLY ONE**. More on that [here](Do_not_mix_the_implementations.md). 

Below is a quick run-down of both options.

##### Using Unity IAP

From version 1.22 and above, you can implement UDP directly from Unity IAP ([how to get set up](Using_Unity_IAP_detail.md))

If your game is already using Unity IAP, it should be the obvious and simplest choice.

Make sure you follow the general implementation guidelines in [Unity IAP’s Documentation](https://docs.unity3d.com/Manual/UnityIAP.html).

Check the details of the [Editor UI elements involved in implementing UDP via Unity IAP](Editor_UI_elements_for_UDP_implementation_via_Unity_IAP.md).

##### Using the UDP Package

This is a standalone package that is totally independent from Unity IAP.

The current version is 1.0.3 ([where to get it from](UDP_Package.md))

It also has, necessarily, a [notion of IAP Catalog](Before_you_begin_know_this.md), so that your IAP Products can be synced with the UDP Console and then with the stores. 

We designed this implementation to be similar to the Google Play IAB implementation, so that if your game was wired that way then porting it to UDP would be straightforward.

It requires that you explicitly initialize UDP, query the store’s IAP inventory, request to purchase a product, consume the purchase, as is explained in [this section of the UDP Documentation](Implementing_UDP_in-app_purchases.md).

Check the details of the [Editor UI elements involved in implementing the UDP Package](Editor_UI_elements_for_UDP_implementation_via_UDP_Package.md). 