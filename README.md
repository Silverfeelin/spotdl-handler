# [spotify-downloader](https://github.com/ritiek/spotify-downloader) Handler

The main purpose of this repository is to expose a download button on Spotify to call [spotify-downloader](https://github.com/ritiek/spotify-downloader) (`spotdl`) without needing to use the command line.

<img src="https://i.imgur.com/UWh3UrL.png" width="858" height="108" />

**Quick Navigation**
* [Prerequisites](#prerequisites)
* [Installation](#installation)
* [Usage](#usage)
* [Disclaimer](#disclaimer)

## Prerequisites

* [spotify-downloader](https://github.com/ritiek/spotify-downloader)


  > Both `spotdl` and `ffmpeg` must be available globally. For Windows users, make sure the correct directory is included in your PATH variables. More information can be found on the [spotify-downloader installation page](https://github.com/ritiek/spotify-downloader/wiki/Installation).

* [.NET Core Runtime](https://dotnet.microsoft.com/download) <sup>*(2.1 or above)*</sup>

> The application that calls `spotdl` with the correct arguments is written in .NET Core. It should work on all platforms supported by .NET Core.

* [Tampermonkey](https://tampermonkey.net/) for your favourite browser:
  * [Chrome](https://tampermonkey.net/?browser=chrome), [Firefox](https://tampermonkey.net/?browser=firefox), [Edge](https://tampermonkey.net/?browser=edge), [more...](https://tampermonkey.net/)


  > The included script adds a download button to tracks, albums and playlists on the Spotify website.

## Installation

### 1 - Application

* Unpack the contents of `SpotifyDownloader` somewhere. It doesn't matter where.
* Execture `configure.bat` or run `dotnet SpotifyDownloader.dll` from the command line.
* Press enter to make the application download music to your *My Music* folder or enter a path.
* Press enter to download music as `.m4a` files, or enter a file extension.
* *Press any key to exit...*

### 2 - Protocol

To register the `spotdl:` protocol to the application, you'll have to configure some OS-specific stuff. The included files will aid you.

#### Windows

* Run `regedit-install.bat` as administrator.
* Enter the full path to the `SpotifyDownloader.dll` file and press enter.
* *Press any key to exit...*

> To remove the registry keys, simply run `regedit-uninstall.reg`.

### 3 - Tampermonkey

* Install [Spotify Web - Download](https://greasyfork.org/en/scripts/376669-spotify-web-download).


## Usage

* Visit https://open.spotify.com/
* Right click any track, album or playlist and select <kbd>Download</kbd>.

## Disclaimer

This tool is merely a bridge between existing software. The software by itself does not allow users to download media or perform any tasks that may be illegal in your country. I am not a lawyer and can not give legal advise on the use of this software or `spotdl` for that matter. Use at your own risk!
