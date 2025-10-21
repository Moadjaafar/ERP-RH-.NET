<%@ Page Title="Gestion des Pages" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PagesManagment.aspx.cs" Inherits="APPLICATION_RH.permissions.PagesManagment" %>

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
        
        // Enhanced delete confirmation - using SweetAlert2 if available, fallback to confirm
        function confirmDelete(button) {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'Confirmer la suppression',
                    text: 'Êtes-vous sûr de vouloir supprimer cette page ? Cette action est irréversible.',
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
                return confirm('Êtes-vous sûr de vouloir supprimer cette page ?');
            }
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
                    <h4 class="page-title">Gestion des Pages</h4>
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item"><a href="~/Default.aspx">Dashboard</a></li>
                        <li class="breadcrumb-item active">Gestion des Pages</li>
                    </ol>
                </div>
            </div>
        </div>

        <!-- Add New Page Card -->
        <div class="row">
            <div class="col-md-4">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-plus-circle me-2"></i>Ajouter une Page
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="mb-3">
                            <label class="form-label">Nom de la Page</label>
                            <asp:TextBox ID="txtPageName" runat="server" CssClass="form-control" 
                                         placeholder="Ex: Gestion des Employés..." MaxLength="100" />
                            <asp:RequiredFieldValidator ID="rfvPageName" runat="server" 
                                ControlToValidate="txtPageName" 
                                ErrorMessage="Le nom de la page est requis" 
                                CssClass="text-danger small" Display="Dynamic" 
                                ValidationGroup="AddPage" />
                        </div>
                        
                        <div class="mb-3">
                            <label class="form-label">URL de la Page</label>
                            <asp:TextBox ID="txtPageURL" runat="server" CssClass="form-control" 
                                         placeholder="Ex: ~/personnel/ListeEmployes.aspx" MaxLength="255" />
                            <asp:RequiredFieldValidator ID="rfvPageURL" runat="server" 
                                ControlToValidate="txtPageURL" 
                                ErrorMessage="L'URL de la page est requis" 
                                CssClass="text-danger small" Display="Dynamic" 
                                ValidationGroup="AddPage" />
                            <small class="text-muted">Format: ~/dossier/fichier.aspx</small>
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Groupe de Menu</label>
                            <asp:DropDownList ID="ddlMenuGroup" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Sélectionner un groupe..." Value="" />
                                <asp:ListItem Text="Dashboard" Value="Dashboard" />
                                <asp:ListItem Text="Personnel" Value="Personnel" />
                                <asp:ListItem Text="Pointage" Value="Pointage" />
                                <asp:ListItem Text="Permissions" Value="Permissions" />
                                <asp:ListItem Text="Rapports" Value="Rapports" />
                                <asp:ListItem Text="Administration" Value="Administration" />
                            </asp:DropDownList>
                        </div>

                        <div class="mb-3">
                            <div class="form-check">
                                <asp:CheckBox ID="chkIsActive" runat="server" CssClass="form-check-input" Checked="true" />
                                <label class="form-check-label">
                                    Page active
                                </label>
                            </div>
                        </div>

                        <hr />
                        <!-- Page Discovery Section -->
                        <div class="mb-3">
                            <label class="form-label">
                                <i class="fas fa-search me-2"></i>Découvrir les Pages
                            </label>
                            <div class="d-grid gap-2">
                                <asp:Button ID="btnDiscoverPages" runat="server" CssClass="btn btn-outline-info btn-sm" 
                                            Text="Scanner le Projet" OnClick="btnDiscoverPages_Click" 
                                            CausesValidation="false" />
                            </div>
                        </div>

                        <div class="mb-3">
                            <label class="form-label">Pages Découvertes</label>
                            <asp:DropDownList ID="ddlDiscoveredPages" runat="server" CssClass="form-select">
                                <asp:ListItem Text="-- Cliquez sur 'Scanner le Projet' d'abord --" Value="" />
                            </asp:DropDownList>
                        </div>

                        <div class="mb-3">
                            <div class="d-grid">
                                <asp:Button ID="btnUseDiscoveredPage" runat="server" CssClass="btn btn-outline-success btn-sm" 
                                            Text="Utiliser cette Page" OnClick="btnUseDiscoveredPage_Click" 
                                            CausesValidation="false" />
                            </div>
                        </div>

                        <hr />

                        <div class="d-grid">
                            <asp:Button ID="btnAddPage" runat="server" CssClass="btn btn-primary" 
                                        Text="Ajouter la Page" OnClick="btnAddPage_Click" 
                                        ValidationGroup="AddPage" />
                        </div>
                    </div>
                </div>
            </div>

            <!-- Pages List -->
            <div class="col-md-8">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-file-alt me-2"></i>Liste des Pages
                        </h5>
                        <div>
                            <asp:Button ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm" 
                                        Text="Actualiser" OnClick="btnRefresh_Click" />
                        </div>
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvPages" runat="server" CssClass="table table-striped table-hover" 
                                      AutoGenerateColumns="false" DataKeyNames="IdPage"
                                      OnRowEditing="gvPages_RowEditing" OnRowCancelingEdit="gvPages_RowCancelingEdit"
                                      OnRowUpdating="gvPages_RowUpdating" OnRowDeleting="gvPages_RowDeleting"
                                      EmptyDataText="Aucune page trouvée">
                            <Columns>
                                <asp:BoundField DataField="IdPage" HeaderText="ID" ReadOnly="true" 
                                               ItemStyle-Width="60px" />
                                
                                <asp:TemplateField HeaderText="Nom de la Page">
                                    <ItemTemplate>
                                        <strong><%# Eval("PageName") %></strong>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtEditPageName" runat="server" 
                                                     Text='<%# Bind("PageName") %>' 
                                                     CssClass="form-control form-control-sm" MaxLength="100" />
                                    </EditItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="URL">
                                    <ItemTemplate>
                                        <code><%# Eval("PageURL") %></code>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtEditPageURL" runat="server" 
                                                     Text='<%# Bind("PageURL") %>' 
                                                     CssClass="form-control form-control-sm" MaxLength="255" />
                                    </EditItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Groupe de Menu">
                                    <ItemTemplate>
                                        <span class="badge bg-info"><%# Eval("MenuGroup") %></span>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:DropDownList ID="ddlEditMenuGroup" runat="server" 
                                                         CssClass="form-select form-select-sm"
                                                         SelectedValue='<%# Bind("MenuGroup") %>'>
                                            <asp:ListItem Text="" Value="" />
                                            <asp:ListItem Text="Dashboard" Value="Dashboard" />
                                            <asp:ListItem Text="Personnel" Value="Personnel" />
                                            <asp:ListItem Text="Pointage" Value="Pointage" />
                                            <asp:ListItem Text="Permissions" Value="Permissions" />
                                            <asp:ListItem Text="Rapports" Value="Rapports" />
                                            <asp:ListItem Text="Administration" Value="Administration" />
                                        </asp:DropDownList>
                                    </EditItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Statut" ItemStyle-Width="80px">
                                    <ItemTemplate>
                                        <span class="badge bg-<%# Convert.ToBoolean(Eval("IsActive")) ? "success" : "secondary" %>">
                                            <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
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
