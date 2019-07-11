### Using the UDP Package

This is a standalone package that is totally independent from Unity IAP.

The current version is 1.0.3 ([where to get it from](UDP Package.md))

It also has, necessarily, a [notion of IAP Catalog](Notion of IAP Catalog.md), so that your IAP Products can be synced with the UDP Console and then with the stores. 

We designed this implementation to be similar to the Google Play IAB implementation, so that if your game was wired that way then porting it to UDP would be straightforward.

It requires that you explicitly initialize UDP, query the store’s IAP inventory, request to purchase a product, consume the purchase, as is explained in [this section of the UDP Documentation](Implementing UDP in-app purchases.md).

Check the details of the [Editor UI elements involved in implementing the UDP Package](Editor UI elements for UDP implementation via UDP Package.md). 

