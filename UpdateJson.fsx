// This script uses Google's Analysis Service docs to generate C# classes
// that are used for serialising/deserialising JSON.

// TODO: This script (like most of this repo) is currently a mess. Tidy it up!

#r "System.Xml.Linq"
open System
open System.IO
open System.Text
open System.Xml.Linq
open System.Xml.XPath

Directory.SetCurrentDirectory(__SOURCE_DIRECTORY__)
let root = Directory.GetCurrentDirectory()
let apiDocFilename = """M:\Coding\Applications\Dart\dart\pkg\analysis_server\tool\spec\spec_input.html"""
let outputJsonFilename = """DanTup.DartAnalysis\Json.cs"""
let outputRequestFilename = """DanTup.DartAnalysis\Requests.cs"""

let doc = XDocument.Load(apiDocFilename)
let ( !! ) : string -> XName = XName.op_Implicit;;

let mutable mappedTypes =
    Map.empty
        .Add("String", "string")

let mutable unionTypes =
    Map.empty

let collect (f : XElement -> string) (x : seq<XElement>) : string =
    x |> Seq.map f |> String.concat ""


let getCSharpType t =
    if mappedTypes.ContainsKey t then
        mappedTypes.[t]
    else
        t

let populateTypeMapping (typeNode : XElement) =
    if typeNode.Element(!!"ref") <> null then
        mappedTypes <- mappedTypes.Add ((typeNode.Attribute(!!"name").Value), getCSharpType (typeNode.Element(!!"ref").Value))

let extractDoc (indent : int) (fieldNode : XElement) =
    let indentString = new String('\t', indent)
    match fieldNode.Element(!!"p") with
        | null -> ""
        | _ ->
            fieldNode.Element(!!"p").Value.Trim().Split('\n')
                |> Seq.map (fun s -> s.Trim())
                |> Seq.map (sprintf "%s/// %s" indentString)
                |> String.concat "\r\n"
                |> (fun x -> sprintf "%s/// <summary>\r\n%s\r\n%s/// </summary>\r\n" indentString x indentString)

let formatName (x : string) = x.[0].ToString().ToUpper() + x.[1..]

let formatConstantName (x : string) =
    let words = x.Split('_')
    words |> Array.map (fun x -> x.[0].ToString().ToUpper() + x.[1..].ToLower()) |> String.concat ""

let rec extractCSharpDictionary (fieldNode : XElement) =
    sprintf "Dictionary<%s, %s>"
        (fieldNode.Element(!!"map").Descendants(!!"key") |> Seq.exactlyOne |> extractCSharpType)
        (fieldNode.Element(!!"map").Descendants(!!"value") |> Seq.exactlyOne |> extractCSharpType) // TODO: Eliminate this Seq.head; support unions properly! :(
and extractCSharpUnion (unionNode : XElement) =
    "I" + (unionNode.Elements(!!"ref") |> Seq.map (fun n -> n.Value |> getCSharpType) |> String.concat "Or")
and extractCSharpType (fieldNode : XElement) =
    let cSharpType = fieldNode.XPathSelectElement(".//ref").Value |> getCSharpType
    match fieldNode.Element(!!"list"), fieldNode.Element(!!"map"), fieldNode.Element(!!"union") with
        | null, null, null -> sprintf "%s" cSharpType
        | _ , null, null -> sprintf "%s[]" cSharpType
        | null, _, null -> fieldNode |> extractCSharpDictionary
        | null, null, unionNode -> unionNode |> extractCSharpUnion
        | _, _, _ -> failwithf "Confused by both map and list %s" (fieldNode.Attribute(!!"name").Value)

let populateUnionMapping (unionNode : XElement) =
    let unionInterface = unionNode |> extractCSharpUnion
    unionNode.Descendants(!!"ref") |> Seq.iter (fun i -> unionTypes <- unionTypes.Add(i.Value, unionInterface))

let getField (fieldNode : XElement) =
    sprintf "%s\t\t[JsonProperty(\"%s\")]\r\n\t\tpublic %s %s;\r\n"
        (fieldNode |> extractDoc 2)
        (fieldNode.Attribute(!!"name").Value)
        (fieldNode |> extractCSharpType)
        (fieldNode.Attribute(!!"name").Value |> formatName)

let getEnum (enumCodeNode : XElement) = enumCodeNode.Value |> formatConstantName

let getUnionInterfaces (unionNode : XElement) =
    unionNode |> extractCSharpUnion |> sprintf "\tpublic interface %s\r\n\t{\r\n\t}\r\n"

let getInterfaces s =
    if unionTypes.ContainsKey(s) then
        sprintf " : %s" unionTypes.[s]
    else
        ""

let getType (typeNode : XElement) =
    if typeNode.Element(!!"object") <> null then
        sprintf "%s\tpublic class %s%s\r\n\t{\r\n%s\t}\r\n\r\n"
            (typeNode |> extractDoc 1)
            (typeNode.Attribute(!!"name").Value)
            (typeNode.Attribute(!!"name").Value |> getInterfaces)
            (typeNode.Descendants(!!"field") |> collect getField)
    else if typeNode.Element(!!"ref") <> null then // This type will be mapped onto a primitive
        ""
    else if typeNode.Element(!!"enum") <> null then
        sprintf "%s\tpublic enum %s\r\n\t{\r\n\t\t%s\r\n\t}\r\n\r\n"
            (typeNode |> extractDoc 1)
            (typeNode.Attribute(!!"name").Value)
            ((typeNode.Descendants(!!"code") |> Seq.map getEnum |> String.concat ", "))
    else
        failwithf "Don't know how to handle type %s" (typeNode.Attribute(!!"name").Value)

