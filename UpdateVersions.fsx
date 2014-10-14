// This script copies the version from SharedAssemblyInfo into various other files that
// contain the version, as I couldn't find a nice way to share them :(

#r "System.Xml.Linq"
open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Xml.Linq

Directory.SetCurrentDirectory(__SOURCE_DIRECTORY__)
let root = Directory.GetCurrentDirectory()
let sharedAssemblyInfo = "DanTup.DartVS.Vsix\Properties\SharedAssemblyInfo.cs"

// Get the version from the master file (SharedAssemblyInfo)
let regex pattern input = Regex.Match(input, pattern)
let versionMatch =
    File.ReadAllLines(Path.Combine(root, sharedAssemblyInfo))
        |> Array.find (fun l -> l.Contains("AssemblyVersion"))
        |> regex "\(\"(\d+\.\d+)\."
let version = versionMatch.Groups.[1].Value


// Get the files we'll need to update
// Only go on level deep for files; we don't want bin\Debug etc.
let getFiles pattern dir = Directory.GetFiles(dir, pattern)
let manifestFiles = Directory.GetDirectories(root) |> Array.collect (getFiles "*.vsixmanifest")
let packageFiles = Directory.GetDirectories(root) |> Array.collect (getFiles "*Package.cs")

// Print summary
printfn "Updating %d manifests and %d packages to version: %s" manifestFiles.Length packageFiles.Length version

// Replace manifest versions
let updateManifest (file : string) =
    let xd = XDocument.Load(file, LoadOptions.PreserveWhitespace)
    let xname s = XName.Get(s, "http://schemas.microsoft.com/developer/vsx-schema/2011")
    let versionAttribute = (xd.Element(xname "PackageManifest").Element(xname "Metadata").Element(xname "Identity").Attribute(XName.Get("Version")))
    versionAttribute.Value <- version
    xd.Save(file)
manifestFiles |> Array.iter updateManifest

// Replace package versions
let updatePackage (file : string) =
    let replaceVersion (s : string) =
        match s with
        | s when s.Contains("[InstalledProductRegistration") -> Regex.Replace(s, "\"\\d+\.\d+\"\)]", (sprintf "\"%s\")]" version))
        | s -> s
    let lines = File.ReadAllLines file
    lines |> Array.map replaceVersion |> fun l -> File.WriteAllLines(file, l, Encoding.UTF8)
packageFiles |> Array.iter updatePackage
