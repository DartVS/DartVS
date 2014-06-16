DartVS: Google Dart support for Visual Studio
=========

![Screenshot of DartVS](DanTup.DartVS.Vsix/Screenshot.png)

## Implemented
- Use DartAnalyzer from the SDK when saving any .dart file and report errors/warnings/hints to the Visual Studio error list
- Clicking errors navigates to the correct place in code
- Mostly-reliable Syntax Highlighting

## Not Yet Implemented
- Code Completion
- Tooltips to show doc comments
- Show errors "live" without having to save
- Show errors with squiggles in the editor
- Debugging
- Project System
  - Automatic `pub get`
  - F5 -> `pub serve`
  - `pub build`
- Reliable Syntax Highlighting
- Goto Definition
- Find References
- Refactor->Rename

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
