module Constants

type Table =
    | EthOhlc
    | AlgoOhlc
    | BtcOhlc
    override this.ToString() =
        match this with
        | EthOhlc -> "testing.eth_ohlc"
        | AlgoOhlc -> "testing.algo_ohlc"
        | BtcOhlc -> "testing.btc_ohlc"
