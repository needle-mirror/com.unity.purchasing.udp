#Before you begin, know this 

### Notion of IAP Catalog

It is <u>very important</u> that you understand how the **IAP Catalog** works on UDP.

The IAP Catalog is an inventory of the IAP Products implemented in your game. For each IAP Product, you define a:

- Description
- Price
- Consumable type
- Product ID

Your game, once repacked and published on a store, will query its IAP inventory from the store’s back-end. The store is given your game’s IAP Catalog via UDP. When players make in-game purchases, your game asks the store to confirm the IAP Catalog. UDP must be properly implemented in your game for this step to work smoothly.

With UDP, the IAP Catalog you define in the Unity Editor is synced with the UDP Console (web portal). And when your game is repacked and submitted to a store, the IAP Catalog is synced with the store’s back-end:

![img](images/image_1.png)

The **IAP Catalog on UDP Console** is the keystone.

Unity is responsible for the sync between the UDP Console and the store’s back-end.

**The IAP Catalog implemented in the Editor must sync properly with the UDP Console**. This relies on your implementation and requires that you closely follow the UDP implementation guidance.

A misguided implementation could break the IAP Catalog sync between the game client and the server, and typically result in symptoms in the final game such as:

- The wallet doesn’t appear when invoked
- IAPs are unresponsive

This would mean your game could not be monetized. 

### How to implement UDP

You can implement UDP in your game in the following ways:

- **Using Unity IAP**. If your game is already using Unity IAP, it should be the obvious choice
- **Using the UDP Package**. This implementation is similar to the Google Play In-App Billing implementation, if your game was originally wired that way then porting it to UDP should be straightforward

You can choose either implementation, but you must stick to **ONLY ONE**. For more information, see [Don't mix the implementations](Do_not_mix_the_implementations.md). 

Below is a quick run-down of both options.

##### Using Unity IAP

From version 1.22 and above, you can implement UDP directly from Unity IAP ([Setting up UDP](Using_Unity_IAP_detail.md)).

If your game is already using Unity IAP, it should be the obvious and simplest choice.

Make sure you follow the general implementation guidelines in [Unity IAP’s Documentation](https://docs.unity3d.com/Manual/UnityIAP.html).

Check the details of the [Editor UI elements involved in implementing UDP via Unity IAP](Editor_UI_elements_for_UDP_implementation_via_Unity_IAP.md).

##### Using the UDP Package

The UDP Package is a standalone package that is independent from Unity IAP.

The current version is 1.2.0 ([see where to get the UDP Package](UDP_Package.md)).

It also has a [notion of IAP Catalog](Before_you_begin_know_this.md), so that your IAP Products can be synced with the UDP Console and then with the stores. 

This implementation is similar to the Google Play IAB implementation, so if your game was wired that way porting it to UDP should be straightforward. The overall steps required are:

1. Explicitly initialize UDP.
2. Query the store’s IAP inventory.
3. Request to purchase a product.
4. Consume the purchase.

For more information on these steps, see [implementing UDP IAP on the client side](Client-side_implementation_of_UDP.md).

Check the details of the [Editor UI elements involved in implementing the UDP Package](Editor_UI_elements_for_UDP_implementation_via_UDP_Package.md). 