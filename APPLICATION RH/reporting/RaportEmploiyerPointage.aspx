<%@ Page Title="Rapport Employé - Détails Pointage" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RaportEmploiyerPointage.aspx.cs" Inherits="APPLICATION_RH.reporting.RaportEmploiyerPointage" %>
<%@ Import Namespace="System.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* ==========================================================================
           EMPLOYEE DETAIL PAGE STYLES
           ========================================================================== */
        
        /* Header Styles */
        .employee-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            border-radius: 0.5rem;
            margin-bottom: 25px;
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
        }
        
        .employee-avatar {
            width: 80px;
            height: 80px;
            border-radius: 50%;
            background: rgba(255, 255, 255, 0.2);
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 2rem;
            margin-right: 20px;
        }
        
        .employee-info h2 {
            margin: 0 0 5px 0;
            font-weight: 600;
        }
        
        .employee-info .badge {
            font-size: 0.9rem;
            padding: 0.5em 0.8em;
        }
        
        /* Statistics Cards */
        .stats-container {
            margin-bottom: 30px;
        }
        
        .stat-card {
            background: white;
            border-radius: 0.5rem;
            padding: 25px;
            text-align: center;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
            border-left: 4px solid;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }
        
        .stat-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
        }
        
        .stat-card.primary { border-left-color: #0d6efd; }
        .stat-card.success { border-left-color: #198754; }
        .stat-card.danger { border-left-color: #dc3545; }
        .stat-card.warning { border-left-color: #ffc107; }
        
        .stat-number {
            font-size: 2.5rem;
            font-weight: 700;
            margin: 0;
            line-height: 1;
        }
        
        .stat-label {
            font-size: 0.9rem;
            color: #6c757d;
            margin: 5px 0 0 0;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .stat-icon {
            font-size: 2rem;
            margin-bottom: 10px;
            opacity: 0.7;
        }
        
        /* Filter Section */
        .filter-section {
            background: linear-gradient(135deg, #f8f9fa 0%, #ffffff 100%);
            padding: 25px;
            border-radius: 12px;
            margin-bottom: 30px;
            border: 1px solid #e9ecef;
            box-shadow: 0 2px 15px rgba(0, 0, 0, 0.08);
        }
        
        .filter-title {
            font-size: 1.1rem;
            font-weight: 600;
            color: #495057;
            margin-bottom: 0;
        }
        
        .filter-controls {
            display: flex;
            align-items: center;
            gap: 20px;
            flex-wrap: wrap;
        }
        
        .date-inputs {
            display: flex;
            align-items: center;
            gap: 15px;
            flex: 1;
            min-width: 300px;
        }
        
        .date-inputs .input-group {
            flex: 1;
            max-width: 200px;
        }
        
        .date-inputs .input-group-text {
            background: #ffffff;
            border: 1px solid #ced4da;
            padding: 0.5rem 0.8rem;
        }
        
        .date-inputs .form-control {
            border-left: none;
            padding: 0.5rem 0.8rem;
            font-size: 0.95rem;
        }
        
        .date-inputs .form-control:focus {
            border-color: #007bff;
            box-shadow: 0 0 0 0.2rem rgba(0, 123, 255, 0.25);
        }
        
        .date-separator {
            font-weight: 500;
            color: #6c757d;
            font-size: 0.9rem;
            padding: 0 5px;
            white-space: nowrap;
        }
        
        .action-buttons {
            display: flex;
            gap: 12px;
            align-items: center;
        }
        
        .filter-btn {
            padding: 0.6rem 1.5rem;
            font-weight: 500;
            font-size: 0.95rem;
            border-radius: 8px;
            transition: all 0.3s ease;
            min-width: 100px;
        }
        
        .filter-btn:hover {
            transform: translateY(-1px);
            box-shadow: 0 4px 12px rgba(0, 123, 255, 0.3);
        }
        
        .print-btn {
            padding: 0.6rem 1.5rem;
            font-weight: 500;
            font-size: 0.95rem;
            border-radius: 8px;
            transition: all 0.3s ease;
            min-width: 120px;
            background: #28a745;
            border-color: #28a745;
        }
        
        .print-btn:hover {
            transform: translateY(-1px);
            box-shadow: 0 4px 12px rgba(40, 167, 69, 0.3);
            background: #218838;
            border-color: #1e7e34;
        }
        
        .print-btn:active {
            transform: translateY(0);
        }
        
        /* Table Styles */
        .pointage-table {
            background: white;
            border-radius: 0.5rem;
            overflow: hidden;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
        }
        
        .table-header {
            background: #343a40;
            color: white;
            padding: 15px 20px;
        }
        
        .table-container {
            max-height: 600px;
            overflow-y: auto;
        }
        
        .table thead th {
            background: #495057;
            color: white;
            border: none;
            font-weight: 600;
            text-transform: uppercase;
            font-size: 0.85rem;
            letter-spacing: 0.5px;
            padding: 15px 10px;
            position: sticky;
            top: 0;
            z-index: 10;
        }
        
        .table tbody tr:hover {
            background-color: rgba(0, 123, 255, 0.05);
        }
        
        .table td {
            padding: 12px 10px;
            vertical-align: middle;
        }
        
        /* Status Badges */
        .status-entree {
            background-color: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }
        
        .status-sortie {
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }
        
        /* Navigation */
        .back-button {
            margin-bottom: 20px;
        }
        
        /* Export Section */
        .export-actions {
            background: white;
            padding: 15px 20px;
            border-bottom: 1px solid #dee2e6;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        
        /* Daily Summary Cards */
        .daily-summary {
            background: white;
            border-left: 4px solid #0d6efd;
            padding: 20px;
            margin-bottom: 15px;
            border-radius: 0.5rem;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }
        
        .daily-summary:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.12);
        }
        
        .daily-summary h6 {
            margin: 0 0 15px 0;
            color: #0d6efd;
            font-weight: 600;
            font-size: 1.1rem;
        }
        
        .daily-summary .row .col-md-2 {
            border-right: 1px solid #e9ecef;
            padding: 0 15px;
        }
        
        .daily-summary .row .col-md-2:last-child {
            border-right: none;
        }
        
        .daily-summary small.text-muted {
            font-size: 0.75rem;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            font-weight: 500;
        }
        
        /* Clickable daily summary styling */
        .clickable-day {
            position: relative;
            transition: all 0.3s ease;
        }
        
        .clickable-day:hover {
            transform: translateY(-3px);
            box-shadow: 0 6px 20px rgba(0, 0, 0, 0.15) !important;
            border-left-color: #0056b3;
        }
        
        .clickable-day::after {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: linear-gradient(90deg, rgba(13, 110, 253, 0.05) 0%, rgba(13, 110, 253, 0.02) 100%);
            opacity: 0;
            transition: opacity 0.3s ease;
            pointer-events: none;
            border-radius: 0.5rem;
        }
        
        .clickable-day:hover::after {
            opacity: 1;
        }
        
        .clickable-day:active {
            transform: translateY(-1px);
        }
        
        /* Day Details Modal Table Styles */
        .day-details-table-container {
            max-height: 450px;
            overflow-y: auto;
            border: 1px solid #dee2e6;
            border-radius: 0.375rem;
            background: #ffffff;
        }
        
        .day-details-table-container .table {
            margin-bottom: 0;
        }
        
        .day-details-table-container .sticky-header {
            position: sticky;
            top: 0;
            z-index: 10;
            background: #495057 !important;
        }
        
        .day-details-table-container .sticky-header th {
            border-bottom: 2px solid #343a40;
            box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.1);
        }
        
        .day-details-table-container::-webkit-scrollbar {
            width: 8px;
        }
        
        .day-details-table-container::-webkit-scrollbar-track {
            background: #f1f1f1;
            border-radius: 4px;
        }
        
        .day-details-table-container::-webkit-scrollbar-thumb {
            background: #c1c1c1;
            border-radius: 4px;
        }
        
        .day-details-table-container::-webkit-scrollbar-thumb:hover {
            background: #a8a8a8;
        }
        
        .scroll-indicator {
            padding: 8px;
            background: linear-gradient(to bottom, transparent, rgba(248, 249, 250, 0.8));
            margin-top: -10px;
            position: relative;
            z-index: 5;
        }
        
        .scroll-indicator i {
            animation: bounce 2s infinite;
        }
        
        @keyframes bounce {
            0%, 20%, 50%, 80%, 100% {
                transform: translateY(0);
            }
            40% {
                transform: translateY(-3px);
            }
            60% {
                transform: translateY(-2px);
            }
        }
        
        /* Loading States */
        .loading-container {
            text-align: center;
            padding: 40px;
            color: #6c757d;
        }
        
        .loading-spinner {
            width: 40px;
            height: 40px;
            border: 4px solid #f3f3f3;
            border-top: 4px solid #0d6efd;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            margin: 0 auto 15px;
        }
        
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        
        /* Responsive Design */
        @media (max-width: 768px) {
            .employee-header {
                padding: 20px;
                text-align: center;
            }
            
            .employee-avatar {
                margin: 0 auto 15px;
            }
            
            .stat-card {
                margin-bottom: 15px;
            }
            
            .table-container {
                max-height: 400px;
            }
            
            .export-actions {
                flex-direction: column;
                gap: 10px;
            }
            
            /* Mobile Filter Section */
            .filter-section {
                padding: 20px 15px;
            }
            
            .filter-controls {
                flex-direction: column;
                align-items: stretch;
                gap: 15px;
            }
            
            .date-inputs {
                flex-direction: column;
                gap: 10px;
                min-width: auto;
            }
            
            .date-inputs .input-group {
                max-width: none;
            }
            
            .date-separator {
                text-align: center;
                padding: 5px 0;
            }
            
            .action-buttons {
                flex-direction: column;
                gap: 10px;
            }
            
            .filter-btn, .print-btn {
                width: 100%;
                min-width: auto;
            }
        }
        
        @media (max-width: 576px) {
            .filter-section {
                margin-left: -15px;
                margin-right: -15px;
                border-radius: 0;
            }
            
            .filter-title {
                text-align: center;
                margin-bottom: 15px;
            }
        }
        
        /* Print Styles */
        @media print {
            .back-button,
            .export-actions,
            .no-print {
                display: none !important;
            }
            
            .employee-header {
                background: #f8f9fa !important;
                color: #000 !important;
                border: 2px solid #000;
            }
            
            .table thead th {
                background: #f0f0f0 !important;
                color: #000 !important;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        
        <!-- Back Navigation -->
        <div class="back-button no-print">
            <asp:LinkButton ID="lnkBack" runat="server" CssClass="btn btn-outline-secondary" OnClick="lnkBack_Click">
                <i class="fas fa-arrow-left me-2"></i>Retour à la liste
            </asp:LinkButton>
        </div>

        <!-- Employee Header -->
        <div class="employee-header">
            <div class="row align-items-center">
                <div class="col-auto">
                    <div class="employee-avatar">
                        <i class="fas fa-user"></i>
                    </div>
                </div>
                <div class="col">
                    <div class="employee-info">
                        <h2><asp:Literal ID="litEmployeeName" runat="server" /></h2>
                        <p class="mb-2">
                            <span class="badge bg-light text-dark me-2">
                                <i class="fas fa-id-card me-1"></i>
                                <asp:Literal ID="litMatricule" runat="server" />
                            </span>
                            <span class="badge bg-light text-dark">
                                <i class="fas fa-door-open me-1"></i>
                                <asp:Literal ID="litPortAssigne" runat="server" />
                            </span>
                        </p>
                        <small class="opacity-75">
                            <i class="fas fa-calendar-alt me-1"></i>
                            Période: <asp:Literal ID="litPeriode" runat="server" />
                        </small>
                    </div>
                </div>
                <div class="col-auto text-end">
                    <asp:Panel ID="pnlCurrentStatus" runat="server" CssClass="text-end">
                        <small class="opacity-75 d-block mb-1">Statut Actuel</small>
                        <asp:Literal ID="litCurrentStatus" runat="server" />
                    </asp:Panel>
                </div>
            </div>
        </div>

        <!-- Statistics Cards -->
        <div class="stats-container">
            <div class="row">
                <div class="col-lg-3 col-md-6 mb-3">
                    <div class="stat-card primary">
                        <div class="stat-icon text-primary">
                            <i class="fas fa-clock"></i>
                        </div>
                        <p class="stat-number text-primary">
                            <asp:Literal ID="litTotalPointages" runat="server" />
                        </p>
                        <p class="stat-label">Total Pointages</p>
                    </div>
                </div>
                <div class="col-lg-3 col-md-6 mb-3">
                    <div class="stat-card success">
                        <div class="stat-icon text-success">
                            <i class="fas fa-sign-in-alt"></i>
                        </div>
                        <p class="stat-number text-success">
                            <asp:Literal ID="litTotalEntrees" runat="server" />
                        </p>
                        <p class="stat-label">Entrées</p>
                    </div>
                </div>
                <div class="col-lg-3 col-md-6 mb-3">
                    <div class="stat-card danger">
                        <div class="stat-icon text-danger">
                            <i class="fas fa-sign-out-alt"></i>
                        </div>
                        <p class="stat-number text-danger">
                            <asp:Literal ID="litTotalSorties" runat="server" />
                        </p>
                        <p class="stat-label">Sorties</p>
                    </div>
                </div>
                <div class="col-lg-3 col-md-6 mb-3">
                    <div class="stat-card warning">
                        <div class="stat-icon text-warning">
                            <i class="fas fa-hourglass-half"></i>
                        </div>
                        <p class="stat-number text-warning">
                            <asp:Literal ID="litHeuresTravaillees" runat="server" />
                        </p>
                        <p class="stat-label">Heures Travaillées</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Filter Section -->
        <div class="filter-section no-print">
            <div class="row align-items-center">
                <div class="col-md-4">
                    <h6 class="mb-0 filter-title">
                        <i class="fas fa-filter me-2 text-primary"></i>Filtrer par période
                    </h6>
                </div>
                <div class="col-md-8">
                    <div class="filter-controls">
                        <div class="date-inputs">
                            <div class="input-group">
                                <span class="input-group-text">
                                    <i class="fas fa-calendar-alt text-muted"></i>
                                </span>
                                <asp:TextBox ID="txtFilterDateFrom" runat="server" CssClass="form-control" TextMode="Date" placeholder="Date de début" />
                            </div>
                            <span class="date-separator">à</span>
                            <div class="input-group">
                                <span class="input-group-text">
                                    <i class="fas fa-calendar-alt text-muted"></i>
                                </span>
                                <asp:TextBox ID="txtFilterDateTo" runat="server" CssClass="form-control" TextMode="Date" placeholder="Date de fin" />
                            </div>
                        </div>
                        <div class="action-buttons">
                            <asp:Button ID="btnFilterApply" runat="server" Text="Appliquer" CssClass="btn btn-primary filter-btn" OnClick="btnFilterApply_Click" />
                            <button type="button" class="btn btn-success print-btn" onclick="printDetailedReport()" title="Imprimer le rapport détaillé">
                                <i class="fas fa-print me-2"></i>Imprimer
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Pointage Details Table (Hidden by default) -->
        <div id="pointageDetailsSection" class="pointage-table" style="display: none;">
            <div class="export-actions no-print">
                <div>
                    <h5 class="mb-0">
                        <i class="fas fa-list-alt me-2"></i>Détail des Pointages
                        <small class="text-muted ms-2">
                            (<asp:Literal ID="litRecordCount" runat="server" /> enregistrements)
                        </small>
                    </h5>
                </div>
                <div>
                    <asp:Button ID="btnExportExcel" runat="server" Text="Export Excel" CssClass="btn btn-success btn-sm me-2" OnClick="btnExportExcel_Click" />
                    <button type="button" class="btn btn-outline-primary btn-sm" onclick="window.print()">
                        <i class="fas fa-print me-1"></i>Imprimer
                    </button>
                </div>
            </div>
            
            <div class="table-container">
                <asp:GridView ID="gvPointageDetails" runat="server" CssClass="table table-hover mb-0"
                    AutoGenerateColumns="false" EmptyDataText="Aucun pointage trouvé pour la période sélectionnée"
                    OnRowDataBound="gvPointageDetails_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="DateFormatted" HeaderText="Date" ItemStyle-CssClass="fw-bold" />
                        <asp:BoundField DataField="HeureFormatted" HeaderText="Heure" ItemStyle-CssClass="text-primary fw-bold" />
                        <asp:TemplateField HeaderText="Type">
                            <ItemTemplate>
                                <span class='badge <%# Eval("Type").ToString() == "ENTREE" ? "status-entree" : "status-sortie" %>'>
                                    <i class='fas <%# Eval("Type").ToString() == "ENTREE" ? "fa-sign-in-alt" : "fa-sign-out-alt" %> me-1'></i>
                                    <%# Eval("Type") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Port" HeaderText="Port" />
                        <asp:BoundField DataField="JourSemaine" HeaderText="Jour" ItemStyle-CssClass="text-muted" />
                        <asp:TemplateField HeaderText="Durée Travaillée">
                            <ItemTemplate>
                                <asp:Literal ID="litDureeTravaillee" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Observations">
                            <ItemTemplate>
                                <small class="text-muted">
                                    <asp:Literal ID="litObservations" runat="server" />
                                </small>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="loading-container">
                            <i class="fas fa-calendar-times fa-3x text-muted mb-3"></i>
                            <p class="text-muted">Aucun pointage trouvé pour la période sélectionnée</p>
                            <small class="text-muted">Essayez de modifier les dates ou vérifiez que l'employé a effectué des pointages.</small>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>

        <!-- Daily Summary Section -->
        <asp:Panel ID="pnlDailySummary" runat="server" CssClass="mt-4">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <h5 class="mb-0">
                    <i class="fas fa-chart-line me-2"></i>Résumé par Jour
                </h5>
                <small class="text-muted">
                    <i class="fas fa-info-circle me-1"></i>Cliquez sur un jour pour voir les détails
                </small>
            </div>
            <asp:Repeater ID="rptDailySummary" runat="server">
                <ItemTemplate>
                    <div class="daily-summary clickable-day" 
                         onclick="showDayDetails('<%# Eval("DateFormatted") %>', '<%# Eval("DatePointage", "{0:yyyy-MM-dd}") %>')" 
                         style="cursor: pointer;" 
                         title="Cliquer pour voir les détails du <%# Eval("DateFormatted") %>">
                        <div class="row align-items-center">
                            <div class="col-md-2">
                                <h6 class="mb-0"><%# Eval("DateFormatted") %></h6>
                                <small class="text-muted"><%# Eval("JourSemaine") %></small>
                            </div>
                            <div class="col-md-2">
                                <small class="text-muted d-block">Première Entrée</small>
                                <strong class="text-success"><%# Eval("PremiereEntree") ?? "--:--" %></strong>
                            </div>
                            <div class="col-md-2">
                                <small class="text-muted d-block">Dernière Sortie</small>
                                <strong class="text-danger"><%# Eval("DerniereSortie") ?? "--:--" %></strong>
                            </div>
                            <div class="col-md-2">
                                <small class="text-muted d-block">Heures Travaillées</small>
                                <strong class="text-primary"><%# Eval("HeuresTravaillees") %></strong>
                            </div>
                            <div class="col-md-2">
                                <small class="text-muted d-block">Nb Pointages</small>
                                <span class="badge bg-secondary"><%# Eval("NombrePointages") %></span>
                            </div>
                            <div class="col-md-2 text-end">
                                <small class="text-muted d-block">Statut</small>
                                <small class="text-muted">
                                    <%# Eval("StatusText") %>
                                </small>
                                <div class="mt-1">
                                    <i class="fas fa-eye text-primary" style="font-size: 0.9rem;"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <!-- Error Panel -->
        <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert alert-danger">
            <h5><i class="fas fa-exclamation-triangle me-2"></i>Erreur</h5>
            <asp:Literal ID="litError" runat="server" />
        </asp:Panel>

    </div>

    <!-- Day Details Modal -->
    <div class="modal fade" id="dayDetailsModal" tabindex="-1" aria-labelledby="dayDetailsModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-xl">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title" id="dayDetailsModalLabel">
                        <i class="fas fa-calendar-day me-2"></i>Détails du <span id="modalDayDate"></span>
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div id="dayDetailsLoading" class="loading-container" style="display: none;">
                        <div class="loading-spinner"></div>
                        <p class="text-muted">Chargement des détails...</p>
                    </div>
                    <div id="dayDetailsContent">
                        <!-- Content will be loaded here -->
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        <i class="fas fa-times me-1"></i>Fermer
                    </button>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        // Initialize page functionality
        document.addEventListener('DOMContentLoaded', function() {
            // Set current date range in filter inputs
            initializeFilterDates();
            
            // Add smooth scrolling
            enableSmoothScrolling();
            
            // Initialize tooltips if Bootstrap 5 is available
            if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
                var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
                var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
                    return new bootstrap.Tooltip(tooltipTriggerEl);
                });
            }
        });
        
        function initializeFilterDates() {
            var dateFromInput = document.getElementById('<%= txtFilterDateFrom.ClientID %>');
            var dateToInput = document.getElementById('<%= txtFilterDateTo.ClientID %>');
            
            // Get current date range from server-side values
            var urlParams = new URLSearchParams(window.location.search);
            var dateFrom = urlParams.get('dateFrom');
            var dateTo = urlParams.get('dateTo');
            
            if (dateFrom && dateFromInput) dateFromInput.value = dateFrom;
            if (dateTo && dateToInput) dateToInput.value = dateTo;
        }
        
        function enableSmoothScrolling() {
            // Add smooth scrolling to all internal links
            document.querySelectorAll('a[href^="#"]').forEach(anchor => {
                anchor.addEventListener('click', function (e) {
                    e.preventDefault();
                    const target = document.querySelector(this.getAttribute('href'));
                    if (target) {
                        target.scrollIntoView({
                            behavior: 'smooth',
                            block: 'start'
                        });
                    }
                });
            });
        }
        
        // Show loading state during postbacks
        function showLoadingState() {
            var loadingHtml = '<div class="loading-container"><div class="loading-spinner"></div><p class="text-muted">Chargement des données...</p></div>';
            var tableContainer = document.querySelector('.table-container');
            if (tableContainer) {
                tableContainer.innerHTML = loadingHtml;
            }
        }
        
        // Print functionality
        function printPage() {
            window.print();
        }
        
        // Validate date range
        function validateDateRange() {
            var dateFrom = document.getElementById('<%= txtFilterDateFrom.ClientID %>').value;
            var dateTo = document.getElementById('<%= txtFilterDateTo.ClientID %>').value;
            
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
        
        // Show day-specific details in modal
        function showDayDetails(dateFormatted, dateValue) {
            var modal = new bootstrap.Modal(document.getElementById('dayDetailsModal'));
            var modalTitle = document.getElementById('modalDayDate');
            var modalContent = document.getElementById('dayDetailsContent');
            var loadingDiv = document.getElementById('dayDetailsLoading');
            
            // Set modal title
            modalTitle.textContent = dateFormatted;
            
            // Show loading
            loadingDiv.style.display = 'block';
            modalContent.innerHTML = '';
            
            // Show modal
            modal.show();
            
            // Load day details via AJAX
            loadDayDetails(dateValue, modalContent, loadingDiv);
        }
        
        // Load specific day details
        function loadDayDetails(dateValue, contentDiv, loadingDiv) {
            var employeeId = '<%= EmployeeId %>';
            
            // Make AJAX request to get day details
            var xhr = new XMLHttpRequest();
            xhr.open('POST', 'RaportEmploiyerPointage.aspx/GetDayDetails', true);
            xhr.setRequestHeader('Content-Type', 'application/json');
            xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
            
            xhr.onreadystatechange = function() {
                if (xhr.readyState === 4) {
                    loadingDiv.style.display = 'none';
                    
                    if (xhr.status === 200) {
                        try {
                            var response = JSON.parse(xhr.responseText);
                            if (response.d && response.d.length > 0) {
                                displayDayDetails(response.d, contentDiv);
                            } else {
                                contentDiv.innerHTML = '<div class=\"alert alert-info text-center\"><i class=\"fas fa-info-circle me-2\"></i>Aucun pointage trouvé pour cette journée.</div>';
                            }
                        } catch (e) {
                            contentDiv.innerHTML = '<div class=\"alert alert-danger\"><i class=\"fas fa-exclamation-triangle me-2\"></i>Erreur lors du chargement des données.</div>';
                        }
                    } else {
                        // Fallback: load from existing data
                        loadDayDetailsFromExistingData(dateValue, contentDiv);
                    }
                }
            };
            
            var requestData = JSON.stringify({
                employeeId: employeeId,
                date: dateValue
            });
            
            xhr.send(requestData);
        }
        
        // Display day details in modal
        function displayDayDetails(data, contentDiv) {
            var html = '<div class=\"day-details-table-container\">';
            html += '<div class=\"table-responsive\">';
            html += '<table class=\"table table-hover mb-0\">';
            html += '<thead class=\"table-dark sticky-header\">';
            html += '<tr>';
            html += '<th><i class=\"fas fa-calendar-alt me-1\"></i>Date</th>';
            html += '<th><i class=\"fas fa-clock me-1\"></i>Heure Exacte</th>';
            html += '<th><i class=\"fas fa-tag me-1\"></i>Type</th>';
            html += '<th><i class=\"fas fa-door-open me-1\"></i>Port</th>';
            html += '<th><i class=\"fas fa-comment me-1\"></i>Observation</th>';
            html += '</tr>';
            html += '</thead><tbody>';
            
            data.forEach(function(item, index) {
                var typeClass = item.Type === 'ENTREE' ? 'status-entree' : 'status-sortie';
                var typeIcon = item.Type === 'ENTREE' ? 'fa-sign-in-alt' : 'fa-sign-out-alt';
                var observation = item.Type === 'ENTREE' ? 'Arrivée au bureau' : 'Sortie du bureau';
                var rowClass = index % 2 === 0 ? 'table-light' : '';
                
                html += '<tr class=\"' + rowClass + '\">';
                html += '<td><strong class=\"text-info\">' + (item.DateFormatted || 'N/A') + '</strong><br><small class=\"text-muted\">' + (item.JourSemaine || '') + '</small></td>';
                html += '<td><span class=\"fw-bold text-primary\" style=\"font-size: 1.1em;\">' + (item.HeureFormatted || 'N/A') + '</span></td>';
                html += '<td><span class=\"badge ' + typeClass + ' fs-6\"><i class=\"fas ' + typeIcon + ' me-1\"></i>' + item.Type + '</span></td>';
                html += '<td><span class=\"text-secondary fw-semibold\">' + (item.Port || 'N/A') + '</span></td>';
                html += '<td><small class=\"text-muted fst-italic\">' + observation + '</small></td>';
                html += '</tr>';
            });
            
            html += '</tbody></table></div>';
            
            // Add scroll indicator if many items
            if (data.length > 8) {
                html += '<div class=\"scroll-indicator text-center mt-2\">';
                html += '<small class=\"text-muted\"><i class=\"fas fa-chevron-down me-1\"></i>Faites défiler pour voir tous les événements (' + data.length + ' total)</small>';
                html += '</div>';
            }
            
            html += '</div>';
            
            // Add detailed summary with times
            var entryCount = data.filter(function(item) { return item.Type === 'ENTREE'; }).length;
            var exitCount = data.filter(function(item) { return item.Type === 'SORTIE'; }).length;
            
            // Find first entry and last exit
            var entries = data.filter(function(item) { return item.Type === 'ENTREE'; });
            var exits = data.filter(function(item) { return item.Type === 'SORTIE'; });
            
            var firstEntry = entries.length > 0 ? entries[0].HeureFormatted : 'Aucune';
            var lastExit = exits.length > 0 ? exits[exits.length - 1].HeureFormatted : 'Aucune';
            
            html += '<div class=\"mt-4\">';
            html += '<div class=\"row\">';
            html += '<div class=\"col-md-3 text-center\">';
            html += '<div class=\"stat-card primary\" style=\"padding: 15px; margin: 5px;\">';
            html += '<div class=\"stat-icon text-primary\"><i class=\"fas fa-list-ol\"></i></div>';
            html += '<p class=\"stat-number text-primary\" style=\"font-size: 2rem;\">' + data.length + '</p>';
            html += '<p class=\"stat-label\">Total Pointages</p>';
            html += '</div></div>';
            
            html += '<div class=\"col-md-3 text-center\">';
            html += '<div class=\"stat-card success\" style=\"padding: 15px; margin: 5px;\">';
            html += '<div class=\"stat-icon text-success\"><i class=\"fas fa-sign-in-alt\"></i></div>';
            html += '<p class=\"stat-number text-success\" style=\"font-size: 2rem;\">' + entryCount + '</p>';
            html += '<p class=\"stat-label\">Entrées</p>';
            html += '<small class=\"text-muted\">Première: ' + firstEntry + '</small>';
            html += '</div></div>';
            
            html += '<div class=\"col-md-3 text-center\">';
            html += '<div class=\"stat-card danger\" style=\"padding: 15px; margin: 5px;\">';
            html += '<div class=\"stat-icon text-danger\"><i class=\"fas fa-sign-out-alt\"></i></div>';
            html += '<p class=\"stat-number text-danger\" style=\"font-size: 2rem;\">' + exitCount + '</p>';
            html += '<p class=\"stat-label\">Sorties</p>';
            html += '<small class=\"text-muted\">Dernière: ' + lastExit + '</small>';
            html += '</div></div>';
            
            html += '<div class=\"col-md-3 text-center\">';
            html += '<div class=\"stat-card warning\" style=\"padding: 15px; margin: 5px;\">';
            html += '<div class=\"stat-icon text-warning\"><i class=\"fas fa-clock\"></i></div>';
            html += '<p class=\"stat-number text-warning\" style=\"font-size: 1.5rem;\">';
            if (firstEntry !== 'Aucune' && lastExit !== 'Aucune') {
                html += calculateTimeDifference(firstEntry, lastExit);
            } else {
                html += 'N/A';
            }
            html += '</p>';
            html += '<p class=\"stat-label\">Temps Total</p>';
            html += '</div></div>';
            
            html += '</div></div>';
            
            contentDiv.innerHTML = html;
        }
        
        // Calculate time difference between first entry and last exit
        function calculateTimeDifference(startTime, endTime) {
            try {
                var start = new Date('2000-01-01 ' + startTime);
                var end = new Date('2000-01-01 ' + endTime);
                
                if (end < start) {
                    // Handle next day case
                    end = new Date('2000-01-02 ' + endTime);
                }
                
                var diffMs = end - start;
                var diffHours = Math.floor(diffMs / (1000 * 60 * 60));
                var diffMinutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));
                
                return diffHours + 'h' + (diffMinutes < 10 ? '0' : '') + diffMinutes;
            } catch (e) {
                return 'N/A';
            }
        }
        
        // Fallback: Load from existing GridView data
        function loadDayDetailsFromExistingData(dateValue, contentDiv) {
            var gridView = document.getElementById('<%= gvPointageDetails.ClientID %>');
            var dayData = [];
            
            if (gridView) {
                var rows = gridView.getElementsByTagName('tr');
                for (var i = 1; i < rows.length; i++) { // Skip header
                    var cells = rows[i].getElementsByTagName('td');
                    if (cells.length > 0) {
                        var rowDate = cells[0].textContent.trim();
                        var targetDateFormatted = dateValue.split('-').reverse().join('/');
                        
                        if (rowDate.includes(targetDateFormatted)) {
                            // Extract day name from the date
                            var dayNames = ['Dimanche', 'Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi', 'Samedi'];
                            var dateObj = new Date(dateValue);
                            var dayName = dayNames[dateObj.getDay()];
                            
                            dayData.push({
                                DateFormatted: rowDate,
                                HeureFormatted: cells[1].textContent.trim(),
                                Type: cells[2].textContent.trim().includes('ENTREE') ? 'ENTREE' : 'SORTIE',
                                Port: cells[3].textContent.trim(),
                                JourSemaine: dayName,
                                DateHeureComplete: rowDate + ' ' + cells[1].textContent.trim()
                            });
                        }
                    }
                }
            }
            
            if (dayData.length > 0) {
                displayDayDetails(dayData, contentDiv);
            } else {
                contentDiv.innerHTML = '<div class=\"alert alert-info text-center\"><i class=\"fas fa-info-circle me-2\"></i>Aucun pointage trouvé pour cette journée.</div>';
            }
        }
        
        // Print detailed report with all employee information and current filters
        function printDetailedReport() {
            try {
                // Get current filter values
                var dateFrom = document.getElementById('<%= txtFilterDateFrom.ClientID %>').value;
                var dateTo = document.getElementById('<%= txtFilterDateTo.ClientID %>').value;

                if (!dateFrom) dateFrom = '<%= DateFrom.ToString("yyyy-MM-dd") %>';
                if (!dateTo) dateTo = '<%= DateTo.ToString("yyyy-MM-dd") %>';

                // Get employee information
                var employeeName = '<%= HttpUtility.JavaScriptStringEncode(EmployeeName) %>';
                var matricule = '<%= Matricule %>';
                var portAssigne = '<%= HttpUtility.JavaScriptStringEncode(string.IsNullOrEmpty(PortFilter) ? "Tous les ports" : PortFilter) %>';

                // Convert dates for display
                var dateFromFormatted = formatDateForDisplay(dateFrom);
                var dateToFormatted = formatDateForDisplay(dateTo);

                // Open print window and create content directly
                var printWindow = window.open('', '_blank', 'width=800,height=600');
                createPrintContent(printWindow, employeeName, matricule, portAssigne, dateFromFormatted, dateToFormatted);

            } catch (error) {
                alert('Erreur lors de la génération du rapport: ' + error.message);
            }
        }

        // Create print content directly in the window
        function createPrintContent(printWindow, employeeName, matricule, portAssigne, dateFrom, dateTo) {
            var currentDate = new Date().toLocaleDateString('fr-FR', {
                weekday: 'long',
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });

            var doc = printWindow.document;
            doc.open();
            doc.write('<!DOCTYPE html>');
            doc.write('<html><head>');
            doc.write('<title>Rapport de Pointage - ' + employeeName + '</title>');
            doc.write('<meta charset="utf-8">');
            doc.write('<style>');
            doc.write('body { font-family: Arial, sans-serif; margin: 20px; }');
            doc.write('.header { text-align: center; border-bottom: 2px solid #007bff; padding-bottom: 20px; margin-bottom: 30px; }');
            doc.write('.company-name { font-size: 24px; font-weight: bold; color: #007bff; }');
            doc.write('.report-title { font-size: 18px; margin: 10px 0; }');
            doc.write('.employee-info { background: #f5f5f5; padding: 15px; margin-bottom: 20px; border-left: 4px solid #007bff; }');
            doc.write('.employee-info table { width: 100%; }');
            doc.write('.employee-info td { padding: 5px 10px; }');
            doc.write('.label { font-weight: bold; width: 25%; }');
            doc.write('.value { color: #007bff; }');
            doc.write('.summary-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 15px; margin: 20px 0; }');
            doc.write('.summary-item { text-align: center; padding: 15px; border: 1px solid #ddd; background: white; }');
            doc.write('.summary-number { font-size: 24px; font-weight: bold; margin-bottom: 5px; }');
            doc.write('.summary-label { font-size: 12px; color: #666; text-transform: uppercase; }');
            doc.write('.daily-card { border: 1px solid #ddd; margin-bottom: 15px; }');
            doc.write('.daily-header { background: #007bff; color: white; padding: 10px 15px; font-weight: bold; }');
            doc.write('.daily-content { padding: 15px; }');
            doc.write('.daily-stats { display: grid; grid-template-columns: repeat(6, 1fr); gap: 10px; }');
            doc.write('.daily-stat { text-align: center; padding: 8px; background: #f9f9f9; }');
            doc.write('.daily-stat-number { font-weight: bold; }');
            doc.write('.daily-stat-label { font-size: 10px; color: #666; }');
            doc.write('.footer { margin-top: 30px; padding-top: 15px; border-top: 1px solid #ddd; text-align: center; color: #666; font-size: 12px; }');
            doc.write('</style>');
            doc.write('</head><body>');

            doc.write('<div class="header">');
            doc.write('<div class="company-name">SYSTÈME DE GESTION RH</div>');
            doc.write('<div class="report-title">RAPPORT DÉTAILLÉ DE POINTAGE</div>');
            doc.write('</div>');

            doc.write('<div class="employee-info">');
            doc.write('<table>');
            doc.write('<tr><td class="label">Nom Complet:</td><td class="value">' + employeeName + '</td><td class="label">Matricule:</td><td class="value">' + matricule + '</td></tr>');
            doc.write('<tr><td class="label">Port Assigné:</td><td class="value">' + portAssigne + '</td><td class="label">Période:</td><td class="value">' + dateFrom + ' - ' + dateTo + '</td></tr>');
            doc.write('<tr><td class="label">Date d\'impression:</td><td class="value">' + currentDate + '</td><td class="label">Statut:</td><td class="value">Employé Actif</td></tr>');
            doc.write('</table>');
            doc.write('</div>');

            doc.write('<div id="summarySection"></div>');
            doc.write('<div id="dailyReportSection"></div>');

            doc.write('<div class="footer">');
            doc.write('<p>Ce rapport a été généré automatiquement par le système de gestion RH</p>');
            doc.write('<p>Confidentiel - Usage interne uniquement</p>');
            doc.write('</div>');

            doc.write('</body></html>');
            doc.close();

            // Populate data and print
            setTimeout(function () {
                populatePrintData(printWindow);
                setTimeout(function () {
                    printWindow.print();
                }, 300);
            }, 500);
        }

        // Helper function to format date for display
        function formatDateForDisplay(dateString) {
            var date = new Date(dateString);
            return date.toLocaleDateString('fr-FR', {
                weekday: 'long',
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            });
        }

        // Populate print data with current page information
        function populatePrintData(printWindow) {
            try {
                var printDoc = printWindow.document;

                // Calculate summary statistics from daily data instead of page literals
                var dailyCards = document.querySelectorAll('.daily-summary');
                var totalPointages = 0;
                var totalEntrees = 0;
                var totalSorties = 0;
                var totalHours = 0;

                console.log('Found daily cards:', dailyCards.length);

                dailyCards.forEach(function (card, index) {
                    console.log('Processing card', index);

                    // Get number of pointages from badge (more specific selector)
                    var nbPointagesElement = card.querySelector('.badge.bg-secondary');
                    if (!nbPointagesElement) {
                        nbPointagesElement = card.querySelector('.badge');
                    }
                    if (nbPointagesElement) {
                        var nbPointages = parseInt(nbPointagesElement.textContent) || 0;
                        totalPointages += nbPointages;
                        console.log('Day', index, 'pointages:', nbPointages);
                    }

                    // More specific selectors for entry and exit
                    var rowElements = card.querySelectorAll('.col-md-2');

                    // Check for première entrée (usually in second column)
                    if (rowElements[1]) {
                        var premiereEntreeElement = rowElements[1].querySelector('.text-success');
                        if (premiereEntreeElement && premiereEntreeElement.textContent.trim() !== '--:--' && premiereEntreeElement.textContent.trim() !== '') {
                            totalEntrees += 1;
                            console.log('Day', index, 'has entry:', premiereEntreeElement.textContent);
                        }
                    }

                    // Check for dernière sortie (usually in third column)  
                    if (rowElements[2]) {
                        var derniereSortieElement = rowElements[2].querySelector('.text-danger');
                        if (derniereSortieElement && derniereSortieElement.textContent.trim() !== '--:--' && derniereSortieElement.textContent.trim() !== '') {
                            totalSorties += 1;
                            console.log('Day', index, 'has exit:', derniereSortieElement.textContent);
                        }
                    }

                    // Get hours worked (usually in fourth column)
                    if (rowElements[3]) {
                        var heuresElement = rowElements[3].querySelector('.text-primary');
                        if (heuresElement) {
                            var heuresText = heuresElement.textContent;
                            // Extract number from text like "4.7h" or "4h30"
                            var hoursMatch = heuresText.match(/(\d+\.?\d*)/);
                            if (hoursMatch) {
                                var dayHours = parseFloat(hoursMatch[1]);
                                totalHours += dayHours;
                                console.log('Day', index, 'hours:', dayHours);
                            }
                        }
                    }
                });

                console.log('Totals - Pointages:', totalPointages, 'Entrées:', totalEntrees, 'Sorties:', totalSorties, 'Hours:', totalHours);

                // Fallback: if no data found from daily cards, try to get from page statistics
                if (totalPointages === 0) {
                    var litTotalPointages = document.getElementById('<%= litTotalPointages.ClientID %>');
                    var litTotalEntrees = document.getElementById('<%= litTotalEntrees.ClientID %>');
                    var litTotalSorties = document.getElementById('<%= litTotalSorties.ClientID %>');
                    var litHeuresTravaillees = document.getElementById('<%= litHeuresTravaillees.ClientID %>');

                    if (litTotalPointages && litTotalPointages.textContent) {
                        totalPointages = parseInt(litTotalPointages.textContent) || 0;
                    }
                    if (litTotalEntrees && litTotalEntrees.textContent) {
                        totalEntrees = parseInt(litTotalEntrees.textContent) || 0;
                    }
                    if (litTotalSorties && litTotalSorties.textContent) {
                        totalSorties = parseInt(litTotalSorties.textContent) || 0;
                    }
                    if (litHeuresTravaillees && litHeuresTravaillees.textContent) {
                        var hoursText = litHeuresTravaillees.textContent;
                        var hoursMatch = hoursText.match(/(\d+\.?\d*)/);
                        if (hoursMatch) {
                            totalHours = parseFloat(hoursMatch[1]);
                        }
                    }

                    console.log('Using fallback totals - Pointages:', totalPointages, 'Entrées:', totalEntrees, 'Sorties:', totalSorties, 'Hours:', totalHours);
                }

                // Format total hours
                var totalHoursFormatted = totalHours.toFixed(1) + 'h';

                // Create summary section with calculated values
                var summaryHtml =
                    '<div class="period-summary">' +
                    '<div class="summary-grid">' +
                    '<div class="summary-item primary">' +
                    '<div class="summary-number">' + totalPointages + '</div>' +
                    '<div class="summary-label">Total Pointages</div>' +
                    '</div>' +
                    '<div class="summary-item success">' +
                    '<div class="summary-number">' + totalEntrees + '</div>' +
                    '<div class="summary-label">Total Entrées</div>' +
                    '</div>' +
                    '<div class="summary-item danger">' +
                    '<div class="summary-number">' + totalSorties + '</div>' +
                    '<div class="summary-label">Total Sorties</div>' +
                    '</div>' +
                    '<div class="summary-item warning">' +
                    '<div class="summary-number">' + totalHoursFormatted + '</div>' +
                    '<div class="summary-label">Heures Travaillées</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>';

                // Insert summary
                var summarySection = printDoc.getElementById('summarySection');
                if (summarySection) {
                    summarySection.innerHTML = summaryHtml;
                }

                // Get daily summary data
                var dailyReportHtml = '<div class="daily-section"><h3 class="section-title">Résumé Journalier Détaillé</h3>';

                // Extract daily data from the repeater
                var dailyCards = document.querySelectorAll('.daily-summary');
                if (dailyCards.length > 0) {
                    dailyCards.forEach(function (card) {
                        var dateText = card.querySelector('h6') ? card.querySelector('h6').textContent : 'Date inconnue';
                        var dayName = card.querySelector('small.text-muted') ? card.querySelector('small.text-muted').textContent : '';
                        var premiereEntree = card.querySelector('.text-success') ? card.querySelector('.text-success').textContent : '--:--';
                        var derniereSortie = card.querySelector('.text-danger') ? card.querySelector('.text-danger').textContent : '--:--';
                        var heuresJour = card.querySelector('.text-primary') ? card.querySelector('.text-primary').textContent : '0h00';
                        var nbPointages = card.querySelector('.badge') ? card.querySelector('.badge').textContent : '0';
                        var status = card.querySelector('.col-md-2:last-child .text-muted') ? card.querySelector('.col-md-2:last-child .text-muted').textContent : 'N/A';

                        dailyReportHtml += `
                        <div class="daily-card">
                            <div class="daily-header">${dateText} - ${dayName}</div>
                            <div class="daily-content">
                                <div class="daily-stats">
                                    <div class="daily-stat">
                                        <div class="daily-stat-number success">${premiereEntree}</div>
                                        <div class="daily-stat-label">Première Entrée</div>
                                    </div>
                                    <div class="daily-stat">
                                        <div class="daily-stat-number danger">${derniereSortie}</div>
                                        <div class="daily-stat-label">Dernière Sortie</div>
                                    </div>
                                    <div class="daily-stat">
                                        <div class="daily-stat-number primary">${heuresJour}</div>
                                        <div class="daily-stat-label">Heures Travaillées</div>
                                    </div>
                                    <div class="daily-stat">
                                        <div class="daily-stat-number">${nbPointages}</div>
                                        <div class="daily-stat-label">Nb Pointages</div>
                                    </div>
                                    <div class="daily-stat">
                                        <div class="daily-stat-number">${status}</div>
                                        <div class="daily-stat-label">Statut Jour</div>
                                    </div>
                                    <div class="daily-stat">
                                        <div class="daily-stat-number">✓</div>
                                        <div class="daily-stat-label">Vérifié</div>
                                    </div>
                                </div>
                            </div>
                        </div>`;
                    });
                } else {
                    dailyReportHtml += '<div class="no-data">Aucune donnée de pointage disponible pour la période sélectionnée.</div>';
                }

                dailyReportHtml += '</div>';

                // Insert daily report
                var dailyReportSection = printDoc.getElementById('dailyReportSection');
                if (dailyReportSection) {
                    dailyReportSection.innerHTML = dailyReportHtml;
                }

            } catch (error) {
                console.log('Erreur lors de la population des données d\'impression:', error);
            }
        }

        // Add validation to filter button
        document.addEventListener('DOMContentLoaded', function () {
            var filterBtn = document.getElementById('<%= btnFilterApply.ClientID %>');
            if (filterBtn) {
                filterBtn.addEventListener('click', function (e) {
                    if (!validateDateRange()) {
                        e.preventDefault();
                    } else {
                        showLoadingState();
                    }
                });
            }
        });
    </script>
</asp:Content>



