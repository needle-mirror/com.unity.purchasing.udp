### Editing in-app purchases 

You can create and edit In-App Purchase (IAP) products both in the Unity Editor and on the UDP console. To create IAP products in the Unity Editor, refer to [Implementing UDP IAP on the client side](Using the UDP Package.md). Unity synchronizes your IAP settings in the Editor with the IAP settings on the UDP console automatically. 

![img](images/image_22.png)

To create or edit IAP products on the UDP console:

- Be sure you are in editing mode (via the top **EDIT INFO** button).
- Click **ADD ITEM** to create a new item
- Click the **pencil icon** to edit an existing item 
- Specify your product information:
- - **Product ID,** the unique identifier for the IAP product. Please take note of the required syntax for Product IDs
  - **Product Name**, the name of the IAP product
  - **Consumable**, to indicate whether the IAP product is consumable or not
  - **Description**, to succinctly describe the IAP product
![img](images/image_23.png)

**Product Name** and **Description** can also be entered in the different languages of your game listing. Select the desired language at the top on the Game Information page and input your IAP copy in that language.

Set the price of the IAP product by clicking **Manage amounts and currencies.**

**Note**: if you used the IAP Catalog during your game implementation, you shouldn’t have to create IAP products from the UDP console again - the IAP Catalog syncs between the Unity Editor and the UDP console. However, if you [implemented your IAPs directly in code](https://docs.unity3d.com/Manual/UnityIAPDefiningProducts.html), you have to enter your IAP products manually on the UDP console, and be vigilant that the Product IDs match the ones implemented in your code. 

