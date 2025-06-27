using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

namespace TravelWeb.Models 
{
    public static class StringExtensions
    {
        public static string RemoveDiacritics(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in text)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string NormalizeSearchString(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            return text.RemoveDiacritics().ToLowerInvariant().Trim();
        }

    }
    public static class ControllerExtensions
    {
        public static async Task<string> RenderViewAsync<TModel>(
            this Controller controller,
            string viewName,
            TModel model,
            bool partial = false)
        {
            controller.ViewData.Model = model;

            using var sw = new StringWriter();
            var services = controller.HttpContext.RequestServices;
            var engine = services.GetRequiredService<ICompositeViewEngine>();
            var viewResult = engine.FindView(controller.ControllerContext, viewName, !partial);

            if (!viewResult.Success)
                throw new InvalidOperationException($"Không tìm thấy view: {viewName}");

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}