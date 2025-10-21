<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="invoice-info-extractor.aspx.cs" Inherits="APPLICATION_RH.invoice_info_extractor" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Invoice Data Extractor - ERP System</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
            min-height: 100vh;
            padding: 20px;
        }
        
        .container {
            max-width: 1400px;
            margin: 0 auto;
            background: white;
            border-radius: 20px;
            box-shadow: 0 25px 50px rgba(0,0,0,0.15);
            overflow: hidden;
        }
        
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px;
            text-align: center;
            position: relative;
        }
        
        .header h1 {
            font-size: 3em;
            margin-bottom: 15px;
            position: relative;
            z-index: 2;
        }
        
        .header p {
            font-size: 1.2em;
            opacity: 0.9;
            position: relative;
            z-index: 2;
        }
        
        .main-content {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 40px;
            padding: 40px;
        }
        
        .upload-section {
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            border-radius: 20px;
            padding: 40px;
            text-align: center;
            border: 3px dashed #dee2e6;
            transition: all 0.4s ease;
        }
        
        .upload-section h2 {
            font-size: 2em;
            margin-bottom: 20px;
            color: #495057;
        }
        
        .file-upload-container {
            margin: 30px 0;
        }
        
        .custom-file-upload {
            display: inline-block;
            background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
            color: white;
            padding: 18px 40px;
            border-radius: 50px;
            font-size: 1.2em;
            font-weight: bold;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 10px 30px rgba(40,167,69,0.2);
        }
        
        .custom-file-upload:hover {
            transform: translateY(-3px);
            box-shadow: 0 15px 40px rgba(40,167,69,0.3);
        }
        
        .file-upload {
            display: none;
        }
        
        .preview-section {
            text-align: center;
            background: #f8f9fa;
            border-radius: 20px;
            padding: 40px;
        }
        
        .preview-section h2 {
            font-size: 2em;
            margin-bottom: 30px;
            color: #495057;
        }
        
        .preview-image {
            max-width: 100%;
            max-height: 500px;
            border-radius: 15px;
            box-shadow: 0 15px 35px rgba(0,0,0,0.1);
            margin: 20px 0;
        }
        
        .process-btn {
            background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
            color: white;
            padding: 18px 50px;
            border-radius: 50px;
            font-size: 1.3em;
            font-weight: bold;
            border: none;
            cursor: pointer;
            margin: 30px 0;
            transition: all 0.3s ease;
            box-shadow: 0 10px 30px rgba(220,53,69,0.2);
        }
        
        .process-btn:hover {
            transform: translateY(-3px);
            box-shadow: 0 15px 40px rgba(220,53,69,0.3);
        }
        
        .process-btn:disabled {
            background: #6c757d;
            cursor: not-allowed;
            transform: none;
            box-shadow: none;
        }
        
        .results-section {
            grid-column: span 2;
            background: linear-gradient(135deg, #f8f9fa 0%, #ffffff 100%);
            border-radius: 20px;
            padding: 40px;
            margin-top: 30px;
            border: 1px solid #dee2e6;
        }
        
        .results-header {
            text-align: center;
            margin-bottom: 40px;
        }
        
        .results-header h2 {
            font-size: 2.5em;
            color: #495057;
            margin-bottom: 10px;
        }
        
        .results-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
            gap: 30px;
            margin-top: 30px;
        }
        
        .result-card {
            background: white;
            padding: 30px;
            border-radius: 15px;
            box-shadow: 0 10px 25px rgba(0,0,0,0.1);
            border-left: 5px solid #667eea;
        }
        
        .result-card h3 {
            color: #667eea;
            margin-bottom: 20px;
            font-size: 1.4em;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        
        .result-value {
            font-size: 1.1em;
            color: #495057;
            line-height: 1.8;
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            padding: 20px;
            border-radius: 10px;
            border: 1px solid #dee2e6;
        }
        
        .result-value strong {
            color: #212529;
        }
        
        .error {
            color: #dc3545;
            background: linear-gradient(135deg, #f8d7da 0%, #f5c6cb 100%);
            padding: 20px;
            border-radius: 12px;
            margin: 15px 0;
            border: 1px solid #f5c6cb;
            font-weight: 500;
        }
        
        .success {
            color: #155724;
            background: linear-gradient(135deg, #d4edda 0%, #c3e6cb 100%);
            padding: 20px;
            border-radius: 12px;
            margin: 15px 0;
            border: 1px solid #c3e6cb;
            font-weight: 500;
        }
        
        .info-message {
            padding: 80px;
            color: #6c757d;
            font-size: 1.2em;
        }
        
        .export-buttons {
            margin-top: 30px;
            text-align: center;
        }
        
        .export-btn {
            background: linear-gradient(135deg, #17a2b8 0%, #138496 100%);
            color: white;
            padding: 12px 30px;
            border-radius: 25px;
            border: none;
            margin: 0 10px;
            cursor: pointer;
            font-weight: 600;
            transition: all 0.3s ease;
        }
        
        .export-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(23,162,184,0.3);
        }
        
        @media (max-width: 768px) {
            .main-content {
                grid-template-columns: 1fr;
                gap: 30px;
                padding: 30px 20px;
            }
            
            .results-section {
                grid-column: span 1;
            }
            
            .header h1 {
                font-size: 2.2em;
            }
            
            .results-grid {
                grid-template-columns: 1fr;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="header">
                <h1>🧾 Invoice Data Extractor</h1>
                <p>Intelligent invoice processing for your ERP system</p>
            </div>
            
            <div class="main-content">
                <div class="upload-section">
                    <h2>📁 Upload Invoice</h2>
                    <p style="margin-bottom: 30px; color: #6c757d;">Select an image (JPG, PNG) or PDF file</p>
                    
                    <div class="file-upload-container">
                        <label for="<%= FileUploadControl.ClientID %>" class="custom-file-upload">
                            📤 Choose Invoice File
                        </label>
                        <asp:FileUpload ID="FileUploadControl" runat="server" CssClass="file-upload" 
                            accept=".jpg,.jpeg,.png,.pdf" onchange="showFileName(this)" />
                    </div>
                    
                    <div id="fileInfo">
                        <asp:Label ID="FileInfoLabel" runat="server" />
                    </div>
                    
                    <asp:Button ID="ProcessButton" runat="server" Text="🔍 Extract Invoice Data" 
                        CssClass="process-btn" OnClick="ProcessButton_Click" Enabled="false" 
                        OnClientClick="return preventMultipleSubmit();" UseSubmitBehavior="true" />
                    
                    <div>
                        <asp:Label ID="MessageLabel" runat="server" />
                    </div>
                </div>
                
                <div class="preview-section">
                    <h2>👁️ File Preview</h2>
                    <div id="imagePreview">
                        <asp:Panel ID="PreviewPanel" runat="server">
                            <div class="info-message">
                                📄 Your invoice preview will appear here
                            </div>
                        </asp:Panel>
                        <asp:Image ID="PreviewImage" runat="server" CssClass="preview-image" Visible="false" />
                    </div>
                </div>
            </div>
            
            <asp:Panel ID="ResultsSection" runat="server" CssClass="results-section" Visible="false">
                <div class="results-header">
                    <h2>📊 Extracted Invoice Data</h2>
                    <p style="color: #6c757d; font-size: 1.1em;">Processed invoice information</p>
                </div>
                
                <div class="results-grid">
                    <asp:Panel ID="CompanyPanel" runat="server" CssClass="result-card">
                        <h3>🏢 Company Information</h3>
                        <div class="result-value">
                            <asp:Literal ID="CompanyInfo" runat="server" />
                        </div>
                    </asp:Panel>
                    
                    <asp:Panel ID="ClientPanel" runat="server" CssClass="result-card">
                        <h3>👤 Client Information</h3>
                        <div class="result-value">
                            <asp:Literal ID="ClientInfo" runat="server" />
                        </div>
                    </asp:Panel>
                    
                    <asp:Panel ID="InvoicePanel" runat="server" CssClass="result-card">
                        <h3>🧾 Invoice Details</h3>
                        <div class="result-value">
                            <asp:Literal ID="InvoiceInfo" runat="server" />
                        </div>
                    </asp:Panel>
                    
                    <asp:Panel ID="FinancialPanel" runat="server" CssClass="result-card">
                        <h3>💰 Financial Summary</h3>
                        <div class="result-value">
                            <asp:Literal ID="FinancialInfo" runat="server" />
                        </div>
                    </asp:Panel>
                    
                    <asp:Panel ID="ServicesPanel" runat="server" CssClass="result-card" Visible="false">
                        <h3>📝 Services/Items</h3>
                        <div class="result-value">
                            <asp:Literal ID="ServicesInfo" runat="server" />
                        </div>
                    </asp:Panel>
                    
                    <asp:Panel ID="ShippingPanel" runat="server" CssClass="result-card" Visible="false">
                        <h3>🚢 Shipping Information</h3>
                        <div class="result-value">
                            <asp:Literal ID="ShippingInfo" runat="server" />
                        </div>
                    </asp:Panel>
                </div>
                
                <div class="export-buttons">
                    <asp:Button ID="ExportJsonButton" runat="server" Text="📄 Export JSON" 
                        CssClass="export-btn" OnClick="ExportJsonButton_Click" />
                    <asp:Button ID="ExportCsvButton" runat="server" Text="📊 Export CSV" 
                        CssClass="export-btn" OnClick="ExportCsvButton_Click" />
                </div>
                
                <!-- Hidden field to store extracted data for export -->
                <asp:HiddenField ID="ExtractedDataHidden" runat="server" />
            </asp:Panel>
        </div>
    </form>
    
    <script type="text/javascript">
        function showFileName(input) {
            try {
                var fileInfo = document.getElementById('<%= FileInfoLabel.ClientID %>');
                var processBtn = document.getElementById('<%= ProcessButton.ClientID %>');
                
                if (input.files && input.files[0]) {
                    var file = input.files[0];
                    var size = (file.size / 1024 / 1024).toFixed(2);
                    
                    fileInfo.innerHTML = '<div class="success"><strong>✅ File Ready:</strong> ' + 
                        file.name + '<br><strong>📏 Size:</strong> ' + size + 
                        ' MB<br><strong>📄 Type:</strong> ' + file.type + '</div>';
                    
                    processBtn.disabled = false;
                    processBtn.className = processBtn.className.replace(' disabled', '');
                    
                    // Show preview for images
                    if (file.type.startsWith('image/')) {
                        var reader = new FileReader();
                        reader.onload = function(e) {
                            var preview = document.getElementById('<%= PreviewImage.ClientID %>');
                            var panel = document.getElementById('<%= PreviewPanel.ClientID %>');

                            if (preview && panel) {
                                preview.src = e.target.result;
                                preview.style.display = 'block';
                                panel.style.display = 'none';
                            }
                        };
                        reader.readAsDataURL(file);
                    }
                }
            } catch (error) {
                console.error('Error in showFileName:', error);
            }
        }
        
        function preventMultipleSubmit() {
            var processBtn = document.getElementById('<%= ProcessButton.ClientID %>');
            if (processBtn) {
                processBtn.disabled = true;
                processBtn.value = '⏳ Processing...';
                setTimeout(function() {
                    if (processBtn) {
                        processBtn.disabled = false;
                        processBtn.value = '🔍 Extract Invoice Data';
                    }
                }, 30000); // Re-enable after 30 seconds as fallback
            }
            return true;
        }
    </script>
</body>
</html>