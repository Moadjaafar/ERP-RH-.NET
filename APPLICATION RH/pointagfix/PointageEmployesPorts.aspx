<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PointageEmployesPorts.aspx.cs" Inherits="APPLICATION_RH.pointagfix.PointageEmployesPorts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .status-badge-in {
            background-color: #28a745 !important;
        }

        .status-badge-out {
            background-color: #dc3545 !important;
        }

        .hours-summary {
            font-weight: bold;
            color: #007bff;
        }

        .quick-action-btn {
            min-width: 70px;
            margin: 1px;
        }

        .btn-group .btn {
            border-radius: 0;
        }

            .btn-group .btn:first-child {
                border-top-left-radius: 0.25rem;
                border-bottom-left-radius: 0.25rem;
            }

            .btn-group .btn:last-child {
                border-top-right-radius: 0.25rem;
                border-bottom-right-radius: 0.25rem;
            }

        .edit-mode {
            background-color: #fff3cd;
        }

        .present-employees {
            border-left: 4px solid #28a745;
        }

        .absent-employees {
            border-left: 4px solid #dc3545;
        }

        .toast-container {
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
        }

        .toast {
            min-width: 300px;
            margin-bottom: 10px;
            padding: 15px;
            border-radius: 5px;
            color: white;
            opacity: 0;
            transform: translateX(100%);
            transition: all 0.3s ease;
        }

            .toast.show {
                opacity: 1;
                transform: translateX(0);
            }

            .toast.success {
                background-color: #28a745;
            }

            .toast.error {
                background-color: #dc3545;
            }

        .employee-count-badge {
            font-size: 0.9em;
            margin-left: 10px;
        }

        .grid-container {
            max-height: 400px;
            overflow-y: auto;
            border: 1px solid #dee2e6;
            border-radius: 0.375rem;
        }

        /* Search Styles */
        .search-highlight {
            background-color: #fff3cd !important;
            font-weight: bold;
        }

        .search-input {
            border: 2px solid #e9ecef;
            transition: all 0.3s;
        }

            .search-input:focus {
                border-color: #007bff;
                box-shadow: 0 0 0 0.2rem rgba(0,123,255,.25);
            }

        .search-counter {
            font-size: 0.85em;
            color: #6c757d;
        }

        .no-results {
            text-align: center;
            padding: 20px;
            color: #6c757d;
        }

        .employee-panel {
            transition: all 0.3s ease;
        }

        .btn-group .btn {
            border-radius: 0.375rem !important;
            margin-right: 2px;
        }

            .btn-group .btn.active {
                box-shadow: 0 0 0 0.2rem rgba(0,123,255,.25);
            }

        .card-header .btn-group {
            flex-grow: 1;
        }

        .search-counter {
            font-size: 0.75em;
        }

        .time-input-container {
            position: relative;
        }

        .time-highlight {
            background-color: #fff3cd !important;
            border: 2px solid #ffc107 !important;
        }

        .time-display {
            font-size: 1.2em;
            font-weight: bold;
            color: #007bff;
        }

        .date-highlight {
            background-color: #e3f2fd !important;
            border: 2px solid #2196f3 !important;
        }

        .today-indicator {
            color: #28a745;
            font-weight: bold;
        }

        .past-date-indicator {
            color: #ffc107;
            font-weight: bold;
        }

        .datetime-display {
            font-size: 0.85em;
            line-height: 1.2;
        }

        .datetime-display .date-part {
            display: block;
            color: #6c757d;
            font-size: 0.8em;
        }

        .datetime-display .time-part {
            display: block;
            font-weight: 500;
            color: #495057;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Toast Container -->
        <div class="toast-container" id="toastContainer"></div>

        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Pointage des Employés aux Ports</h4>
            </div>
            <div class="text-end">
                <ol class="breadcrumb m-0 py-0">
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlDashboard" runat="server" NavigateUrl="~/Default.aspx">Dashboard</asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlPointage" runat="server" NavigateUrl="~/pointage/GestionEquipes.aspx">Gestion des Équipes</asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item active">Pointage aux Ports</li>
                </ol>
            </div>
        </div>

        <!-- Control Panel -->
        <div class="row">
            <div class="col-lg-3">
                <div class="mb-3">
                    <asp:Label ID="lblPort" runat="server" Text="Port *" CssClass="form-label" AssociatedControlID="ddlPort" />
                    <asp:DropDownList ID="ddlPort" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlPort_SelectedIndexChanged">
                    </asp:DropDownList>
                </div>
            </div>
            <div class="col-lg-3">
                <div class="mb-3">
                    <asp:Label ID="lblPointageDate" runat="server" Text="Date de Pointage" CssClass="form-label" AssociatedControlID="txtPointageDate" />
                    <div class="input-group">
                        <span class="input-group-text"><i class="fas fa-calendar"></i></span>
                        <asp:TextBox ID="txtPointageDate" runat="server" CssClass="form-control" TextMode="Date" AutoPostBack="true" OnTextChanged="txtPointageDate_TextChanged" />
                        <asp:Button ID="btnSetToday" runat="server" Text="Aujourd'hui" CssClass="btn btn-outline-primary" OnClick="btnSetToday_Click" CausesValidation="false" />
                    </div>
                    <small class="text-muted">
                        <asp:Literal ID="litDateStatus" runat="server" />
                    </small>
                </div>
            </div>
            <div class="col-lg-3">
                <div class="mb-3">
                    <asp:Label ID="lblPointageTime" runat="server" Text="Heure de Pointage" CssClass="form-label" AssociatedControlID="txtPointageTime" />
                    <div class="input-group">
                        <span class="input-group-text"><i class="fas fa-clock"></i></span>
                        <asp:TextBox ID="txtPointageTime" runat="server" CssClass="form-control" TextMode="Time" />
                        <asp:Button ID="btnSetCurrentTime" runat="server" Text="Maintenant" CssClass="btn btn-outline-secondary" OnClick="btnSetCurrentTime_Click" CausesValidation="false" />
                    </div>
                    <small class="text-muted">Format 24h (HH:mm)</small>
                </div>
            </div>
            <div class="col-lg-3">
                <div class="mb-3">
                    <asp:Label ID="lblMatricule" runat="server" Text="Recherche Employé" CssClass="form-label" AssociatedControlID="txtMatricule" />
                    <div class="input-group">
                        <asp:TextBox ID="txtMatricule" runat="server" CssClass="form-control" placeholder="Nom ou prénom employé..." />
                        <asp:Button ID="btnSearchEmployee" runat="server" Text="Chercher" CssClass="btn btn-primary" OnClick="btnSearchEmployee_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Employee Management Panel -->
        <asp:Panel ID="pnlEmployeePointage" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card">
                    <!-- Toggle Buttons Header -->
                    <div class="card-header bg-light">
                        <div class="d-flex justify-content-between align-items-center">
                            <div class="btn-group" role="group">
                                <button type="button" id="btnShowPresent" class="btn btn-success active" onclick="toggleEmployeeView('present')">
                                    <i class="fas fa-user-check me-2"></i>Employé dans l'usine
                                    <span class="badge bg-light text-success ms-2">
                                        <span id="presentTotal">
                                            <asp:Literal ID="litPresentCount" runat="server" /></span>
                                        <span id="presentFiltered" class="search-counter" style="display: none;"></span>
                                    </span>
                                </button>
                                <button type="button" id="btnShowAbsent" class="btn btn-outline-danger" onclick="toggleEmployeeView('absent')">
                                    <i class="fas fa-user-times me-2"></i>Employés hors de l'usine
                                    <span class="badge bg-light text-danger ms-2">
                                        <span id="absentTotal">
                                            <asp:Literal ID="litAbsentCount" runat="server" /></span>
                                        <span id="absentFiltered" class="search-counter" style="display: none;"></span>
                                    </span>
                                </button>
                            </div>
                            <div>
                                <asp:Button ID="btnRefreshPresent" runat="server" Text="↻" CssClass="btn btn-outline-secondary btn-sm"
                                    OnClick="btnRefreshEmployees_Click" CausesValidation="false" ToolTip="Actualiser" />
                            </div>
                        </div>
                    </div>

                    <!-- Present Employees Panel -->
                    <div id="presentEmployeesPanel" class="employee-panel">
                        <div class="card-header bg-success text-white">
                            <div class="d-flex justify-content-between align-items-center">
                                <h5 class="card-title mb-0">
                                    <i class="fas fa-user-check me-2"></i>Employé dans l'usine
                                </h5>
                                <!-- Live Search Input -->
                                <div class="ms-auto" style="width: 300px;">
                                    <div class="input-group input-group-sm">
                                        <span class="input-group-text"><i class="fas fa-search"></i></span>
                                        <input type="text" id="searchPresent" class="form-control search-input"
                                            placeholder="Rechercher présents..." onkeyup="searchEmployees('present')" autocomplete="off">
                                        <button type="button" class="btn btn-outline-light" onclick="clearSearch('present')">
                                            <i class="fas fa-times"></i>
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-body p-0">
                            <div class="grid-container">
                                <asp:GridView ID="gvEmployeesPresent" runat="server" CssClass="table table-striped table-hover mb-0"
                                    AutoGenerateColumns="false" OnRowCommand="gvEmployeesPresent_RowCommand" EmptyDataText="Aucun employé dans l'usine">
                                    <Columns>
                                        <asp:BoundField DataField="Matricule" HeaderText="Matricule" />
                                        <asp:BoundField DataField="NomComplet" HeaderText="Nom Complet" />
                                        <asp:TemplateField HeaderText="Dernière Entrée">
                                            <ItemTemplate>
                                                <div class="datetime-display">
                                                    <%# FormatDateTime(Eval("DerniereDateHeure")) %>
                                                </div>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Heures">
                                            <ItemTemplate>
                                                <span class="hours-summary"><%# Eval("HeuresJour") %></span>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Actions">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="btnSortie" runat="server" CommandName="PointageSortie"
                                                    CommandArgument='<%# Eval("IdEmploye") %>' CssClass="btn btn-sm btn-danger"
                                                    OnClientClick='<%# "return confirmPointage(\"SORTIE\", \"" + Eval("NomComplet") + "\");" %>'>
                                                    <i class="fas fa-sign-out-alt"></i> Sortie
                                                </asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                            <!-- No results message -->
                            <div id="noResultsPresent" class="no-results" style="display: none;">
                                <i class="fas fa-search text-muted"></i>
                                <br>
                                Aucun employé présent trouvé
                            </div>
                        </div>
                    </div>

                    <!-- Absent Employees Panel -->
                    <div id="absentEmployeesPanel" class="employee-panel" style="display: none;">
                        <div class="card-header bg-danger text-white">
                            <div class="d-flex justify-content-between align-items-center">
                                <h5 class="card-title mb-0">
                                    <i class="fas fa-user-times me-2"></i>Employés hors de l'usine
                                </h5>
                                <!-- Live Search Input -->
                                <div class="ms-auto" style="width: 300px;">
                                    <div class="input-group input-group-sm">
                                        <span class="input-group-text"><i class="fas fa-search"></i></span>
                                        <input type="text" id="searchAbsent" class="form-control search-input"
                                            placeholder="Rechercher absents..." onkeyup="searchEmployees('absent')" autocomplete="off">
                                        <button type="button" class="btn btn-outline-light" onclick="clearSearch('absent')">
                                            <i class="fas fa-times"></i>
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card-body p-0">
                            <div class="grid-container">
                                <asp:GridView ID="gvEmployeesAbsent" runat="server" CssClass="table table-striped table-hover mb-0"
                                    AutoGenerateColumns="false" OnRowCommand="gvEmployeesAbsent_RowCommand" EmptyDataText="Tous les employés sont dans l'usine">
                                    <Columns>
                                        <asp:BoundField DataField="Matricule" HeaderText="Matricule" />
                                        <asp:BoundField DataField="NomComplet" HeaderText="Nom Complet" />
                                        <asp:TemplateField HeaderText="Dernière Sortie">
                                            <ItemTemplate>
                                                <div class="datetime-display">
                                                    <%# FormatDateTime(Eval("DerniereDateHeure")) %>
                                                </div>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Heures">
                                            <ItemTemplate>
                                                <span class="hours-summary"><%# Eval("HeuresJour") %></span>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Actions">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="btnEntree" runat="server" CommandName="PointageEntree"
                                                    CommandArgument='<%# Eval("IdEmploye") %>' CssClass="btn btn-sm btn-success"
                                                    OnClientClick='<%# "return confirmPointage(\"ENTREE\", \"" + Eval("NomComplet") + "\");" %>'>
                                                    <i class="fas fa-sign-in-alt"></i> Entrée
                                                </asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                            <!-- No results message -->
                            <div id="noResultsAbsent" class="no-results" style="display: none;">
                                <i class="fas fa-search text-muted"></i>
                                <br>
                                Aucun employé absent trouvé
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Quick Employee Search Result -->
        <asp:Panel ID="pnlQuickEmployee" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-user me-2"></i>Employé Trouvé
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row align-items-center">
                            <div class="col-md-3">
                                <strong>Matricule:</strong>
                                <asp:Literal ID="litQuickMatricule" runat="server" />
                            </div>
                            <div class="col-md-3">
                                <strong>Nom:</strong>
                                <asp:Literal ID="litQuickNom" runat="server" />
                            </div>
                            <div class="col-md-3">
                                <strong>Statut:</strong>
                                <asp:Literal ID="litQuickStatut" runat="server" />
                            </div>
                            <div class="col-md-3">
                                <asp:Button ID="btnQuickEntree" runat="server" Text="Entrée" CssClass="btn btn-success me-2" OnClick="btnQuickEntree_Click" />
                                <asp:Button ID="btnQuickSortie" runat="server" Text="Sortie" CssClass="btn btn-danger" OnClick="btnQuickSortie_Click" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Daily Summary -->
        <asp:Panel ID="pnlDailySummary" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-chart-bar me-2"></i>Résumé du 
                            <asp:Literal ID="litSelectedDate" runat="server" />
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-3">
                                <div class="card border-primary">
                                    <div class="card-body text-center">
                                        <h3 class="text-primary">
                                            <asp:Literal ID="litTotalPresents" runat="server" /></h3>
                                        <p class="mb-0">Employés Présents</p>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="card border-success">
                                    <div class="card-body text-center">
                                        <h3 class="text-success">
                                            <asp:Literal ID="litTotalEntrees" runat="server" /></h3>
                                        <p class="mb-0">Total Entrées</p>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="card border-danger">
                                    <div class="card-body text-center">
                                        <h3 class="text-danger">
                                            <asp:Literal ID="litTotalSorties" runat="server" /></h3>
                                        <p class="mb-0">Total Sorties</p>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="card border-info">
                                    <div class="card-body text-center">
                                        <h3 class="text-info">
                                            <asp:Literal ID="litMoyenneHeures" runat="server" /></h3>
                                        <p class="mb-0">Heures Moyenne</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        // Simple and Working Live Search - Simplified for Present/Absent only
        function searchEmployees(type) {
            var searchValue = document.getElementById('search' + getCapitalizedType(type)).value.toLowerCase();
            var gridId = getGridId(type);

            var grid = document.getElementById(gridId);
            if (!grid) return;

            var tbody = grid.getElementsByTagName('tbody')[0];
            if (!tbody) return;

            var rows = tbody.getElementsByTagName('tr');
            var visibleCount = 0;
            var totalCount = rows.length;

            // Remove existing highlights
            clearHighlights(tbody);

            for (var i = 0; i < rows.length; i++) {
                var row = rows[i];
                var cells = row.getElementsByTagName('td');
                var found = false;

                if (searchValue === '') {
                    found = true;
                } else {
                    // Search in first 4 columns (Matricule, Nom, Time, Hours)
                    for (var j = 0; j < Math.min(cells.length, 4); j++) {
                        var cellText = (cells[j].innerText || cells[j].textContent || '').toLowerCase();
                        if (cellText.indexOf(searchValue) !== -1) {
                            found = true;
                            // Highlight matching text
                            highlightText(cells[j], searchValue);
                        }
                    }
                }

                if (found) {
                    row.style.display = '';
                    visibleCount++;
                } else {
                    row.style.display = 'none';
                }
            }

            // Update counters
            updateCounter(type, visibleCount, totalCount, searchValue);

            // Show/hide no results message
            showNoResults(type, visibleCount > 0 || searchValue === '');
        }

        function getCapitalizedType(type) {
            switch (type) {
                case 'present': return 'Present';
                case 'absent': return 'Absent';
                default: return 'Present';
            }
        }

        function getGridId(type) {
            switch (type) {
                case 'present': return '<%= gvEmployeesPresent.ClientID %>';
                case 'absent': return '<%= gvEmployeesAbsent.ClientID %>';
                default: return '<%= gvEmployeesPresent.ClientID %>';
            }
        }

        function clearHighlights(container) {
            var highlights = container.querySelectorAll('.search-highlight');
            for (var i = 0; i < highlights.length; i++) {
                var highlight = highlights[i];
                var parent = highlight.parentNode;
                parent.replaceChild(document.createTextNode(highlight.textContent), highlight);
                parent.normalize();
            }
        }

        function highlightText(cell, searchValue) {
            if (!searchValue) return;

            var text = cell.textContent || cell.innerText;
            var regex = new RegExp('(' + searchValue.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + ')', 'gi');
            var highlightedText = text.replace(regex, '<span class="search-highlight">$1</span>');

            if (highlightedText !== text) {
                cell.innerHTML = highlightedText;
            }
        }

        function updateCounter(type, visible, total, searchValue) {
            var totalSpan = document.getElementById(type + 'Total');
            var filteredSpan = document.getElementById(type + 'Filtered');

            if (totalSpan && filteredSpan) {
                if (searchValue === '') {
                    totalSpan.style.display = '';
                    filteredSpan.style.display = 'none';
                } else {
                    totalSpan.style.display = 'none';
                    filteredSpan.style.display = '';
                    filteredSpan.textContent = visible + '/' + total;
                }
            }
        }

        function showNoResults(type, hasResults) {
            var noResultsDiv = document.getElementById('noResults' + getCapitalizedType(type));
            if (noResultsDiv) {
                noResultsDiv.style.display = hasResults ? 'none' : 'block';
            }
        }

        function clearSearch(type) {
            var searchInput = document.getElementById('search' + getCapitalizedType(type));
            if (searchInput) {
                searchInput.value = '';
                searchInput.focus();
                searchEmployees(type);
            }
        }

        // Toast notifications
        function showToast(type, message) {
            var container = document.getElementById('toastContainer');
            var toast = document.createElement('div');
            toast.className = 'toast ' + type;
            toast.innerHTML = '<div style="display: flex; justify-content: space-between; align-items: center;"><span>' + message + '</span><button onclick="this.parentElement.parentElement.remove()" style="background:none;border:none;color:white;font-size:18px;cursor:pointer;">&times;</button></div>';

            container.appendChild(toast);
            setTimeout(function () { toast.classList.add('show'); }, 100);
            setTimeout(function () {
                toast.classList.remove('show');
                setTimeout(function () { if (toast.parentElement) toast.parentElement.removeChild(toast); }, 300);
            }, 5000);
        }

        function confirmPointage(action, employee) {
            return confirm('Confirmer ' + action + ' pour ' + employee + '?');
        }

        // Keyboard shortcuts - Simplified
        document.addEventListener('keydown', function (e) {
            if (e.ctrlKey && e.key === 'f') {
                e.preventDefault();
                var presentSearch = document.getElementById('searchPresent');
                if (presentSearch) {
                    presentSearch.focus();
                    presentSearch.select();
                }
            }
        });

        // Auto-refresh (simplified - no recent search check)
        setInterval(function () {
            var presentSearch = document.getElementById('searchPresent');
            var absentSearch = document.getElementById('searchAbsent');

            if ((!presentSearch || presentSearch.value === '') &&
                (!absentSearch || absentSearch.value === '')) {
                var refreshBtn = document.getElementById('<%= btnRefreshPresent.ClientID %>');
                if (refreshBtn) {
                    __doPostBack('<%= btnRefreshPresent.UniqueID %>', '');
                }
            }
        }, 60000);

        // Initialize search functionality on page load
        document.addEventListener('DOMContentLoaded', function () {
            // Clear any existing search values
            var searchInputs = ['searchPresent', 'searchAbsent'];
            searchInputs.forEach(function (inputId) {
                var input = document.getElementById(inputId);
                if (input) {
                    input.value = '';
                }
            });
        });

        // Toggle between Present and Absent employee views
        function toggleEmployeeView(viewType) {
            var presentPanel = document.getElementById('presentEmployeesPanel');
            var absentPanel = document.getElementById('absentEmployeesPanel');
            var btnPresent = document.getElementById('btnShowPresent');
            var btnAbsent = document.getElementById('btnShowAbsent');

            if (viewType === 'present') {
                // Show Present panel
                presentPanel.style.display = 'block';
                absentPanel.style.display = 'none';

                // Update button states
                btnPresent.className = 'btn btn-success active';
                btnAbsent.className = 'btn btn-outline-danger';

                // Clear search in absent panel
                clearSearch('absent');

            } else if (viewType === 'absent') {
                // Show Absent panel
                presentPanel.style.display = 'none';
                absentPanel.style.display = 'block';

                // Update button states
                btnPresent.className = 'btn btn-outline-success';
                btnAbsent.className = 'btn btn-danger active';

                // Clear search in present panel
                clearSearch('present');
            }

            // Store current view in localStorage for persistence
            localStorage.setItem('currentEmployeeView', viewType);
        }

        // Initialize view on page load
        document.addEventListener('DOMContentLoaded', function () {
            // Get saved view preference or default to 'present'
            var savedView = localStorage.getItem('currentEmployeeView') || 'present';
            toggleEmployeeView(savedView);
        });

        // Keyboard shortcut to toggle views
        document.addEventListener('keydown', function (e) {
            if (e.ctrlKey && e.key === 't') {
                e.preventDefault();
                var currentView = localStorage.getItem('currentEmployeeView') || 'present';
                var newView = currentView === 'present' ? 'absent' : 'present';
                toggleEmployeeView(newView);
            }
        });
    </script>
</asp:Content>


