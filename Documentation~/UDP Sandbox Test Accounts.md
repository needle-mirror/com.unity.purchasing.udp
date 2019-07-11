### UDP Sandbox Test Accounts

You can create test accounts for users to test your game in the UDP Sandbox environment. Think of the UDP Sandbox as a pretend “Store” where you can verify that your IAP implementation is correct before pushing your game to the real stores. 

Sandbox test accounts only work on the generic UDP build, they will NOT work on repacked builds. Repacked builds can be tested separately (see [here](Select the Target Step.md)), but should only be generated once your IAP implementation clears the Sandbox. 

Refer also to [this section](Test your IAPs in the Sandbox environment .md) of the Troubleshooting guide. 

Your testers will run the generic UDP build on an emulator or a real Android device, gaining access with the credentials that you define in the UDP Sandbox Test Accounts. 

| Property | Function                                                     | Editable |
| -------- | ------------------------------------------------------------ | -------- |
| Email    | The email address for the sandbox test account. It is used as the login name. | Yes      |
| Password | The password for the test account.                           | Yes      |

Sandbox test accounts can also be added / edited [from the UDP console](Sandbox Test Accounts.md).

