# Changelog
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