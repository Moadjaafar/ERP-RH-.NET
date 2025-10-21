using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using ClosedXML.Excel;
using System.Diagnostics;

namespace APPLICATION_RH
{
    public partial class invoice_info_extractor : System.Web.UI.Page
    {
        // Add your Claude API key in web.config: <add key="ClaudeApiKey" value="sk-ant-..." />
        private readonly string _claudeApiKey = ConfigurationManager.AppSettings["ClaudeApiKey"];

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    MessageLabel.Text = "";
                    FileInfoLabel.Text = "";
                    
                    // Log page access
                    LogInfo("Invoice extractor page loaded successfully");
                    
                    // Check if API key is configured
                    if (string.IsNullOrEmpty(_claudeApiKey))
                    {
                        ShowError("System configuration error. Claude API key is missing. Please contact administrator.");
                        GenerateButton.Enabled = false;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in Page_Load: {ex.Message}", ex);
                ShowError("An error occurred while loading the page. Please refresh and try again.");
            }
        }

        protected async void GenerateButton_Click(object sender, EventArgs e)
        {
            try
            {
                LogInfo("Generate button clicked - starting dashboard generation");
                
                // Clear previous messages
                MessageLabel.Text = "";
                
                // Use async/await directly - simpler and more reliable for Web Forms 4.5+
                await GenerateDashboardAsync();
            }
            catch (Exception ex)
            {
                LogError($"Error in GenerateButton_Click: {ex.Message}", ex);
                ShowError("Failed to start dashboard generation. Please try again.");
                
                // Reset button state
                GenerateButton.Text = "Generate Dashboard with AI";
                GenerateButton.Enabled = true;
            }
        }

        private async Task GenerateDashboardAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                LogInfo("Starting dashboard generation process");

                if (!FileUploadControl.HasFile)
                {
                    ShowError("Please select an Excel file first");
                    LogWarning("Dashboard generation failed: No file selected");
                    return;
                }

                if (string.IsNullOrEmpty(_claudeApiKey))
                {
                    ShowError("System configuration error. Please contact administrator.");
                    LogError("Claude API key is not configured", null);
                    return;
                }

                // Validate file
                if (!ValidateFile())
                {
                    LogWarning("File validation failed");
                    return;
                }

                // Update button text
                GenerateButton.Text = "AI is analyzing your data...";
                GenerateButton.Enabled = false;

                LogInfo($"Reading file: {FileUploadControl.FileName}, Size: {FileUploadControl.PostedFile.ContentLength} bytes");

                // Read Excel data with timeout protection
                DataTable excelData = null;
                using (var timeoutCts = new System.Threading.CancellationTokenSource(TimeSpan.FromMinutes(2)))
                {
                    try
                    {
                        excelData = await ReadExcelFile();
                    }
                    catch (OperationCanceledException)
                    {
                        throw new Exception("File reading timeout. Please try with a smaller file.");
                    }
                }

                if (excelData == null || excelData.Rows.Count == 0)
                {
                    ShowError("No data found in the Excel file.");
                    LogWarning("Excel file contains no data");
                    return;
                }

                LogInfo($"Successfully read Excel data: {excelData.Rows.Count} rows, {excelData.Columns.Count} columns");

                // Generate dashboard with Claude AI
                var dashboardHtml = await GenerateDashboardWithClaude(excelData);

