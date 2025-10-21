<%@ Page Title="Demande de Congé" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="absence-permissions.aspx.cs" Inherits="APPLICATION_RH.conge.absence_permissions" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .form-control:focus {
            border-color: #007bff;
            box-shadow: 0 0 0 0.2rem rgba(0,123,255,.25);
        }
        .card-header {
            background: linear-gradient(45deg, #28a745, #20c997);
            color: white;
        }
        .request-form {
            background: #f8f9fa;
            border-radius: 10px;
            padding: 30px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        .step-indicator {
            display: flex;
            justify-content: space-between;
            margin-bottom: 30px;
        }
        .step {
            flex: 1;
            text-align: center;
            position: relative;
        }
        .step-number {
            display: inline-block;
            width: 40px;
            height: 40px;
            line-height: 40px;
            border-radius: 50%;
            background: #dee2e6;
            color: #6c757d;
            font-weight: bold;
            margin-bottom: 10px;
        }
        .step.active .step-number {
            background: #28a745;
            color: white;
        }
        .step.completed .step-number {
            background: #007bff;
            color: white;
        }
        .btn-submit {
            background: linear-gradient(45deg, #28a745, #20c997);
            border: none;
            padding: 12px 40px;
            font-size: 16px;
            font-weight: bold;
        }
        .btn-submit:hover {
            background: linear-gradient(45deg, #218838, #1ba085);
        }
        .form-section {
            margin-bottom: 25px;
        }
        .section-title {
            color: #495057;
            font-weight: 600;
            margin-bottom: 15px;
            padding-bottom: 5px;
            border-bottom: 2px solid #e9ecef;
        }
        .table-hover tbody tr:hover {
            background-color: #f8f9fa;
        }
        .sticky-top {
            position: sticky;
            top: 0;
            z-index: 10;
        }
        .btn-group-sm .btn {
            font-size: 0.75rem;
            padding: 0.25rem 0.5rem;
        }
        .badge.fs-6 {
            font-size: 0.75rem !important;
        }
        .grid-container {
            border-radius: 0.375rem;
            overflow: hidden;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-12">
                <div class="page-title-box d-flex align-items-center justify-content-between">
                    <h4 class="mb-0"><i class="fas fa-calendar-plus me-2"></i>Demande de Congé</h4>
                    <div class="page-title-right">
                        <ol class="breadcrumb m-0">
                            <li class="breadcrumb-item"><a href="javascript: void(0);">Congés</a></li>
                            <li class="breadcrumb-item active">Nouvelle Demande</li>
                        </ol>
                    </div>
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header text-center">
                        <h5 class="mb-0"><i class="fas fa-user-clock me-2"></i>Formulaire de Demande de Congé</h5>
                    </div>
                    <div class="card-body">
                        <div class="request-form">
                            <!-- Step Indicator -->
                            <div class="step-indicator">
                                <div class="step active">
                                    <div class="step-number">1</div>
                                    <div class="step-label">Employé</div>
                                </div>
                                <div class="step">
                                    <div class="step-number">2</div>
                                    <div class="step-label">Type de Congé</div>
                                </div>
                                <div class="step">
                                    <div class="step-number">3</div>
                                    <div class="step-label">Période</div>
                                </div>
                            </div>

                            <asp:UpdatePanel ID="UpdatePanelForm" runat="server">
                                <ContentTemplate>
                                    <!-- Section 1: Sélection de l'employé -->
                                    <div class="form-section">
                                        <h6 class="section-title">
                                            <i class="fas fa-user me-2"></i>1. Sélectionner l'Employé
                                        </h6>
                                        
                                        <!-- Employee Filter -->
                                        <div class="row mb-3">
                                            <div class="col-md-4">
                                                <label for="txtEmployeeFilter" class="form-label">Rechercher Employé</label>
                                                <div class="input-group">
                                                    <asp:TextBox ID="txtEmployeeFilter" runat="server" CssClass="form-control" 
                                                        placeholder="Nom, prénom ou matricule..." AutoPostBack="true" OnTextChanged="txtEmployeeFilter_TextChanged"></asp:TextBox>
                                                    <asp:Button ID="btnClearFilter" runat="server" Text="×" CssClass="btn btn-outline-secondary" 
                                                        OnClick="btnClearFilter_Click" ToolTip="Effacer le filtre" />
                                                </div>
                                                <small class="text-muted">Tapez pour filtrer la liste des employés</small>
                                            </div>
                                            <div class="col-md-8">
                                                <label for="ddlEmploye" class="form-label">Employé <span class="text-danger">*</span></label>
                                                <asp:DropDownList ID="ddlEmploye" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEmploye_SelectedIndexChanged">
                                                    <asp:ListItem Value="" Text="-- Sélectionner un employé --"></asp:ListItem>
                                                </asp:DropDownList>
                                                <asp:RequiredFieldValidator ID="rfvEmploye" runat="server" ControlToValidate="ddlEmploye" 
                                                    ErrorMessage="Veuillez sélectionner un employé" CssClass="text-danger small" ValidationGroup="SubmitRequest" />
                                            </div>
                                        </div>
                                        
                                        <div class="row">
                                            <div class="col-12">
                                                <label class="form-label">Informations Employé</label>
                                                <div class="border rounded p-3 bg-light">
                                                    <asp:Label ID="lblEmployeInfo" runat="server" Text="Sélectionnez un employé pour voir ses informations" CssClass="text-muted small"></asp:Label>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Section 2: Type de congé -->
                                    <div class="form-section">
                                        <h6 class="section-title">
                                            <i class="fas fa-tags me-2"></i>2. Type de Congé
                                        </h6>
                                        <div class="row">
                                            <div class="col-md-4">
                                                <label for="ddlTypeConge" class="form-label">Type de Congé <span class="text-danger">*</span></label>
                                                <asp:DropDownList ID="ddlTypeConge" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlTypeConge_SelectedIndexChanged">
                                                    <asp:ListItem Value="" Text="-- Sélectionner le type --"></asp:ListItem>
                                                </asp:DropDownList>
                                                <asp:RequiredFieldValidator ID="rfvTypeConge" runat="server" ControlToValidate="ddlTypeConge" 
                                                    ErrorMessage="Veuillez sélectionner un type de congé" CssClass="text-danger small" ValidationGroup="SubmitRequest" />
                                            </div>
                                            <div class="col-md-8">
                                                <label class="form-label">Description du Type</label>
                                                <div class="border rounded p-3 bg-light">
                                                    <asp:Label ID="lblTypeDescription" runat="server" Text="Sélectionnez un type pour voir sa description" CssClass="text-muted small"></asp:Label>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Section 3: Période -->
                                    <div class="form-section">
                                        <h6 class="section-title">
                                            <i class="fas fa-calendar-alt me-2"></i>3. Période de Congé
                                        </h6>
                                        <div class="row">
                                            <div class="col-md-3">
                                                <label for="txtDateDebut" class="form-label">Date de Début <span class="text-danger">*</span></label>
                                                <asp:TextBox ID="txtDateDebut" runat="server" CssClass="form-control" TextMode="Date" AutoPostBack="true" OnTextChanged="txtDateDebut_TextChanged"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="rfvDateDebut" runat="server" ControlToValidate="txtDateDebut" 
                                                    ErrorMessage="La date de début est obligatoire" CssClass="text-danger small" ValidationGroup="SubmitRequest" />
                                            </div>
                                            <div class="col-md-3">
                                                <label for="txtDateFin" class="form-label">Date de Fin <span class="text-danger">*</span></label>
                                                <asp:TextBox ID="txtDateFin" runat="server" CssClass="form-control" TextMode="Date" AutoPostBack="true" OnTextChanged="txtDateFin_TextChanged"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="rfvDateFin" runat="server" ControlToValidate="txtDateFin" 
                                                    ErrorMessage="La date de fin est obligatoire" CssClass="text-danger small" ValidationGroup="SubmitRequest" />
                                                <asp:CompareValidator ID="cvDateFin" runat="server" ControlToValidate="txtDateFin" ControlToCompare="txtDateDebut" 
                                                    Operator="GreaterThanEqual" Type="Date" ErrorMessage="La date de fin doit être >= date de début" 
                                                    CssClass="text-danger small" ValidationGroup="SubmitRequest" />
                                            </div>
                                            <div class="col-md-6">
                                                <label class="form-label">Durée du Congé</label>
                                                <div class="alert alert-info mb-0">
                                                    <i class="fas fa-info-circle me-2"></i>
                                                    <strong><asp:Label ID="lblDureeConge" runat="server" Text="Sélectionnez les dates pour calculer la durée"></asp:Label></strong>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Section 4: Commentaire optionnel -->
                                    <div class="form-section">
                                        <h6 class="section-title">
                                            <i class="fas fa-comment me-2"></i>Commentaire (Optionnel)
                                        </h6>
                                        <div class="row">
                                            <div class="col-md-8">
                                                <asp:TextBox ID="txtCommentaire" runat="server" CssClass="form-control" TextMode="MultiLine" 
                                                    Rows="3" placeholder="Ajoutez un commentaire ou justification si nécessaire..."></asp:TextBox>
                                            </div>
                                            <div class="col-md-4 d-flex align-items-end">
                                                <asp:Button ID="btnSoumettreDemande" runat="server" Text="🚀 Soumettre la Demande" 
                                                    CssClass="btn btn-success btn-lg btn-submit w-100" OnClick="btnSoumettreDemande_Click" 
                                                    ValidationGroup="SubmitRequest" />
                                            </div>
                                        </div>
                                        <div class="row mt-3">
                                            <div class="col-12 text-center">
                                                <small class="text-muted">
                                                    <i class="fas fa-check-circle me-1"></i>
                                                    La demande sera soumise pour approbation par votre supérieur hiérarchique
                                                </small>
                                            </div>
                                        </div>
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Section des demandes récentes -->
        <div class="row mt-4">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <div class="d-flex justify-content-between align-items-center">
                            <h6 class="mb-0"><i class="fas fa-history me-2"></i>Dernières Demandes</h6>
                            <small class="text-white">
                                <i class="fas fa-info-circle me-1"></i>
                                Gérez les demandes existantes
                            </small>
                        </div>
                    </div>
                    <div class="card-body p-0">
                        <asp:UpdatePanel ID="UpdatePanelHistory" runat="server">
                            <ContentTemplate>
                                <div style="max-height: 400px; overflow-y: auto; border: 1px solid #dee2e6;">
                                    <asp:GridView ID="gvDemandes" runat="server" CssClass="table table-striped table-hover mb-0" 
                                        AutoGenerateColumns="false" EmptyDataText="Aucune demande trouvée."
                                        DataKeyNames="IdDemandeAbsence" 
                                        OnRowCommand="gvDemandes_RowCommand"
                                        OnRowDataBound="gvDemandes_RowDataBound"
                                        AllowPaging="true" PageSize="10" OnPageIndexChanging="gvDemandes_PageIndexChanging">
                                        <HeaderStyle CssClass="table-dark sticky-top" />
                                        <EmptyDataRowStyle CssClass="text-center p-4 text-muted" />
                                        <Columns>
                                            <asp:BoundField DataField="IdDemandeAbsence" HeaderText="ID" Visible="false" />
                                            <asp:BoundField DataField="NomEmploye" HeaderText="Employé" />
                                            <asp:BoundField DataField="TypeAbsence" HeaderText="Type" />
                                            <asp:BoundField DataField="DateDebut" HeaderText="Du" DataFormatString="{0:dd/MM/yyyy}" />
                                            <asp:BoundField DataField="DateFin" HeaderText="Au" DataFormatString="{0:dd/MM/yyyy}" />
                                            <asp:TemplateField HeaderText="Durée">
                                                <ItemTemplate>
                                                    <%# CalculateDays(Eval("DateDebut"), Eval("DateFin")) %>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Statut">
                                                <ItemTemplate>
                                                    <span class='badge bg-<%# Convert.ToBoolean(Eval("EstApprouvee")) ? "success" : "warning" %> fs-6'>
                                                        <i class='fas <%# Convert.ToBoolean(Eval("EstApprouvee")) ? "fa-check-circle" : "fa-clock" %> me-1'></i>
                                                        <%# Convert.ToBoolean(Eval("EstApprouvee")) ? "Approuvée" : "En attente" %>
                                                    </span>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="DateCreation" HeaderText="Demandé le" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                                            <asp:TemplateField HeaderText="Actions" ItemStyle-Width="200px">
                                                <ItemTemplate>
                                                    <div class="btn-group btn-group-sm" role="group">
                                                        <asp:LinkButton ID="btnToggleStatus" runat="server" 
                                                            CommandName="ToggleStatus" 
                                                            CommandArgument='<%# Eval("IdDemandeAbsence") %>'
                                                            CssClass='<%# Convert.ToBoolean(Eval("EstApprouvee")) ? "btn btn-warning" : "btn btn-success" %>'
                                                            ToolTip='<%# Convert.ToBoolean(Eval("EstApprouvee")) ? "Rejeter la demande" : "Approuver la demande" %>'>
                                                            <i class='<%# Convert.ToBoolean(Eval("EstApprouvee")) ? "fas fa-times" : "fas fa-check" %>'></i>
                                                            <%# Convert.ToBoolean(Eval("EstApprouvee")) ? "Rejeter" : "Approuver" %>
                                                        </asp:LinkButton>
                                                        
                                                        <asp:LinkButton ID="btnDelete" runat="server" 
                                                            CommandName="DeleteRequest" 
                                                            CommandArgument='<%# Eval("IdDemandeAbsence") %>'
                                                            CssClass="btn btn-danger"
                                                            OnClientClick="return confirm('Êtes-vous sûr de vouloir supprimer cette demande de congé ?');"
                                                            ToolTip="Supprimer la demande">
                                                            <i class="fas fa-trash"></i>
                                                            Supprimer
                                                        </asp:LinkButton>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                        <PagerSettings Mode="NumericFirstLast" PageButtonCount="5" FirstPageText="Premier" LastPageText="Dernier" />
                                        <PagerStyle CssClass="pagination-sm justify-content-center" />
                                    </asp:GridView>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function showMessage(message, type) {
            try {
                Swal.fire({
                    title: type === 'success' ? 'Succès!' : type === 'error' ? 'Erreur!' : 'Information',
                    text: message,
                    icon: type,
                    confirmButtonText: 'OK',
                    timer: type === 'success' ? 3000 : 0
                });
            } catch (e) {
                console.error('Error showing message:', e);
                alert(message); // fallback
            }
        }

        // Update step indicators based on form completion
        function updateStepIndicators() {
            var employeSelected = document.getElementById('<%= ddlEmploye.ClientID %>').value !== '';
            var typeSelected = document.getElementById('<%= ddlTypeConge.ClientID %>').value !== '';
            var dateStarted = document.getElementById('<%= txtDateDebut.ClientID %>').value !== '';

            var steps = document.querySelectorAll('.step');
            
            // Step 1
            if (employeSelected) {
                steps[0].classList.add('completed');
                steps[1].classList.add('active');
            }
            
            // Step 2
            if (typeSelected) {
                steps[1].classList.add('completed');
                steps[2].classList.add('active');
            }
            
            // Step 3
            if (dateStarted) {
                steps[2].classList.add('active');
            }
        }

        // Call on page load and after updates
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(updateStepIndicators);
        window.addEventListener('load', updateStepIndicators);
    </script>
</asp:Content>
