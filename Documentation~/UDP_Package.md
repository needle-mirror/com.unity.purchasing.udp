# UDP Package

### How do I get the UDP Package?

**From the Package Manager**

From the Unity Editor’s top menu: **Window > Package Manager:**
![img](images/image_49.png)

Select the tab **All** and look for Unity Distribution Portal:
![img](images/image_50.png)

Make sure in the **Advanced** menu that you have **Show preview packages** selected:

![img](images/image_77.png)

Learn more about the [Unity Package Manager](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@2.1/manual/index.html).

**From the Asset Store**

No access to Unity Package Manager? No problem. 

Download the **UDP Package** from the [Unity Asset Store](https://assetstore.unity.com/packages/add-ons/services/billing/unity-distribution-portal-138507) and install it in your project.

### Editor UI elements for UDP implementation via UDP Package

So you [installed the UDP Package](UDP_Package.md).

You should only have "Unity Distribution Portal" in the **Window** menu.

(That is, you should not have Window > Unity IAP)

![img](images/image_51.png)

Everything you need is in the **UDP Settings** inspector window:

**Window > Unity Distribution Portal > Settings**
![img](images/image_52.png)

The **IAP Catalog** for UDP is directly included in the **UDP Settings** window:
![img](images/image_53.png)

When you add / change IAP Products, make sure you save them to the UDP Console by using the PUSH functions:

![img](images/image_54.png)

The top **Push** button syncs everything with the UDP console (all IAP Products, Game Title, Settings, Test Accounts)

The Product-specific **Push** only syncs the information about that IAP Product.

The top **Pull** button retrieves the latest UDP Settings that were saved on the UDP Console (all IAP Products, Game Title, Settings, Test Accounts). It also overrides any unsaved inputs in your Editor window.

**Don’t forget to PUSH your IAP Product changes once you’re done, and BEFORE you go on to build your game client.**

Keep an eye out for any unsaved changes:
![img](images/image_55.png)

The “edited” label disappears once your IAP Product is synced.

Remember that in the case of an implementation via UDP Package, you need to explicitly implement in your game the IAP-related steps explained in [Implementing UDP IAP on the client side](Client-side_implementation_of_UDP.md).

