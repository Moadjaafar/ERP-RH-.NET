using System.Data.OleDb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH
{
    public partial class chartsAI : System.Web.UI.Page
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private string claudeApiKey;
        private List<Dictionary<string, object>> currentData;
        private List<Dictionary<string, object>> originalData;
        private List<string> dateColumns;


        protected void Page_Load(object sender, EventArgs e)
        {
            // Get Claude API key from web.config
            claudeApiKey = ConfigurationManager.AppSettings["ClaudeApiKey"];

            if (string.IsNullOrEmpty(claudeApiKey))
            {
                ShowError("Claude API key not found in configuration.");
                return;
            }

            // Restore data from ViewState
            if (ViewState["CurrentData"] != null)
            {
                currentData = (List<Dictionary<string, object>>)ViewState["CurrentData"];
                originalData = (List<Dictionary<string, object>>)ViewState["OriginalData"];
                dateColumns = (List<string>)ViewState["DateColumns"];

                if (!IsPostBack)
                {
                    RestoreDataState();
                }
            }
        }

        protected async void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                if (!fileUpload.HasFile)
                {
                    ShowError("Please select an Excel file to upload.");
                    return;
                }

                // Validate file extension
                string fileExtension = Path.GetExtension(fileUpload.FileName).ToLower();
                if (fileExtension != ".xlsx" && fileExtension != ".xls")
                {
                    ShowError("Please upload only Excel files (.xlsx or .xls).");
                    return;
                }

                // Show loading
                loadingSection.Visible = true;

                // Process Excel file
                await ProcessExcelFile();

                // Hide loading
                loadingSection.Visible = false;

                ShowSuccess("Excel file processed successfully!");
            }
            catch (Exception ex)
            {
                loadingSection.Visible = false;
                ShowError($"Error processing file: {ex.Message}");
            }
        }

        private async Task ProcessExcelFile()
        {
            // Save uploaded file temporarily
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(fileUpload.FileName));
            
            try
            {
                fileUpload.SaveAs(tempPath);
                
                // Determine connection string based on file extension
                string connectionString;
                string fileExtension = Path.GetExtension(fileUpload.FileName).ToLower();
                
                if (fileExtension == ".xlsx")
                {
                    connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={tempPath};Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1'";
                }
                else if (fileExtension == ".xls")
                {
                    connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={tempPath};Extended Properties='Excel 8.0;HDR=YES;IMEX=1'";
                }
                else
                {
                    ShowError("Unsupported file format. Please use .xlsx or .xls files.");
                    return;
                }

                using (var connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    
                    // Get the first worksheet
                    DataTable schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (schemaTable.Rows.Count == 0)
                    {
                        ShowError("No worksheets found in the Excel file.");
                        return;
                    }
                    
                    string sheetName = schemaTable.Rows[0]["TABLE_NAME"].ToString();
                    
                    // Query all data from the first sheet
                    string query = $"SELECT * FROM [{sheetName}]";
                    using (var command = new OleDbCommand(query, connection))
                    using (var adapter = new OleDbDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        
                        if (dataTable.Rows.Count == 0)
                        {
                            ShowError("The Excel file appears to be empty.");
                            return;
                        }
                        
                        // Convert DataTable to List<Dictionary<string, object>>
                        currentData = new List<Dictionary<string, object>>();
                        
                        foreach (DataRow row in dataTable.Rows)
                        {
                            var dataRow = new Dictionary<string, object>();
                            
                            foreach (DataColumn column in dataTable.Columns)
                            {
                                dataRow[column.ColumnName] = row[column] == DBNull.Value ? null : row[column];
                            }
                            
                            currentData.Add(dataRow);
                        }
                        
                        originalData = new List<Dictionary<string, object>>(currentData);

                        // Identify date columns
                        IdentifyDateColumns();

                        // Save to ViewState
                        ViewState["CurrentData"] = currentData;
                        ViewState["OriginalData"] = originalData;
                        ViewState["DateColumns"] = dateColumns;

                        // Update UI
                        UpdateDataDisplay();
                        SetupControls();

                        // Show data section
                        dataSection.Visible = true;
                        btnGetAISuggestions.Visible = true;
                    }
                }
            }
            finally
            {
                // Clean up temporary file
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

        private void IdentifyDateColumns()
        {
            dateColumns = new List<string>();

            if (currentData.Count == 0) return;

            var headers = currentData[0].Keys.ToList();

            foreach (var header in headers)
            {
                // Check first 5 rows for date values
                var sampleValues = currentData.Take(5).Select(row => row[header]).ToList();

                bool isDateColumn = sampleValues.Any(value =>
                {
                    if (value == null) return false;

                    DateTime dateValue;
                    return DateTime.TryParse(value.ToString(), out dateValue) &&
                           dateValue.Year > 1900;
                });

                if (isDateColumn)
                {
                    dateColumns.Add(header);
                }
            }

            if (dateColumns.Count > 0)
            {
                SetupDateFilter();
            }
        }

        private void SetupDateFilter()
        {
            ddlDateColumn.Items.Clear();
            ddlDateColumn.Items.Add(new ListItem("Select date column...", ""));

            foreach (var col in dateColumns)
            {
                ddlDateColumn.Items.Add(new ListItem(col, col));
            }

            pnlDateFilter.Visible = true;
        }

        private void UpdateDataDisplay()
        {
            if (currentData == null || currentData.Count == 0)
            {
                ltlDataPreview.Text = "<p class='text-muted'>No data to display</p>";
                return;
            }

            var headers = currentData[0].Keys.ToList();
            var sampleSize = Math.Min(10, currentData.Count);

            var html = new StringBuilder();
            html.Append("<table class='table table-sm table-striped'>");
            html.Append("<thead class='table-dark'><tr>");

            foreach (var header in headers)
            {
                html.AppendFormat("<th>{0}</th>", HttpUtility.HtmlEncode(header));
            }

            html.Append("</tr></thead><tbody>");

            for (int i = 0; i < sampleSize; i++)
            {
                html.Append("<tr>");
                foreach (var header in headers)
                {
                    var value = currentData[i][header]?.ToString() ?? "";
                    if (value.Length > 20)
                    {
                        value = value.Substring(0, 20) + "...";
                    }
                    html.AppendFormat("<td>{0}</td>", HttpUtility.HtmlEncode(value));
                }
                html.Append("</tr>");
            }

            html.Append("</tbody></table>");
            html.AppendFormat("<p class='text-muted small'>Showing {0} of {1} rows</p>",
                sampleSize, currentData.Count);

            ltlDataPreview.Text = html.ToString();

            // Pass data to JavaScript
            var serializer = new JavaScriptSerializer();
            hdnExcelData.Value = serializer.Serialize(currentData);
            hdnDateColumns.Value = serializer.Serialize(dateColumns);
        }

        private void SetupControls()
        {
            if (currentData == null || currentData.Count == 0) return;

            var headers = currentData[0].Keys.ToList();

            // Clear existing items
            ddlXAxis.Items.Clear();
            ddlYAxis.Items.Clear();

            ddlXAxis.Items.Add(new ListItem("Select column...", ""));
            ddlYAxis.Items.Add(new ListItem("Select column...", ""));

            foreach (var header in headers)
            {
                ddlXAxis.Items.Add(new ListItem(header, header));
                ddlYAxis.Items.Add(new ListItem(header, header));
            }
        }

        private void RestoreDataState()
        {
            UpdateDataDisplay();
            SetupControls();

            if (dateColumns != null && dateColumns.Count > 0)
            {
                SetupDateFilter();
            }

            dataSection.Visible = true;
            btnGetAISuggestions.Visible = true;
        }

        protected async void btnGetAISuggestions_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentData == null || currentData.Count == 0)
                {
                    ShowError("No data available. Please upload an Excel file first.");
                    return;
                }

                loadingSection.Visible = true;

                await GetAISuggestions();

                loadingSection.Visible = false;
            }
            catch (Exception ex)
            {
                loadingSection.Visible = false;
                ShowError($"Error getting AI suggestions: {ex.Message}");
            }
        }

        private async Task GetAISuggestions()
        {
            try
            {
                // Prepare data sample for AI analysis
                var dataSample = new
                {
                    headers = currentData[0].Keys.ToArray(),
                    sampleRows = currentData.Take(5).ToArray(),
                    totalRows = currentData.Count,
                    dateColumns = dateColumns.ToArray()
                };

                var prompt = $@"Analyze this Excel data and suggest the best chart types and configurations. Consider the fish industry/ERP context if relevant.

Data structure:
{new JavaScriptSerializer().Serialize(dataSample)}

Please suggest 3-5 different chart configurations with explanations. For each suggestion, provide:
1. Chart type (line, bar, pie, scatter, etc.)
2. X-axis column
3. Y-axis column (if applicable)
4. Brief explanation of why this chart would be insightful
5. Title suggestion

Respond in JSON format like this:
{{
  ""suggestions"": [
    {{
      ""type"": ""bar"",
      ""xAxis"": ""column_name"",
      ""yAxis"": ""column_name"", 
      ""title"": ""Chart Title"",
      ""explanation"": ""Why this chart is useful""
    }}
  ]
}}";

                var requestBody = new
                {
                    model = "claude-sonnet-4-20250514",
                    max_tokens = 1500,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var json = new JavaScriptSerializer().Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("x-api-key", claudeApiKey);
                httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                var response = await httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = new JavaScriptSerializer().Deserialize<dynamic>(responseContent);
                        string claudeText = apiResponse["content"][0]["text"];
                        
                        // Clean up the Claude response text - remove markdown code blocks if present
                        claudeText = claudeText.Trim();
                        if (claudeText.StartsWith("```json"))
                        {
                            claudeText = claudeText.Substring(7).Trim();
                        }
                        if (claudeText.StartsWith("```"))
                        {
                            claudeText = claudeText.Substring(3).Trim();
                        }
                        if (claudeText.EndsWith("```"))
                        {
                            claudeText = claudeText.Substring(0, claudeText.Length - 3).Trim();
                        }
                        
                        // Parse the cleaned JSON
                        var suggestions = new JavaScriptSerializer().Deserialize<dynamic>(claudeText);

                        if (suggestions != null && suggestions.ContainsKey("suggestions"))
                        {
                            // Pass suggestions to JavaScript
                            hdnSuggestionsData.Value = new JavaScriptSerializer().Serialize(suggestions["suggestions"]);

                            // Display suggestions in UI
                            DisplaySuggestions(suggestions["suggestions"]);

                            ShowSuccess("AI suggestions generated successfully!");
                        }
                        else
                        {
                            throw new Exception("Invalid suggestions format received from AI");
                        }
                    }
                    catch (Exception parseEx)
                    {
                        // Log the actual response for debugging
                        throw new Exception($"Error parsing AI response: {parseEx.Message}. Response: {responseContent}");
                    }
                }
                else
                {
                    throw new Exception($"API request failed: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calling Claude API: {ex.Message}");
            }
        }

        private void DisplaySuggestions(dynamic suggestions)
        {
            var html = new StringBuilder();

            for (int i = 0; i < suggestions.Length; i++)
            {
                var suggestion = suggestions[i];
                html.AppendFormat(@"
                    <div class='suggestion-item' onclick='applySuggestion({0})'>
                        <strong>{1}</strong><br>
                        <small class='text-muted'>{2}: {3} vs {4}</small><br>
                        <small>{5}</small>
                    </div>",
                    i,
                    HttpUtility.HtmlEncode(suggestion["title"].ToString()),
                    HttpUtility.HtmlEncode(suggestion["type"].ToString().ToUpper()),
                    HttpUtility.HtmlEncode(suggestion["xAxis"].ToString()),
                    HttpUtility.HtmlEncode(suggestion["yAxis"]?.ToString() ?? "values"),
                    HttpUtility.HtmlEncode(suggestion["explanation"].ToString())
                );
            }

            ltlSuggestions.Text = html.ToString();
            pnlSuggestions.Visible = true;
        }

        protected void btnApplyFilter_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyDateFilter();
            }
            catch (Exception ex)
            {
                ShowError($"Error applying filter: {ex.Message}");
            }
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            try
            {
                ClearDateFilter();
            }
            catch (Exception ex)
            {
                ShowError($"Error clearing filter: {ex.Message}");
            }
        }

        private void ApplyDateFilter()
        {
            if (originalData == null || originalData.Count == 0)
            {
                ShowError("No original data available.");
                return;
            }

            string dateCol = ddlDateColumn.SelectedValue;
            string dateFrom = txtDateFrom.Text;
            string dateTo = txtDateTo.Text;

            if (string.IsNullOrEmpty(dateCol) || string.IsNullOrEmpty(dateFrom) || string.IsNullOrEmpty(dateTo))
            {
                ShowError("Please select a date column and date range.");
                return;
            }

            DateTime fromDate = DateTime.Parse(dateFrom);
            DateTime toDate = DateTime.Parse(dateTo);

            currentData = originalData.Where(row =>
            {
                var cellValue = row[dateCol];
                if (cellValue == null) return false;

                DateTime rowDate;
                return DateTime.TryParse(cellValue.ToString(), out rowDate) &&
                       rowDate >= fromDate && rowDate <= toDate;
            }).ToList();

            // Update ViewState
            ViewState["CurrentData"] = currentData;

            // Update display
            UpdateDataDisplay();

            ShowSuccess($"Filter applied. Showing {currentData.Count} of {originalData.Count} rows.");
        }

        private void ClearDateFilter()
        {
            if (originalData == null)
            {
                ShowError("No original data available.");
                return;
            }

            currentData = new List<Dictionary<string, object>>(originalData);
            ViewState["CurrentData"] = currentData;

            txtDateFrom.Text = "";
            txtDateTo.Text = "";

            UpdateDataDisplay();

            ShowSuccess("Filter cleared. Showing all data.");
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            pnlSuccess.Visible = false;
            ltlError.Text = HttpUtility.HtmlEncode(message);
        }

        private void ShowSuccess(string message)
        {
            pnlSuccess.Visible = true;
            pnlError.Visible = false;
            ltlSuccess.Text = HttpUtility.HtmlEncode(message);
        }
    }
}