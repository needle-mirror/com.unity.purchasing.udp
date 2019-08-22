# Check your IAP implementation

### For implementation via the UDP Package

The UDP Package implementation requires that you explicitly 

- initialize UDP 
- query the store’s IAP inventory
- request to purchase a product
- consume the purchase

as explained in [this section of the UDP Documentation](Client-side_implementation_of_UDP.md).

Please read carefully and ensure your implementation is compliant. Again, [test your IAPs in your generic UDP build](Test_your_IAPs_in_the_Sandbox_environment.md) before repacking it for submission to the stores.

### For implementation via Unity IAP

The following steps: initialize UDP, query the store’s IAP inventory, request to purchase a product, consume the purchase - <u>need NOT be specifically implemented for UDP</u> because your game relies on the underlying Unity IAP implementation.

However, your game needs to properly use Unity IAP’s similar functions (initialization, purchase, etc) according to the [Unity IAP Documentation](https://docs.unity3d.com/Manual/UnityIAP.html).

(Note that the steps involved are a little different between UDP and Unity IAP. For example, Unity IAP does the “consume” automatically, so there is no API in Unity IAP for the consumption.)

Again, [test your IAPs in your generic UDP build](Test_your_IAPs_in_the_Sandbox_environment.md) before repacking it for submission to the stores.

