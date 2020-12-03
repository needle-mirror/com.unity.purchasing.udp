# Getting Started with the UDP package

This section explains how to install and start using the UDP package in your Unity project.

**Note**: If you want to use the UDP package and the Unity IAP package, see [UDP implementation](https://docs.unity3d.com/Manual/udp-getting-started.html#how-to-implement).

<a name="install"></a>
## Installing the UDP package

You can install the UDP package from:
* The [Package Manager](https://docs.unity3d.com/Manual/Packages.html)
* The [Asset Store](https://assetstore.unity.com/packages/add-ons/services/billing/unity-distribution-portal-138507)

### Installing the UDP package from the Package Manager

To install the UDP package via the Package Manager:

1. In the Unity Editor, select **Window** > **Package Manager**.
1. In the Packages filter select **All Packages**.
1. Select the Unity Distribution Portal package and select **Install**.

### Installing the UDP package from the Asset Store

To install the UDP package from the Asset Store:

1. Download the UDP package from the [Asset Store](https://assetstore.unity.com/packages/add-ons/services/billing/unity-distribution-portal-138507).
1. In the Unity Editor, select **Assets** > **Import Package** > **Custom Package**.
1. Select the package and select **Open** > **Import**.

<a name="linking"></a>
## Linking your Unity project to UDP

To be able to use the UDP Settings window, you need to link your Unity project to a UDP Client. 

Prerequisites

You have:

* [Created a game in the UDP console](https://docs.unity3d.com/Manual/udp-distribution.html)
* [Set up your project for Unity Services](https://docs.unity3d.com/Manual/SettingUpProjectServices.html)

To link your Unity project to a UDP client:

1. In the Unity Editor, select **Window** > **Unity Distribution Portal** > **Settings**.
1. In the UDP Client ID field, enter your UDP Client ID.
<br/> The Client ID is displayed in the Integration Information section of the Game Info page in the UDP console.
1. Select **Link project to this UDP game**.
<br/>This creates a link between the Unity project and the UDP client.

Your project is now set up to use UDP. The relevant settings from the UDP console are included in the UDP Settings window. These settings are stored in the UDPSettings.asset file. 

### Creating a UDP client ID from the Unity Editor

Unity recommends creating your UDP client in the UDP console and linking it to your project, as described above.
Alternatively, to create a UDP client from the Unity Editor:

1. In the Unity Editor, select **Window** > **Unity Distribution Portal** > **Settings**. 
1. Select **Create your UDP game directly from inside the Unity Editor**.
1. Select **Generate new UDP client**.

When you generate your UDP client, your game is automatically created in the UDP console. This also creates a UDP settings file for your project.

<a name="init"></a>
## Initialize the UDP SDK
To access the UDP SDK, declare the UDP namespace in your game manager script.

```
using UnityEngine.UDP;
```

To initialize the UDP SDK, your game needs to have a UDPSettings.asset file linked to a UDP client.


![Initializing game integration with UDP](Images/5-GamesWithIAP_01.png)<br/>

In your game manager script, define the initialization listener:

```
IInitListener listener = new InitListener();
```

In the Start() function, call the `Initialize` method.

```
StoreService.Initialize(listener);
```

**Note**: This is required to be able to publish your game to app stores via UDP.

The InitListener then returns a success or failure message to inform your game if the initialization was successful.

```
using UnityEngine;
using UnityEngine.UDP;
  
public class InitListener : IInitListener
    {
    public void OnInitialized(UserInfo userInfo)
    {
        Debug.Log("Initialization succeeded");
        // You can call the QueryInventory method here
        // to check whether there are purchases that haven’t been consumed.       
    }

    public void OnInitializeFailed(string message)
    {
        Debug.Log("Initialization failed: " + message);
    }
}
```

<a name="sample"></a>
## Sample scene

The UDP package contains a sample scene, which provides example methods to help you implement and test UDP in your game. The table below describes these example methods.

If you installed UDP from the Asset Store, the sample is included in the package.

If you installed UDP from the Package Manager, the sample is supported in Unity Editor version 2019.1 and above. To import the sample scene, go to Package Manager > Unity Distribution Portal and select **Samples** > **Import**.

|Method|Description|
|---|---|
|Init|Initializes the UDP SDK. On successful initialization, you will be asked to login with your sandbox credentials. This simulates a player opening the game.|
|Buy|Attempts to purchase an IAP product in the game. Returns whether the purchase succeeded or failed.|
|Buy & consume|Purchases a product and consumes it. Returns whether or not the IAP product was consumed.|
|Query inventory|Queries the IAPs configured in the game, and the IAPs you’ve |urchased but not consumed.|
|Query & consume|Consumes all unconsumed IAPs. This lets you return to the riginal status after the game is initialized.|