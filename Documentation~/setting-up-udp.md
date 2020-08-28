# Setting up UDP   

So you’ve chosen between using the UDP Package or using Unity IAP (see [How to implement UDP](getting-started.html#how-to-implement)).

1. Open the Unity Editor and log in with a Unity account. If you don’t have a Unity account yet, create one in [https://id.unity.com](https://id.unity.com). 
2. Create or open a Unity project in the Editor.
3. Set up UDP according to your choice: UDP Package, or Unity IAP implementation.

<a name="install"></a>
## Installing the UDP Package

You can install the UDP package from:

* The Package Manager
* The Asset Store

<a name="packman-install"></a>
### Installing the UDP package from the Package Manager

1. In the Unity Editor, select **Window** > **Package Manager**.
2. In the Packages filter select **All Packages**.
3. Select the Unity Distribution Portal package and select **Install**.

Learn more about the [Unity Package Manager](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@2.1/manual/index.html).

### Installing the UDP package from the Asset Store

If you don’t have access to Unity Package Manager, download the **UDP Package** from the [Unity Asset Store](https://assetstore.unity.com/packages/add-ons/services/billing/unity-distribution-portal-138507) and install it in your project.

<a name="install-with-iap"></a>
## Installing UDP via Unity IAP

Unity IAP versions 1.22.0-1.23.5 include the UDP package. If using these versions of the Unity IAP package, you just need to [enable Unity IAP](https://docs.unity3d.com/Manual/UnityIAPSettingUp.html).

To check which Unity IAP version is installed, go to **Window** \> **Unity IAP** \> **IAP Updates**.

## Installing the UDP package and the Unity IAP package

The Unity IAP package version 2.0.0 and above does not contain the UDP DLL. This requires the UDP package version 2.0.0 and above. From these versions on, [install the UDP package](#install) and install the Unity IAP package from the Asset Store.


<a name="navigate"></a>
## Navigating the UDP console

The [UDP console](https://distribute.dashboard.unity.com) is a web-based portal where you can prepare your games for submission to multiple app stores. The UDP console lets you:

* Consolidate and manage your game's information
* Edit your game's in-app purchases
* Publish your game to multiple app stores 
* View your games’ performance across all UDP stores

Access the UDP console via the URL [https://distribute.dashboard.unity.com](https://distribute.dashboard.unity.com). 

To access the UDP console from the Unity Editor, select **Window** > **Unity Distribution Portal**> **Settings**. In the **UDP Settings** inspector window, select **Go to UDP console**.

This section provides a high-level overview of the UDP console’s information architecture.

The UDP console contains a navigation bar that lets you navigate between the following sections of the UDP console:

* **My Games**
* **Reporting**
* **Partner Stores**
* **Resources**

### My Games 

The **My Games** tab displays your UDP projects. From here you can switch between UDP projects, [create a new game](creating-a-game-on-udp.html#udp-new-game), and [archive and restore games](best-practices.html#archive).

To view the **Game Info** page for a game, select the card in the **My Games** tab. The **Game Info** page lets you [edit your game information](managing-and-publishing-your-game.html#edit-info), and [publish your game and view its status](managing-and-publishing-your-game.html#publish).

Each game card displays the status of the game. The **Published** status indicates the game has completed all of the available steps to be repacked and submitted to the store.

### Reporting

The [**Reporting**]((reporting-dashboard.md)) tab displays performance information for your published games.

<a name="partner"></a>
### Partner Stores

The **Partner Stores** tab displays information about the stores you can distribute your game to using UDP.

![](Images/3-SettingUp_07.png)

Click on the **More** link for more information on a store, such as overview, FAQs, and other useful links specific to each store.

## Resources

The **Resources** tab displays useful information to help you get started, including:

* A link to the UDP documentation
* A Get Started guide
* Guides to different app stores