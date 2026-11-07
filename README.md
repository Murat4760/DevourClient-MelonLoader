# DevourClient (MelonLoader) – friend-lobby fork

This is a fork of [ALittlePatate/DevourClient](https://github.com/ALittlePatate/DevourClient) (MelonLoader branch). The original author now hosts it at [git.patate.dev](https://git.patate.dev). It's licensed under GPL-3.0, the same as the original. See [LICENSE.txt](LICENSE.txt).

## What this fork changes
- **Unlimited UV Light** refills only your own character's battery every frame, and other players' batteries are left alone. It works whether you're the host or a guest in a friend's lobby.
- **Auto Respawn** first sends the game's normal revive event. If the host refuses it, or you're still down 3 seconds later, it revives your own character locally.
- `build.bat` and a `DevourPath` MSBuild property, so the project builds against any Devour install.
- It adds extra perks, a chase speed boost, an Azazel stagger counter, an EXP multiplier, and other quality-of-life options.

## Install (one click)
1. Click **Code → Download ZIP** on this page.
2. Right-click the ZIP and choose **Extract All**.
3. Close DEVOUR, then double-click **`build.bat`** in the extracted folder.

That's all. `build.bat` handles the rest:
- Finds DEVOUR through Steam, in any library or on any drive. If it can't find the game, it asks you for the folder.
- Installs the .NET 6 SDK into the project folder if you don't have one. This happens once, takes about 200 MB, and doesn't need admin.
- Installs **MelonLoader v0.6.4** into DEVOUR, or switches it back on if it was turned off (`version.dll.off`).
- On the very first run, starts DEVOUR once so MelonLoader can prepare the game files (about 2–10 minutes), then closes the game.
- Builds `DevourClient.dll` and copies it into `DEVOUR\Mods`.

When it says **All done**, start DEVOUR from Steam and press **INSERT** to open the menu. Unlimited UV Light is in the Visuals tab, and Auto Respawn is in the Misc tab.

Windows may show a "Windows protected your PC" warning for a downloaded `.bat`. Click **More info → Run anyway**.

If DEVOUR is somewhere unusual, you can drag the DEVOUR folder onto `build.bat`, or run `build.bat "D:\path\to\Devour"`.

Host-only features (spawning, map changes, Kill, Force Start) still need you to be the lobby owner. Please don't use the mod to ruin other people's games in public lobbies.

---

# DevourClient

Very based cheat for the game Devour.

## Detection rate

Well at this point i don't really know, i think there is some sort of native Unity anti cheat template but it doesn't seem active. You're fine, no anti cheat !

## Menu
![menu screenshot](Screenshots/menu.png)

## Features
Everything about spoofing ehre (steam name, server name, level...) will persist if you don't uncheck it (it will be reseted when you'll restart the game obv).
* An IMGUI menu thanks to UnityEngine
* Fully compatible with the new IL2CPP version of the game
* Detects if you're in game (with bad code lol), so no chances of crashing on main menu by activating features
* Detects the map you are playing on (useful for the instant win)
* Big Flashlight (allows your flashlight to light a lot more)
* Flashlight color customization (with a home made color picker)
* Unlimited UV light (thanks to [@jadis0x](https://github.com/jadis0x))
* A chat spammer for Lobby and InGame chat (i couldn't do a text entry because of the limitations of [Il2CppAssemblyUnhollower](https://github.com/knah/Il2CppAssemblyUnhollower))
* Achievements unlocker (couldn't do all of them, my code is crashing for some reasons at some point, i may fix it, for now it's commented out)
* Doors unlocker (should work fine, though it doesn't seem to work sometimes)
* Keys teleporter
* LV spoofer
* Fly
* Unlock all, including flashlights, perks, outfits. Active by default, can't be turned off, no persistance.
* Instant Win (allows you to win instantaniously on any map, works in singleplayer, but not as a client. May be working as host)
* Random Sound (make your character play a random acting sound)
* Always carrying a medkit
* Change your exp at the end of the game, changing it is permanant !
* Player ESP (with a home made color picker)
* Player skeleton ESP
* Player snaplines (with a home made color picker)
* Azazel ESP (with a home made color picker)
* Azazel Skeleton ESP
* Azazel snapline (with a home made color picker)
* Item ESP
* Demon ESP
* Goat/Rat ESP
* TP all the items to your position ! (thanks to [@jadis0x](https://github.com/jadis0x))
* Spawn any item/entity to your position
* Walk in the lobby
* Change the player's speed
* Fullbright
* Infinite mirrors (Manor update)
* Switch between realms (Manor update)
* Due to the game update, I deleted "Steam name spoofer", "Server name spoofer" and "Create a lobby with no player limit" these three functions.For "steam name spoofer", even changed your name by this function, your teamates can still see your name by steam profile, escape button, and your message in game. For "create a lobby with no player limit", if create a lobby with more than four players, the ghost will be stuck or some of the players will not be able to move. So I have to delete this function.(by manafeng)

## English Installation Tutorial

Raz did a great job at writing a guide on how to install the mod, link here : [link](https://docs.google.com/presentation/d/1YdIE5wwGWiJZ2RVughFYrlXUnFrxol-HI7QyLY_m0zc)<br>

## French Installation Tutorial

For my French fellas out there, 1tap2times made a French video tutorial for the installation of the Mod : [link](https://vimeo.com/789315436)<br>

## German Video Installation Tutorial

For my German friends, KiwiJuice02 made a german video tutorial right here : [link](https://www.youtube.com/watch?v=Ntablvo6y-I)<br>

In order to get all of this working you need to generate the DevourClient.dll file by building the source code.<br>

0. Install [.NET 6 SDK and runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
1. [Build the cheat from source](https://github.com/ALittlePatate/DevourClient#building-from-source).
2. Put the DevourClient.dll file located in `DevourClient\bin\Release\net6.0` inside `C:\Program Files (x86)\Steam\steamapps\common\Devour\Mods` folder.
3. Start the game, now you have successfully installed DevourClient. Use INSERT to open the menu

## 中文安装指南
如果你只是想要安装这个插件，直接在游戏里使用的话

1、安装 .net 6 的运行环境 → (https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

2、安装melonloader → (https://github.com/LavaGang/MelonLoader/releases)
        版本无限制，尽量选择新版即可。打开melonloader页面后，点击devour进入安装界面，全部默认即可，无需勾选或修改其他选项，点击install进行安装（安装过程中可能需要vpn支持）
    
3、安装dll文件 → 从本项目的release中下载最新的dll文件，然后将此文件添加到你的devour的安装目录中的mods文件夹里（不知道目录的情况下，可以在steam中右键devour，选择“管理”-“浏览本地文件”即可）

4、运行devour → 如果安装成功，你会看到一个windows窗口进行各类安装提示后，自动进入游戏。点击insert键即可打开和关闭devourclient窗口

ps:有些电脑在安装melonloader之后，会出现fatal error的提示，这个我目前并没有碰到过。但是出现这个提示的主要原因，基本是melonloader安装过程中，提取到devour根目录的melonloader文件夹里的文件出现了问题，比较简单的解决办法就是（1）在别人的同系统同位宽（x86，x32）的电脑里拷贝出来他的melonloader文件夹，然后直接粘贴到自己的电脑里。（2）将melonloader文件夹完全删除，然后重装。

如果你想要对代码进行修改和开发，请按照下面的”building from source“的步骤，逐步进行


## Uninstallation

0. Delete the folders `MelonLoader`, `Mods`, `Plugins`, `UserData`, and the file `version.dll` from `C:\Program Files (x86)\Steam\steamapps\common\Devour`

## Building from source

0. Clone the repository (or Code -> Download Zip)
1. Install [.NET 6 SDK and runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
2. Install [MelonLoader](https://github.com/LavaGang/MelonLoader/releases) V0.6.4 (go to Settings -> tick "Show ALPHA Pre-Releases") to Devour.
3. Start your game. A cmd should appear, don't close it, MelonLoader is installing and decompiling Devour's game assemblies.
4. Wait for the process to finish, once it's done close the game.
5. Open the solution file (DevourClient.sln) in Visual Studio
6. Go to : Project --> Add a reference --> Browse --> Click on the browse button in the down right corner of the window.
7. Add those files :
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\net6\MelonLoader.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\net6\0Harmony.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\net6\Il2CppInterop.Runtime.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\Il2CppOpsive.UltimateCharacterController.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\Il2CppBehaviorDesigner.Runtime.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\Il2Cppbolt.user.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\Il2Cppbolt.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\Il2Cppmscorlib.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\UnityEngine.IMGUIModule.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\UnityEngine.InputLegacyModule.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\UnityEngine.HotReloadModule.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\UnityEngine.UI.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\UnityEngine.UIModule.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\UnityEngine.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\UnityEngine.CoreModule.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\UnityEngine.InputModule.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\Il2Cppudpkit.common.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\Il2Cppudpkit.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\Il2Cppudpkit.platform.photon.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\UnityEngine.AnimationModule.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\UnityEngine.PhysicsModule.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\Il2Cppcom.rlabrecque.steamworks.net.dll`
* `C:\Program Files (x86)\Steam\steamapps\common\Devour\MelonLoader\Il2CppAssemblies\unity.TextMeshPro.dll`
8. Build the solutions in Release | Any CPU

## Contact

You can add me on discord at _.patate or on the [discord server](https://discord.gg/2amMFvqjYd)

## Code used

For teaching me the basics :
* [A Begginner's Guide To Hacking Unity Games](https://www.unknowncheats.me/wiki/A_Beginner%27s_Guide_To_Hacking_Unity_Games)

For teaching me about the MelonLoader mods API and Il2Cpp specifications :
* [MelonLoader's quickstart documentation](https://melonwiki.xyz/#/modders/quickstart)
* [MelonLoader's Il2Cpp differences chapter in the documentation](https://melonwiki.xyz/#/modders/il2cppdifferences)

For teaching me about the UnityEngine API :
* [Unity User Manual 2020.3 (LTS)](https://docs.unity3d.com/Manual/index.html)

For decompiling and looking in the source code of the game :
* [dnSpy : a .NET debugger and assembly editor](https://github.com/dnSpy/dnSpy)

For teaching me the basics about Devour game hacking, and i pasted the Key TP hack and the non working part of the Achievements Unlocker from it :
* [DevourCheatMonoInjector](https://github.com/Glatrix/DevourCheatMonoInjector)

Game's last update before il2cpp :
* https://steamdb.info/depot/1274571/history/?changeid=M:1960656865974212833

## Contributing

Open an [issue](https://github.com/ALittlePatate/DevourClient/issues/new) or make a [pull request](https://github.com/ALittlePatate/DevourClient/pulls), i'll be glad to improve my project with you !

## License

[GPL 3.0](https://www.gnu.org/licenses/gpl-3.0.md)


