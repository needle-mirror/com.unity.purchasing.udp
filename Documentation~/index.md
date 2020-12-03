# Unity Distribution Portal

## Overview

Unity Distribution Portal (UDP) lets you distribute your Android games to multiple app stores through a single hub. UDP repacks your Android build with each store’s dedicated In-App Purchase SDK. You can manage all your store submissions from the [UDP console](https://docs.unity3d.com/Manual/udp.html).

## UDP package

The UDP package contains an SDK for working with UDP. It also enables the [UDP Settings](udp-package-reference.html#udp-settings) window in the Unity Editor.

You can use the UDP Settings window to:

* [Link your Unity project to the UDP client](getting-started.html#linking)
* [Create a catalog of IAP products](games-with-iap.html#iap-catalog) for your game
  <br/>For more flexibility, [create your IAP products in the UDP console](https://docs.unity3d.com/Manual/udp-distribution.html#create).

Additionally, the UDP package provides:

* A [sample scene](getting-started.html#sample) to help you get started with UDP
* A Sandbox environment where you can [test your game](building-your-game.html#testing) for UDP

## UDP journey

The overall steps required to [distribute a game with UDP]((https://docs.unity3d.com/Manual/udp-distribution.html)) are as follows:

1. Creating a game in the UDP console.
    1. Entering your game info on the UDP console.
    1. Defining supported languages.
1. Implement UDP in your Unity project.
    1. [Install the package](getting-started.html#install).
    1. [Link your project to the UDP client](getting-started.html#linking).
    1. [Initialize the UDP SDK](getting-started.html#init).
    1. [Implement IAP](games-with-iap.md) (if applicable).
    1. [Implement LicenceCheck for premium games](premium-games.md) (optional).
1. Build and test your game.
    1. Build your game APK.
    1. Ensure all your IAP products are listed in the UDP console (if applicable).
    1. Test your game in the Sandbox.
1. Upload your game APK to the UDP console.
1. Finalize the game information page in the UDP console.
    1. Verify testing was successful.
    1. Upload your app signing private key.
    1. Set a premium price (if applicable).
    1. Localize your game information for additional languages (if applicable).
1. Release your game on UDP.
1. Publish your game to stores.

The image below illustrates the overall workflow.

![](Images/1-UDPWorkflow.png)

## System requirements

UDP is supported in Unity 5.6.1 or higher. Unity recommends to use 2018.4 or above.

You can implement UDP in your game in one of the following ways:

* Using the UDP package only (see [Installing the UDP package](getting-started.html#install))
* Using the UDP package and Unity IAP package
* Using Unity IAP only

The implementation you choose does not affect the UDP console.

## Using the UDP package and the Unity IAP package

From the following package versions and above, you can use the UDP and Unity IAP packages together:

* UDP - 2.0.0
* Unity IAP - 2.0.0 (Asset Store version)

The Unity IAP package version 2.0.0 and above does not contain the UDP DLL. This requires the UDP package version 2.0.0 and above. From these versions on, install both the UDP package and the Unity IAP package.

## Using Unity IAP only

Unity IAP versions 1.22.0 - 1.23.5  include the UDP package. If using these versions of the Unity IAP package, you just need to enable Unity IAP.
To check which Unity IAP version is installed, go to **Window** > **Unity IAP** > **IAP Updates**.