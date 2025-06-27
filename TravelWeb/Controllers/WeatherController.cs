using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TravelWeb.Models.OpenWeather;

namespace TravelWeb.Controllers
{
    public class WeatherController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public WeatherController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient();
            _config = config;
        }

       public async Task<IActionResult> Forecast()
{
    string apiKey = _config["OpenWeatherMap:ApiKey"];
    var result = new Dictionary<string, List<WeatherDailySummary>>();
    var tasks = new List<Task<(string City, List<WeatherDailySummary> Summary)>>();

    foreach (var city in CityListVN.Cities)
    {
        string url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units=metric&lang=vi";

        // Tạo một Task cho mỗi yêu cầu API
        tasks.Add(Task.Run(async () =>
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var forecast = JsonSerializer.Deserialize<WeatherForecastResponse>(json);

                    var grouped = forecast.list
                        .GroupBy(f => DateTime.TryParse(f.dt_txt, out var dt) ? dt.Date : DateTime.MinValue)
                        .Select(g => new WeatherDailySummary
                        {
                            Date = g.Key,
                            TempAverage = g.Average(i => i.main.temp),
                            FeelsLike = g.Average(i => i.main.feels_like),
                            Humidity = (int)g.Average(i => i.main.humidity),
                            Description = g.First().weather.First().description,
                            Icon = g.First().weather.First().icon
                        })
                        .Take(5)
                        .ToList();
                    return (city, grouped);
                }
            }
            catch (HttpRequestException ex)
            {
                // Ghi log lỗi HTTP để dễ debug
                Console.WriteLine($"Error fetching weather for {city}: {ex.Message}");
            }
            catch (JsonException ex)
            {
                // Ghi log lỗi JSON để dễ debug
                Console.WriteLine($"Error deserializing JSON for {city}: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Ghi log các lỗi khác
                Console.WriteLine($"An unexpected error occurred for {city}: {ex.Message}");
            }
            return (city, null); // Trả về null nếu có lỗi
        }));
    }

    // Chờ tất cả các Task hoàn thành
    var results = await Task.WhenAll(tasks);

    // Thêm các kết quả thành công vào dictionary
    foreach (var (city, summary) in results)
    {
        if (summary != null)
        {
            result.Add(city, summary);
        }
        // else: Bạn có thể thêm logic để ghi lại hoặc hiển thị các thành phố không có dữ liệu
    }

    return View(result);
}

    }

    public class WeatherDailySummary
    {
        public DateTime Date { get; set; }
        public double TempAverage { get; set; }
        public double FeelsLike { get; set; }      // Thêm
        public int Humidity { get; set; }          // Thêm
        public string Description { get; set; }
        public string Icon { get; set; }           // Thêm
    }

    public static class CityListVN
    {
        public static List<string> Cities = new List<string>
    {
        "An Giang", "Ba Ria - Vung Tau", "Bac Giang", "Bac Kan", "Bac Lieu",
        "Bac Ninh", "Ben Tre", "Binh Dinh", "Binh Duong", "Binh Phuoc",
        "Binh Thuan", "Ca Mau", "Can Tho", "Cao Bang", "Da Nang",
        "Dak Lak", "Dak Nong", "Dien Bien", "Dong Nai", "Dong Thap",
        "Gia Lai", "Ha Giang", "Ha Nam", "Ha Noi", "Ha Tinh",
        "Hai Duong", "Hai Phong", "Hau Giang", "Hoa Binh", "Ho Chi Minh",
        "Hung Yen", "Khanh Hoa", "Kien Giang", "Kon Tum", "Lai Chau",
        "Lam Dong", "Lang Son", "Lao Cai", "Long An", "Nam Dinh",
        "Nghe An", "Ninh Binh", "Ninh Thuan", "Phu Tho", "Phu Yen",
        "Quang Binh", "Quang Nam", "Quang Ngai", "Quang Ninh", "Quang Tri",
        "Soc Trang", "Son La", "Tay Ninh", "Thai Binh", "Thai Nguyen",
        "Thanh Hoa", "Thua Thien Hue", "Tien Giang", "Tra Vinh", "Tuyen Quang",
        "Vinh Long", "Vinh Phuc", "Yen Bai"
    };
    }
}
