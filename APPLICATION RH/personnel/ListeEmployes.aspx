<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ListeEmployes.aspx.cs" Inherits="APPLICATION_RH.personnel.ListEmploye" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .table-responsive-custm {
            max-height: calc(100vh - 250px) !important;
            overflow-y: auto;
        }

        .table tbody tr:hover {
            background-color: #f8f9fa;
        }

        . table-responsive-custm .table tbody tr:nth-of-type(1) {
            background-color: #f8f9fa;
            border-bottom: 2px solid #dee2e6;
            position: sticky;
            top: 0;
            z-index: 10;
        }
        /* Custom scrollbar */
        .table-responsive::-webkit-scrollbar {
            height: 8px;
            width: 8px;
        }

        .table-responsive::-webkit-scrollbar-track {
            background: #f1f1f1;
        }

        .table-responsive::-webkit-scrollbar-thumb {
            background: #c1c1c1;
            border-radius: 4px;
        }

            .table-responsive::-webkit-scrollbar-thumb:hover {
                background: #a8a8a8;
            }
    </style>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container-fluid">
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Liste des employés</h4>
            </div>
            <div class="text-end">
                <ol class="breadcrumb m-0 py-0">
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlGestionPersonnel" runat="server" NavigateUrl="~/Default.aspx">
                        Gestion du Personnel
                        </asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item active">Liste des employés</li>
                </ol>
            </div>
        </div>

        <!-- Search Section -->
        <div class="row mb-3">
            <div class="col-md-12">
                <div class="card">
                    <div class="card-header">
                        <div class="d-flex align-items-center justify-content-between">
                            <h6 class="card-title mb-0">
                                <i class="mdi mdi-magnify me-2"></i>Rechercher des employés
                            </h6>
                            <asp:LinkButton ID="btnToggleSearch" runat="server"
                                CssClass="btn btn-sm btn-outline-primary"
                                OnClientClick="toggleSearchPanel(); return false;">
                                <i class="mdi mdi-chevron-down" id="searchToggleIcon"></i>
                            </asp:LinkButton>
                        </div>
                    </div>
                    <div class="card-body" id="searchPanel" style="display: none;">
                        <div class="row g-3">
                            <!-- Quick Search -->
                            <div class="col-md-4">
                                <label for="txtQuickSearch" class="form-label">Recherche rapide</label>
                                <div class="input-group">
                                    <asp:TextBox ID="txtQuickSearch" runat="server"
                                        CssClass="form-control"
                                        placeholder="Nom, prénom, matricule, email..." />
                                    <asp:Button ID="btnQuickSearch" runat="server"
                                        Text="Rechercher"
                                        CssClass="btn btn-primary"
                                        OnClick="btnQuickSearch_Click" />
                                </div>
                            </div>

                            <!-- Society Filter -->
                            <div class="col-md-3">
                                <label for="ddlSociety" class="form-label">Société</label>
                                <asp:DropDownList ID="ddlSociety" runat="server"
                                    CssClass="form-select"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlSociety_SelectedIndexChanged">
                                    <asp:ListItem Value="" Text="Toutes les sociétés" />
                                </asp:DropDownList>
                            </div>

                            <!-- Department Filter (cascading with Society) -->
                            <div class="col-md-3">
                                <label for="ddlDepartment" class="form-label">Département</label>
                                <asp:DropDownList ID="ddlDepartment" runat="server"
                                    CssClass="form-select"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlDepartment_SelectedIndexChanged"
                                    Enabled="false">
                                    <asp:ListItem Value="" Text="Sélectionnez d'abord une société" />
                                </asp:DropDownList>
                                <small class="text-muted">Veuillez d'abord sélectionner une société</small>
                            </div>

                        </div>

                        <div class="row g-3 mt-2">
                            <!-- Employee Type Filter -->
                            <div class="col-md-3">
                                <label for="ddlEmployeeType" class="form-label">Type d'employé</label>
                                <asp:DropDownList ID="ddlEmployeeType" runat="server"
                                    CssClass="form-select"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlEmployeeType_SelectedIndexChanged">
                                    <asp:ListItem Value="" Text="Tous les types" />
                                    <asp:ListItem Value="OUVRIER" Text="Ouvrier" />
                                    <asp:ListItem Value="EMPLOYE" Text="Employé" />
                                    <asp:ListItem Value="CADRE" Text="Cadre" />
                                    <asp:ListItem Value="MANAGER" Text="Manager" />
                                </asp:DropDownList>
                            </div>

                            <!-- Contract Type Filter -->
                            <div class="col-md-3">
                                <label for="ddlContractType" class="form-label">Type de contrat</label>
                                <asp:DropDownList ID="ddlContractType" runat="server"
                                    CssClass="form-select"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlContractType_SelectedIndexChanged">
                                    <asp:ListItem Value="" Text="Tous les contrats" />
                                    <asp:ListItem Value="CDI" Text="CDI" />
                                    <asp:ListItem Value="CDD" Text="CDD" />
                                    <asp:ListItem Value="Temporaire" Text="Temporaire" />
                                    <asp:ListItem Value="Stage" Text="Stage" />
                                </asp:DropDownList>
                            </div>

                            <!-- Date Range Filter -->
                            <div class="col-md-2">
                                <label for="txtDateFrom" class="form-label">Date embauche (De)</label>
                                <asp:TextBox ID="txtDateFrom" runat="server"
                                    CssClass="form-control"
                                    TextMode="Date" />
                            </div>

                            <div class="col-md-2">
                                <label for="txtDateTo" class="form-label">Date embauche (À)</label>
                                <asp:TextBox ID="txtDateTo" runat="server"
                                    CssClass="form-control"
                                    TextMode="Date" />
                            </div>

                            <!-- Action Buttons -->
                            <div class="col-md-2">
                                <label class="form-label">&nbsp;</label>
                                <div class="d-flex gap-1">
                                    <asp:Button ID="btnAdvancedSearch" runat="server"
                                        Text="Filtrer"
                                        CssClass="btn btn-success btn-sm"
                                        OnClick="btnAdvancedSearch_Click" />
                                    <asp:Button ID="btnClearFilters" runat="server"
                                        Text="Effacer"
                                        CssClass="btn btn-outline-secondary btn-sm"
                                        OnClick="btnClearFilters_Click" />
                                </div>
                            </div>
                        </div>

                        <!-- Search Results Info -->
                        <div class="row mt-3" id="searchResultsInfo" runat="server" visible="false">
                            <div class="col-12">
                                <div class="alert alert-info d-flex align-items-center">
                                    <i class="mdi mdi-information-outline me-2"></i>
                                    <asp:Label ID="lblSearchResults" runat="server" />
                                    <asp:LinkButton ID="btnShowAll" runat="server"
                                        CssClass="btn btn-sm btn-outline-primary ms-auto"
                                        Text="Afficher tout"
                                        OnClick="btnShowAll_Click" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <div class="card overflow-hidden">
                    <div class="card-header">
                        <div class="d-flex align-items-center justify-content-between">
                            <h5 class="card-title mb-0">Employés 
                                <asp:Label ID="lblTotalCount" runat="server" CssClass="badge bg-primary ms-2" />
                            </h5>
                            <div class="d-flex gap-2">
                                <asp:Button ID="btnExportExcel" runat="server"
                                    Text="Exporter Excel"
                                    CssClass="btn btn-success btn-sm"
                                    OnClick="btnExportExcel_Click" />
                                <asp:Button ID="btnAddEmployee" runat="server"
                                    Text="Ajouter Employé"
                                    CssClass="btn btn-primary btn-sm"
                                    OnClick="btnAddEmployee_Click" />
                            </div>
                        </div>
                    </div>

                    <div class="card-body p-0">
                        <div class="table-responsive table-responsive-custm">
                            <asp:GridView ID="gvEmployees" runat="server" CssClass="table table-traffic mb-0"
                                AutoGenerateColumns="false" DataKeyNames="IdEmploye"
                                OnRowCommand="gvEmployees_RowCommand" OnPageIndexChanging="gvEmployees_PageIndexChanging"
                                AllowPaging="true" PageSize="10" PagerStyle-CssClass="d-none">

                                <HeaderStyle CssClass="table-header" />

                                <Columns>
                                    <asp:TemplateField HeaderText="Matricule">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lnkMatricule" runat="server"
                                                Text='<%# Eval("Matricule") %>'
                                                CommandName="ViewDetails"
                                                CommandArgument='<%# Eval("IdEmploye") %>'
                                                CssClass="text-dark fw-medium" />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Employé">
                                        <ItemTemplate>
                                            <div class="d-flex align-items-center">
                                                <div>
                                                    <p class="mb-0 fw-medium fs-14">
                                                        <%# Eval("Prenom") %> <%# Eval("Nom") %>
                                                    </p>
                                                    <p class="text-muted fs-13 mb-0">
                                                        <%# Eval("Email") != DBNull.Value && !string.IsNullOrEmpty(Eval("Email").ToString()) 
                                            ? Eval("Email") : "Pas d'email" %>
                                                    </p>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Type">
                                        <ItemTemplate>
                                            <p class="mb-0 text-muted"><%# Eval("TypeEmploye") %></p>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Téléphone">
                                        <ItemTemplate>
                                            <p class="mb-0 text-muted">
                                                <%# Eval("Telephone") != DBNull.Value && !string.IsNullOrEmpty(Eval("Telephone").ToString()) 
                                    ? Eval("Telephone") : "N/A" %>
                                            </p>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Date Embauche">
                                        <ItemTemplate>
                                            <p class="mb-0 text-muted">
                                                <%# Eval("DateEmbauche") != DBNull.Value 
                                    ? Convert.ToDateTime(Eval("DateEmbauche")).ToString("dd/MM/yyyy") 
                                    : "N/A" %>
                                            </p>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Contrat">
                                        <ItemTemplate>
                                            <p class="mb-0 text-muted"><%# Eval("TypeContrat") %></p>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Actions">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btnViewDetails" runat="server"
                                                CommandName="ViewDetails"
                                                CommandArgument='<%# Eval("IdEmploye") %>'
                                                CssClass="btn btn-primary btn-sm me-1"
                                                data-bs-toggle="tooltip"
                                                data-bs-original-title="Détails"
                                                CausesValidation="false">
                                                <i class="mdi mdi-eye-outline fs-14"></i> Détails
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnEdit" runat="server"
                                                CommandName="EditEmployee"
                                                CommandArgument='<%# Eval("IdEmploye") %>'
                                                CssClass="btn btn-icon btn-sm bg-primary-subtle me-1"
                                                data-bs-toggle="tooltip"
                                                data-bs-original-title="Modifier"
                                                CausesValidation="false">
                                <i class="mdi mdi-pencil-outline fs-14 text-primary"></i>
                                            </asp:LinkButton>

                                            <asp:LinkButton ID="btnDelete" runat="server"
                                                CommandName="DeleteEmployee"
                                                CommandArgument='<%# Eval("IdEmploye") %>'
                                                CssClass="btn btn-icon btn-sm bg-danger-subtle"
                                                data-bs-toggle="tooltip"
                                                data-bs-original-title="Supprimer"
                                                OnClientClick="return confirm('Êtes-vous sûr de vouloir supprimer cet employé?');"
                                                CausesValidation="false">
                                <i class="mdi mdi-delete fs-14 text-danger"></i>
                                            </asp:LinkButton>

                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>

                    <%--<div class="card-footer py-0 border-top">
                        <div class="row align-items-center">
                            <div class="col-sm">
                                <div class="text-block text-center text-sm-start">
                                    <asp:Literal ID="litRecordInfo" runat="server"></asp:Literal>
                                </div>
                            </div>
                            <div class="col-sm-auto mt-3 mt-sm-0">
                                <div class="pagination gap-2 justify-content-center py-3 ps-0 pe-3">
                                    <ul class="pagination mb-0">
                                        <li class="page-item">
                                            <asp:LinkButton ID="btnPrevious" runat="server"
                                                CssClass="page-link me-2 rounded-2"
                                                Text="Précédent"
                                                OnClick="btnPrevious_Click"
                                                CausesValidation="false" />
                                        </li>
                                        <asp:Repeater ID="rptPager" runat="server">
                                            <ItemTemplate>
                                                <li class="page-item <%# (int)Eval("PageIndex") == CurrentPageIndex ? "active" : "" %>">
                                                    <asp:LinkButton ID="btnPage" runat="server"
                                                        CssClass="page-link rounded-2 me-2"
                                                        Text='<%# Eval("PageText") %>'
                                                        CommandName="Page"
                                                        CommandArgument='<%# Eval("PageIndex") %>'
                                                        OnCommand="rptPager_ItemCommand"
                                                        CausesValidation="false" />
                                                </li>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                        <li class="page-item">
                                            <asp:LinkButton ID="btnNext" runat="server"
                                                CssClass="page-link text-primary rounded-2"
                                                Text="Suivant"
                                                OnClick="btnNext_Click"
                                                CausesValidation="false" />
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>--%>
                </div>
            </div>
            <!-- End Recent Order -->
            <asp:HiddenField ID="hfSelectedEmployeeId" runat="server" />

        </div>
    </div>
    <!-- Modal -->
    <div class="modal fade" id="employeeDetailsModal" tabindex="-1" aria-labelledby="employeeDetailsModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="employeeDetailsModalLabel">
                        <asp:Literal ID="litModalTitle" runat="server" Text="Détails de l'employé" />
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="upEmployeeDetails" runat="server">
                        <ContentTemplate>
                            <div class="row">
                                <div class="col-md-6">
                                    <div class="card">
                                        <div class="card-header">
                                            <h6 class="card-title mb-0">Informations personnelles</h6>
                                        </div>
                                        <div class="card-body">
                                            <table class="table table-borderless table-sm">
                                                <tr>
                                                    <td><strong>Matricule:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litMatricule" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Nom:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litNom" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Prénom:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litPrenom" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Date de naissance:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litDateNaissance" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Lieu de naissance:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litLieuNaissance" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>CIN/Passeport:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litCinPassport" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Sexe:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litSexe" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Situation familiale:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litSituationFamiliale" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Nombre d'enfants:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litNombreEnfants" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Téléphone:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litTelephone" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Email:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litEmail" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Nationalité:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litNationalite" runat="server" /></td>
                                                </tr>
                                            </table>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="card">
                                        <div class="card-header">
                                            <h6 class="card-title mb-0">Informations professionnelles</h6>
                                        </div>
                                        <div class="card-body">
                                            <table class="table table-borderless table-sm">
                                                <tr>
                                                    <td><strong>Date d'embauche:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litDateEmbauche" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Date de sortie:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litDateSortie" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Type de contrat:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litTypeContrat" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Type d'employé:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litTypeEmploye" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Société:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litNomSociete" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Département:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litNomDepartement" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Shift:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litShift" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Situation CNSS:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litSituationCNSS" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Numéro CNSS:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litNumeroCNSS" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Niveau scolaire:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litNiveauScolaire" runat="server" /></td>
                                                </tr>
                                                <tr>
                                                    <td><strong>Diplôme:</strong></td>
                                                    <td>
                                                        <asp:Literal ID="litDiplome" runat="server" /></td>
                                                </tr>
                                            </table>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <asp:Panel ID="pnlAdresse" runat="server" Visible="false" CssClass="row mt-3">
                                <div class="col-12">
                                    <div class="card">
                                        <div class="card-header">
                                            <h6 class="card-title mb-0">Adresse</h6>
                                        </div>
                                        <div class="card-body">
                                            <asp:Literal ID="litAdresse" runat="server" />
                                        </div>
                                    </div>
                                </div>
                            </asp:Panel>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Fermer</button>
                    <asp:LinkButton ID="btnEditFromModal" runat="server"
                        CssClass="btn btn-primary"
                        OnClick="btnEditFromModal_Click"
                        Visible="false">
                    <i class="mdi mdi-pencil-outline me-1"></i>Modifier
                    </asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <script type="text/javascript">
        function showEmployeeModal() {
            var modal = new bootstrap.Modal(document.getElementById('employeeDetailsModal'));
            modal.show();
        }
        function toggleSearchPanel() {
            const panel = document.getElementById('searchPanel');
            const icon = document.getElementById('searchToggleIcon');

            if (panel.style.display === 'none' || panel.style.display === '') {
                panel.style.display = 'block';
                icon.className = 'mdi mdi-chevron-up';
            } else {
                panel.style.display = 'none';
                icon.className = 'mdi mdi-chevron-down';
            }
        }
        // Initialize tooltips
        $(document).ready(function () {
            $('[data-bs-toggle="tooltip"]').tooltip();
        });
    </script>

</asp:Content>
