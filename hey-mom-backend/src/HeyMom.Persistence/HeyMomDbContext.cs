using Microsoft.EntityFrameworkCore;

namespace HeyMom.Persistence;

public sealed class HeyMomDbContext(DbContextOptions<HeyMomDbContext> options) : DbContext(options);
