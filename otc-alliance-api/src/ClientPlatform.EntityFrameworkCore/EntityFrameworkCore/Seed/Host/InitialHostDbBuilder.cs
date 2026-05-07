namespace ClientPlatform.EntityFrameworkCore.Seed.Host;

public class InitialHostDbBuilder
{
    private readonly ClientPlatformDbContext _context;

    public InitialHostDbBuilder(ClientPlatformDbContext context)
    {
        _context = context;
    }

    public void Create()
    {
        new DefaultEditionCreator(_context).Create();
        new DefaultLanguagesCreator(_context).Create();
        new HostRoleAndUserCreator(_context).Create();
        new DefaultSettingsCreator(_context).Create();

        _context.SaveChanges();
    }
}
