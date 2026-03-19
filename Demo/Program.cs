
global using System;
global using System.Linq;
global using System.Text;
using IncrementAsAService;
using IncrementAsAService.Configuration;
using IncrementAsAService.Core.Domain.Models;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║   IncrementAsAService™ Enterprise Demo              ║");
Console.WriteLine("║   \"Because n++ wasn't scalable enough\"              ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.WriteLine();

// The elegant fluent API
Console.WriteLine("━━━ Demo 1: Fluent API ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("Goal: Add 1 to the number 41.\n");

var result = await Increment.ThisNumber(41)
    .AsUser("DemoUser")
    .BecauseINeedTo("calculate the meaning of life + 1")
    .Urgently()
    .PleaseAsync();

Console.WriteLine($"  Input:      {result.OriginalValue}");
Console.WriteLine($"  Output:     {result.ResultValue}");
Console.WriteLine($"  Success:    {result.IsSuccess}");
Console.WriteLine($"  Strategy:   {result.StrategyUsed}");
Console.WriteLine($"  Duration:   {result.Duration}");
Console.WriteLine($"  Confidence: {result.Confidence}");
Console.WriteLine($"  Cached:     {result.WasCached}");
Console.WriteLine($"  Audit trail entries: {result.AuditTrail.Count}");
Console.WriteLine();

// The quick way 
Console.WriteLine("━━━ Demo 2: JustDoIt™ ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
var quick = await Increment.JustDoIt(99);
Console.WriteLine($"  99 + 1 = {quick.ResultValue} (in {quick.Duration})\n");

// ── Demo 3: Using the Monte Carlo strategy
Console.WriteLine("━━━ Demo 3: Monte Carlo Strategy ━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("(Finding 6 by generating random numbers until we hit it)\n");

var mcResult = await Increment.ThisNumber(59)
    .UsingStrategy("MonteCarlo")
    .BecauseINeedTo("prove that randomness works")
    .PleaseAsync();

Console.WriteLine($"  59 + 1 = {mcResult.ResultValue}");
Console.WriteLine($"  Random attempts needed: {mcResult.RetryCount}");
Console.WriteLine($"  Duration: {mcResult.Duration}\n");

// Demo 4: Batch increment
Console.WriteLine("━━━ Demo 4: Batch Increment ━━━━━━━━━━━━━━━━━━━━━━━━━");
var batch = await Increment.ThoseNumbers(1, 2, 3, 4, 5);

Console.WriteLine($"  Batch size:    {batch.TotalRequests}");
Console.WriteLine($"  Successes:     {batch.SuccessCount}");
Console.WriteLine($"  Failures:      {batch.FailureCount}");
Console.WriteLine($"  Success rate:  {batch.SuccessRate:P0}");
Console.WriteLine($"  Total time:    {batch.TotalDuration}");
Console.Write("  Results:       ");
Console.WriteLine(string.Join(", ",
    batch.Results.OrderBy(r => r.OriginalValue)
        .Select(r => $"{r.OriginalValue}→{r.ResultValue}")));
Console.WriteLine();

// ── Demo 5: Superstitious number
Console.WriteLine("━━━ Demo 5: Incrementing the Number 13 ━━━━━━━━━━━━━━");
var spooky = await Increment.ThisNumber(13)
    .BecauseINeedTo("test superstition validator")
    .PleaseAsync();

Console.WriteLine($"  13 + 1 = {spooky.ResultValue}");
var warnings = spooky.AuditTrail.Where(a => a.Contains("[WARNING]")).ToList();
foreach (var w in warnings)
    Console.WriteLine($"  {w}");
Console.WriteLine();

// ── Demo 6: Cache hit 
Console.WriteLine("━━━ Demo 6: Cache Hit ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
var first = await Increment.JustDoIt(7);
var second = await Increment.JustDoIt(7);
Console.WriteLine($"  First call:  7→{first.ResultValue} (cached: {first.WasCached}, {first.Duration})");
Console.WriteLine($"  Second call: 7→{second.ResultValue} (cached: {second.WasCached}, {second.Duration})");
Console.WriteLine();

// ── Audit trail sample 
Console.WriteLine("\n━━━ Sample Audit Trail (from Demo 1) ━━━━━━━━━━━━━━━━");
foreach (var entry in result.AuditTrail.Take(20))
    Console.WriteLine($"  {entry}");
if (result.AuditTrail.Count > 20)
    Console.WriteLine($"  ... and {result.AuditTrail.Count - 20} more entries");

Console.WriteLine("\n╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║   Thank you for using IncrementAsAService™           ║");
Console.WriteLine("║   Total operations performed: n + 1                  ║");
Console.WriteLine("║   ROI: Incalculable                                  ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
