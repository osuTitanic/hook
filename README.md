# Titanic! Hook

A small executable that allows to connect old osu! clients without `-devserver` support to private servers, without any permanent modifications to the client.

## Supported clients

Titanic! Hook supports any osu! (stable) client released since 2008.

> [!CAUTION]
> Titanic! Hook is NOT compatible with the new osu!auth anti-cheat used since 2021. The game will automatically close if you try to use it in such clients.

## How does it work?

Titanic! Hook uses Harmony to alter the behavior of the client in order to redirect traffic to another server and patch some issues. It uses Reflection and IL reading to find target methods, so it's version-agnostic.

## Usage

Throw in the release executable for the correct .NET Framework version to the osu! directory. A configuration file will be automatically created.

> [!TIP]
> osu! versions before 2015 require the .NET Framework 2.0 build. Cuttingedge since April 2015 and Stable since November 2015 require the .NET Framework 4 build. 

### Configuration

The configuration file is pretty self-explanatory, and you most likely don't need to touch it. It will automatically use titanic.sh as the server, however you can use any server that supports the client that you are using.

> [!TIP]
> The IP address for Bancho in clients that use TCP (b20130815 and older) is resolved using the first DNS A record for the `server` subdomain, for example `server.titanic.sh`.

## Building a self-contained release from source

- Clone the repository including submodules
- Build HookLoader in the Release configuration
- In the build output directory you will get a `Titanic!netVERSION_merged.exe` that contains all dependencies built-in

## Developing

- (Optional) Attach your IDE's debugger to osu!.exe. This will allow you to set breakpoints while debugging Titanic! Hook
- Run TestInjector, which will inject Titanic! Hook into osu!

## License

This project is licensed under the GNU GPLv3 or later license. It would be greatly appreciated to change the Titanic! branding before redistribution.
