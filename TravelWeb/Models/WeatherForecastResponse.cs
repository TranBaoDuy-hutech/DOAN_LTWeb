using System;
using System.Collections.Generic;

namespace TravelWeb.Models.OpenWeather
{
    public class WeatherForecastResponse
    {
        public List<ForecastItem> list { get; set; }
        public CityInfo city { get; set; }  // Thêm nếu muốn hiển thị tên thành phố
    }

    public class ForecastItem
    {
        public string dt_txt { get; set; }  // <-- Đổi từ DateTime sang string
        public MainData main { get; set; }
        public List<WeatherData> weather { get; set; }
    }


    public class MainData
    {
        public float temp { get; set; }
        public int humidity { get; set; }
        public float feels_like { get; set; }
    }

    public class WeatherData
    {
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class CityInfo
    {
        public string name { get; set; }
        public string country { get; set; }
    }

    public class WeatherSettings
    {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
    }
}
