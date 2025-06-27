using Microsoft.AspNetCore.Mvc;
using TravelWeb.Data;
using TravelWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace TravelWeb.Controllers
{
    public class TourController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TourController> _logger;

        public TourController(ApplicationDbContext context, ILogger<TourController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index(
            string searchString,
            string departureLocation,
            string location,
            string locationFilter,
            string priceFilter,
            string durationFilter,
            string sortOrder)
        {
            // Log các tham số đầu vào
            _logger.LogInformation("Search parameters: searchString={Search}, departureLocation={Departure}, location={Location}, locationFilter={LocationFilter}, priceFilter={PriceFilter}, durationFilter={DurationFilter}",
                searchString, departureLocation, location, locationFilter, priceFilter, durationFilter);

            // Normalize các giá trị tìm kiếm
            string normalizedSearch = searchString?.NormalizeSearchString();
            string normalizedDeparture = departureLocation?.NormalizeSearchString();
            string normalizedLocation = location?.NormalizeSearchString();
            string normalizedLocationFilter = locationFilter?.NormalizeSearchString();

            // Fetch all tours to client side first
            var tours = await _context.Tours.ToListAsync();

            // Apply filtering on client side
            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                tours = tours.Where(t => t.Name != null && t.Name.NormalizeSearchString().Contains(normalizedSearch)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(normalizedDeparture))
            {
                tours = tours.Where(t => t.DepartureLocation != null && t.DepartureLocation.NormalizeSearchString().Contains(normalizedDeparture)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(normalizedLocation))
            {
                tours = tours.Where(t => t.Location != null && t.Location.NormalizeSearchString().Contains(normalizedLocation)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(normalizedLocationFilter))
            {
                tours = tours.Where(t => t.Location != null && t.Location.NormalizeSearchString().Contains(normalizedLocationFilter)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(priceFilter))
            {
                switch (priceFilter)
                {
                    case "under2":
                        tours = tours.Where(t => t.Price < 2_000_000).ToList();
                        break;
                    case "2to5":
                        tours = tours.Where(t => t.Price >= 2_000_000 && t.Price <= 5_000_000).ToList();
                        break;
                    case "over5":
                        tours = tours.Where(t => t.Price > 5_000_000).ToList();
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(durationFilter))
            {
                switch (durationFilter)
                {
                    case "under3":
                        tours = tours.Where(t => t.DurationDays < 3).ToList();
                        break;
                    case "3to5":
                        tours = tours.Where(t => t.DurationDays >= 3 && t.DurationDays <= 5).ToList();
                        break;
                    case "over5":
                        tours = tours.Where(t => t.DurationDays > 5).ToList();
                        break;
                }
            }
            switch (sortOrder)
            {
                case "price_asc":
                    tours = tours.OrderBy(t => t.Price).ToList();
                    break;
                case "price_desc":
                    tours = tours.OrderByDescending(t => t.Price).ToList();
                    break;
                case "duration_asc":
                    tours = tours.OrderBy(t => t.DurationDays).ToList();
                    break;
                case "duration_desc":
                    tours = tours.OrderByDescending(t => t.DurationDays).ToList();
                    break;
                case "name_asc":
                    tours = tours.OrderBy(t => t.Name).ToList();
                    break;
                case "name_desc":
                    tours = tours.OrderByDescending(t => t.Name).ToList();
                    break;
                default:
                    tours = tours.OrderBy(t => t.Name).ToList(); // mặc định theo tên
                    break;
            }

            // Log số lượng tour tìm được
            _logger.LogInformation("Found {Count} tours after applying filters.", tours.Count);

            // Gửi lại giá trị filter cho View
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentDeparture"] = departureLocation;
            ViewData["CurrentLocation"] = location;
            ViewData["CurrentLocationFilter"] = locationFilter;
            ViewData["CurrentPriceFilter"] = priceFilter;
            ViewData["CurrentDurationFilter"] = durationFilter;
            ViewData["CurrentSort"] = sortOrder;

            return View(tours);
        }

        public IActionResult Details(int id)
        {
            var tour = _context.Tours.FirstOrDefault(t => t.Id == id);
            if (tour == null)
            {
                return NotFound();
            }
            return View(tour);
        }
    }
}