# Implementing premium games with the UDP package

To enable premium games for UDP using the UDP package, you just need to follow the steps described in [UDP journey](index.html#udp-journey).

If you will publish your game to Viveport and/or QooApp, [implement the LicenseCheck method](#license).

<a name="license"></a>
## Implement the LicenseCheck method

To distribute premium games with Viveport and QooApp, you must implement the `LicenseCheck` method. This does not impact the behaviour of your game in other stores.

The LicenseCheck method determines, at each game start, if the current player has purchased the game.

This step is only required if deploying premium games to Viveport and/or QooApp.

**Note**: The LicenceCheck method is only supported from UDP package 1.2.0 and above, and Unity IAP 1.23 and above.

Call the LicenseCheck method in your game code (typically, before calling the UDP Init method):

```
StoreService.LicenseCheck(ILicensingListener listener)
```

The InitListener then tells your game whether it is licensed or not:

```
public class LicenseCheckListener : ILicensingListener
  {
      public void allow(LicensingCode code, string message)
      {
          //LicensingCode enum:
          //RETRY, LICENSED, NOT_LICENSED, STORE_NOT_SUPPORT
          Show(message);   //some meaningful message
      }
      public void dontAllow(LicensingCode code, string message)
      {
          //LicensingCode enum:
          //RETRY, LICENSED, NOT_LICENSED, STORE_NOT_SUPPORT
          Show(message);   //some meaningful message
      }
      public void applicationError(LicensingErrorCode code, string message)
      {
          //LicensingErrorCode enum:
          //ERROR_INVALID_PACKAGE_NAME, ERROR_NON_MATCHING_UID, ERROR_NOT_MARKET_MANAGED, ERROR_CHECK_IN_PROGRESS, ERROR_INVALID_PUBLIC_KEY, ERROR_MISSING_PERMISSION
          Show(message);   //some meaningful message
      }
}
```