# Building your game

Build an APK from the Editor (**File** > **Build Settings** > **Android** > **Build**). For more information on building for Android, see [Building apps for Android](https://docs.unity3d.com/Manual/android-BuildProcess.html).

**Note**: Your APK must contain a versionName to be accepted to UDP. Add the version for your build in the Unity Editor at **File** > **Build Settings** > **Player Settings** > **Player** > **Other Settings** > **Version**.

**Note**: If you’re using the Unity IAP package, set UDP as the build target. In the Unity Editor, select **Window** > **Unity IAP** > **Android** > **Target Unity Distribution Portal (UDP)**.

The error “Packing Failed: No UDP SDK detected” can occur in the console if: 
* You’re using Unity IAP and you didn’t set UDP as the build target.
* If you use the Minify option while building your APK; UDP may not be able to find files/directories that it needs because of it.
<br/>In this case, keep UDP-related packages in a customized proguard file (or disable Minify option) and rebuild your game.

If you have fully implemented your IAP products in the Unity Editor, using the UDP package, then you can proceed to [test your game](#testing).

If you still need to finalize your [IAP products in the UDP console](https://docs.unity3d.com/Manual/udp-implementing-iap.html#edit), ensure all of your IAP products are listed in the In-App Purchases section before you test your game. You can also convert currencies and add localized language descriptions at this point, but these are not essential for testing.


<a name="testing"></a>
## Testing your game

The UDP package contains a sandbox environment. This acts as a virtual store where you can verify that your UDP (and IAP) implementation is correct before you submit your game to the real stores. 

Before you can publish your game, test your game in the Sandbox environment to ensure:
* It initializes properly (for all games)
    * The `Initialize()` method is called (for all games)
* IAP purchases work correctly (for IAP games only)
    * The `Purchase()` method is called

UDP verifies whether or not the sandbox tests were successful. You can only release games which have been successfully tested.

To test your game in the sandbox:

1. In the UDP Settings window, go to the UDP Sandbox Test Accounts section and create login credentials for the sandbox environment.<br/>If you have already added credentials in the UDP console, you can use those.
1. Run your game on an Android device or emulator.<br/>When your game launches it should call the Initialize method, which displays the login screen automatically in the sandbox environment. If you didn’t call Initialize on game launch, trigger the necessary step to call Initialize.
    1. Enter your login credentials for the sandbox test account.<br/>This is all that’s required to check that the game initializes.
    1. For IAP games, make a purchase to test your IAP purchases.<br/>No real transaction is made in the sandbox environment.

When you’ve successfully completed the testing steps, the Game Info > Sandbox Testing section in the UDP console displays the **Tested** status.

If your purchase buttons are unresponsive, or if you cannot complete a purchase in the Sandbox environment, check your IAP / UDP implementation and fix it.
When the tests are successful, you can continue with your submission.

**Note**: The credentials you add for the sandbox test environment only work with the generic UDP build that you create from your Unity project when you’ve implemented UDP. 
When your game is repacked for a specific store, it no longer points to the sandbox environment.

Sandbox mode also supports server-side validation.