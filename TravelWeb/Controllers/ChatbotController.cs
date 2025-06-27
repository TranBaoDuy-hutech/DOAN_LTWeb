using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Mvc;

namespace TravelWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly string projectId = "scenic-arc-459806-h2";
        private const float ConfidenceThreshold = 0.6f;

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] ChatRequest request)
        {
            // Kiểm tra đầu vào
            if (string.IsNullOrWhiteSpace(request.Message) || string.IsNullOrWhiteSpace(request.SessionId))
            {
                return BadRequest(new { error = "SessionId và Message không được để trống." });
            }

            try
            {
                // Tạo client và session cho Dialogflow
                var client = await SessionsClient.CreateAsync();
                var session = new SessionName(projectId, request.SessionId);

                // Tạo query input
                var queryInput = new QueryInput
                {
                    Text = new TextInput
                    {
                        Text = request.Message,
                        LanguageCode = "en" 
                    }
                };

                // Gửi yêu cầu đến Dialogflow
                var response = await client.DetectIntentAsync(session, queryInput);
                var queryResult = response.QueryResult;

                string reply;
                // Kiểm tra độ tin cậy và phản hồi từ intent
                if (queryResult.IntentDetectionConfidence >= ConfidenceThreshold && !string.IsNullOrWhiteSpace(queryResult.FulfillmentText))
                {
                    reply = queryResult.FulfillmentText;
                }
                else
                {
                    // Phản hồi fallback khi không khớp intent hoặc độ tin cậy thấp
                    reply = "Xin lỗi, mình chưa hiểu rõ câu hỏi này. Bạn muốn hỏi về giá tour, đặt tour, hay lịch trình tour không? Gọi hotline 0123-456-789 để được hỗ trợ nhé!";
                }

                // Ghi log để debug
                Console.WriteLine($"Câu hỏi: {request.Message}, Intent: {queryResult.Intent.DisplayName}, Confidence: {queryResult.IntentDetectionConfidence}");
                if (queryResult.Intent.IsFallback)
                {
                    Console.WriteLine($"Câu hỏi không khớp: {request.Message}");
                    // Lưu vào file để phân tích sau
                    System.IO.File.AppendAllText("unmatched_questions.txt", $"{DateTime.Now}: {request.Message}\n");
                }

                return Ok(new { reply });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                return StatusCode(500, new { error = "Đã có lỗi xảy ra, vui lòng thử lại sau." });
            }
        }
    }

    public class ChatRequest
    {
        public string SessionId { get; set; }
        public string Message { get; set; }
    }

}