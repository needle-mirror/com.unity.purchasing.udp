# Getting Started

There are two ways to implement UDP in your game:

- **UDP package**
  The UDP package is in preview and readily [available from Unity Package Manager or from the Unity Asset Store](How do I get the UDP Package.md).
- **UDP with Unity IAP**
  UDP is included in Unity IAP from version 1.22. To use UDP with Unity IAP, [enable Unity IAP from the Editor’s service window.](How do I get Unity IAP set up with UDP.md)

Refer to [this section](Two different ways to implement UDP.md) to understand the difference between the 2 options, which one suits you best, and more details on getting set up with either one. Once your implementation choice is made, these will be your next steps:

1. **In the Unity Editor, [configure UDP](Configuring Unity Distribution Portal.md) and create your game’s catalog of[ in-app purchases](Implementing UDP in-app purchases.md).**
   Create a new UDP game client via the **UDP Settings** window, implement UDP in your game, and populate your **IAP Catalog** with your in-game products.
2. [**Build and deploy your game to UDP**](Building your game and deploying it to the UDP console.md)
   Build your UDP Android package, test it in the UDP Sandbox environment, and deploy it to the UDP console.
3. **Edit your** [**game information**](Editing your game information on the UDP console.md).
   On the [UDP console](https://distribute.dashboard.unity.com), provide app store listing information and resources for your game, such as multi-language descriptions, screenshots, video trailers, and create releases for distribution.
4. [**Prepare your game for submission**](Publishing your game to stores.md).
   Register your game with the app stores directly from inside the UDP console and push your game to your beta users.

5. [**Publish to stores**](Monitoring submission statuses and downloading builds.md).
   Select the stores you want to submit your game to. UDP automatically repacks your game and submit specific builds to each store. Once your game is published, monitor its performance from the UDP reporting dashboard.

