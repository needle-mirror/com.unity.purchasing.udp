# UDP via Unity IAP

### How do I get Unity IAP set up with UDP?

Follow these steps to install the latest **Unity IAP** version and check you have 1.22 or above installed (which has UDP)

From the Unity Services Window:

1 - [enable Unity IAP from the Service Window](https://docs.unity3d.com/Manual/UnityIAPSettingUp.html);

2 - import the latest package using the **Import** button;
![img](images/image_39.png)

3 - from the Unity Editor top menu, go to IAP Updates:
![img](images/image_40.png)
![img](images/image_41.png)

UDP is included in version 1.22 and above

### Editor UI elements for UDP implementation via Unity IAP

So you have **Unity IAP 1.22** installed.

It is NORMAL you have both “Unity IAP” and “Unity Distribution Portal” in the **Window** menu, as Unity IAP 1.22 includes a UDP implementation.

![img](images/image_42.png)

- **Unity Distribution Portal** > gives you access to the UDP Settings window specific to the Unity IAP implementation. 
- **Unity IAP** > gives you access to the regular Unity IAP features, including the IAP Catalog.

However make sure you are getting the following.

**Window > Unity Distribution Portal > Settings** opens the UDP Settings inspector window:
![img](images/image_43.png)

The **UDP Settings** window, for Unity IAP, looks like this:
![img](images/image_44.png)

In that window, you can only set Game Title, Test Account Settings, and view/copy some the UDP client settings (yes there’s some clutter, and we are working on it).

The IAP Catalog is in a separate window, accessed via  **Window > Unity IAP > IAP Catalog**:
![img](images/image_45.png) 

In the Unity IAP Catalog, there is a dedicated UDP section. **You must enter your IAP Products in this section for them to become part of the UDP Catalog:**
![img](images/image_46.png)

Remember to **Sync to UDP** every IAP Product that you add to the catalog under the **UDP Configuration** section, using the button immediately below the price field:
![img](images/image_47.png)

Otherwise your IAP Product will NOT be synced with the IAP Catalog on the UDP Console.

Which will ultimately result in this IAP Product not being synced with the store.

Before you build, make sure you set **UDP as Build Target:**
![img](images/image_48.png)

Finally, remember that UDP is only compatible with **Unity 5.6.1** and above.

If you are using Unity IAP with a lower version of Unity, you will encounter problems.

