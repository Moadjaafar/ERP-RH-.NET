using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Configuration;
using System.IO;
// Temporarily commented out to test basic functionality
// using System.Net.Http;
// using System.Threading.Tasks;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;
// using System.Web.Services;

namespace APPLICATION_RH
{
    public partial class invoice_info_extractor : System.Web.UI.Page
    {
        // Add your Claude API key in web.config: <add key="ClaudeApiKey" value="sk-ant-..." />
        private readonly string _claudeApiKey = ConfigurationManager.AppSettings["ClaudeApiKey"];

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Initialize page
                MessageLabel.Text = "";
                FileInfoLabel.Text = "";

                // Temporarily skip API key validation for testing
                ShowMessage("✅ System ready for testing. Upload an invoice file to test basic functionality.");
                
                /* Temporarily disabled API key validation
                // Check if API key is loaded and valid format
                if (string.IsNullOrEmpty(_claudeApiKey))
                {
                    ShowError("⚠️ API Key not configured. Please check web.config file.");
                    ProcessButton.Enabled = false;
                }
                else if (!_claudeApiKey.StartsWith("sk-ant-"))
                {
                    ShowError("⚠️ Invalid API Key format. Please check web.config file.");
                    ProcessButton.Enabled = false;
                }
                else
                {
                    ShowMessage("✅ System ready. Upload an invoice file to begin extraction.");
                }
                */
            }
        }

        protected void ProcessButton_Click(object sender, EventArgs e)
        {
            try
            {
                ShowError(""); // Clear previous messages
                LogDebugInfo("ProcessButton_Click started");

                // Basic validation
                if (!FileUploadControl.HasFile)
                {
                    ShowError("Please select a file first");
                    LogDebugInfo("No file selected");
                    return;
                }

                LogDebugInfo($"File selected: {FileUploadControl.FileName}");

                // Validate file type
                var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var fileExtension = Path.GetExtension(FileUploadControl.FileName).ToLower();

                if (!allowedTypes.Contains(fileExtension))
                {
                    ShowError("Invalid file type. Please upload JPG, PNG, or PDF files only.");
                    LogDebugInfo($"Invalid file type: {fileExtension}");
                    return;
                }

                // Validate file size (10MB limit)
                if (FileUploadControl.PostedFile.ContentLength > 10 * 1024 * 1024)
                {
                    ShowError("File too large. Maximum size is 10MB.");
                    LogDebugInfo($"File too large: {FileUploadControl.PostedFile.ContentLength} bytes");
                    return;
                }

                // Validate file content length
                if (FileUploadControl.PostedFile.ContentLength == 0)
                {
                    ShowError("The uploaded file appears to be empty.");
                    LogDebugInfo("File is empty");
                    return;
                }

                LogDebugInfo($"File validation passed. Size: {FileUploadControl.PostedFile.ContentLength} bytes");

                // TEMPORARY: Just show file info without calling API
                ShowTestResults(FileUploadControl.PostedFile);
                
                LogDebugInfo("ProcessButton_Click completed successfully");
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                LogDebugInfo($"Exception in ProcessButton_Click: {ex.ToString()}");
                ShowError($"An error occurred: {ex.Message}");
            }
        }
        
        private void ShowTestResults(HttpPostedFile file)
        {
            try
            {
                LogDebugInfo("ShowTestResults started");
                
                // Create fake data to test display
                var testData = new
                {
                    company = new { name = "Test Company", address = "123 Test St", phone = "555-1234", email = "test@test.com", ice = "12345" },
                    client = new { name = "Test Client", address = "456 Client Ave", ice = "67890" },
                    invoice = new { number = "INV-001", date = DateTime.Now.ToString("yyyy-MM-dd"), type = "Standard Invoice" },
                    totals = new { subtotal = "100.00", tax_amount = "20.00", total = "120.00", currency = "USD" }
                };
                
                // Show file information
                CompanyInfo.Text = $"<strong>File Name:</strong> {file.FileName}<br><strong>Size:</strong> {file.ContentLength} bytes<br><strong>Type:</strong> {file.ContentType}";
                ClientInfo.Text = "This is a test to verify the page works without API calls.";
                InvoiceInfo.Text = $"<strong>Upload Time:</strong> {DateTime.Now}<br><strong>Status:</strong> File received successfully";
                FinancialInfo.Text = "API functionality temporarily disabled for testing.";
                
                ResultsSection.Visible = true;
                ShowSuccess("File uploaded successfully! (Test mode - no API call made)");
                
                LogDebugInfo("ShowTestResults completed successfully");
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Exception in ShowTestResults: {ex.ToString()}");
                ShowError($"Error displaying test results: {ex.Message}");
            }
        }

        /* Temporarily disabled - requires Newtonsoft.Json and System.Net.Http
        private JObject ProcessInvoiceSync(HttpPostedFile file)
        {
            try
            {
                // Convert file to base64 with proper stream handling
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    file.InputStream.Position = 0; // Reset stream position
                    file.InputStream.CopyTo(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
                
                if (fileBytes.Length == 0)
                {
                    throw new Exception("File content is empty");
                }

                string base64Data = Convert.ToBase64String(fileBytes);

                // Determine media type
                string mediaType;
                string contentType = file.ContentType.ToLower();

                if (contentType.Contains("pdf"))
                    mediaType = "application/pdf";
                else if (contentType.Contains("png"))
                    mediaType = "image/png";
                else
                    mediaType = "image/jpeg";

                // Create the request body
                var requestBody = new
                {
                    model = "claude-3-sonnet-20240229",
                    max_tokens = 2000,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new
                                {
                                    type = contentType.Contains("pdf") ? "document" : "image",
                                    source = new
                                    {
                                        type = "base64",
                                        media_type = mediaType,
                                        data = base64Data
                                    }
                                },
                                new
                                {
                                    type = "text",
                                    text = GetExtractionPrompt()
                                }
                            }
                        }
                    }
                };

                // Make the HTTP request synchronously with better error handling
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(3); // Increased timeout

                    try
                    {
                        // Set headers
                        client.DefaultRequestHeaders.Add("x-api-key", _claudeApiKey);
                        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                        // Serialize request
                        var jsonContent = JsonConvert.SerializeObject(requestBody);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        // Make synchronous request
                        var response = client.PostAsync("https://api.anthropic.com/v1/messages", content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = response.Content.ReadAsStringAsync().Result;
                            
                            if (string.IsNullOrEmpty(responseContent))
                            {
                                throw new Exception("Empty response from API");
                            }

                            var responseJson = JObject.Parse(responseContent);

                            // Check if the response has the expected structure
                            if (responseJson["content"] == null || !responseJson["content"].HasValues)
                            {
                                throw new Exception("Invalid API response structure");
                            }

                            var messageContent = responseJson["content"][0]["text"].ToString();

                            // Extract JSON from the response
                            var startIndex = messageContent.IndexOf('{');
                            var lastIndex = messageContent.LastIndexOf('}');

                            if (startIndex >= 0 && lastIndex > startIndex)
                            {
                                var jsonString = messageContent.Substring(startIndex, lastIndex - startIndex + 1);
                                return JObject.Parse(jsonString);
                            }
                            else
                            {
                                throw new Exception("Could not find JSON in API response");
                            }
                        }
                        else
                        {
                            var errorContent = response.Content.ReadAsStringAsync().Result;
                            
                            // Handle specific error cases
                            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                throw new Exception("Invalid API key. Please check configuration.");
                            }
                            else if (response.StatusCode == System.Net.HttpStatusCode.PaymentRequired)
                            {
                                throw new Exception("API quota exceeded. Please check your Claude API account.");
                            }
                            else
                            {
                                throw new Exception($"API Error ({response.StatusCode}): {errorContent}");
                            }
                        }
                    }
                    catch (TaskCanceledException ex)
                    {
                        throw new Exception("Request timed out. Please try again with a smaller file.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log more detailed error information
                System.Diagnostics.Debug.WriteLine($"ProcessInvoiceSync Exception: {ex.ToString()}");
                
                // Re-throw with more context
                if (ex.Message.Contains("API"))
                {
                    throw; // Re-throw API specific errors as-is
                }
                else
                {
                    throw new Exception($"Processing failed: {ex.Message}", ex);
                }
            }
        }
        */

        /* Temporarily disabled - requires Newtonsoft.Json
        private string GetExtractionPrompt()
        {
            return @"Please analyze this invoice/receipt and extract the following information in JSON format:

{
  ""company"": {
    ""name"": ""Company name"",
    ""address"": ""Full address"",
    ""phone"": ""Phone number"",
    ""email"": ""Email if available"",
    ""ice"": ""ICE number or tax ID""
  },
  ""client"": {
    ""name"": ""Client/customer name"",
    ""address"": ""Client address"",
    ""ice"": ""Client ICE number if available""
  },
  ""invoice"": {
    ""number"": ""Invoice number"",
    ""date"": ""Invoice date in YYYY-MM-DD format"",
    ""type"": ""Invoice type""
  },
  ""totals"": {
    ""subtotal"": ""Subtotal amount"",
    ""tax_rate"": ""Tax rate percentage"",
    ""tax_amount"": ""Tax amount"",
    ""total"": ""Final total amount"",
    ""currency"": ""Currency (MAD, EUR, USD, etc.)""
  },
  ""services"": [
    {
      ""code"": ""Service code"",
      ""description"": ""Service description"",
      ""quantity"": ""Quantity"",
      ""unit"": ""Unit type"",
      ""rate"": ""Unit rate"",
      ""amount"": ""Line total""
    }
  ]
}

Focus on accuracy and extract all visible information. If information is not clearly visible, use null for that field.";
        }
        */

        /* Temporarily disabled - all JSON-related methods
        private void DisplayResults(JObject data)
        {
            try
            {
                // Company Information
                var company = data["company"];
                CompanyInfo.Text = FormatCompanyInfo(company);

                // Client Information
                var client = data["client"];
                ClientInfo.Text = FormatClientInfo(client);

                // Invoice Details
                var invoice = data["invoice"];
                InvoiceInfo.Text = FormatInvoiceInfo(invoice);

                // Financial Summary
                var totals = data["totals"];
                FinancialInfo.Text = FormatFinancialInfo(totals);

                // Services (if available)
                var services = data["services"];
                if (services != null && services.HasValues)
                {
                    ServicesInfo.Text = FormatServicesInfo(services, totals?["currency"]?.ToString());
                    ServicesPanel.Visible = true;
                }

                ResultsSection.Visible = true;
            }
            catch (Exception ex)
            {
                ShowError($"Error displaying results: {ex.Message}");
            }
        }

        private string FormatCompanyInfo(JToken company)
        {
            if (company == null) return "No company information available";

            var sb = new StringBuilder();
            sb.Append($"<strong>Name:</strong> {GetValueOrNA(company["name"])}<br>");
            sb.Append($"<strong>Address:</strong> {GetValueOrNA(company["address"])}<br>");
            sb.Append($"<strong>Phone:</strong> {GetValueOrNA(company["phone"])}<br>");
            sb.Append($"<strong>Email:</strong> {GetValueOrNA(company["email"])}<br>");
            sb.Append($"<strong>ICE:</strong> {GetValueOrNA(company["ice"])}");

            return sb.ToString();
        }

        private string FormatClientInfo(JToken client)
        {
            if (client == null) return "No client information available";

            var sb = new StringBuilder();
            sb.Append($"<strong>Name:</strong> {GetValueOrNA(client["name"])}<br>");
            sb.Append($"<strong>Address:</strong> {GetValueOrNA(client["address"])}<br>");
            sb.Append($"<strong>ICE:</strong> {GetValueOrNA(client["ice"])}");

            return sb.ToString();
        }

        private string FormatInvoiceInfo(JToken invoice)
        {
            if (invoice == null) return "No invoice information available";

            var sb = new StringBuilder();
            sb.Append($"<strong>Number:</strong> {GetValueOrNA(invoice["number"])}<br>");
            sb.Append($"<strong>Date:</strong> {GetValueOrNA(invoice["date"])}<br>");
            sb.Append($"<strong>Type:</strong> {GetValueOrNA(invoice["type"])}");

            return sb.ToString();
        }

        private string FormatFinancialInfo(JToken totals)
        {
            if (totals == null) return "No financial information available";

            var currency = GetValueOrNA(totals["currency"]);
            var sb = new StringBuilder();
            sb.Append($"<strong>Subtotal:</strong> {currency} {GetValueOrNA(totals["subtotal"])}<br>");
            sb.Append($"<strong>Tax:</strong> {currency} {GetValueOrNA(totals["tax_amount"])}<br>");
            sb.Append($"<strong>Total:</strong> {currency} {GetValueOrNA(totals["total"])}");

            return sb.ToString();
        }

        private string FormatServicesInfo(JToken services, string currency)
        {
            if (services == null || !services.HasValues) return "No services available";

            var sb = new StringBuilder();
            foreach (var service in services)
            {
                sb.Append("<div style=\"margin-bottom: 15px; padding-bottom: 15px; border-bottom: 1px solid #dee2e6;\">");
                sb.Append($"<strong>{GetValueOrNA(service["description"])}</strong><br>");
                sb.Append($"<small>Code: {GetValueOrNA(service["code"])} | Qty: {GetValueOrNA(service["quantity"])}</small><br>");
                sb.Append($"Rate: {currency} {GetValueOrNA(service["rate"])} = {currency} {GetValueOrNA(service["amount"])}");
                sb.Append("</div>");
            }

            return sb.ToString();
        }

        private string GetValueOrNA(JToken token)
        {
            if (token == null || string.IsNullOrWhiteSpace(token.ToString()) || token.ToString().ToLower() == "null")
                return "N/A";
            return HttpUtility.HtmlEncode(token.ToString());
        }
        */

        protected void ExportJsonButton_Click(object sender, EventArgs e)
        {
            ShowError("Export functionality temporarily disabled for testing.");
            // Temporarily disabled to test basic functionality
            /*
            try
            {
                if (!string.IsNullOrEmpty(ExtractedDataHidden.Value))
                {
                    var jsonData = ExtractedDataHidden.Value;
                    var fileName = $"invoice_{DateTime.Now:yyyyMMdd_HHmmss}.json";

                    Response.Clear();
                    Response.ContentType = "application/json";
                    Response.AddHeader("Content-Disposition", $"attachment; filename={fileName}");
                    Response.Write(jsonData);
                    Response.End();
                }
                else
                {
                    ShowError("No data available for export");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Export failed: {ex.Message}");
            }
            */
        }

        protected void ExportCsvButton_Click(object sender, EventArgs e)
        {
            ShowError("Export functionality temporarily disabled for testing.");
            // Temporarily disabled to test basic functionality
            /*
            try
            {
                if (!string.IsNullOrEmpty(ExtractedDataHidden.Value))
                {
                    var data = JObject.Parse(ExtractedDataHidden.Value);
                    var csv = ConvertToCSV(data);
                    var fileName = $"invoice_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    Response.Clear();
                    Response.ContentType = "text/csv";
                    Response.AddHeader("Content-Disposition", $"attachment; filename={fileName}");
                    Response.Write(csv);
                    Response.End();
                }
                else
                {
                    ShowError("No data available for export");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Export failed: {ex.Message}");
            }
            */
        }

        /* Temporarily disabled - requires Newtonsoft.Json
        private string ConvertToCSV(JObject data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Field,Value");

            if (data["invoice"] != null)
            {
                sb.AppendLine($"Invoice Number,{EscapeCsvValue(data["invoice"]["number"]?.ToString())}");
                sb.AppendLine($"Date,{EscapeCsvValue(data["invoice"]["date"]?.ToString())}");
            }

            if (data["company"] != null)
            {
                sb.AppendLine($"Company,{EscapeCsvValue(data["company"]["name"]?.ToString())}");
            }

            if (data["client"] != null)
            {
                sb.AppendLine($"Client,{EscapeCsvValue(data["client"]["name"]?.ToString())}");
            }

            if (data["totals"] != null)
            {
                var currency = data["totals"]["currency"]?.ToString() ?? "";
                sb.AppendLine($"Total,{EscapeCsvValue(data["totals"]["total"]?.ToString())} {currency}");
            }

            return sb.ToString();
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }
        */

        private void ShowError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                MessageLabel.Text = "";
            }
            else
            {
                MessageLabel.Text = $"<div class=\"error\">❌ {HttpUtility.HtmlEncode(message)}</div>";
            }
        }

        private void ShowSuccess(string message)
        {
            MessageLabel.Text = $"<div class=\"success\">✅ {HttpUtility.HtmlEncode(message)}</div>";
        }

        private void ShowMessage(string message)
        {
            MessageLabel.Text = $"<div style=\"padding: 15px; background: #e7f3ff; border: 1px solid #b3d9ff; border-radius: 10px; color: #0066cc;\">ℹ️ {HttpUtility.HtmlEncode(message)}</div>";
        }
        
        // Debug helper method
        private void LogDebugInfo(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Invoice Extractor: {message}");
        }
    }
}