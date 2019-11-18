#Implementing a Premium price

If you intend to set a Premium price for your game (aka Pay-To-Download) use the UDP LicenseCheck service. This determines, at each game start, whether the game has been purchased by the current player.

If your game is free to download, you don’t need to implement this method.

> **Note**: Pay-to-Download is only supported from UDP Package 1.2.0 and above, and Unity IAP 1.23 and above.

Call the LicenseCheck method with ILicensingListener in your game code (typically, before calling the UDP Init method):

```StoreService.LicenseCheck(ILicensingListener listener)```

The InitListener then tells your game whether it is licensed or not:

```java
public class LicenseCheckListener : ILicensingListener 
{   
  public void allow(LicensingCode code, string message)   
  {     
    //LicensingCode enum:     
    //RETRY, LICENSED, NOT_LICENSED, STORE_NOT_SUPPORT     
    Show(message);  //some meaningful message
  }   
  public void dontAllow(LicensingCode code, string message)   
  {    
    //LicensingCode enum:
    //RETRY, LICENSED, NOT_LICENSED, STORE_NOT_SUPPORT
    Show(message);  //some meaningful message 
  }   
  public void applicationError(LicensingErrorCode code, string message)   {     
    //LicensingErrorCode enum:
    //ERROR_INVALID_PACKAGE_NAME, ERROR_NON_MATCHING_UID, ERROR_NOT_MARKET_MANAGED, ERROR_CHECK_IN_PROGRESS, ERROR_INVALID_PUBLIC_KEY, ERROR_MISSING_PERMISSION     
    Show(message);  //some meaningful message   
  }
}
```

The Premium price for your game is set in the UDP console. See [Premium Price](Editing_your_game_information_on_the_UDP_console) for details.

