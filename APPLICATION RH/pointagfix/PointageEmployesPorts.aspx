<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PointageEmployesPorts.aspx.cs" Inherits="APPLICATION_RH.pointagfix.PointageEmployesPorts" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .status-badge-in { background-color: #28a745 !important; }
        .status-badge-out { background-color: #dc3545 !important; }
        .hours-summary { font-weight: bold; color: #007bff; }
        .quick-action-btn { min-width: 70px; margin: 1px; }
        .btn-group .btn { border-radius: 0; }
        .btn-group .btn:first-child { border-top-left-radius: 0.25rem; border-bottom-left-radius: 0.25rem; }
        .btn-group .btn:last-child { border-top-right-radius: 0.25rem; border-bottom-right-radius: 0.25rem; }
        .custom-time-panel { border-left: 4px solid #ffc107; }
        .edit-mode { background-color: #fff3cd; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid">
        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Pointage des Employés aux Ports</h4>
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
                    <li class="breadcrumb-item active">Pointage aux Ports</li>
                </ol>
            </div>
        </div>

        <!-- Filter Panel -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-clock me-2"></i>Sélection Port et Date
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <asp:Label ID="lblPort" runat="server" Text="Port *" CssClass="form-label" AssociatedControlID="ddlPort" />
                                    <asp:DropDownList ID="ddlPort" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlPort_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Sélectionner Port --" />
                                        <asp:ListItem Value="Port A - Entrée Principale" Text="Port A - Entrée Principale" />
                                        <asp:ListItem Value="Port B - Entrée Personnel" Text="Port B - Entrée Personnel" />
                                        <asp:ListItem Value="Port C - Entrée Livraisons" Text="Port C - Entrée Livraisons" />
                                        <asp:ListItem Value="Port D - Entrée Urgences" Text="Port D - Entrée Urgences" />
                                        <asp:ListItem Value="Port E - Entrée VIP" Text="Port E - Entrée VIP" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <asp:Label ID="lblDatePointage" runat="server" Text="Date de Pointage" CssClass="form-label" AssociatedControlID="txtDatePointage" />
                                    <asp:TextBox ID="txtDatePointage" runat="server" CssClass="form-control" TextMode="Date" AutoPostBack="true" OnTextChanged="txtDatePointage_TextChanged" />
                                </div>
                            </div>
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <asp:Label ID="lblMatricule" runat="server" Text="Recherche Employé" CssClass="form-label" AssociatedControlID="txtMatricule" />
                                    <div class="input-group">
                                        <asp:TextBox ID="txtMatricule" runat="server" CssClass="form-control" placeholder="Nom ou prénom employé..." />
                                        <asp:Button ID="btnSearchEmployee" runat="server" Text="Chercher" CssClass="btn btn-primary" OnClick="btnSearchEmployee_Click" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Employee Pointage Panel -->
        <asp:Panel ID="pnlEmployeePointage" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-users-clock me-2"></i>Employés Assignés au Port
                            <span class="badge bg-primary ms-2">
                                <asp:Literal ID="litEmployeeCount" runat="server" />
                            </span>
                        </h5>
                        <asp:Button ID="btnRefreshEmployees" runat="server" Text="Actualiser" 
                            CssClass="btn btn-outline-primary btn-sm" OnClick="btnRefreshEmployees_Click" CausesValidation="false" />
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvEmployees" runat="server" 
                            CssClass="table table-striped table-hover" 
                            AutoGenerateColumns="false"
                            OnRowCommand="gvEmployees_RowCommand"
                            EmptyDataText="Aucun employé assigné à ce port">
                            <Columns>
                                <asp:BoundField DataField="Matricule" HeaderText="Matricule" />
                                <asp:BoundField DataField="NomComplet" HeaderText="Nom Complet" />
                                <asp:TemplateField HeaderText="Statut Actuel">
                                    <ItemTemplate>
                                        <asp:Panel ID="pnlStatus" runat="server">
                                            <span class='<%# "badge " + (Eval("StatutActuel").ToString() == "ENTREE" ? "status-badge-in" : "status-badge-out") %>'>
                                                <%# Eval("StatutActuel").ToString() == "ENTREE" ? "PRÉSENT" : "SORTI" %>
                                            </span>
                                            <small class="text-muted d-block"><%# Eval("DerniereHeure") %></small>
                                        </asp:Panel>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Heures Aujourd'hui">
                                    <ItemTemplate>
                                        <span class="hours-summary"><%# Eval("HeuresJour") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <div class="btn-group" role="group">
                                            <asp:LinkButton ID="btnEntree" runat="server"
                                                CommandName="PointageEntree"
                                                CommandArgument='<%# Eval("IdEmploye") %>'
                                                CssClass="btn btn-sm btn-success quick-action-btn"
                                                Visible='<%# Convert.ToBoolean(Eval("PeutEntrer")) %>'
                                                OnClientClick='<%# "return confirmPointage(\"ENTREE\", \"" + Eval("NomComplet") + "\");" %>'>
                                                <i class="fas fa-sign-in-alt"></i> Entrée
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnSortie" runat="server"
                                                CommandName="PointageSortie"
                                                CommandArgument='<%# Eval("IdEmploye") %>'
                                                CssClass="btn btn-sm btn-danger quick-action-btn"
                                                Visible='<%# Convert.ToBoolean(Eval("PeutSortir")) %>'
                                                OnClientClick='<%# "return confirmPointage(\"SORTIE\", \"" + Eval("NomComplet") + "\");" %>'>
                                                <i class="fas fa-sign-out-alt"></i> Sortie
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnCustomPointage" runat="server"
                                                CommandName="CustomPointage"
                                                CommandArgument='<%# Eval("IdEmploye") %>'
                                                CssClass="btn btn-sm btn-outline-primary quick-action-btn"
                                                ToolTip="Pointage avec heure personnalisée">
                                                <i class="fas fa-clock"></i> Heure
                                            </asp:LinkButton>
                                        </div>
                                        <asp:Label ID="lblNoAction" runat="server" 
                                            Text="<span class='badge bg-secondary'>Aucune action</span>"
                                            Visible='<%# !Convert.ToBoolean(Eval("PeutEntrer")) && !Convert.ToBoolean(Eval("PeutSortir")) %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </asp:Panel>
    
        <!-- Custom Time Pointage Modal Panel -->
        <asp:Panel ID="pnlCustomTime" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card border-warning">
                    <div class="card-header bg-secondary text-white">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-clock me-2"></i>Pointage avec Heure Personnalisée
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-3">
                                <strong>Employé:</strong> <asp:Literal ID="litCustomEmploye" runat="server" />
                            </div>
                            <div class="col-md-2">
                                <asp:Label ID="lblCustomType" runat="server" Text="Type *" CssClass="form-label" />
                                <asp:DropDownList ID="ddlCustomType" runat="server" CssClass="form-select">
                                    <asp:ListItem Value="ENTREE" Text="ENTRÉE" />
                                    <asp:ListItem Value="SORTIE" Text="SORTIE" />
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-2">
                                <asp:Label ID="lblCustomDate" runat="server" Text="Date *" CssClass="form-label" />
                                <asp:TextBox ID="txtCustomDate" runat="server" CssClass="form-control" TextMode="Date" />
                            </div>
                            <div class="col-md-2">
                                <asp:Label ID="lblCustomTime" runat="server" Text="Heure *" CssClass="form-label" />
                                <asp:TextBox ID="txtCustomTime" runat="server" CssClass="form-control" TextMode="Time" />
                            </div>
                            <div class="col-md-3">
                                <label class="form-label">&nbsp;</label>
                                <div>
                                    <asp:Button ID="btnSaveCustom" runat="server" Text="Enregistrer" 
                                        CssClass="btn btn-warning me-2" OnClick="btnSaveCustom_Click" />
                                    <asp:Button ID="btnCancelCustom" runat="server" Text="Annuler" 
                                        CssClass="btn btn-secondary" OnClick="btnCancelCustom_Click" CausesValidation="false" />
                                </div>
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
                                <strong>Matricule:</strong> <asp:Literal ID="litQuickMatricule" runat="server" />
                            </div>
                            <div class="col-md-3">
                                <strong>Nom:</strong> <asp:Literal ID="litQuickNom" runat="server" />
                            </div>
                            <div class="col-md-3">
                                <strong>Statut:</strong> 
                                <asp:Literal ID="litQuickStatut" runat="server" />
                            </div>
                            <div class="col-md-3">
                                <asp:Button ID="btnQuickEntree" runat="server" Text="Entrée" 
                                    CssClass="btn btn-success me-2" OnClick="btnQuickEntree_Click" />
                                <asp:Button ID="btnQuickSortie" runat="server" Text="Sortie" 
                                    CssClass="btn btn-danger" OnClick="btnQuickSortie_Click" />
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
                            <i class="fas fa-chart-bar me-2"></i>Résumé de la Journée - <asp:Literal ID="litSelectedDate" runat="server" />
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-3">
                                <div class="card border-primary">
                                    <div class="card-body text-center">
                                        <h3 class="text-primary"><asp:Literal ID="litTotalPresents" runat="server" /></h3>
                                        <p class="mb-0">Employés Présents</p>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="card border-success">
                                    <div class="card-body text-center">
                                        <h3 class="text-success"><asp:Literal ID="litTotalEntrees" runat="server" /></h3>
                                        <p class="mb-0">Total Entrées</p>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="card border-danger">
                                    <div class="card-body text-center">
                                        <h3 class="text-danger"><asp:Literal ID="litTotalSorties" runat="server" /></h3>
                                        <p class="mb-0">Total Sorties</p>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="card border-info">
                                    <div class="card-body text-center">
                                        <h3 class="text-info"><asp:Literal ID="litMoyenneHeures" runat="server" /></h3>
                                        <p class="mb-0">Heures Moyenne</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Recent Pointages -->
        <div class="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-history me-2"></i>Derniers Pointages (24h) - Un par Employé
                        </h5>
                        <asp:Button ID="btnRefreshPointages" runat="server" Text="Actualiser" 
                            CssClass="btn btn-outline-primary btn-sm" OnClick="btnRefreshPointages_Click" CausesValidation="false" />
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvRecentPointages" runat="server" 
                            CssClass="table table-striped table-hover" 
                            AutoGenerateColumns="false"
                            OnRowCommand="gvRecentPointages_RowCommand"
                            OnRowEditing="gvRecentPointages_RowEditing"
                            OnRowUpdating="gvRecentPointages_RowUpdating"
                            OnRowCancelingEdit="gvRecentPointages_RowCancelingEdit"
                            DataKeyNames="IdPointage"
                            EmptyDataText="Aucun pointage récent">
                            <Columns>
                                <asp:BoundField DataField="NomEmploye" HeaderText="Employé" ReadOnly="true" />
                                <asp:BoundField DataField="MatriculeEmploye" HeaderText="Matricule" ReadOnly="true" />
                                <asp:BoundField DataField="NomPort" HeaderText="Port" ReadOnly="true" />
                                <asp:TemplateField HeaderText="Type">
                                    <ItemTemplate>
                                        <span class='<%# "badge " + (Eval("TypePointage").ToString() == "ENTREE" ? "bg-success" : "bg-danger") %>'>
                                            <%# Eval("TypePointage") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Date/Heure">
                                    <ItemTemplate>
                                        <%# Convert.ToDateTime(Eval("DateHeure")).ToString("dd/MM/yyyy HH:mm") %>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <div class="row">
                                            <div class="col-md-6">
                                                <asp:TextBox ID="txtEditDate" runat="server" 
                                                    Text='<%# Convert.ToDateTime(Eval("DateHeure")).ToString("yyyy-MM-dd") %>'
                                                    TextMode="Date" CssClass="form-control form-control-sm" />
                                            </div>
                                            <div class="col-md-6">
                                                <asp:TextBox ID="txtEditTime" runat="server" 
                                                    Text='<%# Convert.ToDateTime(Eval("DateHeure")).ToString("HH:mm") %>'
                                                    TextMode="Time" CssClass="form-control form-control-sm" />
                                            </div>
                                        </div>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="AssignePar" HeaderText="Enregistré par" ReadOnly="true" />
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnModifier" runat="server"
                                            CommandName="Edit"
                                            CssClass="btn btn-sm btn-outline-primary me-1">
                                            <i class="fas fa-edit"></i> Modifier
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnAnnuler" runat="server"
                                            CommandName="AnnulerPointage"
                                            CommandArgument='<%# Eval("IdPointage") %>'
                                            CssClass="btn btn-sm btn-outline-warning"
                                            OnClientClick="return confirm('Annuler ce pointage?');">
                                            <i class="fas fa-undo"></i> Annuler
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:LinkButton ID="btnSauvegarder" runat="server"
                                            CommandName="Update"
                                            CssClass="btn btn-sm btn-success me-1">
                                            <i class="fas fa-save"></i> Sauver
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnAnnulerEdit" runat="server"
                                            CommandName="Cancel"
                                            CssClass="btn btn-sm btn-secondary">
                                            <i class="fas fa-times"></i> Annuler
                                        </asp:LinkButton>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>

    </div>

    <script type="text/javascript">
        // Auto-refresh every 30 seconds for real-time updates
        setInterval(function () {
            if (document.getElementById('<%= btnRefreshEmployees.ClientID %>')) {
                __doPostBack('<%= btnRefreshEmployees.UniqueID %>', '');
            }
        }, 30000);

        // Confirmation for pointage actions
        function confirmPointage(action, employee) {
            return confirm('Confirmer ' + action + ' pour ' + employee + '?');
        }

        // Highlight editing rows
        function highlightEditRow() {
            var editCells = document.querySelectorAll('td[style*="background-color"]');
            editCells.forEach(function(cell) {
                cell.parentElement.classList.add('edit-mode');
            });
        }

        // Set current time when opening custom pointage
        function setCurrentTime() {
            var now = new Date();
            var date = now.toISOString().split('T')[0];
            var time = now.toTimeString().split(' ')[0].substring(0,5);
            
            var dateField = document.getElementById('<%= txtCustomDate.ClientID %>');
            var timeField = document.getElementById('<%= txtCustomTime.ClientID %>');
            
            if (dateField && timeField) {
                dateField.value = date;
                timeField.value = time;
            }
        }

        // Validate custom time input
        function validateCustomTime() {
            var dateField = document.getElementById('<%= txtCustomDate.ClientID %>');
            var timeField = document.getElementById('<%= txtCustomTime.ClientID %>');
            
            if (dateField && timeField) {
                var inputDateTime = new Date(dateField.value + 'T' + timeField.value);
                var now = new Date();
                var thirtyDaysAgo = new Date(now.getTime() - (30 * 24 * 60 * 60 * 1000));
                
                if (inputDateTime > now) {
                    alert('La date/heure ne peut pas être dans le futur');
                    return false;
                }
                
                if (inputDateTime < thirtyDaysAgo) {
                    alert('La date/heure ne peut pas être antérieure à 30 jours');
                    return false;
                }
            }
            
            return true;
        }

        // Run on page load
        document.addEventListener('DOMContentLoaded', function() {
            highlightEditRow();
            
            // Add validation to custom save button
            var saveBtn = document.getElementById('<%= btnSaveCustom.ClientID %>');
            if (saveBtn) {
                saveBtn.onclick = function(e) {
                    if (!validateCustomTime()) {
                        e.preventDefault();
                        return false;
                    }
                };
            }
        });
    </script>
</asp:Content>