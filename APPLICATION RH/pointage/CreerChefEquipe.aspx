<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CreerChefEquipe.aspx.cs" Inherits="APPLICATION_RH.pointage.CreerChefEquipe" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Créer Chef d'Équipe</h4>
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
                    <li class="breadcrumb-item active">Créer Chef d'Équipe</li>
                </ol>
            </div>
        </div>

        <!-- Main Content -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-user-tie me-2"></i>Informations du Chef d'Équipe
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <!-- Left Column -->
                            <div class="col-lg-6">
                                <!-- Society Selection -->
                                <div class="mb-3">
                                    <asp:Label ID="lblSociete" runat="server" Text="Société *" CssClass="form-label" AssociatedControlID="ddlSociete" />
                                    <asp:DropDownList ID="ddlSociete" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlSociete_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Sélectionner Société --" />
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="rfvSociete" runat="server"
                                        ControlToValidate="ddlSociete"
                                        InitialValue=""
                                        ErrorMessage="La société est obligatoire"
                                        CssClass="text-danger"
                                        Display="Dynamic" />
                                </div>

                                <!-- Team Name -->
                                <div class="mb-3">
                                    <asp:Label ID="lblNomEquipe" runat="server" Text="Nom de l'Équipe *" CssClass="form-label" AssociatedControlID="txtNomEquipe" />
                                    <asp:TextBox ID="txtNomEquipe" runat="server" CssClass="form-control" MaxLength="100"
                                        placeholder="Ex: Équipe Production A, Équipe Qualité B..." />
                                    <asp:RequiredFieldValidator ID="rfvNomEquipe" runat="server"
                                        ControlToValidate="txtNomEquipe"
                                        ErrorMessage="Le nom de l'équipe est obligatoire"
                                        CssClass="text-danger"
                                        Display="Dynamic" />
                                    <div class="form-text">Le nom doit être unique</div>
                                </div>

                                <!-- Assignment Date -->
                                <div class="mb-3">
                                    <asp:Label ID="lblDateAssignation" runat="server" Text="Date d'Assignation *" CssClass="form-label" AssociatedControlID="txtDateAssignation" />
                                    <asp:TextBox ID="txtDateAssignation" runat="server" CssClass="form-control" TextMode="Date" />
                                    <asp:RequiredFieldValidator ID="rfvDateAssignation" runat="server"
                                        ControlToValidate="txtDateAssignation"
                                        ErrorMessage="La date d'assignation est obligatoire"
                                        CssClass="text-danger"
                                        Display="Dynamic" />
                                    <div class="form-text">Date d'entrée en fonction du chef d'équipe</div>
                                </div>
                            </div>

                            <!-- Right Column -->
                            <div class="col-lg-6">
                                <!-- Employee Search -->
                                <div class="mb-3">
                                    <asp:Label ID="lblSearchEmployee" runat="server" Text="Rechercher Employé par Nom ou Matricule *" CssClass="form-label" AssociatedControlID="txtSearchEmployee" />
                                    <div class="input-group">
                                        <asp:TextBox ID="txtSearchEmployee" runat="server" CssClass="form-control"
                                            placeholder="Tapez le nom, prénom ou matricule de l'employé..." />
                                        <asp:Button ID="btnSearchEmployee" runat="server" Text="Rechercher"
                                            CssClass="btn btn-outline-primary" OnClick="btnSearchEmployee_Click" CausesValidation="false" />
                                    </div>
                                    <div class="form-text">Saisissez au moins 2 caractères pour rechercher par nom/prénom ou le matricule complet</div>
                                </div>

                                <!-- Employee Selection -->
                                <asp:Panel ID="pnlEmployeeSelection" runat="server" Visible="false" CssClass="mb-3">
                                    <asp:Label ID="lblEmploye" runat="server" Text="Sélectionner l'Employé *" CssClass="form-label" AssociatedControlID="ddlEmploye" />
                                    <asp:DropDownList ID="ddlEmploye" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEmploye_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Sélectionner Employé --" />
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="rfvEmploye" runat="server"
                                        ControlToValidate="ddlEmploye"
                                        InitialValue=""
                                        ErrorMessage="La sélection d'un employé est obligatoire"
                                        CssClass="text-danger"
                                        Display="Dynamic" />
                                </asp:Panel>

                                <!-- Employee Info Display -->
                                <asp:Panel ID="pnlEmployeeInfo" runat="server" Visible="false" CssClass="mb-3">
                                    <div class="card bg-light">
                                        <div class="card-body p-3">
                                            <h6 class="card-title mb-2">Informations de l'Employé</h6>
                                            <div class="row">
                                                <div class="col-6">
                                                    <small class="text-muted">Matricule:</small><br>
                                                    <asp:Literal ID="litMatricule" runat="server" />
                                                </div>
                                                <div class="col-6">
                                                    <small class="text-muted">Type:</small><br>
                                                    <asp:Literal ID="litTypeEmploye" runat="server" />
                                                </div>
                                                <div class="col-12 mt-2">
                                                    <small class="text-muted">Département:</small><br>
                                                    <asp:Literal ID="litDepartementEmploye" runat="server" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>

                        <!-- Validation Messages -->
                        <asp:Panel ID="pnlValidationMessage" runat="server" Visible="false" CssClass="alert alert-warning">
                            <h6 class="alert-heading">Attention!</h6>
                            <asp:Literal ID="litValidationMessage" runat="server" />
                        </asp:Panel>

                        <!-- Action Buttons -->
                        <div class="row mt-4">
                            <div class="col-12">
                                <div class="d-flex gap-2">
                                    <asp:Button ID="btnCreer" runat="server" Text="Créer Chef d'Équipe"
                                        CssClass="btn btn-primary" OnClick="btnCreer_Click" />
                                    <asp:Button ID="btnAnnuler" runat="server" Text="Annuler"
                                        CssClass="btn btn-secondary" OnClick="btnAnnuler_Click" CausesValidation="false" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Current Chef Equipes List -->
        <div class="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-list me-2"></i>Chefs d'Équipe Existants
                        </h5>
                        <asp:Button ID="btnRefresh" runat="server" Text="Actualiser"
                            CssClass="btn btn-outline-primary btn-sm" OnClick="btnRefresh_Click" CausesValidation="false" />
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvChefEquipes" runat="server"
                            CssClass="table table-striped table-hover"
                            AutoGenerateColumns="false"
                            OnRowCommand="gvChefEquipes_RowCommand"
                            EmptyDataText="Aucun chef d'équipe trouvé">
                            <Columns>
                                <asp:BoundField DataField="NomEquipe" HeaderText="Nom Équipe" />
                                <asp:BoundField DataField="NomChef" HeaderText="Chef d'Équipe" />
                                <asp:BoundField DataField="MatriculeChef" HeaderText="Matricule" />
                                <asp:BoundField DataField="NomSociete" HeaderText="Société" />
                                <asp:BoundField DataField="NomDepartement" HeaderText="Département" />
                                <asp:BoundField DataField="DateAssignation" HeaderText="Date Assignation" DataFormatString="{0:dd/MM/yyyy}" />
                                <asp:BoundField DataField="NombreEmployes" HeaderText="Employés" />
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnSupprimer" runat="server"
                                            CommandName="SupprimerChef"
                                            CommandArgument='<%# Eval("IdChefEquipe") %>'
                                            CssClass="btn btn-sm btn-outline-danger"
                                            data-bs-toggle="tooltip"
                                            data-bs-original-title="Supprimer"
                                            OnClientClick="return confirm('Êtes-vous sûr de vouloir supprimer ce chef d\'équipe?');">
                                            <i class="fas fa-trash"></i>
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
