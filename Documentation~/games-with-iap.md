# Implementing in-app purchases with the UDP package

To implement in-app purchases in your game for UDP, follow these steps:

1. [Query the partner store's inventory](#query).
1. [Purchase an IAP product](#purchase).
1. [Consume a purchase](#consume).
1. [Validate the client-side integration](#validate).
1. [Validating purchases on the server](#server-side)
1. [Fill in the IAP Catalog](#iap-catalog).


When you’ve completed these steps, you can proceed to [build your game](building-your-game.md).

<a name="query"></a>
## Querying the partner store’s inventory

When initialization is successful, call the `QueryInventory` method. This method queries the partner stores’ inventories.

You can use this method to:

* Check for unconsumed IAP products
* Check for purchased products that are not delivered
* Query the product details

This method returns the product information (product name, ID, price, description) for [non-consumable purchases](https://docs.unity3d.com/Manual/udp.html#non-consumable) and [consumable purchases](https://docs.unity3d.com/Manual/udp.html#consumable) which have not yet been consumed.

This lets you, for example, restore unconsumed purchases after an app crash.

![](Images/5-GamesWithIAP_02.png)<br/>
Sending a query from your game to the UDP inventory

If you specify product IDs, you get the product information for your specified IAP products.

```
StoreService.QueryInventory(List<string> productIds, IPurchaseListener listener);
```

If you don’t specify product IDs, you get the information of all IAP products.

```
StoreService.QueryInventory(IPurchaseListener listener);
```

Implement [listeners](udp-package-reference.html#listener) for events that are related to the purchase service.

Here is an example:

```
public class PurchaseListener : IPurchaseListener
{
    public void OnPurchase(PurchaseInfo purchaseInfo)
    {
        // The purchase has succeeded.
        // If the purchased product is consumable, you should consume it here.
        // Otherwise, deliver the product.
    }

    public void OnPurchaseFailed(string message, PurchaseInfo purchaseInfo)
    {
        Debug.Log("Purchase Failed: " + message);
    }

    public void OnPurchaseRepeated(string productCode)
    {
        // Some stores don't support queryInventory.
        
    }

    public void OnPurchaseConsume(PurchaseInfo purchaseInfo)
    {
        // The consumption succeeded.
        // You should deliver the product here.        
    }

    public void OnPurchaseConsumeFailed(string message, PurchaseInfo purchaseInfo)
    {
        // The consumption failed.
    }

    public void OnQueryInventory(Inventory inventory)
    {
        // Querying inventory succeeded.
    }

    public void OnQueryInventoryFailed(string message)
    {
        // Querying inventory failed.
    }
}
```

<a name="purchase"></a>
## Purchasing an IAP product

To start a purchase request from your game, call the **Purchase** method when the user purchases an item. The UDP automatically checks the purchase receipt to check the purchase is valid.

![](Images/5-GamesWithIAP_03.png)<br/>
Sending a purchase request from your game to UDP

When you call the **Purchase** method, provide the:

* `productId` - The unique identifier of the IAP product that the player wants to buy. 
* `developerPayload` - The information you want to send to the UDP SDK.
* `IPurchaseListener` - The listener that tells you the results of all purchase-related events.

For example:

```
StoreService.Purchase(string productId, string developerPayload, IPurchaseListener listener);
```

**Note**: Games with IAP must contain a `Purchase` method.

The UDP returns information to your game when the purchase is complete. 

Some partner stores' payment gateways can't get payment callbacks in real-time. This can prevent UDP quickly receiving payment SUCCESS or FAILED callbacks. In this case, UDP regards the callback as FAILED. To handle this issue, Unity recommends you [query orders](#query-order) on the server to get the latest status.

For online games, you can verify the purchase on your game server via a callback notification. UDP sends the callback notification to the URL that you specify in the [UDP Settings](udp-package-reference.html#settings)).

 ![](Images/5-GamesWithIAP_04.png)

<a name="consume"></a>
## Consuming a product

For consumable products, your game needs to send a `Consume` request to the UDP SDK. Your game should deliver a product when it is consumed. This prevents the product being delivered repeatedly.

**Note**: that **OnPurchase** returns **PurchaseInfo**. 

In the `OnPurchase` event in the `PurchaseListener` class, check if the item is consumable. If the item is consumable, consume the item and implement the game logic for the purchased in the `OnPurchaseConsume` event in the `PurchaseListener` class. For example:

```
StoreService.ConsumePurchase(PurchaseInfo, IPurchaseListener);
// implement game logic for purchase
```

![](Images/5-GamesWithIAP_05.png)<br/>
Sending a consume request from your game to UDP

<a name="validate"></a>
## Validating the client-side integration

UDP performs client-side validations automatically. When a user purchases an IAP product, partner stores return the payload and signature. The UDP SDK then validates the signature. If the validation fails, the purchase fails accordingly.

<a name="server-side"></a>
## Validating purchases on the server

You can validate purchases on the server side in one of the following ways:

* [Querying orders](#query-order)
* [Receiving callback notifications](#callback-notif)

**Note**: Callback notifications are not currently supported for the Huawei AppGallery store.

You can test your server-side implementation in the UDP Sandbox environment.

<a name="query-order"></a>
### Querying orders

Your game can query UDP about orders by calling an HTTP GET request.

![](Images/5-GamesWithIAP_11.png)<br/>
Querying UDP about orders 

GET https://distribute.dashboard.unity.com/udp/developer/api/order?orderQueryToken=\<orderQueryToken>&orderId=\<orderId>&clientId=\<clientId>&sign=\<sign>

The API can return an "unconfirmed" status for the following reasons:

* The store can’t get the order status at this time
* The store doesn't support real-time order status query

In this case, retry the QueryOrder API with an interval. The store will send a callback (in near real-time) to UDP and UDP can return the status to the game.

Get more information on the [Order Query parameters](udp-package-reference.html#order-query).

<a name="callback-notif"></a>
### Receiving callback notifications

After a purchase succeeds, if you have specified a Callback URL, the UDP server notifies the game server with the payment result. Implement an HTTP POST request and accept the following request body with JSON format:

|Attribute Name|Format|Required/Optional|Description|
|---|---|---|---|
|payload|JSON String|Required|The contents of the purchase order. Get more information on the [JSON payload](udp-package-reference.html#json-payload).|
|signature|String|Required|The PKCS1 v1.5signature of the payload|

**Using the certificate**

To verify the certificate, use the Unity Client RSA Public Key. If the certificate passes verification, extract the RSA public key from the certificate and use this key to verify the signature. To generate the signature, encrypt the payload with the RSA-SHA1 algorithm. 

Here is an example：

**Public key:**
```
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA4qxbtUqsrvwk2FZ+F2J0EkUDKLdZSVE3qPgxzKxOrgScGrCZULLav9CPzRP91HN9GccvmShH2bsegP3RVtMdwU1eV7C2JdOW1sylCyKIgylCT8tLdQeUMRaIlt7fOfl+k3bkUouWJx8WnrQYM6a7oDeCGklIlekvpQ2NcS1eg7Jp646vBzyu8FMBiuj5LZOhCJg/XXs0kRpvSOBAPndUu/HgqD9aFaXNZBxMN++efxq6PnAVRzRdTtRur+OZSBGjXxgaBKrdbXCkEM3fkMgXP9egq6vnzCiQhZ7UDFXtXQ3DPqviqrTY5WsR9t4X6JxCXo6yGlQAEK/ft9MWN13nrQIDAQAB
```
**Request body:**
```
{
    "signature": "swWWZpg0/Y26XBohvqqC/for4nyhS5zwzru5s8AJI7YYC+ECHOk7KQjOyFw7cWxM3QNpd7N7E7Umy3vYwDXjV2Y4BLnuJy5gGIpO5jKU4xBNQf793FmI0Fk93YrU31QyiIjXymg1O/H1nKSJXqMz6bycBugiStqsuGp1/CctTHE0Dpv4hC6fZoNWIHYpPJQuKh4DyP1lgE32omcuKUh7IAQduRPDa+qiYJRCA8bV17xK6T8ajS3RlhKue9hjE2a21t8p017ViaOS5OWdzptUwgnWaFi6gs1k0cjdn7o/0QJEgk5j6a8WYE/S8F7YfsYcAwUQV4KY3ex0ULsH3GQEGA==",

    "payload": "{\"ClientId\":\"Q_sX9CXfn-rTcWmpP9VEfw\",\"CpOrderId\":\"0bckmoqhel5yd13f\",\"ProductId\":\"com.mystudio.mygame.productid1\",\"ChannelType\":\"APTOIDE\",\"Currency\":\"APPC\",\"Amount\":\"1.01\",\"Country\":\"CHINA\",\"Quantity\":1,\"Rev\":\"0\",\"Status\":\"SUCCESS\",\"PaidTime\":\"2018-09-28T06:43:20Z\",\"Extension\":\"{\\\"key\\\":\\\"value\\\"}\"}"
}
```
**A code sample showing how to verify the certificate in Go:**
```
func verify(data []byte, publicKey string, sign string) bool {
    decodePublic, err := base64.StdEncoding.DecodeString(publicKey)
    if err != nil {
        panic(err)
    }

    pubInterface, err := x509.ParsePKIXPublicKey(decodePublic)
    if err != nil {
        panic(err)
    }

    pub := pubInterface.(*rsa.PublicKey)
    decodeSign, err := base64.StdEncoding.DecodeString(sign)
    if err != nil {
        return false
    }

    sh1 := sha1.New()
    sh1.Write(data)
    hashData := sh1.Sum(nil)

    err = rsa.VerifyPKCS1v15(pub, crypto.SHA1, hashData, decodeSign)
    if err != nil {
        return false
    }
    return true
}
```

<a name="iap-catalog"></a>
## Filling in the IAP Catalog

Unity recommends adding your IAP products in the UDP console, as this gives you more options for defining your IAPs.
However, Unity recommends you still create at least one IAP in the Editor to test that the Unity Editor and UDP console sync properly.

**Note:** If you don’t use an [IAP Catalog](https://docs.unity3d.com/Manual/udp.html#iap-catalog) in your game client (for example, your IAP items are maintained solely on your game server) you must still [create your IAP Catalog on the UDP console](https://docs.unity3d.com/Manual/udp-implementing-iap.html).

1. For Unity Editor versions 2019.4 and below, open the **UDP Settings** inspector window.<br/>For Unity Editor versions 2020.1 and above, open the **IAP Catalog** window.<br/>
2. In the **IAP Catalog** section, enter your product information for each IAP product.
    1. Follow the requirements for Product IDs to make sure they're valid for app stores.
    1. Make sure the IAP products you define in your game use the same **Product ID** that is set in the IAP Catalog.
3. To save an individual IAP product to the UDP console, select **Push** in the drop-down next to the product.
    1. To save all IAP products to the UDP console, select the top **Push** button.<br/>
  ![](Images/5-GamesWithIAP_07.png)
4. To add more products, select **Add new IAP**.<br/>

To make sure the IAP Catalog is properly saved, check that the items you’ve added are displayed in the UDP console.

When you’ve implemented and configured UDP, follow the steps to [build your game](building-your-game.md).