using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Backend.Data;
using Backend.Email;
using Backend.PasswordRecovery;
using Backend.Registration;
using Backend.Verification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var pruebas = new PruebasBackend();
await pruebas.EjecutarAsync();

internal sealed class PruebasBackend
{
    private int comprobaciones;

    public async Task EjecutarAsync()
    {
        await ProbarVerificacionAsync();
        await ProbarRegistroAsync();
        await ProbarRecuperacionAsync();
        ProbarContratos();
        Console.WriteLine($"Correctas: {comprobaciones}; errores: 0");
    }

    private async Task ProbarVerificacionAsync()
    {
        var repositorio = new RepositorioVerificacionFalso();
        var correo = new RemitenteCorreoFalso();
        var servicio = new VerificationService(repositorio, correo, Opciones());

        await servicio.SendAsync(" usuario@example.com ", CancellationToken.None);
        Comprobar(repositorio.Email == "usuario@example.com", "normaliza el correo antes de preparar el código");
        Comprobar(correo.EmailVerificacion == repositorio.Email, "envía al mismo correo preparado en el repositorio");
        Comprobar(correo.CodigoVerificacion is { Length: 6 } && correo.CodigoVerificacion.All(char.IsAsciiDigit),
            "genera y envía un código de seis dígitos");
        Comprobar(repositorio.Hash is { Length: 64 } && !repositorio.Hash.Contains(correo.CodigoVerificacion!),
            "persiste el resumen del código, no el código en claro");

        bool confirmado = await servicio.ConfirmAsync("usuario@example.com", correo.CodigoVerificacion!, CancellationToken.None);
        Comprobar(confirmado && repositorio.HashConfirmado == repositorio.Hash,
            "confirma usando el mismo resumen derivado del código");

        int consultasAntes = repositorio.ConsultasSal;
        bool codigoInvalido = await servicio.ConfirmAsync("usuario@example.com", "abc123", CancellationToken.None);
        Comprobar(!codigoInvalido && repositorio.ConsultasSal == consultasAntes,
            "rechaza un código no numérico sin consultar el repositorio");

        var sinClave = new VerificationService(repositorio, correo,
            Options.Create(new VerificationOptions { HashKey = "", CodeLifetimeMinutes = 10 }));
        await EsperarAsync<InvalidOperationException>(
            () => sinClave.SendAsync("usuario@example.com", CancellationToken.None),
            "falla claramente si falta la clave de hash");

        correo.FallarEnvioVerificacion = true;
        await EsperarAsync<EmailDeliveryException>(
            () => servicio.SendAsync("usuario@example.com", CancellationToken.None),
            "no informa éxito cuando falla la entrega SMTP");
    }

    private async Task ProbarRegistroAsync()
    {
        var repositorio = new RepositorioRegistroFalso();
        var servicio = new AccountRegistrationService(repositorio);
        string hash = CrearHashConFormatoAdmitido();

        int id = await servicio.RegisterAsync(" Ana ", " Ana@example.com ", hash, CancellationToken.None);
        Comprobar(id == 42 && repositorio.Nombre == "Ana" && repositorio.Email == "Ana@example.com",
            "recorta los datos y conserva el IdUsuario idempotente del repositorio");
        Comprobar(repositorio.Hash == hash, "transfiere únicamente el hash de contraseña recibido");

        foreach (string nombreValido in new[] { "Ana", "Ana_2026", "Élodie", "用户12" })
            Comprobar(UserNameRules.IsValid(nombreValido), $"acepta el nombre de usuario válido '{nombreValido}'");
        foreach (string nombreInvalido in new[] { "Ana María", "Ana!", "", "   ", new string('a', 51) })
            Comprobar(!UserNameRules.IsValid(nombreInvalido), "rechaza un nombre vacío, con espacios/caracteres especiales o demasiado largo");
        int llamadasAntesDeNombreInvalido = repositorio.Llamadas;
        await EsperarAsync<InvalidUsernameException>(
            () => servicio.RegisterAsync("Ana María", "ana@example.com", hash, CancellationToken.None),
            "rechaza nombres no válidos antes de acceder al repositorio");
        Comprobar(repositorio.Llamadas == llamadasAntesDeNombreInvalido,
            "no intenta registrar un nombre no válido en el repositorio");

        int llamadas = repositorio.Llamadas;
        await EsperarAsync<ArgumentException>(
            () => servicio.RegisterAsync("Ana", "ana@example.com", "password", CancellationToken.None),
            "rechaza una contraseña que no llega en formato PBKDF2 esperado");
        Comprobar(repositorio.Llamadas == llamadas, "rechaza el hash inválido antes de acceder al repositorio");

        repositorio.DevolverConflicto = true;
        await EsperarAsync<AccountRegistrationConflictException>(
            () => servicio.RegisterAsync("Ana", "ana@example.com", hash, CancellationToken.None),
            "mantiene el conflicto cuando no hay registro idempotente coincidente");
    }

