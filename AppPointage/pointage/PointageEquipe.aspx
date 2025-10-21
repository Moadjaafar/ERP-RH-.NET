<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PointageEquipe.aspx.cs" Inherits="APPLICATION_RH.pointage.PointageEquipe" %>

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
            max-height: calc(100vh - 250px) !important;
            overflow-y: auto;
        }

        .table tbody tr:hover {
            background-color: #f8f9fa;
        }

        . table-responsive-custm .table tbody tr:nth-of-type(1) {
            background-color: #f8f9fa;
            border-bottom: 2px solid #dee2e6;
            position: sticky;
            top: 0;
            z-index: 10;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Pointage de l'Équipe</h4>
            </div>
            <div class="text-end">
                <ol class="breadcrumb m-0 py-0">
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlDashboard" runat="server" NavigateUrl="~/Default.aspx">
                            Dashboard
                        </asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlMonEquipe" runat="server" NavigateUrl="~/pointage/MonEquipe.aspx">
                            Mon Équipe
                        </asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item active">Pointage</li>
                </ol>
            </div>
        </div>

        <!-- Team Info & Date Selection -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <div class="row align-items-center">
                            <div class="col-md-6">
                                <h5 class="card-title mb-0">
                                    <i class="fas fa-clock me-2"></i>
                                    <asp:Literal ID="litTeamName" runat="server" Text="Équipe" />
                                </h5>
                            </div>
                            <div class="col-md-6">
                                <div class="row">
                                    <div class="col-md-4">
                                        <label class="form-label">Date:</label>
                                        <asp:TextBox ID="txtDatePointage" runat="server"
                                            CssClass="form-control" TextMode="Date" AutoPostBack="true"
                                            OnTextChanged="txtDatePointage_TextChanged" />
                                    </div>
                                    <div class="col-md-4">
                                        <label class="form-label">Shift:</label>
                                        <asp:DropDownList ID="ddlShift" runat="server"
                                            CssClass="form-select" AutoPostBack="true"
                                            OnSelectedIndexChanged="ddlShift_SelectedIndexChanged">
                                            <asp:ListItem Value="JOUR" Selected="True">JOUR</asp:ListItem>
                                            <asp:ListItem Value="NUIT">NUIT</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-4 d-flex align-items-end">
                                        <asp:Button ID="btnCharger" runat="server" Text="Charger"
                                            CssClass="btn btn-primary" OnClick="btnCharger_Click" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- No Team Message -->
        <asp:Panel ID="pnlNoTeam" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="alert alert-warning text-center">
                    <i class="fas fa-exclamation-triangle fa-2x mb-2"></i>
                    <h5>Aucune équipe assignée</h5>
                    <p class="mb-0">Vous n'êtes actuellement chef d'aucune équipe.</p>
                </div>
            </div>
        </asp:Panel>

        <!-- Pointage Grid -->
        <asp:Panel ID="pnlPointage" runat="server" Visible="false" CssClass="row mt-4">
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
                                EmptyDataText="Aucun employé dans cette équipe"
                                OnRowDataBound="gvPointage_RowDataBound">
                                <Columns>
                                    <asp:BoundField DataField="Matricule" HeaderText="Matricule" />
                                    <asp:BoundField DataField="NomComplet" HeaderText="Nom Complet" />
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

                        <asp:Panel ID="pnlNoEmployees" runat="server" Visible="false">
                            <div class="alert alert-info text-center">
                                <i class="fas fa-info-circle"></i>
                                Aucun employé dans votre équipe pour cette date.
                            </div>
                        </asp:Panel>
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
                            <i class="fas fa-chart-pie me-2"></i>Résumé
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

        // Also update the handleStatusChange function to clear motif when not absent
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
    </script>
</asp:Content>
