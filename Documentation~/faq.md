# Frequently Asked Questions 

## Editor implementation of UDP

#### Which Unity versions does UDP support?

Unity 5.6.1 and above, though we recommend that you choose a version covered by Unity support (2018.4 and above as of February 2020).

#### Should I use the UDP Package or implement via Unity IAP?

If your game already uses Unity IAP you are better off implementing UDP with Unity IAP.

The UDP Package implementation is similar to that of Google Play In-App Billing, and is recommended if you are porting from such a project, or simply if you’re more familiar with that implementation.

In terms of output, both will let you build an Android package for UDP, however the implementation is different. 

For more information on this topic, see [How to implement UDP](getting-started.html#how-to-implement).

#### As UDP is available in Unity IAP from version 1.22, how do I check that I have Unity IAP 1.22 and above?

In the Unity Editor, go to the top menu and select **Window** > **Unity IAP** > **IAP Updates**. The window that pops up shows the version number of your Unity IAP package. For more information, see [How do I get Unity IAP set up with UDP?](setting-up-udp.html#install-with-iap)

#### How do I know that UDP is installed?

You should have Unity Distribution Portal show in the Window menu of the Unity Editor. For more details refer to [Setting up UDP](setting-up-udp.md).

#### Do I need to enable Unity Analytics to use UDP?

Not necessarily. If you implement with the UDP Package, you don't have to. If you implement UDP through Unity IAP, Analytics is a prerequisite for UnityIAP and needs to be enabled.

#### When is a good time to implement UDP in my game development cycle?

Once you have decided what your game’s purchasable in-app products will be. That’s typically towards the end of the dev cycle. It also means it’s easy to implement UDP in your back-catalogue games to give them a new lease of life on new app stores.

#### Do I need to rebuild my game when stores update their SDKs, or when new stores become available on UDP?

No. The UDP build has a generic IAP implementation, like a placeholder. When you push your game to stores, we repack the generic build with the store-specific SDK. We take care of which store SDKs to repack your game with, so you only need to focus on a single implementation. However, for any given store, you can choose to repack your game for a specific SDK target (Advanced settings on the UDP console).

#### Can I link my Unity project to another UDP client?

Yes, but you need to create a new project ID, because the current one is linked to your existing UDP client irrevocably. Begin in the Unity Editor:

1. Under **Service** > **Settings** > **Unity Project ID**: click **Unlink project**.
2. Under **Service**: select your organization, and create a new Unity project ID.
3. Under **Assets/Plugins/UDP/Resources**, delete the file UDP `Settings.asset`.
4. Co to **Window** > **Unity Distribution Portal** > **Settings** to reload the UDP settings.
5. Choose **link to existing UDP client** and paste your new UDP client ID (found on the UDP Console under *Game Info > Integration Information*: the field "Client ID").

#### Can I only set IAP prices in USD?

In the Unity Editor’s IAP Catalog, yes. But when you prepare your game for submission to the stores on the UDP console, you can set IAP prices in different currencies. The UDP console lets you convert your prices from USD to local currencies, and upload your IAP items in bulk using a CSV file where prices in multiple currencies can be set up.

However, these local currencies do not sync with the Unity Editor, which only reflects the USD price. 

#### How do I build my UDP package?

The same way you would for an Android package. If you are implementing via Unity IAP, make sure you choose UDP as the build target. 

#### How do I deploy my game to the UDP console?

You can deploy your game to the UDP console in the following ways:

* Build locally, and upload your APK directly on the UDP console
* Build locally, upload to CloudBuild, deploy via CloudBuild
* Build via CloudBuild, and deploy via CloudBuild

#### Can I give my UDP build to stores directly, bypassing the UDP console part?

No. This build is only an artefact created during the course of the UDP workflow. It only works with the Unity sandbox. It is calling stubbed payment APIs and is not capable of making any transaction on any of the UDP stores’ billing systems. To be used, this build needs to be repacked with the store’s specific SDK. This is automatically done on the UDP console during the game submission process.

## The UDP console

#### How do I access the UDP console?

This is the URL: [https://distribute.dashboard.unity.com/](https://distribute.dashboard.unity.com/). You can also find a link to the UDP console in the **UDP Settings** inspector window, as well as in the Cloud Build panel for the games which were successfully pushed to UDP.

#### Is the UDP console workflow different for implementations via UDP Package or UDP with Unity IAP?

No. Both implementations give you a generic UDP build. This build is uploaded to the UDP console, and it’s the same workflow going forward. 

#### Is the "Game Information" data going to be used for all the stores I’ll submit to?

Yes that’s the idea. We recommend that you enter all the languages, assets and currencies relevant to your distribution plans at this stage, so that everything is set when you select the stores you will distribute your game to. We recommend you build a superset of assets that can easily address all stores; if anything is missing UDP will flag it during the Publish process and before your submission goes through.

#### Which information is synced between the Unity Editor and the UDP console?

The Game Title, all the IAP information, the integration information (specifically the Callback URL) and the sandbox test accounts. Note that for the IAPs, only the USD price is synced.

#### Can I tweak submissions for specific stores?

Yes, but only for prices, not for game metadata. You can do that in the **Publish** tab; each store features an **Advanced** section where you can make price changes *before* you submit your game.

If you want to submit different game metadata to different stores, read the next FAQs about the Release system and Release labels.

#### Will my screenshots be compatible for all stores?

The UDP console screens your uploaded images to ensure they are compatible with the stores’ requirements. You will be notified if they do not meet the requirements. It is recommended to at least upload 4 screenshots in portrait orientation, and 4 screenshots in landscape orientation.

#### Should I provide videos in mp4 format or Youtube link?

We suggest you do both. Some stores take mp4 files, others only take Youtube URLs. Maximize your coverage by having both if possible.

#### I’m in the UDP console, and I want to change my IAPs. Must I rebuild the game?

No. If you change them in the general Game Info section of the UDP console, the IAP changes will be synced with your Unity project. In the Editor you will see the new info in the IAP Catalog (you may have to "pull IAP information" if the project was open in the Unity Editor while changes were made on the UDP console). Note that only the USD amounts will be synced. 

#### Can I submit Premium games?

Yes. There is a Premium Price field in the Game Information section for the pay-to-download price of your game; for more details see [Premium Games](#premium-games). Note that not all stores accept Premium games; the stores which do, carry a "Pay-to-Download" label (in the **Publish** tab). If you try to submit a Premium game to a store which doesn’t support them, you will be warned before the submission can go through. 

<a name="org-permissions"></a>
#### In my team who can do what on UDP?

Permissions are based on your Organization roles (Owner, Manager, User)

* Both Manager and Owner can generate a new UDP client in the Editor 
* Both Manager and Owner can link a Unity project with a UDP client in the Editor 
* Both Manager and Owner can modify UDP Settings in the Editor 
* Anyone (User, Manager, Owner) can create or modify IAPs in the Editor 
* Both Manager and Owner can create a new game on the UDP console
* Anyone (User, Manager, Owner) can edit a game revision on the UDP console
* Both Manager and Owner can RELEASE a game revision on the UDP console
* Both Manager and Owner can sign up the Organization with a store
* Both Manager and Owner can register a game to a store
* Both Manager and Owner can publish a game to a store
* Both Manager and Owner can view the Reporting dashboard

#### We use several Organizations within our studio. How will that work?

When signing up to a store via UDP, you are mapping that store account to your Organization. If you sign up to a store while being under another Organization, this will create a distinct account on the store side.

#### I signed up to store "X" before, but now it’s asking me to sign up again. Why is that?

Check you’re under the same Organization as when you first signed up. If you’re under a different Organization, the system (and the store) will consider you a new user.

#### Who do the submitted games belong to?

Games are tied to your Organization and are visible to all the users under that Organization.

#### My game is showing up on the UDP console but there are no IAPs. Why is that?

This can happen if you forget to Push your IAP Catalog information from the Editor.

<a name="revisions"></a>
#### How do the Revisions work?

A revision’s key components are your game build (APK file), your IAP catalog, and your game metadata. If you change one of these elements, your store submission will be different, and a new revision must be created. The revision system is incremental (+1 each time) and is managed by UDP. You cannot create custom revision numbers. Revision notes and labels (entered when you release a new revision) will help you keep track of revisions.

#### I pushed a new build, this created a new revision. Will I have to re-enter all the metadata from scratch on the UDP console?

No. Your new revision will inherit all the metadata from the last revision, but will have the latest APK.

#### How do I edit a Revision?

Choose the existing revision that you want to start from (usually the latest). You will inherit all the Game Information data of this revision. "Edit" to make your changes. Saving your changes will only create a Revision Draft. For this draft to become the latest revision, you need to release it (“Release” button). Be sure to enter release notes and a release label to keep track of your revisions. 

Example: there are already 5 revisions. You want to make a new revision based on Revision 3. You select Revision 3, and Edit it. Saving it will create the Revision Draft. Releasing the Revision Draft will create a new Revision 6. 

#### Why is my first revision Revision 3 (or higher)?

If you created your UDP client in the Unity Editor (UDP Settings inspector window) you started at 1. When you set a Game Title, this will increment the revision number to 2 (you updated the game’s metadata). Then once you prepare your game for submission (category, screenshots, description, etc) you are editing revision 2 and preparing a new one. This is why your first submission to stores can be a Revision 3 (or higher, if your preparations involved many iterations).

#### I’m working on a new Revision but my colleague is making changes to the game in the Editor. What’s going to happen?

If a Draft Revision exists (*saved* on the UDP console) it will not be possible to push changes from inside the Editor that would result in revision number increment.

If the Draft Revision has not been saved yet, the Editor colleague will be able to create a new Revision.

<a name="submit-revisions"></a>
#### Can I submit different Revisions to different stores?

Yes - but remember that at any one time you can only submit the last Revision that was released. If you intend to submit different Revisions to different stores, we strongly recommend you use the Release Labels and Notes to easily identify your Revisions.

Example: let’s say you intend to submit a revision for StoreA and another revision for StoresBCD.

* you create Revision 10 for StoreA. When releasing Revision 10, write clear release notes and label your release "StoreA"
* submit Revision 10 to StoreA, and do it before you work on the other revision
* next up, you create a new revision for StoresBCD. The Revision Draft will ultimately become Revision 11. When releasing Revision 11, write clear release notes and label your release "StoresBCD"
* submit Revision 11 to StoresBCD, and do it before you work on any other revision
* later on, you need to submit a new release to StoreA, based on your previous submission to that store. Your Release labels will show that Revision 10 was the last one labelled "StoreA"
* Select Revision10, and Edit it. This will create a Revision Draft based on Revision 10. When you release it, it will however become Revision 12 (since 11 was already released previously) 
* When releasing Revision 12, make sure you perpetuate the "StoreA" label. Submit Revision 12 to StoreA
* and so forth

#### How do the Sandbox Test Accounts work?

Your generic UDP game build runs in Unity’s Sandbox environment, which simulates a store. When you run that build on an emulator or a smartphone, it will ask the user for access credentials. You define and manage these credentials in that section. Learn more about [Sandbox Test Accounts](creating-a-game-on-udp.html#udp-sandbox).

#### What happens if I don't set any Sandbox Test Account?

You will not be able to log into your generic UDP build. Authentication is mandatory.

#### Will the Sandbox Test Accounts also work on my store-specific game builds?

No. The test accounts are only for Unity’s Sandbox environment, and will only apply to the generic UDP build. Once a game is repacked for a specific store, it no longer points to the Sandbox environment where the test accounts belong.

#### Are the Sandbox Test Accounts in the Game Settings inspector window, and the UDP console, the same?

Yes. The Editor and the UDP console will sync changes that you make on either side.

#### What happens if I created my generic UDP build without any Sandbox Test Account, and only added them in the UDP console - do I need to rebuild my game?

No. You can log into this build with the Sandbox Test Accounts defined in the UDP console.

#### When repacking I get an error "Packing Failed: No UDP SDK detected"

This can happen with an implementation via Unity IAP, if you forget to set UDP as the build target.

It can also happen if you use the Minify option while building your APK; UDP may not able to find files/directories that it needs because of it. Keep UDP-related packages in a customised proguard file (or disable Minify option) and rebuild your game.

#### Why do I need to register my game before submitting it?

The UDP console calls the store back-end to fetch the App ID / Key / Secret for the game you are about to submit. This is possible thanks to the back-end integration between UDP and the stores. It saves you from running errands on the store’s dev console to find and retrieve these parameters.

#### What are the two target steps?

- "Repack Game" unpacks the generic UDP build, and repacks your game with the corresponding store SDK. 
- "Submit to Store" pushes the repacked game build to the store’s content vetting system. This is your formal game submission to the app store. We strongly encourage you to test your repacked builds before submitting them to the stores.

#### So if I submit to 10 stores, do you jam 10 different SDKs in my game?

No. If you submit to 10 stores, we clone the generic UDP build 10 times. Then we repack each of these clones with the store’s SDK - and only that one. Basically 10 store-specific builds have been generated - each one using only the store’s specific SDK - but it is transparent to you as UDP handles all that heavy lifting when you set the submission process in motion.

#### What’s the Advanced section for?

This is where you can tweak your game submission for the given store beyond what UDP does by default. For instance, repacking for an older store SDK version, tweaking the prices of your IAPs or your Premium game.

#### I submitted my game with "Advanced" tweaks for a given store. I’m now pushing out an update of the game to the same store. Do I need to redo all the Advanced settings?

Yes, at this stage you have to. UDP checks and optimizes your submission for each store, but it doesn’t re-apply the tweaks you manually did during a previous submission.

#### What changes are made to my build when repacking it for stores?

When your game is repacked for a store, UDP removes the UDP Sandbox implementation and replaces it with the store-specific SDK. The game is then signed again.

Some stores require a suffix to be added to the package name. At time of writing, the following stores modify the package name: Samsung Galaxy Store (.gs), Mi Game Centre (.unity.migc), Huawei AppGallery (.huawei), QooApp Game Store (.qooapp). This is highlighted during the Register step in the UDP console. 

Each store-specific build is signed with a UDP certificate that is specific to your game and to each store. You can find and retrieve the certificate from the Advanced section of each store.

#### Can I use my own certificate to repack store-specific builds?

No. When UDP repacks your game for a store, it applies a new certificate that is specific to your game and to that store. This ensures that each store-specific build is distinct. You can find and retrieve this certificate from the Advanced section of each store.

#### Does UDP block third-party SDKs?

No. However, certain stores alter your game’s package name, and UDP signs your store-specific builds with its own certificate. Therefore, check directly with the third-party service providers how this impacts the ability of their service to operate properly in your game.

**Note**: Certain UDP stores have their own rules and regulations about what SDKs are allowed or prohibited in their store. For more details, see [Partner Stores](setting-up-udp.html#partner).

#### Can I download and test my store-specific build?

Yes. Once your game is successfully repacked, go to the Status tab where you will find a download link for your repacked game.

Note however that a repacked game will always fetch the last IAP catalog that was submitted to the store, so for the first time, you need to submit your game in order to create an IAP catalog on the store’s servers.