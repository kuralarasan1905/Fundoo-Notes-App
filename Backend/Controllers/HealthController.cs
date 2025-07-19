using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Diagnostics;
using fundoo_notes.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using fundoo_notes.Infrastructure.Data;

namespace fundoo_notes.Controllers
{
    /// <summary>
    /// Health check controller for monitoring application status
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly FundooNotesDbContext _context;
        private readonly IPerformanceMonitoringService? _performanceService;

        public HealthController(
            ILogger<HealthController> logger,
            FundooNotesDbContext context,
            IPerformanceMonitoringService? performanceService = null)
        {
            _logger = logger;
            _context = context;
            _performanceService = performanceService;
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        /// <returns>Application health status</returns>
        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public ActionResult GetHealth()
        {
            var response = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                MachineName = Environment.MachineName,
                Message = "Fundoo Notes API is running successfully"
            };

            _logger.LogInformation("Health check requested - Status: Healthy");
            return Ok(response);
        }

        /// <summary>
        /// Detailed health check with system information
        /// </summary>
        /// <returns>Detailed application health status</returns>
        [HttpGet("detailed")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public ActionResult GetDetailedHealth()
        {
            var response = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Application = new
                {
                    Name = "Fundoo Notes API",
                    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    StartTime = Process.GetCurrentProcess().StartTime,
                    Uptime = DateTime.Now - Process.GetCurrentProcess().StartTime
                },
                System = new
                {
                    MachineName = Environment.MachineName,
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingSet = Environment.WorkingSet,
                    Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                    Is64BitProcess = Environment.Is64BitProcess
                },
                Runtime = new
                {
                    Version = Environment.Version.ToString(),
                    FrameworkDescription = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
                }
            };

            _logger.LogInformation("Detailed health check requested");
            return Ok(response);
        }

        /// <summary>
        /// Database health check
        /// </summary>
        /// <returns>Database connectivity status</returns>
        [HttpGet("database")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> GetDatabaseHealth()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var canConnect = await _context.Database.CanConnectAsync();
                stopwatch.Stop();

                if (canConnect)
                {
                    var response = new
                    {
                        Status = "Healthy",
                        Database = "Connected",
                        ConnectionTime = $"{stopwatch.ElapsedMilliseconds}ms",
                        Timestamp = DateTime.UtcNow
                    };

                    _logger.LogInformation("Database health check - Status: Healthy, Connection time: {ConnectionTime}ms",
                        stopwatch.ElapsedMilliseconds);
                    return Ok(response);
                }
                else
                {
                    var response = new
                    {
                        Status = "Unhealthy",
                        Database = "Disconnected",
                        Timestamp = DateTime.UtcNow
                    };

                    _logger.LogWarning("Database health check - Status: Unhealthy");
                    return StatusCode(503, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                var response = new
                {
                    Status = "Unhealthy",
                    Database = "Error",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
                return StatusCode(503, response);
            }
        }

        /// <summary>
        /// Performance metrics endpoint
        /// </summary>
        /// <returns>Application performance metrics</returns>
        [HttpGet("metrics")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public ActionResult GetMetrics()
        {
            try
            {
                if (_performanceService == null)
                {
                    return Ok(new
                    {
                        Status = "Performance monitoring not available",
                        Timestamp = DateTime.UtcNow
                    });
                }

                var metrics = _performanceService.GetMetrics();
                var process = Process.GetCurrentProcess();

                var response = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Performance = metrics,
                    Memory = new
                    {
                        WorkingSetMB = process.WorkingSet64 / (1024 * 1024),
                        PrivateMemoryMB = process.PrivateMemorySize64 / (1024 * 1024),
                        GCTotalMemoryMB = GC.GetTotalMemory(false) / (1024 * 1024)
                    },
                    GarbageCollection = new
                    {
                        Gen0Collections = GC.CollectionCount(0),
                        Gen1Collections = GC.CollectionCount(1),
                        Gen2Collections = GC.CollectionCount(2)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving performance metrics");
                return Ok(new
                {
                    Status = "Error retrieving metrics",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
