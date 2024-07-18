using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using DemoMVC.Models;
using Microsoft.AspNetCore.Authorization;
using SharedLibrary;

namespace DemoMVC.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Policy = nameof(SystemPermissions.EmployeeView))]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Employee.ToListAsync());
        }

        [Authorize(Policy = nameof(SystemPermissions.EmployeeView))]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [Authorize(Policy = nameof(SystemPermissions.EmployeeCreate))]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = nameof(SystemPermissions.EmployeeCreate))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,FirstName,LastName,Address,DateOfBirth,Position,Email,HireDate")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        [Authorize(Policy = nameof(SystemPermissions.EmployeeEdit))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [Authorize(Policy = nameof(SystemPermissions.EmployeeEdit))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,FirstName,LastName,Address,DateOfBirth,Position,Email,HireDate")] Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        [Authorize(Policy = nameof(SystemPermissions.EmployeeDelete))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [Authorize(Policy = nameof(SystemPermissions.EmployeeDelete))]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employee.FindAsync(id);
            if (employee != null)
            {
                _context.Employee.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employee.Any(e => e.EmployeeId == id);
        }
    }
}
