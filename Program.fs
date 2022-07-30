open dotenv.net
DotEnv.Load()

open System
open System.Net.Http

open Queries

open Constants

let pairToTable (pair: string) =
    match pair with
    | "ETHUSD" -> Some EthOhlc
    | "BTCUSD" -> Some BtcOhlc
    | "ALGOUSD" -> Some AlgoOhlc
    | _ -> None

let pairToApiPair (pair: string) =
    match pair with
    | "ETHUSD" -> Some "ETH/USD"
    | "BTCUSD" -> Some "BTC/USD"
    | "ALGOUSD" -> Some "ALGO/USD"
    | _ -> None

[<EntryPoint>]
let main args =

    let pair = args[0]

    let table = pairToTable pair

    if table.IsNone then
        raise (Exception($"{pair} does not map to any table"))


    let apiPair = pairToApiPair pair

    if apiPair.IsNone then
        raise (Exception($"{pair} does not map to any api pair"))


    DotEnv.Load()
    use httpClient = new HttpClient()
    let connstr = Environment.GetEnvironmentVariable("CONNECTION_STRING")

    let startDate =
        try
            getLatestDateTime connstr table.Value
        with
        | :? NoResultsException -> DateTime.Now.AddDays(-1).ToUniversalTime()

    printfn "Latest start date: %A" startDate
    printfn "Fetching data from api"

    let ohlc =
        TwelveDataApi.getOhlcMinute httpClient apiPair.Value startDate false
        // Only insert records for which we don't already have an inserted timestamp
        |> Seq.filter (fun record -> not (isTimestampPresent connstr table.Value record.Time))

    printfn "Inserting: %d records" (Seq.length ohlc)

    insertOhlc connstr table.Value ohlc |> ignore
    printfn "Insert complete"

    let newStartDate = getLatestDateTime connstr table.Value
    printfn "New latest start date in db: %A" newStartDate

    0
