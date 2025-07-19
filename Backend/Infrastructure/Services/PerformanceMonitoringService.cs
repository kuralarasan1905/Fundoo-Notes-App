using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// Service for monitoring application performance metrics
    /// </summary>
    public interface IPerformanceMonitoringService
    {
        void RecordQueryExecutionTime(string queryName, TimeSpan executionTime);
        void RecordApiRequestTime(string endpoint, TimeSpan executionTime);
        void IncrementCounter(string counterName);
        void RecordMemoryUsage();
        PerformanceMetrics GetMetrics();
        void ResetMetrics();
    }

    public class PerformanceMonitoringService : IPerformanceMonitoringService
    {
        private readonly ILogger<PerformanceMonitoringService> _logger;
        private readonly ConcurrentDictionary<string, PerformanceCounter> _counters;
        private readonly ConcurrentDictionary<string, List<TimeSpan>> _executionTimes;
        private readonly object _lockObject = new object();

        public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
        {
            _logger = logger;
            _counters = new ConcurrentDictionary<string, PerformanceCounter>();
            _executionTimes = new ConcurrentDictionary<string, List<TimeSpan>>();
        }

        public void RecordQueryExecutionTime(string queryName, TimeSpan executionTime)
        {
            try
            {
                var key = $"query_{queryName}";
                _executionTimes.AddOrUpdate(key, 
                    new List<TimeSpan> { executionTime },
                    (k, list) => 
                    {
                        lock (_lockObject)
                        {
                            list.Add(executionTime);
                            // Keep only last 100 measurements to prevent memory issues
                            if (list.Count > 100)
                            {
                                list.RemoveAt(0);
                            }
                            return list;
                        }
                    });

                // Log slow queries
                if (executionTime.TotalMilliseconds > 1000) // > 1 second
                {
                    _logger.LogWarning("Slow query detected: {QueryName} took {ExecutionTime}ms", 
                        queryName, executionTime.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording query execution time for {QueryName}", queryName);
            }
        }

        public void RecordApiRequestTime(string endpoint, TimeSpan executionTime)
        {
            try
            {
                var key = $"api_{endpoint}";
                _executionTimes.AddOrUpdate(key, 
                    new List<TimeSpan> { executionTime },
                    (k, list) => 
                    {
                        lock (_lockObject)
                        {
                            list.Add(executionTime);
                            if (list.Count > 100)
                            {
                                list.RemoveAt(0);
                            }
                            return list;
                        }
                    });

                // Log slow API requests
                if (executionTime.TotalMilliseconds > 2000) // > 2 seconds
                {
                    _logger.LogWarning("Slow API request: {Endpoint} took {ExecutionTime}ms", 
                        endpoint, executionTime.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording API request time for {Endpoint}", endpoint);
            }
        }

        public void IncrementCounter(string counterName)
        {
            try
            {
                _counters.AddOrUpdate(counterName, 
                    new PerformanceCounter { Name = counterName, Count = 1, LastUpdated = DateTime.UtcNow },
                    (k, counter) => 
                    {
                        counter.Count++;
                        counter.LastUpdated = DateTime.UtcNow;
                        return counter;
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing counter {CounterName}", counterName);
            }
        }

        public void RecordMemoryUsage()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var memoryUsage = process.WorkingSet64;
                
                _counters.AddOrUpdate("memory_usage_bytes", 
                    new PerformanceCounter { Name = "memory_usage_bytes", Count = memoryUsage, LastUpdated = DateTime.UtcNow },
                    (k, counter) => 
                    {
                        counter.Count = memoryUsage;
                        counter.LastUpdated = DateTime.UtcNow;
                        return counter;
                    });

                // Log high memory usage
                var memoryMB = memoryUsage / (1024 * 1024);
                if (memoryMB > 500) // > 500 MB
                {
                    _logger.LogWarning("High memory usage detected: {MemoryUsage}MB", memoryMB);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording memory usage");
            }
        }

        public PerformanceMetrics GetMetrics()
        {
            try
            {
                var metrics = new PerformanceMetrics
                {
                    Counters = _counters.Values.ToList(),
                    ExecutionTimes = new Dictionary<string, ExecutionTimeMetrics>(),
                    GeneratedAt = DateTime.UtcNow
                };

                foreach (var kvp in _executionTimes)
                {
                    lock (_lockObject)
                    {
                        var times = kvp.Value.ToList();
                        if (times.Any())
                        {
                            metrics.ExecutionTimes[kvp.Key] = new ExecutionTimeMetrics
                            {
                                Name = kvp.Key,
                                Count = times.Count,
                                AverageMs = times.Average(t => t.TotalMilliseconds),
                                MinMs = times.Min(t => t.TotalMilliseconds),
                                MaxMs = times.Max(t => t.TotalMilliseconds),
                                MedianMs = GetMedian(times.Select(t => t.TotalMilliseconds).ToList())
                            };
                        }
                    }
                }

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                return new PerformanceMetrics { GeneratedAt = DateTime.UtcNow };
            }
        }

        public void ResetMetrics()
        {
            try
            {
                _counters.Clear();
                _executionTimes.Clear();
                _logger.LogInformation("Performance metrics reset");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting performance metrics");
            }
        }

        private double GetMedian(List<double> values)
        {
            if (!values.Any()) return 0;
            
            values.Sort();
            var count = values.Count;
            
            if (count % 2 == 0)
            {
                return (values[count / 2 - 1] + values[count / 2]) / 2.0;
            }
            else
            {
                return values[count / 2];
            }
        }
    }

    public class PerformanceCounter
    {
        public string Name { get; set; } = string.Empty;
        public long Count { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ExecutionTimeMetrics
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public double AverageMs { get; set; }
        public double MinMs { get; set; }
        public double MaxMs { get; set; }
        public double MedianMs { get; set; }
    }

    public class PerformanceMetrics
    {
        public List<PerformanceCounter> Counters { get; set; } = new List<PerformanceCounter>();
        public Dictionary<string, ExecutionTimeMetrics> ExecutionTimes { get; set; } = new Dictionary<string, ExecutionTimeMetrics>();
        public DateTime GeneratedAt { get; set; }
    }
}
