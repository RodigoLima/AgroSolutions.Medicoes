namespace AgroSolutions.Medicoes.Application.Services;

public class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public string Port { get; set; } = "587";
    public string From { get; set; } = string.Empty;
    public bool UseSsl { get; set; }

    public int PortNumber => int.TryParse(Port, out var p) ? p : 587;
}
