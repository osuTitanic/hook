# Titanic! Hook

A small executable that allows to connect old osu! clients without `-devserver` support to private servers, without any permanent modifications to the client. It includes a GUI launcher, which allows you to download original files of any supported client.

## Supported clients

Titanic! Hook supports any osu! (stable) client released since 2008.

> [!CAUTION]
> Titanic! Hook is NOT compatible with the new osu!auth anti-cheat used since 2021. The game will automatically close if you try to use it in such clients.

## How does it work?

Titanic! Hook uses Harmony to alter the behavior of the client in order to redirect traffic to another server and patch some issues. It uses Reflection and IL reading to find target methods, so it's version-agnostic.

## Usage

Download the Titanic! Launcher binary, put it into an empty directory, select an osu! version, and run the game!

### Configuration

The configuration file is pretty self-explanatory, and you most likely don't need to touch it. It will automatically use titanic.sh as the server, however you can use any server that supports the client that you are using.

> [!TIP]
> The IP address for Bancho in clients that use TCP (b20130815 and older) is resolved using the first DNS A record for the `server` subdomain, for example `server.titanic.sh`.

## Building a self-contained release from source

- Clone the repository including submodules
- Build `TitanicHook.GUI` in the Release configuration
- In the build output directory you will get a `Titanic!.exe` binary of the launcher that contains all dependencies built-in

## Developing and debugging

> [!TIP]
> If you are using Visual Studio for debugging, it is highly recommended to disable Just My Code to massively speed up the initialization process.

### Through the launcher
- Run the launcher using the `Titanic (Launcher)` configuration to download nescessary game files.
- Switch the configuration to `Titanic (Loader)` to actually debug Titanic! Hook. This configuration does not start the game through a child process, which allows the debugger to load into the process.

### Through DLL injection
- Run a vanilla osu! executable.
- Attach your IDE's debugger to osu!.exe.
- Run `TitanicHook.Injector`, which will inject Titanic! Hook into osu!.

## License

This project is licensed under the GNU GPLv3 or later license. It would be greatly appreciated to change the Titanic! branding before redistribution.
