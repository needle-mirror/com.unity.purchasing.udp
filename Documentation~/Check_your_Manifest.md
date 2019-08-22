## Check your Manifest

Make sure your AndroidManifest.xml is compliant with the [Android developer guide](https://developer.android.com/guide/topics/manifest/manifest-intro), otherwise errors will appear when your game is repacked for submission to the UDP stores.

Typically, if you get

- *"Error: Unable to compile resources. Please make sure your AndroidManifest.xml is correct or contact support."*

it means your AndroidManifest is malformed, and UDP is not able to analyze the APK file.

This can, for instance, be due to elements such as <service>, <activity>, <provider>, <receiver> being outside the <application> class.

