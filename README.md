DartAnalysis.NET
=========

A .NET wrapper around Google's Analysis Service for .NET.

## This is waaaaay incomplete. Come back soon!

Sample Usage
===

Creation
====
```csharp
// Note: Service is disposable!
var service = new DartAnalysisService(SdkFolder, ServerScript);
```

Usage
====
```csharp
// Subscribe to server notifications
service.AnalysisErrorsNotification += (s, e) => PrintErrors(e.Errors);
service.ServerStatusNotification += (s, e) => { if (!e.IsAnalyzing) { Print("Finished!") } };

// Send a request to set analysis roots
await service.SetAnalysisRoots(new[] { @"C:\MyCode\MyDartProject\" });

// Send updated content for changed files (eg. user is editing documents but has not saved)
await service.UpdateContent(
    @"C:\MyCode\MyDartProject\my_dart_project.dart",
    @"// New file contents..."
});
```


Feedback
===
Please send your feedback/issues/feature requests! :-)

- GitHub Issues: [DartVS/issues](https://github.com/DanTup/DartAnalysis.NET/issues)
- Twitter: [@DanTup](https://twitter.com/DanTup)
- Google+: [Danny Tuppeny](http://profile.dantup.com/)
- Email: [danny+dartanalysisnet@tuppeny.com](mailto:danny+dartanalysisnet@tuppeny.com)
