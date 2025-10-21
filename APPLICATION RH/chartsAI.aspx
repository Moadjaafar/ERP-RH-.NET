<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="chartsAI.aspx.cs" Inherits="APPLICATION_RH.chartsAI" Async="true" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Excel to Charts - AI Powered</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.0/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet" />
    <style>
        .chart-container {
            position: relative;
            height: 400px;
            margin: 20px 0;
        }
        .data-preview {
            max-height: 300px;
            overflow-y: auto;
        }
        .chart-suggestions {
            background: #f8f9fa;
            border-radius: 8px;
            padding: 15px;
            margin: 15px 0;
        }
        .suggestion-item {
            cursor: pointer;
            padding: 8px 12px;
            margin: 5px;
            background: white;
            border: 1px solid #dee2e6;
            border-radius: 5px;
            display: inline-block;
        }
        .suggestion-item:hover {
            background: #e9ecef;
        }
        .loading {
            display: none;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container-fluid py-4">
            <div class="row">
                <div class="col-12">
                    <h1 class="text-center mb-4">
                        <i class="fas fa-chart-line text-primary"></i>
                        Excel to Charts - AI Powered
                    </h1>
                </div>
            </div>

            <!-- File Upload Section -->
            <div class="row mb-4">
                <div class="col-md-8 offset-md-2">
                    <div class="card">
                        <div class="card-header">
                            <h5><i class="fas fa-upload"></i> Upload Excel File</h5>
                        </div>
                        <div class="card-body">
                            <div class="mb-3">
                                <asp:FileUpload ID="fileUpload" runat="server" CssClass="form-control" accept=".xlsx,.xls" />
                                <small class="form-text text-muted">Supported: .xlsx, .xls files</small>
                            </div>
                            <asp:Button ID="btnUpload" runat="server" Text="Upload and Analyze" 
                                CssClass="btn btn-primary" OnClick="btnUpload_Click" />
                            <asp:Button ID="btnGetAISuggestions" runat="server" Text="Get AI Suggestions" 
                                CssClass="btn btn-success ms-2" OnClick="btnGetAISuggestions_Click" Visible="false" />
                        </div>
                    </div>
                </div>
            </div>

            <!-- Loading Indicator -->
            <div class="row" id="loadingSection" runat="server" visible="false">
                <div class="col-12 text-center">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <p class="mt-2">Processing your data with Claude AI...</p>
                </div>
            </div>

            <!-- Error Messages -->
            <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
                <asp:Literal ID="ltlError" runat="server"></asp:Literal>
            </asp:Panel>

            <!-- Success Messages -->
            <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success" Visible="false">
                <asp:Literal ID="ltlSuccess" runat="server"></asp:Literal>
            </asp:Panel>

            <!-- Data Section -->
            <div class="row" id="dataSection" runat="server" visible="false">
                <div class="col-md-4">
                    <div class="card">
                        <div class="card-header">
                            <h5><i class="fas fa-table"></i> Data Preview</h5>
                        </div>
                        <div class="card-body">
                            <div class="data-preview">
                                <asp:Literal ID="ltlDataPreview" runat="server"></asp:Literal>
                            </div>
                            
                            <!-- Date Filter Section -->
                            <asp:Panel ID="pnlDateFilter" runat="server" CssClass="mt-3" Visible="false">
                                <h6>Date Filters</h6>
                                <div class="mb-2">
                                    <label class="form-label small">Date Column:</label>
                                    <asp:DropDownList ID="ddlDateColumn" runat="server" CssClass="form-select form-select-sm">
                                        <asp:ListItem Text="Select date column..." Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="row">
                                    <div class="col-6">
                                        <label class="form-label small">From:</label>
                                        <asp:TextBox ID="txtDateFrom" runat="server" TextMode="Date" CssClass="form-control form-control-sm"></asp:TextBox>
                                    </div>
                                    <div class="col-6">
                                        <label class="form-label small">To:</label>
                                        <asp:TextBox ID="txtDateTo" runat="server" TextMode="Date" CssClass="form-control form-control-sm"></asp:TextBox>
                                    </div>
                                </div>
                                <asp:Button ID="btnApplyFilter" runat="server" Text="Apply Filter" 
                                    CssClass="btn btn-sm btn-primary mt-2" OnClick="btnApplyFilter_Click" />
                                <asp:Button ID="btnClearFilter" runat="server" Text="Clear" 
                                    CssClass="btn btn-sm btn-outline-secondary mt-2" OnClick="btnClearFilter_Click" />
                            </asp:Panel>
                        </div>
                    </div>
                </div>
                
                <div class="col-md-8">
                    <!-- AI Suggestions -->
                    <asp:Panel ID="pnlSuggestions" runat="server" CssClass="chart-suggestions" Visible="false">
                        <h6><i class="fas fa-robot"></i> AI Chart Suggestions</h6>
                        <asp:Literal ID="ltlSuggestions" runat="server"></asp:Literal>
                    </asp:Panel>

                    <!-- Manual Chart Creation -->
                    <div class="card mb-3">
                        <div class="card-header">
                            <h5><i class="fas fa-chart-bar"></i> Create Chart</h5>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-4">
                                    <label class="form-label">Chart Type:</label>
                                    <asp:DropDownList ID="ddlChartType" runat="server" CssClass="form-select">
                                        <asp:ListItem Text="Line Chart" Value="line"></asp:ListItem>
                                        <asp:ListItem Text="Bar Chart" Value="bar" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="Pie Chart" Value="pie"></asp:ListItem>
                                        <asp:ListItem Text="Doughnut Chart" Value="doughnut"></asp:ListItem>
                                        <asp:ListItem Text="Scatter Plot" Value="scatter"></asp:ListItem>
                                        <asp:ListItem Text="Area Chart" Value="area"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="col-md-4">
                                    <label class="form-label">X Axis:</label>
                                    <asp:DropDownList ID="ddlXAxis" runat="server" CssClass="form-select">
                                        <asp:ListItem Text="Select column..." Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="col-md-4">
                                    <label class="form-label">Y Axis:</label>
                                    <asp:DropDownList ID="ddlYAxis" runat="server" CssClass="form-select">
                                        <asp:ListItem Text="Select column..." Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="mt-3">
                                <button type="button" class="btn btn-primary" onclick="createChart()">
                                    <i class="fas fa-chart-line"></i> Create Chart
                                </button>
                            </div>
                        </div>
                    </div>

                    <!-- Chart Display -->
                    <div class="card" id="chartCard" style="display: none;">
                        <div class="card-header d-flex justify-content-between align-items-center">
                            <h5><i class="fas fa-chart-area"></i> Generated Chart</h5>
                            <button type="button" class="btn btn-sm btn-outline-primary" onclick="exportChart()">
                                <i class="fas fa-download"></i> Export
                            </button>
                        </div>
                        <div class="card-body">
                            <div class="chart-container">
                                <canvas id="mainChart"></canvas>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Hidden fields to pass data to JavaScript -->
        <asp:HiddenField ID="hdnExcelData" runat="server" />
        <asp:HiddenField ID="hdnDateColumns" runat="server" />
        <asp:HiddenField ID="hdnSuggestionsData" runat="server" />

        <script src="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.0/js/bootstrap.bundle.min.js"></script>
        <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
        
        <script>
            let currentData = null;
            let currentChart = null;
            let dateColumns = [];
            let aiSuggestions = [];

            // Initialize data when page loads
            window.addEventListener('load', function() {
                const dataField = document.getElementById('<%= hdnExcelData.ClientID %>');
                const dateColsField = document.getElementById('<%= hdnDateColumns.ClientID %>');
                const suggestionsField = document.getElementById('<%= hdnSuggestionsData.ClientID %>');
                
                if (dataField.value) {
                    currentData = JSON.parse(dataField.value);
                }
                
                if (dateColsField.value) {
                    dateColumns = JSON.parse(dateColsField.value);
                }
                
                if (suggestionsField.value) {
                    aiSuggestions = JSON.parse(suggestionsField.value);
                    displaySuggestions(aiSuggestions);
                }
            });

            function applySuggestion(index) {
                const suggestion = aiSuggestions[index];
                
                document.getElementById('<%= ddlChartType.ClientID %>').value = suggestion.type;
                document.getElementById('<%= ddlXAxis.ClientID %>').value = suggestion.xAxis;
                document.getElementById('<%= ddlYAxis.ClientID %>').value = suggestion.yAxis || '';
                
                createChart();
            }

            function displaySuggestions(suggestions) {
                const suggestionsPanel = document.getElementById('<%= pnlSuggestions.ClientID %>');
                if (suggestions && suggestions.length > 0) {
                    let html = '<h6><i class="fas fa-robot"></i> AI Chart Suggestions</h6>';
                    suggestions.forEach((suggestion, index) => {
                        html += `
                            <div class="suggestion-item" onclick="applySuggestion(${index})">
                                <strong>${suggestion.title}</strong><br>
                                <small class="text-muted">${suggestion.type.toUpperCase()}: ${suggestion.xAxis} vs ${suggestion.yAxis || 'values'}</small><br>
                                <small>${suggestion.explanation}</small>
                            </div>
                        `;
                    });
                    
                    suggestionsPanel.innerHTML = html;
                    suggestionsPanel.style.display = 'block';
                }
            }

            function createChart() {
                // Check if Chart.js is loaded
                if (typeof Chart === 'undefined') {
                    alert('Chart.js library is not loaded. Please refresh the page and try again.');
                    return;
                }
                
                if (!currentData || currentData.length === 0) {
                    alert('No data available. Please upload an Excel file first.');
                    return;
                }

                const chartType = document.getElementById('<%= ddlChartType.ClientID %>').value;
                const xAxisCol = document.getElementById('<%= ddlXAxis.ClientID %>').value;
                const yAxisCol = document.getElementById('<%= ddlYAxis.ClientID %>').value;
                
                if (!xAxisCol) {
                    alert('Please select X axis column');
                    return;
                }
                
                if (['line', 'bar', 'scatter'].includes(chartType) && !yAxisCol) {
                    alert('Please select Y axis column for this chart type');
                    return;
                }
                
                // Destroy existing chart
                if (currentChart) {
                    currentChart.destroy();
                }
                
                const ctx = document.getElementById('mainChart').getContext('2d');
                const chartData = prepareChartData(chartType, xAxisCol, yAxisCol);
                
                currentChart = new Chart(ctx, {
                    type: chartType === 'area' ? 'line' : chartType,
                    data: chartData,
                    options: getChartOptions(chartType, xAxisCol, yAxisCol)
                });
                
                document.getElementById('chartCard').style.display = 'block';
            }

            function prepareChartData(chartType, xAxisCol, yAxisCol) {
                if (['pie', 'doughnut'].includes(chartType)) {
                    // For pie charts, aggregate data by category
                    const aggregated = {};
                    currentData.forEach(row => {
                        const key = row[xAxisCol];
                        const value = parseFloat(row[yAxisCol]) || 0;
                        aggregated[key] = (aggregated[key] || 0) + value;
                    });
                    
                    return {
                        labels: Object.keys(aggregated),
                        datasets: [{
                            data: Object.values(aggregated),
                            backgroundColor: generateColors(Object.keys(aggregated).length)
                        }]
                    };
                } else {
                    // For other charts
                    const labels = currentData.map(row => row[xAxisCol]);
                    const data = currentData.map(row => parseFloat(row[yAxisCol]) || 0);
                    
                    return {
                        labels: labels,
                        datasets: [{
                            label: yAxisCol,
                            data: data,
                            backgroundColor: 'rgba(13, 110, 253, 0.2)',
                            borderColor: 'rgba(13, 110, 253, 1)',
                            borderWidth: 2,
                            fill: chartType === 'area'
                        }]
                    };
                }
            }

            function getChartOptions(chartType, xAxisCol, yAxisCol) {
                const baseOptions = {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        title: {
                            display: true,
                            text: `${yAxisCol || 'Values'} by ${xAxisCol}`
                        },
                        legend: {
                            display: ['pie', 'doughnut'].includes(chartType)
                        }
                    }
                };
                
                if (!['pie', 'doughnut'].includes(chartType)) {
                    baseOptions.scales = {
                        x: {
                            title: {
                                display: true,
                                text: xAxisCol
                            }
                        },
                        y: {
                            title: {
                                display: true,
                                text: yAxisCol
                            }
                        }
                    };
                }
                
                return baseOptions;
            }

            function generateColors(count) {
                const colors = [
                    'rgba(255, 99, 132, 0.8)',
                    'rgba(54, 162, 235, 0.8)',
                    'rgba(255, 205, 86, 0.8)',
                    'rgba(75, 192, 192, 0.8)',
                    'rgba(153, 102, 255, 0.8)',
                    'rgba(255, 159, 64, 0.8)',
                    'rgba(199, 199, 199, 0.8)',
                    'rgba(83, 102, 255, 0.8)'
                ];
                
                const result = [];
                for (let i = 0; i < count; i++) {
                    result.push(colors[i % colors.length]);
                }
                return result;
            }

            function exportChart() {
                if (!currentChart) return;
                
                const link = document.createElement('a');
                link.download = 'chart.png';
                link.href = currentChart.toBase64Image();
                link.click();
            }
        </script>
    </form>
</body>
</html>