let getRequest (typeNode : XElement) =
    match typeNode.Element(!!"params") with
        | null -> ""
        | _ ->
            sprintf "\tpublic class %s%sRequest\r\n\t{\r\n%s\t}\r\n\r\n"
                (typeNode.Parent.Attribute(!!"name").Value |> formatName)
                (typeNode.Attribute(!!"method").Value |> formatName)
                (typeNode.Element(!!"params").Descendants(!!"field") |> collect getField)

let getResponse (typeNode : XElement) =
    match typeNode.Element(!!"result") with
        | null -> ""
        | _ ->
            sprintf "\tpublic class %s%sResponse\r\n\t{\r\n%s\t}\r\n\r\n"
                (typeNode.Parent.Attribute(!!"name").Value |> formatName)
                (typeNode.Attribute(!!"method").Value |> formatName)
                (typeNode.Element(!!"result").Descendants(!!"field") |> collect getField)

let getNotification (notificationNode : XElement) =
    match notificationNode.Element(!!"params") with
        | null ->
            sprintf "\t[AnalysisNotification(\"%s.%s\")]\r\n\tpublic class %s%sNotification\r\n\t{\r\n\t}\r\n\r\n"
                (notificationNode.Parent.Attribute(!!"name").Value)
                (notificationNode.Attribute(!!"event").Value)
                (notificationNode.Parent.Attribute(!!"name").Value |> formatName)
                (notificationNode.Attribute(!!"event").Value |> formatName)
        | _ ->
            sprintf "\t[AnalysisNotification(\"%s.%s\")]\r\n\tpublic class %s%sNotification\r\n\t{\r\n%s\t}\r\n\r\n"
                (notificationNode.Parent.Attribute(!!"name").Value)
                (notificationNode.Attribute(!!"event").Value)
                (notificationNode.Parent.Attribute(!!"name").Value |> formatName)
                (notificationNode.Attribute(!!"event").Value |> formatName)
                (notificationNode.Element(!!"params").Descendants(!!"field") |> collect getField)

let getAllJsonTypes () =
    sprintf """// Code generated by UpdateJson.fsx. Do not hand-edit!

using System.Collections.Generic;
using Newtonsoft.Json;

namespace DanTup.DartAnalysis.Json
{
%s
%s
%s
%s
%s
}"""
        (doc.Document.XPathSelectElements("//union") |> collect getUnionInterfaces)
        (doc.Document.XPathSelectElements("//types/type") |> collect getType)
        (doc.Document.XPathSelectElements("//domain/request") |> collect getRequest)
        (doc.Document.XPathSelectElements("//domain/request") |> collect getResponse)
        (doc.Document.XPathSelectElements("//notification") |> collect getNotification)



let getRequestWrapper (typeNode : XElement) =
    let methodName =
        (typeNode.Parent.Attribute(!!"name").Value)
        + "."
        + (typeNode.Attribute(!!"method").Value)

    let className =
        (typeNode.Parent.Attribute(!!"name").Value |> formatName)
        + (typeNode.Attribute(!!"method").Value |> formatName)

    let requestClassName =
        className
        + "Request"

    let responseClassName =
        match typeNode.Element(!!"result") with
            | null -> ""
            | _ ->
                sprintf "%s%sResponse"
                    (typeNode.Parent.Attribute(!!"name").Value |> formatName)
                    (typeNode.Attribute(!!"method").Value |> formatName)

    let baseClass =
        match typeNode.Element(!!"params"), typeNode.Element(!!"result") with
            | null, null -> "Request<Response>"
            | null, _ ->
                sprintf "Request<Response<%s>>"
                    responseClassName
            | _, null ->
                sprintf "Request<%s, Response>"
                    requestClassName
            | _, _ ->
                sprintf "Request<%s, Response<%s>>"
                    requestClassName
                    responseClassName

    let ctor =
        match typeNode.Element(!!"params") with
            | null -> ""
            | _ ->
                sprintf "\t\tpublic %s(%s @params)\r\n\t\t\t: base(@params)\r\n\t\t{\r\n\t\t}"
                    className
                    requestClassName

    sprintf "\t[AnalysisMethod(\"%s\")]\r\n\tpublic class %s : %s\r\n\t{\r\n%s\r\n\t}\r\n\r\n"
        methodName
        className
        baseClass
        ctor
                
let populateTypeMappings () =
    doc.Document.XPathSelectElements("//types/type") |> Seq.iter populateTypeMapping
    doc.Document.XPathSelectElements("//union") |> Seq.iter populateUnionMapping

let getAllRequestTypes () =
    sprintf """// Code generated by UpdateJson.fsx. Do not hand-edit!

using DanTup.DartAnalysis.Json;

namespace DanTup.DartAnalysis
{
%s
}"""
        (doc.Document.XPathSelectElements("//domain/request") |> collect getRequestWrapper)




// Do the stuff!
populateTypeMappings()
File.WriteAllText(outputJsonFilename, getAllJsonTypes())
File.WriteAllText(outputRequestFilename, getAllRequestTypes())
