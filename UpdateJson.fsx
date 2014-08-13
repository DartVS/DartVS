// This script uses Google's Analysis Service docs to generate C# classes
// that are used for serialising/deserialising JSON.

#r "System.Xml.Linq"
open System
open System.IO
open System.Text
open System.Xml.Linq
open System.Xml.XPath

Directory.SetCurrentDirectory(__SOURCE_DIRECTORY__)
let root = Directory.GetCurrentDirectory()
let apiDocFilename = """M:\Coding\Applications\Dart\dart\pkg\analysis_server\tool\spec\spec_input.html"""
let outputFilename = """DanTup.DartAnalysis\Json.cs"""

let doc = XDocument.Load(apiDocFilename)
let ( !! ) : string -> XName = XName.op_Implicit;;

let collect (f : XElement -> string) (x : seq<XElement>) : string =
    x |> Seq.map f |> String.concat ""

let extractDoc (fieldNode : XElement) =
    match fieldNode.Element(!!"p") with
        | null -> ""
        | _ ->
            fieldNode.Element(!!"p").Value.Trim().Split('\n')
                |> Seq.map (fun s -> s.Trim())
                |> Seq.map (sprintf "\t\t/// %s")
                |> String.concat "\r\n"
                |> sprintf "\t\t/// <summary>\r\n%s\r\n\t\t/// </summary>\r\n"

let getCSharpType = function
    | "String" -> "string"
    | x -> x

let formatPropertyName (x : string) = x.[0].ToString().ToUpper() + x.[1..]

let extractCSharpType (fieldNode : XElement) =
    let cSharpType = fieldNode.XPathSelectElement(".//ref").Value |> getCSharpType
    match fieldNode.Element(!!"list") with
        | null -> sprintf "%s" cSharpType
        | _ -> sprintf "%s[]" cSharpType

let getField (fieldNode : XElement) =
    sprintf "%s\t\tpublic %s %s;\r\n"
        (fieldNode |> extractDoc)
        (fieldNode |> extractCSharpType)
        (fieldNode.Attribute(!!"name").Value |> formatPropertyName)
            

let getType (typeNode : XElement) =
    sprintf "\tclass %s\r\n\t{\r\n%s\t}\r\n\r\n"
        (typeNode.Attribute(!!"name").Value)
        (typeNode.Descendants(!!"field") |> collect getField)

let getAllTypes () =
    sprintf """namespace DanTup.DartAnalysis.Json
{
%s
}""" (doc.Document.XPathSelectElements("//types/type") |> collect getType)




// Do the stuff!
File.WriteAllText(outputFilename, getAllTypes())
