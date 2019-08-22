# Getting Started

There are two ways to implement UDP in your game:

- **UDP package**
  The UDP package is in preview and readily [available from Unity Package Manager or from the Unity Asset Store](UDP_Package.md).
- **UDP with Unity IAP**
  UDP is included in Unity IAP from version 1.22. To use UDP with Unity IAP, [enable Unity IAP from the Editor’s service window.](Finding_and_navigating_UDP_in_the_Editor.md)

Refer to [this section](Before_you_begin_know_this.md) to understand the difference between the 2 options, which one suits you best, and more details on getting set up with either one. Once your implementation choice is made, these will be your next steps:

1. **In the Unity Editor, [configure UDP](Configuring_Unity_Distribution_Portal.md) and create your game’s catalog of[ in-app purchases](Implementing_UDP_in-app_purchases.md).**
   Create a new UDP game client via the **UDP Settings** window, implement UDP in your game, and populate your **IAP Catalog** with your in-game products.
2. [**Build and deploy your game to UDP**](Building_your_game_and_deploying_it_to_the_UDP_console.md)
   Build your UDP Android package, test it in the UDP Sandbox environment, and deploy it to the UDP console.
3. **Edit your** [**game information**](Editing_your_game_information_on_the_UDP_console.md).
   On the [UDP console](https://distribute.dashboard.unity.com), provide app store listing information and resources for your game, such as multi-language descriptions, screenshots, video trailers, and create releases for distribution.
4. [**Prepare your game for submission**](Publishing_your_game_to_stores.md).
   Register your game with the app stores directly from inside the UDP console and push your game to your beta users.

5. [**Publish to stores**](Monitoring_submission_statuses_and_downloading_builds.md).
   Select the stores you want to submit your game to. UDP automatically repacks your game and submit specific builds to each store. Once your game is published, monitor its performance from the UDP reporting dashboard.

