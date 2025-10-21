<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SupervisionPointage.aspx.cs" Inherits="APPLICATION_RH.pointage.SupervisionPointage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .status-badge {
            font-size: 0.85em;
            padding: 0.25rem 0.5rem;
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

        .team-header {
            background-color: #f8f9fa;
            border-left: 4px solid #0d6efd;
            font-weight: bold;
        }

        .correction-indicator {
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
        }

        .table-responsive-custm {
            max-height: calc(100vh - 300px) !important;
            overflow-y: auto;
        }

        .team-summary {
            background: linear-gradient(45deg, #f8f9fa, #e9ecef);
            border-radius: 8px;
            margin-bottom: 10px;
        }

        .chef-info {
            font-size: 0.9em;
            color: #6c757d;
        }

        .modification-badge {
            font-size: 0.75em;
            background-color: #17a2b8;
            color: white;
            padding: 2px 6px;
            border-radius: 3px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Supervision des Pointages</h4>
                <p class="text-muted mb-0">Révision et correction des pointages d'équipes</p>
            </div>
            <div class="text-end">
                <ol class="breadcrumb m-0 py-0">
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlDashboard" runat="server" NavigateUrl="~/Default.aspx">
                            Dashboard
                        </asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlPointage" runat="server" NavigateUrl="~/pointage/GestionEquipes.aspx">
                            Gestion des Équipes
                        </asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item active">Supervision Pointages</li>
                </ol>
            </div>
        </div>

        <!-- Filters -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-filter me-2"></i>Filtres de Recherche
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblSociete" runat="server" Text="Société *" CssClass="form-label" />
                                    <asp:DropDownList ID="ddlSociete" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlSociete_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Sélectionner --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-2">
                                <div class="mb-3">
                                    <asp:Label ID="lblDate" runat="server" Text="Date *" CssClass="form-label" />
                                    <asp:TextBox ID="txtDatePointage" runat="server" CssClass="form-control" TextMode="Date" AutoPostBack="true" OnTextChanged="txtDatePointage_TextChanged" />
                                </div>
                            </div>
                            <div class="col-lg-2">
                                <div class="mb-3">
                                    <asp:Label ID="lblShift" runat="server" Text="Shift" CssClass="form-label" />
                                    <asp:DropDownList ID="ddlShift" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlShift_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Tous --" />
                                        <asp:ListItem Value="JOUR">JOUR</asp:ListItem>
                                        <asp:ListItem Value="NUIT">NUIT</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblEquipe" runat="server" Text="Équipe" CssClass="form-label" />
                                    <asp:DropDownList ID="ddlEquipe" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEquipe_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Toutes les équipes --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-2">
                                <div class="mb-3">
                                    <label class="form-label">&nbsp;</label>
                                    <div>
                                        <asp:Button ID="btnRechercher" runat="server" Text="Rechercher" CssClass="btn btn-primary w-100" OnClick="btnRechercher_Click" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Results Panel -->
        <asp:Panel ID="pnlResults" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-users-cog me-2"></i>Pointages du 
                            <asp:Literal ID="litDateResults" runat="server" />
                            <asp:Panel ID="pnlShiftInfo" runat="server" CssClass="d-inline">
                                - <asp:Literal ID="litShiftResults" runat="server" />
                            </asp:Panel>
                        </h5>
                        <div>
                            <asp:Button ID="btnSauvegarderTout" runat="server" Text="Sauvegarder Toutes les Modifications" 
                                CssClass="btn btn-success" OnClick="btnSauvegarderTout_Click" />
                        </div>
                    </div>
                    <div class="card-body">
                        <!-- Teams Summary -->
                        <asp:Panel ID="pnlTeamsSummary" runat="server" CssClass="mb-4">
                            <asp:Repeater ID="rptTeamsSummary" runat="server">
                                <ItemTemplate>
                                    <div class="team-summary p-3 mb-2">
                                        <div class="row align-items-center">
                                            <div class="col-md-3">
                                                <strong><%# Eval("NomEquipe") %></strong>
                                                <div class="chef-info">Chef: <%# Eval("NomChef") %></div>
                                            </div>
                                            <div class="col-md-9">
                                                <div class="row text-center">
                                                    <div class="col-3">
                                                        <span class="badge bg-success"><%# Eval("Presents") %> Présents</span>
                                                    </div>
                                                    <div class="col-3">
                                                        <span class="badge bg-danger"><%# Eval("Absents") %> Absents</span>
                                                    </div>
                                                    <div class="col-3">
                                                        <span class="badge bg-warning"><%# Eval("DemiJournees") %> Demi-J.</span>
                                                    </div>
                                                    <div class="col-3">
                                                        <span class="badge bg-primary"><%# Eval("Total") %> Total</span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </asp:Panel>

                        <!-- Detailed Pointage Grid -->
                        <div class="table-responsive table-responsive-custm">
                            <asp:GridView ID="gvSupervisionPointage" runat="server"
                                CssClass="table table-striped table-hover"
                                AutoGenerateColumns="false"
                                EmptyDataText="Aucun pointage trouvé pour ces critères"
                                OnRowDataBound="gvSupervisionPointage_RowDataBound">
                                <Columns>
                                    <asp:TemplateField HeaderText="Équipe">
                                        <ItemTemplate>
                                            <div class="fw-bold"><%# Eval("NomEquipe") %></div>
                                            <small class="text-muted"><%# Eval("NomChef") %></small>
                                        </ItemTemplate>
                                        <ItemStyle Width="150px" />
                                    </asp:TemplateField>
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
                                            <asp:HiddenField ID="hdnIdChefEquipe" runat="server" Value='<%# Eval("IdChefEquipe") %>' />
                                            <asp:HiddenField ID="hdnOriginalStatut" runat="server" Value='<%# Eval("StatutPresence") %>' />
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
                                    <asp:TemplateField HeaderText="Informations">
                                        <ItemTemplate>
                                            <div class="small">
                                                <%# Eval("PointePar") != DBNull.Value ? "Par: " + Eval("PointePar").ToString() : "" %>
                                                <asp:Panel ID="pnlModification" runat="server" Visible="false" CssClass="mt-1">
                                                    <span class="modification-badge">MODIFIÉ</span>
                                                </asp:Panel>
                                            </div>
                                        </ItemTemplate>
                                        <ItemStyle Width="150px" />
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>

                        <asp:Panel ID="pnlNoResults" runat="server" Visible="false">
                            <div class="alert alert-info text-center">
                                <i class="fas fa-info-circle"></i>
                                Aucun pointage trouvé pour les critères sélectionnés.
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Global Summary -->
        <asp:Panel ID="pnlGlobalSummary" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-chart-bar me-2"></i>Résumé Global
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row text-center">
                            <div class="col-md-2">
                                <div class="p-3 bg-info bg-opacity-10 rounded">
                                    <h3 class="text-info">
                                        <asp:Literal ID="litTotalEquipes" runat="server" Text="0" />
                                    </h3>
                                    <p class="mb-0">Équipes</p>
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="p-3 bg-primary bg-opacity-10 rounded">
                                    <h3 class="text-primary">
                                        <asp:Literal ID="litTotalEmployes" runat="server" Text="0" />
                                    </h3>
                                    <p class="mb-0">Employés</p>
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="p-3 bg-success bg-opacity-10 rounded">
                                    <h3 class="text-success">
                                        <asp:Literal ID="litTotalPresents" runat="server" Text="0" />
                                    </h3>
                                    <p class="mb-0">Présents</p>
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="p-3 bg-danger bg-opacity-10 rounded">
                                    <h3 class="text-danger">
                                        <asp:Literal ID="litTotalAbsents" runat="server" Text="0" />
                                    </h3>
                                    <p class="mb-0">Absents</p>
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="p-3 bg-warning bg-opacity-10 rounded">
                                    <h3 class="text-warning">
                                        <asp:Literal ID="litTotalDemiJournees" runat="server" Text="0" />
                                    </h3>
                                    <p class="mb-0">Demi-J.</p>
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="p-3 bg-secondary bg-opacity-10 rounded">
                                    <h3 class="text-secondary">
                                        <asp:Literal ID="litTauxPresence" runat="server" Text="0%" />
                                    </h3>
                                    <p class="mb-0">Taux Présence</p>
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
            var hdnOriginal = row.querySelector('[id*="hdnOriginalStatut"]');
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
                    ddlMotif.selectedIndex = 0;
                    if (txtMotifAutre) {
                        txtMotifAutre.style.display = 'none';
                        txtMotifAutre.value = '';
                    }
                }
            }

            // Add visual feedback
            row.className = row.className.replace(/\b(present|absent|partial|correction-indicator)\b/g, '');
            if (status === 'P') row.classList.add('present');
            else if (status === 'A') row.classList.add('absent');
            else if (status === 'P/2') row.classList.add('partial');

            // Check if this is a modification from original
            if (hdnOriginal && hdnOriginal.value !== status) {
                row.classList.add('correction-indicator');
            }

            updateGlobalSummary();
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

        function updateGlobalSummary() {
            var presents = 0, absents = 0, demiJournees = 0, total = 0;

            var rows = document.querySelectorAll('#<%= gvSupervisionPointage.ClientID %> tr');
            for (var i = 1; i < rows.length; i++) {
                var row = rows[i];
                total++;

                var chkPresent = row.querySelector('[id*="chkPresent"]');
                var chkAbsent = row.querySelector('[id*="chkAbsent"]');
                var chkDemiJournee = row.querySelector('[id*="chkDemiJournee"]');

                if (chkPresent && chkPresent.checked) presents++;
                else if (chkAbsent && chkAbsent.checked) absents++;
                else if (chkDemiJournee && chkDemiJournee.checked) demiJournees++;
                else presents++;
            }

            // Update the display
            if (document.getElementById('<%= litTotalEmployes.ClientID %>')) {
                document.getElementById('<%= litTotalEmployes.ClientID %>').textContent = total;
                document.getElementById('<%= litTotalPresents.ClientID %>').textContent = presents;
                document.getElementById('<%= litTotalAbsents.ClientID %>').textContent = absents;
                document.getElementById('<%= litTotalDemiJournees.ClientID %>').textContent = demiJournees;
                
                var tauxPresence = total > 0 ? Math.round((presents + (demiJournees * 0.5)) / total * 100) : 0;
                document.getElementById('<%= litTauxPresence.ClientID %>').textContent = tauxPresence + '%';
            }
        }

        // Initialize on page load
        document.addEventListener('DOMContentLoaded', function() {
            // Add event listeners to all motif dropdowns
            var motifDropdowns = document.querySelectorAll('[id*="ddlMotifAbsence"]');
            motifDropdowns.forEach(function(dropdown) {
                dropdown.addEventListener('change', function() {
                    handleMotifChange(this);
                });

                if (dropdown.value === 'Autre') {
                    var row = dropdown.closest('tr');
                    var txtMotifAutre = row.querySelector('[id*="txtMotifAutre"]');
                    if (txtMotifAutre) {
                        txtMotifAutre.style.display = 'block';
                    }
                }
            });

            updateGlobalSummary();
        });
    </script>
</asp:Content>
