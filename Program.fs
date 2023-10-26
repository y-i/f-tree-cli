open System.IO
open Argu

type CliArguments =
    | Root_Directory of path:string
    | [<AltCommandLineAttribute("-d")>] Depth of depth:int

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | _ -> "TODO"

let parser = ArgumentParser.Create<CliArguments>()

let result = parser.Parse()
let args = result.GetAllResults()

let targetDirectory = result.GetResult(Root_Directory, "./")
let maxDepth = result.GetResult(Depth, 1)

if not (Directory.Exists(targetDirectory)) then
    printfn "Directory %s does not exists" targetDirectory
    exit 1
else

let rec show (entryName: string) (depth: int) =
    if depth = maxDepth + 1 then
        ()
    else
    
    for i in 0..depth-1 do
        printf " "
    printf "- "
    if depth = 0 then
        printfn "%s" entryName
    else
        printfn "%s" (Path.GetFileName entryName)

    if not (Directory.Exists(entryName)) then
        ()
    else

    let entries = Array.sort(Directory.GetFileSystemEntries(entryName))
    for entry in entries do
        show entry (depth + 1)

let rec asyncShow (entryName: string) (depth: int): Async<string> = async {
    if depth = maxDepth + 1 then
        return ""
    else
    
    let mutable ret = ""
    for i in 0..depth-1 do
        ret <- ret + " "
    ret <- ret + "- "
    if depth = 0 then
        ret <- ret + entryName + "\n"
    else
        ret <- ret + (Path.GetFileName entryName) + "\n"

    if not (Directory.Exists(entryName)) then
        return ret
    else

    let entries = Array.sort(Directory.GetFileSystemEntries(entryName))
    let! strings = (Async.Parallel [ for entry in entries -> asyncShow entry (depth + 1) ])

    return List.fold (fun p s -> p+s) ret (strings |> Array.toList)
}

show targetDirectory 0

// for i in 1..1000 do
    // show targetDirectory 0 // 2.92s user 2.16s system 92% cpu 5.509 total
    // asyncShow targetDirectory 0 |> Async.RunSynchronously |> printfn "%s" // 4.11s user 10.36s system 302% cpu 4.785 total
