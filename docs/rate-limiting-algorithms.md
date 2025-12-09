# Rate Limiting Algorithms:

## Token bucket:

This algorithm regulates the amount of requests by attaching a token to each request and if a request is missing a token - it gets turned down. Tokens are generated in a time basis in a continuous manner.

### Advantages:
* Relatively simple to understand and implement.
* Can handle bursts of traffic.
* Flexible in its approach.

### Disadvantages:
* May require an adjustment when there is a slowdown in traffic.
* Requires careful tuning of token refill rate and capacity.

### How does it work?
* Initiate a counter and set it to 0 and a capacity (e.g., 100 tokens).
* Each n time unit a token is added but never beyond the capacity.
* Each request consumes a token.
* When tokens are depleted - no requests are sent through.

```csharp
class TokenBucket
{
    int Rate { get; set; }
    int Capacity { get; set; }
    int Tokens { get; set; }
    DateTime LastRefill { get; set; }

    public TokenBucket(int rate, int capacity)
    {
        Rate = rate;
        Capacity = capacity;
        Tokens = capacity;
        LastRefill = DateTime.UtcNow;
    }

    public bool AllowRequest()
    {
        bool isAllowed = true;
        RefillTokens();

        if (Tokens > 0)
            Tokens--;
        else
            isAllowed = false;

        return isAllowed;
    }

    private void RefillTokens()
    {
        var currentTimestamp = DateTime.UtcNow;
        var elapsedSeconds = (currentTimestamp - LastRefill).TotalSeconds;

        if (elapsedSeconds <= 0) return;

        var tokensToAdd = (int)(elapsedSeconds * Rate);
        if (tokensToAdd <= 0) return;

        Tokens = Math.Min(Capacity, Tokens + tokensToAdd);
        LastRefill = currentTimestamp;
    }
}

```

## Leaky bucket:

The bucket holds a constant size of requests that "leaks" and allows it to shrink progressively. New requests are added to the bucket and once full it won't accept them to the bucket. 

### Advantages:
* Handles bursts of traffic by handling steady output rate.
* Ensures fair distribution of resources fairly by users.
* Relatively simple to understand and implement.

### Disadvantages:
* Requires more logic to manage the tokens.
* May struggle to handle the short lived outbursts that exceed the bucket capacity.
* Strict with its implementation limits required bursts.
* Choosing optimal size and refill rate might be complex.

## How does it work?
* Data arrives to the bucket at irregular intervals.
* Each unit of data is held in the bucket until it can get processed.
* Data is processed in a constant rate determined by the leak rate.
* If a bucket is full the data is rejected.

```csharp
class LeakyBucket
{
    int Capacity { get; set; }
    int LeakRate  { get; set; }
    int Size { get; set; }
    DateTime LastUpdate { get; set; }

    public LeakyBucket(int leakRate , int capacity)
    {
        LeakRate  = leakRate ;
        Capacity = capacity;
        Size = 0;
        LastUpdate = DateTime.UtcNow;
    }

    public bool AddData(int dataSize)
    {
        bool isAllowed = true;

        var currentTimestamp = DateTime.UtcNow;
        var elapsedSeconds = (currentTimestamp - LastUpdate).TotalSeconds;

        Size -= elapsedSeconds * LeakRate;
        if (Size < 0) Size = 0;

        LastUpdate = now;

        if (Size + dataSize > Capacity)
            isAllowed = false;
        else
            Size += dataSize;

        return isAllowed;
    }
}
```

## Fixed window:

This algorithms creates fixed time slots known as windows, and it restricts the number of requests to a specific number in the window. If the limit is met, no additional requests can be processed until the next window.

### Advantages:
* Relatively simple to understand and implement.
* Works best with stable flow of traffic.

### Disadvantages:
* Fixed window has a boundary problem where a user can hit the limit at the end of one window and again at the beginning of the next, creating a short-term burst.

### How does it work?
* A window is defined - (e.g. one minute, one hour) and the limit of requests permitted.
* Requests exceeding the threshold are deferred or rejected.

```csharp
class FixedWindow
{
    int Requests { get; set; }
    int Capacity { get; set; }
    TimeSpan WindowSize { get; set; }
    DateTime WindowStart { get; set; }

    public FixedWindow(TimeSpan windowSize, int capacity)
    {
        WindowSize = windowSize;
        Capacity = capacity;
        Requests = 0;
        WindowStart = DateTime.UtcNow;
    }

    public bool AllowRequest()
    {
        bool isAllowed = true;

        var currentTimestamp = DateTime.UtcNow;
        if (currentTimestamp - WindowStart >= WindowSize)
        {
            Requests = 0;
            WindowStart = currentTimestamp;
        }

        if (++Requests > Capacity)
            isAllowed = false;
        
        return isAllowed;
    }
}
```

## Sliding window:

This is a variation of the fixed window and leaky bucket algorithms. It keeps a moving time frame and limits the requests in that frame.

### Advantages:
* It is less strict than fixed window and allows flexibility.
* Handles inconsistent traffic better.

### Disadvantages:
* More complicated to execute.
* Requires more logic/memory to handle.

```csharp
class SlidingWindow
{
    int Size { get; set; }
    Queue<DateTime> Requests { get; set;}
    int Capacity { get; set; }

    public SlidingWindow(int size, int capacity)
    {
        Size = size;
        Capacity = capacity;
        Requests = new Queue<DateTime>();
    }

    public bool AllowRequest()
    {
        bool isAllowed = true;

        var currentTimestamp = DateTime.UtcNow;
        while (Requests.Count > 0 && (currentTimestamp - Requests.Peek()).TotalSeconds >= WindowSizeSeconds)
        {
            Requests.Dequeue();
        }

        if (Requests.Count >= Capacity)
            isAllowed = false;
        else
            Requests.Enqueue(currentTimestamp);
        
        return isAllowed;
    }
}
```

### How does it work?
* The sliding window log algorithm maintains a log of timestamps for each request received.
* Requests older than a predefined time interval are removed from the log, and new requests are added.
* The rate of requests is calculated based on the number of requests within the sliding window.

## Note to self:
When I implement this in C#, I'll need to be careful with time units (TimeSpan, milliseconds vs seconds) and concurrency/thread safety, especially once I move to Redis and multiple instances.