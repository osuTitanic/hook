# Titanic Hook

A small executable that allows to connect old osu! clients without `-devserver` support to private servers, without any permanent modifications to the client.

## Supported clients

Titanic Hook supports any osu! (stable) client released since 2008.

> [!CAUTION]
> Titanic Hook is NOT compatible with the new osu!auth anti-cheat used since 2021. The game will automatically close if you try to use it in such clients.

## How does it work?

Titanic Hook uses Harmony to alter the behavior of the client and redirect traffic to another server and patch some issues. It uses Reflection and IL reading to find target methods, so it's version-agnostic.

## Usage

Throw in the release executable for the correct .NET Framework version to the osu! directory. A configuration file will be automatically created.

### Configuration

The configuration file is pretty self-explanatory, and you probably don't need to touch it. It will automatically use titanic.sh as the server, however you can use any server that supports the client that you are using.

> [!TIP]
> The IP address for Bancho in clients that use TCP (b20130815 and older) is resolved using the first DNS A record for the `server` subdomain, for example `server.titanic.sh`.

## Building from source

- Clone the repository including submodules
- Build HookLoader in Release
- You will get a `Titanic!netVERSION_merged.exe` that contains all dependencies built-in

## Developing

You can use your IDE's debugger to attach to osu!.exe process to get access to debugging features. TestInjector allows you to load Titanic Hook at runtime.

## License

This project is licensed under the GNU GPLv3 or later license. It would be greatly appreciated to change the Titanic! branding before redistribution.
