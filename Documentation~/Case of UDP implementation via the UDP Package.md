### Case of UDP implementation via the UDP Package

In the **IAP Catalog** in the UDP Settings window, when you add / change IAP Products, make sure you save them to the UDP Console by using the PUSH functions:
![img](images/image_60.png)

The top **Push** button will sync everything with the UDP console (all IAP Products, Game Title, Settings, Test Accounts)

The Product-specific **Push** will only sync the information about that IAP Product.

The top **Pull** button will retrieve the latest UDP Settings that were saved on the UDP Console (all IAP Products, Game Title, Settings, Test Accounts). It will also override any unsaved inputs in your Editor window.

Keep an eye out for any unsaved changes:
![img](images/image_61.png)

The “edited” label disappears once your IAP Product is synced.

**Warning**: closing the UDP Settings inspector window without pushing the changes **doesn’t** pop any warning message, so until we add that in, make sure push your IAP Products diligently.