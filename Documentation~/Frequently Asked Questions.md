# Frequently Asked Questions 

## Editor implementation / SDK

#### Which Unity versions does UDP support?

Unity 5.6.1 and above. We recommend using 2018.x as that’s where UDP uses the latest instrumentation and editor functionalities.

#### Should I use the UDP Package or implement via Unity IAP?

If your game already uses Unity IAP you are better off implementing UDP with Unity IAP.

The UDP Package implementation is similar to that of Google Play in-app Billing, and is recommended  if you are porting from such a project, or simply if you’re more familiar with that implementation.

In terms of output, both will let you build a Android package for UDP. The implementation is however different. 

More information on this topic [here](Two different ways to implement UDP.md)

#### As UDP is available in Unity IAP from version 1.22, how do I check that I have Unity IAP 1.22 and above?

In the Unity Editor, go to the top menu and select **Window > Unity IAP > IAP Updates**. The window that pops up shows the version number of your Unity IAP package. Check [here](How do I get Unity IAP set up with UDP.md) for more details

#### How do I know that UDP is installed?

If you’re implementing the UDP package, check [this section](Editor UI elements for UDP implementation via UDP Package.md).

If you’re implementing via Unity IAP, check [that section](Editor UI elements for UDP implementation via Unity IAP.md).

Check also [this section of the Troubleshooting guide](Do not mix the implementations.md) to make sure you havent mixed implementations up.

#### Do I need to enable Unity Analytics to use UDP?

Yes. The UDP console reports IAP and game start metrics which are routed through our Analytics instrumentation.

#### When is a good time to implement UDP in my game development cycle?

Once you have settled what your game’s purchasable in-app products will be. That’s typically towards the end of the dev cycle. It also means it’s easy to implement UDP in your back-catalogue games to give them a new lease of life on new app stores.

####I encounter an error message “Error: Permission Denied” when trying to generate the game client” in UDP Settings. What’s the problem?

Generating the client needs you to be the Owner of the organization. Manager and User can retrieve the client but cannot generate it.

#### Do I need to rebuild my game when stores update their SDKs, or when new stores become available on UDP?

No. The UDP build has a generic IAP implementation, like a placeholder. When you push your game to stores, we repack the generic build with the store-specific SDK. We take care of which store SDKs to repack your game with, so you only need to focus on a single implementation. However, for any given store, you can choose to repack your game for a specific SDK target (Advanced settings on the UDP console).

#### Can I only set IAP prices in USD?

In the Editor’s IAP Catalog, yes. But when you’ll prepare your game for submission on the UDP console, you will be able to set different IAP prices in different currencies. These new currencies will however not sync with the Editor component, which will always only reflect the USD price. 

Note: as of March 2019 the stores connected to UDP only accept IAP prices in USD and we therefore disabled other currencies until the setting becomes relevant.

#### How do I build my UDP package?

The same way you would for an Android package. If you are implementing via Unity IAP, make sure you choose UDP as the build target. You can build locally (build window) or via CloudBuild. 

#### How do I deploy my game to the UDP console?

There are 3 ways:

- you build locally, upload to CloudBuild, deploy via CloudBuild
- you build via CloudBuild, and deploy via CloudBuild
- you build locally, and upload your APK directly on the UDP console

#### If I don’t have a paid CloudBuild subscription, is my only option to build locally and upload on the UDP console?

No. The subscription is only required to build via CloudBuild. If you want to Upload to CloudBuild, and then deploy via CloudBuild, you don’t need a paid CloudBuild subscription – it is free.

#### Can I give my UDP build to stores directly, bypassing the Portal part?

No. This build is only an artefact created during the course of the UDP build-and-submit process. It only works with the Unity sandbox. It is calling stubbed payment APIs and is not capable of making any transaction on any of the UDP stores’ billing systems. To be used, this build needs to be repacked with the store’s specific SDK. This is automatically done on the UDP console during the game submission process.