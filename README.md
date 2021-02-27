# AIR-tools

Command line tools for common AIR tasks

Download and install the latest package from [Releases](https://github.com/tuarua/AIR-Tools/releases)

-------------

Install Dependencies for air_package.json

```shell
air-tools install
```

Clear Installed Dependencies from AppData cache folder

```shell
air-tools clear-cache
```

Removes air-tools config files eg air_package.json, AndroidManifest.xml from extensions/*.ane

```shell
air-tools clear-config
```

Creates a values.xml from the provided google-services.json

```shell
air-tools convert-firebase-config google-services.json
```

Apply Firebase config to FirebaseANE.ane

```shell
air-tools apply-firebase-config google-services.json
```

Add raw assets to FirebaseANE.ane

```shell
air-tools add-raw-asset alert.mp3
```

Create Assets.car from icon

```shell
air-tools create-assets-car icon_1024x1024.png
```

Create icons/*.png from icon and add to app XML

```shell
air-tools create-icons icon_1024x1024.png src/Main-app.xml
```

### Prerequisites

You will need

- AIR 33.1.1.217+
- Xcode 11.6
- [.Net Core Runtime](https://dotnet.microsoft.com/download/dotnet-core/3.1)
