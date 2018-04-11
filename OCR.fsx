//#r @"C:\Users\Jeremiah Jeschke\Documents\Visual Studio 2015\Projects\officeAutomata\packages\FSharp.Data.2.3.2\lib\net40\FSharp.Data.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6\System.Net.Http.dll"
//#r @"C:\Users\Jeremiah Jeschke\Documents\Visual Studio 2015\Projects\Contana\packages\Microsoft.AspNet.WebApi.Core.5.2.3\lib\net45\System.Web.Http.dll"
#r @"C:\Users\Jeremiah Jeschke\Documents\Visual Studio 2015\Projects\officeAutomata\packages\FSharp.Data.2.4.3\lib\net45\FSharp.Data.dll"
#r @"C:\Users\Jeremiah Jeschke\Documents\Visual Studio 2015\Projects\officeAutomata\packages\Newtonsoft.Json.dll"
#r @"C:\Users\Jeremiah Jeschke\Documents\Visual Studio 2015\Projects\Packages\System.Drawing.dll"

open System 
open System.Diagnostics
open System.IO
open System.Text.RegularExpressions
open System.Net.Http.Headers
open System.Net.Http
open System.Drawing
open System.Drawing.Imaging
open System.Drawing.Drawing2D
open System.Windows.Forms
open FSharp.Data
open FSharp.Data.JsonExtensions
open Newtonsoft.Json.Linq

let timer1 = new Stopwatch()
let mutable proc1 = Process.GetCurrentProcess()
let mutable time1 = proc1.TotalProcessorTime   
let time () =  //f
    let proc = Process.GetCurrentProcess()
    proc1 <- proc
    let cpu_time_stamp = proc.TotalProcessorTime 
    time1 <- cpu_time_stamp
    timer1.Start()

let timeend () = 
        let cpu_time = (proc1.TotalProcessorTime-time1).TotalMilliseconds
        timer1.Stop()
        printfn "%A" ("CPU time: " + cpu_time.ToString() + " Absolute time: " + timer1.ElapsedMilliseconds.ToString()) 
    
type Webfile = JsonProvider<Sample="""{"language": "en","textAngle": 0.0,"orientation": "Up",
                                          "regions": [
                                            {
                                              "boundingBox": "29,76,352,38",
                                              "lines": [
                                                {
                                                  "boundingBox": "29,76,352,38",
                                                  "words": [
                                                    {
                                                      "boundingBox": "29,76,203,38",
                                                      "text": "apaull@exte"
                                                    },
                                                    {
                                                      "boundingBox": "239,76,41,29",
                                                      "text": "nsi"
                                                    },
                                                    {
                                                      "boundingBox": "287,84,21,21",
                                                      "text": "s."
                                                    },
                                                    {
                                                      "boundingBox": "315,84,66,22",
                                                      "text": "com"
                                                    }
                                                  ]
                                                }
                                              ]
                                            }
                                          ]
                                        }""">
                                        
let parseWeb (json) = 
        let docAsync = Webfile.Parse(json)        
        let regions = docAsync.Regions
        let mutable text = ""
        regions |> Array.iter (fun a -> a.Lines |> Array.iter (fun b -> b.Words |> Array.iter (fun c -> text <- text + "  " + c.Text)))
        printfn "%A" text
        text

let GetImageAsByteArray  (imageFilePath:string) = //byte[] (
    let fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read)
    let binaryReader = new BinaryReader(fileStream)
    binaryReader.ReadBytes((int)fileStream.Length)

//Change to your API Key
let MakeOCRRequest (image: Image) =
        async
            {
                try
                    let client = new HttpClient()
                    // Request headers. Replace the example key with a valid subscription key.
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", " API KEY HERE ");

                    let requestParameters = "language=en&detectOrientation =false" 
                    let uri = "https://westus.api.cognitive.microsoft.com/vision/v1.0/ocr?" + requestParameters
                    let byteData = ImageConverter().ConvertTo(image, typeof<byte[]>) :?> byte[]

                    use content = new ByteArrayContent(byteData)
                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json" and "multipart/form-data".
                    content.Headers.ContentType <- new MediaTypeHeaderValue("application/octet-stream")

                    let response = client.PostAsync(uri, content).Result
                    let result = response.Content.ReadAsStringAsync().Result
                    //printfn "%A" result
                    return parseWeb result
                with
                | exn -> printfn "%A" exn
                         return ""
        } |> Async.RunSynchronously


try 
    printfn "Start"
    time ()
    let path = Image.FromFile @"C:\Imange_File.jpg"
    let text = MakeOCRRequest(path)
    timeend ()

    printfn "%A" text
with
| exn -> printfn "%A" exn

printfn "Done"
System.Console.ReadKey(true) |>ignore
