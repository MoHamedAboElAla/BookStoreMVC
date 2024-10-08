using Ecommerce.DataAcess.Data;
using Ecommerce.Models.Models;
using Ecommerce.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.DataAcess.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DBInitializer(AppDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public void Initialize()
        {
            //migration if they are not applied
            try
            {
                if (_context.Database.GetPendingMigrations().Count() > 0)
                    _context.Database.Migrate();
            }
            catch (Exception ex)
            {
                throw ex;
            }


            //create roles if they are not created
            if (!_roleManager.RoleExistsAsync(StaticDetails.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Company)).GetAwaiter().GetResult();


                //if roles are not created, then will create admin user as well
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "Admin@gmail.com",
                    Email = "Admin@gmail.com",
                    Name = "Admin",
                    PhoneNumber = "01129031",
                    StreetAddress = "test 123 Cairo",
                    City = "Cairo",
                    PostalCode = "099",
                    State = "IL",


                }, "Admin123#").GetAwaiter().GetResult();

                ApplicationUser user = _context.ApplicationUsers.FirstOrDefault(e => e.Email == "Admin@gmail.com");
                _userManager.AddToRoleAsync(user, StaticDetails.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        }
        }
}
