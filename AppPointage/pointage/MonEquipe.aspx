<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MonEquipe.aspx.cs" Inherits="APPLICATION_RH.pointage.MonEquipe" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Mon Équipe</h4>
            </div>
            <div class="text-end">
                <ol class="breadcrumb m-0 py-0">
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlDashboard" runat="server" NavigateUrl="~/Default.aspx">
                            Dashboard
                        </asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item active">Mon Équipe</li>
                </ol>
            </div>
        </div>

        <!-- Team Information -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-users me-2"></i>Informations de l'Équipe
                        </h5>
                    </div>
                    <div class="card-body">
                        <asp:Panel ID="pnlTeamInfo" runat="server" Visible="false">
                            <div class="row">
                                <div class="col-md-3">
                                    <div class="text-center">
                                        <h3 class="text-primary">
                                            <asp:Literal ID="litNombreEmployes" runat="server" />
                                        </h3>
                                        <p class="text-muted mb-0">Employés</p>
                                    </div>
                                </div>
                                <div class="col-md-9">
                                    <div class="row">
                                        <div class="col-md-6">
                                            <strong>Nom de l'équipe:</strong><br>
                                            <asp:Literal ID="litNomEquipe" runat="server" />
                                        </div>
                                        <div class="col-md-6">
                                            <strong>Société:</strong><br>
                                            <asp:Literal ID="litSociete" runat="server" />
                                        </div>
                                        <div class="col-md-6 mt-2">
                                            <strong>Date d'assignation:</strong><br>
                                            <asp:Literal ID="litDateAssignation" runat="server" />
                                        </div>
                                        <div class="col-md-6 mt-2">
                                            <strong>Département:</strong><br>
                                            <asp:Literal ID="litDepartement" runat="server" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="pnlNoTeam" runat="server" Visible="false">
                            <div class="alert alert-warning text-center">
                                <i class="fas fa-exclamation-triangle fa-2x mb-2"></i>
                                <h5>Aucune équipe assignée</h5>
                                <p class="mb-0">Vous n'êtes actuellement chef d'aucune équipe. Contactez votre responsable pour plus d'informations.</p>
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>

        <!-- Team Members -->
        <asp:Panel ID="pnlTeamMembers" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-user-friends me-2"></i>Membres de l'Équipe
                        </h5>
                        <asp:Button ID="btnRefresh" runat="server" Text="Actualiser"
                            CssClass="btn btn-outline-primary btn-sm" OnClick="btnRefresh_Click" CausesValidation="false" />
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvTeamMembers" runat="server"
                            CssClass="table table-striped table-hover"
                            AutoGenerateColumns="false"
                            EmptyDataText="Aucun employé dans cette équipe">
                            <Columns>
                                <asp:BoundField DataField="Matricule" HeaderText="Matricule" />
                                <asp:BoundField DataField="NomComplet" HeaderText="Nom Complet" />
                                <asp:BoundField DataField="TypeEmploye" HeaderText="Type" />
                                <asp:BoundField DataField="NomDepartement" HeaderText="Département" />
                                <asp:BoundField DataField="Telephone" HeaderText="Téléphone" />
                                <asp:BoundField DataField="DateAssignation" HeaderText="Assigné le" DataFormatString="{0:dd/MM/yyyy}" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </asp:Panel>
        <!-- Recent Activity -->
        <asp:Panel ID="pnlRecentActivity" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-history me-2"></i>Activité Récente
                        </h5>
                    </div>
                    <div class="card-body">
                        <asp:Literal ID="litRecentActivity" runat="server" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
