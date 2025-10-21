<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EditPointagePort.aspx.cs" Inherits="APPLICATION_RH.pointagfix.EditPointagePort" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Toast Container -->
        <div class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 9999;">
            <div id="toastContainer"></div>
        </div>

        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-4 fw-semibold m-0">
                    <i class="fas fa-edit me-2"></i>Modifier/Annuler Pointages
                </h4>
                <p class="text-muted mb-0">Recherchez et modifiez les pointages existants</p>
            </div>
            <div class="text-end">
                <nav aria-label="breadcrumb">
                    <ol class="breadcrumb m-0 py-0">
                        <li class="breadcrumb-item">
                            <asp:HyperLink ID="hlDashboard" runat="server" NavigateUrl="~/Default.aspx">Dashboard</asp:HyperLink>
                        </li>
                        <li class="breadcrumb-item">
                            <asp:HyperLink ID="hlPointage" runat="server" NavigateUrl="~/pointage/GestionEquipes.aspx">Gestion des Équipes</asp:HyperLink>
                        </li>
                        <li class="breadcrumb-item active">Modifier Pointages</li>
                    </ol>
                </nav>
            </div>
        </div>

        <!-- Filter Panel -->
        <div class="card mb-4">
            <div class="card-header bg-primary text-white">
                <h5 class="card-title mb-0">
                    <i class="fas fa-filter me-2"></i>Filtres de Recherche
                </h5>
            </div>
            <div class="card-body">
                <div class="row g-3">
                    <div class="col-md-3">
                        <label for="<%= ddlPort.ClientID %>" class="form-label">Port</label>
                        <asp:DropDownList ID="ddlPort" runat="server" CssClass="form-select">
                        </asp:DropDownList>
                    </div>
                    <div class="col-md-3">
                        <label for="<%= txtDateFrom.ClientID %>" class="form-label">Date de début</label>
                        <asp:TextBox ID="txtDateFrom" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>
                    <div class="col-md-3">
                        <label for="<%= txtDateTo.ClientID %>" class="form-label">Date de fin</label>
                        <asp:TextBox ID="txtDateTo" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>
                    <div class="col-md-3">
                        <label for="<%= txtEmployeeSearch.ClientID %>" class="form-label">Employé</label>
                        <asp:TextBox ID="txtEmployeeSearch" runat="server" CssClass="form-control" placeholder="Nom ou matricule..." />
                    </div>
                </div>
                <div class="row g-3 mt-2">
                    <div class="col-md-3">
                        <label for="<%= ddlTypePointage.ClientID %>" class="form-label">Type</label>
                        <asp:DropDownList ID="ddlTypePointage" runat="server" CssClass="form-select">
                            <asp:ListItem Value="" Text="Tous les types" />
                            <asp:ListItem Value="ENTREE" Text="Entrées" />
                            <asp:ListItem Value="SORTIE" Text="Sorties" />
                        </asp:DropDownList>
                    </div>
                    <div class="col-md-6">
                        <label class="form-label d-block">&nbsp;</label>
                        <asp:Button ID="btnSearch" runat="server" Text="Rechercher" CssClass="btn btn-primary me-2" OnClick="btnSearch_Click" />
                        <asp:Button ID="btnClear" runat="server" Text="Effacer" CssClass="btn btn-outline-secondary me-2" OnClick="btnClear_Click" CausesValidation="false" />
                        <asp:Button ID="btnSetToday" runat="server" Text="Aujourd'hui" CssClass="btn btn-outline-info" OnClick="btnSetToday_Click" CausesValidation="false" />
                    </div>
                    <div class="col-md-3">
                        <label class="form-label d-block">&nbsp;</label>
                        <div class="text-end">
                            <asp:Literal ID="litResultCount" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Results Panel -->
        <asp:Panel ID="pnlResults" runat="server" Visible="false">
            <div class="card">
                <div class="card-header bg-light">
                    <div class="d-flex justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-list me-2"></i>Pointages Trouvés
                        </h5>
                        <asp:Button ID="btnRefresh" runat="server" Text="↻ Actualiser" CssClass="btn btn-outline-primary btn-sm" OnClick="btnSearch_Click" CausesValidation="false" />
                    </div>
                </div>
                <div class="card-body p-0">
                    <div class="table-responsive" style="max-height: 500px; overflow-y: auto;">
                        <asp:GridView ID="gvPointages" runat="server" CssClass="table table-striped table-hover mb-0"
                            AutoGenerateColumns="false" DataKeyNames="IdPointage" 
                            OnRowCommand="gvPointages_RowCommand" 
                            OnRowEditing="gvPointages_RowEditing"
                            OnRowUpdating="gvPointages_RowUpdating" 
                            OnRowCancelingEdit="gvPointages_RowCancelingEdit"
                            EmptyDataText="Aucun pointage trouvé avec les critères sélectionnés">
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
                                        <div class="small">
                                            <div class="fw-medium"><%# Convert.ToDateTime(Eval("DateHeure")).ToString("dd/MM/yyyy") %></div>
                                            <div class="text-muted"><%# Convert.ToDateTime(Eval("DateHeure")).ToString("HH:mm") %></div>
                                        </div>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <div class="row g-1">
                                            <div class="col">
                                                <asp:TextBox ID="txtEditDate" runat="server"
                                                    Text='<%# Convert.ToDateTime(Eval("DateHeure")).ToString("yyyy-MM-dd") %>'
                                                    TextMode="Date" CssClass="form-control form-control-sm" />
                                            </div>
                                            <div class="col">
                                                <asp:TextBox ID="txtEditTime" runat="server"
                                                    Text='<%# Convert.ToDateTime(Eval("DateHeure")).ToString("HH:mm") %>'
                                                    TextMode="Time" CssClass="form-control form-control-sm" />
                                            </div>
                                        </div>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="AssignePar" HeaderText="Créé par" ReadOnly="true" />
                                <asp:TemplateField HeaderText="Statut">
                                    <ItemTemplate>
                                        <span class='<%# "badge " + (Convert.ToBoolean(Eval("is_deleted")) ? "bg-warning" : "bg-success") %>'>
                                            <%# Convert.ToBoolean(Eval("is_deleted")) ? "Annulé" : "Actif" %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions" ItemStyle-Width="200px">
                                    <ItemTemplate>
                                        <div class="btn-group btn-group-sm" role="group">
                                            <asp:LinkButton ID="btnEdit" runat="server" CommandName="Edit" 
                                                CssClass='<%# "btn btn-outline-primary" + (Convert.ToBoolean(Eval("is_deleted")) ? " disabled" : "") %>'
                                                Enabled='<%# !Convert.ToBoolean(Eval("is_deleted")) %>'
                                                ToolTip="Modifier">
                                                <i class="fas fa-edit"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnCancel" runat="server" CommandName="CancelPointage"
                                                CommandArgument='<%# Eval("IdPointage") %>'
                                                CssClass='<%# "btn btn-outline-warning" + (Convert.ToBoolean(Eval("is_deleted")) ? " disabled" : "") %>'
                                                Enabled='<%# !Convert.ToBoolean(Eval("is_deleted")) %>'
                                                OnClientClick="return confirm('Êtes-vous sûr de vouloir annuler ce pointage?');"
                                                ToolTip="Annuler">
                                                <i class="fas fa-ban"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnRestore" runat="server" CommandName="RestorePointage"
                                                CommandArgument='<%# Eval("IdPointage") %>'
                                                CssClass='<%# "btn btn-outline-success" + (!Convert.ToBoolean(Eval("is_deleted")) ? " disabled" : "") %>'
                                                Visible='<%# Convert.ToBoolean(Eval("is_deleted")) %>'
                                                OnClientClick="return confirm('Êtes-vous sûr de vouloir restaurer ce pointage?');"
                                                ToolTip="Restaurer">
                                                <i class="fas fa-undo"></i>
                                            </asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <div class="btn-group btn-group-sm" role="group">
                                            <asp:LinkButton ID="btnSave" runat="server" CommandName="Update" 
                                                CssClass="btn btn-success">
                                                <i class="fas fa-save"></i> Sauver
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnCancelEdit" runat="server" CommandName="Cancel" 
                                                CssClass="btn btn-secondary">
                                                <i class="fas fa-times"></i> Annuler
                                            </asp:LinkButton>
                                        </div>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="text-center py-4">
                                    <i class="fas fa-search fa-2x text-muted mb-3"></i>
                                    <h6 class="text-muted">Aucun pointage trouvé</h6>
                                    <p class="text-muted small">Essayez de modifier vos critères de recherche</p>
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Warning Panel for Business Rules -->
        <asp:Panel ID="pnlWarning" runat="server" Visible="false" CssClass="mt-3">
            <div class="alert alert-warning alert-dismissible fade show" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>
                <strong>Attention!</strong>
                <asp:Literal ID="litWarningMessage" runat="server" />
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        // Toast notifications
        function showToast(type, message) {
            var container = document.getElementById('toastContainer');
            var toastId = 'toast-' + Date.now();
            
            var toastHtml = `
                <div id="${toastId}" class="toast align-items-center text-white bg-${type === 'success' ? 'success' : 'danger'} border-0" role="alert">
                    <div class="d-flex">
                        <div class="toast-body">
                            <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>
                            ${message}
                        </div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                    </div>
                </div>
            `;
            
            container.innerHTML += toastHtml;
            var toastElement = new bootstrap.Toast(document.getElementById(toastId));
            toastElement.show();
            
            // Auto remove after 5 seconds
            setTimeout(function() {
                var element = document.getElementById(toastId);
                if (element) element.remove();
            }, 5000);
        }

        // Quick date setters
        function setDateRange(days) {
            var today = new Date();
            var fromDate = new Date(today);
            fromDate.setDate(today.getDate() - days);
            
            document.getElementById('<%= txtDateFrom.ClientID %>').value = fromDate.toISOString().split('T')[0];
            document.getElementById('<%= txtDateTo.ClientID %>').value = today.toISOString().split('T')[0];
        }

        // Keyboard shortcuts
        document.addEventListener('keydown', function(e) {
            if (e.ctrlKey && e.key === 'f') {
                e.preventDefault();
                document.getElementById('<%= txtEmployeeSearch.ClientID %>').focus();
            }
        });

        // Auto-focus on employee search when page loads
        document.addEventListener('DOMContentLoaded', function() {
            var employeeSearch = document.getElementById('<%= txtEmployeeSearch.ClientID %>');
            if (employeeSearch && employeeSearch.value === '') {
                employeeSearch.focus();
            }
        });
    </script>
</asp:Content>


