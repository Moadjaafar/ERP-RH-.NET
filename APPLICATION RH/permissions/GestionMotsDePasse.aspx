<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GestionMotsDePasse.aspx.cs" Inherits="APPLICATION_RH.permissions.GestionMotsDePasse" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        
        <!-- Page Title -->
        <div class="row">
            <div class="col-12">
                <div class="page-title-box">
                    <div class="page-title-right">
                        <ol class="breadcrumb m-0">
                            <li class="breadcrumb-item"><a href="javascript: void(0);">Sécurité</a></li>
                            <li class="breadcrumb-item active">Gestion des Mots de Passe</li>
                        </ol>
                    </div>
                    <h4 class="page-title">Gestion des Mots de Passe</h4>
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

        <!-- Password Change Form (Hidden by default) -->
        <asp:Panel ID="pnlPasswordForm" runat="server" Visible="false">
            <div class="row">
                <div class="col-12">
                    <div class="card border-primary">
                        <div class="card-header bg-primary text-white">
                            <h5 class="card-title mb-0">
                                <i class="mdi mdi-account-edit me-2"></i>
                                Modifier Informations - <asp:Literal ID="litSelectedEmployee" runat="server"></asp:Literal>
                            </h5>
                        </div>
                        <div class="card-body">
                            <asp:HiddenField ID="hfSelectedEmployeeId" runat="server" />
                            
                            <div class="row">
                                <div class="col-12">
                                    <div class="form-group mb-3">
                                        <label for="txtEmail" class="form-label">
                                            Adresse Email <span class="text-danger">*</span>
                                        </label>
                                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" 
                                            TextMode="SingleLine" placeholder="Entrez l'adresse email"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                                            ControlToValidate="txtEmail"
                                            ErrorMessage="L'email est requis"
                                            CssClass="text-danger"
                                            ValidationGroup="PasswordGroup"
                                            Display="Dynamic" />
