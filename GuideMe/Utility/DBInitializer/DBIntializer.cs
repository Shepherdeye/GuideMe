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

                // ------------------ CREATE ROLES -------------------------
                if (!_roleManager.Roles.Any())
                {
                    _roleManager.CreateAsync(new IdentityRole(SD.SuperAdminRole)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.AdminRole)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.VisitorRole)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.GuideRole)).GetAwaiter().GetResult();
                }

                // ------------------ CREATE SUPERADMIN USER -------------------------
                if (_userManager.FindByEmailAsync("SuperAdmin@EraaSoft.com").GetAwaiter().GetResult() == null)
                {
                    var superAdmin = new ApplicationUser
                    {
                        FirstName = "SuperAdmin",
                        LastName = "One",
                        Email = "SuperAdmin@EraaSoft.com",
                        UserName = "SuperAdmin",
                        PhoneNumber = "01012939912",
                        EmailConfirmed = true,
                        Role = UserRole.Admin
                    };

                    var result = _userManager.CreateAsync(superAdmin, "Admin123@").GetAwaiter().GetResult();

                    if (result.Succeeded)
                    {
                        // Assign role
                        _userManager.AddToRoleAsync(superAdmin, SD.SuperAdminRole).GetAwaiter().GetResult();

                        // ------------------ INSERT Visitor Linked to User -------------------------
                        var visitor = new Visitor
                        {
                            Passport = "P123456789",
                            visitorStatus = VisitorStatus.Available,
                            ApplicationUserId = superAdmin.Id
                        };

                        _context.Visitors.Add(visitor);

                        // ------------------ INSERT Guide Linked to User -------------------------
                        var guide = new Guide
                        {
                            LicenseNumber = "LIC-987654987654",
                            YearsOfExperience = 5,
                            NationalId = "29811223344556",
                            ApplicationUserId = superAdmin.Id
                        };

                        _context.Guides.Add(guide);

                        _context.SaveChanges();
                    }
                
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
