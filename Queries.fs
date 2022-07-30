module Queries

open Npgsql.FSharp
open System
open Constants


type OhlcRecord =
    { Time: DateTime
      Open: double
      High: double
      Low: double
      Close: double
      Symbol: string }


let GetOhlcQuery (table: Table) =
    sprintf "SELECT (symbol,time,open,high,low,close) from %s LIMIT 5" (table.ToString())

let InsertOhlcQuery (table: Table) =
    sprintf
        "INSERT INTO %s (symbol,time,open,high,low,close) VALUES (@symbol,@time,@open,@high,@low,@close)"
        (table.ToString())


let GetLatestDateTimeQuery (table: Table) =
    sprintf "SELECT time FROM %s ORDER BY time DESC LIMIT 1" (table.ToString())

let IsTimestampPresentQuery (table: Table) =
    sprintf "SELECT EXISTS(SELECT 1 FROM %s WHERE time = @time)" (table.ToString())




let isTimestampPresent (connectionString: string) (table: Table) (time: DateTime) : bool =
    connectionString
    |> Sql.connect
    |> Sql.query (IsTimestampPresentQuery table)
    |> Sql.parameters [ "time", Sql.timestamptz time ]
    |> Sql.executeRow (fun read -> read.bool "exists")

let getLatestDateTime (connectionString: string) (table: Table) : DateTime =
    connectionString
    |> Sql.connect
    |> Sql.query (GetLatestDateTimeQuery table)
    |> Sql.executeRow (fun read -> read.dateTime "time")


let getEthOhlc (connectionString: string) : OhlcRecord list =
    connectionString
    |> Sql.connect
    |> Sql.query (GetOhlcQuery EthOhlc)
    |> Sql.execute (fun read ->
        { Time = read.dateTime "time"
          Open = read.double "open"
          High = read.double "high"
          Low = read.double "low"
          Close = read.double "close"
          Symbol = read.string "symbol" })

let toParameterTuple (record: OhlcRecord) =
    [ "@symbol", Sql.string record.Symbol
      "@time", Sql.timestamptz record.Time
      "@close", Sql.double record.Close
      "@open", Sql.double record.Open
      "@high", Sql.double record.High
      "@low", Sql.double record.Low ]

let insertOhlc (connectionString: string) (table: Table) (records: OhlcRecord seq) =
    connectionString
    |> Sql.connect
    |> Sql.executeTransaction [ (InsertOhlcQuery table), (records |> Seq.toList |> List.map toParameterTuple) ]
