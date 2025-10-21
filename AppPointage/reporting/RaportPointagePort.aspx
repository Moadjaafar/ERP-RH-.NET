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
                                        <asp:ListItem Value="" Text="-- Tous les Ports --" />
                                        <asp:ListItem Value="Port A - Entrée Principale" Text="Port A - Entrée Principale" />
                                        <asp:ListItem Value="Port B - Entrée Personnel" Text="Port B - Entrée Personnel" />
                                        <asp:ListItem Value="Port C - Entrée Livraisons" Text="Port C - Entrée Livraisons" />
                                        <asp:ListItem Value="Port D - Entrée Urgences" Text="Port D - Entrée Urgences" />
                                        <asp:ListItem Value="Port E - Entrée VIP" Text="Port E - Entrée VIP" />
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
                                <div class="btn-group" role="group">
                                    <asp:Button ID="btnToday" runat="server" Text="Aujourd'hui" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnToday_Click" CausesValidation="false" />
                                    <asp:Button ID="btnYesterday" runat="server" Text="Hier" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnYesterday_Click" CausesValidation="false" />
                                    <asp:Button ID="btnThisWeek" runat="server" Text="Cette Semaine" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnThisWeek_Click" CausesValidation="false" />
                                    <asp:Button ID="btnThisMonth" runat="server" Text="Ce Mois" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnThisMonth_Click" CausesValidation="false" />
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
        // Set default dates on page load
        document.addEventListener('DOMContentLoaded', function() {
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
            
            // Initialize scroll indicator
            initializeScrollIndicator();
        });

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
        document.getElementById('<%= btnSearch.ClientID %>').addEventListener('click', function(e) {
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
                var historyItems = historyData.split(', ').filter(function(item) { return item.trim() !== ''; });
                
                if (historyItems.length === 0) {
                    historyContent.innerHTML = '<div class="alert alert-info text-center"><i class="fas fa-info-circle me-2"></i>Aucun pointage trouvé pour cette période.</div>';
                    return;
                }
                
                var tableHtml = '<table class="table table-striped table-hover mb-0">';
                tableHtml += '<thead><tr><th><i class="fas fa-calendar me-1"></i>Date</th><th><i class="fas fa-clock me-1"></i>Heure</th><th><i class="fas fa-tag me-1"></i>Type</th><th><i class="fas fa-circle me-1"></i>Statut</th></tr></thead>';
                tableHtml += '<tbody>';
                
                historyItems.forEach(function(item, index) {
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
                var entreeCount = historyItems.filter(function(item) { return item.includes('(ENTREE)'); }).length;
                var sortieCount = historyItems.filter(function(item) { return item.includes('(SORTIE)'); }).length;
                
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
        
        // Re-initialize scroll indicator after postback
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        if (prm) {
            prm.add_endRequest(function() {
                setTimeout(initializeScrollIndicator, 100);
            });
        }
    </script>
</asp:Content>

