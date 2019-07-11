### Querying Orders

Your game can query UDP about orders by calling an HTTP GET request.

![img](images/image_13.png)

Querying UDP about orders：

```
/udp/developer/api/order?orderQueryToken=<orderQueryToken>&orderId=<orderId>&clientId=<clientId>&sign=<sign>
```



Parameters in the request:

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
        <td>The order query token returned by the client SDK when finishing a purchase. The token needs to be encoded Base64 before being used in the query.(UDP SDK will return PurchaseInfo.OrderQueryToken)</td>
<td style="word-break:break-all;">eyJjaGFubmVsUHJvZHVjdElkIjoiaWFwLl9mM2YzZiIsImNoYW5uZWxUeXBlIjoiQVBUT0lERSIsImNsaWVudElkIjoiQUFJZ3g5VmNGaDJZQ1ZxbUs2VWNDUSIsImNwT3JkZXJJZCI6IjJhNGQ5MWY4NDgzZjQ3YjlhYzFhNGY5MDAwZDVhNTRhIiwicGFja2FnZU5hbWUiOiJjb20udW5pdHkudW5pdHl0ZXN0Z2FtZV9mZWZ3In0=
        </td>
    </tr>
  <tr>
        <td>orderId</td>
        <td>String</td>
        <td>Required</td>
        <td>The orderId returned by the client SDK when finishing a purchase.(UDP SDK will return PurchaseInfo.GameOrderId)</td>
        <td >2a4d91f8483f47b9ac1a4f9000d5a54a
        </td>
    </tr>
   <tr>
        <td>clientId</td>
        <td>String</td>
        <td>Required</td>
        <td>The clientId can be found in the Game info - integration information of UDP console</td>
        <td >AAIgx9VcFh2YCVqmK6UcCQ
        </td>
    </tr>
    <tr>
        <td>sign</td>
        <td>String</td>
        <td>Required</td>
      <td>Generate signature with orderQueryToken and client secret, MD5.hash(orderQueryToken + clientSecret).</br>
Client Secret can also been found in the Game info - integration information of UDP console.
 </td>
      <td style="word-break:break-all;">Client Secret:  KKcCyAgej06MxjKX31WuFNeHSaTJAjLDlgoDWsPJDAM </br></br>
Sign:
90a4e440897623c7cd0b2b80a97c267e</td>
</tr>
</table>

  


Parameters in the response：

| Attribute name | Format                                        | Required /optional | Description                                                  | Example                      |
| -------------- | --------------------------------------------- | ------------------ | ------------------------------------------------------------ | ---------------------------- |
| clientId       | String                                        | Required           | The clientId that is returned by Unity after the game has created a client in the Unity IAP. | Q4AnJDW2-rxLAPujqrk1zQ       |
| cpOrderId      | String                                        | Required           | The order ID assigned by your game, or Unity if the game does not generate it. | 66mea52wne                   |
| channelType    | String                                        | Required           | Channel type                                                 | APTOIDE,CLOUDMOOLAH          |
| status         | String                                        | Required           | Indicates the status of the order.                           | SUCCESS, FAILED, UNCONFIRMED |
| productId      | String                                        | Required           | The product ID associated with the order                     | product_1                    |
| amount         | String                                        | Required           | The payment amount of the order                              | 1                            |
| quantity       | Integer                                       | Required           | Indicates the quantity of the product.                       | 1                            |
| currency       | ISO 4217                                      | Required           | The currency used to purchase the product                    | CNY                          |
| country        | ISO 3166-2                                    | Required           | The country or geographic region in which the user is located | CN                           |
| paidTime       | ISO8601 yyyy-MM-ddThh:mm:ssXXX， UTC timezone | Required           | Specifies the time when the order is paid.                   | 2017-03-08T06:43:20Z         |
| rev            | String                                        | Required           | The revision of the order (only for update)                  | 0                            |
| extension      | Json String                                   | Optional           | The developer payload used to add reference information      | {"abc" : "123"}              |

![img](images/image_70.png)

Here is an example request from your game server to the UDP server and response from the UDP server back to your game server：

**The content of the orderQueryToken:**

```
{“channelProductId”:“iap._f3f3f”,“channelType”:“APTOIDE”,“clientId”:“AAIgx9VcFh2YCVqmK6UcCQ”,“cpOrderId”:“2a4d91f8483f47b9ac1a4f9000d5a54a”,“packageName”:“com.unity.unitytestgame_fefw”}
```

**Being encoded using Base64, the orderQueryToken:**

```
eyJjaGFubmVsUHJvZHVjdElkIjoiaWFwLl9mM2YzZiIsImNoYW5uZWxUeXBlIjoiQVBUT0lERSIsImNsaWVudElkIjoiQUFJZ3g5VmNGaDJZQ1ZxbUs2VWNDUSIsImNwT3JkZXJJZCI6IjJhNGQ5MWY4NDgzZjQ3YjlhYzFhNGY5MDAwZDVhNTRhIiwicGFja2FnZU5hbWUiOiJjb20udW5pdHkudW5pdHl0ZXN0Z2FtZV9mZWZ3In0=
```

**orderId**:

```
2a4d91f8483f47b9ac1a4f9000d5a54a
```

**clientId**:

```
AAIgx9VcFh2YCVqmK6UcCQ
```

**clientSecret**:

```
KKcCyAgej06MxjKX31WuFNeHSaTJAjLDlgoDWsPJDAM
```

**Sign**:

```
90a4e440897623c7cd0b2b80a97c267e
```

**Request**:

```
Get 
https://distribute.dashboard.unity.com/udp/developer/api/order?orderQueryToken=eyJjaGFubmVsUHJvZHVjdElkIjoiaWFwLl9mM2YzZiIsImNoYW5uZWxUeXBlIjoiQVBUT0lERSIsImNsaWVudElkIjoiQUFJZ3g5VmNGaDJZQ1ZxbUs2VWNDUSIsImNwT3JkZXJJZCI6IjJhNGQ5MWY4NDgzZjQ3YjlhYzFhNGY5MDAwZDVhNTRhIiwicGFja2FnZU5hbWUiOiJjb20udW5pdHkudW5pdHl0ZXN0Z2FtZV9mZWZ3In0%3D&orderId=2a4d91f8483f47b9ac1a4f9000d5a54a&clientId=AAIgx9VcFh2YCVqmK6UcCQ&sign=90a4e440897623c7cd0b2b80a97c267e
```

**Response**:

```
{
"ClientId":"AAIgx9VcFh2YCVqmK6UcCQ",
"CpOrderId":"2a4d91f8483f47b9ac1a4f9000d5a54a",
"ProductId":"iap._f3f3f",
"ChannelType":"APTOIDE",
"Currency":"APPC",
"Amount":"0.1",
"Country":"HK",
"Quantity":1,
"Rev":"0",
"Status":"SUCCESS",
"PaidTime":"2019-06-12T03:59:42Z",
"Extension":"unity://unity3d.com?cpOrderId=2a4d91f8483f47b9ac1a4f9000d5a54a\u0026payload=payload2"
}
```

