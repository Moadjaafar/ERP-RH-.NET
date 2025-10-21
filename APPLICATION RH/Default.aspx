<%@ Page Title="Dashboard RH - Analyse des Pointages" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="APPLICATION_RH._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Chart.js -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/3.9.1/chart.min.js"></script>
    
    <!-- Custom Dashboard Styles -->
    <style>
        .dashboard-card {
            background: white;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            border-left: 4px solid #007bff;
        }
        
        .dashboard-card.success { border-left-color: #28a745; }
        .dashboard-card.warning { border-left-color: #ffc107; }
        .dashboard-card.danger { border-left-color: #dc3545; }
        .dashboard-card.info { border-left-color: #17a2b8; }
        
        .stat-number {
            font-size: 2.5rem;
            font-weight: bold;
            color: #333;
            margin: 0;
        }
        
        .stat-label {
            color: #666;
            font-size: 0.9rem;
            text-transform: uppercase;
            margin: 0;
        }
        
        .chart-container {
            position: relative;
            height: 400px;
            margin: 20px 0;
        }
        
        .chart-container.small {
            height: 300px;
        }
        
        .dashboard-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            border-radius: 8px;
            margin-bottom: 30px;
        }
        
        .filter-buttons {
            background: rgba(255,255,255,0.1);
            border-radius: 6px;
            padding: 10px;
            margin-top: 15px;
        }
        
        .filter-btn {
            background: rgba(255,255,255,0.2);
            border: 1px solid rgba(255,255,255,0.3);
            color: white;
            padding: 8px 16px;
            margin: 0 5px 5px 0;
            border-radius: 20px;
            cursor: pointer;
            font-size: 13px;
            transition: all 0.3s ease;
        }
        
        .filter-btn:hover {
            background: rgba(255,255,255,0.3);
            transform: translateY(-1px);
        }
        
        .filter-btn.active {
            background: rgba(255,255,255,0.9);
            color: #667eea;
            font-weight: bold;
        }
        
        .period-indicator {
            background: rgba(40, 167, 69, 0.1);
            color: #28a745;
            padding: 5px 12px;
            border-radius: 15px;
            font-size: 12px;
            font-weight: bold;
            display: inline-block;
            margin-left: 10px;
        }
    </style>

    <!-- Dashboard Header -->
    <div class="dashboard-header">
        <div class="row">
            <div class="col-md-8">
                <h1>📊 Dashboard des Pointages</h1>
                <p class="mb-0">Analyse en temps réel de l'activité des ports</p>
                <span class="period-indicator" id="periodIndicator">Derniers 30 jours</span>
            </div>
            <div class="col-md-4 text-right">
                <p class="mb-0">
                    <small>Dernière mise à jour: <span id="lastUpdate"></span></small>
                </p>
            </div>
        </div>
        
        <!-- Filter Buttons -->
        <div class="filter-buttons">
            <div class="row">
                <div class="col-md-12">
                    <small><strong>📅 Période d'analyse:</strong></small><br>
                    <asp:Button ID="btn7Days" runat="server" Text="7 jours" CommandArgument="7" 
                               OnClick="FilterButton_Click" CssClass="filter-btn" />
                    <asp:Button ID="btn30Days" runat="server" Text="30 jours" CommandArgument="30" 
                               OnClick="FilterButton_Click" CssClass="filter-btn active" />
                    <asp:Button ID="btn90Days" runat="server" Text="3 mois" CommandArgument="90" 
                               OnClick="FilterButton_Click" CssClass="filter-btn" />
                    <asp:Button ID="btn180Days" runat="server" Text="6 mois" CommandArgument="180" 
                               OnClick="FilterButton_Click" CssClass="filter-btn" />
                    <asp:Button ID="btn365Days" runat="server" Text="1 année" CommandArgument="365" 
                               OnClick="FilterButton_Click" CssClass="filter-btn" />
                </div>
            </div>
        </div>
    </div>

    <!-- Summary Statistics Row -->
    <div class="row" id="summaryCards">
        <div class="col-md-3">
            <div class="dashboard-card success">
                <h2 class="stat-number" id="todayPunches">-</h2>
                <p class="stat-label">Pointages Aujourd'hui</p>
            </div>
        </div>
        <div class="col-md-3">
            <div class="dashboard-card info">
                <h2 class="stat-number" id="activeEmployees">-</h2>
                <p class="stat-label">Employés Actifs Aujourd'hui</p>
            </div>
        </div>
        <div class="col-md-3">
            <div class="dashboard-card warning">
                <h2 class="stat-number" id="activePorts">-</h2>
                <p class="stat-label">Ports Actifs</p>
            </div>
        </div>
        <div class="col-md-3">
            <div class="dashboard-card danger">
                <h2 class="stat-number" id="monthPunches">-</h2>
                <p class="stat-label">Total Période</p>
            </div>
        </div>
    </div>

    <!-- Charts Row 1 -->
    <div class="row">
        <div class="col-md-8">
            <div class="dashboard-card">
                <h4>📈 Évolution des Pointages</h4>
                <div class="chart-container">
                    <canvas id="dailyPunchesChart"></canvas>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="dashboard-card">
                <h4>🔄 Distribution par Type</h4>
                <div class="chart-container small">
                    <canvas id="punchTypeChart"></canvas>
                </div>
            </div>
        </div>
    </div>

    <!-- Charts Row 2 -->
    <div class="row">
        <div class="col-md-6">
            <div class="dashboard-card">
                <h4>🏭 Activité par Port</h4>
                <div class="chart-container">
                    <canvas id="portDistributionChart"></canvas>
                </div>
            </div>
        </div>
        <div class="col-md-6">
            <div class="dashboard-card">
                <h4>⏰ Distribution Horaire</h4>
                <div class="chart-container">
                    <canvas id="hourlyDistributionChart"></canvas>
                </div>
            </div>
        </div>
    </div>

    <!-- Charts Row 3 -->
    <div class="row">
        <div class="col-md-8">
            <div class="dashboard-card">
                <h4>📊 Tendance Hebdomadaire</h4>
                <div class="chart-container">
                    <canvas id="weeklyTrendChart"></canvas>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="dashboard-card">
                <h4>👥 Top Employés</h4>
                <div style="max-height: 400px; overflow-y: auto;">
                    <table class="table table-sm table-striped" id="topEmployeesTable">
                        <thead>
                            <tr>
                                <th>Employé</th>
                                <th>Pointages</th>
                                <th>Ports</th>
                            </tr>
                        </thead>
                        <tbody>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <script>
        // Global variables
        let chartInstances = {};
        let currentPeriod = 30;

        // Initialize dashboard when page loads
        document.addEventListener('DOMContentLoaded', function () {
            updateLastUpdateTime();

            // Wait for data to be available
            setTimeout(function () {
                if (typeof dashboardData !== 'undefined' && window.dashboardDataLoaded) {
                    initializeDashboard();
                }
            }, 100);
        });

        // Initialize dashboard
        function initializeDashboard() {
            if (typeof dashboardData === 'undefined') {
                console.warn('Dashboard data not available');
                return;
            }

            // Update period indicator
            updatePeriodIndicator();

            // Update active button
            updateActiveButton();

            // Load summary statistics
            loadSummaryStats();

            // Destroy existing charts
            destroyCharts();

            // Initialize all charts
            initializeDailyPunchesChart();
            initializePortDistributionChart();
            initializeHourlyDistributionChart();
            initializePunchTypeChart();
            initializeWeeklyTrendChart();
            loadTopEmployees();
        }

        function updatePeriodIndicator() {
            const period = dashboardData.period || 30;
            currentPeriod = period;
            let text = "";

            switch (period) {
                case 7: text = "Derniers 7 jours"; break;
                case 30: text = "Derniers 30 jours"; break;
                case 90: text = "Derniers 3 mois"; break;
                case 180: text = "Derniers 6 mois"; break;
                case 365: text = "Dernière année"; break;
                default: text = `Derniers ${period} jours`;
            }

            document.getElementById('periodIndicator').textContent = text;
        }

        function updateActiveButton() {
            // Remove active class from all buttons
            document.querySelectorAll('.filter-btn').forEach(btn => {
                btn.classList.remove('active');
            });

            // Add active class to current period button
            const period = dashboardData.period || 30;
            document.querySelectorAll('.filter-btn').forEach(btn => {
                if (btn.textContent.includes('7 jours') && period == 7) btn.classList.add('active');
                else if (btn.textContent.includes('30 jours') && period == 30) btn.classList.add('active');
                else if (btn.textContent.includes('3 mois') && period == 90) btn.classList.add('active');
                else if (btn.textContent.includes('6 mois') && period == 180) btn.classList.add('active');
                else if (btn.textContent.includes('1 année') && period == 365) btn.classList.add('active');
            });
        }

        function destroyCharts() {
            Object.keys(chartInstances).forEach(key => {
                if (chartInstances[key]) {
                    chartInstances[key].destroy();
                    delete chartInstances[key];
                }
            });
        }

        function loadSummaryStats() {
            const statsData = dashboardData.summaryStats;
            if (statsData) {
                document.getElementById('todayPunches').textContent = statsData.todayPunches || 0;
                document.getElementById('activeEmployees').textContent = statsData.todayActiveEmployees || 0;
                document.getElementById('activePorts').textContent = statsData.activePorts || 0;
                document.getElementById('monthPunches').textContent = statsData.monthPunches || 0;
            }
        }

        function initializeDailyPunchesChart() {
            const data = dashboardData.dailyPunches || [];
            if (data.length === 0) return;

            const ctx = document.getElementById('dailyPunchesChart');
            if (!ctx) return;

            chartInstances.dailyPunches = new Chart(ctx.getContext('2d'), {
                type: 'line',
                data: {
                    labels: data.map(d => new Date(d.date).toLocaleDateString('fr-FR')),
                    datasets: [{
                        label: 'Pointages par jour',
                        data: data.map(d => d.count),
                        borderColor: '#667eea',
                        backgroundColor: 'rgba(102, 126, 234, 0.1)',
                        borderWidth: 3,
                        fill: true,
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: { precision: 0 }
                        }
                    },
                    plugins: {
                        legend: { display: false }
                    }
                }
            });
        }

        function initializePortDistributionChart() {
            const data = dashboardData.portDistribution || [];
            if (data.length === 0) return;

            const ctx = document.getElementById('portDistributionChart');
            if (!ctx) return;

            chartInstances.portDistribution = new Chart(ctx.getContext('2d'), {
                type: 'bar',
                data: {
                    labels: data.map(d => d.port),
                    datasets: [{
                        label: 'Nombre de Pointages',
                        data: data.map(d => d.punchCount),
                        backgroundColor: [
                            '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0',
                            '#9966FF', '#FF9F40', '#FF6384', '#C9CBCF'
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    indexAxis: 'y',
                    scales: { x: { beginAtZero: true } }
                }
            });
        }

        function initializeHourlyDistributionChart() {
            const data = dashboardData.hourlyDistribution || [];
            if (data.length === 0) return;

            const ctx = document.getElementById('hourlyDistributionChart');
            if (!ctx) return;

            chartInstances.hourlyDistribution = new Chart(ctx.getContext('2d'), {
                type: 'bar',
                data: {
                    labels: data.map(d => d.hour + 'h'),
                    datasets: [{
                        label: 'Pointages par heure',
                        data: data.map(d => d.count),
                        backgroundColor: '#36A2EB',
                        borderRadius: 4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: { y: { beginAtZero: true } }
                }
            });
        }

        function initializePunchTypeChart() {
            const data = dashboardData.punchTypeDistribution || [];
            if (data.length === 0) return;

            const ctx = document.getElementById('punchTypeChart');
            if (!ctx) return;

            chartInstances.punchType = new Chart(ctx.getContext('2d'), {
                type: 'doughnut',
                data: {
                    labels: data.map(d => d.type),
                    datasets: [{
                        data: data.map(d => d.count),
                        backgroundColor: ['#28a745', '#dc3545', '#ffc107', '#17a2b8']
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { position: 'bottom' }
                    }
                }
            });
        }

        function initializeWeeklyTrendChart() {
            const data = dashboardData.weeklyTrend || [];
            if (data.length === 0) return;

            const ctx = document.getElementById('weeklyTrendChart');
            if (!ctx) return;

            chartInstances.weeklyTrend = new Chart(ctx.getContext('2d'), {
                type: 'line',
                data: {
                    labels: data.map(d => d.week),
                    datasets: [{
                        label: 'Pointages',
                        data: data.map(d => d.punchCount),
                        borderColor: '#667eea',
                        backgroundColor: 'rgba(102, 126, 234, 0.1)',
                        yAxisID: 'y'
                    }, {
                        label: 'Employés Actifs',
                        data: data.map(d => d.activeEmployees),
                        borderColor: '#28a745',
                        backgroundColor: 'rgba(40, 167, 69, 0.1)',
                        yAxisID: 'y1'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: { type: 'linear', display: true, position: 'left' },
                        y1: { type: 'linear', display: true, position: 'right', grid: { drawOnChartArea: false } }
                    }
                }
            });
        }

        function loadTopEmployees() {
            const data = dashboardData.employeeActivity || [];
            const tbody = document.querySelector('#topEmployeesTable tbody');
            if (!tbody) return;

            tbody.innerHTML = '';

            data.forEach((emp) => {
                const row = tbody.insertRow();
                row.innerHTML = `
                    <td>
                        <small><strong>${emp.employeeName}</strong><br>
                        <span class="text-muted">${emp.matricule}</span></small>
                    </td>
                    <td><span class="badge badge-primary">${emp.punchCount}</span></td>
                    <td><span class="badge badge-info">${emp.portsWorked}</span></td>
                `;
            });
        }

        function updateLastUpdateTime() {
            const element = document.getElementById('lastUpdate');
            if (element) {
                element.textContent = new Date().toLocaleString('fr-FR');
            }
        }
    </script>
</asp:Content>


