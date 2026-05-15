# CyptoSeller

Multi-exchange cryptocurrency arbitrage detection platform. Uses WebSocket feeds to collect real-time trade and order book data from 6+ exchanges, publishes through Redis, and analyzes cross-exchange and triangular arbitrage opportunities.

## Architecture

```
┌─────────────────────────┐     ┌──────────────────┐
│   DataGather            │     │  DataStreamDisplayer │
│   (WebSocket clients)   │────▶│  (Order book agg.)   │────▶ CSV
│   Binance, Kraken, OKX, │     │  Computes max-bid/   │
│   Bitstamp, NiceHash    │     │  min-ask per symbol  │
└──────────┬──────────────┘     └──────────┬───────────┘
           │ Pub/Sub                       │ Redis
           ▼                               ▼
┌─────────────────────────────────────────────────────┐
│                    Redis                              │
│            (Message broker + cache)                   │
└──────────┬──────────────────────────────┬────────────┘
           │ Pub/Sub                     │ Key/Value
           ▼                             ▼
┌──────────────────┐          ┌──────────────────────┐
│  WebRouter (API) │          │  Chainer Engine       │
│  ASP.NET Core    │          │  Triangular arbitrage │
│  REST + Swagger  │          │  chain calculation    │
└──────────────────┘          └──────────────────────┘
```

### Projects

| Project | Description |
|---------|-------------|
| **CoreLib** | Shared library: chainer engine, WebSocket base, Redis utilities, serialization, data models |
| **DataGather** | WebSocket clients per exchange, collects trades + order book updates into Redis |
| **DataStreamDisplayer** | Aggregates order books across exchanges, computes best bid/ask, writes CSV logs |
| **WebRouter** | ASP.NET Core REST API with Swagger (stub) |
| **SellChainer** | Arbitrage chain executor: links buy/sell pairs into triangular routes |
| **GraphLoop** | Builds a graph of connected coin pairs to find viable triangular arbitrage paths |
| **NodeStatistics** | Redis subscription message rate monitor |
| **OrderbookVisualiser** | Windows Forms desktop order book viewer |
| **CoreLib.Tests** | Unit tests |

## Tech Stack

- **C# / .NET 10.0**
- **Redis** (StackExchange.Redis) for pub/sub data distribution
- **WebSocket** (System.Net.WebSockets) for exchange connections
- **MessagePack** for compact binary serialization
- **Newtonsoft.Json** for REST API responses
- **Polly** for resilience/retry policies
- **ASP.NET Core** for REST API
- **xUnit** for testing

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Redis instance (default: `localhost:6379`)

## Getting Started

1. **Start Redis** (if not running):
   ```bash
   docker run -d -p 6379:6379 redis
   ```

2. **Build the solution**:
   ```bash
   dotnet build
   ```

3. **Run tests**:
   ```bash
   dotnet test
   ```

4. **Run DataGather** (collects exchange data):
   ```bash
   dotnet run --project DataGather
   ```

5. **Run DataStreamDisplayer** (view order book margins):
   ```bash
   dotnet run --project DataStreamDisplayer
   ```

## Configuration

Configure via environment variables:

| Variable | Default | Description |
|----------|---------|-------------|
| `REDIS_CONNECTION_STRING` | `localhost:6379,abortConnect=false` | Redis connection string |
| `STATS_ENDPOINT` | `http://localhost:5000/give/` | Statistics HTTP endpoint |

Example:
```bash
export REDIS_CONNECTION_STRING="myredis:6379,abortConnect=false"
dotnet run --project DataGather
```

## Testing

```bash
dotnet test
```

Tests cover:
- Chainer math (buy/sell amount calculation with fees)
- CoinPair operations
- BasicObj serialization properties
- OrderBookObj comparison logic
- ChainerFactory chain validation
