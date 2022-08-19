namespace BugReport.Quartz;

using global::Quartz;
using global::Quartz.Impl;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // This creates a global logger that can be used during
        // bootstrap, but can later be configured using the host
        // builder
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Starting Test Project");
        
        var exitCode = 0;
        try
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog((h, s, l) =>
                {
                    // l.MinimumLevel.Error();
                    l.MinimumLevel.Information();
                    
                    l.MinimumLevel.Override("Quartz", LogEventLevel.Information);
                    
                    l.WriteTo.Console();
                })
                .ConfigureServices((hb, services) =>
                {
                    // Configure Quartz
                    services.AddQuartz(q =>
                    {
                        q.UseMicrosoftDependencyInjectionJobFactory();
                        var maxConcurrency = Environment.ProcessorCount * 4;
                        q.MisfireThreshold = TimeSpan.FromSeconds(60);
                        q.UseDefaultThreadPool(options => { options.MaxConcurrency = maxConcurrency; });
                        q.SetProperty(StdSchedulerFactory.PropertySchedulerMaxBatchSize, maxConcurrency.ToString());
                        q.UsePersistentStore(s =>
                        {
                            s.UseProperties = true;
                            s.UseClustering();
                            s.UsePostgres(o =>
                            {
                                o.ConnectionString =
                                    "Host=localhost;Port=5432;Database=demo;Username=demo;Password=demo;Include Error Detail=true;";
                            });
                            s.UseJsonSerializer();
                            
                        });
                    });
                    services.AddQuartzHostedService(options =>
                    {
                        // when shutting down we want jobs to complete gracefully
                        options.WaitForJobsToComplete = false;
                    });
                }).Build();
            
            await host.RunAsync();
            
            Console.WriteLine("HUH");
        }
        catch (Exception ex)
        {
            Console.WriteLine("BOB: {0}", ex.Message);
            Log.Fatal(ex, "Encountered an error. Shutting Down");
            exitCode = -1;
        }
        finally
        {
            Log.CloseAndFlush();
        }

        return exitCode;
    }
}