pushd DanTup.DartAnalysis

# This version will need updating every time; can't see an easy way to set pre-release
# without maintaining a NuSpec :/

nuget pack
nuget push *.nupkg
del *.nupkg

popd