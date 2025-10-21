<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RaportPointagePort.aspx.cs" Inherits="APPLICATION_RH.reporting.RaportPointagePort" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .report-filters { 
            background-color: #f8f9fa; 
            border-radius: 0.375rem; 
            padding: 20px; 
            margin-bottom: 20px;
        }
        .export-section { 
            text-align: right; 
            margin-bottom: 15px; 
        }
        .results-summary { 
            background-color: #e7f3ff; 
            padding: 10px; 
            border-radius: 0.375rem; 
            margin-bottom: 15px; 
        }

        .table-container { 
            border: 1px solid #dee2e6; 
            border-radius: 0.375rem; 
            overflow: hidden;
            max-height: 600px;
            overflow-y: auto;
            position: relative;
        }

        .table-container .table {
            margin-bottom: 0;
        }

        .table-container .table thead th {
            position: sticky;
            top: 0;
            background-color: #f8f9fa;
            border-bottom: 2px solid #dee2e6;
            z-index: 10;
            box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.1);
            font-weight: 600;
            color: #495057;
            text-transform: uppercase;
            font-size: 0.875rem;
            letter-spacing: 0.5px;
            padding: 12px 8px;
        }
        .table-container .table tbody tr:hover {
            background-color: rgba(0, 123, 255, 0.075);
        }
        .table-container .table td {
            padding: 10px 8px;
            vertical-align: middle;
            border-top: 1px solid #dee2e6;
        }
        .btn-export { 
            background-color: #28a745; 
            border-color: #28a745; 
        }
        .btn-export:hover { 
            background-color: #218838; 
            border-color: #1e7e34; 
        }
        .history-btn { 
            min-width: 30px; 
            height: 25px; 
            padding: 2px 6px; 
            font-size: 11px;
        }
        .history-cell { 
            min-width: 200px; 
        }
        .modal-body .table th { 
            background-color: #343a40; 
            color: white; 
            border: none;
        }
        .modal-body .table td { 
            vertical-align: middle;
        }
        
        /* Scrollbar styling for better appearance */
        .table-container::-webkit-scrollbar {
            width: 8px;
        }
        .table-container::-webkit-scrollbar-track {
            background: #f1f1f1;
            border-radius: 4px;
        }
        .table-container::-webkit-scrollbar-thumb {
            background: #888;
            border-radius: 4px;
        }
        .table-container::-webkit-scrollbar-thumb:hover {
            background: #555;
        }

        /* Loading indicator for better UX */
        .table-loading {
            text-align: center;
            padding: 40px;
            color: #6c757d;
        }
        
        /* Enhanced badge styling */
        .badge {
            font-size: 0.75em;
            padding: 0.35em 0.65em;
        }
        
        /* Back to top button styling */
        #backToTopBtn {
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
            transition: all 0.3s ease;
        }
        #backToTopBtn:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 12px rgba(0, 0, 0, 0.3);
        }
        
        /* Smooth scroll behavior */
        .table-container {
            scroll-behavior: smooth;
        }
        
        /* Loading animation */
        @keyframes fadeInOut {
            0%, 100% { opacity: 0.5; }
            50% { opacity: 1; }
        }
        .table-loading .fa-spinner {
            animation: fadeInOut 1.5s infinite;
        }

        /* Search Section Styling */
        .search-section { 
            background-color: #fff3cd; 
            border-radius: 0.375rem; 
            padding: 15px; 
            margin-bottom: 15px;
            border-left: 4px solid #ffc107;
            display: none;
        }

        .search-input-container {
            position: relative;
        }

        .search-input {
            width: 100%;
            padding: 8px 35px 8px 12px;
            border: 1px solid #ced4da;
            border-radius: 0.375rem;
            font-size: 14px;
        }

        .search-input:focus {
            border-color: #ffc107;
            box-shadow: 0 0 0 0.2rem rgba(255, 193, 7, 0.25);
            outline: 0;
        }

        .search-clear-btn {
            position: absolute;
            right: 8px;
            top: 50%;
            transform: translateY(-50%);
            background: none;
            border: none;
            color: #6c757d;
            font-size: 16px;
            cursor: pointer;
            padding: 2px;
        }

        .search-clear-btn:hover {
            color: #495057;
        }

        /* Search highlighting */
        .search-highlight {
            background-color: #fff3cd !important;
            font-weight: bold;
            padding: 1px 2px;
            border-radius: 2px;
        }
        
        /* Responsive adjustments */
        @media (max-width: 768px) {
            .table-container {
                max-height: 400px;
            }
            .table-container .table {
                font-size: 0.875rem;
            }
            .table-container .table th,
            .table-container .table td {
                padding: 8px 6px;
            }
            #backToTopBtn {
                width: 40px;
                height: 40px;
                bottom: 15px;
                right: 15px;
            }
        }

        /* Print styles */
        @media print {
            body * {
                visibility: hidden;
            }
            #printContent, #printContent * {
                visibility: visible;
            }
            #printContent {
                position: absolute;
                left: 0;
                top: 0;
                width: 100%;
            }
            .print-header {
                text-align: center;
                margin-bottom: 20px;
                border-bottom: 2px solid #000;
                padding-bottom: 10px;
            }
            .print-employee-info {
                background-color: #f8f9fa;
                padding: 15px;
                margin-bottom: 20px;
                border: 1px solid #ddd;
            }
            .print-table {
                width: 100%;
                border-collapse: collapse;
                margin-bottom: 20px;
            }
            .print-table th, .print-table td {
                border: 1px solid #000;
                padding: 8px;
                text-align: left;
            }
            .print-table th {
                background-color: #f0f0f0;
                font-weight: bold;
            }
            .print-summary {
                background-color: #e7f3ff;
                padding: 15px;
                border: 1px solid #007bff;
                margin-top: 20px;
            }
            .no-print {
                display: none !important;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        
        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Rapport Pointage par Port</h4>
            </div>
            <div class="text-end">
                <ol class="breadcrumb m-0 py-0">
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlDashboard" runat="server" NavigateUrl="~/Default.aspx">Dashboard</asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlReports" runat="server" NavigateUrl="~/reporting/">Rapports</asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item active">Rapport Pointage Port</li>
                </ol>
            </div>
        </div>

        <!-- Filter Panel -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-filter me-2"></i>Filtres de Recherche
                        </h5>
                    </div>
                    <div class="card-body report-filters">
                        <div class="row">
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblDateFrom" runat="server" Text="Date Début *" CssClass="form-label" AssociatedControlID="txtDateFrom" />
                                    <asp:TextBox ID="txtDateFrom" runat="server" CssClass="form-control" TextMode="Date" />
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblDateTo" runat="server" Text="Date Fin *" CssClass="form-label" AssociatedControlID="txtDateTo" />
                                    <asp:TextBox ID="txtDateTo" runat="server" CssClass="form-control" TextMode="Date" />
                                </div>
                            </div>
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <asp:Label ID="lblPort" runat="server" Text="Port" CssClass="form-label" AssociatedControlID="ddlPort" />
                                    <asp:DropDownList ID="ddlPort" runat="server" CssClass="form-select">
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-2">
                                <div class="mb-3">
                                    <label class="form-label">&nbsp;</label>
                                    <div class="d-grid">
                                        <asp:Button ID="btnSearch" runat="server" Text="Rechercher" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Quick Date Buttons -->
                        <div class="row">
                            <div class="col-12">
                                <div class="d-flex justify-content-between align-items-center flex-wrap">
                                    <div class="btn-group" role="group">
                                        <asp:Button ID="btnToday" runat="server" Text="Aujourd'hui" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnToday_Click" CausesValidation="false" />
                                        <asp:Button ID="btnYesterday" runat="server" Text="Hier" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnYesterday_Click" CausesValidation="false" />
                                        <asp:Button ID="btnThisWeek" runat="server" Text="Cette Semaine" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnThisWeek_Click" CausesValidation="false" />
                                        <asp:Button ID="btnThisMonth" runat="server" Text="Ce Mois" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnThisMonth_Click" CausesValidation="false" />
                                    </div>
                                    <button type="button" class="btn btn-outline-info btn-sm" onclick="showSearchHelp()" title="Aide recherche">
                                        <i class="fas fa-question-circle me-1"></i>Aide
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Results Section -->
        <asp:Panel ID="pnlResults" runat="server" Visible="false" CssClass="row">
            <div class="col-12">
                
                <!-- Search Section -->
                <div class="search-section" id="searchSection">
                    <div class="row align-items-center">
                        <div class="col-md-6">
                            <h6 class="mb-2">
                                <i class="fas fa-search me-2"></i>Recherche Rapide
                            </h6>
                            <div class="search-input-container">
                                <input type="text" 
                                       id="employeeSearch" 
                                       class="search-input" 
                                       placeholder="Rechercher par nom, prénom ou matricule..." 
                                       autocomplete="off">
                                <asp:HiddenField ID="hdnSearchValue" runat="server" />
                                <button class="search-clear-btn" 
                                        type="button" 
                                        id="clearSearch" 
                                        title="Effacer la recherche">
                                    <i class="fas fa-times"></i>
                                </button>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <small class="text-muted">
                                <span id="searchResults"></span>
                            </small>
                        </div>
                    </div>
                </div>

                <!-- Export Section -->
                <div class="export-section">
                    <asp:Button ID="btnExportExcel" runat="server" Text="Exporter Excel" CssClass="btn btn-export" OnClick="btnExportExcel_Click" />
                    <small class="text-muted ms-2">
                        <i class="fas fa-download"></i> 
                        <asp:Literal ID="litRecordCount" runat="server" /> employés
                    </small>
                </div>

                <!-- Results Summary -->
                <div class="results-summary">
                    <div class="row">
                        <div class="col-md-3">
                            <strong>Période:</strong> <asp:Literal ID="litPeriode" runat="server" />
                        </div>
                        <div class="col-md-3">
                            <strong>Port:</strong> <asp:Literal ID="litPortSelected" runat="server" />
                        </div>
                        <div class="col-md-3">
                            <strong>Total Présents:</strong> <span class="text-success"><asp:Literal ID="litTotalEntrees" runat="server" /></span>
                        </div>
                        <div class="col-md-3">
                            <strong>Total Sortis:</strong> <span class="text-danger"><asp:Literal ID="litTotalSorties" runat="server" /></span>
                        </div>
                    </div>
                </div>

                <!-- Results Grid -->
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-users me-2"></i>Rapport Employés par Port
                            <small class="text-muted ms-2">(Scroll pour voir plus d'employés)</small>
                        </h5>
                    </div>
                    <div class="card-body p-0">
                        <div class="table-container" id="employeeTableContainer">
                            <asp:GridView ID="gvPointageReport" runat="server" CssClass="table table-striped table-hover mb-0"
                                AutoGenerateColumns="false" EmptyDataText="Aucun employé trouvé pour les critères sélectionnés">
                                <Columns>
                                    <asp:BoundField DataField="Matricule" HeaderText="Matricule" />
                                    <asp:BoundField DataField="NomComplet" HeaderText="Employé" />
                                    <asp:BoundField DataField="NomPort" HeaderText="Port Assigné" />
                                    <asp:TemplateField HeaderText="Statut Actuel">
                                        <ItemTemplate>
                                            <span class='<%# "badge " + (Eval("StatutActuel").ToString() == "ENTREE" ? "bg-success" : "bg-danger") %>'>
                                                <%# Eval("StatutActuel").ToString() == "ENTREE" ? "PRÉSENT" : "SORTI" %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="DernierPointage" HeaderText="Dernier Pointage" />
                                    <asp:BoundField DataField="HeuresTravaillees" HeaderText="Heures Travaillées" ItemStyle-CssClass="text-primary fw-bold" />
                                    <asp:BoundField DataField="NombrePointages" HeaderText="Nb Pointages" ItemStyle-CssClass="text-center" />
                                    <asp:TemplateField HeaderText="Historique Période" ItemStyle-CssClass="history-cell">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center">
                                                <small class="text-muted me-2 flex-grow-1" title='<%# Eval("HistoriquePointages") %>'>
                                                    <%# TruncateText(Eval("HistoriquePointages").ToString(), 30) %>
                                                </small>
                                                <button type="button" class="btn btn-outline-info history-btn" 
                                                    onclick="showHistoryModal('<%# Eval("NomComplet") %>', '<%# Eval("HistoriquePointages").ToString().Replace("'", "\\'") %>')"
                                                    title="Voir historique complet">
                                                    <i class="fas fa-eye"></i>
                                                </button>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-CssClass="text-center" ItemStyle-Width="80px">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btnPrint" runat="server" 
                                                CssClass="btn btn-outline-success btn-sm" 
                                                CommandName="PrintEmployee" 
                                                CommandArgument='<%# Eval("IdEmploye") %>'
                                                OnCommand="btnPrint_Command"
                                                title="Imprimer récap employé">
                                                <i class="fas fa-print"></i>
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate>
                                    <div class="table-loading">
                                        <i class="fas fa-users fa-3x text-muted mb-3"></i>
                                        <p class="text-muted">Aucun employé trouvé pour les critères sélectionnés</p>
                                        <small class="text-muted">Essayez de modifier vos filtres de recherche</small>
                                    </div>
                                </EmptyDataTemplate>
                            </asp:GridView>
                        </div>
                        <!-- Scroll indicator and back to top -->
                        <div id="scrollIndicator" class="text-center py-2 bg-light border-top" style="display: none;">
                            <small class="text-muted">
                                <i class="fas fa-arrow-down me-1"></i>
                                Scroll vers le bas pour voir plus d'employés
                                <i class="fas fa-arrow-down ms-1"></i>
                            </small>
                        </div>
                        <!-- Back to top button -->
                        <button id="backToTopBtn" class="btn btn-primary btn-sm" 
                            style="position: fixed; bottom: 20px; right: 20px; z-index: 1000; display: none; border-radius: 50%; width: 45px; height: 45px;"
                            onclick="scrollToTop()" title="Retour en haut">
                            <i class="fas fa-arrow-up"></i>
                        </button>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- No Results Message -->
        <asp:Panel ID="pnlNoResults" runat="server" Visible="false" CssClass="row">
            <div class="col-12">
                <div class="alert alert-info text-center">
                    <i class="fas fa-info-circle me-2"></i>
                    Veuillez sélectionner une période et cliquer sur "Rechercher" pour afficher le rapport des employés.
                </div>
            </div>
        </asp:Panel>

    </div>

    <!-- History Modal -->
    <div class="modal fade" id="historyModal" tabindex="-1" aria-labelledby="historyModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="historyModalLabel">
                        <i class="fas fa-history me-2"></i>Historique Pointage - <span id="employeeName"></span>
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div id="historyContent" class="table-responsive">
                        <!-- History content will be populated here -->
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Fermer</button>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        // Search functionality variables
        var searchInput = null;
        var clearSearchBtn = null;
        var tableRows = [];
        var totalEmployees = 0;
        var searchSection = null;
        var hiddenSearchField = null;

        // Set default dates on page load
        document.addEventListener('DOMContentLoaded', function () {
            var dateFrom = document.getElementById('<%= txtDateFrom.ClientID %>');
            var dateTo = document.getElementById('<%= txtDateTo.ClientID %>');
            
            if (dateFrom && !dateFrom.value) {
                var today = new Date();
                var firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
                dateFrom.value = firstDay.toISOString().split('T')[0];
            }
            
            if (dateTo && !dateTo.value) {
                var today = new Date();
                dateTo.value = today.toISOString().split('T')[0];
            }
            
            // Initialize scroll indicator and search
            initializeScrollIndicator();
            initializeSearch();
        });

        // Initialize search functionality with state preservation
        function initializeSearch() {
            searchInput = document.getElementById('employeeSearch');
            clearSearchBtn = document.getElementById('clearSearch');
            searchSection = document.getElementById('searchSection');
            hiddenSearchField = document.getElementById('<%= hdnSearchValue.ClientID %>');
            
            if (!searchInput || !clearSearchBtn) return;

            // Restore search value from hidden field after postback
            if (hiddenSearchField && hiddenSearchField.value) {
                searchInput.value = hiddenSearchField.value;
            }

            // Get all employee rows from the GridView
            var gridView = document.getElementById('<%= gvPointageReport.ClientID %>');
            if (!gridView) return;

            // Get all data rows (skip header)
            var allRows = gridView.querySelectorAll('tr');
            tableRows = Array.from(allRows).slice(1); // Skip header row
            totalEmployees = tableRows.length;

            // Show search section if there are employees
            if (totalEmployees > 0) {
                searchSection.style.display = 'block';
                updateSearchResults(totalEmployees, totalEmployees);
                
                // Apply search filter if there was a previous search
                if (searchInput.value) {
                    performSearch();
                }
            }

            // Add event listeners
            searchInput.addEventListener('input', function() {
                performSearch();
                saveSearchValue();
            });
            
            searchInput.addEventListener('keyup', function(e) {
                if (e.key === 'Escape') {
                    clearSearch();
                }
            });
            
            clearSearchBtn.addEventListener('click', clearSearch);

            // Save search value before any postback
            setupPostbackHandlers();
        }

        // Setup handlers to preserve search state across postbacks
        function setupPostbackHandlers() {
            // Save search value before postback occurs
            var form = document.forms[0];
            if (form) {
                var originalSubmit = form.submit;
                form.submit = function() {
                    saveSearchValue();
                    return originalSubmit.apply(this, arguments);
                };
            }

            // Handle ASP.NET button clicks
            var buttons = document.querySelectorAll('input[type="submit"], button[type="submit"]');
            buttons.forEach(function(button) {
                button.addEventListener('click', function() {
                    saveSearchValue();
                });
            });

            // Handle ASP.NET LinkButton clicks  
            var linkButtons = document.querySelectorAll('a[href*="__doPostBack"]');
            linkButtons.forEach(function(linkButton) {
                linkButton.addEventListener('click', function() {
                    saveSearchValue();
                });
            });
        }

        function saveSearchValue() {
            if (searchInput && hiddenSearchField) {
                hiddenSearchField.value = searchInput.value;
            }
        }

        // Perform search filtering
        function performSearch() {
            var searchTerm = searchInput.value.toLowerCase().trim();
            var visibleCount = 0;

            // Remove previous highlights
            removeHighlights();

            if (searchTerm === '') {
                // Show all rows if search is empty
                tableRows.forEach(function(row) {
                    row.style.display = '';
                    row.classList.remove('filtered-out');
                });
                visibleCount = totalEmployees;
            } else {
                // Filter rows based on search term
                tableRows.forEach(function(row) {
                    var shouldShow = false;
                    var cells = row.getElementsByTagName('td');
                    
                    if (cells.length >= 2) {
                        // Search in Matricule (column 0) and Employee Name (column 1)
                        var matricule = cells[0].textContent.toLowerCase();
                        var employeeName = cells[1].textContent.toLowerCase();
                        
                        // Check if search term matches matricule or employee name
                        if (matricule.includes(searchTerm) || employeeName.includes(searchTerm)) {
                            shouldShow = true;
                            
                            // Highlight matching text
                            highlightText(cells[0], searchTerm);
                            highlightText(cells[1], searchTerm);
                        }
                    }

                    if (shouldShow) {
                        row.style.display = '';
                        row.classList.remove('filtered-out');
                        visibleCount++;
                    } else {
                        row.style.display = 'none';
                        row.classList.add('filtered-out');
                    }
                });
            }

            // Update search results display
            updateSearchResults(visibleCount, totalEmployees);

            // Show "no results" message if needed
            showNoResultsMessage(visibleCount === 0 && searchTerm !== '');
        }

        // Highlight matching text
        function highlightText(element, searchTerm) {
            if (!searchTerm) return;
            
            var originalText = element.textContent;
            var lowerText = originalText.toLowerCase();
            var lowerSearchTerm = searchTerm.toLowerCase();
            
            if (lowerText.includes(lowerSearchTerm)) {
                var startIndex = lowerText.indexOf(lowerSearchTerm);
                var endIndex = startIndex + searchTerm.length;
                
                var highlightedText = 
                    originalText.substring(0, startIndex) +
                    '<span class="search-highlight">' + 
                    originalText.substring(startIndex, endIndex) + 
                    '</span>' +
                    originalText.substring(endIndex);
                
                element.innerHTML = highlightedText;
            }
        }

        // Remove all highlights
        function removeHighlights() {
            var highlights = document.querySelectorAll('.search-highlight');
            highlights.forEach(function(highlight) {
                var parent = highlight.parentNode;
                parent.replaceChild(document.createTextNode(highlight.textContent), highlight);
                parent.normalize();
            });
        }

        // Update search results counter
        function updateSearchResults(visible, total) {
            var searchResults = document.getElementById('searchResults');
            if (!searchResults) return;

            if (visible === total) {
                searchResults.innerHTML = '<i class="fas fa-users me-1"></i>' + total + ' employé(s) affiché(s)';
                searchResults.className = 'text-muted';
            } else {
                searchResults.innerHTML = 
                    '<i class="fas fa-filter me-1"></i>' + visible + ' sur ' + total + ' employé(s)' +
                    (visible === 0 ? ' - <span class="text-warning">Aucun résultat</span>' : '');
                searchResults.className = visible > 0 ? 'text-info' : 'text-warning';
            }
        }

        // Show/hide no results message
        function showNoResultsMessage(show) {
            var gridView = document.getElementById('<%= gvPointageReport.ClientID %>');
            if (!gridView) return;

            var existingMessage = gridView.querySelector('.search-no-results-row');
            
            if (show && !existingMessage) {
                // Create no results row
                var noResultsRow = document.createElement('tr');
                noResultsRow.className = 'search-no-results-row';
                noResultsRow.innerHTML = 
                    '<td colspan="100%" class="text-center py-4">' +
                    '<i class="fas fa-search fa-2x mb-3 text-muted"></i><br>' +
                    '<strong>Aucun employé trouvé</strong><br>' +
                    '<small class="text-muted">Aucun employé ne correspond à votre recherche. Essayez un autre terme.</small>' +
                    '</td>';
                
                gridView.querySelector('tbody').appendChild(noResultsRow);
            } else if (!show && existingMessage) {
                existingMessage.remove();
            }
        }

        // Clear search
        function clearSearch() {
            if (searchInput) {
                searchInput.value = '';
                if (hiddenSearchField) {
                    hiddenSearchField.value = '';
                }
                searchInput.focus();
                performSearch();
            }
        }

        // Initialize scroll behavior and indicator
        function initializeScrollIndicator() {
            var tableContainer = document.getElementById('employeeTableContainer');
            var scrollIndicator = document.getElementById('scrollIndicator');
            var backToTopBtn = document.getElementById('backToTopBtn');
            
            if (tableContainer && scrollIndicator && backToTopBtn) {
                // Check if scrolling is needed
                function checkScrollNeeded() {
                    if (tableContainer.scrollHeight > tableContainer.clientHeight) {
                        scrollIndicator.style.display = 'block';
                    } else {
                        scrollIndicator.style.display = 'none';
                        backToTopBtn.style.display = 'none';
                    }
                }
                
                // Handle scroll events
                tableContainer.addEventListener('scroll', function() {
                    var scrollTop = tableContainer.scrollTop;
                    var scrollHeight = tableContainer.scrollHeight;
                    var clientHeight = tableContainer.clientHeight;
                    
                    // Show/hide back to top button
                    if (scrollTop > 100) {
                        backToTopBtn.style.display = 'block';
                    } else {
                        backToTopBtn.style.display = 'none';
                    }
                    
                    // Hide indicator when user starts scrolling or reaches bottom
                    if (scrollTop > 10 || (scrollTop + clientHeight >= scrollHeight - 10)) {
                        scrollIndicator.style.display = 'none';
                    } else if (scrollHeight > clientHeight) {
                        scrollIndicator.style.display = 'block';
                    }
                });
                
                // Check on page load and after any updates
                setTimeout(checkScrollNeeded, 500);
                
                // Re-check when window is resized
                window.addEventListener('resize', checkScrollNeeded);
            }
        }

        // Validate date range
        function validateDateRange() {
            var dateFrom = document.getElementById('<%= txtDateFrom.ClientID %>').value;
            var dateTo = document.getElementById('<%= txtDateTo.ClientID %>').value;
            
            if (!dateFrom || !dateTo) {
                alert('Veuillez sélectionner les dates de début et fin.');
                return false;
            }
            
            if (new Date(dateFrom) > new Date(dateTo)) {
                alert('La date de début doit être antérieure à la date de fin.');
                return false;
            }
            
            return true;
        }

        // Add validation to search button
        document.getElementById('<%= btnSearch.ClientID %>').addEventListener('click', function (e) {
            if (!validateDateRange()) {
                e.preventDefault();
            } else {
                // Show loading state
                showLoadingState();
            }
        });

        // Show loading state
        function showLoadingState() {
            var tableContainer = document.getElementById('employeeTableContainer');
            if (tableContainer) {
                tableContainer.innerHTML = '<div class="table-loading"><i class="fas fa-spinner fa-spin fa-2x text-primary mb-3"></i><p class="text-muted">Chargement des données...</p></div>';
            }
        }

        // Scroll to top function with smooth animation
        function scrollToTop() {
            var tableContainer = document.getElementById('employeeTableContainer');
            if (tableContainer) {
                tableContainer.scrollTo({
                    top: 0,
                    behavior: 'smooth'
                });
            }
        }

        // Show history modal
        function showHistoryModal(employeeName, historyData) {
            document.getElementById('employeeName').textContent = employeeName;

            var historyContent = document.getElementById('historyContent');

            if (!historyData || historyData.trim() === '' || historyData === 'Aucun pointage sur la période') {
                historyContent.innerHTML = '<div class="alert alert-info text-center"><i class="fas fa-info-circle me-2"></i>Aucun pointage trouvé pour cette période.</div>';
            } else {
                // Parse the history string and create a nice table
                var historyItems = historyData.split(', ').filter(function (item) { return item.trim() !== ''; });

                if (historyItems.length === 0) {
                    historyContent.innerHTML = '<div class="alert alert-info text-center"><i class="fas fa-info-circle me-2"></i>Aucun pointage trouvé pour cette période.</div>';
                    return;
                }

                var tableHtml = '<table class="table table-striped table-hover mb-0">';
                tableHtml += '<thead><tr><th><i class="fas fa-calendar me-1"></i>Date</th><th><i class="fas fa-clock me-1"></i>Heure</th><th><i class="fas fa-tag me-1"></i>Type</th><th><i class="fas fa-circle me-1"></i>Statut</th></tr></thead>';
                tableHtml += '<tbody>';

                historyItems.forEach(function (item, index) {
                    if (item.trim()) {
                        // Parse format: "15/07 08:30 (ENTREE)" or "15/07 08:30 (SORTIE)"
                        var match = item.trim().match(/(\d{2}\/\d{2})\s+(\d{2}:\d{2})\s+\((\w+)\)/);
                        if (match) {
                            var date = match[1];
                            var time = match[2];
                            var type = match[3];
                            var badgeClass = type === 'ENTREE' ? 'bg-success' : 'bg-danger';
                            var typeText = type === 'ENTREE' ? 'ENTRÉE' : 'SORTIE';
                            var icon = type === 'ENTREE' ? 'fa-sign-in-alt' : 'fa-sign-out-alt';
                            var rowClass = type === 'ENTREE' ? 'table-success' : 'table-danger';

                            tableHtml += '<tr class="' + rowClass + '">';
                            tableHtml += '<td><strong>' + date + '</strong></td>';
                            tableHtml += '<td><strong>' + time + '</strong></td>';
                            tableHtml += '<td>' + type + '</td>';
                            tableHtml += '<td><span class="badge ' + badgeClass + '"><i class="fas ' + icon + ' me-1"></i>' + typeText + '</span></td>';
                            tableHtml += '</tr>';
                        } else {
                            // If parsing fails, show raw data
                            tableHtml += '<tr><td colspan="4" class="text-muted"><em>' + item + '</em></td></tr>';
                        }
                    }
                });

                tableHtml += '</tbody></table>';

                // Add summary info
                var entreeCount = historyItems.filter(function (item) { return item.includes('(ENTREE)'); }).length;
                var sortieCount = historyItems.filter(function (item) { return item.includes('(SORTIE)'); }).length;

                var summaryHtml = '<div class="alert alert-light mt-3 mb-0">';
                summaryHtml += '<div class="row text-center">';
                summaryHtml += '<div class="col-md-4"><strong>Total Pointages:</strong> <span class="badge bg-primary">' + historyItems.length + '</span></div>';
                summaryHtml += '<div class="col-md-4"><strong>Entrées:</strong> <span class="badge bg-success">' + entreeCount + '</span></div>';
                summaryHtml += '<div class="col-md-4"><strong>Sorties:</strong> <span class="badge bg-danger">' + sortieCount + '</span></div>';
                summaryHtml += '</div></div>';

                historyContent.innerHTML = tableHtml + summaryHtml;
            }

            // Show the modal
            var modal = new bootstrap.Modal(document.getElementById('historyModal'));
            modal.show();
        }

        function showSearchHelp() {
            alert('Conseils de recherche:\n\n' +
                '• Tapez le nom, prénom ou matricule pour filtrer\n' +
                '• La recherche est insensible à la casse\n' +
                '• Utilisez Ctrl+F ou Ctrl+K pour accéder rapidement à la recherche\n' +
                '• Appuyez sur Échap pour effacer la recherche\n' +
                '• Les résultats sont mis à jour en temps réel');
        }

        // Keyboard shortcuts
        document.addEventListener('keydown', function (e) {
            // Ctrl + F or Ctrl + K to focus search
            if ((e.ctrlKey && (e.key === 'f' || e.key === 'k')) && searchInput) {
                e.preventDefault();
                searchInput.focus();
                searchInput.select();
            }
        });

        // Re-initialize search after postback
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        if (prm) {
            prm.add_endRequest(function () {
                setTimeout(function () {
                    initializeScrollIndicator();
                    initializeSearch();
                }, 100);
            });
        }
    </script>
</asp:Content>