<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="APPLICATION_RH.permissions.Profile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        
        <!-- Page Title -->
        <div class="row">
            <div class="col-12">
                <div class="page-title-box">
                    <div class="page-title-right">
                        <ol class="breadcrumb m-0">
                            <li class="breadcrumb-item"><a href="javascript: void(0);">Compte</a></li>
                            <li class="breadcrumb-item active">Mon Profil</li>
                        </ol>
                    </div>
                    <h4 class="page-title">Mon Profil</h4>
                </div>
            </div>
        </div>

        <!-- Success/Error Messages -->
        <asp:Panel ID="pnlMessages" runat="server" Visible="false">
            <div class="row">
                <div class="col-12">
                    <div id="alertDiv" runat="server" class="alert alert-dismissible fade show" role="alert">
                        <asp:Literal ID="litMessage" runat="server"></asp:Literal>
                        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <div class="row">
            <!-- User Information Card -->
            <div class="col-lg-4">
                <div class="card">
                    <div class="card-body">
                        <div class="text-center">
                            <div class="mb-3">
                                <div class="avatar-lg mx-auto">
                                    <div class="avatar-title rounded-circle bg-primary text-white" style="font-size: 3rem;">
                                        <i class="mdi mdi-account"></i>
                                    </div>
                                </div>
                            </div>
                            
                            <h5 class="mb-1">
                                <asp:Literal ID="litFullName" runat="server"></asp:Literal>
                            </h5>
                            <p class="text-muted mb-3">
                                <asp:Literal ID="litRole" runat="server"></asp:Literal>
                            </p>
                        </div>

                        <div class="border-top pt-3">
                            <div class="row">
                                <div class="col-6">
                                    <div class="text-center">
                                        <h6 class="text-muted mb-0">Matricule</h6>
                                        <asp:Literal ID="litMatricule" runat="server"></asp:Literal>
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="text-center">
                                        <h6 class="text-muted mb-0">Département</h6>
                                        <asp:Literal ID="litDepartement" runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="border-top pt-3 mt-3">
                            <div class="row">
                                <div class="col-12">
                                    <h6 class="text-muted mb-2">Informations Personnelles</h6>
                                    <p class="mb-1"><i class="mdi mdi-email me-1"></i> 
                                        <asp:Literal ID="litCurrentEmail" runat="server"></asp:Literal>
                                    </p>
                                    <p class="mb-1"><i class="mdi mdi-phone me-1"></i> 
                                        <asp:Literal ID="litTelephone" runat="server"></asp:Literal>
                                    </p>
                                    <p class="mb-0"><i class="mdi mdi-calendar me-1"></i> 
                                        Embauché le <asp:Literal ID="litDateEmbauche" runat="server"></asp:Literal>
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Edit Profile Forms -->
            <div class="col-lg-8">
                
                <!-- Change Email Form -->
                <div class="card mb-3">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="mdi mdi-email-edit me-2"></i>
                            Modifier mon Email
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-8">
                                <div class="form-group mb-3">
                                    <label for="txtNewEmail" class="form-label">
                                         Adresse Email <span class="text-danger">*</span>
                                    </label>
                                    <asp:TextBox ID="txtNewEmail" Enabled="false" runat="server" CssClass="form-control" 
                                        TextMode="Email" placeholder="Entrez votre nouvelle adresse email"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfvNewEmail" runat="server"
                                        ControlToValidate="txtNewEmail"
                                        ErrorMessage="L'email est requis"
                                        CssClass="text-danger"
                                        ValidationGroup="EmailGroup"
                                        Display="Dynamic" />
                                    <asp:RegularExpressionValidator ID="revNewEmail" runat="server"
                                        ControlToValidate="txtNewEmail"
                                        ErrorMessage="Format d'email invalide"
                                        ValidationExpression="^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
                                        CssClass="text-danger"
                                        ValidationGroup="EmailGroup"
                                        Display="Dynamic" />
                                </div>
                            </div>
                            <div class="col-md-4">
                                <div class="form-group mb-3">
                                    <label>&nbsp;</label><br />
                                    <asp:Button ID="btnUpdateEmail" Visible="false" runat="server" 
                                        Text="Mettre à jour Email" 
                                        CssClass="btn btn-outline-primary w-100"
                                        OnClick="btnUpdateEmail_Click"
                                        ValidationGroup="EmailGroup" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Change Password Form -->
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="mdi mdi-lock-reset me-2"></i>
                            Changer mon Mot de Passe
                        </h5>
                    </div>
                    <div class="card-body">
                        
                        <!-- Current Password Display (Optional) -->
                        <div class="row mb-3">
                            <div class="col-12">
                                <div class="alert alert-info">
                                    <i class="mdi mdi-information me-2"></i>
                                    <strong>Mot de passe actuel:</strong> 
                                    <span class="text-monospace">
                                        <asp:Literal ID="litCurrentPassword" runat="server"></asp:Literal>
                                    </span>
                                </div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <div class="form-group mb-3">
                                    <label for="txtCurrentPassword" class="form-label">
                                        Mot de Passe Actuel <span class="text-danger">*</span>
                                    </label>
                                    <asp:TextBox ID="txtCurrentPassword" runat="server" CssClass="form-control" 
                                        TextMode="Password" placeholder="Entrez votre mot de passe actuel"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfvCurrentPassword" runat="server"
                                        ControlToValidate="txtCurrentPassword"
                                        ErrorMessage="Le mot de passe actuel est requis"
                                        CssClass="text-danger"
                                        ValidationGroup="PasswordGroup"
                                        Display="Dynamic" />
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="form-group mb-3">
                                    <label>&nbsp;</label><br />
                                    <small class="text-muted">
                                        Entrez votre mot de passe actuel pour confirmer votre identité.
                                    </small>
                                </div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <div class="form-group mb-3">
                                    <label for="txtNewPassword" class="form-label">
                                        Nouveau Mot de Passe <span class="text-danger">*</span>
                                    </label>
                                    <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" 
                                        TextMode="Password" placeholder="Entrez votre nouveau mot de passe"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfvNewPassword" runat="server"
                                        ControlToValidate="txtNewPassword"
                                        ErrorMessage="Le nouveau mot de passe est requis"
                                        CssClass="text-danger"
                                        ValidationGroup="PasswordGroup"
                                        Display="Dynamic" />
                                    <small class="form-text text-muted">
                                        Minimum 4 caractères recommandés.
                                    </small>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="form-group mb-3">
                                    <label for="txtConfirmNewPassword" class="form-label">
                                        Confirmer Nouveau Mot de Passe <span class="text-danger">*</span>
                                    </label>
                                    <asp:TextBox ID="txtConfirmNewPassword" runat="server" CssClass="form-control" 
                                        TextMode="Password" placeholder="Confirmez votre nouveau mot de passe"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfvConfirmNewPassword" runat="server"
                                        ControlToValidate="txtConfirmNewPassword"
                                        ErrorMessage="La confirmation est requise"
                                        CssClass="text-danger"
                                        ValidationGroup="PasswordGroup"
                                        Display="Dynamic" />
                                    <asp:CompareValidator ID="cvNewPassword" runat="server"
                                        ControlToValidate="txtConfirmNewPassword"
                                        ControlToCompare="txtNewPassword"
                                        ErrorMessage="Les nouveaux mots de passe ne correspondent pas"
                                        CssClass="text-danger"
                                        ValidationGroup="PasswordGroup"
                                        Display="Dynamic" />
                                </div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-12">
                                <div class="d-flex justify-content-between">
                                    <asp:Button ID="btnGenerateRandomPassword" runat="server" 
                                        Text="Générer Mot de Passe Aléatoire" 
                                        CssClass="btn btn-outline-info"
                                        OnClick="btnGenerateRandomPassword_Click"
                                        CausesValidation="false" />
                                    
                                    <div>
                                        <asp:Button ID="btnClearPasswordForm" runat="server" 
                                            Text="Effacer" 
                                            CssClass="btn btn-outline-secondary me-2"
                                            OnClick="btnClearPasswordForm_Click"
                                            CausesValidation="false" />
                                        <asp:Button ID="btnUpdatePassword" runat="server" 
                                            Text="Changer Mot de Passe" 
                                            CssClass="btn btn-primary"
                                            OnClick="btnUpdatePassword_Click"
                                            ValidationGroup="PasswordGroup" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Activity Log (Optional) -->
        <div class="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="mdi mdi-history me-2"></i>
                            Historique des Modifications
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="table-responsive">
                            <asp:GridView ID="gvUserActivity" runat="server" CssClass="table table-sm" 
                                AutoGenerateColumns="false" AllowPaging="true" PageSize="5" 
                                OnPageIndexChanging="gvUserActivity_PageIndexChanging">
                                <Columns>
                                    <asp:BoundField DataField="ActionDate" HeaderText="Date/Heure" 
                                        DataFormatString="{0:dd/MM/yyyy HH:mm}" ItemStyle-Width="150px" />
                                    <asp:BoundField DataField="Action" HeaderText="Action" ItemStyle-Width="200px" />
                                    <asp:BoundField DataField="IPAddress" HeaderText="Adresse IP" ItemStyle-Width="150px" />
                                </Columns>
                                <PagerSettings Mode="NumericFirstLast" PageButtonCount="3" 
                                    FirstPageText="«" LastPageText="»" />
                                <PagerStyle CssClass="pagination justify-content-center" />
                                <EmptyDataTemplate>
                                    <div class="text-center py-3 text-muted">
                                        <i class="mdi mdi-information-outline me-1"></i>
                                        Aucune activité récente trouvée.
                                    </div>
                                </EmptyDataTemplate>
                            </asp:GridView>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <style>
        .avatar-lg {
            height: 5rem;
            width: 5rem;
        }
        
        .avatar-title {
            align-items: center;
            background-color: #6c757d;
            color: #fff;
            display: flex;
            font-weight: 600;
            height: 100%;
            justify-content: center;
            width: 100%;
        }
        
        .page-title-box {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 20px;
            border-radius: 10px;
            margin-bottom: 20px;
        }
        
        .page-title-box .breadcrumb {
            background: transparent;
            margin-bottom: 0;
        }
        
        .page-title-box .breadcrumb-item a {
            color: rgba(255, 255, 255, 0.8);
            text-decoration: none;
        }
        
        .page-title-box .breadcrumb-item.active {
            color: white;
        }
        
        .card {
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
            border: none;
            border-radius: 8px;
        }
        
        .card-header {
            background-color: #f8f9fa;
            border-bottom: 1px solid #dee2e6;
        }
    </style>
</asp:Content>