                if (!string.IsNullOrEmpty(dashboardHtml))
                {
                    // Display the generated dashboard
                    DashboardLiteral.Text = dashboardHtml;
                    dashboardContainer.Visible = true;
                    uploadSection.Visible = false;
                    
                    var successMessage = $"Dashboard generated successfully! Analyzed {excelData.Rows.Count} rows with {excelData.Columns.Count} columns in {stopwatch.ElapsedMilliseconds}ms";
                    ShowSuccess(successMessage);
                    LogInfo(successMessage);
                }
                else
                {
                    ShowError("Failed to generate dashboard. Please try again.");
                    LogError("Dashboard HTML generation returned empty result", null);
                }
            }
            catch (OutOfMemoryException ex)
            {
                var errorMsg = "File too large for processing. Please try with a smaller file.";
                ShowError(errorMsg);
                LogError($"Out of memory error: {errorMsg}", ex);
            }
            catch (HttpRequestException ex)
            {
                var errorMsg = "Network connection error. Please check your internet connection and try again.";
                ShowError(errorMsg);
                LogError($"HTTP request error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Unexpected error: {ex.Message}";
                ShowError(errorMsg);
                LogError($"Dashboard generation error after {stopwatch.ElapsedMilliseconds}ms", ex);
                
                // Force garbage collection to free up memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            finally
            {
                GenerateButton.Text = "Generate Dashboard with AI";
                GenerateButton.Enabled = true;
                stopwatch.Stop();
                
                LogInfo($"Dashboard generation process completed in {stopwatch.ElapsedMilliseconds}ms");
            }
        }

        private bool ValidateFile()
        {
            var file = FileUploadControl.PostedFile;

            // Check file size (10MB limit)
            if (file.ContentLength > 10 * 1024 * 1024)
            {
                ShowError("File size must be less than 10MB");
                return false;
            }

            // Check file extension
            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".xlsx", ".xls", ".csv" };

            if (!allowedExtensions.Contains(fileExtension))
            {
                ShowError("Please upload only Excel (.xlsx, .xls) or CSV files");
                return false;
            }

            return true;
        }

        private async Task<DataTable> ReadExcelFile()
        {
            try
            {
                var file = FileUploadControl.PostedFile;
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (fileExtension == ".csv")
                {
                    return ReadCsvFile();
                }
                else
                {
                    return await ReadExcelWithClosedXML();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading file: {ex.Message}");
            }
        }

        private async Task<DataTable> ReadExcelWithClosedXML()
        {
            var dataTable = new DataTable();
            var stream = FileUploadControl.PostedFile.InputStream;
            
            try
            {
                LogInfo("Starting Excel file reading with ClosedXML");
                
                // Reset stream position
                stream.Position = 0;
                
                // Use memory stream for better performance
                byte[] buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                
                using (var memoryStream = new MemoryStream(buffer))
                using (var workbook = new XLWorkbook(memoryStream))
                {
                    var worksheet = workbook.Worksheets.First();
                    LogInfo($"Reading worksheet: {worksheet.Name}");

                    // Get the used range
                    var range = worksheet.RangeUsed();
                    if (range == null)
                    {
                        throw new Exception("The Excel file appears to be empty or has no data.");
                    }

                    var totalRows = range.RowCount();
                    var totalColumns = range.ColumnCount();
                    LogInfo($"Excel file contains {totalRows} rows and {totalColumns} columns");

                    // Limit processing for large files to prevent memory issues
                    const int MAX_ROWS = 10000; // Limit to prevent memory issues
                    const int MAX_COLUMNS = 50;
                    
                    if (totalRows > MAX_ROWS)
                    {
                        LogWarning($"Large file detected ({totalRows} rows). Processing only first {MAX_ROWS} rows.");
                        totalRows = MAX_ROWS;
                    }
                    
                    if (totalColumns > MAX_COLUMNS)
                    {
                        LogWarning($"Wide file detected ({totalColumns} columns). Processing only first {MAX_COLUMNS} columns.");
                        totalColumns = MAX_COLUMNS;
                    }

                    // Add columns from first row
                    var firstRow = range.FirstRow();
                    var columnNames = new List<string>();
                    var cellsToProcess = firstRow.Cells().Take(totalColumns);

                    foreach (var cell in cellsToProcess)
                    {
                        string columnName = cell.GetValue<string>()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(columnName))
                            columnName = $"Column{dataTable.Columns.Count + 1}";

                        // Ensure unique column names
                        string uniqueColumnName = columnName;
                        int counter = 1;
                        while (columnNames.Contains(uniqueColumnName))
                        {
                            uniqueColumnName = $"{columnName}_{counter}";
                            counter++;
                        }

                        columnNames.Add(uniqueColumnName);
                        dataTable.Columns.Add(uniqueColumnName, typeof(string));
                    }

                    LogInfo($"Created {dataTable.Columns.Count} columns");

                    // Add data rows (skip header) with progress tracking
                    var dataRows = range.Rows().Skip(1).Take(totalRows - 1);
                    int rowCount = 0;
                    int processedRows = 0;
                    
                    foreach (var row in dataRows)
                    {
                        var dataRow = dataTable.NewRow();
                        var cells = row.Cells().Take(dataTable.Columns.Count).ToArray();
                        bool hasData = false;

                        for (int i = 0; i < cells.Length && i < dataTable.Columns.Count; i++)
                        {
                            var cellValue = cells[i].GetValue<string>()?.Trim() ?? "";
                            dataRow[i] = cellValue;
                            
                            if (!string.IsNullOrWhiteSpace(cellValue))
                                hasData = true;
                        }

                        // Only add row if it has some data
                        if (hasData)
                        {
                            dataTable.Rows.Add(dataRow);
                            processedRows++;
                        }
                        
                        rowCount++;
                        
                        // Log progress for large files
                        if (rowCount % 1000 == 0)
                        {
                            LogInfo($"Processed {rowCount} rows, added {processedRows} data rows");
                            
                            // Force garbage collection periodically for large files
                            if (rowCount % 5000 == 0)
                            {
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                            }
                        }
                    }
                    
                    LogInfo($"Successfully processed {processedRows} data rows from {rowCount} total rows");
                }
                
                // Clear buffer from memory
                buffer = null;
            }
            catch (OutOfMemoryException ex)
            {
                LogError("Out of memory while reading Excel file", ex);
                throw new Exception("File too large for processing. Please try with a smaller file or contact administrator.");
            }
            catch (Exception ex)
            {
                LogError($"Failed to read Excel file: {ex.Message}", ex);
                throw new Exception($"Failed to read Excel file with ClosedXML: {ex.Message}");
            }
            finally
            {
                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            return dataTable;
        }

        private DataTable ReadCsvFile()
        {
            var dataTable = new DataTable();

            using (var reader = new StreamReader(FileUploadControl.PostedFile.InputStream))
            {
                string line;
                bool isFirstLine = true;

                while ((line = reader.ReadLine()) != null)
                {
                    var values = ParseCsvLine(line);

                    if (isFirstLine)
                    {
                        // Create columns
                        for (int i = 0; i < values.Length; i++)
                        {
                            var columnName = string.IsNullOrEmpty(values[i]) ? $"Column{i + 1}" : values[i].Trim();
                            dataTable.Columns.Add(columnName, typeof(string));
                        }
                        isFirstLine = false;
                    }
                    else
                    {
                        // Add data row
                        var row = dataTable.NewRow();
                        for (int i = 0; i < Math.Min(values.Length, dataTable.Columns.Count); i++)
                        {
                            row[i] = values[i];
                        }
                        dataTable.Rows.Add(row);
                    }
                }
            }

            return dataTable;
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString().Trim());
            return result.ToArray();
        }

        private async Task<string> GenerateDashboardWithClaude(DataTable data)
        {
            LogInfo("Starting Claude API dashboard generation");
            
            try
            {
                // Prepare optimized data sample for Claude
                var dataSample = PrepareDataSample(data);
                LogInfo($"Prepared data sample: {dataSample.Length} characters");

                string prompt = $@"You are a professional dashboard generator. I have Excel data that I want you to analyze and create a complete, interactive dashboard.

**Data Sample (first 20 rows):**
{dataSample}

**Your task:** Create a COMPLETE HTML dashboard with the following requirements:

1. **Professional Design:**
   - Modern, clean interface with gradients and shadows
   - Responsive layout that works on all devices
   - Professional color scheme (blues, grays, whites)
   - Card-based layout for different sections

2. **Smart Filters:**
   - Automatically detect which columns should have filters
   - Create dropdowns for categorical data
   - Create range inputs for numerical data
   - Date pickers for date columns
   - ""Apply Filters"" and ""Clear All"" buttons

3. **Interactive Charts:**
   - Automatically choose appropriate chart types based on data
   - Bar charts for categorical data
   - Line charts for time series data
   - Pie charts for distributions
   - Use Chart.js library (include from CDN)

4. **Summary Cards:**
   - Show key metrics like total records, averages, counts
   - Calculate meaningful statistics from the data
   - Use icons and colors for visual appeal

5. **Data Table:**
   - Filterable and sortable data table
   - Pagination if needed
   - Search functionality

6. **Export Features:**
   - Export filtered data as CSV
   - Export charts as images

**Technical Requirements:**
- Use only HTML, CSS, and JavaScript (no external frameworks except Chart.js)
- Include Chart.js from CDN: https://cdnjs.cloudflare.com/ajax/libs/Chart.js/3.9.1/chart.min.js
- Make it fully functional with the provided data
- Use modern CSS (flexbox, grid, gradients, shadows)
- Include all JavaScript inline in script tags
- NO external dependencies except Chart.js

**Data Analysis Instructions:**
- Automatically detect data types (text, numbers, dates, categories)
- Generate appropriate filters based on data types
- Create meaningful charts based on the data structure
- Calculate relevant statistics and KPIs
- Handle missing or empty values gracefully

Please generate the COMPLETE, FUNCTIONAL HTML code that I can display directly in my web page. The dashboard should work immediately with the provided data.

**Response Format:** Return ONLY the complete HTML code, starting with proper HTML structure and including all CSS and JavaScript inline.";

                using (var httpClient = new HttpClient())
                {
                    // Set appropriate timeout
                    httpClient.Timeout = TimeSpan.FromMinutes(3);
                    LogInfo("Created HTTP client for Claude API");

                    var requestBody = new
                    {
                          max_tokens = 4000,
                        temperature = 0.1, // Lower temperature for more consistent output
                        messages = new[]
                        {
                            new
                            {
                                role = "user",
                                content = prompt
                            }
                        }
                    };

                    httpClient.DefaultRequestHeaders.Add("x-api-key", _claudeApiKey);
                    httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "ERP-RH-Dashboard-Generator/1.0");

                    var jsonContent = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    
                    LogInfo($"Sending request to Claude API, payload size: {jsonContent.Length} characters");

                    var response = await httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        LogInfo($"Received Claude API response: {responseContent.Length} characters");
                        
                        dynamic responseJson = JsonConvert.DeserializeObject(responseContent);

                        if (responseJson?.content != null && responseJson.content.Count > 0)
                        {
                            string dashboardHtml = responseJson.content[0].text;
                            LogInfo($"Extracted dashboard HTML: {dashboardHtml.Length} characters");

                            // Clean up the response (remove markdown code blocks if present)
                            if (dashboardHtml.Contains("```html"))
                            {
                                var startIndex = dashboardHtml.IndexOf("```html") + 7;
                                var endIndex = dashboardHtml.LastIndexOf("```");
                                if (endIndex > startIndex)
                                {
                                    dashboardHtml = dashboardHtml.Substring(startIndex, endIndex - startIndex);
                                }
                            }
                            else if (dashboardHtml.Contains("```"))
                            {
                                var startIndex = dashboardHtml.IndexOf("```") + 3;
                                var endIndex = dashboardHtml.LastIndexOf("```");
                                if (endIndex > startIndex)
                                {
                                    dashboardHtml = dashboardHtml.Substring(startIndex, endIndex - startIndex);
                                }
                            }

                            var cleanedHtml = dashboardHtml.Trim();
                            LogInfo($"Dashboard HTML generation successful: {cleanedHtml.Length} characters");
                            return cleanedHtml;
                        }
                        else
                        {
                            LogError("Claude API returned empty content", null);
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();

                        // Better error handling
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            throw new Exception("Claude model not available. Please check your API configuration.");
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            throw new Exception("Invalid API key. Please check your Claude API key configuration.");
                        }
                        else
                        {
                            throw new Exception($"Claude API error: {response.StatusCode} - {errorContent}");
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Network error while connecting to Claude API: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception("Request timeout. The dashboard generation took too long. Please try with a smaller file.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Dashboard generation failed: {ex.Message}");
            }

            return null;
        }

        private string PrepareDataSample(DataTable data)
        {
            var sb = new StringBuilder();

            // Add column headers
            var headers = data.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
            sb.AppendLine("Columns: " + string.Join(" | ", headers));
            sb.AppendLine();

            // Add data types analysis
            sb.AppendLine("Data Types Analysis:");
            foreach (DataColumn column in data.Columns)
            {
                var sampleValues = data.AsEnumerable()
                    .Take(10)
                    .Select(row => row[column.ColumnName]?.ToString()?.Trim())
                    .Where(val => !string.IsNullOrEmpty(val))
                    .ToList();

                var dataType = AnalyzeDataType(sampleValues);
                var samples = sampleValues.Take(3).ToList();
                sb.AppendLine($"- {column.ColumnName}: {dataType} (samples: {string.Join(", ", samples)})");
            }
            sb.AppendLine();

            // Add sample data rows (limit to prevent token overflow)
            sb.AppendLine("Sample Data Rows:");
            var rowCount = Math.Min(15, data.Rows.Count); // Reduced from 20 to 15
            for (int i = 0; i < rowCount; i++)
            {
                var row = data.Rows[i];
                var values = row.ItemArray.Select(field => {
                    var val = field?.ToString() ?? "";
                    return val.Length > 50 ? val.Substring(0, 50) + "..." : val; // Truncate long values
                }).ToArray();
                sb.AppendLine($"Row {i + 1}: " + string.Join(" | ", values));
            }

            sb.AppendLine();
            sb.AppendLine($"Total Rows: {data.Rows.Count}");
            sb.AppendLine($"Total Columns: {data.Columns.Count}");

            return sb.ToString();
        }

        private string AnalyzeDataType(List<string> values)
        {
            if (values.Count == 0) return "Empty";

            int numericCount = 0;
            int dateCount = 0;
            int booleanCount = 0;

            foreach (var value in values)
            {
                // Check for numeric
                if (decimal.TryParse(value.Replace("$", "").Replace(",", "").Replace("%", ""), out _))
                    numericCount++;

                // Check for date
                if (DateTime.TryParse(value, out _))
                    dateCount++;

                // Check for boolean
                if (value.ToLower() == "true" || value.ToLower() == "false" ||
                    value.ToLower() == "yes" || value.ToLower() == "no" ||
                    value == "1" || value == "0")
                    booleanCount++;
            }

            double numericRatio = (double)numericCount / values.Count;
            double dateRatio = (double)dateCount / values.Count;
            double booleanRatio = (double)booleanCount / values.Count;

            if (booleanRatio > 0.7) return "Boolean";
            if (dateRatio > 0.5) return "Date";
            if (numericRatio > 0.7) return "Number";

            var distinctCount = values.Distinct().Count();
            if (distinctCount < values.Count * 0.5 && distinctCount < 15) return "Category";

            return "Text";
        }

        private void ShowError(string message)
        {
            MessageLabel.Text = $"<div class=\"message error\">{HttpUtility.HtmlEncode(message)}</div>";
        }

        private void ShowSuccess(string message)
        {
            MessageLabel.Text = $"<div class=\"message success\">{HttpUtility.HtmlEncode(message)}</div>";
        }
        
        // Logging methods for better debugging and monitoring
        private void LogInfo(string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                // Also log to Windows Event Log in production
                if (!System.Web.HttpContext.Current.IsDebuggingEnabled)
                {
                    EventLog.WriteEntry("ERP-RH Application", $"[INFO] {message}", EventLogEntryType.Information);
                }
            }
            catch
            {
                // Ignore logging errors to prevent cascading failures
            }
        }
        
        private void LogWarning(string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[WARNING] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                if (!System.Web.HttpContext.Current.IsDebuggingEnabled)
                {
                    EventLog.WriteEntry("ERP-RH Application", $"[WARNING] {message}", EventLogEntryType.Warning);
                }
            }
            catch
            {
                // Ignore logging errors
            }
        }
        
        private void LogError(string message, Exception ex)
        {
            try
            {
                var fullMessage = ex != null 
                    ? $"{message} - Exception: {ex.ToString()}" 
                    : message;
                    
                System.Diagnostics.Debug.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {fullMessage}");
                
                if (!System.Web.HttpContext.Current.IsDebuggingEnabled)
                {
                    EventLog.WriteEntry("ERP-RH Application", $"[ERROR] {fullMessage}", EventLogEntryType.Error);
                }
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}


