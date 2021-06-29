# UDP package reference

## Editor UI

### Unity Distribution Portal settings
The Unity Distribution Portal settings in the Project Settings window manage the connection between your Unity project and the UDP client. To enable the Unity Distribution Portal settings, you must [install the UDP package](getting-started.html#install) and [link your Unity project to a UDP client](getting-started.html#linking).

To access the Unity Distribution Portal settings in the Unity Editor, select **Services** > **Unity Distribution Portal** > **Configure**.

The table below describes the interface of the Unity Distribution Portal settings.

|Field|Description|
|---|---|
|**Pull**|Retrieves (pulls) the information you last saved on the UDP server into the Editor.<br/>The following information is synced:<br/>- Game Title<br/>- Settings<br/>- UDP Sandbox Test Accounts<br/>- IAP Catalog (if using the UDP package only)<br/>This overrides any unsaved changes in the Editor.|
|**Push**|Saves (pushes) your changes to the UDP server.<br/>The following information is synced:<br/>- Game Title<br/>- Settings<br/>- UDP Sandbox Test Accounts<br/>- IAP Catalog (if you defined the IAP Catalog using the UDP package)|
|**Game Title**|Input the title of your game, and press Push to save. This field syncs with the UDP console.<br/>By default and on first load, Game Title is populated with Game ID (see [Settings](#settings)).| 
|**Unity Project ID**|Your Unity project ID, which is now linked to your UDP client ID. This field is not editable.|
|**Settings**|Contains additional game [settings](#settings).|
|**UDP Sandbox Test Accounts**|Contains settings for UDP Sandbox Test Accounts.|
|**Go to UDP Console**|Opens the UDP console in a web browser.|
|**Go to IAP Catalog**|Opens the IAP Catalog window.|

### IAP Catalog
The IAP Catalog section lets you define IAP products for UDP. It displays a list of your IAP products and a count for the total number of IAP products. To add a new item, select **Add new IAP Product**.

The table below describes the fields of the IAP Catalog section.

|Field|Description|
|---|---|
|**Product ID**|The unique ID used to identify the IAP product.<br/>**Product IDs** must follow these requirements:<br/>- Start with a letter or a digit<br/>- Contain only letters, digits, dots (.) and underscores (_)<br/>- Must not use capitalized letters|
|**Name**|The name of the IAP product.|
|**Type**|Indicates whether the IAP product is consumable or not.|
|**Price**|The price of the IAP product in USD. You can [set prices for additional currencies](https://docs.unity3d.com/Manual/udp.html#edit) in the UDP console.<br/>You must specify a price to enable players to purchase products in your game.|
|**Description**|A short description of the IAP product.<br/>This field only supports a description in a single language (English). You can [add further languages](https://docs.unity3d.com/Manual/udp.html#edit) in the UDP console.|

If you have any unsaved changes for your IAP products, an **edited** label is displayed next to the modified products and the total count. This disappears when you sync the IAP product.

**Note**: If you use the separate Unity IAP package, select **Services** > **Unity Distribution Portal** > **IAP Catalog** > **Go to IAP Catalog**, and define your IAP products in the IAP Catalog window.

<a name="settings"></a>
### Settings

The Settings section contains additional settings that are synced from the UDP console.
**Callback URL** is the only editable setting. Press **Push** to save any changes.

|Field|Function|Editable|
|---|---|---|
|**Game ID**|The identifier for the game|No|
|**Client ID**|The UDP client identifier|No|
|**Client Key**|Used when initializing the UDP SDK|No|
|**Client RSA Public Key**|Used to verify the callback notification|No|
|**Client Secret**|A Unity key to sign your request that your game sends to the UDP server|No|
|**Callback URL**|Specifies the URL for the server that receives the callback notification|Yes|

### UDP Sandbox Test Accounts

The UDP Sandbox Test Accounts section lets you add login credentials for the UDP sandbox. This is required to test your game in the UDP sandbox environment. You can also set these credentials in the UDP console.

|Field|Function|Editable|
|---|---|---|
|**Email**|The email address for the sandbox test account. It is used as the login name.|Yes|
|**Password**|The password for the test account.|Yes|

<a name="order-query"></a>
## QueryOrder parameters

To query UDP about orders, call an HTTP GET request.
The table below describes the parameters in the QueryOrder request:

<table>
  <tr>
    <td>Attribute name</td>
    <td>Format</td>
    <td>Required/Optional</td>
    <td>Description</td>
    <td>Example</td>
  </tr>
  <tr>
    <td>orderQueryToken</td>
    <td>String</td>
    <td>Required</td>
    <td>The order query token returned by the client SDK when finishing a purchase. The token is Base64 encoded. (UDP SDK will return PurchaseInfo.OrderQueryToken)</td>
    <td style="word-break:break-all;">eyJjaGFubmVsUHJvZHVjdElkIjoiaWFwLl9mM2YzZiIsImNoYW5uZWxUeXBlIjoiQVBUT0lERSIsImNsaWVudElkIjoiQUFJZ3g5VmNGaDJZQ1ZxbUs2VWNDUSIsImNwT3JkZXJJZCI6IjJhNGQ5MWY4NDgzZjQ3YjlhYzFhNGY5MDAwZDVhNTRhIiwicGFja2FnZU5hbWUiOiJjb20udW5pdHkudW5pdHl0ZXN0Z2FtZV9mZWZ3In0=
</td>
  </tr>
  <tr>
    <td>orderId</td>
    <td>String</td>
    <td>Required</td>
    <td>The orderId returned by the client SDK when finishing a purchase. (UDP SDK will return PurchaseInfo.GameOrderId)</td>
    <td>2a4d91f8483f47b9ac1a4f9000d5a54a</td>
  </tr>
  <tr>
    <td>clientId</td>
    <td>String</td>
    <td>Required</td>
    <td>The clientId can be found in the Game info - integration information of UDP console.</td>
    <td>AAIgx9VcFh2YCVqmK6UcCQ</td>
  </tr>
  <tr>
    <td>sign</td>
    <td>String</td>
    <td>Required</td>
    <td>Generate signature with orderQueryToken and client secret, MD5.hash(orderQueryToken + clientSecret).

Client Secret can also been found in the Game info - integration information of UDP console.</td>
    <td style="word-break:break-all;">Client Secret:  KKcCyAgej06MxjKX31WuFNeHSaTJAjLDlgoDWsPJDAM

Sign:
90a4e440897623c7cd0b2b80a97c267e</td>
  </tr>
</table>

The store where your game is published returns the QueryOrder response to UDP. UDP forwards these parameters to your game.
The table below describes the parameters in the QueryOrder response：

<table>
  <tr>
    <td>Attribute name</td>
    <td>Format</td>
    <td>Required /optional</td>
    <td>Description</td>
    <td>Example</td>
  </tr>
  <tr>
    <td>clientId</td>
    <td>String </td>
    <td>Required</td>
    <td>The clientId that Unity returns after the game has created a client in the Unity IAP. </td>
    <td>Q4AnJDW2-rxLAPujqrk1zQ</td>
  </tr>
  <tr>
    <td>cpOrderId</td>
    <td>String </td>
    <td>Required</td>
    <td>The order ID assigned by your game, or Unity if the game does not generate it.</td>
    <td>66mea52wne</td>
  </tr>
    <tr>
    <td>storeOrderId</td>
    <td>String</td>
    <td>Optional</td>
    <td>The order ID that the store returns.</td>
    <td>Stores have their own Order ID format</td>
  </tr>
  <tr>
    <td>channelType</td>
    <td>String</td>
    <td>Required</td>
    <td>Channel type.</td>
    <td>APTOIDE,
CLOUDMOOLAH</td>
  </tr>
  <tr>
    <td>status </td>
    <td>String</td>
    <td>Required</td>
    <td>Indicates the status of the order.</td>
    <td>SUCCESS, FAILED, UNCONFIRMED, STORE_NOT_SUPPORT</td>
  </tr>
  <tr>
    <td>productId</td>
    <td>String</td>
    <td>Required</td>
    <td>The product ID associated with the order.</td>
    <td>product_1</td>
  </tr>
  <tr>
    <td>amount</td>
    <td>String</td>
    <td>Required</td>
    <td>The payment amount of the order.</td>
    <td>1</td>
  </tr>
  <tr>
    <td>quantity</td>
    <td>Integer</td>
    <td>Required</td>
    <td>Indicates the quantity of the product.</td>
    <td>1</td>
  </tr>
  <tr>
    <td>currency</td>
    <td>ISO 4217</td>
    <td>Required</td>
    <td>The currency used to purchase the product.</td>
    <td>CNY</td>
  </tr>
  <tr>
    <td>country</td>
    <td>ISO 3166-2</td>
    <td>Required</td>
    <td>The country or geographic region in which the user is located.</td>
    <td>CN</td>
  </tr>
  <tr>
    <td>paidTime</td>
    <td>ISO8601 yyyy-MM-ddThh:mm:ssXXX， UTC timezone</td>
    <td>Optional</td>
    <td>Specifies the time when the order is paid.</td>
    <td>2017-03-08T06:43:20Z
</td>
  </tr>
  <tr>
    <td>rev</td>
    <td>String</td>
    <td>Required</td>
    <td>The revision of the order (only for update).</td>
    <td>0</td>
  </tr>
  <tr>
    <td>extension</td>
    <td>Json String</td>
    <td>Optional</td>
    <td>The developer payload used to add reference information.</td>
    <td>{"abc" : "123"}</td>
  </tr>
</table>

Here is an example request from your game server to the UDP server and response from the UDP server back to your game server：

**The content of the orderQueryToken:**
```
{"channelProductId":“iap._f3f3f”,“channelType”:“APTOIDE”,“clientId”:“AAIgx9VcFh2YCVqmK6UcCQ”,“cpOrderId”:“2a4d91f8483f47b9ac1a4f9000d5a54a”,“packageName”:“com.unity.unitytestgame_fefw”}
```

**orderQueryToken (encoded as Base64):**
```
eyJjaGFubmVsUHJvZHVjdElkIjoiaWFwLl9mM2YzZiIsImNoYW5uZWxUeXBlIjoiQVBUT0lERSIsImNsaWVudElkIjoiQUFJZ3g5VmNGaDJZQ1ZxbUs2VWNDUSIsImNwT3JkZXJJZCI6IjJhNGQ5MWY4NDgzZjQ3YjlhYzFhNGY5MDAwZDVhNTRhIiwicGFja2FnZU5hbWUiOiJjb20udW5pdHkudW5pdHl0ZXN0Z2FtZV9mZWZ3In0=
```
**Order ID:**
```
2a4d91f8483f47b9ac1a4f9000d5a54a
```
**Client ID:**
```
AAIgx9VcFh2YCVqmK6UcCQ
```
**Client Secret:**
```
KKcCyAgej06MxjKX31WuFNeHSaTJAjLDlgoDWsPJDAM
```
**Sign:**
```
90a4e440897623c7cd0b2b80a97c267e
```
**Request**:
```
GET 

https://distribute.dashboard.unity.com/udp/developer/api/order?orderQueryToken=eyJjaGFubmVsUHJvZHVjdElkIjoiaWFwLl9mM2YzZiIsImNoYW5uZWxUeXBlIjoiQVBUT0lERSIsImNsaWVudElkIjoiQUFJZ3g5VmNGaDJZQ1ZxbUs2VWNDUSIsImNwT3JkZXJJZCI6IjJhNGQ5MWY4NDgzZjQ3YjlhYzFhNGY5MDAwZDVhNTRhIiwicGFja2FnZU5hbWUiOiJjb20udW5pdHkudW5pdHl0ZXN0Z2FtZV9mZWZ3In0%3D&orderId=2a4d91f8483f47b9ac1a4f9000d5a54a&clientId=AAIgx9VcFh2YCVqmK6UcCQ&sign=90a4e440897623c7cd0b2b80a97c267e
```
**Response:**
```
{"ClientId":"AAIgx9VcFh2YCVqmK6UcCQ","CpOrderId":"2a4d91f8483f47b9ac1a4f9000d5a54a","ProductId":"iap._f3f3f","ChannelType":"APTOIDE","Currency":"APPC","Amount":"0.1","Country":"HK","Quantity":1,"Rev":"0","Status":"SUCCESS","PaidTime":"2019-06-12T03:59:42Z","Extension":"unity://unity3d.com?cpOrderId=2a4d91f8483f47b9ac1a4f9000d5a54a\u0026payload=payload2"}
```

<a name="json-payload"></a>
## JSON payload

Here is the content of a JSON payload:

<table>
  <tr>
    <td>Attribute Name</td>
    <td>Format</td>
    <td>Required/Optional</td>
    <td>Description</td>
    <td>Example</td>
  </tr>
  <tr>
    <td>cpOrderId</td>
    <td>String</td>
    <td>Required</td>
    <td>The unique order identifier assigned by your game.</td>
    <td>0bckmoqhel5yd13f</td>
  </tr>
  <tr>
    <td>status</td>
    <td>String</td>
    <td>Required</td>
    <td>Indicates the status of the order.</td>
    <td>SUCCESS</td>
  </tr>
  <tr>
    <td>amount</td>
    <td>String</td>
    <td>Required</td>
    <td>Specifies the amount of money that the order cost.</td>
    <td>1.01</td>
  </tr>
  <tr>
    <td>productId</td>
    <td>String</td>
    <td>Required</td>
    <td>Specifies the unique identifiers of the products that belong to the order.</td>
    <td>com.mystudio.mygame.productid1</td>
  </tr>
  <tr>
    <td>paidTime</td>
    <td>ISO8601 yyyy-MM-ddThh:mm:ssZ, UTC timezone</td>
    <td>Optional</td>
    <td>The time when the order was paid. This is also returned in sandbox mode, even though no actual payment is made in the sandbox environment.</td>
    <td>2018-09-28T06:43:20Z</td>
  </tr>
  <tr>
    <td>country</td>
    <td>ISO 3166-2</td>
    <td>Required</td>
    <td>The country where the order was paid.</td>
    <td>CHINA</td>
  </tr>
  <tr>
    <td>currency</td>
    <td>ISO 4217 or cryptocurrency type</td>
    <td>Required</td>
    <td>The currency of the country where the order was placed.</td>
    <td>CNY</td>
  </tr>
  <tr>
    <td>quantity</td>
    <td>Integer</td>
    <td>Required</td>
    <td>The number of products in the order.</td>
    <td>1</td>
  </tr>
  <tr>
    <td>clientId</td>
    <td>String</td>
    <td>Required</td>
    <td>The unique client identifier that is returned after your game generates a client in Unity IAP.</td>
    <td>Q_sX9CXfn-rTcWmpP9VEfw</td>
  </tr>
  <tr>
    <td>extension</td>
    <td>String</td>
    <td>Optional</td>
    <td>The developer payload which is used to contain reference information for developers.</td>
    <td>"{\"key\":\"value\"}"</td>
  </tr>
</table>