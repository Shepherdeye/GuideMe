using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GuideMe.Utility.DBInitializer
{
    public class DBIntializer : IDBInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DBIntializer> _logger;

        public DBIntializer(ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,ILogger<DBIntializer> logger)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public void Initialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }

                if (_roleManager.Roles.IsNullOrEmpty())
                {
                    _roleManager.CreateAsync(new(SD.SuperAdminRole)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.AdminRole)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.VisitorRole)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(SD.GuideRole)).GetAwaiter().GetResult();

                    //create => Super Admin
                    _userManager.CreateAsync(new()
                    {
                        FirstName = "SuperAdmin",
                        LastName="One",
                        Email = "SuperAdmin@EraaSoft.com",
                        EmailConfirmed = true,
                        UserName = "SuperAdmin"

                    }, "Admin123@").GetAwaiter().GetResult();

                    var user = _userManager.FindByEmailAsync("SuperAdmin@EraaSoft.com").GetAwaiter().GetResult();

                    _userManager.AddToRoleAsync(user, SD.SuperAdminRole).GetAwaiter().GetResult();

                }
            }
            catch (Exception ex )
            {
                _logger.LogError(ex.Message);
                _logger.LogError("check your connection ,use Db on  lcoal server (.)");
            }

        }
    }
}
