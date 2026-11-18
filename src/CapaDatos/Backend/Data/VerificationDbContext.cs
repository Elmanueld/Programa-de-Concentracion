using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public sealed class VerificationDbContext(DbContextOptions<VerificationDbContext> options)
    : DbContext(options)
{
}
