<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PointageProduction.aspx.cs" Inherits="APPLICATION_RH.pointage.PointageProduction" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .status-badge {
            font-size: 0.85em;
            padding: 0.25rem 0.5rem;
        }

        .time-input {
            width: 80px;
        }

        .pointage-row {
            border-left: 4px solid #e9ecef;
        }

            .pointage-row.present {
                border-left-color: #28a745;
            }

            .pointage-row.absent {
                border-left-color: #dc3545;
            }

            .pointage-row.partial {
                border-left-color: #ffc107;
            }

        .table-responsive-custm {
            max-height: calc(100vh - 350px) !important;
            overflow-y: auto;
        }

        .table tbody tr:hover {
            background-color: #f8f9fa;
        }

        .table-responsive-custm .table tbody tr:nth-of-type(1) {
            background-color: #f8f9fa;
            border-bottom: 2px solid #dee2e6;
            position: sticky;
            top: 0;
            z-index: 10;
        }

        .search-section {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 10px;
            padding: 20px;
            margin-bottom: 20px;
        }

        .search-section .form-label {
            color: white;
            font-weight: 500;
        }

        .search-section .form-select {
            border: none;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        .employee-count-badge {
            background: rgba(255,255,255,0.2);
            color: white;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 0.9em;
            backdrop-filter: blur(10px);
        }

        .filters-summary {
            background: rgba(255,255,255,0.1);
            border-radius: 8px;
            padding: 10px 15px;
            margin-top: 15px;
        }

        .filters-summary .badge {
            background: rgba(255,255,255,0.3);
            color: white;
            margin-right: 8px;
            margin-bottom: 4px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Pointage Production</h4>
                <p class="text-muted mb-0">Gestion du pointage par unités de production</p>
            </div>
            <div class="text-end">
                <ol class="breadcrumb m-0 py-0">
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlDashboard" runat="server" NavigateUrl="~/Default.aspx">
                            Dashboard
                        </asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlPointage" runat="server" NavigateUrl="~/pointage/">
                            Pointage
                        </asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item active">Production</li>
                </ol>
            </div>
        </div>

        <!-- Search Filters Section -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-search me-2"></i>Filtres de Production
                        </h5>
                    </div>
                    <div class="card-body search-section">
                        <div class="row">
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblSociete" runat="server" Text="Société *" CssClass="form-label" AssociatedControlID="ddlSociete" />
                                    <asp:DropDownList ID="ddlSociete" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlSociete_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Sélectionner Société --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblDepartement" runat="server" Text="Département" CssClass="form-label" AssociatedControlID="ddlDepartement" />
                                    <asp:DropDownList ID="ddlDepartement" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlDepartement_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Tous les Départements --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblService" runat="server" Text="Service" CssClass="form-label" AssociatedControlID="ddlService" />
                                    <asp:DropDownList ID="ddlService" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlService_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Tous les Services --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblCellule" runat="server" Text="Cellule" CssClass="form-label" AssociatedControlID="ddlCellule" />
                                    <asp:DropDownList ID="ddlCellule" runat="server" CssClass="form-select">
                                        <asp:ListItem Value="" Text="-- Toutes les Cellules --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Date and Shift Selection -->
                        <div class="row">
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblDate" runat="server" Text="Date *" CssClass="form-label" AssociatedControlID="txtDatePointage" />
                                    <asp:TextBox ID="txtDatePointage" runat="server"
                                        CssClass="form-control" TextMode="Date" />
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblShift" runat="server" Text="Shift *" CssClass="form-label" AssociatedControlID="ddlShift" />
                                    <asp:DropDownList ID="ddlShift" runat="server" CssClass="form-select">
                                        <asp:ListItem Value="JOUR" Selected="True">JOUR</asp:ListItem>
                                        <asp:ListItem Value="NUIT">NUIT</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-6 d-flex align-items-end">
                                <div class="mb-3 w-100">
                                    <asp:Button ID="btnSearch" runat="server" Text="Charger Employés" 
                                        CssClass="btn btn-light me-2" OnClick="btnSearch_Click" />
                                    <asp:Button ID="btnClearFilters" runat="server" Text="Effacer Filtres" 
                                        CssClass="btn btn-outline-light" OnClick="btnClearFilters_Click" CausesValidation="false" />
                                    <span class="employee-count-badge ms-3">
                                        <i class="fas fa-users me-1"></i>
                                        <asp:Literal ID="litEmployeeCount" runat="server" Text="0" /> employé(s) trouvé(s)
                                    </span>
                                </div>
                            </div>
                        </div>

                        <!-- Active Filters Summary -->
                        <asp:Panel ID="pnlFiltersSummary" runat="server" Visible="false" CssClass="filters-summary">
                            <div class="d-flex flex-wrap align-items-center">
                                <small class="text-white-50 me-2">Filtres actifs:</small>
                                <asp:Literal ID="litActiveFilters" runat="server" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>

        <!-- No Results Message -->
        <asp:Panel ID="pnlNoResults" runat="server" Visible="false" CssClass="row">
            <div class="col-12">
                <div class="alert alert-info text-center">
                    <i class="fas fa-search fa-2x mb-2"></i>
                    <h5>Aucun employé trouvé</h5>
                    <p class="mb-0">Veuillez ajuster vos critères de production et réessayer.</p>
                </div>
            </div>
        </asp:Panel>

        <!-- Pointage Grid -->
        <asp:Panel ID="pnlPointage" runat="server" Visible="false" CssClass="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header d-lg-flex d-block justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-user-check me-2"></i>Pointage du 
                            <asp:Literal ID="litDatePointage" runat="server" />
                            - 
                            <asp:Literal ID="litShift" runat="server" />
                        </h5>
                        <div>
                            <asp:Button ID="btnTousPresents" runat="server" Text="Tous Présents"
                                CssClass="btn btn-success me-2" OnClientClick="markAllPresent(); return false;" />
                            <asp:Button ID="btnSauvegarder" runat="server" Text="Sauvegarder"
                                CssClass="btn btn-primary" OnClick="btnSauvegarder_Click" />
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="table-responsive table-responsive-custm">
                            <asp:GridView ID="gvPointage" runat="server"
                                CssClass="table table-striped table-hover"
                                AutoGenerateColumns="false"
                                EmptyDataText="Aucun employé trouvé dans cette unité de production"
                                OnRowDataBound="gvPointage_RowDataBound">
                                <Columns>
                                    <asp:BoundField DataField="Matricule" HeaderText="Matricule" />
                                    <asp:BoundField DataField="NomComplet" HeaderText="Nom Complet" />
                                    <asp:BoundField DataField="SocieteNom" HeaderText="Société" />
                                    <asp:BoundField DataField="DepartementNom" HeaderText="Département" />
                                    <asp:BoundField DataField="ServiceNom" HeaderText="Service" />
                                    <asp:BoundField DataField="CelluleNom" HeaderText="Cellule" />
                                    <asp:TemplateField HeaderText="Présent">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkPresent" runat="server"
                                                Checked='<%# Eval("StatutPresence").ToString() == "P" %>'
                                                CssClass="form-check-input"
                                                onclick="handleStatusChange(this, 'P')" />
                                            <asp:HiddenField ID="hdnIdEmploye" runat="server" Value='<%# Eval("IdEmploye") %>' />
                                            <asp:HiddenField ID="hdnIdPointage" runat="server" Value='<%# Eval("IdPointage") %>' />
                                            <asp:HiddenField ID="hdnStatutPresence" runat="server" Value='<%# Eval("StatutPresence") %>' />
                                        </ItemTemplate>
                                        <HeaderStyle CssClass="text-center" />
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Absent">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkAbsent" runat="server"
                                                Checked='<%# Eval("StatutPresence").ToString() == "A" %>'
                                                CssClass="form-check-input"
                                                onclick="handleStatusChange(this, 'A')" />
                                        </ItemTemplate>
                                        <HeaderStyle CssClass="text-center" />
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Demi-journée">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkDemiJournee" runat="server"
                                                Checked='<%# Eval("StatutPresence").ToString() == "P/2" %>'
                                                CssClass="form-check-input"
                                                onclick="handleStatusChange(this, 'P/2')" />
                                        </ItemTemplate>
                                        <HeaderStyle CssClass="text-center" />
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Motif Absence">
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlMotifAbsence" runat="server"
                                                CssClass="form-select form-select-sm motif-absence-dropdown"
                                                Style='<%# Eval("StatutPresence").ToString() == "A" ? "display:block;": "display:none;" %>'>
                                                <asp:ListItem Value="" Text="-- Sélectionner --" />
                                                <asp:ListItem Value="Maladie" Text="Maladie" />
                                                <asp:ListItem Value="Congé" Text="Congé" />
                                                <asp:ListItem Value="Formation" Text="Formation" />
                                                <asp:ListItem Value="Mission" Text="Mission" />
                                                <asp:ListItem Value="Urgence familiale" Text="Urgence familiale" />
                                                <asp:ListItem Value="Accident de travail" Text="Accident de travail" />
                                                <asp:ListItem Value="Grève" Text="Grève" />
                                                <asp:ListItem Value="Autre" Text="Autre" />
                                            </asp:DropDownList>
                                            <asp:TextBox ID="txtMotifAutre" runat="server"
                                                CssClass="form-control form-control-sm mt-1 motif-autre-text"
                                                placeholder="Précisez..."
                                                Style="display: none;"
                                                Text='<%# GetMotifAutreText(Eval("MotifAbsence")) %>' />
                                        </ItemTemplate>
                                        <HeaderStyle CssClass="text-center" />
                                        <ItemStyle CssClass="text-center" Width="200px" />
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Summary Panel -->
        <asp:Panel ID="pnlSummary" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-chart-pie me-2"></i>Résumé du Pointage
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row text-center">
                            <div class="col-md-3">
                                <div class="p-3 bg-success bg-opacity-10 rounded">
                                    <h3 class="text-success">
                                        <asp:Literal ID="litPresents" runat="server" Text="0" />
                                    </h3>
                                    <p class="mb-0">Présents</p>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="p-3 bg-danger bg-opacity-10 rounded">
                                    <h3 class="text-danger">
                                        <asp:Literal ID="litAbsents" runat="server" Text="0" />
                                    </h3>
                                    <p class="mb-0">Absents</p>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="p-3 bg-warning bg-opacity-10 rounded">
                                    <h3 class="text-warning">
                                        <asp:Literal ID="litDemiJournees" runat="server" Text="0" />
                                    </h3>
                                    <p class="mb-0">Demi-journées</p>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="p-3 bg-primary bg-opacity-10 rounded">
                                    <h3 class="text-primary">
                                        <asp:Literal ID="litTotal" runat="server" Text="0" />
                                    </h3>
                                    <p class="mb-0">Total</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        function handleStatusChange(checkbox, status) {
            var row = checkbox.closest('tr');
            var chkPresent = row.querySelector('[id*="chkPresent"]');
            var chkAbsent = row.querySelector('[id*="chkAbsent"]');
            var chkDemiJournee = row.querySelector('[id*="chkDemiJournee"]');
            var hdnStatut = row.querySelector('[id*="hdnStatutPresence"]');
            var ddlMotif = row.querySelector('[id*="ddlMotifAbsence"]');
            var txtMotifAutre = row.querySelector('[id*="txtMotifAutre"]');

            // Uncheck all other checkboxes in the same row
            chkPresent.checked = false;
            chkAbsent.checked = false;
            chkDemiJournee.checked = false;

            // Check only the clicked one
            checkbox.checked = true;

            // Update hidden field
            if (hdnStatut) {
                hdnStatut.value = status;
            }

            // Show/hide absence reason dropdown
            if (ddlMotif) {
                if (status === 'A') {
                    ddlMotif.style.display = 'block';
                } else {
                    ddlMotif.style.display = 'none';
                    ddlMotif.selectedIndex = 0; // Reset to first option
                    if (txtMotifAutre) {
                        txtMotifAutre.style.display = 'none';
                        txtMotifAutre.value = '';
                    }
                }
            }

            // Add visual feedback
            row.className = row.className.replace(/\b(present|absent|partial)\b/g, '');
            if (status === 'P') row.classList.add('present');
            else if (status === 'A') row.classList.add('absent');
            else if (status === 'P/2') row.classList.add('partial');

            // Update summary counts in real-time
            updateSummaryCounts();
        }

        function markAllPresent() {
            var presentCheckboxes = document.querySelectorAll('[id*="chkPresent"]');
            presentCheckboxes.forEach(function (checkbox) {
                handleStatusChange(checkbox, 'P');
            });
        }

        function handleMotifChange(dropdown) {
            var row = dropdown.closest('tr');
            var txtMotifAutre = row.querySelector('[id*="txtMotifAutre"]');

            if (txtMotifAutre) {
                if (dropdown.value === 'Autre') {
                    txtMotifAutre.style.display = 'block';
                    txtMotifAutre.focus();
                } else {
                    txtMotifAutre.style.display = 'none';
                    txtMotifAutre.value = '';
                }
            }
        }

        function updateSummaryCounts() {
            var presents = 0, absents = 0, demiJournees = 0, total = 0;

            var rows = document.querySelectorAll('#<%= gvPointage.ClientID %> tr');
            for (var i = 1; i < rows.length; i++) { // Skip header row
                var row = rows[i];
                total++;

                var chkPresent = row.querySelector('[id*="chkPresent"]');
                var chkAbsent = row.querySelector('[id*="chkAbsent"]');
                var chkDemiJournee = row.querySelector('[id*="chkDemiJournee"]');

                if (chkPresent && chkPresent.checked) presents++;
                else if (chkAbsent && chkAbsent.checked) absents++;
                else if (chkDemiJournee && chkDemiJournee.checked) demiJournees++;
                else presents++; // Default to present
            }

            // Update the display
            document.getElementById('<%= litPresents.ClientID %>').textContent = presents;
            document.getElementById('<%= litAbsents.ClientID %>').textContent = absents;
            document.getElementById('<%= litDemiJournees.ClientID %>').textContent = demiJournees;
            document.getElementById('<%= litTotal.ClientID %>').textContent = total;
        }

        // Initialize dropdowns on page load
        document.addEventListener('DOMContentLoaded', function () {
            // Add event listeners to all motif dropdowns
            var motifDropdowns = document.querySelectorAll('[id*="ddlMotifAbsence"]');
            motifDropdowns.forEach(function (dropdown) {
                dropdown.addEventListener('change', function () {
                    handleMotifChange(this);
                });

                // Initialize visibility on load
                if (dropdown.value === 'Autre') {
                    var row = dropdown.closest('tr');
                    var txtMotifAutre = row.querySelector('[id*="txtMotifAutre"]');
                    if (txtMotifAutre) {
                        txtMotifAutre.style.display = 'block';
                    }
                }
            });
        });
    </script>
</asp:Content>