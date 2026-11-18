namespace Backend.Email;

public sealed class SmtpOptions
{
    public string Host { get; set; } = "mail.privateemail.com";
    public int Port { get; set; } = 465;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Programa de Concentración";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
