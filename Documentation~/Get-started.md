# **Getting Started**

There are two ways to implement UDP in your game:

* **UDP package**

    The UDP package is in preview and readily available from [Unity Package Manager](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@2.1/manual/index.html).

* **UDP with Unity IAP**

    UDP is included in Unity IAP from version 1.22. To use UDP with Unity IAP, [enable Unity IAP from the Editor’s service window](https://docs.unity3d.com/Manual/UnityIAPSettingUp.html). 

Regardless of which way you choose, follow these steps:

1. In the Unity Editor, [configure UDP and create your game’s catalog of in-app purchases](Configuring-UDP.md).

   Create a new UDP game client via the **UDP Settings** window, populate your **IAP Catalog**, referencing your actual in-game products.

2. [Build and deploy your game to UDP](Building-pushing-to-the-UDP-console.md).

   Build your UDP Android package locally or via Cloud Build and upload or deploy it to the UDP console.

3. [Edit your game information](Editing-game-info.md).

   On the [UDP console](https://distribute.dashboard.unity.com), provide app store listing information and resources for your game, such as multi-language descriptions, screenshots, video trailers, and create releases for distribution.

4. [Prepare your game for submission](Publishing-to-stores.md).

   Register your game with the app stores directly from inside the UDP console and push your game to your beta users.

5. [Publish to stores](Publishing-to-stores.md).
   Select the stores you want to submit your game to. UDP automatically repacks your game and submit specific builds to each store. Once your game is published, monitor its performance from the UDP reporting dashboard.