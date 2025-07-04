namespace WeatherGuard.Api.Models;

public class DatadogLog
{
    public string ddsource { get; set; }
    public string ddtags { get; set; }
    public string hostname { get; set; }
    public string service { get; set; }
    public string session_id { get; set; }
    public string message { get; set; }
    public string status { get; set; }
    public string state { get; set; }
    public string loggerName { get; set; }
    public DateTimeOffset date { get; set; }
    public DatadogUserLog usr { get; set; }
    public DatadogHttpLog http { get; set; }
    public DatadogErrorLog error { get; set; }
}

public class DatadogHttpLog
{
    public string url { get; set; }
    public string status_code { get; set; }
    public string method { get; set; }
    public string userAgent { get; set; }
    public string version { get; set; }
    public DatadogHttpUrlDetails url_details { get; set; }
}

public class DatadogHttpUrlDetails
{
    public string host { get; set; }
    public string port { get; set; }
    public string path { get; set; }
}

public class DatadogUserLog
{
    public string id { get; set; }
}

public class DatadogErrorLog
{
    public string kind { get; set; }
    public string message { get; set; }
    public string stack { get; set; }
}