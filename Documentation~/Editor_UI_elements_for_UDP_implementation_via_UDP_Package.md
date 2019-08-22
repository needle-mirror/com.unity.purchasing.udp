### Editor UI elements for UDP implementation via UDP Package

So you [installed the UDP Package](UDP Package.md).

You should ONLY have “Unity Distribution Portal” in the **Window** menu.

(as in, you should NOT have Window > Unity IAP)
![img](images/image_51.png)

Everything you need is in the UDP Settings inspector window:

**Window > Unity Distribution Portal > Settings**
![img](images/image_52.png)

The IAP Catalog for UDP is directly included in the UDP Settings window:
![img](images/image_53.png)

Make sure that when you add / change IAP Products, you save them to the UDP Console by using the PUSH functions:
![img](images/image_54.png)

The top **Push** button will sync everything with the UDP console (all IAP Products, Game Title, Settings, Test Accounts)

The Product-specific **Push** will only sync the information about that IAP Product.

The top **Pull** button will retrieve the latest UDP Settings that were saved on the UDP Console (all IAP Products, Game Title, Settings, Test Accounts). It will also override any unsaved inputs in your Editor window.

**Don’t forget to PUSH your IAP Product changes once you’re done, and BEFORE you go on to build your game client.**

Keep an eye out for any unsaved changes:
![img](images/image_55.png)

The “edited” label disappears once your IAP Product is synced.

Remember that in the case of an implementation via UDP Package, you need to explicitly implement in your game the IAP-related steps explained in [this section of the UDP Documentation](Client-side implementation of UDP.md).

