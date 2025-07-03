using ErpQueryAssist.Application.DTOs;
using ErpQueryAssist.Application.Interfaces;
using ErpQueryAssist.Application.Models.Pivot;
using ErpQueryAssist.Application.Services;
using ErpQueryAssist.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ErpQueryAssist.Web.Controllers
{
    public class QueryController : Controller
    {
        private readonly IUserQueryService _queryService;
        private readonly ILlmService _llmService;
        private readonly PromptTemplateService _promptService;

        public QueryController(IUserQueryService queryService, ILlmService llmService, PromptTemplateService promptService)
        {
            _queryService = queryService;
            _llmService = llmService;
            _promptService = promptService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.QueryTypes = new List<string> { "Order", "Delivery", "Production", "Accounts" };
            return View();
        }

        [HttpPost]
        public IActionResult AnalyzeFromForm(string queryType, string userQuery)
        {
            if (string.IsNullOrWhiteSpace(userQuery))
                return RedirectToAction("Index");

            return RedirectToAction("AskAndAnalyzeUnified", new { question = userQuery });
        }

        [HttpGet]
        public async Task<IActionResult> AskAndAnalyzeUnified(string question, string type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(question))
                    return BadRequest("Question is required.");

                string prompt = type switch
                {
                    "Delivery" => _promptService.GetUnifiedPromptForDelivery(question),
                    "Production" => _promptService.GetUnifiedPromptForProduction(question),
                    "Accounts" => _promptService.GetUnifiedPromptForAccounts(question),
                    _ => _promptService.GetUnifiedPromptForOrder(question)
                };

                var fullResponse = await _llmService.AskAsync(prompt);

                var (summarySql, detailsSql, pivotSql, reportType) = ExtractQueries(fullResponse);

                if (!fullResponse.Contains("reportType:"))
                {
                    System.IO.File.WriteAllText("bad_llm_response.txt", fullResponse);
                }

                Debug.WriteLine(fullResponse);

                List<MonthPivotData> monthPivot = null;
                List<ClientMonthPivotData> clientMonthPivotDatas = null;
                List<YearPivotData> yearPivot = null;
                List<SummaryData> summaryDataList = null;
                List<DetailsData> detailItems = null;

                if (reportType == "pivot_month")
                {
                    var pivotJson = await _queryService.ExecuteSqlAsync(pivotSql);
                    monthPivot = JsonConvert.DeserializeObject<List<MonthPivotData>>(pivotJson);
                    return View("MonthPivotResult", monthPivot);
                }
                if (reportType == "pivot_month_client")
                {
                    var pivotJson = await _queryService.ExecuteSqlAsync(pivotSql);
                    
                    clientMonthPivotDatas = JsonConvert.DeserializeObject<List<ClientMonthPivotData>>(pivotJson);

                    return View("ClientWisePivotResult", clientMonthPivotDatas);
                }
                else if (reportType == "pivot_year")
                {
                    var pivotJson = await _queryService.ExecuteSqlAsync(pivotSql);
                    yearPivot = JsonConvert.DeserializeObject<List<YearPivotData>>(pivotJson);
                    return View("YearPivotResult", yearPivot);
                }
                else
                {
                    string summaryJson = await _queryService.ExecuteSqlAsync(summarySql);
                    summaryDataList = System.Text.Json.JsonSerializer.Deserialize<List<SummaryData>>(summaryJson);

                    string detailsJson = await _queryService.ExecuteSqlAsync(detailsSql);
                    detailItems = JsonConvert.DeserializeObject<List<DetailsData>>(detailsJson);

                    for (int i = 0; i < detailItems.Count; i++)
                    {
                        detailItems[i].SerialNo = i + 1;
                    }

                    var vm = new UniversalQueryResultViewModel
                    {
                        OriginalQuestion = question,
                        SummaryDataList = summaryDataList,
                        DetailsDataList = detailItems,
                        SummaryTitle = "Summary",
                        DetailsTitle = "Details",                                
                        ChartTitle = "Chart"
                    };

                    return View("UniversalResult", vm);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { msg = "Something went wrong. Please try again" });
            }

        }

        private (string Summary, string Details, string Pivot, string ReportType) ExtractQueries(string llmResponse)
        {
            // Step 1: Remove markdown ```sql and ``` blocks
            llmResponse = Regex.Replace(llmResponse, @"^```sql|```$", "", RegexOptions.IgnoreCase | RegexOptions.Multiline).Trim();

            // Step 2: Try header-based matching (most accurate format)
            var summaryMatch = Regex.Match(llmResponse, @"--\s*Summary Query\s*--\s*(SELECT[\s\S]+?)(?:--|$)", RegexOptions.IgnoreCase);
            var detailsMatch = Regex.Match(llmResponse, @"--\s*Details Query\s*--\s*(SELECT[\s\S]+?)(?:--|$)", RegexOptions.IgnoreCase);
            var pivotMatch = Regex.Match(llmResponse, @"--\s*Pivot Query\s*--\s*(SELECT[\s\S]+?)(?:--|$)", RegexOptions.IgnoreCase);
            var reportTypeMatch = Regex.Match(llmResponse, @"reportType:\s*(\w+)", RegexOptions.IgnoreCase);

            // Step 3: Extract values safely
            var summary = summaryMatch.Success ? summaryMatch.Groups[1].Value.Trim() : "";
            var details = detailsMatch.Success ? detailsMatch.Groups[1].Value.Trim() : "";
            var pivot = pivotMatch.Success ? pivotMatch.Groups[1].Value.Trim() : "";
            var reportType = reportTypeMatch.Success ? reportTypeMatch.Groups[1].Value.Trim() : "";

            // Step 4: Fallback - extract SELECT blocks if header-based fails
            if (string.IsNullOrWhiteSpace(summary) || string.IsNullOrWhiteSpace(details))
            {
                var matches = Regex.Matches(llmResponse, @"SELECT[\s\S]+?(?=(\n\s*SELECT|\Z))", RegexOptions.IgnoreCase);

                if (matches.Count > 0 && string.IsNullOrWhiteSpace(summary))
                    summary = matches[0].Value.Trim();

                if (matches.Count > 1 && string.IsNullOrWhiteSpace(details))
                    details = matches[1].Value.Trim();
            }

            if (string.IsNullOrWhiteSpace(pivot))
            {
                var selectMatches = Regex.Matches(llmResponse, @"SELECT[\s\S]*?;\s*", RegexOptions.IgnoreCase);
                if (selectMatches.Count >= 3)
                    pivot = selectMatches[2].Value.Trim().TrimEnd(';');
            }

            return (summary, details, pivot, reportType);
        }

    }
}
