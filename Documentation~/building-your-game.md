# Building your game

Build an APK or AAB file in the Editor (**File** > **Build Settings** > **Android** > **Build**). For more information on building for Android, see [Building apps for Android](https://docs.unity3d.com/Manual/android-BuildProcess.html).

**Note**: Your build must contain a versionName to be accepted to UDP. Add the version for your build in the Unity Editor at **File** > **Build Settings** > **Player Settings** > **Player** > **Other Settings** > **Version**.

**Note**: If you’re using the Unity IAP package, set UDP as the build target. In the Unity Editor, select **Services** > **Unity IAP** > **Android** > **Target Unity Distribution Portal (UDP)**.

The error “Packing Failed: No UDP SDK detected” can occur in the console if: 
* You’re using Unity IAP and you didn’t set UDP as the build target.
* If you use the Minify option while building your game; UDP may not be able to find files/directories that it needs.
<br/>In this case, keep UDP-related packages in a customized proguard file (or disable Minify option) and rebuild your game.

If you have fully implemented your IAP products in the Unity Editor, using the UDP package, then you can proceed to [test your game](https://docs.unity3d.com/Manual/udp-sandbox-testing.html).

If you still need to finalize your [IAP products in the UDP console](https://docs.unity3d.com/Manual/udp-implementing-iap.html#edit), ensure all of your IAP products are listed in the In-App Purchases section before you test your game. You can also convert currencies and add localized language descriptions at this point, but these are not essential for testing.