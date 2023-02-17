## Wish You Were Here: Jamming in Mixed Reality

This is a rough initial commit, expect further updates and refactoring soon.

The project is partially built using kejiro's pcx project: https://github.com/keijiro/Pcx

If you use this code, please cite our CHI 23 Paper. You can find the preprint here:
https://arxiv.org/abs/2301.09402

### SSI Pipeline Setup (Windows)

- Clone the SSI repository: https://github.com/hcmlab/ssi
- In the root path of the repo, run `setup.exe` as administrator. Leave the dropdowns as they are, check all `PATH`-related checkmarks, set file associations as preferred. Then apply.

#### Azure Kinect Setup

- Install the latest Sensor SDK: https://github.com/microsoft/Azure-Kinect-Sensor-SDK/blob/develop/docs/usage.md
- Install the latest Bodytracking SDK: https://docs.microsoft.com/en-us/azure/kinect-dk/body-sdk-download
- Open the environment variable settings of Windows, edit `PATH`, and add following lines (should you not have installed the SDKs at their default installation directories, adjust paths accordingly):
  - `C:\Program Files\Azure Kinect SDK v1.4.1\tools`
  - `C:\Program Files\Azure Kinect Body Tracking SDK\tools`

#### Shimmer Setup

TODO

### OpenVR Setup

- Install Steam: https://store.steampowered.com
- In Steam, download and install SteamVR: `steam://run/250820`
- Download Steam's UnityXR package and import it via Unity's package manager: https://github.com/ValveSoftware/unity-xr-plugin/releases
- Download the latest Vive SRWorks SDK package and import it into the project: https://developer.vive.com/resources/vive-sense/srworks-sdk/download/latest/