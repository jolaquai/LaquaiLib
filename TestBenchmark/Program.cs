using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using Perfolizer.Metrology;

namespace JsonSerializationBenchmark;

// Complex nested object model for benchmarking
public class PrintJob
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime SubmissionTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public PrintStatus Status { get; set; }
    public Customer Customer { get; set; }
    public List<DocumentSpec> Documents { get; set; }
    public PrintSettings Settings { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
    public decimal TotalCost { get; set; }
}

public enum PrintStatus
{
    Queued,
    Processing,
    Completed,
    Failed,
    Cancelled
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Address BillingAddress { get; set; }
    public Address ShippingAddress { get; set; }
    public string AccountNumber { get; set; }
    public CustomerType Type { get; set; }
    public decimal DiscountRate { get; set; }
}

public enum CustomerType
{
    Individual,
    Corporate,
    Government,
    NonProfit
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
}

public class DocumentSpec
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string FileFormat { get; set; }
    public int PageCount { get; set; }
    public List<PrintOption> Options { get; set; }
    public Dictionary<string, string> CustomSettings { get; set; }
}

public class PrintOption
{
    public string Name { get; set; }
    public string Value { get; set; }
    public decimal AdditionalCost { get; set; }
}

public class PrintSettings
{
    public PaperSize PaperSize { get; set; }
    public bool Duplex { get; set; }
    public ColorMode ColorMode { get; set; }
    public int Copies { get; set; }
    public BindingType? Binding { get; set; }
    public decimal PaperWeight { get; set; }
    public List<string> SpecialInstructions { get; set; }
}

public enum PaperSize
{
    A4,
    Letter,
    Legal,
    Tabloid,
    Custom
}

public enum ColorMode
{
    BlackAndWhite,
    Grayscale,
    FullColor
}

public enum BindingType
{
    None,
    Stapled,
    Coil,
    Perfect,
    Saddle
}

// Source generator context
[JsonSourceGenerationOptions(
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PrintJob))]
[JsonSerializable(typeof(List<PrintJob>))]
public partial class JsonContext : JsonSerializerContext
{
}

[MemoryDiagnoser]
[RankColumn]
public class SerializationBenchmarks
{
    private PrintJob _singleJob;
    private List<PrintJob> _multipleJobs;
    private readonly JsonSerializerOptions _standardOptions;

    public SerializationBenchmarks()
    {
        // Create consistent serializer options
        _standardOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Initialize benchmark data
        InitializeTestData();
    }

    private void InitializeTestData()
    {
        // Create sample print job with complex nested objects
        _singleJob = new PrintJob
        {
            Id = Guid.NewGuid(),
            Name = "Annual Report 2024",
            SubmissionTime = DateTime.Now.AddDays(-2),
            CompletionTime = DateTime.Now.AddDays(-1),
            Status = PrintStatus.Completed,
            Customer = new Customer
            {
                Id = 12345,
                Name = "Acme Corporation",
                Email = "print@acmecorp.com",
                AccountNumber = "ACME-001",
                Type = CustomerType.Corporate,
                DiscountRate = 0.15m,
                BillingAddress = new Address
                {
                    Street = "123 Business St",
                    City = "Metropolis",
                    State = "NY",
                    PostalCode = "10001",
                    Country = "USA"
                },
                ShippingAddress = new Address
                {
                    Street = "456 Warehouse Blvd",
                    City = "Metropolis",
                    State = "NY",
                    PostalCode = "10002",
                    Country = "USA"
                }
            },
            Documents = new List<DocumentSpec>
            {
                new DocumentSpec
                {
                    Id = Guid.NewGuid(),
                    FileName = "AnnualReport.docx",
                    FileFormat = "DOCX",
                    PageCount = 42,
                    Options = new List<PrintOption>
                    {
                        new PrintOption { Name = "Cover", Value = "Glossy", AdditionalCost = 1.5m },
                        new PrintOption { Name = "PageNumbers", Value = "Bottom", AdditionalCost = 0.1m }
                    },
                    CustomSettings = new Dictionary<string, string>
                    {
                        { "Watermark", "Confidential" },
                        { "HeaderText", "Acme Corporation Annual Report" }
                    }
                },
                new DocumentSpec
                {
                    Id = Guid.NewGuid(),
                    FileName = "Financials.xlsx",
                    FileFormat = "XLSX",
                    PageCount = 15,
                    Options = new List<PrintOption>
                    {
                        new PrintOption { Name = "GridLines", Value = "Yes", AdditionalCost = 0.25m }
                    },
                    CustomSettings = new Dictionary<string, string>
                    {
                        { "FitToPage", "True" }
                    }
                }
            },
            Settings = new PrintSettings
            {
                PaperSize = PaperSize.Letter,
                Duplex = true,
                ColorMode = ColorMode.FullColor,
                Copies = 50,
                Binding = BindingType.Perfect,
                PaperWeight = 100.0m,
                SpecialInstructions = new List<string> { "Rush delivery", "Quality check" }
            },
            Metadata = new Dictionary<string, string>
            {
                { "Department", "Finance" },
                { "Project", "Annual Shareholder Meeting" },
                { "CostCenter", "F-123-456" }
            },
            TotalCost = 357.85m
        };

        // Create multiple jobs for bulk testing
        _multipleJobs = new List<PrintJob>();
        for (int i = 0; i < 100; i++)
        {
            var jobCopy = CloneJob(_singleJob);
            jobCopy.Id = Guid.NewGuid();
            jobCopy.Name = $"Print Job {i}";
            _multipleJobs.Add(jobCopy);
        }
    }

