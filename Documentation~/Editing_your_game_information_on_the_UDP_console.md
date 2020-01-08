# Editing your game information on the UDP console

The UDP console creates a game when you [generate a UDP client in the Unity Editor](Configuring_Unity_Distribution_Portal.md). To edit the game information, go to the [UDP console](https://distribute.dashboard.unity.com) and [navigate](Navigating_the_UDP_console.md) to the game that you created in the Editor.

![img](images/image_19.png)

Click **EDIT INFO** to edit your game information. You can then access the following sections:

- Basic Information
- Binary
- Premium Price
- In-App Purchases
- Integration Information
- Sandbox Test Accounts
- Beta Users

### Basic Information

Note that not every store necessarily takes all the assets listed below; however the list represents a superset of all the participating stores’ requirements. The UDP console flags which fields are mandatory and which are optional. Populate the following fields:

| Property                | Description                                                  |
| ----------------------- | ------------------------------------------------------------ |
| Game Title              | The title of your game (this field is synced with the Editor) |
| Genre                   | Indicates the category that your game belongs to. You can choose among **Action, Adventure, Arcade, Board, Card, Casino, Casual, Educational, Music, Puzzle, Racing, Role Playing, Simulation, Sports, Strategy, Trivia**, and **Word**. |
| Device                  | Choose between **Smartphone**, **Tablet**, or **Universal**. |
| Game Icon               | The game icon is shown on the app stores.                    |
| Descriptions & Keywords | The full-length description of your game to players, which is shown on the app stores. There is a short description (max 60 char) and a long description (4000 char); you can also define up to 4 keywords (30 char each). |
| Game Banners            | An image used by stores to feature your game. For example, it can be a placement in a carousel. The landscape banner is mandatory, the portrait banner is optional. |
| Screenshots & Videos    | The screenshots and video trailers of your game, including the cover image (thumbnail) to lay over the mp4 video when it is not playing. </br>**Note for videos:** certain stores only accept mp4 files while others only accept a Youtube link. We recommend you upload both. |

Input metadata for each of your supported languages. To add new languages, go to the editing mode by choosing **EDIT INFO** and go to **MANAGE LANGUAGES**. 

English is the default language. You can switch the default language to any of your supported languages.
![img](images/image_20.png)

> **Hint**: If you have already published the game on Google Play, you can import the basic information via the URL of your game on Google Play.

> - Click **IMPORT FROM GOOGLE PLAY**.
> - Copy the link of your game on Google Play to the following input box and choose **Import**.
>  ![img](images/image_21.png)

### Binary

If necessary upload your APK file and OBB files: 

- **APK File**, which is the file of your game or app. If you have pushed your UDP build via Cloud Build, you don’t need to upload it again.
- **OBB File(Main)**, which is the main extension file for additional resources that your game or app might need.
- **OBB File(Patch)**, which is optional and lets you make small updates to the main expansion file.

OBB files are not pushed during a Cloud Build deployment, so if your game uses them you must upload them manually from the UDP console.

Note that you can change the APK files and OBB files only in the default language view. 

Describe what’s new for the files in the **What’s New** box. UDP publishes this description in the stores together with your game.

### Premium Price

The Premium price is the price it costs to download your game. Set a default price in USD, and adjust prices in other currencies using the **Manage amounts and currencies** link.

![img](images/image_75.png)

Your game must have the LicenceCheck method implemented for the purchase verification to take place. See [Implementing a Premium price](implementing_a_premium_price.md) for details.

Note that not all Stores support Pay-to-Download. If you intend to submit a Premium game through UDP, make sure that in the **Publish** tab you only select Stores that carry the Pay-to-Download label:

![img](images/image_74.png)

If you try to submit a Premium game to a store which doesn’t support them, you will be warned before the submission can go through.

### Editing in-app purchases 

You can create and edit In-App Purchase (IAP) products both in the Unity Editor and on the UDP console. To create IAP products in the Unity Editor, see [Implementing UDP IAP on the client side](Using_the_UDP_package_detail.md). Unity synchronizes your IAP settings in the Editor with the IAP settings on the UDP console automatically. 

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

You can also enter the **Product Name** and **Description** in the different languages of your game listing. Select the desired language at the top on the Game Information page and input your IAP copy in that language.

Set the price of the IAP product by clicking **Manage amounts and currencies.**

> **Note**: if you used the IAP Catalog during your game implementation, you shouldn’t have to create IAP products from the UDP console again - the IAP Catalog syncs between the Unity Editor and the UDP console. However, if you [implemented your IAPs directly in code](https://docs.unity3d.com/Manual/UnityIAPDefiningProducts.html), you have to enter your IAP products manually on the UDP console, and be vigilant that the Product IDs match the ones implemented in your code. 

### Integration Information

This information is synced with the Unity Editor. You can only edit the **IAP Callback URL** field.

### Sandbox Test Accounts

Your generic UDP game build, when it launches in the UDP Sandbox environment, will ask the user for access credentials. You define and manage these credentials in that section. 

Learn more about [UDP Sandbox Test Account](Configuring_Unity_Distribution_Portal.md).

The information you edit on the UDP console is synced with the Unity Editor when you save your Revision draft.

> **Note**: the test accounts are only for the UDP Sandbox environment, and is only apply to the generic UDP build. Once a game is repacked for a specific store, it no longer points to the sandbox environment where the test accounts belong.

### Beta Users

Stores with a Beta test environment allow you to push your game to it, and let you give restricted access to the Beta users of your game. 

The Beta users you set from the general **Game Info** page are automatically set for all the stores you will submit your game to. You can set store-specific beta user lists from the [Advanced](Publishing_your_game_to_stores.md) section in the next Publish step.

### Save your Revision

Once you have entered all the game information for this revision, including in all the languages, save your changes using the **SAVE** button. 

Saving your changes only saves your Revision Draft. Release your Revision Draft to create a new Revision which can be submitted to stores.

> **Note**: you can overwrite an existing Revision Draft by editing a previously-released Revision and saving the ensuing draft. The latter will become the latest Revision Draft.

Learn more about the concept of [Revisions and Releases](FAQs.md)

### Release your revision

Only RELEASED revisions can be published to stores. 

You need to have at least one IAP product defined in order to release a revision. 

Once you have saved your Revision Draft and are ready to publish it, click the **RELEASE** button. Enter release notes to keep track of your revision.

> **Note**: only the last revision is published to the stores. If you need to submit an older revision, edit it and re-release it so it becomes the latest one.

