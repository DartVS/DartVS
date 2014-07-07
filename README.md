DartVS: Google Dart support for Visual Studio
=========

![Screenshot of DartVS](DanTup.DartVS.Vsix/Screenshot.png)

## IMPORTANT!
The current version of DartVS available to install (v0.6) in Visual Studio is very basic and uses regex for syntax highlighting,
and shells out to DartAnalyzer on a per-file basis for errors/warnings. This is not terribly reliable!

A new version of the extension (v0.8/v1) is in-progress which uses Google's Dart Analysis service and is much faster and more reliable,
howver it's still some way off release :(

## Live in v0.6
- [x] Use DartAnalyzer from the SDK when saving any .dart file and report errors/warnings/hints to the Visual Studio error list
- [x] Clicking errors navigates to the correct place in code
- [x] Mostly-reliable Syntax Highlighting

## In Development for v1
- [ ] Switch to Google's Dart analysis service](/../../issues/23)
- [ ] [Reliable Syntax Highlighting](/../../issues/4)
- [ ] [Show errors "live" without having to save](/../../issues/24)
- [ ] [Show error window automatically when errors exist](/../../issues/8)
- [ ] [Show errors with squiggles in the editor](/../../issues/25)
- [ ] [Tooltips to show doc comments](/../../issues/11)
- [ ] [Show Dart icon for .dart files](/../../issues/10)
- [ ] [Goto Definition](/../../issues/14)
- [ ] [Format Document](/../../issues/26)
- [ ] [Format Selection](/../../issues/26)
- [ ] [Automatically format block on closing brace](/../../issues/27)
- [ ] [Highlight other references to selected item](/../../issues/13)
- [ ] [Show outline navigation bars at top of editor](/../../issues/12)
- [ ] [Code Completion](/../../issues/5)
- [ ] [Find References](/../../issues/15)

## Planned for after v1
- [ ] [Code Folding](/../../issues/19)
- [ ] [Debugging](/../../issues/28)
- [ ] [Project System](/../../issues/9)
  - [ ] [Automatic `pub get`](/../../issues/17)
  - [ ] [F5 -> `pub serve`](/../../issues/17)
  - [ ] [`pub build`](/../../issues/17)
- [ ] [Refactorings](/../../issues/18)

## Installation
- Install from the [Visual Studio Gallery](http://visualstudiogallery.msdn.microsoft.com/69112f14-62d0-40fb-9ccc-03e3534e7121)
- Download and unzip the [Dart SDK](https://www.dartlang.org/tools/sdk/)
- Set the DART_SDK environment variable to point at the SDK root


Feedback
===
Please send your feedback/issues/feature requests! :-)

- GitHub Issues: [DartVS/issues](https://github.com/DanTup/DartVS/issues)
- Twitter: [@DanTup](https://twitter.com/DanTup)
- Google+: [Danny Tuppeny](http://profile.dantup.com/)
- Email: [danny+dartvs@tuppeny.com](mailto:danny+dartvs@tuppeny.com)