<%--                                        <asp:RegularExpressionValidator ID="revEmail" runat="server"
                                            ControlToValidate="txtEmail"
                                            ErrorMessage="Format d'email invalide"
                                            CssClass="text-danger"
                                            ValidationGroup="PasswordGroup"
                                            Display="Dynamic" />--%>
                                    </div>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <div class="form-group mb-3">
                                        <label for="txtNewPassword" class="form-label">
                                            Nouveau Mot de Passe
                                        </label>
                                        <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" 
                                            placeholder="Entrez le nouveau mot de passe (laisser vide pour ne pas changer)"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="form-group mb-3">
                                        <label for="txtConfirmPassword" class="form-label">
                                            Confirmer Mot de Passe
                                        </label>
                                        <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" 
                                            placeholder="Confirmez le mot de passe"></asp:TextBox>
                                        <asp:CompareValidator ID="cvPassword" runat="server"
                                            ControlToValidate="txtConfirmPassword"
                                            ControlToCompare="txtNewPassword"
                                            ErrorMessage="Les mots de passe ne correspondent pas"
                                            CssClass="text-danger"
                                            ValidationGroup="PasswordGroup"
                                            Display="Dynamic" />
                                    </div>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-12">
                                    <div class="form-group mb-3">
                                        <asp:CheckBox ID="chkForcePasswordChange" runat="server" CssClass="form-check-input" />
                                        <label class="form-check-label ms-2" for="<%= chkForcePasswordChange.ClientID %>">
                                            Obliger l'utilisateur à changer le mot de passe lors de la prochaine connexion
                                        </label>
                                    </div>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-12">
                                    <div class="d-flex justify-content-between">
                                        <div>
                                            <asp:Button ID="btnGeneratePassword" runat="server" 
                                                Text="Générer Mot de Passe Aléatoire" 
                                                CssClass="btn btn-outline-info me-2"
                                                OnClick="btnGeneratePassword_Click"
                                                CausesValidation="false" />
                                            
                                            <asp:Button ID="btnResetEmail" runat="server" 
                                                Text="Réinitialiser Email" 
                                                CssClass="btn btn-outline-warning"
                                                OnClick="btnResetEmail_Click"
                                                CausesValidation="false" />
                                        </div>
                                        
                                        <div>
                                            <asp:Button ID="btnCancelPasswordChange" runat="server" 
                                                Text="Annuler" 
                                                CssClass="btn btn-secondary me-2"
                                                OnClick="btnCancelPasswordChange_Click"
                                                CausesValidation="false" />
                                            <asp:Button ID="btnSavePassword" runat="server" 
                                                Text="Enregistrer Modifications" 
                                                CssClass="btn btn-primary"
                                                OnClick="btnSavePassword_Click"
                                                ValidationGroup="PasswordGroup" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <br />
        </asp:Panel>

        <!-- Search and Filter Row -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-body">
                        <div class="row g-2">
                            <div class="col-md-4">
                                <div class="form-group">
                                    <label for="txtSearch" class="form-label">Rechercher (Nom, Prénom, Matricule)</label>
                                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" 
                                        placeholder="Tapez pour rechercher..." AutoPostBack="true" 
                                        OnTextChanged="txtSearch_TextChanged"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="form-group">
                                    <label for="ddlRoleFilter" class="form-label">Filtrer par Rôle</label>
                                    <asp:DropDownList ID="ddlRoleFilter" runat="server" CssClass="form-select" 
                                        AutoPostBack="true" OnSelectedIndexChanged="ddlRoleFilter_SelectedIndexChanged">
                                        <asp:ListItem Value="">Tous les Rôles</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="form-group">
                                    <label for="ddlPasswordStatus" class="form-label">Statut Mot de Passe</label>
                                    <asp:DropDownList ID="ddlPasswordStatus" runat="server" CssClass="form-select"
                                        AutoPostBack="true" OnSelectedIndexChanged="ddlPasswordStatus_SelectedIndexChanged">
                                        <asp:ListItem Value="">Tous</asp:ListItem>
                                        <asp:ListItem Value="HasPassword">Avec Mot de Passe</asp:ListItem>
                                        <asp:ListItem Value="NoPassword">Sans Mot de Passe</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="form-group">
                                    <label>&nbsp;</label><br />
                                    <asp:Button ID="btnSearch" runat="server" Text="Rechercher" 
                                        CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Users Grid -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-body">
                        <div class="table-responsive">
                            <asp:GridView ID="gvUsers" runat="server" CssClass="table table-striped table-bordered nowrap w-100" 
                                AutoGenerateColumns="false" OnRowCommand="gvUsers_RowCommand" 
                                OnRowDataBound="gvUsers_RowDataBound" DataKeyNames="IdEmploye"
                                AllowPaging="true" PageSize="20" OnPageIndexChanging="gvUsers_PageIndexChanging">
                                <Columns>
                                    <asp:BoundField DataField="Matricule" HeaderText="Matricule" ItemStyle-Width="100px" />
                                    <asp:BoundField DataField="NomPrenom" HeaderText="Nom Complet" ItemStyle-Width="200px" />
                                    <asp:BoundField DataField="Email" HeaderText="Email" ItemStyle-Width="200px" />
                                    <asp:BoundField DataField="Rolle" HeaderText="Rôle" ItemStyle-Width="120px" />
                                    <asp:TemplateField HeaderText="Statut Mot de Passe" ItemStyle-Width="150px" ItemStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <asp:Label ID="lblPasswordStatus" runat="server" 
                                                Text='<%# !string.IsNullOrEmpty(Eval("PasswordHash").ToString()) ? "Configuré" : "Non Configuré" %>'
                                                CssClass='<%# !string.IsNullOrEmpty(Eval("PasswordHash").ToString()) ? "badge bg-success" : "badge bg-warning" %>'>
                                            </asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Mot de Passe Actuel" ItemStyle-Width="150px">
                                        <ItemTemplate>
                                            <asp:Label ID="lblCurrentPassword" runat="server" 
                                                Text='<%# !string.IsNullOrEmpty(Eval("PasswordHash").ToString()) ? Eval("PasswordHash").ToString() : "Aucun" %>'
                                                CssClass="text-monospace small">
                                            </asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="DateCreation" HeaderText="Créé le" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="100px" />
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="250px" ItemStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <asp:Button ID="btnEditUser" runat="server" 
                                                Text="Modifier" 
                                                CssClass="btn btn-sm btn-outline-primary me-1"
                                                CommandName="EditUser" 
                                                CommandArgument='<%# Eval("IdEmploye") %>'
                                                ToolTip="Modifier email et mot de passe" />
                                            <asp:Button ID="btnGenerateQuickPassword" runat="server" 
                                                Text="Générer MDP" 
                                                CssClass="btn btn-sm btn-outline-success me-1"
                                                CommandName="GeneratePassword" 
                                                CommandArgument='<%# Eval("IdEmploye") %>'
                                                ToolTip="Générer un nouveau mot de passe"
                                                OnClientClick="return confirm('Voulez-vous générer un nouveau mot de passe pour cet utilisateur?');" />
                                            <asp:Button ID="btnResetQuickEmail" runat="server" 
                                                Text="Reset Email" 
                                                CssClass="btn btn-sm btn-outline-warning"
                                                CommandName="ResetEmail" 
                                                CommandArgument='<%# Eval("IdEmploye") %>'
                                                ToolTip="Réinitialiser l'email"
                                                OnClientClick="return confirm('Voulez-vous réinitialiser l\\'email de cet utilisateur?');" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <PagerSettings Mode="NumericFirstLast" PageButtonCount="5" FirstPageText="Premier" LastPageText="Dernier" />
                                <PagerStyle CssClass="pagination justify-content-center" />
                                <EmptyDataTemplate>
                                    <div class="text-center py-4">
                                        <i class="mdi mdi-account-off" style="font-size: 3rem; color: #6c757d;"></i>
                                        <h5 class="mt-2 text-muted">Aucun employé trouvé</h5>
                                        <p class="text-muted">Aucun employé ne correspond aux critères de recherche.</p>
                                    </div>
                                </EmptyDataTemplate>
                            </asp:GridView>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>