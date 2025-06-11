<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PointageHeuresSupp.aspx.cs" Inherits="APPLICATION_RH.pointage.PointageHeuresSupp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .time-input { 
    width: 100px; 
}

.hours-input { 
    width: 80px;
    text-align: center;
}

.overtime-row { 
    border-left: 4px solid #e9ecef; 
}

.overtime-row.has-hours { 
    border-left-color: #28a745; 
    background-color: #f8fef9; 
}

.table-responsive-custm {
    max-height: calc(100vh - 250px) !important;
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

.coef-badge {
    font-size: 0.75em;
    padding: 0.25rem 0.5rem;
    border-radius: 0.25rem;
}

/* New styles for enhanced UX */
.time-input:focus, .hours-input:focus {
    border-color: #80bdff;
    box-shadow: 0 0 0 0.2rem rgba(0, 123, 255, 0.25);
}

.hours-input:valid {
    border-color: #28a745;
}

.time-input::placeholder, .hours-input::placeholder {
    color: #6c757d;
    font-style: italic;
}

/* Visual indicator for manual vs calculated hours */
.hours-input[readonly] {
    background-color: #f8f9fa;
    border-style: dashed;
}

.hours-input:not([readonly]) {
    background-color: #fff;
    border-style: solid;
    font-weight: 500;
}

/* Highlight active input method */
.has-hours .time-input {
    border-color: #28a745;
    border-width: 2px;
}

.has-hours .hours-input {
    border-color: #28a745;
    border-width: 2px;
    background-color: #f8fff9;
}
        /*.time-input {
            width: 100px;
        }

        .hours-input {
            width: 80px;
        }*/

        /*.overtime-row {
            border-left: 4px solid #e9ecef;
        }

            .overtime-row.has-hours {
                border-left-color: #28a745;
                background-color: #f8fef9;
            }

        .table-responsive-custm {
            max-height: calc(100vh - 250px) !important;
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

        .coef-badge {
            font-size: 0.75em;
            padding: 0.25rem 0.5rem;
            border-radius: 0.25rem;
        }*/
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Heures Supplémentaires de l'Équipe</h4>
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
                    <li class="breadcrumb-item active">Heures Supplémentaires</li>
                </ol>
            </div>
        </div>

        <!-- Team Info & Date Selection -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <div class="row align-items-center">
                            <div class="col-md-4">
                                <h5 class="card-title mb-0">
                                    <i class="fas fa-clock me-2"></i>
                                    <asp:Literal ID="litTeamName" runat="server" Text="Équipe" />
                                </h5>
                            </div>
                            <div class="col-md-8">
                                <div class="row">
                                    <div class="col-md-4">
                                        <label class="form-label">Date:</label>
                                        <asp:TextBox ID="txtDateHeureSupp" runat="server"
                                            CssClass="form-control" TextMode="Date" AutoPostBack="true"
                                            OnTextChanged="txtDateHeureSupp_TextChanged" />
                                    </div>
                                    <div class="col-md-4">
                                        <label class="form-label">Type Jour:</label>
                                        <asp:DropDownList ID="ddlTypeJour" runat="server"
                                            CssClass="form-select" AutoPostBack="true"
                                            OnSelectedIndexChanged="ddlTypeJour_SelectedIndexChanged">
                                            <asp:ListItem Value="SEMAINE" Selected="True">Jour de semaine</asp:ListItem>
                                            <asp:ListItem Value="WEEKEND">Weekend</asp:ListItem>
                                            <asp:ListItem Value="FERIE">Jour férié</asp:ListItem>
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

        <!-- Overtime Hours Grid -->
        <asp:Panel ID="pnlHeuresSupp" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header d-lg-flex d-block justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-business-time me-2"></i>Heures Supplémentaires du 
                            <asp:Literal ID="litDateHeureSupp" runat="server" />
                        </h5>
                        <div>
                            <asp:Button ID="btnEffacerTout" runat="server" Text="Effacer Tout"
                                CssClass="btn btn-secondary me-2" OnClientClick="clearAllOvertime(); return false;" />
                            <asp:Button ID="btnSauvegarder" runat="server" Text="Sauvegarder"
                                CssClass="btn btn-primary" OnClick="btnSauvegarder_Click" />
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="table-responsive table-responsive-custm">
                            <asp:GridView ID="gvHeuresSupp" runat="server"
                                CssClass="table table-striped table-hover"
                                AutoGenerateColumns="false"
                                EmptyDataText="Aucun employé dans cette équipe"
                                OnRowDataBound="gvHeuresSupp_RowDataBound">
                                <Columns>
                                    <asp:BoundField DataField="Matricule" HeaderText="Matricule" />
                                    <asp:BoundField DataField="NomComplet" HeaderText="Nom Complet" />
                                    <asp:TemplateField HeaderText="Heure Début">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtHeureDebut" runat="server"
                                                CssClass="form-control form-control-sm time-input"
                                                TextMode="Time"
                                                Text='<%# FormatTime(Eval("HeureDebut")) %>'
                                                onchange="calculateOvertimeHours(this)"
                                                placeholder="Optionnel" />
                                            <asp:HiddenField ID="hdnIdEmploye" runat="server" Value='<%# Eval("IdEmploye") %>' />
                                            <asp:HiddenField ID="hdnIdHeureSupp" runat="server" Value='<%# Eval("IdHeureSupp") %>' />
                                        </ItemTemplate>
                                        <HeaderStyle CssClass="text-center" />
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Heure Fin">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtHeureFin" runat="server"
                                                CssClass="form-control form-control-sm time-input"
                                                TextMode="Time"
                                                Text='<%# FormatTime(Eval("HeureFin")) %>'
                                                onchange="calculateOvertimeHours(this)"
                                                placeholder="Optionnel" />
                                        </ItemTemplate>
                                        <HeaderStyle CssClass="text-center" />
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Nb Heures">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtNombreHeures" runat="server"
                                                CssClass="form-control form-control-sm hours-input"
                                                Text='<%# Eval("NombreHeures") %>'
                                                onchange="calculateFromHours(this)"
                                                placeholder="0.00"
                                                step="0.25"
                                                min="0" />
                                        </ItemTemplate>
                                        <HeaderStyle CssClass="text-center" />
                                        <ItemStyle CssClass="text-center" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Coefficient">
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlCoefficient" runat="server"
                                                CssClass="form-select form-select-sm"
                                                onchange="updateRowHighlight(this)">
                                                <asp:ListItem Value="1.00" Text="100%"></asp:ListItem>
                                                <asp:ListItem Value="1.25" Text="125%"></asp:ListItem>
                                                <asp:ListItem Value="1.50" Text="150%"></asp:ListItem>
                                                <asp:ListItem Value="2.00" Text="200%"></asp:ListItem>
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                        <HeaderStyle CssClass="text-center" />
                                        <ItemStyle CssClass="text-center" Width="100px" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Justification">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtJustification" runat="server"
                                                CssClass="form-control form-control-sm"
                                                Text='<%# Eval("Justification") %>'
                                                placeholder="Raison des heures supplémentaires..."
                                                TextMode="MultiLine" Rows="1" />
                                        </ItemTemplate>
                                        <HeaderStyle CssClass="text-center" />
                                        <ItemStyle Width="300px" />
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>

                        <asp:Panel ID="pnlNoEmployees" runat="server" Visible="false">
                            <div class="alert alert-info text-center">
                                <i class="fas fa-info-circle"></i>
                                Aucun employé dans votre équipe.
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
                            <i class="fas fa-chart-bar me-2"></i>Résumé des Heures Supplémentaires
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row text-center">
                            <div class="col-md-3">
                                <div class="p-3 bg-primary bg-opacity-10 rounded">
                                    <h3 class="text-primary">
                                        <asp:Literal ID="litTotalEmployes" runat="server" Text="0" />
                                    </h3>
                                    <p class="mb-0">Employés avec H.S.</p>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="p-3 bg-success bg-opacity-10 rounded">
                                    <h3 class="text-success">
                                        <asp:Literal ID="litTotalHeures" runat="server" Text="0" />
                                    </h3>
                                    <p class="mb-0">Total Heures</p>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="p-3 bg-warning bg-opacity-10 rounded">
                                    <h3 class="text-warning">
                                        <asp:Literal ID="litHeuresPonderees" runat="server" Text="0" />
                                    </h3>
                                    <p class="mb-0">Heures Pondérées</p>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="p-3 bg-info bg-opacity-10 rounded">
                                    <h3 class="text-info">
                                        <asp:Literal ID="litCoeffMoyen" runat="server" Text="100%" />
                                    </h3>
                                    <p class="mb-0">Coefficient Moyen</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        function calculateOvertimeHours(element) {
            var row = element.closest('tr');
            var txtHeureDebut = row.querySelector('[id*="txtHeureDebut"]');
            var txtHeureFin = row.querySelector('[id*="txtHeureFin"]');
            var txtNombreHeures = row.querySelector('[id*="txtNombreHeures"]');

            if (txtHeureDebut && txtHeureFin && txtNombreHeures) {
                var heureDebut = txtHeureDebut.value;
                var heureFin = txtHeureFin.value;

                if (heureDebut && heureFin) {
                    // Calculate hours difference
                    var start = new Date('2000-01-01T' + heureDebut);
                    var end = new Date('2000-01-01T' + heureFin);

                    // If end time is before start time, assume it's the next day
                    if (end < start) {
                        end = new Date('2000-01-02T' + heureFin);
                    }

                    var diff = (end - start) / 1000 / 60 / 60; // Convert to hours
                    txtNombreHeures.value = diff.toFixed(2);

                    // Update row highlight
                    if (diff > 0) {
                        row.classList.add('has-hours');
                    } else {
                        row.classList.remove('has-hours');
                    }
                } else if (!txtNombreHeures.value || parseFloat(txtNombreHeures.value) <= 0) {
                    // Only clear hours if no manual hours entered and no valid times
                    txtNombreHeures.value = '';
                    row.classList.remove('has-hours');
                }
            }

            updateSummaryCounts();
        }

        function calculateFromHours(element) {
            var row = element.closest('tr');
            var txtNombreHeures = element;
            var txtHeureDebut = row.querySelector('[id*="txtHeureDebut"]');
            var txtHeureFin = row.querySelector('[id*="txtHeureFin"]');

            var heures = parseFloat(txtNombreHeures.value) || 0;

            // Update row highlight based on hours entered
            if (heures > 0) {
                row.classList.add('has-hours');

                // Optional: Clear start/end times when hours are manually entered
                // Uncomment these lines if you want to clear times when hours are entered directly
                // txtHeureDebut.value = '';
                // txtHeureFin.value = '';
            } else {
                row.classList.remove('has-hours');
            }

            updateSummaryCounts();
        }

        function updateRowHighlight(element) {
            var row = element.closest('tr');
            var txtNombreHeures = row.querySelector('[id*="txtNombreHeures"]');

            if (txtNombreHeures && parseFloat(txtNombreHeures.value) > 0) {
                row.classList.add('has-hours');
            }
        }

        function clearAllOvertime() {
            if (confirm('Êtes-vous sûr de vouloir effacer toutes les heures supplémentaires?')) {
                var rows = document.querySelectorAll('#<%= gvHeuresSupp.ClientID %> tr');
                for (var i = 1; i < rows.length; i++) {
                    var row = rows[i];
                    var txtHeureDebut = row.querySelector('[id*="txtHeureDebut"]');
                    var txtHeureFin = row.querySelector('[id*="txtHeureFin"]');
                    var txtNombreHeures = row.querySelector('[id*="txtNombreHeures"]');
                    var txtJustification = row.querySelector('[id*="txtJustification"]');

                    if (txtHeureDebut) txtHeureDebut.value = '';
                    if (txtHeureFin) txtHeureFin.value = '';
                    if (txtNombreHeures) txtNombreHeures.value = '';
                    if (txtJustification) txtJustification.value = '';

                    row.classList.remove('has-hours');
                }
                updateSummaryCounts();
            }
        }

        function updateSummaryCounts() {
            var totalEmployes = 0;
            var totalHeures = 0;
            var heuresPonderees = 0;

            var rows = document.querySelectorAll('#<%= gvHeuresSupp.ClientID %> tr');
    for (var i = 1; i < rows.length; i++) {
        var row = rows[i];
        var txtNombreHeures = row.querySelector('[id*="txtNombreHeures"]');
        var ddlCoefficient = row.querySelector('[id*="ddlCoefficient"]');

        if (txtNombreHeures && txtNombreHeures.value) {
            var heures = parseFloat(txtNombreHeures.value);
            if (heures > 0) {
                totalEmployes++;
                totalHeures += heures;

                if (ddlCoefficient) {
                    var coef = parseFloat(ddlCoefficient.value);
                    heuresPonderees += heures * coef;
                }
            }
        }
    }

    // Update display
    document.getElementById('<%= litTotalEmployes.ClientID %>').textContent = totalEmployes;
    document.getElementById('<%= litTotalHeures.ClientID %>').textContent = totalHeures.toFixed(2);
    document.getElementById('<%= litHeuresPonderees.ClientID %>').textContent = heuresPonderees.toFixed(2);

    // Calculate average coefficient
    if (totalHeures > 0) {
        var coeffMoyen = (heuresPonderees / totalHeures * 100).toFixed(0);
        document.getElementById('<%= litCoeffMoyen.ClientID %>').textContent = coeffMoyen + '%';
    } else {
        document.getElementById('<%= litCoeffMoyen.ClientID %>').textContent = '100%';
            }
        }

        // Initialize on page load
        document.addEventListener('DOMContentLoaded', function () {
            // Set default coefficient based on day type
            var typeJour = document.getElementById('<%= ddlTypeJour.ClientID %>').value;
            var defaultCoef = '1.00'; // Default for weekday

            if (typeJour === 'WEEKEND') defaultCoef = '1.50';
            else if (typeJour === 'FERIE') defaultCoef = '2.00';

            var coeffDropdowns = document.querySelectorAll('[id*="ddlCoefficient"]');
            coeffDropdowns.forEach(function (dropdown) {
                if (!dropdown.value || dropdown.value === '1.00') {
                    dropdown.value = defaultCoef;
                }
            });

            updateSummaryCounts();
        });
    </script>
</asp:Content>