    private async Task ProbarRecuperacionAsync()
    {
        var repositorio = new RepositorioRecuperacionFalso();
        var correo = new RemitenteCorreoFalso();
        var servicio = new PasswordRecoveryService(repositorio, correo, Opciones());

        repositorio.Preparar = false;
        await servicio.SendAsync("no-existe@example.com", CancellationToken.None);
        Comprobar(correo.CodigoRecuperacion is null,
            "no revela ni envía código cuando la cuenta no existe o no puede recuperarse");

        repositorio.Preparar = true;
        await servicio.SendAsync(" cuenta@example.com ", CancellationToken.None);
        Comprobar(correo.EmailRecuperacion == "cuenta@example.com" &&
                  correo.CodigoRecuperacion is { Length: 6 } && correo.CodigoRecuperacion.All(char.IsAsciiDigit),
            "envía un código de recuperación al correo normalizado");
        Comprobar(repositorio.Hash is { Length: 64 } && !repositorio.Hash.Contains(correo.CodigoRecuperacion!),
            "guarda el resumen del código de recuperación y no su texto");

        string hashContrasena = CrearHashConFormatoAdmitido();
        bool recuperada = await servicio.ConfirmAsync("cuenta@example.com", correo.CodigoRecuperacion!,
            hashContrasena, CancellationToken.None);
        Comprobar(recuperada && repositorio.HashContrasenaActualizado == hashContrasena,
            "actualiza la contraseña solo después de confirmar el código válido");

        await EsperarAsync<ArgumentException>(
            () => servicio.ConfirmAsync("cuenta@example.com", "123456", "texto-plano", CancellationToken.None),
            "rechaza una contraseña que no está protegida en el formato esperado");
    }

    private void ProbarContratos()
    {
        RevisarParametros(typeof(SendVerificationRequest), ("Email", typeof(EmailAddressAttribute)));
        RevisarParametros(typeof(ConfirmVerificationRequest), ("Email", typeof(EmailAddressAttribute)),
            ("Code", typeof(RegularExpressionAttribute)));
        RevisarParametros(typeof(RegisterAccountRequest), ("Name", typeof(RequiredAttribute)),
            ("Email", typeof(EmailAddressAttribute)),
            ("PasswordHash", typeof(RequiredAttribute)));
        RevisarParametros(typeof(SendPasswordRecoveryRequest), ("Email", typeof(EmailAddressAttribute)));
        RevisarParametros(typeof(ConfirmPasswordRecoveryRequest), ("Code", typeof(RegularExpressionAttribute)),
            ("PasswordHash", typeof(RequiredAttribute)));
    }

    private void RevisarParametros(Type tipo, params (string Nombre, Type Atributo)[] requeridos)
    {
        var parametros = tipo.GetConstructors().Single().GetParameters();
        foreach (var (nombre, atributo) in requeridos)
        {
            var parametro = parametros.SingleOrDefault(p => p.Name == nombre);
            Comprobar(parametro?.GetCustomAttribute(atributo) is not null,
                $"{tipo.Name}.{nombre} conserva metadatos de validación en el parámetro del record");
        }
    }

