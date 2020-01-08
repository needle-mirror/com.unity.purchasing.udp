# Frequently Asked Questions 

### Editor implementation / SDK

</br>

**- Which Unity versions does UDP support?**

Unity 5.6.1 and above, though we recommend that you choose a version covered by Unity support (2017.4 and above as of October 2019).

</br>

**- Should I use the UDP Package or implement via Unity IAP?**

If your game already uses Unity IAP you are better off implementing UDP with Unity IAP.

The UDP Package implementation is similar to that of Google Play in-app Billing, and is recommended if you are porting from such a project, or simply if you’re more familiar with that implementation.

In terms of output, both will let you build an Android package for UDP, however the implementation is different. 

For more information on this topic, see [Two different ways to implement UDP](Getting_Started.md)

</br>

**- As UDP is available in Unity IAP from version 1.22, how do I check that I have Unity IAP 1.22 and above?**

In the Unity Editor, go to the top menu and select **Window > Unity IAP > IAP Updates**. The window that pops up shows the version number of your Unity IAP package. For more information, see [How do I get Unity IAP set up with UDP?](UDP_via_Unity_IAP.md) for more details

</br>

**- How do I know that UDP is installed?**

If you’re implementing the UDP package, see [Editor UI elements for UDP implementation via UDP Package](Editor_UI_elements_for_UDP_implementation_via_UDP_Package.md).

If you’re implementing via Unity IAP, see [Editor UI elements for UDP implementation via Unity IAP](Editor_UI_elements_for_UDP_implementation_via_Unity_IAP.md).

