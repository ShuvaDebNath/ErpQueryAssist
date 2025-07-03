# ERP Query Assistant - RAG UI Rendering (Dynamic View)

This README explains how we implemented a dynamic Razor view in an ASP.NET Core MVC application to support a Retrieval-Augmented Generation (RAG) workflow that generates SQL from natural language and displays fully dynamic UI outputs.

---

## 🚀 Purpose

Enable users to ask questions in natural language (e.g., "How many PIs were created in the last 7 days?") and receive a fully rendered UI with Summary, Detailed Table, or Pivot results from ERP data.

---

## 📊 Architecture Overview (RAG Flow)

1. **User asks a question** from the web UI.
2. **LLM server** receives prompt and generates SQL query.
3. **Query service** executes SQL and returns dynamic result.
4. **Controller** detects the report type (Summary, Details, Pivot).
5. **Razor View (********`cshtml`****\*\*\*\*\*\*\*\*)** renders the dynamic result UI.

> Note: Razor views are **static templates**, but render **dynamic data** from the LLM-generated SQL result.

---

## 🔄 Dynamic Razor View Sample

### ViewModel: `UniversalQueryResult`

```csharp
public class UniversalQueryResult
{
    public int TotalPIs { get; set; }
    public decimal TotalQty { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PIDetail> Details { get; set; }
}

public class PIDetail
{
    public DateTime ApproveDate { get; set; }
    public string PIType { get; set; }
    public string ClientName { get; set; }
    public string PINumber { get; set; }
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public string MarketingPerson { get; set; }
}
```

### Razor View: `UniversalResult.cshtml`

```csharp
@model UniversalQueryResult

<div class="row">
    <div class="col-md-4">
        <div class="card border-success text-center">
            <h6>No of PI</h6>
            <h3 class="text-success">@Model.TotalPIs</h3>
        </div>
    </div>
    <div class="col-md-4">
        <div class="card border-primary text-center">
            <h6>Total Qty in KGs</h6>
            <h3 class="text-primary">@Model.TotalQty.ToString("N0")</h3>
        </div>
    </div>
    <div class="col-md-4">
        <div class="card border-dark text-center">
            <h6>Total Amount</h6>
            <h3 class="text-dark">@Model.TotalAmount.ToString("N0")</h3>
        </div>
    </div>
</div>

<table class="table table-bordered mt-4">
    <thead class="table-light">
        <tr>
            <th>S/N</th>
            <th>Approve Date</th>
            <th>PI Type</th>
            <th>Client Name</th>
            <th>PI Number</th>
            <th class="text-end">Quantity</th>
            <th class="text-end">Amount</th>
            <th>Marketing Person</th>
        </tr>
    </thead>
    <tbody>
        @for (int i = 0; i < Model.Details.Count; i++)
        {
            var item = Model.Details[i];
            <tr>
                <td>@(i + 1)</td>
                <td>@item.ApproveDate.ToString("yyyy-MM-dd")</td>
                <td>@item.PIType</td>
                <td>@item.ClientName</td>
                <td>@item.PINumber</td>
                <td class="text-end">@item.Quantity.ToString("N2")</td>
                <td class="text-end">@item.Amount.ToString("N2")</td>
                <td>@item.MarketingPerson</td>
            </tr>
        }
    </tbody>
    <tfoot class="table-success fw-bold">
        <tr>
            <td colspan="5" class="text-end">Total</td>
            <td class="text-end">@Model.TotalQty.ToString("N2")</td>
            <td class="text-end">@Model.TotalAmount.ToString("N2")</td>
            <td></td>
        </tr>
    </tfoot>
</table>
```
🖼️ Sample Output

### Universal Summary with Details Report  
![Universal Summary](https://raw.githubusercontent.com/ShuvaDebNath/ErpQueryAssist/main/WebAssets/samples/summary-with-details.png)

---

### Month-wise Pivot by Client  
![Universal Summary](https://raw.githubusercontent.com/ShuvaDebNath/ErpQueryAssist/main/WebAssets/samples/client-wise-pivot.png)

---

### Month-wise Pivot  
![Universal Summary](https://raw.githubusercontent.com/ShuvaDebNath/ErpQueryAssist/main/WebAssets/samples/month-summary.png)



---

## 📈 Benefits

* Fully dynamic UI using Razor + ViewModel
* Easily extendable for monthly/yearly pivot, charts
* Follows Clean Architecture & separation of concern
* Scalable for any ERP module (Sales, Purchase, Accounts)

---

## 🔧 To Extend

* Add `ChartViewModel` to draw bar/line graphs
* Add Export to Excel/PDF buttons
* Make year/month accordion view (already planned)

---

## ✅ Final Note

This project proves that **.cshtml can act as reusable UI templates** for **LLM-generated SQL outputs**, completing the Retrieval-Augmented Generation pipeline in a visually rich ERP assistant system.
