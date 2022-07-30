module TwelveDataApi

open System.Net.Http
open System

open Queries


let apiKey = Environment.GetEnvironmentVariable("API_KEY")

let getAsync (client: HttpClient) (url: string) =
    task {
        let! response = client.GetAsync(url)
        response.EnsureSuccessStatusCode() |> ignore
        let! content = response.Content.ReadAsStringAsync()
        return content
    }


let toOhlcRecords (symbol: string) (csv: string) =
    let symbolWithoutSlash = symbol.Replace("/", "")

    csv.Split("\n")
    |> Seq.tail
    |> Seq.filter (fun row -> not (String.IsNullOrEmpty(row)))
    |> Seq.map (fun row ->
        let elems = row.Split(";")
        let time = DateTime.Parse(elems[0] + "Z").ToUniversalTime()

        { Symbol = symbolWithoutSlash
          Time = time
          Open = double elems[1]
          High = double elems[2]
          Low = double elems[3]
          Close = double elems[4] })


let getOhlcMinute (httpClient: HttpClient) (pair: string) (startDate: DateTime) (isLocalStartDate: bool) =
    let startDateStr =
        if isLocalStartDate then
            startDate
                .ToUniversalTime()
                .ToString("yyyy-MM-dd HH:mm:ss")
        else
            startDate.ToString("yyyy-MM-dd HH:mm:ss")

    let res =
        getAsync
            httpClient
            $"https://api.twelvedata.com/time_series?apikey={apiKey}&interval=1min&symbol={pair}&start_date={startDateStr}&format=CSV"

    res.Wait()
    toOhlcRecords pair res.Result