Also, make sure you [Don't mix the implementations](Do_not_mix_the_implementations.md).

</br>

**- Do I need to enable Unity Analytics to use UDP?**

Yes. The UDP console reports IAP and game start metrics which are routed through our Analytics instrumentation.

</br>

**- When is a good time to implement UDP in my game development cycle?**

Once you have decided what your game’s purchasable in-app products will be. That’s typically towards the end of the dev cycle. It also means it’s easy to implement UDP in your back-catalogue games to give them a new lease of life on new app stores.

</br>

**- Do I need to rebuild my game when stores update their SDKs, or when new stores become available on UDP?**

No. The UDP build has a generic IAP implementation, like a placeholder. When you push your game to stores, we repack the generic build with the store-specific SDK. We take care of which store SDKs to repack your game with, so you only need to focus on a single implementation. However, for any given store, you can choose to repack your game for a specific SDK target (Advanced settings on the UDP console).

</br>

**- Can I only set IAP prices in USD?**

In the Editor’s IAP Catalog, yes. But when you prepare your game for submission on the UDP console, you can set different IAP prices in different currencies. However, these new currencies do not sync with the Editor component, which only reflects the USD price. 

**Note**: Certain stores only accept IAP prices in specific currencies (e.g. onestore requests IAP prices in KRW).

</br>

**- How do I build my UDP package?**

The same way you would for an Android package. If you are implementing via Unity IAP, make sure you choose UDP as the build target.

</br>

**- How do I deploy my game to the UDP console?**

You can deploy your game to the UDP console in the following ways:

- Build locally, upload to CloudBuild, deploy via CloudBuild
- Build via CloudBuild, and deploy via CloudBuild
- Build locally, and upload your APK directly on the UDP console

</br>

**- If I don’t have a paid CloudBuild subscription, is my only option to build locally and upload on the UDP console?**

No. The subscription is only required to build via CloudBuild. If you want to Upload to CloudBuild, and then deploy via CloudBuild, you don’t need a paid CloudBuild subscription – it is free.

</br>

**- Can I give my UDP build to stores directly, bypassing the UDP console?**

No. This build is only an artefact created during the course of the UDP build-and-submit process. It only works with the Unity sandbox. It is calling stubbed payment APIs and is not capable of making any transaction on any of the UDP stores’ billing systems. To be used, this build needs to be repacked with the store’s specific SDK. This is automatically done on the UDP console during the game submission process.

</br>

</br>

### The UDP console

</br>

**- How do I access the UDP console?**

This is the URL: <https://distribute.dashboard.unity.com/>. You can also find a link to the UDP console in the **UDP Settings** inspector window, as well as in the Cloud Build panel for the games which were successfully pushed to UDP.

</br>

**- Is the UDP console workflow different for implementations via UDP Package or UDP with Unity IAP?**

No. Both implementations give you a generic UDP build. This build is pushed to the UDP console, and it’s the same workflow going forward. 

</br>

**- Is the “Game Information” data going to be used for all the stores I’ll submit to?**

Yes that’s the idea. We recommend that you enter all the languages, assets and currencies relevant to your distribution plans at this stage, so that everything is set when you select the stores you will distribute your game to. We recommend you build a superset of assets that can easily address all stores; if anything is missing UDP will flag it during the Publish process (and before your submission goes through).

</br>

**- Which information is synced between the Unity Editor and the UDP console?**

The Game Title, all the IAP information, the integration information (specifically the Callback URL) and the sandbox test accounts. Note that for the IAPs, only the USD price is synced.

</br>

**- Can I tweak submissions for specific stores?**

Yes. You can do that in the Publish tab; each store features an Advanced section where you can make changes before you submit your game.

</br>

**- Will my screenshots be compatible for all stores?**

The UDP console screens your uploaded images to ensure they are compatible with the stores’ requirements. You will be notified if they do not meet the requirements.

Stores’ screenshot requirements are listed on the Partner Store pages so you can determine which are your optimal assets for the store(s) you intend to submit to. 

</br>

**- Should I provide videos in mp4 format or Youtube link?**

We suggest you do both. Some stores take mp4 files, others only take Youtube URLs. Maximize your coverage by having both if possible.

</br>

**- I’m in the UDP console, and I want to change my IAPs. Must I rebuild the game?**

No. If you change them in the general Game Info section of the UDP console, the IAP changes will be synced with your Unity project. In the Editor you will see the new info in the IAP Catalog (you may have to “pull IAP information” if the project was open in the Unity Editor while changes were made on the UDP console). Note that only the USD amounts will be synced. 

</br>

**- Can I submit Premium games?**

Yes. There is a Price field in the Game Information section for the pay-to-download price of your game, and a method to call in your game to check whether your game was purchased; for more details see [Implementing a Premium price](implementing_a_premium_price.md). Note that not all stores accept Premium games; the stores which do, carry a “Pay-to-Download” label (in the **Publish** tab). If you try to submit a Premium game to a store which doesn’t support them, you will be warned before the submission can go through. 

</br>

**- In my team who can do what on UDP?**

Permissions are based on your Organization roles (Owner, Manager, User)

- Anyone (User, Manager, Owner) can generate a new UDP client in the Editor 
- Anyone (User, Manager, Owner) can link a Unity project with a UDP client in the Editor 
- Anyone (User, Manager, Owner) can modify UDP Settings in the Editor 
- Anyone (User, Manager, Owner) can create or modify IAPs in the Editor 
- Anyone (User, Manager, Owner) can create a new game on the UDP console
- Anyone (User, Manager, Owner) can edit a game revision on the UDP console
- Both Manager and Owner can RELEASE a game revision on the UDP console
- Both Manager and Owner can sign up the Organization with a store
- Both Manager and Owner can register a game to a store
- Both Manager and Owner can publish a game to a store
- Both Manager and Owner can view the Reporting dashboard

</br>

**- We use several Organizations within our studio. How will that work?**

When signing up to a store via UDP, you are mapping that store account to your Organization. If you sign up to a store while being under another Organization, this will create a distinct account on the store side.

</br>

**- I signed up to store “X” before, but now it’s asking me to sign up again. Why is that?**

Check you’re under the same Organization as when you first signed up. If you’re under a different Organization, the system (and the store) will consider you a new user.

</br>

**- Who do the submitted games belong to?**

Games are tied to your Organization and are visible to all the users under that Organization.

</br>

**- My game is showing up on the UDP console but there are no IAPs. Why is that?**

This can happen if you forget to Push your IAP Catalog information from the Editor.

</br>

**- How do the Revisions work?**

Among all your changes to the game, build (APK file), IAP settings and game metadata can increment the revision. The moment you alter one of these elements, your store submission will be different, therefore a new revision must be created. The revision system is incremental (++1 each time) and is managed by UDP. You cannot create custom revision numbers. Revision notes (entered when you release a new revision) will help you keep track of revisions.

</br>

**- I pushed a new build, this created a new revision. Will I have to re-enter all the metadata from scratch on the UDP console?**

No. Your new revision will inherit all the metadata from the last revision, but will have the latest APK.

</br>

**- How do I edit a Revision?**

Choose the existing revision that you want to start from (usually the latest). You will inherit all the Game Information data of this revision. “Edit” to make your changes. Saving your changes will only create a Revision Draft. For this draft to become the latest revision, you need to release it (“Release” button). Be sure to enter release notes to keep track of your revisions. 

Example: there are already 5 revisions. You want to make a new revision based on Revision 3. You select Revision 3, and Edit it. Saving it will create the Revision Draft. Releasing the Revision Draft will create a new Revision 6. 

</br>

**- Why is my first revision Revision 2 (or 3)?**

When you created your UDP client in the Unity Editor (UDP Settings inspector window) you started at 1. When you set a Game Title, this will increment the revision number to 2 (you updated the game’s metadata). Then once you prepare your game for submission (category, screenshots, description, etc) you are editing revision 2 and preparing a new one. This is why your first submission to stores usually is a Revision 3. 

</br>

**- I’m working on a new Revision but my colleague is making changes to the game in the Editor. What’s going to happen?**

If a Draft Revision exists (saved on the UDP console) it will not be possible to push changes from inside the Editor that would result in revision number increment.

If the Draft Revision has not been saved yet, the Editor colleague will be able to create a new Revision.

</br>

**- How do the Sandbox Test Accounts work?**

Your generic UDP game build runs in Unity’s Sandbox environment, which simulates a store. When you run that build on an emulator or a smartphone, it will ask the user for access credentials. You define and manage these credentials in that section. More information on Sandbox Test Accounts can be found [here](Configuring_Unity_Distribution_Portal.md)

</br>

**- Will the Sandbox Test Accounts also work on my store-specific game builds?**

No. The test accounts are only for Unity’s Sandbox environment, and will only apply to the generic UDP build. Once a game is repacked for a specific store, it no longer points to the Sandbox environment where the test accounts belong.

</br>

**- Are the Sandbox Test Accounts in the Game Settings inspector window, and the UDP console, the same?**

Yes. The Editor and the UDP console will sync changes that you make on either side.

</br>

**- How do the Beta Users work?**

Your game, after having been repacked for the target store, can be pushed to the store’s beta environment and shared with specific users that you define (Beta Users). They will be given a private link to download the game from the corresponding store.

</br>

**- Can I have different Beta Users for different stores?**

Yes. The Beta Users that you define at the main Game Information section will be common to all the stores. Before pushing your game to a store’s beta environment, go to the Advanced section and find the Beta User list. You can then remove or add users to your store-specific list.

</br>

**- My game is ready on the store’s beta environment. How do it send it to my Beta Users?**

Once your game has been successfully synced to the store’s beta environment, a download URL will be created. You can find it in the Advanced section of each store (under the Beta Users sub-section). Copy/paste it, or (from the same location) simply select your beta users from the list, and press Send - an email will be generated and sent to them.

</br>

**- Do all UDP stores have a Beta environment?**

No. When they do, there will be a step called “Push to Test” in the Publish flow.

</br>

**- When repacking I get an error “Packing Failed: No UDP SDK detected”**

This can happen with an implementation via Unity IAP, if you forget to set UDP as the build target.

It can also happen if you use the Minify option while building your APK; UDP may not able to find files/directories that it needs because of it. Keep UDP-related packages in a customised proguard file (or disable Minify option) and rebuild your game.

</br>

**- I signed up to the store already. Why do I need to register my game before submitting it?**

The UDP console calls the store back-end to fetch the App ID / Key / Secret for the game you are about to submit. This is possible thanks to the back-end integration between UDP and the Stores. It saves you from running an errand to the store’s dev console to find and retrieve these parameters.

</br>

**- What are the 3 target steps?**

\- “Repack Game” unpacks the generic UDP build, and repack your game with the corresponding store’s SDK. 

\- “Push to Test” pushes this repacked build to the store’s beta environment.

\- “Submit to Store” pushes the repacked game build to the store’s content review and ingestion system. This is your formal game submission to the app store. We strongly encourage you to test your repacked builds before submitting them to the stores.

</br>

**- So if I submit to 10 stores, do you jam 10 different SDKs in my game?**

No. If you submit to 10 stores, we clone the generic UDP build 10 times. Then we repack each of these clones with the store’s SDK - and only that one. Basically 10 store-specific builds have been generated - each one using only the store’s specific SDK - but it is transparent to you as UDP handles all that heavy lifting when you set the submission process in motion.

</br>

**- What’s the Advanced section for?**

This is where you can tweak your game submission for the given store beyond what UDP does by default. For instance, overriding a package name, repacking for an older store SDK version, tweaking the prices of your IAPs.

</br>

**- I submitted my game with “Advanced” tweaks for a given store. I’m now pushing out an update of the game to the same store. Do i need to redo all the Advanced settings?**

Yes, at this stage you have to. UDP does check and optimize your submission for each store, but it doesn’t re-apply the tweaks you manually did during a previous submission.

</br>

**- Can I download and test my store-specific build?**

Yes. Once your game is successfully repacked, go to the Status tab where you will find a download link for your repacked game.

Note however that a repacked game will always fetch the last IAP catalog that was submitted to the store, so for the first time, you need to submit your game in order to create an IAP catalog on the store’s servers.

