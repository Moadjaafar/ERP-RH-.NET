<%@ Page Title="Gestion Rôle-Pages" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RollePage.aspx.cs" Inherits="APPLICATION_RH.permissions.RollePage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .stats-card {
            border-left: 4px solid #007bff;
            transition: transform 0.2s;
        }
        .stats-card:hover {
            transform: translateY(-2px);
        }
        .permission-matrix-container {
            max-height: 500px;
            overflow: auto;
            border: 1px solid #dee2e6;
            border-radius: 0.375rem;
        }
        .permission-table {
            width: 100%;
            margin: 0;
        }
        .permission-table th {
            background-color: #495057;
            color: white;
            text-align: center;
            padding: 12px 8px;
            position: sticky;
            top: 0;
            z-index: 10;
        }
        .permission-table td {
            text-align: center;
            padding: 8px;
            border: 1px solid #dee2e6;
        }
        .permission-table .page-cell {
            text-align: left;
            position: sticky;
            left: 0;
            background-color: white;
            z-index: 5;
            min-width: 200px;
        }
        .permission-table .page-cell strong {
            color: #495057;
        }
        .permission-table .menu-badge {
            font-size: 0.7em;
            padding: 2px 6px;
        }
        .permission-checkbox {
            transform: scale(1.3);
            margin: 0;
        }
        .bulk-actions {
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            border-radius: 0.5rem;
            padding: 20px;
            margin-bottom: 20px;
        }
        .role-header-btn {
            font-size: 0.7em;
            padding: 2px 6px;
            margin: 2px 1px;
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
        .toast.success { background-color: #28a745; }
        .toast.error { background-color: #dc3545; }
        .toast.warning { background-color: #ffc107; color: #212529; }
        .toast.info { background-color: #17a2b8; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Toast Container -->
        <div class="toast-container" id="toastContainer"></div>
        
        <!-- Page Header -->
        <div class="row mb-4">
            <div class="col-12">
                <h4 class="page-title">Gestion des Permissions Rôle-Pages</h4>
                <nav aria-label="breadcrumb">
                    <ol class="breadcrumb">
                        <li class="breadcrumb-item"><a href="../Default.aspx">Dashboard</a></li>
                        <li class="breadcrumb-item">Permissions</li>
                        <li class="breadcrumb-item active">Rôle-Pages</li>
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
                            <asp:Literal ID="litTotalRoles" runat="server" Text="0" />
                        </h3>
                        <p class="mb-0">Rôles Actifs</p>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card stats-card">
                    <div class="card-body text-center">
                        <h3 class="text-success">
                            <asp:Literal ID="litTotalPages" runat="server" Text="0" />
                        </h3>
                        <p class="mb-0">Pages Actives</p>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card stats-card">
                    <div class="card-body text-center">
                        <h3 class="text-info">
                            <asp:Literal ID="litTotalPermissions" runat="server" Text="0" />
                        </h3>
                        <p class="mb-0">Permissions Accordées</p>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card stats-card">
                    <div class="card-body text-center">
                        <h3 class="text-warning">
                            <asp:Literal ID="litPermissionPercentage" runat="server" Text="0%" />
                        </h3>
                        <p class="mb-0">% Couverture</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Bulk Actions -->
        <div class="bulk-actions">
            <div class="row align-items-center">
                <div class="col-md-12 text-end">
                    <asp:Button ID="btnSaveAllPermissions" runat="server" CssClass="btn btn-primary me-2" 
                                Text="💾 Enregistrer Tout" OnClick="btnSaveAllPermissions_Click" />
                    <asp:Button ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary" 
                                Text="🔄 Actualiser" OnClick="btnRefresh_Click" />
                </div>
            </div>
        </div>

        <!-- Role Selection for Matrix -->
        <div class="card mb-4">
            <div class="card-header">
                <h5 class="card-title mb-0">
                    <i class="fas fa-user-shield me-2"></i>Sélection du Rôle
                </h5>
            </div>
            <div class="card-body">
                <div class="row align-items-center">
                    <div class="col-md-6">
                        <label class="form-label">Choisir un rôle pour afficher ses permissions:</label>
                        <asp:DropDownList ID="ddlMatrixRole" runat="server" CssClass="form-select" 
                                          AutoPostBack="true" OnSelectedIndexChanged="ddlMatrixRole_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>
                    <div class="col-md-6">
                        <asp:Panel ID="pnlMatrixInfo" runat="server" Visible="false" CssClass="alert alert-info mb-0">
                            <strong>Rôle sélectionné:</strong> <asp:Literal ID="litSelectedRole" runat="server" />
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>

        <!-- Permissions Matrix -->
        <div class="card">
            <div class="card-header">
                <h5 class="card-title mb-0">
                    <i class="fas fa-shield-alt me-2"></i>Permissions du Rôle
                </h5>
            </div>
            <div class="card-body p-0">
                <div class="permission-matrix-container">
                    <asp:Literal ID="litPermissionMatrix" runat="server" />
                </div>
            </div>
        </div>

        <!-- Individual Permission Management -->
        <div class="row mt-4">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-user-shield me-2"></i>Gestion par Rôle
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="mb-3">
                            <label class="form-label">Sélectionner un Rôle</label>
                            <asp:DropDownList ID="ddlRoleManagement" runat="server" CssClass="form-select" 
                                              AutoPostBack="true" OnSelectedIndexChanged="ddlRoleManagement_SelectedIndexChanged">
                            </asp:DropDownList>
                        </div>
                        <asp:Panel ID="pnlRolePermissions" runat="server" Visible="false">
                            <h6>Pages autorisées pour ce rôle:</h6>
                            <div style="max-height: 300px; overflow-y: auto; border: 1px solid #dee2e6; border-radius: 0.375rem; padding: 15px;">
                                <asp:CheckBoxList ID="cblRolePages" runat="server" CssClass="list-unstyled" 
                                                  RepeatLayout="UnorderedList">
                                </asp:CheckBoxList>
                            </div>
                            <div class="mt-3">
                                <asp:Button ID="btnSaveRolePermissions" runat="server" CssClass="btn btn-primary btn-sm" 
                                            Text="💾 Sauvegarder" OnClick="btnSaveRolePermissions_Click" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>

            <div class="col-md-6">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-file-shield me-2"></i>Gestion par Page
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="mb-3">
                            <label class="form-label">Sélectionner une Page</label>
                            <asp:DropDownList ID="ddlPageManagement" runat="server" CssClass="form-select" 
                                              AutoPostBack="true" OnSelectedIndexChanged="ddlPageManagement_SelectedIndexChanged">
                            </asp:DropDownList>
                        </div>
                        <asp:Panel ID="pnlPagePermissions" runat="server" Visible="false">
                            <h6>Rôles autorisés pour cette page:</h6>
                            <div style="max-height: 300px; overflow-y: auto; border: 1px solid #dee2e6; border-radius: 0.375rem; padding: 15px;">
                                <asp:CheckBoxList ID="cblPageRoles" runat="server" CssClass="list-unstyled" 
                                                  RepeatLayout="UnorderedList">
                                </asp:CheckBoxList>
                            </div>
                            <div class="mt-3">
                                <asp:Button ID="btnSavePagePermissions" runat="server" CssClass="btn btn-primary btn-sm" 
                                            Text="💾 Sauvegarder" OnClick="btnSavePagePermissions_Click" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </div>

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

        // Toggle permissions functions
        function toggleAllPermissions(grant) {
            var checkboxes = document.querySelectorAll('.permission-checkbox');
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].checked = grant;
            }
            showToast('info', grant ? 'Toutes les permissions accordées' : 'Toutes les permissions révoquées');
        }

        function toggleRolePermissions(roleId, grant) {
            var checkboxes = document.querySelectorAll('.permission-checkbox[data-role="' + roleId + '"]');
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].checked = grant;
            }
            showToast('info', grant ? 'Permissions accordées pour le rôle' : 'Permissions révoquées pour le rôle');
        }

        function togglePagePermissions(pageId, grant) {
            var checkboxes = document.querySelectorAll('.permission-checkbox[data-page="' + pageId + '"]');
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].checked = grant;
            }
            showToast('info', grant ? 'Permissions accordées pour la page' : 'Permissions révoquées pour la page');
        }

        // Initialize page
        document.addEventListener('DOMContentLoaded', function() {
            console.log('Role-Page management loaded');
        });
    </script>
</asp:Content>