### Select the Target Step

Select the target step that you want to take with the given store:

- **Repack Game**, UDP repacks your game with the SDK from the selected store.
- **Push to Test**, UDP repacks and submits your game, its metadata and its IAP catalog to the test environment of the selected stores. Note that not all stores have this target step. 
- **Submit to Store**, UDP repacks and submits your game, its metadata and its IAP catalog to the production environment of the selected stores.
![img](images/image_27.png)

Test your repacked build before submitting it to the stores to ensure your in-app purchases work as expected in the store’s commercial environment. If you [tested your IAPs in the sandbox environment](Test your IAPs in the Sandbox environment.md) there should be no problem, but it’s always safer to double-check on the final build. Select **Repack Game** only, and click on the **PUBLISH** button; once repacking is completed download the APK from the **Status** tab.

**Important note**: an IAP catalog is only synced with the store’s servers when the game is Pushed to Test or Submitted to Store. A game that was only repacked would fetch the last IAP catalog that was submitted to the store. For the first time, you’d need to submit your game in order to create an IAP catalog on the store’s servers.