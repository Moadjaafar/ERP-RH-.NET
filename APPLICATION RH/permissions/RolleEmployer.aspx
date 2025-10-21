<%@ Page Title="Attribution des Rôles aux Employés" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RolleEmployer.aspx.cs" Inherits="APPLICATION_RH.permissions.RolleEmployer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .card { margin-bottom: 20px; }
        .btn-group-sm > .btn { margin-right: 5px; }
        .employee-card {
            border-left: 4px solid #007bff;
            transition: transform 0.2s;
        }
        .employee-card:hover {
            transform: translateY(-2px);
        }
        .role-badge {
            font-size: 0.85em;
            padding: 0.4rem 0.8rem;
        }
        .employee-search {
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            border-radius: 0.5rem;
            padding: 20px;
            margin-bottom: 20px;
        }
        .stats-card {
            border-left: 4px solid #28a745;
            transition: transform 0.2s;
        }
        .stats-card:hover {
            transform: translateY(-2px);
        }
        
        /* Toast notification styles */
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

        .toast.success { background-color: #28a745; }
        .toast.error { background-color: #dc3545; }
        .toast.warning { background-color: #ffc107; color: #212529; }
        .toast.info { background-color: #17a2b8; }
    </style>
    
    <script type="text/javascript">
        // Toast notification system
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
        
        // Enhanced delete confirmation
        function confirmRoleChange(employeeName, newRole) {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'Confirmer le changement de rôle',
                    text: `Voulez-vous vraiment changer le rôle de ${employeeName} vers "${newRole}" ?`,
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: '#007bff',
                    cancelButtonColor: '#6c757d',
                    confirmButtonText: 'Oui, changer',
                    cancelButtonText: 'Annuler',
                    reverseButtons: true
                }).then((result) => {
                    if (result.isConfirmed) {
                        return true;
                    }
                    return false;
                });
            } else {
                return confirm(`Voulez-vous vraiment changer le rôle de ${employeeName} ?`);
            }
        }
        
        // Initialize when page loads
        $(document).ready(function() {
            console.log('Employee-Role management loaded');
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Toast Container -->
        <div class="toast-container" id="toastContainer"></div>
        
        <!-- Page Header -->
        <div class="row mb-4">
            <div class="col-12">
                <h4 class="page-title">Attribution des Rôles aux Employés</h4>
                <nav aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item"><a href="../Default.aspx">Dashboard</a></li>
                        <li class="breadcrumb-item">Permissions</li>
                        <li class="breadcrumb-item active">Rôles Employés</li>
                    </ol>
                </nav>
            </div>
        </div>

        <!-- Statistics Cards -->
        <div class="row mb-4">
            <div class="col-md-3">
                <div class="card stats-card">
                    <div class="card-body text-center">
                        <h3 class="text-primary">
                            <asp:Literal ID="litTotalEmployees" runat="server" Text="0" />
                        </h3>
                        <p class="mb-0">Employés Actifs</p>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card stats-card">
                    <div class="card-body text-center">
                        <h3 class="text-success">
                            <asp:Literal ID="litEmployeesWithRoles" runat="server" Text="0" />
                        </h3>
                        <p class="mb-0">Avec Rôles</p>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card stats-card">
                    <div class="card-body text-center">
                        <h3 class="text-warning">
                            <asp:Literal ID="litEmployeesWithoutRoles" runat="server" Text="0" />
                        </h3>
                        <p class="mb-0">Sans Rôles</p>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card stats-card">
                    <div class="card-body text-center">
                        <h3 class="text-info">
                            <asp:Literal ID="litActiveRoles" runat="server" Text="0" />
                        </h3>
                        <p class="mb-0">Rôles Disponibles</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Search and Filters -->
        <div class="employee-search">
            <div class="row align-items-end">
                <div class="col-md-3">
                    <label class="form-label">Société</label>
                    <asp:DropDownList ID="ddlSociete" runat="server" CssClass="form-select" 
                                      AutoPostBack="true" OnSelectedIndexChanged="ddlSociete_SelectedIndexChanged">
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Département</label>
                    <asp:DropDownList ID="ddlDepartement" runat="server" CssClass="form-select" 
                                      AutoPostBack="true" OnSelectedIndexChanged="ddlDepartement_SelectedIndexChanged">
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Statut Rôle</label>
                    <asp:DropDownList ID="ddlRoleStatus" runat="server" CssClass="form-select" 
                                      AutoPostBack="true" OnSelectedIndexChanged="ddlRoleStatus_SelectedIndexChanged">
                        <asp:ListItem Text="Tous" Value="ALL" />
                        <asp:ListItem Text="Avec rôles" Value="WITH_ROLES" />
                        <asp:ListItem Text="Sans rôles" Value="WITHOUT_ROLES" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Recherche</label>
                    <div class="input-group">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" 
                                     placeholder="Nom, Prénom, Matricule..." />
                        <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-outline-primary" 
                                    Text="🔍" OnClick="btnSearch_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Bulk Role Assignment -->
        <div class="card mb-4">
            <div class="card-header">
                <h5 class="card-title mb-0">
                    <i class="fas fa-users-cog me-2"></i>Attribution de Rôle en Masse
                </h5>
            </div>
            <div class="card-body">
                <div class="row align-items-end">
                    <div class="col-md-4">
                        <label class="form-label">Sélectionner un Rôle</label>
                        <asp:DropDownList ID="ddlBulkRole" runat="server" CssClass="form-select">
                        </asp:DropDownList>
                    </div>
                    <div class="col-md-4">
                        <label class="form-label">Appliquer à</label>
                        <asp:DropDownList ID="ddlBulkTarget" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Employés sélectionnés" Value="SELECTED" />
                            <asp:ListItem Text="Tous les employés visibles" Value="ALL_VISIBLE" />
                            <asp:ListItem Text="Employés sans rôles" Value="WITHOUT_ROLES" />
                        </asp:DropDownList>
                    </div>
                    <div class="col-md-4">
                        <asp:Button ID="btnBulkAssign" runat="server" CssClass="btn btn-warning w-100" 
                                    Text="Attribuer en Masse" OnClick="btnBulkAssign_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Employees List -->
        <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
                <h5 class="card-title mb-0">
                    <i class="fas fa-users me-2"></i>Liste des Employés
                    <asp:Literal ID="litEmployeeCount" runat="server" />
                </h5>
                <div>
                    <asp:Button ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm" 
                                Text="🔄 Actualiser" OnClick="btnRefresh_Click" />
                </div>
            </div>
            <div class="card-body">
                <asp:GridView ID="gvEmployees" runat="server" CssClass="table table-striped table-hover" 
                              AutoGenerateColumns="false" DataKeyNames="IdEmploye"
                              OnRowEditing="gvEmployees_RowEditing" OnRowCancelingEdit="gvEmployees_RowCancelingEdit"
                              OnRowUpdating="gvEmployees_RowUpdating" OnRowDataBound="gvEmployees_RowDataBound"
                              EmptyDataText="Aucun employé trouvé" AllowPaging="true" PageSize="20"
                              OnPageIndexChanging="gvEmployees_PageIndexChanging">
                    <Columns>
                        <asp:TemplateField HeaderText="Sélection" ItemStyle-Width="50px">
                            <HeaderTemplate>
                                <asp:CheckBox ID="chkSelectAll" runat="server" onclick="toggleAllCheckboxes(this);" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="chkSelect" runat="server" CssClass="employee-select" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        
                        <asp:BoundField DataField="Matricule" HeaderText="Matricule" ReadOnly="true" 
                                       ItemStyle-Width="100px" />
                        
                        <asp:TemplateField HeaderText="Employé">
                            <ItemTemplate>
                                <div>
                                    <strong><%# Eval("Nom") %> <%# Eval("Prenom") %></strong><br />
                                    <small class="text-muted"><%# Eval("Email") %></small>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Organisation">
                            <ItemTemplate>
                                <div>
                                    <span class="badge bg-info"><%# Eval("NomSociete") %></span><br />
                                    <small><%# Eval("NomDepartement") ?? "Non assigné" %></small>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Rôle Actuel">
                            <ItemTemplate>
                                <%# GetRoleBadge(Eval("RoleName"), Eval("IdRole")) %>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:DropDownList ID="ddlEditRole" runat="server" CssClass="form-select form-select-sm"
                                                 DataTextField="RoleName" DataValueField="IdRole">
                                </asp:DropDownList>
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Type Employé" ItemStyle-Width="100px">
                            <ItemTemplate>
                                <span class="badge bg-secondary"><%# Eval("TypeEmploye") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Statut" ItemStyle-Width="80px">
                            <ItemTemplate>
                                <span class="badge bg-<%# Convert.ToBoolean(Eval("is_deleted")) ? "danger" : "success" %>">
                                    <%# Convert.ToBoolean(Eval("is_deleted")) ? "Inactif" : "Actif" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Actions" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <div class="btn-group btn-group-sm">
                                    <asp:LinkButton ID="btnEdit" runat="server" CommandName="Edit" 
                                                   CssClass="btn btn-outline-primary btn-sm" 
                                                   ToolTip="Modifier le rôle">
                                        <i class="fas fa-edit"></i>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnViewPermissions" runat="server" 
                                                   CssClass="btn btn-outline-info btn-sm" 
                                                   ToolTip="Voir les permissions"
                                                   OnClick="btnViewPermissions_Click"
                                                   CommandArgument='<%# Eval("IdEmploye") %>'>
                                        <i class="fas fa-shield-alt"></i>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <div class="btn-group btn-group-sm">
                                    <asp:LinkButton ID="btnUpdate" runat="server" CommandName="Update" 
                                                   CssClass="btn btn-success btn-sm" 
                                                   ToolTip="Enregistrer">
                                        <i class="fas fa-check"></i>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnCancel" runat="server" CommandName="Cancel" 
                                                   CssClass="btn btn-secondary btn-sm" 
                                                   ToolTip="Annuler">
                                        <i class="fas fa-times"></i>
                                    </asp:LinkButton>
                                </div>
                            </EditItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <HeaderStyle CssClass="table-dark" />
                    <EditRowStyle CssClass="table-warning" />
                    <PagerStyle CssClass="pagination justify-content-center" />
                </asp:GridView>
            </div>
        </div>

        <!-- Employee Permissions Modal Placeholder -->
        <asp:Panel ID="pnlPermissionsModal" runat="server" Visible="false" CssClass="modal-overlay">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Permissions de l'Employé</h5>
                </div>
                <div class="modal-body">
                    <asp:Literal ID="litEmployeePermissions" runat="server" />
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnCloseModal" runat="server" CssClass="btn btn-secondary" 
                                Text="Fermer" OnClick="btnCloseModal_Click" />
                </div>
            </div>
        </asp:Panel>

    </div>

    <script type="text/javascript">
        // Toggle all checkboxes
        function toggleAllCheckboxes(headerCheckbox) {
            var checkboxes = document.querySelectorAll('.employee-select input[type="checkbox"]');
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].checked = headerCheckbox.checked;
            }
        }

        // Confirm bulk assignment
        function confirmBulkAssignment() {
            var selectedRole = document.getElementById('<%= ddlBulkRole.ClientID %>');
            var target = document.getElementById('<%= ddlBulkTarget.ClientID %>');
            
            if (selectedRole.value === '') {
                showToast('warning', 'Veuillez sélectionner un rôle à attribuer.');
                return false;
            }
            
            var message = `Voulez-vous vraiment attribuer le rôle "${selectedRole.options[selectedRole.selectedIndex].text}" ${target.options[target.selectedIndex].text.toLowerCase()} ?`;
            
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'Confirmer l\'attribution en masse',
                    text: message,
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: '#ffc107',
                    cancelButtonColor: '#6c757d',
                    confirmButtonText: 'Oui, attribuer',
                    cancelButtonText: 'Annuler'
                }).then((result) => {
                    if (result.isConfirmed) {
                        return true;
                    }
                    return false;
                });
            } else {
                return confirm(message);
            }
        }
    </script>
</asp:Content>