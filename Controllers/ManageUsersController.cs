using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;

namespace PhuKienShop.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class ManageUsersController : Controller
    {
        private readonly PkShopContext _context;

        public ManageUsersController(PkShopContext context)
        {
            _context = context;
        }

        // GET: ManageUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // POST: ManageUsers/EditRoleAndDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoleAndDelete(int UserId, string Role, string action)
        {
            var user = await _context.Users.FindAsync(UserId);
            if (user == null)
            {
                return NotFound();
            }

            if (action == "update")
            {
                user.Role = Role;
                user.UpdatedAt = DateTime.Now;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            else if (action == "delete")
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
