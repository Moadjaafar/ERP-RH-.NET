<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AssignerEmployes.aspx.cs" Inherits="APPLICATION_RH.pointage.AssignerEmployes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid">
        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Assignation en Masse des Employés</h4>
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
                    <li class="breadcrumb-item active">Assignation en Masse</li>
                </ol>
            </div>
        </div>

        <!-- Search Filters -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-filter me-2"></i>Filtres de Recherche
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblSociete" runat="server" Text="Société *" CssClass="form-label" AssociatedControlID="ddlSociete" />
                                    <asp:DropDownList ID="ddlSociete" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlSociete_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Sélectionner --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblDepartement" runat="server" Text="Département" CssClass="form-label" AssociatedControlID="ddlDepartement" />
                                    <asp:DropDownList ID="ddlDepartement" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlDepartement_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Tous --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblService" runat="server" Text="Service" CssClass="form-label" AssociatedControlID="ddlService" />
                                    <asp:DropDownList ID="ddlService" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlService_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Tous --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <asp:Label ID="lblCellule" runat="server" Text="Cellule" CssClass="form-label" AssociatedControlID="ddlCellule" />
                                    <asp:DropDownList ID="ddlCellule" runat="server" CssClass="form-select">
                                        <asp:ListItem Value="" Text="-- Toutes --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-12">
                                <asp:Button ID="btnSearch" runat="server" Text="Rechercher Employés" 
                                    CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                                <asp:Button ID="btnClearFilters" runat="server" Text="Effacer Filtres" 
                                    CssClass="btn btn-outline-secondary ms-2" OnClick="btnClearFilters_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Employee Results -->
        <asp:Panel ID="pnlResults" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-users me-2"></i>Employés Disponibles 
                            <span class="badge bg-primary ms-2">
                                <asp:Literal ID="litEmployeeCount" runat="server" />
                            </span>
                        </h5>
                        <div>
                            <asp:Button ID="btnSelectAll" runat="server" Text="Tout Sélectionner" 
                                CssClass="btn btn-outline-primary btn-sm me-2" OnClick="btnSelectAll_Click" CausesValidation="false" />
                            <asp:Button ID="btnUnselectAll" runat="server" Text="Tout Désélectionner" 
                                CssClass="btn btn-outline-secondary btn-sm" OnClick="btnUnselectAll_Click" CausesValidation="false" />
                        </div>
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvEmployees" runat="server" 
                            CssClass="table table-striped table-hover" 
                            AutoGenerateColumns="false"
                            EmptyDataText="Aucun employé disponible trouvé">
                            <Columns>
                                <asp:TemplateField HeaderText="Sélection">
                                    <HeaderTemplate>
                                        <asp:CheckBox ID="chkSelectAllHeader" runat="server" />
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkSelect" runat="server" />
                                        <asp:HiddenField ID="hfIdEmploye" runat="server" Value='<%# Eval("IdEmploye") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Matricule" HeaderText="Matricule" />
                                <asp:BoundField DataField="NomComplet" HeaderText="Nom Complet" />
                                <asp:BoundField DataField="TypeEmploye" HeaderText="Type" />
                                <asp:BoundField DataField="NomDepartement" HeaderText="Département" />
                                <asp:BoundField DataField="NomService" HeaderText="Service" />
                                <asp:BoundField DataField="NomCellule" HeaderText="Cellule" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Assignment Panel -->
        <asp:Panel ID="pnlAssignment" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-user-plus me-2"></i>Assignation aux Équipes
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <asp:Label ID="lblChefEquipe" runat="server" Text="Chef d'Équipe *" CssClass="form-label" AssociatedControlID="ddlChefEquipe" />
                                    <asp:DropDownList ID="ddlChefEquipe" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlChefEquipe_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Sélectionner Chef d'Équipe --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <asp:Label ID="lblDateAssignation" runat="server" Text="Date d'Assignation *" CssClass="form-label" AssociatedControlID="txtDateAssignation" />
                                    <asp:TextBox ID="txtDateAssignation" runat="server" CssClass="form-control" TextMode="Date" />
                                </div>
                            </div>
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <label class="form-label">&nbsp;</label>
                                    <div>
                                        <asp:Button ID="btnAssignSelected" runat="server" Text="Assigner les Sélectionnés" 
                                            CssClass="btn btn-success w-100" OnClick="btnAssignSelected_Click" />
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Team Info Display -->
                        <asp:Panel ID="pnlTeamInfo" runat="server" Visible="false" CssClass="mt-3">
                            <div class="alert alert-info">
                                <h6 class="alert-heading">Informations de l'Équipe</h6>
                                <div class="row">
                                    <div class="col-md-4">
                                        <strong>Équipe:</strong> <asp:Literal ID="litNomEquipe" runat="server" />
                                    </div>
                                    <div class="col-md-4">
                                        <strong>Chef:</strong> <asp:Literal ID="litNomChef" runat="server" />
                                    </div>
                                    <div class="col-md-4">
                                        <strong>Employés actuels:</strong> <asp:Literal ID="litNombreEmployes" runat="server" />
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Recent Assignments -->
        <div class="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-history me-2"></i>Assignations Récentes
                        </h5>
                        <asp:Button ID="btnRefresh" runat="server" Text="Actualiser" 
                            CssClass="btn btn-outline-primary btn-sm" OnClick="btnRefresh_Click" CausesValidation="false" />
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvRecentAssignments" runat="server" 
                            CssClass="table table-striped table-hover" 
                            AutoGenerateColumns="false"
                            OnRowCommand="gvRecentAssignments_RowCommand"
                            EmptyDataText="Aucune assignation récente">
                            <Columns>
                                <asp:BoundField DataField="NomEmploye" HeaderText="Employé" />
                                <asp:BoundField DataField="MatriculeEmploye" HeaderText="Matricule" />
                                <asp:BoundField DataField="NomEquipe" HeaderText="Équipe" />
                                <asp:BoundField DataField="NomChef" HeaderText="Chef d'Équipe" />
                                <asp:BoundField DataField="DateAssignation" HeaderText="Date Assignation" DataFormatString="{0:dd/MM/yyyy}" />
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnRetirer" runat="server"
                                            CommandName="RetirerEmploye"
                                            CommandArgument='<%# Eval("IdEmployeChefEquipe") %>'
                                            CssClass="btn btn-sm btn-outline-danger"
                                            OnClientClick="return confirm('Retirer cet employé de l\'équipe?');">
                                            <i class="fas fa-user-minus"></i> Retirer
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

    <script type="text/javascript">
        // Handle select all checkbox in header
        function toggleSelectAll(selectAllCheckbox) {
            var checkboxes = document.querySelectorAll('[id*="chkSelect"]');
            checkboxes.forEach(function(checkbox) {
                if (checkbox !== selectAllCheckbox) {
                    checkbox.checked = selectAllCheckbox.checked;
                }
            });
        }

        // Add event listener for header checkbox
        document.addEventListener('DOMContentLoaded', function() {
            var headerCheckbox = document.querySelector('[id*="chkSelectAllHeader"]');
            if (headerCheckbox) {
                headerCheckbox.addEventListener('change', function() {
                    toggleSelectAll(this);
                });
            }
        });
    </script>
</asp:Content>
