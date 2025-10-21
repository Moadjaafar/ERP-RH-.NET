<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TransfertAssignation.aspx.cs" Inherits="APPLICATION_RH.pointage.TransfertAssignation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .team-card {
            border-left: 4px solid #0d6efd;
            background-color: #f8f9fa;
        }

        .member-row:hover {
            background-color: #e9ecef;
        }

        .transfer-arrow {
            font-size: 2rem;
            color: #28a745;
            display: flex;
            align-items: center;
            justify-content: center;
            height: 100%;
        }

         .team-info {
            background-color: #0d6efd;
            color: white;
            border-radius: 10px;
        }
        .member-card {
            border-left: 3px solid transparent;
        }

        .member-card.selected {
            border-left-color: #28a745;
            background-color: #d4edda;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Transfert d'Assignation d'Équipe</h4>
                <p class="text-muted mb-0">Transférer des employés d'une équipe à une autre</p>
            </div>
            <div class="text-end">
                <ol class="breadcrumb m-0 py-0">
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlDashboard" runat="server" NavigateUrl="~/Default.aspx">
                            Dashboard
                        </asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlGestionEquipes" runat="server" NavigateUrl="~/pointage/GestionEquipes.aspx">
                            Gestion des Équipes
                        </asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item active">Transfert d'Assignation</li>
                </ol>
            </div>
        </div>

        <!-- Team Selection -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-exchange-alt me-2"></i>Sélection des Équipes
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <asp:Label ID="lblSociete" runat="server" Text="Société *" CssClass="form-label" />
                                    <asp:DropDownList ID="ddlSociete" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlSociete_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Sélectionner --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <asp:Label ID="lblEquipeSource" runat="server" Text="Équipe Source *" CssClass="form-label" />
                                    <asp:DropDownList ID="ddlEquipeSource" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEquipeSource_SelectedIndexChanged">
                                        <asp:ListItem Value="" Text="-- Sélectionner Équipe Source --" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-lg-4">
                                <div class="mb-3">
                                    <asp:Label ID="lblDateTransfert" runat="server" Text="Date de Transfert *" CssClass="form-label" />
                                    <asp:TextBox ID="txtDateTransfert" runat="server" CssClass="form-control" TextMode="Date" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Transfer Interface -->
        <asp:Panel ID="pnlTransfer" runat="server" Visible="false" CssClass="row mt-4">
            <div class="col-lg-5">
                <!-- Source Team -->
                <div class="card team-card">
                    <div class="card-header team-info">
                        <h5 class="card-title mb-0 text-white">
                            <i class="fas fa-users me-2"></i>Équipe Source
                        </h5>
                        <div class="mt-2">
                            <div><strong>Équipe:</strong>
                                <asp:Literal ID="litSourceTeamName" runat="server" /></div>
                            <div><strong>Chef:</strong>
                                <asp:Literal ID="litSourceChefName" runat="server" /></div>
                            <div><strong>Membres:</strong>
                                <asp:Literal ID="litSourceMemberCount" runat="server" /></div>
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="d-flex justify-content-between mb-3">
                            <div>
                                <asp:Button ID="btnSelectAll" runat="server" Text="Tout Sélectionner"
                                    CssClass="btn btn-outline-primary btn-sm" OnClick="btnSelectAll_Click" CausesValidation="false" />
                                <asp:Button ID="btnUnselectAll" runat="server" Text="Désélectionner"
                                    CssClass="btn btn-outline-secondary btn-sm ms-1" OnClick="btnUnselectAll_Click" CausesValidation="false" />
                            </div>
                            <span class="badge bg-info">Sélectionnés: <span id="selectedCount">0</span>
                            </span>
                        </div>
                        <div class="table-responsive ">
                            <asp:GridView ID="gvSourceMembers" runat="server"
                                CssClass="table table-hover"
                                AutoGenerateColumns="false"
                                EmptyDataText="Aucun membre dans cette équipe"
                                GridLines="None">
                                <Columns>
                                    <asp:TemplateField HeaderText="Sélection" HeaderStyle-Width="50px">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkSelect" runat="server" onclick="updateSelectedCount()" />
                                            <asp:HiddenField ID="hfIdEmploye" runat="server" Value='<%# Eval("IdEmploye") %>' />
                                            <asp:HiddenField ID="hfIdAssignation" runat="server" Value='<%# Eval("IdEmployeChefEquipe") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Matricule" HeaderText="Matricule" />
                                    <asp:BoundField DataField="NomComplet" HeaderText="Nom Complet" />
                                    <asp:BoundField DataField="DateAssignation" HeaderText="Assigné le" DataFormatString="{0:dd/MM/yyyy}" />
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-lg-2">
                <!-- Transfer Arrow -->
                <div class="transfer-arrow">
                    <i class="fas fa-arrow-right"></i>
                </div>
            </div>

            <div class="col-lg-5">
                <!-- Target Team Selection -->
                <div class="card">
                    <div class="card-header bg-success text-white">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-bullseye me-2"></i>Équipe Destination
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="mb-3">
                            <asp:Label ID="lblEquipeDestination" runat="server" Text="Sélectionner Équipe Destination *" CssClass="form-label" />
                            <asp:DropDownList ID="ddlEquipeDestination" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEquipeDestination_SelectedIndexChanged">
                                <asp:ListItem Value="" Text="-- Sélectionner Équipe Destination --" />
                            </asp:DropDownList>
                        </div>

                        <!-- Destination Team Info -->
                        <asp:Panel ID="pnlDestinationInfo" runat="server" Visible="false" CssClass="alert alert-info">
                            <h6 class="alert-heading">Informations de l'Équipe Destination</h6>
                            <div><strong>Équipe:</strong>
                                <asp:Literal ID="litDestTeamName" runat="server" /></div>
                            <div><strong>Chef:</strong>
                                <asp:Literal ID="litDestChefName" runat="server" /></div>
                            <div><strong>Membres actuels:</strong>
                                <asp:Literal ID="litDestMemberCount" runat="server" /></div>
                        </asp:Panel>

                        <!-- Transfer Button -->
                        <div class="d-grid gap-2 mt-4">
                            <asp:Button ID="btnTransfer" runat="server" Text="Effectuer le Transfert"
                                CssClass="btn btn-success btn-lg" OnClick="btnTransfer_Click"
                                OnClientClick="return confirmTransfer();" />
                        </div>

                        <!-- Transfer Summary -->
                        <div class="mt-3 p-3 bg-light rounded">
                            <h6>Résumé du Transfert</h6>
                            <div id="transferSummary">
                                <span class="text-muted">Sélectionnez des employés pour voir le résumé</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Transfer History -->
        <div class="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-history me-2"></i>Historique des Transferts Récents
                        </h5>
                        <asp:Button ID="btnRefreshHistory" runat="server" Text="Actualiser"
                            CssClass="btn btn-outline-primary btn-sm" OnClick="btnRefreshHistory_Click" CausesValidation="false" />
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvTransferHistory" runat="server"
                            CssClass="table table-striped table-hover"
                            AutoGenerateColumns="false"
                            EmptyDataText="Aucun transfert récent">
                            <Columns>
                                <asp:BoundField DataField="NomEmploye" HeaderText="Employé" />
                                <asp:BoundField DataField="MatriculeEmploye" HeaderText="Matricule" />
                                <asp:BoundField DataField="EquipeSource" HeaderText="Équipe Source" />
                                <asp:BoundField DataField="EquipeDestination" HeaderText="Équipe Destination" />
                                <asp:BoundField DataField="DateTransfert" HeaderText="Date Transfert" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                                <asp:BoundField DataField="TransferePar" HeaderText="Transféré Par" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function updateSelectedCount() {
            var checkboxes = document.querySelectorAll('[id*="chkSelect"]:checked');
            var count = checkboxes.length;
            document.getElementById('selectedCount').textContent = count;

            // Update transfer summary
            updateTransferSummary(count);
        }

        function updateTransferSummary(count) {
            var summaryDiv = document.getElementById('transferSummary');
            var destTeam = document.querySelector('[id*="ddlEquipeDestination"]');

            if (count > 0 && destTeam && destTeam.value) {
                var destTeamText = destTeam.options[destTeam.selectedIndex].text;
                summaryDiv.innerHTML = `
                    <div class="fw-bold text-success">
                        <i class="fas fa-arrow-right me-2"></i>
                        Transférer ${count} employé(s) vers "${destTeamText}"
                    </div>
                `;
            } else {
                summaryDiv.innerHTML = '<span class="text-muted">Sélectionnez des employés pour voir le résumé</span>';
            }
        }

        function confirmTransfer() {
            var checkboxes = document.querySelectorAll('[id*="chkSelect"]:checked');
            var count = checkboxes.length;

            if (count === 0) {
                alert('Veuillez sélectionner au moins un employé à transférer.');
                return false;
            }

            var destTeam = document.querySelector('[id*="ddlEquipeDestination"]');
            if (!destTeam || !destTeam.value) {
                alert('Veuillez sélectionner une équipe de destination.');
                return false;
            }

            var destTeamText = destTeam.options[destTeam.selectedIndex].text;
            return confirm(`Confirmer le transfert de ${count} employé(s) vers l'équipe "${destTeamText}"?`);
        }

        // Initialize on page load
        document.addEventListener('DOMContentLoaded', function () {
            updateSelectedCount();

            // Add change listener for destination dropdown
            var destDropdown = document.querySelector('[id*="ddlEquipeDestination"]');
            if (destDropdown) {
                destDropdown.addEventListener('change', function () {
                    updateSelectedCount();
                });
            }
        });
    </script>
</asp:Content>
