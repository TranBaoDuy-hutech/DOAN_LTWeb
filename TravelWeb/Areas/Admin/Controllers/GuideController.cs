using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using TravelWeb.Data;
using TravelWeb.Models;

namespace TravelWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GuideController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuideController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Guide
        public IActionResult Index()
        {
            var guides = _context.Guides.ToList();
            return View(guides);
        }

        // GET: Admin/Guide/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Guide/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Guides guide)
        {
            if (ModelState.IsValid)
            {
                _context.Guides.Add(guide);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            // In lỗi ra log để debug
            foreach (var modelState in ModelState)
            {
                foreach (var error in modelState.Value.Errors)
                {
                    Console.WriteLine($"Lỗi ở trường {modelState.Key}: {error.ErrorMessage}");
                }
            }

            return View(guide);
        }



        // GET: Admin/Guide/Edit
        public IActionResult Edit(int id)
        {
            var guide = _context.Guides.Find(id);
            if (guide == null)
                return NotFound();

            return View(guide);
        }

        // POST: Admin/Guide/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Guides guide)
        {
            if (id != guide.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingGuide = _context.Guides.AsNoTracking().FirstOrDefault(g => g.Id == id);
                if (existingGuide == null)
                    return NotFound();

                try
                {
                    // Gán thủ công các trường để đảm bảo đúng đối tượng cần sửa
                    var guideToUpdate = new Guides
                    {
                        Id = guide.Id,
                        Name = guide.Name,
                        Phone = guide.Phone,
                        Email = guide.Email,
                        IsActive = guide.IsActive,
                        Skills = guide.Skills,                     
                    };

                    _context.Guides.Update(guideToUpdate);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi khi lưu: " + ex.Message);
                    throw;
                }
            }

            return View(guide);
        }



        // GET: Admin/Guide/Delete
        public IActionResult Delete(int id)
        {
            var guide = _context.Guides.Find(id);
            if (guide == null)
                return NotFound();

            return View(guide);
        }

        // POST: Admin/Guide/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var guide = _context.Guides.Find(id);
            if (guide == null)
                return NotFound();

            _context.Guides.Remove(guide);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}
