using System;
using System.Diagnostics;

namespace AgroSolutions.Medicoes.Infrastructure.Observability;

public static class ActivitySourceProvider
{
    public const string ServiceName = "AgroSolutions.Medicoes.Worker";
    public const string ServiceVersion = "1.0.0";
    
    public static readonly ActivitySource Source = new(ServiceName, ServiceVersion);
}