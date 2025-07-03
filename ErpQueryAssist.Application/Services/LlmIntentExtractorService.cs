
using ErpQueryAssist.Application.DTOs;
using ErpQueryAssist.Application.Interfaces;
using System.Diagnostics;

namespace ErpQueryAssist.Application.Services;

public class LlmIntentExtractorService
{
    private readonly ILlmService _llmService;

    public LlmIntentExtractorService(ILlmService llmService)
    {
        _llmService = llmService;
    }

    public async Task<QueryIntentDto?> ExtractAsync(string question)
    {
        string prompt = $@"
You are an assistant that extracts structured SQL query intent from natural language.

✅ Always respond in valid JSON using this format exactly:

{{
  ""action"": ""summary | details | chart"",
  ""table"": ""Order"",
  ""filters"": [
    {{ ""column"": ""MakeDate"", ""operator"": ""YEAR"", ""value"": ""LAST_YEAR"" }}
  ],
  ""orderBy"": {{ ""column"": ""MakeDate"", ""direction"": ""DESC"" }},
  ""limit"": 15,
  ""metrics"": [""COUNT(*)"", ""SUM(TotalAmount)""] or null,
  ""groupBy"": ""MakeBy"" or null
}}

Example:
User: “Give me last 15 PI details from last year”
Response:
{{
  ""action"": ""details"",
  ""table"": ""Order"",
  ""filters"": [
    {{ ""column"": ""MakeDate"", ""operator"": ""YEAR"", ""value"": ""LAST_YEAR"" }}
  ],
  ""orderBy"": {{ ""column"": ""MakeDate"", ""direction"": ""DESC"" }},
  ""limit"": 15,
  ""metrics"": null,
  ""groupBy"": null
}}

Now extract structured intent from this user question:
{question}

Respond in JSON only. Do not write SQL. Do not explain.
";

        var rawResponse = await _llmService.AskAsync(prompt);

        if (!rawResponse.Trim().StartsWith("{"))
        {
            Debug.WriteLine("❌ Not a JSON response:\n" + rawResponse);
            return null;
        }

        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var intent = System.Text.Json.JsonSerializer.Deserialize<QueryIntentDto>(rawResponse, options);

            // 🔥 ADD THIS BLOCK TO NORMALIZE COLUMNS 🔥
            if (intent != null)
            {
                foreach (var filter in intent.Filters)
                {
                    filter.Column = ColumnNormalizer.Normalize(filter.Column);
                }

                if (intent.OrderBy != null)
                {
                    intent.OrderBy.Column = ColumnNormalizer.Normalize(intent.OrderBy.Column);
                }

                if (!string.IsNullOrEmpty(intent.GroupBy))
                {
                    intent.GroupBy = ColumnNormalizer.Normalize(intent.GroupBy);
                }
            }

            return intent;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Intent Parse Error]: " + ex.Message);
            return null;
        }





        

        //Debug.WriteLine("[RAW OPENAI RESPONSE]");
        //Debug.WriteLine(rawResponse);

        //try
        //{
        //    var options = new System.Text.Json.JsonSerializerOptions
        //    {
        //        PropertyNameCaseInsensitive = true
        //    };
        //    return System.Text.Json.JsonSerializer.Deserialize<QueryIntentDto>(rawResponse, options);
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine("[Intent Parse Error]: " + ex.Message);
        //    return null;
        //}
    }
}
