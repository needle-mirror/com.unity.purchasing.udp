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

You can implement UDP in your game in one of the following ways.

* Using Unity IAP only (for Unity IAP package versions 1.22.0-1.23.5)
* Using the UDP Package only
* Using the UDP package and Unity IAP package (for Unity IAP package versions 2.0.0+)

**Note**: Prior to Unity IAP 2.0.0, the package contained a UDP DLL. This meant that installing the Unity IAP package also installed the UDP package. From Unity IAP version 2.0.0, the UDP DLL is not included. Unity recommends using the UDP package along with the Unity IAP package version 2.0.0+, available from the [Asset Store](https://assetstore.unity.com/packages/add-ons/services/billing/unity-iap-68207).

<a name="using-iap"></a>
### Using Unity IAP

If your game already uses Unity IAP, this should be the obvious choice. 

UDP is included in Unity IAP from version 1.22.0-1.23.5. If you use the Unity IAP package (1.22.0-1.23.5) do not install the UDP package separately.

Follow the general implementation guidance of [Unity IAP’s Documentation](https://docs.unity3d.com/Manual/UnityIAP.html) before you begin [Game client implementation with Unity IAP](games-with-iap.html#with-unity-iap).

<a name="using-udp"></a>
### Using the UDP Package

This implementation is similar to the Google Play In-App Billing implementation, so if your game was originally wired that way then porting it to UDP should be straightforward. 

The UDP package is available from Unity Package Manager or from the Unity Asset Store. 

Implementation guidance is given in [Game client implementation with the UDP Package](games-with-iap.html#with-udp).

## Ownership

UDP games belong to a [Unity Organization](https://docs.unity3d.com/Manual/OrgsManagingyourOrganization.html) and not to any individual user. All users of an Organization have access to its UDP games. Permissions vary depending on the User / Manager / Owner role of a given user within the Organization.

You can also add users, who aren't in the Organization, to specific projects. Add users in the Unity Dashboard under **Project** > **Settings** > **Users**.

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

### Project-related permissions
Members of an organization and individuals granted access to a project can both work on Unity projects.

Project-related permissions relate to what UDP features you have access to on a specific Unity project, both in the Unity Editor and in the UDP console. This applies to:

* Members of the organization that the project belongs to (with organization-level permissions)
* Individuals granted access to the project only (with project-level permissions)

The table below lists the project-related UDP permissions for Users, Managers and Owners in the Unity Editor. These are the same for both project-level and organization-level permissions.

||User|Manager|Owner|
|---|---|---|---|
|Generate a new UDP client|Yes|Yes|Yes|
|Link a Unity project to the UDP client|Yes|Yes|Yes|
|Modify UDP settings|Yes|Yes|Yes|
|Create or modify IAPs|Yes|Yes|Yes|

The table below lists the project-related UDP permissions for Users, Managers and Owners in the UDP console. These are the same for both project-level and organization-level permissions.

||User|Manager|Owner|
|---|---|---|---|
|Generate a new UDP client|Yes|Yes|Yes|
|Archive a game in game list|No|Yes|Yes|
|Edit a game revision|Yes|Yes|Yes|
|Link a Unity project with a UDP client|Yes|Yes|Yes|
|Release a game revision|No|Yes|Yes|
|Register a game to a store|No|Yes|Yes|
|Publish a game to a store|No|Yes|Yes|
|Advanced page operation|No|Yes|Yes|
|Status page access and operation|No|Yes|Yes|

### Organization-related permissions
Organization-related permissions relate to what UDP features you have access to in the Organization. These features are generally restricted to Organization members only, that is, individuals granted access only to specific projects do not have organization-level permissions. The exceptions to this are:

* the project Owner can view the Reporting dashboard
* any project role can view the projects they have access to in the game list

The table below lists additional Organization-related permissions for Users, Managers and Owners.

||Project-level|||Org-level|||
|---|---|---|---|---|---|---|
||User|Manager|Owner|User|Manager|Owner|
|View the Reporting dashboard|No|No|Yes|No|Yes|Yes|
|Access the game list|Yes*|Yes*|Yes*|Yes|Yes|Yes|
|Edit the Company profile|No|No|No|No|Yes|Yes|
|Sign up the Organization to a store|No|Yes|Yes|No|Yes|Yes|

**Note**: Project-level users can assess the games within the host organization that owns the project, and any other projects they have access to within their own Organizations.