    private void Comprobar(bool condicion, string nombre)
    {
        if (!condicion) throw new InvalidOperationException("Prueba fallida: " + nombre);
        comprobaciones++;
        Console.WriteLine("OK " + nombre);
    }

    private async Task EsperarAsync<TException>(Func<Task> accion, string nombre)
        where TException : Exception
    {
        try
        {
            await accion();
        }
        catch (TException)
        {
            Comprobar(true, nombre);
            return;
        }

        Comprobar(false, nombre);
    }

    private static IOptions<VerificationOptions> Opciones() =>
        Options.Create(new VerificationOptions { HashKey = "clave-de-prueba-no-secreta", CodeLifetimeMinutes = 10 });

    private static string CrearHashConFormatoAdmitido() =>
        $"PBKDF2-SHA256$600000${Convert.ToBase64String(new byte[16])}${Convert.ToBase64String(new byte[32])}";
}

internal sealed class RemitenteCorreoFalso : IEmailSender
{
    public string? EmailVerificacion { get; private set; }
    public string? CodigoVerificacion { get; private set; }
    public string? EmailRecuperacion { get; private set; }
    public string? CodigoRecuperacion { get; private set; }
    public bool FallarEnvioVerificacion { get; set; }

    public Task SendVerificationCodeAsync(string recipient, string code, CancellationToken cancellationToken)
    {
        if (FallarEnvioVerificacion) throw new InvalidOperationException("fallo SMTP simulado");
        EmailVerificacion = recipient;
        CodigoVerificacion = code;
        return Task.CompletedTask;
    }

    public Task SendPasswordRecoveryCodeAsync(string recipient, string code, CancellationToken cancellationToken)
    {
        EmailRecuperacion = recipient;
        CodigoRecuperacion = code;
        return Task.CompletedTask;
    }
}

internal sealed class RepositorioVerificacionFalso : IVerificationRepository
{
    public string? Email { get; private set; }
    public string? Salt { get; private set; }
    public string? Hash { get; private set; }
    public string? HashConfirmado { get; private set; }
    public int ConsultasSal { get; private set; }

    public Task<bool> PrepareAsync(string email, string salt, string codeHash, CancellationToken cancellationToken)
    {
        Email = email;
        Salt = salt;
        Hash = codeHash;
        return Task.FromResult(true);
    }

    public Task<string?> GetSaltAsync(string email, CancellationToken cancellationToken)
    {
        ConsultasSal++;
        return Task.FromResult(Salt);
    }

    public Task<bool> ConfirmAsync(string email, string codeHash, CancellationToken cancellationToken)
    {
        HashConfirmado = codeHash;
        return Task.FromResult(Hash == codeHash);
    }
}

internal sealed class RepositorioRegistroFalso : IAccountRegistrationRepository
{
    public int Llamadas { get; private set; }
    public bool DevolverConflicto { get; set; }
    public string? Nombre { get; private set; }
    public string? Email { get; private set; }
    public string? Hash { get; private set; }

    public Task<int?> RegisterOrGetIdempotentAsync(
        string name, string email, string passwordHash, CancellationToken cancellationToken)
    {
        Llamadas++;
        Nombre = name;
        Email = email;
        Hash = passwordHash;
        return Task.FromResult<int?>(DevolverConflicto ? null : 42);
    }
}

internal sealed class RepositorioRecuperacionFalso : IPasswordRecoveryRepository
{
    public bool Preparar { get; set; }
    public string? Salt { get; private set; }
    public string? Hash { get; private set; }
    public string? HashContrasenaActualizado { get; private set; }

    public Task<bool> PrepareAsync(string email, string salt, string codeHash, CancellationToken cancellationToken)
    {
        Salt = salt;
        Hash = codeHash;
        return Task.FromResult(Preparar);
    }

    public Task<string?> GetSaltAsync(string email, CancellationToken cancellationToken) => Task.FromResult(Salt);

    public Task<bool> ConfirmAsync(
        string email, string codeHash, string passwordHash, CancellationToken cancellationToken)
    {
        if (Hash != codeHash) return Task.FromResult(false);
        HashContrasenaActualizado = passwordHash;
        return Task.FromResult(true);
    }
}