    private PrintJob CloneJob(PrintJob source)
    {
        // Simple deep-copy implementation using serialization
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<PrintJob>(json);
    }

    [Benchmark(Baseline = true)]
    public string SerializeSingleJobAdHoc()
    {
        return JsonSerializer.Serialize(_singleJob, _standardOptions);
    }

    [Benchmark]
    public string SerializeSingleJobSourceGen()
    {
        return JsonSerializer.Serialize(_singleJob, JsonContext.Default.PrintJob);
    }

    [Benchmark]
    public string SerializeMultipleJobsAdHoc()
    {
        return JsonSerializer.Serialize(_multipleJobs, _standardOptions);
    }

    [Benchmark]
    public string SerializeMultipleJobsSourceGen()
    {
        return JsonSerializer.Serialize(_multipleJobs, JsonContext.Default.ListPrintJob);
    }

    [Benchmark]
    public PrintJob DeserializeSingleJobAdHoc()
    {
        string json = JsonSerializer.Serialize(_singleJob, _standardOptions);
        return JsonSerializer.Deserialize<PrintJob>(json, _standardOptions);
    }

    [Benchmark]
    public PrintJob DeserializeSingleJobSourceGen()
    {
        string json = JsonSerializer.Serialize(_singleJob, JsonContext.Default.PrintJob);
        return JsonSerializer.Deserialize<PrintJob>(json, JsonContext.Default.PrintJob);
    }

    [Benchmark]
    public List<PrintJob> DeserializeMultipleJobsAdHoc()
    {
        string json = JsonSerializer.Serialize(_multipleJobs, _standardOptions);
        return JsonSerializer.Deserialize<List<PrintJob>>(json, _standardOptions);
    }

    [Benchmark]
    public List<PrintJob> DeserializeMultipleJobsSourceGen()
    {
        string json = JsonSerializer.Serialize(_multipleJobs, JsonContext.Default.ListPrintJob);
        return JsonSerializer.Deserialize<List<PrintJob>>(json, JsonContext.Default.ListPrintJob);
    }
}

class Program
{
    static void Main()
    {
        var config = DefaultConfig.Instance
            .WithSummaryStyle(new SummaryStyle(
                CultureInfo.InvariantCulture,
                printUnitsInHeader: true,
                printUnitsInContent: true,
                timeUnit: Perfolizer.Horology.TimeUnit.Nanosecond,
                sizeUnit: SizeUnit.B));

        BenchmarkRunner.Run<SerializationBenchmarks>(config);
    }
}