using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TravelWeb.Data;
using TravelWeb.Models;

namespace TravelWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TourController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TourController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Tour
        public async Task<IActionResult> Index()
        {
            var tours = await _context.Tours.ToListAsync();
            return View(tours);
        }

        // GET: Admin/Tour/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Tour/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tour tour)
        {
            if (ModelState.IsValid)
            {
                _context.Tours.Add(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tour);
        }

        // GET: Admin/Tour/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
                return NotFound();

            return View(tour);
        }

        // POST: Admin/Tour/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tour tour)
        {
            if (id != tour.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tour);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourExists(tour.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tour);
        }

        // GET: Admin/Tour/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
                return NotFound();

            return View(tour);
        }

        // POST: Admin/Tour/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
                return NotFound();

            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Tour/ImportFromExcel
        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile)
        {
            // Kiểm tra file
            if (excelFile == null || excelFile.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file Excel.");
                return RedirectToAction("Create");
            }

            // Kiểm tra loại file và kích thước (tối đa 10MB)
            if (!excelFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) || excelFile.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError("", "File không hợp lệ. Vui lòng tải lên file .xlsx hợp lệ nhỏ hơn 10MB.");
                return RedirectToAction("Create");
            }

            var tours = new List<Tour>();
            var errorRows = new List<int>();

            try
            {
                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null || worksheet.Dimension == null)
                        {
                            ModelState.AddModelError("", "Định dạng file Excel không hợp lệ.");
                            return RedirectToAction("Create");
                        }

                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                // Kiểm tra và phân tích từng trường
                                if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text))
                                {
                                    errorRows.Add(row);
                                    continue;
                                }

                                if (!decimal.TryParse(worksheet.Cells[row, 2].Text, out var price))
                                {
                                    errorRows.Add(row);
                                    continue;
                                }

                                if (!int.TryParse(worksheet.Cells[row, 3].Text, out var durationDays))
                                {
                                    errorRows.Add(row);
                                    continue;
                                }

                                if (!DateTime.TryParse(worksheet.Cells[row, 4].Text, out var startDate))
                                {
                                    errorRows.Add(row);
                                    continue;
                                }

                                if (!float.TryParse(worksheet.Cells[row, 11].Text, out var rating))
                                {
                                    errorRows.Add(row);
                                    continue;
                                }

                                if (!int.TryParse(worksheet.Cells[row, 12].Text, out var reviewCount))
                                {
                                    errorRows.Add(row);
                                    continue;
                                }

                                if (!bool.TryParse(worksheet.Cells[row, 13].Text, out var isHot))
                                {
                                    errorRows.Add(row);
                                    continue;
                                }

                                var tour = new Tour
                                {
                                    Name = worksheet.Cells[row, 1].Text,
                                    Price = price,
                                    DurationDays = durationDays,
                                    StartDate = startDate,
                                    Location = worksheet.Cells[row, 5].Text,
                                    DepartureLocation = worksheet.Cells[row, 6].Text,
                                    Transportation = worksheet.Cells[row, 7].Text,
                                    TourType = worksheet.Cells[row, 8].Text,
                                    HotelName = worksheet.Cells[row, 9].Text,
                                    ImageUrl = worksheet.Cells[row, 10].Text,
                                    Rating = rating,
                                    ReviewCount = reviewCount,
                                    IsHot = isHot,
                                    Description = worksheet.Cells[row, 14].Text,
                                    Itinerary = worksheet.Cells[row, 15].Text
                                };

                                tours.Add(tour);
                            }
                            catch
                            {
                                errorRows.Add(row);
                                continue;
                            }
                        }
                    }
                }

                // Lưu các tour hợp lệ vào cơ sở dữ liệu
                if (tours.Any())
                {
                    _context.Tours.AddRange(tours);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã nhập thành công {tours.Count} tour.";
                }
                else
                {
                    ModelState.AddModelError("", "Không tìm thấy tour hợp lệ trong file Excel.");
                    return RedirectToAction("Create");
                }

                // Báo cáo các hàng có lỗi
                if (errorRows.Any())
                {
                    TempData["WarningMessage"] = $"Một số hàng ({string.Join(", ", errorRows)}) không thể nhập do dữ liệu không hợp lệ.";
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi xử lý file Excel: {ex.Message}");
                return RedirectToAction("Create");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TourExists(int id)
        {
            return _context.Tours.Any(e => e.Id == id);
        }
    }
}