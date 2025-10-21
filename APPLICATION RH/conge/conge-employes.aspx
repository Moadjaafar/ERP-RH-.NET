<%@ Page Title="Gestion des Absences" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="conge-employes.aspx.cs" Inherits="APPLICATION_RH.conge.conge_employes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .motif-badge {
            padding: 5px 10px;
            border-radius: 4px;
            font-weight: 500;
            color: white;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Gestion des Absences Employes</h4>
            </div>
            <div class="text-end">
                <ol class="breadcrumb m-0 py-0">
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlDashboard" runat="server" NavigateUrl="~/Default.aspx">Dashboard</asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item">Conges</li>
                    <li class="breadcrumb-item active">Gestion Absences</li>
                </ol>
            </div>
        </div>

        <!-- Employee Filters -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-filter me-2"></i>Filtres de Recherche Employe
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <label class="form-label">Societe</label>
                                    <asp:DropDownList ID="ddlSociete" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlSociete_SelectedIndexChanged">
                                        <asp:ListItem Value="">-- Toutes --</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <label class="form-label">Departement</label>
                                    <asp:DropDownList ID="ddlDepartement" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlDepartement_SelectedIndexChanged">
                                        <asp:ListItem Value="">-- Tous --</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <label class="form-label">Service</label>
                                    <asp:DropDownList ID="ddlService" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlService_SelectedIndexChanged">
                                        <asp:ListItem Value="">-- Tous --</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-3">
                                <div class="mb-3">
                                    <label class="form-label">Cellule</label>
                                    <asp:DropDownList ID="ddlCellule" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCellule_SelectedIndexChanged">
                                        <asp:ListItem Value="">-- Toutes --</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Add Absence Form -->
        <div class="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header bg-primary text-white">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-plus-circle me-2"></i>
                            <asp:Literal ID="litFormTitle" runat="server" Text="Ajouter une Absence" />
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-4">
                                <div class="mb-3">
                                    <label class="form-label">Employe *</label>
                                    <asp:DropDownList ID="ddlEmploye" runat="server" CssClass="form-select">
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="rfvEmploye" runat="server"
                                        ControlToValidate="ddlEmploye" InitialValue=""
                                        ErrorMessage="Employe obligatoire" CssClass="text-danger" Display="Dynamic" />
                                </div>
                            </div>

                            <div class="col-md-4">
                                <div class="mb-3">
                                    <label class="form-label">Motif *</label>
                                    <asp:DropDownList ID="ddlMotif" runat="server" CssClass="form-select">
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="rfvMotif" runat="server"
                                        ControlToValidate="ddlMotif" InitialValue=""
                                        ErrorMessage="Motif obligatoire" CssClass="text-danger" Display="Dynamic" />
                                </div>
                            </div>

                            <div class="col-md-4">
                                <div class="mb-3">
                                    <label class="form-label">Date Debut *</label>
                                    <asp:TextBox ID="txtDateDebut" runat="server" CssClass="form-control" TextMode="Date" />
                                    <asp:RequiredFieldValidator ID="rfvDateDebut" runat="server"
                                        ControlToValidate="txtDateDebut"
                                        ErrorMessage="Date debut obligatoire" CssClass="text-danger" Display="Dynamic" />
                                </div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-4">
                                <div class="mb-3">
                                    <label class="form-label">Date Fin *</label>
                                    <asp:TextBox ID="txtDateFin" runat="server" CssClass="form-control" TextMode="Date" />
                                    <asp:RequiredFieldValidator ID="rfvDateFin" runat="server"
                                        ControlToValidate="txtDateFin"
                                        ErrorMessage="Date fin obligatoire" CssClass="text-danger" Display="Dynamic" />
                                </div>
                            </div>

                            <div class="col-md-8">
                                <div class="mb-3">
                                    <label class="form-label">Description</label>
                                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control"
                                        TextMode="MultiLine" Rows="2" placeholder="Details de l'absence..." />
                                </div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-12">
                                <asp:Button ID="btnSave" runat="server" Text="Enregistrer"
                                    CssClass="btn btn-primary me-2" OnClick="btnSave_Click" />
                                <asp:Button ID="btnCancel" runat="server" Text="Annuler"
                                    CssClass="btn btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
                                <asp:HiddenField ID="hfEditId" runat="server" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Absences List -->
        <div class="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-list me-2"></i>Liste des Absences
                        </h5>
                    </div>
                    <div class="card-body p-0">
                        <div class="table-responsive">
                            <asp:GridView ID="gvAbsences" runat="server" CssClass="table table-hover mb-0"
                                AutoGenerateColumns="false" DataKeyNames="IdAbsence"
                                OnRowCommand="gvAbsences_RowCommand"
                                EmptyDataText="Aucune absence enregistree">
                                <Columns>
                                    <asp:BoundField DataField="Matricule" HeaderText="Matricule" />
                                    <asp:BoundField DataField="NomComplet" HeaderText="Employe" />
                                    <asp:TemplateField HeaderText="Motif">
                                        <ItemTemplate>
                                            <span class="motif-badge" style='background-color: <%# Eval("CouleurBadge") %>;'>
                                                <%# Eval("NomMotif") %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="DateDebut" HeaderText="Date Debut" DataFormatString="{0:dd/MM/yyyy}" />
                                    <asp:BoundField DataField="DateFin" HeaderText="Date Fin" DataFormatString="{0:dd/MM/yyyy}" />
                                    <asp:BoundField DataField="NombreJours" HeaderText="Jours" />
                                    <asp:BoundField DataField="Description" HeaderText="Description" />
                                    <asp:TemplateField HeaderText="Actions">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btnEdit" runat="server"
                                                CommandName="EditAbsence" CommandArgument='<%# Eval("IdAbsence") %>'
                                                CssClass="btn btn-sm btn-outline-primary me-1" CausesValidation="false">
                                                <i class="fas fa-edit"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnDelete" runat="server"
                                                CommandName="DeleteAbsence" CommandArgument='<%# Eval("IdAbsence") %>'
                                                CssClass="btn btn-sm btn-outline-danger" CausesValidation="false"
                                                OnClientClick="return confirm('Supprimer cette absence?');">
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
    </div>
</asp:Content>
