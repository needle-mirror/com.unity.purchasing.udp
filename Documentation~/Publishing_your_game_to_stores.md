# Publishing your game to stores

The **Publish** panel is where you set the distribution of your game in motion. Remember that only RELEASED revisions can be published to stores. 

![img](images/image_24.png)

Select the stores you want to publish to, and follow these 4 steps for each store: 

- Sign up to the store
- Register your game with the store
- Select the Target Step
- Set Advanced settings

Click on the **PUBLISH** button once your set-up is complete for all stores.

### Sign up to the store

If this is the first time you work with this store, sign up for a store account. The sign-up redirects you outside the UDP console to complete your signup process with the store. 
![img](images/image_25.png)

**Note**: Only the Organization Owner and Manager can sign up with a store.

Once you signed up to the store, this step will no longer be required.

**Note**: signing up to a store is based on your Unity Organization. If you return to the UDP console under a different Organization, you will be asked to sign up again and this will create a different account.

### Register your game with the store

Once you signed up, you can register your game with the store.
![img](images/image_26.png)

Confirm the package name you want to register with the store and click **REGISTER**.
![img](images/image_67.png)

Once your game is registered with the store, you can no longer change the package name for that store. 

### Select the Target Step

Select the target step that you want to take with the given store:

- **Repack Game**, UDP repacks your game with the SDK from the selected store.
- **Push to Test**, UDP repacks and submits your game, its metadata and its IAP catalog to the test environment of the selected stores. Note that not all stores have this target step. 
- **Submit to Store**, UDP repacks and submits your game, its metadata and its IAP catalog to the production environment of the selected stores.
  ![img](images/image_27.png)

Test your repacked build before submitting it to the stores to ensure your in-app purchases work as expected in the store’s commercial environment. If you [tested your IAPs in the sandbox environment](Test_your_IAPs_in_the_Sandbox_environment.md) there should be no problem, but it’s always safer to double-check on the final build. Select **Repack Game** only, and click on the **PUBLISH** button; once repacking is completed download the APK from the **Status** tab.

**Important note**: an IAP catalog is only synced with the store’s servers when the game is Pushed to Test or Submitted to Store. A game that was only repacked would fetch the last IAP catalog that was submitted to the store. For the first time, you’d need to submit your game in order to create an IAP catalog on the store’s servers.

### Countries and Advanced settings

Click **Countries** and select the countries that you want to distribute your game to. 

Click **Advanced** to configure more store-specific settings. 

The following properties can be configured specifically for a store:

| Property   | Function                                                     |
| ---------- | ------------------------------------------------------------ |
| Target SDK | The version of the store SDK that you publish your game to. By default, UDP repacks for the latest version of the store SDK. |
| IAP        | The name, price and currency for your IAP products           |
| Beta Users | Specifies the users who can receive a link to the game. This list is inherited from the Beta Users list in the **Game Info** section, and can be further edited. Note that only stores with the **Test** target step have this property. |

### Publish

Once you completed all the above steps, select each store you want to submit this game revision to, and click on the **PUBLISH** button. 

Remember that only the latest released revision of your game will be taken through the target steps selected for each store.



