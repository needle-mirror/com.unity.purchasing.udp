# Configuring Unity Distribution Portal

To configure Unity Distribution Portal (UDP) in your game:

1. Set up UDP      

2. Generate a Unity client

      

#### Setting up UDP   

1. Open the Unity Editor and log in with a Unity account. If you don’t have a Unity account yet, create one in [https://id.unity.com](https://id.unity.com). 

2. Create or open a Project in the Editor.

3. [Install UDP](Get-started.md). 

To complete the UDP IAP integration, refer to [Implementing UDP IAP](Implementing-UDP-IAP.md).

<table>
  <tr>
    <td>Tip: Unity IAP supports UDP as of version 1.22. You can enable UDP by setting up Unity IAP. To complete the Unity IAP integration, refer to Implementing Unity IAP for UDP.</td>
  </tr>
</table>


#### Generating a Unity client

The UDP SDK contains a **UDP Settings** file. You can use the file to generate a Unity client and test accounts.

1. Create a **UDP Settings** file by selecting **Window** > **Unity Distribution Portal** > **Settings**. 

2. In the **UDP Settings** window, link your Project with an existing client or generate a new client. 

Note that one Project can only be linked with one client.

3. View the UDP Settings.asset file in the **Inspector** window:

* IAP Catalog: see [the dedicated section](Implementing-UDP-IAP-on-the-client-side.md).

* Settings

<table>
  <tr>
    <td>Property</td>
    <td>Function</td>
    <td>Editable</td>
  </tr>
  <tr>
    <td>Game ID</td>
    <td>The identifier for the game</td>
    <td>No</td>
  </tr>
  <tr>
    <td>Client ID</td>
    <td>The UDP client identifier </td>
    <td>No</td>
  </tr>
  <tr>
    <td>Client Key</td>
    <td>Which is used when initializing the UDP SDK</td>
    <td>No</td>
  </tr>
  <tr>
    <td>Client RSA Public Key</td>
    <td>Which is used to verify the callback notification</td>
    <td>No</td>
  </tr>
  <tr>
    <td>Client Secret</td>
    <td>A Unity key to signing your request that your game send to the UDP server</td>
    <td>No</td>
  </tr>
  <tr>
    <td>Callback URL</td>
    <td>Specifies the URL for the server that receives the callback notification</td>
    <td>Yes</td>
  </tr>
</table>

* UDP Sandbox Test Accounts

Designated users will be able to log in the UDP sandbox environment to test the game’s IAPs. These test accounts only work on the generic UDP build, they will NOT work on repacked builds.

<table>
  <tr>
    <td>Property</td>
    <td>Function</td>
    <td>Editable</td>
  </tr>
  <tr>
    <td>Email</td>
    <td>The email address for the sandbox test account. It is used as the login name.</td>
    <td>Yes</td>
  </tr>
  <tr>
    <td>Password</td>
    <td>The password for the test account.</td>
    <td>Yes</td>
  </tr>
</table>
