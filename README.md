# Notepad++ Discord Rich Presence Plugin

[![contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/MikeCoder96/NotePad-Discord-Rich-Presence-Plugin/issues)

| Architecture | Tested |
| ------ | ------ |
| x64 | Yes |
| x86 | Yes |

### How to use?
- Compile
- Copy NotePad++RichDiscord.dll
- Go to Notepad++ plugin folder and locate NotePad++RichDiscord Folder
- Paste inside NotePad++RichDiscord Folder the file NotePad++RichDiscord.dll
- Copy DiscordRPC.dll and Newtonsoft.Json.dll (this two files come when you compile the solution, check bin folder)
- Paste the 2 dll quoted above inside NotePad++RichDiscord Folder
- Open Notepad++ and go in Plugins->Discord Rich Presence and press to Run Rich Presence Discord

## How to debug?
You have to set right path inside solution. 
- Open solution manager
- Inside Debug tab, set (start external program) and select notepad++.exe
- Inside Build tab, set (output path) to NotePad++RichDiscord Folder "Example: C:/Program File/Notepad++/plugin/NotePad++RichDiscord"

### NuGet Packages

Notepad++ Rich Presence Plugin is currently extended with the following NuGet extension:

| NuGet Package | WebPage |
| ------ | ------ |
| DiscordRPC | [https://github.com/Lachee/discord-rpc-csharp] |
| Newtonsoft Json | [https://github.com/JamesNK/Newtonsoft.Json] |


