# VL.IO.OpenVR

A VL wrapper for Valve's [OpenVR](https://github.com/ValveSoftware/openvr) that enables the use of [Vive trackers](https://www.vive.com/au/support/tracker3/category_howto/tracker.html) inside vvvvv without a HMD. It heavily leans on [VVVV.OpenVR](https://github.com/tebjan/VVVV.OpenVR) by [Tebjan Halm](https://github.com/tebjan).


## Using the library
In order to use this library with VL you have to install the nuget that is available via nuget.org. For information on how to use nugets with VL, see [Managing Nugets](https://thegraybook.vvvv.org/reference/hde/managing-nugets.html) in the VL documentation. As described there you go to the commandline and then type:

    nuget install VL.IO.OpenVR


Try it with vvvv, the visual live-programming environment for .NET  
Download: http://visualprogramming.net


## How to setup _SteamVR_ to be able to use trackers without HMD.

1. Close _SteamVR_ if it's currently running.

2. Open this file in a text/code editor:
   `C:\Program Files (x86)\Steam\config\steamvr.vrsettings`

3. Add this to the `steamvr` section:
```
   "forcedDriver": "null",
   "activateMultipleDrivers": "true"
```
4. Then open:
   `C:\Program Files (x86)\Steam\steamapps\common\SteamVR\drivers\null\resources\settings\default.vrsettings`

5. Change the value for _enable_ from `false` to `true`.

6. __Warning__: Having the borderless mirror window open can cause Windows to crash [during Play Mode in Unity, idk if this also applies to vvvv].
    To avoid this, remove the following lines of code from `default.vrsettings`:
    ```
    "windowX": 0,
    "windowY": 0,
    "windowWidth": 2160,
    "windowHeight": 1200,
    ```    
    You can always select _Display VR View_ from the _SteamVR_ menu if you ever need to see the null HMD view separately. That one won't crash. 
    (Neat feature for when you're trying to figure out if _SteamVR_ can see your trackers but vvvv can't - you'll be able to see them floating in there)

7.  Launch _SteamVR_. You should now have a “Standing by” status message instead of the annoying “Headset not detected”.
    (If you're still getting HMD complaints at this point, either you're too far in the future and things have slightly changed, or your _SteamVR_ needs an update.)

8. Right-click the _SteamVR_ status window, select `Developer > Developer Settings`, then scroll down to the Room and Tracking section and click “Quick Calibrate”. 
    This will get rid of the “Please perform room setup” popup.

9.  You can dismiss the headset notice about "Direct Display Mode".

10. (Recommended) In Settings under the _Video_ tab, enable _Show Advanced Settings_, then set “Pause VR when headset is idle” to `Off`.

11. (Recommended by a few forums due to a memory leak?) In Settings under the General tab, disable SteamVR Home.

The base stations don't to talk to _SteamVR_ the way that trackers do - their _SteamVR_ indicators only start lighting up once a tracker is recognized and "sees" the base station. They're basically just (distinguishable, thanks to being on different channels) beacons of light that the trackers look at in order to orient themselves. So if your base stations aren't being found after you followed their setup, just troubleshoot the trackers.

For the trackers, you do need a Bluetooth dongle - typically 1 for each tracker. _Vive_ sends one dongle with each tracker, but it looks like [_Tundra_](https://tundra-labs.com/) ships you a bundle of trackers with one super dongle that can talk to all 3 or 4 of them.

Make sure you have the dongle(s) plugged into your PC, then go to `SteamVR > Devices > Pair Controller` and select Tracker (or "I want to pair a different type of controller" if that's still there) from the options. It'll walk you through the rest on-screen - pretty much just pressing and holding the power button for each tracker until it goes into pairing mode.

[Sauce](https://www.reddit.com/r/SteamVR/comments/vum0lh/im_in_need_of_help_using_steamvr_without_a_vive/?rdt=47110)


### Troubleshooting the trackers

If VIVE Tracker (3.0) turns off by itself, it could be due to one of the following reasons:

- The battery is drained.
- Pairing has timed out after being idle for more than 30 seconds.
- No movement has occurred for more than 5 minutes.

Note: You can set how long VIVE Tracker (3.0) waits idle before turning off. 
In _SteamVR_  `> Settings > Startup / Shutdown` set the time in "Turn off controllers after".

Find additional info in the [Tracker 3.0 FAQ](https://www.vive.com/au/support/tracker3/).

### Pogo Pins
If you need input from pogo pins, the tracker needs to be configured as handheld. It then no longer is listed as tracker but as controller unfortunately.

For more info please refer to:
- https://developer.vive.com/documents/850/HTC_Vive_Tracker_3.0_Developer_Guidelines_v1.1_06022021.pdf
- https://github.com/ValveSoftware/steamvr_unity_plugin/issues/152
- https://github.com/ValveSoftware/steamvr_unity_plugin/issues/484


---
### License

### [MIT](https://github.com/bj-rn/VL.IO.MouseKeyGlobal/blob/master/LICENSE)

<sub>Depends on [Valve's OpenVR SDK](https://github.com/ValveSoftware/openvr) please see their [license](https://github.com/ValveSoftware/openvr/blob/master/LICENSE) for further details.</sub>
