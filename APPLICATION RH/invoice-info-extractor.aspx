<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="invoice-info-extractor.aspx.cs" Inherits="APPLICATION_RH.invoice_info_extractor" Async="true" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Excel Dashboard Generator - AI Powered</title>
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
            max-width: 1200px;
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
        }
        
        .header h1 {
            font-size: 3em;
            margin-bottom: 15px;
        }
        
        .header p {
            font-size: 1.2em;
            opacity: 0.9;
        }
        
        .upload-section {
            padding: 60px 40px;
            text-align: center;
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
        }
        
        .upload-area {
            border: 3px dashed #007bff;
            border-radius: 20px;
            padding: 60px;
            background: white;
            transition: all 0.4s ease;
            cursor: pointer;
        }
        
        .upload-area:hover {
            border-color: #0056b3;
            transform: translateY(-5px);
            box-shadow: 0 15px 40px rgba(0,123,255,0.2);
        }
        
        .custom-file-upload {
            display: inline-block;
            background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
            color: white;
            padding: 20px 50px;
            border-radius: 50px;
            font-size: 1.3em;
            font-weight: bold;
            cursor: pointer;
            transition: all 0.3s ease;
            margin: 20px 0;
        }
        
        .custom-file-upload:hover {
            transform: translateY(-3px);
            box-shadow: 0 15px 40px rgba(40,167,69,0.3);
        }
        
        .file-upload {
            display: none;
        }
        
        .generate-btn {
            background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
            color: white;
            padding: 20px 60px;
            border-radius: 50px;
            font-size: 1.4em;
            font-weight: bold;
            border: none;
            cursor: pointer;
            margin: 30px 0;
            transition: all 0.3s ease;
            box-shadow: 0 10px 30px rgba(220,53,69,0.2);
        }
        
        .generate-btn:hover {
            transform: translateY(-3px);
            box-shadow: 0 15px 40px rgba(220,53,69,0.3);
        }
        
        .generate-btn:disabled {
            background: #6c757d;
            cursor: not-allowed;
            transform: none;
            box-shadow: none;
        }
        
        .loading {
            display: none;
            text-align: center;
            padding: 40px;
            background: rgba(255,255,255,0.98);
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            z-index: 9999;
        }
        
        .loading-content {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
        }
        
        .spinner {
            width: 80px;
            height: 80px;
            border: 8px solid #f3f3f3;
            border-top: 8px solid #667eea;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            margin: 0 auto 30px;
        }
        
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        
        .message {
            margin: 20px 0;
            padding: 15px;
            border-radius: 10px;
            font-weight: 500;
        }
        
        .error {
            background: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }
        
        .success {
            background: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }
        
        .dashboard-container {
            min-height: 600px;
            padding: 0;
            width: 100%;
        }
        
        .file-info {
            background: #d4edda;
            color: #155724;
            padding: 20px;
            border-radius: 15px;
            margin: 20px 0;
            font-size: 1.1em;
        }
        
        .reset-button {
            position: fixed;
            top: 20px;
            right: 20px;
            background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
            color: white;
            padding: 12px 25px;
            border: none;
            border-radius: 25px;
            font-size: 1em;
            font-weight: bold;
            cursor: pointer;
            z-index: 1000;
            box-shadow: 0 5px 15px rgba(40,167,69,0.3);
            transition: all 0.3s ease;
        }
        
        .reset-button:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 25px rgba(40,167,69,0.4);
        }
        
        .progress-bar {
            width: 100%;
            height: 6px;
            background: #e9ecef;
            border-radius: 3px;
            margin: 20px 0;
            overflow: hidden;
        }
        
        .progress-fill {
            height: 100%;
            background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
            width: 0%;
            transition: width 0.3s ease;
            border-radius: 3px;
        }
        
        @media (max-width: 768px) {
            .container {
                margin: 10px;
                border-radius: 15px;
            }
            
            .header h1 {
                font-size: 2.2em;
            }
            
            .upload-section {
                padding: 40px 20px;
            }
            
            .upload-area {
                padding: 40px 20px;
            }
            
            .reset-button {
                position: relative;
                top: auto;
                right: auto;
                margin: 20px 0;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- Loading Overlay -->
        <div class="loading" id="loadingDiv">
            <div class="loading-content">
                <div class="spinner"></div>
                <h2>Claude AI is Analyzing Your Excel Data...</h2>
                <p>Creating professional dashboard with filters and charts</p>
                <div class="progress-bar">
                    <div class="progress-fill" id="progressFill"></div>
                </div>
                <p id="progressText">Initializing...</p>
            </div>
        </div>

        <div class="container">
            <div class="header">
                <h1>Excel Dashboard Generator</h1>
                <p>Upload Excel → AI analyzes → Professional Dashboard with Filters</p>
            </div>
            
            <!-- Upload Section -->
            <div class="upload-section" id="uploadSection" runat="server">
                <div class="upload-area" onclick="document.getElementById('<%= FileUploadControl.ClientID %>').click();">
                    <h2>Upload Your Excel File</h2>
                    <p style="margin: 20px 0; color: #6c757d;">Claude AI will analyze your data and create a professional dashboard</p>
                    
                    <label for="<%= FileUploadControl.ClientID %>" class="custom-file-upload">
                        Choose Excel File
                    </label>
                    <asp:FileUpload ID="FileUploadControl" runat="server" CssClass="file-upload" 
                        accept=".xlsx,.xls,.csv" onchange="showFileName(this)" />
                    
                    <div id="fileInfo">
                        <asp:Label ID="FileInfoLabel" runat="server" />
                    </div>
                    
                    <br />
                    <asp:Button ID="GenerateButton" runat="server" Text="Generate Dashboard with AI" 
                        CssClass="generate-btn" OnClick="GenerateButton_Click" Enabled="false" 
                        OnClientClick="showLoading(); return true;" />
                    
                    <div>
                        <asp:Label ID="MessageLabel" runat="server" />
                    </div>
                </div>
            </div>
            
            <!-- Dashboard Display Section -->
            <div class="dashboard-container" id="dashboardContainer" runat="server" visible="false">
                <button type="button" class="reset-button" onclick="resetDashboard()">
                    Upload New File
                </button>
                <asp:Literal ID="DashboardLiteral" runat="server" />
            </div>
        </div>
    </form>
    
    <script type="text/javascript">
        let progressInterval;

        function showLoading() {
            document.getElementById('loadingDiv').style.display = 'block';
            startProgressAnimation();
        }

        function hideLoading() {
            document.getElementById('loadingDiv').style.display = 'none';
            stopProgressAnimation();
        }

        function startProgressAnimation() {
            const progressFill = document.getElementById('progressFill');
            const progressText = document.getElementById('progressText');
            let progress = 0;

            const steps = [
                'Reading Excel file...',
                'Analyzing data structure...',
                'Detecting column types...',
                'Sending to Claude AI...',
                'Generating dashboard...',
                'Creating charts and filters...',
                'Finalizing interface...'
            ];

            progressInterval = setInterval(function () {
                progress += Math.random() * 15;
                if (progress > 90) progress = 90;

                progressFill.style.width = progress + '%';

                const stepIndex = Math.floor((progress / 100) * steps.length);
                if (stepIndex < steps.length) {
                    progressText.textContent = steps[stepIndex];
                }
            }, 1000);
        }

        function stopProgressAnimation() {
            if (progressInterval) {
                clearInterval(progressInterval);
                progressInterval = null;
            }
        }

        function showFileName(input) {
            const fileInfo = document.getElementById('<%= FileInfoLabel.ClientID %>');
            const generateBtn = document.getElementById('<%= GenerateButton.ClientID %>');

            if (input.files && input.files[0]) {
                const file = input.files[0];
                const sizeMB = (file.size / 1024 / 1024).toFixed(2);

                // Validate file size (max 10MB)
                if (file.size > 10 * 1024 * 1024) {
                    fileInfo.innerHTML = '<div class="message error">File too large! Please select a file smaller than 10MB.</div>';
                    generateBtn.disabled = true;
                    return;
                }

                // Validate file type
                const allowedTypes = ['.xlsx', '.xls', '.csv'];
                const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
                if (!allowedTypes.includes(fileExtension)) {
                    fileInfo.innerHTML = '<div class="message error">Invalid file type! Please select .xlsx, .xls, or .csv files only.</div>';
                    generateBtn.disabled = true;
                    return;
                }

                fileInfo.innerHTML = '<div class="file-info">' +
                    '<strong>' + file.name + '</strong><br>' +
                    'Size: ' + sizeMB + ' MB<br>' +
                    'Type: ' + file.type +
                    '</div>';

                generateBtn.disabled = false;
            } else {
                fileInfo.innerHTML = '';
                generateBtn.disabled = true;
            }
        }

        function resetDashboard() {
            if (confirm('Are you sure you want to upload a new file? This will clear the current dashboard.')) {
                window.location.href = window.location.href.split('?')[0];
            }
        }

        // Hide loading on page load if dashboard is visible
        window.onload = function () {
            const dashboard = document.getElementById('<%= dashboardContainer.ClientID %>');
            if (dashboard && dashboard.style.display !== 'none') {
                hideLoading();
            }
        };

        // Auto-hide messages after 5 seconds
        setTimeout(function () {
            const messages = document.querySelectorAll('.message');
            messages.forEach(function (msg) {
                if (msg.classList.contains('success')) {
                    msg.style.opacity = '0';
                    setTimeout(function () { msg.style.display = 'none'; }, 500);
                }
            });
        }, 5000);
    </script>
</body>
</html>


