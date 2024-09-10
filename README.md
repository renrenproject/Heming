# Heming
Heming is a automatic fishing bot to help you fishing in World of Warcraft.

## Getting started
### Runtime
The app built by .Net Core 8.0, Type `dotnet --version` in Windows Terminal to see the version of dotnet, if your .net environment doesn't support 8.0, please install via [Dotnet download](https://dotnet.microsoft.com/en-us/download).

### Game settings
#### Graphic options
Heming using splash around the bobber to determine the fish is hooked or not. So make sure the graphic option ***Spell detail level***(Classic) or ***Particle Density***(Retail) is not the lowest.

#### Fishing spell
Heming will use key '0' to start fishing, so please drag the **Fishing** spell from your spell book to the key '0' in your action bar. Besides, in classic game don't forget to equip your fishing pole.(Heming currently doesn't support key binding.)

#### Camera distance
Zoom in your camera as first personal, that will make app easier to predicting bobber.

### App settings
After bobber dropped, Heming will take a screenshot to find the bobber's location, so you must set the screenshot area. Use Notepad to open file **appsettings.json**, there 4 argunments need to set which are `X, Y, W, H`, these 4 argunments defines the start position and size of screenshot.
For example
[![example](https://imgur.la/images/2024/09/10/example.md.png)](https://imgur.la/image/example.leD67)
In this case the screen resolution is 1920x1080, and the 4 argunments are:
>{
"X": 437,
"Y": 300,
"W": 927,
"H": 426
}

If you feel difficult to find the coordinate of predicting area, you can simply set the X & Y to 0, and W & H to your screen resolution, which is:
>{
"X": 0,
"Y": 0,
"W": 1920,
"H": 1080
}

that will take the entire screen for predicting, but that will make the prediction little longer and prediction result not so good.

### Start fishing!
Start app Heming.Console.exe, enter 1 for local predicting, wait 10s count down and Heming will start fishing.

## Know issues
### Multiple screen
Heming tested on sinble screen, use Heming on multiple screen might cause problem.

### Mac support
Heming use **gdi32.dll** to get graphic info, and **user32.dll** to simulate keyboard & mouse input. So Heming currently only available on Windows and not support on Mac, this might fix in following version.

## Disclaimer
This project only for research & study, please don't use for comercial activity.
Using any unauthorized 3rd part application in game might cause your account banned, for more information please refer to [Blizzard end user license agreement](https://www.blizzard.com/en-us/legal/fba4d00f-c7e4-4883-b8b9-1b4500a402ea/blizzard-end-user-license-agreement) and [Blizzard legal](https://www.blizzard.com/en-us/legal)
Use Heming means you know the risk and consequence, Heming developer not responsable for these risks.