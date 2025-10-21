<%@ Page Title="Gestion des Rôles" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RollesManagment.aspx.cs" Inherits="APPLICATION_RH.permissions.RollesManagment" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .card { margin-bottom: 20px; }
        .btn-group-sm > .btn { margin-right: 5px; }
        
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

            .toast.success {
                background-color: #28a745;
            }

            .toast.error {
                background-color: #dc3545;
            }
            
            .toast.warning {
                background-color: #ffc107;
                color: #212529;
            }
            
            .toast.info {
                background-color: #17a2b8;
            }
    </style>
    
    <script type="text/javascript">
        // Toast notification system - same as PointageEmployesPorts.aspx
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
        
        // Test toast function
        function testToast() {
            console.log('Testing toast function...');
            showToast('info', 'Test de notification - Le système fonctionne correctement !');
        }
        
        // Enhanced delete confirmation - using SweetAlert2 if available, fallback to confirm
        function confirmDelete(button) {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'Confirmer la suppression',
                    text: 'Êtes-vous sûr de vouloir supprimer ce rôle ? Cette action est irréversible.',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#d33',
                    cancelButtonColor: '#3085d6',
                    confirmButtonText: 'Oui, supprimer',
                    cancelButtonText: 'Annuler',
                    reverseButtons: true
                }).then((result) => {
                    if (result.isConfirmed) {
                        // Trigger the actual postback
                        __doPostBack(button.name, '');
                    }
                });
                return false; // Prevent default postback
            } else {
                return confirm('Êtes-vous sûr de vouloir supprimer ce rôle ?');
            }
        }
        
        // Debug function - call this in browser console
        function debugToast() {
            console.log('=== Toast Debug Info ===');
            console.log('showToast function:', typeof showToast);
            console.log('SweetAlert2 (Swal):', typeof Swal);
            console.log('Toast container:', document.getElementById('toastContainer'));
            testToast();
        }
        
        // Initialize when page loads
        $(document).ready(function() {
            console.log('Page loaded, notification system ready');
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Toast Container -->
        <div class="toast-container" id="toastContainer"></div>
        <div class="row">
            <div class="col-12">
                <div class="page-title-box">
                    <h4 class="page-title">Gestion des Rôles</h4>
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item"><a href="~/Default.aspx">Dashboard</a></li>
                        <li class="breadcrumb-item active">Gestion des Rôles</li>
                    </ol>
                </div>
            </div>
        </div>

        <!-- Add New Role Card -->
        <div class="row">
            <div class="col-md-4">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-plus-circle me-2"></i>Ajouter un Rôle
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="mb-3">
                            <label class="form-label">Nom du Rôle</label>
                            <asp:TextBox ID="txtRoleName" runat="server" CssClass="form-control" 
                                         placeholder="Ex: Manager, Superviseur..." MaxLength="50" />
                            <asp:RequiredFieldValidator ID="rfvRoleName" runat="server" 
                                ControlToValidate="txtRoleName" 
                                ErrorMessage="Le nom du rôle est requis" 
                                CssClass="text-danger small" Display="Dynamic" 
                                ValidationGroup="AddRole" />
                        </div>
                        
                        <div class="mb-3">
                            <label class="form-label">Description</label>
                            <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" 
                                         TextMode="MultiLine" Rows="3"
                                         placeholder="Description du rôle..." MaxLength="255" />
                        </div>

                        <div class="mb-3">
                            <div class="form-check">
                                <asp:CheckBox ID="chkIsActive" runat="server" CssClass="form-check-input" Checked="true" />
                                <label class="form-check-label">
                                    Rôle actif
                                </label>
                            </div>
                        </div>

                        <div class="d-grid">
                            <asp:Button ID="btnAddRole" runat="server" CssClass="btn btn-primary" 
                                        Text="Ajouter le Rôle" OnClick="btnAddRole_Click" 
                                        ValidationGroup="AddRole" />
                        </div>
                    </div>
                </div>
            </div>

            <!-- Roles List -->
            <div class="col-md-8">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-users-cog me-2"></i>Liste des Rôles
                        </h5>
                        <div>
                            <asp:Button ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm" 
                                        Text="Actualiser" OnClick="btnRefresh_Click" />
                        </div>
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvRoles" runat="server" CssClass="table table-striped table-hover" 
                                      AutoGenerateColumns="false" DataKeyNames="IdRole"
                                      OnRowEditing="gvRoles_RowEditing" OnRowCancelingEdit="gvRoles_RowCancelingEdit"
                                      OnRowUpdating="gvRoles_RowUpdating" OnRowDeleting="gvRoles_RowDeleting"
                                      EmptyDataText="Aucun rôle trouvé">
                            <Columns>
                                <asp:BoundField DataField="IdRole" HeaderText="ID" ReadOnly="true" 
                                               ItemStyle-Width="60px" />
                                
                                <asp:TemplateField HeaderText="Nom du Rôle">
                                    <ItemTemplate>
                                        <strong><%# Eval("RoleName") %></strong>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtEditRoleName" runat="server" 
                                                     Text='<%# Bind("RoleName") %>' 
                                                     CssClass="form-control form-control-sm" MaxLength="50" />
                                    </EditItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Description">
                                    <ItemTemplate>
                                        <%# Eval("Description") %>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtEditDescription" runat="server" 
                                                     Text='<%# Bind("Description") %>' 
                                                     CssClass="form-control form-control-sm" 
                                                     TextMode="MultiLine" Rows="2" MaxLength="255" />
                                    </EditItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Statut" ItemStyle-Width="80px">
                                    <ItemTemplate>
                                        <span class="badge bg-<%# Convert.ToBoolean(Eval("IsActive")) ? "success" : "secondary" %>">
                                            <%# Convert.ToBoolean(Eval("IsActive")) ? "Actif" : "Inactif" %>
                                        </span>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:CheckBox ID="chkEditIsActive" runat="server" 
                                                     Checked='<%# Bind("IsActive") %>' 
                                                     CssClass="form-check-input" />
                                    </EditItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Actions" ItemStyle-Width="120px">
                                    <ItemTemplate>
                                        <div class="btn-group btn-group-sm">
                                            <asp:LinkButton ID="btnEdit" runat="server" CommandName="Edit" 
                                                           CssClass="btn btn-outline-primary btn-sm" 
                                                           ToolTip="Modifier">
                                                <i class="fas fa-edit"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnDelete" runat="server" CommandName="Delete" 
                                                           CssClass="btn btn-outline-danger btn-sm" 
                                                           ToolTip="Supprimer"
                                                           OnClientClick="return confirmDelete(this);">
                                                <i class="fas fa-trash"></i>
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
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>