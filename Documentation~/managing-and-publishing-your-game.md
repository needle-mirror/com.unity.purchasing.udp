# Managing and publishing your game on the UDP console

The [UDP console](https://distribute.dashboard.unity.com) is a web-based portal where you can prepare your games for submission to multiple app stores. The UDP console lets you:

* [Consolidate and manage your game's information](#edit-info)
* [Edit your game's in-app purchases](#iap)
* [Publish your game to multiple app stores](#publish)
* [View your games’ performance across all UDP stores](reporting-dashboard.md)

**Note:** Make sure you successfully test your game’s IAPs in the Sandbox environment before continuing to this stage. If your IAPs don’t work in your generic UDP build, the versions that you’d submit to the stores would not work either. For more information, see [UDP Sandbox Test Accounts](creating-a-game-on-udp.html#udp-sandbox).

For a high-level overview of the UDP console’s information architecture, see [Navigating the UDP console](setting-up-udp.html#navigate).

<a name="edit-info"></a>
## Editing your game information on the UDP console

In the [UDP console](https://distribute.dashboard.unity.com), navigate to your game via the **My Games** page. 

![](Images/8-ManagingGame_01.png)

You can view and edit the following sections:

* [Game Description](#desc)
* [Binary](#binary)
* [Ads](#ads)
* [Premium Price](#premium)
* [In-App Purchases](#iap)
* [Sandbox Testing](#sandbox)
* [App Signature](#app-sign)
* [Integration Information](#integrate)

In the **Game Info** page, select the **EDIT INFO** button to enter edit mode. To save changes select **SAVE**. To discard your changes, select **CANCEL**.

<a name="desc"></a>
### Game description

The table below describes a superset of all the participating stores’ requirements. Not all stores use all of the properties described. The UDP console flags which fields are mandatory and which are optional. Additionally, some stores have specific syntax requirements. These are indicated in the UDP console.

<table>
  <tr>
    <td>Property</td>
    <td>Description</td>
  </tr>
  <tr>
    <td>Game Title</td>
    <td>The title of your game (this field is synced with the Editor).</td>
  </tr>
  <tr>
    <td>Genre</td>
    <td>Indicates the category that your game belongs to. You can choose from Action, Adventure, Arcade, Board, Card, Casino, Casual, Educational, Music, Puzzle, Racing, Role Playing, Simulation, Sports, Strategy, Trivia, and Word.</td>
  </tr>
  <tr>
    <td>Device</td>
    <td>Choose between Smartphone, Tablet, or Universal.</td>
  </tr>
  <tr>
    <td>Game Icon</td>
    <td>The game icon to show on the app stores.</td>
  </tr>
  <tr>
    <td>Short Description</td>
    <td>Short description (max 60 char) of your game to show on the app stores.</td>
  </tr>
  <tr>
    <td>Descriptions</td>
    <td>Full-length description (4,000 char) of your game to show on the app stores.</td>
  </tr>
  <tr>
    <td>Game Banners</td>
    <td>An image used by stores to feature your game. For example, it can be a placement in a carousel. The landscape banner is mandatory, the portrait banner is optional.</td>
  </tr>
  <tr>
    <td>Keywords</td>
    <td>Define up to 4 keywords (30 char each). These are used for search purposes in the app stores.</td>
  </tr>
  <tr>
    <td>Feature Video</td>
    <td>Add a video trailer for your game.<br/>
<strong>Note</strong>: For videos, some stores only accept MP4 files while others only accept a Youtube link. It’s recommended to upload both.</td>
  </tr>
  <tr>
    <td>Screenshots</td>
    <td>Add screenshots of your game, including the cover image (thumbnail) to lay over the mp4 video when it is not playing.</td>
  </tr>
  <tr>
    <td>Ratings</td>
    <td>Select the audience which the game is suitable for.</td>
  </tr>
</table>

Input metadata for each of your supported languages. To add new languages, choose **MANAGE LANGUAGES** while in editing mode.

![](Images/8-ManagingGame_02.png)

**Hint**: If you have already published your game on Google Play, you can import the basic information using your game’s Google Play URL:<br/>
1. Click IMPORT FROM GOOGLE PLAY.
2. Copy your game’s Google Play URL and paste it into the input box.
3. You can specify which language to specifically import by using a Google Play URL that contains the language suffix, for instance for Italian &hl=it.
4. If your Google Play URL doesn't contain the language suffix, UDP will fetch the information for the language that you’re currently editing the UDP console for (if it is set on Google Play).
5. Click Import.
6. Double-check the outcome as in some instances your computer’s IP address may ultimately determine which language you get served.

![](Images/8-ManagingGame_03.png)

<a name="binary"></a>
### Binary

Upload your APK file and OBB files:

<table>
<tr>
<td>Property</td>
<td>Description</td>
</tr>
<tr>
<td>APK File</td>
<td>Your UDP game build. If you pushed your UDP build via Cloud Build, you don’t need to upload it again.
For UDP to accept your APK file, the APK file: 
<ul>
<li>Must contain a versionName</li>
<li>Must have an Initialize() method</li>
<li>Must have a Purchase() method (for games with IAP)</li>
</ul>
If you upload a new APK version which contains a different package name, you will receive an error. This also prevents publishing the new APK version.
</td>
</tr>
<tr>
<td>OBB File (Main)</td>
<td>The main extension file for additional resources that your game or app might need.</td>
</tr>
<tr>
<td>OBB File (Patch)</td>
<td>Optional file to make small updates to the main expansion file.</td>
</tr>
<tr>
<td>Does your game use Google Play Services?</td>
<td>Knowing about your Google Play Services usage helps UDP better guide you during the submission stage.</td>
</tr>
</table>

OBB files are not pushed during a CloudBuild deployment, so if your game uses them you must upload them manually from the UDP console.

**Note**: You can only change the APK files and OBB files in the default language view (English).

Describe what’s new for the files in the **What’s New** box. UDP publishes this description in the stores together with your game.

<a name="ads"></a>
### Ads

Certain stores expect you to implement their proprietary ad network in games you submit to them.

UDP asks if your game contains ads to better inform your submissions to the stores, especially if UDP can ascertain that your submission will be rejected from a store because of this.

![](Images/8-ManagingGame_04.png)

If this is the case, these stores display a "warning" icon in the Publish section, and the tooltip explains the cause for concern. It is then easier for you to plan your submission knowing what issues lie ahead.

UDP otherwise does not modify your game’s ad implementation in any way.

The table below describes the Ads section.

|Property|Description|
|---|---|
|Does your game contain ads|Select whether or not your game contains ads. This information is useful for UDP to better guide you during submission.|
|What ad mediation solutions does your game use? (Optional)|If your game contains ads, select which mediation solutions your game uses.|
|What ad networks does your game use? (Optional)|If your game contains ads, select which ad networks your game uses.|

Ongoing tests of the most common mediation layers + ad networks indicate that games repacked for the UDP stores generally have no problem receiving ad campaigns. We will post findings once definite results are available; in the meantime feel free to [ask UDP Support](mailto:udpsupport@unity3d.com) about your ad set-up and its suitability for distribution via UDP. Make sure you mention which mediation SDK and ad networks your game uses, to get a better-informed answer faster.

<a name="premium"></a>
### Premium price

The Premium price is the price it costs players to download your game. Set a default price in USD, and adjust prices in other currencies using the **Manage amounts and currencies** link.

|Property|Description|
|---|---|
|Manage amounts and currencies|Set the cost of the game in USD.<br/>Select Convert to automatically convert the USD price into the other listed currencies.You can also edit prices manually for specific currencies.|

![](Images/8-ManagingGame_05.png)

You can only set the Premium price on the UDP console.

**Note**: Not all Stores support Pay-to-Download. If you intend to submit a Premium game through UDP, make sure that in the **Publish** tab you only select Stores that carry the Pay-to-Download label.

![](Images/8-ManagingGame_06.png)

If you try to submit a Premium game to a store which doesn’t support them, you will be warned before the submission can go through.

<a name="iap"></a>
### In-App Purchases

UDP keeps your game’s IAP Catalog synchronized between the Unity Editor and the UDP console. However, the Unity Editor only handles IAP prices in USD and IAP descriptions in English. It is only in the UDP console that you can set prices in other currencies than USD, descriptions in other languages than English, and [import your IAP products in bulk](bulk-iap-import.md).

|Property|Description|
|---|---|
|Search box|Filter the list of IAP items by text.|
|Type dropdown|Filter the list of IAPs by type, that is, consumable or non-consumable.|
|Sort dropdown|Choose how to sort the list of IAP items.|
|Add Item|Click to add a new IAP item. This opens a window to enter the IAP details.|
|Convert|Converts the USD price of your IAP item into global currencies. This overwrites any manually set prices for other currencies.|

This section covers how to edit, create and delete IAP products from the UDP console.

![](Images/8-ManagingGame_07.png)

To create IAP products in the Unity Editor, see [Game client implementation](games-with-iap.html#implement-game-client).

To understand how the IAP Catalog works in the UDP context, see [Notion of IAP Catalog](best-practices.html#notion).

#### **Editing IAP descriptions**

To edit IAP descriptions, proceed as follows.

1. Select **EDIT INFO** on the **Game Information** page
2. Choose the language which you want to edit your descriptions for via the drop-down language selector
3. In the In-App Purchases section, click the **pencil icon** to edit an existing IAP item 
4. Edit your IAP item information:
    1. **Product Name**, the name of the IAP item
    2. **Description**, to succinctly describe the IAP item
5. Select **SAVE**  to save your changes.

**Note:** The above steps only edit your IAP item information for the language selected. If the combination of *Number of IAP items x Number of Languages* to cover makes this task look daunting, consider importing all this information in bulk using the [Bulk IAP Import](bulk-iap-import.md) feature.

#### Editing IAP prices and currencies

1. Select **EDIT INFO** on the **Game Information** page.
2. Click the link **Manage amounts and currencies** of an IAP item to:
    1. Change the IAP price.
    2. Convert it from USD to foreign currencies. 
    3. Adjust pricing in these currencies.
3. Select **SAVE** to save your changes.

**Hint:** To elect certain currencies as favourite currencies, click the Star next to their currency code. This pins the currency to the top of the list. Click on the star again to unpin.

**Note:** If the combination of *Number of IAP items x Number of Currencies* to cover makes this task look daunting, you can use the `Convert` function to automatically convert all your IAP prices into global currencies, based on the USD amount set for each IAP item. This overrides any local prices you may have previously set.

Alternately, import all this information in bulk using the [Bulk IAP Import](bulk-iap-import.md) feature.

<a name="create-iap"></a>
#### Creating new IAP items

**Note:** if you used the IAP Catalog during your game implementation, you shouldn’t have to create IAP products from the UDP console again - the IAP Catalog syncs between the Unity Editor and the UDP console. However, if you have to enter your IAP items manually on the UDP console, be vigilant that the Product IDs match those implemented in your code. 

1. Select **EDIT INFO** on the **Game Information** page
2. Click **Add Item** to create a new item
3. Specify and save your product information:
    1. **Product ID**, the unique identifier of the IAP item. Please take note of the required syntax for Product IDs
    2. **Product Name**, the name of the IAP item
    3. **Price**, the price in USD of the IAP item
    4. **Consumable**, to indicate whether the IAP item is consumable or not
    5. **Description**, to succinctly describe the IAP item
4. Click the link **Manage amounts and currencies** to:
    1. convert the IAP price from USD to foreign currencies. 
    2. adjust pricing in these currencies.
5. Select **SAVE** to save your changes.

**Note:** If you have many IAP items to create, consider using the [Bulk IAP Import](bulk-iap-import.md) feature to create all your IAP items in one go - including localized description and pricing in multiple currencies.

#### **Deleting IAP items**

1. Select **EDIT INFO** on the **Game Information** page
2. Click the **trash can icon** to delete an IAP item
3. Select **SAVE** to save your changes.

#### **Importing IAP items in bulk**

1. Select **EDIT INFO** on the **Game Information** page.
2. Click **IMPORT CATALOG** in the top-right corner of the In-App Purchases panel.
3. See [Bulk IAP Import](bulk-iap-import.md) for full coverage of this function.
4. Select **SAVE** to save your changes.

<a name="sandbox"></a>
### Sandbox Testing

The **Sandbox Testing** section displays the test status for your game for:
* **UDP Initialization**
* **IAP Transaction**

Test that your in-app purchases work in your generic UDP build before you release your first game revision on UDP.

Once you have built your APK, run it in the emulator or on a real Android device, using the UDP Sandbox Test Environment, and make an IAP transaction. 

To ensure the UDP SDK is implemented in games that are uploaded to UDP, test your game in the Sandbox to check:

* The `Initialize()` method is called (for all games)
* The `Purchase()` method is called (for IAP games only)

UDP will verify whether or not the Sandbox tests were conclusive. Games that don’t meet this criteria can’t be released. If the tests are successful, you can release your revision and move to the submission stages.

Your generic UDP game build, when it launches in the UDP Sandbox environment, asks the user for access credentials. You define and manage these credentials in the Sandbox Testing section. 

Set credentials for sandbox test accounts in either:

* The **UDP Settings** window in the Unity Editor
* The **Game Information** tab of the UDP console:

Learn more about [UDP Sandbox Test Accounts.](creating-a-game-on-udp.html#udp-sandbox)

The information you edit on the UDP console is synced with the Unity Editor when you save your Revision draft.

**Note:** The test accounts are only for the UDP Sandbox environment, and only apply to the generic UDP build. When a game is repacked for a specific store, it no longer points to the sandbox environment where the test accounts belong.

<a name="app-sign"></a>
### App Signature

UDP uses an App-signing private key to sign the repacked APK files that are submitted to the stores.

Unity recommends that you select **Export and upload the key and certificate** and use your own App signing private key. If your game is released on Google Play, use the same key as on Google Play. This significantly decreases the chance of your game being flagged by Google Play Protect when users install it.

**Note**: iIf your game is flagged, this happens you can appeal to Google [here](https://support.google.com/googleplay/android-developer/answer/2992033?hl=en).

The table below describes the App Signature section of the Game Info tab.

|Property|Description|
|---|---|
|Let UDP create and manage App signing private key|UDP generates a different App signing private key for each store the game is repacked for.<br/>This option leaves your game more vulnerable to Google Play Protect warnings.|
|Export and upload the key and certificate (recommended)|UDP uses the App signing private key you upload to sign the repacked builds. If you select this option, it applies to all stores you submit your game to.|
|Steps|Displays the steps to export and upload your private key.|

#### Changing keys

If you change your app-signing preference, a notification informs you that the change won’t apply for stores which the game was already repacked for (and therefore signed).

You can switch from using the UDP key to your own key at any time. However, if you repack for a store using the UDP key, that store will always use the UDP key, even if you then switch from using the UDP key to your own key. Stores you have not repacked for will use your own key.

If you have not repacked your game for a store, you can switch from using your own key to the UDP key. If your own key is repacked for any store, you can no longer switch to using the UDP key.

<a name="integrate"></a>
### Integration Information

This information is synced with the Unity Editor. You can only edit the **Callback URL** field.

### Save your revision

When you have entered all the game information for this revision, including in all the languages, save your changes using the **SAVE** button.

![](Images/8-ManagingGame_08.png)

Saving your changes only saves your Revision Draft. [Release your Revision Draft](#release) to create a new Revision which can be submitted to stores.

**Note**: To overwrite an existing Revision Draft, edit a previously-released Revision and save the ensuing draft. This becomes the latest Revision Draft.

Learn more about the concept of [Revisions and Releases](faq.html#revisions).

<a name="release"></a>
### Release your revision

Only RELEASED revisions can be published to stores. 

When you have saved your Revision Draft and are ready to publish it, click the **RELEASE** button.

![](Images/8-ManagingGame_09.png)

Enter **Release notes** and a **Release label** to keep track of your revision.

![](Images/8-ManagingGame_10.png)

**Note:** You can only publish the latest revision to the stores. If you need to submit an older revision, edit it and re-release it so it becomes the latest one. Check the FAQ section for more tips.

The Release labels and notes of all your revisions are visible in:

* The **Game Info** section, when expanding the Revision drop-down
* The **Status** section, which is organized by Release

If you intend to submit different Releases to different stores, you should label and document your releases diligently. See [Submitting different Revisions to different stores](faq.html#submit-revisions).

<a name="publish"></a>
## Publishing your game to stores

The **Publish** panel is where you set the distribution of your game in motion. You can only publish RELEASED revisions to stores. 

![](Images/8-ManagingGame_11.png)

Follow these steps for each of the stores you want to submit your game to: 

1. [Sign up with the store](#sign-up)
2. [Register your game with the store](#register)
3. [Select the Target Step](#select-target)
4. [Set Advanced settings](#advanced)

Click on the **PUBLISH** button once your set-up is complete for all stores.

**Note:** each store displays an "info" icon mentioning whether it is fully or partially integrated with UDP. A full integration means that you can register and submit your game to the store without having to leave the UDP console. A partial integration means that additional work is required on the store’s developer console at some point during the process. The filter “**Only view stores which are fully integrated with UDP**” hides partially-integrated stores from view.  

<a name="sign-up"></a>
### Sign up with the store

If this is your first time working with this store, sign up for a store account. The sign-up redirects you outside the UDP console to complete your sign-up process with the store. Partner stores each have their own onboarding process.

![](Images/8-ManagingGame_12.png)

**Note:** Only the Organization Owner and Manager can sign up with a store. See UDP role-based permissions for more details.

Once your Organization has signed up with the store, this step is no longer required. 

Store accounts are per Unity Organization. If you return to the UDP console under a different Organization, you need to sign up again and create a different account. For more information on how to sign up to individual stores, see the store guides in the Resources section of the UDP console.

<a name="register"></a>
### Register your game with the store

When you have signed up, you can register your game with the store.

![](Images/8-ManagingGame_13.png)

Confirm the package name you want to register with the store and click **REGISTER**.

![](Images/8-ManagingGame_14.png)

When your game is registered with the store, you can no longer change the package name for that store.

If you use UDP to generate the [App signing](#app-sign) private key, UDP generates a store-specific key to sign the repacked build. This may also affect third-party services integrated in your game. The store-specific certificate is available in the **Advanced** section, once your game has been repacked.
Signing your repacked build in this way makes your game more vulnerable to Google Play Protect warnings.

Some stores ask for additional information when registering the game.

<a name="select-target"></a>
### Select the target step

Select the target step you want to take with the given store:

* **Repack Game**, UDP repacks your game with the SDK from the selected store.
* **Submit to Store**, UDP repacks and submits your game, its metadata and its IAP catalog to the production environment of the selected stores.

![](Images/8-ManagingGame_15.png)

Test your repacked build before submitting it to the stores to ensure your in-app purchases work as expected in the store’s commercial environment. If you [tested your IAPs in the sandbox environment](best-practices.html#test) there should be no problem, but it’s always safer to double-check on the final build. Select **Repack Game** only, and click on the **PUBLISH** button. When repacking is complete, download the APK from the **Status** tab.

**Note:** An IAP catalog is only synced with the store’s servers when the game is submitted to the store. A game that is only repacked would fetch the last IAP catalog submitted to the store. For the first time, you need to submit your game to create an IAP catalog on the store’s servers.

<a name="advanced"></a>
### Countries and Advanced settings

Click **Countries** and select the countries that you want to distribute your game to.

![](Images/8-ManagingGame_16.png)

Click **Advanced** to configure more store-specific settings. 

![](Images/8-ManagingGame_17.png)

You can configure the following properties specifically for a store:

|Property|Function|Affected store|
|---|---|---|
|Target SDK|The version of the store SDK that you publish your game to. By default, UDP repacks for the latest version of the store SDK.|All|
|CP ID|Merchant ID on the Huawei AppGallery Connect console.|Huawei|
|Product ID|Product ID on the Huawei AppGallery Connect console.|Huawei|
|App ID|Application ID on the Huawei AppGallery Connect console.|Huawei|
|Configuration version on the Huawei AppGallery Connect console.|Version of the game|Huawei|
|PubKey|Public key on the Huawei AppGallery Connect console.|Huawei|
|App Secret|App secret on the Huawei AppGallery Connect console.|Huawei|
|privacyPolicy|Privacy statement address on the Huawei AppGallery Connect console.|Huawei|
|Premium Price|The price players will pay to download your game.|All stores which support premium games|
|In-App Purchases|The name, price and currency for your IAP items.|All|
|GRAC certificate|A GRAC certificate is required to distribute games rated 18+ in Korea. Upload the certificate here, if required.|Samsung|
|Approval Number from SAPPRFT (aka ISBN)|Enter the SAPPRFT approval number for games you publish in China. If you don’t have a publishing licence for this, deselect China from the country list.|Xiaomi<br/>Huawei|
|Registration Number from MCPRC|Ministry of Culture Record number for games you publish in China.|Huawei|
|Launch Manually|Set to launch the game manually on the store.|Samsung<br/>Huawei|
|Launch on|Specify a date and time to launch your game on the store.|Samsung<br/>Huawei|
|Renew authentication|Select to renew your authentication token for the selected store.|Huawei|
|Package Name|Displays the name of the package. This cannot be edited once it’s been registered.|QooApp<br/>Viveport|
|URL for Privacy Policy|Enter the URL for your game’s privacy policy.|Viveport|
|URL for EULA/Terms of Use|Enter the URL for your game’s EULA/Terms of use.|Viveport|


### Company Information

Before publishing your game to the UDP stores, you need to create a company information profile. The stores use this information to populate the "About the developer" section of your game’s listing on their app store. You only need to do this once.

1. On the UDP console, choose your organization and click on the **pencil icon** to edit the profile.<br/>
  ![](Images/8-ManagingGame_18.png)
2. Enter and save your company information:

|Field|Description|
|---|---|
|Company Name|The name of your company as you want players to see it.|
|Location|The location you want to define as your studio’s headquarters.|
|Company Size|Use the drop-down to specify the number of people in your company.|
|Official Website|The official website of your company.|
|Support Email Address|The support email that players can use to contact your company.|
|About|Any additional information about your company / studio.|

### Publish

Once you have completed all the above steps, select each store you want to submit this game revision to, and click on the **PUBLISH** button. 

Only the latest released revision of your game is taken through the target steps selected for each store.

If your submission is missing anything, the UDP console displays error or warning messages. Select the **Detail** button to expand for more information on the problem.

Errors are displayed in a red panel. You must fix errors before you can submit your game. Click **Modify** to go to the erroneous area to fix the issue.

You can also choose to only submit to stores where there are no errors.

Warnings are displayed in a yellow panel. You can dismiss the warnings you decide to ignore.

## Submission follow-up

### Monitoring status

When UDP starts processing your game, visit the **Status** panel to monitor progress and check the submission history of your game. 

The **Status** panel displays an overview of your game's history.

|Field|Description|
|---|---|
|Revisions repacked|Total number of repacked APK builds created with UDP.<br/>**Note**: If a game has been repacked three times for the same stores, this counts as three.|
|Revisions submitted|Total number of submissions made via UDP.
Note: If a game was submitted five times to a store, this counts as five.|
|Submissions accepted|Number of submissions that have been accepted to app stores.|
|Submissions rejected|Number of revisions that have been rejected by app stores.|

For each game revision, the **Status** panel displays the following details:

|Field|Description|
|---|---|
|Store|The store(s) that the game was submitted to.|
|Status|The status of the revision.<br/>Not all stores can provide visibility until 'Live'. The Status section only provides the information it can get from the store.|
|Countries|The number of countries which your game is enabled for, for each store. Select the number in this column to view the countries.|
|Action|Perform additional actions, such as download revisions of your game or go to the store to complete publishing steps.|

Your game can have the following statuses:

* **Repacked**: your game was successfully repacked with the SDK of the selected store
* **Published**: your game has passed all the steps required for the store when repacking and submitting to that store
* **Pending**: your game is being repacked with the SDK of the selected store
* **Failed**: your game could not be repacked with the SDK of the selected store
* **Canceled**: your game submission was cancelled by you or someone from your Organization

![](Images/8-ManagingGame_20.png)

When your game is published to the store(s) you can monitor its performance from the [Reporting Dashboard](reporting-dashboard.md).

### Downloading repacked builds

When your game build is repacked or submitted to the store, click **Download APK file** in the **Status** panel to download your repacked build.

### Completing store submissions

Stores which are only partially integrated with UDP require you to take directly on the store’s own developer console. The **Go to Store** button next to a submitted revision indicates this. A tooltip explains which steps are still required. Follow the links and complete your submission on the store’s console.