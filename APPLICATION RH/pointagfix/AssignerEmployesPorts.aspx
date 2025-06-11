<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AssignerEmployesPorts.aspx.cs" Inherits="APPLICATION_RH.pointagfix.AssignerEmployesPorts" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid">
        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Assignation des Employés aux Ports</h4>
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
                    <li class="breadcrumb-item active">Assignation aux Ports</li>
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
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <asp:Label ID="lblSociete" runat="server" Text="Société *" CssClass="form-label" AssociatedControlID="ddlSociete" />
                                    <asp:DropDownList ID="ddlSociete" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlSociete_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Sélectionner --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <asp:Label ID="lblDepartement" runat="server" Text="Département" CssClass="form-label" AssociatedControlID="ddlDepartement" />
                                    <asp:DropDownList ID="ddlDepartement" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlDepartement_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Tous --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <asp:Label ID="lblMatricule" runat="server" Text="Matricule" CssClass="form-label" AssociatedControlID="txtMatricule" />
                                    <asp:TextBox ID="txtMatricule" runat="server" CssClass="form-control" placeholder="Rechercher par matricule..." />
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
                                <asp:TemplateField HeaderText="Port Actuel">
                                    <ItemTemplate>
                                        <span class="badge bg-info"><%# Eval("PortActuel") ?? "Aucun port" %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Port Assignment Panel -->
        <asp:Panel ID="pnlAssignment" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-door-open me-2"></i>Assignation aux Ports
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <asp:Label ID="lblPort" runat="server" Text="Port d'Entrée *" CssClass="form-label" AssociatedControlID="ddlPort" />
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
                                    <asp:Label ID="lblNote" runat="server" Text="Note (Optionnel)" CssClass="form-label" AssociatedControlID="txtNote" />
                                    <asp:TextBox ID="txtNote" runat="server" CssClass="form-control" placeholder="Note ou commentaire..." />
                                </div>
                            </div>
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <label class="form-label">&nbsp;</label>
                                    <div>
                                        <asp:Button ID="btnAssignSelected" runat="server" Text="Assigner au Port" 
                                            CssClass="btn btn-success w-100" OnClick="btnAssignSelected_Click" />
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Port Info Display -->
                        <asp:Panel ID="pnlPortInfo" runat="server" Visible="false" CssClass="mt-3">
                            <div class="alert alert-info">
                                <h6 class="alert-heading">
                                    <i class="fas fa-door-open me-2"></i>Informations du Port
                                </h6>
                                <div class="row">
                                    <div class="col-md-4">
                                        <strong>Port:</strong> <asp:Literal ID="litNomPort" runat="server" />
                                    </div>
                                    <div class="col-md-4">
                                        <strong>Type:</strong> <asp:Literal ID="litTypePort" runat="server" />
                                    </div>
                                    <div class="col-md-4">
                                        <strong>Employés assignés:</strong> <asp:Literal ID="litNombreEmployesPort" runat="server" />
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Recent Port Assignments -->
        <div class="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-history me-2"></i>Assignations aux Ports Récentes
                        </h5>
                        <asp:Button ID="btnRefresh" runat="server" Text="Actualiser" 
                            CssClass="btn btn-outline-primary btn-sm" OnClick="btnRefresh_Click" CausesValidation="false" />
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvRecentPortAssignments" runat="server" 
                            CssClass="table table-striped table-hover" 
                            AutoGenerateColumns="false"
                            OnRowCommand="gvRecentPortAssignments_RowCommand"
                            EmptyDataText="Aucune assignation récente">
                            <Columns>
                                <asp:BoundField DataField="NomEmploye" HeaderText="Employé" />
                                <asp:BoundField DataField="MatriculeEmploye" HeaderText="Matricule" />
                                <asp:TemplateField HeaderText="Port Assigné">
                                    <ItemTemplate>
                                        <span class="badge bg-primary"><%# Eval("NomPort") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="DateCreation" HeaderText="Date Assignation" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                                <asp:BoundField DataField="AssignePar" HeaderText="Assigné par" />
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnRetirer" runat="server"
                                            CommandName="RetirerEmployePort"
                                            CommandArgument='<%# Eval("IdEmployePort") %>'
                                            CssClass="btn btn-sm btn-outline-danger"
                                            OnClientClick="return confirm('Retirer cet employé du port?');">
                                            <i class="fas fa-door-closed"></i> Retirer
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
