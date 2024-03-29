# Changelog

## [2.2.5] - 2022-03-01
- Minor fix

## [2.2.4] - 2021-11-25
- To remain compliant with Unity Editor rules (2021.2 and above,) the UDP package no longer drops its assets in the Android folder

## [2.2.3] - 2021-10-15
- Fix compilation error CS0103 when imported together with Tutorial Framework 2.0.0 package

## [2.2.2] - 2021-06-29
- Misc bugfixes and optimization

## [2.2.1] - 2021-05-26
- Misc bugfixes and optimization

## [2.2.0] - 2021-05-06
- Support of Editor Play mode (Uses IAP products defined locally; UDP methods are stubbed: Purchase and Consume will always be successful; Editor console outputs)
- New Purchase() callback status: onPurchasePending
- Scripting API documentation update
- Inclusion of an Implementation Guide that uses the In-Editor Tutorial framework (in Unity Editor 2019.4 and above)
- Information architecture changes to comply with the centralization of Services in the Package Manager
- Misc bugfixes and optimization

## [2.1.6] - 2021-01-31
- Remove useless file

## [2.1.5] - 2021-01-31
- Minor fix
- UI and UX refinement in the UDP screens

## [2.1.4] - 2020-12-03
- Minor fix

## [2.1.3] - 2020-12-03
- Bug fix

## [2.1.2] - 2020-11-30
- Bug fix

## [2.1.1] - 2020-11-24
- Update documentation; bug fix

## [2.1.0] - 2020-10-28
- UI and UX refinement in the UDP screens
- Sandbox environment improvements: more instructions, confirmation screens, method call notifications 

## [2.0.2] - 2020-08-28
- documentation update to 2.0.0

## [2.0.1] - 2020-06-11
- Fix developer payload missing issue in UDP sandbox. 

## [2.0.0] - 2020-06-05
- Package interoperability with Unity IAP: the Unity IAP package can now work directly with the UDP Package instead of maintaining its own UDP dll. 
- User permissions can be set at the Project level.
- UDP Settings: inspector window activated when invoking the asset file; faster load and sync times.
- General code refinements.

## [1.3.1] - 2020-04-17
- fix inconsistency when displaying prices returned by the stores;
- documentation update to v1.3.

## [1.3.0] - 2020-01-06
- dropping UDP info in persistent data path (for co-op with services such as Remote Config, deltaDNA, etc).

## [1.2.0] - 2019-09-10
- injection of SDK version information into app manifest; 
- premium game support.

## [1.1.3] - 2019-07-29
- ability to delete Sandbox Test Accounts; 
- added syntax check of IAP Product ID fields;
- user permissions aligned between Unity editor and UDP console; 
- improved security around the transmission of telemetry data (the data you see in your reporting dashboard) between the repacked games and the UDP backend;
- misc bugfixes to meet verified-package quality expectations (though package remains in preview)

## [1.1.2] - 2019-07-11
- Fix the init failure on some devices.

## [1.1.1] - 2019-07-07
- Small bug fix

## [1.1.0] - 2019-06-27
- Optimize the procedure of generating clients.
- Remove annoying NPE error log.
- Some little bug fixes
- Add custom UDP Application class: `UdpExtendedApplication`. If you want to make your own Application class, please extend `UdpExtendedApplication`.

## [1.0.3] - 2019-02-22
- Merge the IAP catalog and GameSettings together. 
- Change the UI of GameSettings.
- Remove dependence on permission "READ_PHONE_STATE"
- Fix a bug that GameSettings.asset may report a NPE error.

## [1.0.0] - 2019-01-25
- Update version number.
- Update description.

## [0.2.8] - 2019-01-25
- Fix some bugs

## [0.2.6] - 2018-12-06
- First commit to Packman.