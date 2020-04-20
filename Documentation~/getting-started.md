# Getting Started

## UDP Journey

A typical UDP implementation consists of the following steps:

1. **Implement UDP in your Unity project**.<br/>
Set up and configure UDP in the Unity Editor, implement UDP in your game project, and populate your **IAP Catalog** with your in-app purchase products (if any).
2. **Build and deploy your game to UDP**.<br/>
Build your UDP Android package (apk), test it in the UDP Sandbox environment, and deploy it to the UDP console where you’ll begin preparing your game for submission to the stores.
3. **Edit your game information**.<br/>
On the [UDP console](https://distribute.dashboard.unity.com), provide app store listing information and resources for your game, such as multi-language descriptions, multi-currency price points, screenshots, video trailers, and create releases for distribution.
4. **Prepare your game for submission**.<br/>
Sign up with the stores using your UnityID, and register your game with the app stores directly from inside the UDP console.
5. **Publish to stores**.<br/>
Select the stores you want to submit your game to. UDP automatically repacks your game into store-specific builds and submits them to the stores along with the relevant game descriptions. 
6. **Track game performance**.<br/>
When your game is published, monitor its performance across the different stores from the UDP reporting dashboard.

<a name="how-to-implement"></a>
## How to implement UDP

You can implement UDP in your game in one of the following two ways.

* Using Unity IAP
* Using the UDP Package 

<a name="using-iap"></a>
### Using Unity IAP

If your game already uses Unity IAP, this should be the obvious choice. 

UDP is included in Unity IAP from version 1.22. 

Follow the general implementation guidance of [Unity IAP’s Documentation](https://docs.unity3d.com/Manual/UnityIAP.html) before you begin [Game client implementation with Unity IAP](games-with-iap.html#with-unity-iap).

<a name="using-udp"></a>
### Using the UDP Package

This implementation is similar to the Google Play In-App Billing implementation, so if your game was originally wired that way then porting it to UDP should be straightforward. 

The UDP package is readily available from Unity Package Manager or from the Unity Asset Store; its current version is 1.3.0. 

Implementation guidance is given in [Game client implementation with the UDP Package](games-with-iap.html#with-udp).

## Ownership

UDP games belong to a [Unity Organization](https://docs.unity3d.com/Manual/OrgsManagingyourOrganization.html) and not to any individual user. All users of an Organization have access to its UDP games. Permissions vary depending on the User / Manager / Owner role of a given user within the Organization. See [UDP role-based permissions](faq.html#org-permissions) for details. Project-level permissions are not yet supported.

You can divide tasks within a Unity Organization between users and non-users of the Unity Editor; for example:

* **Publishing Manager** (not an Editor user) 
    * Creates a new game on the UDP console 
    * Passes **Developer** the parameters needed to carry out the UDP implementation
    * Consolidates the material required for distribution
    * Begins signing up with the stores the Organization wants to distribute its games to
* **Developer** (Editor user)
    * Implements UDP in the project
    * Builds and tests the game APK
    * Deploys the game build to the UDP console
* **Publishing Manager** 
    * Creates game releases
    * Finalizes the submissions to the stores