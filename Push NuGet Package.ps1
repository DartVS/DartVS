pushd DanTup.DartAnalysis

# This version will need updating every time; can't see an easy way to set pre-release
# without maintaining a NuSpec :/

nuget pack -Version 0.1.2-alpha
nuget push .\DanTup.DartAnalysis.0.1.2-alpha.nupkg
del .\DanTup.DartAnalysis.0.1.2-alpha.nupkg

popd