<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EditEmployee.aspx.cs" Inherits="APPLICATION_RH.personnel.EditEmployee" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Modifier Employé - 
                <asp:Literal ID="litEmployeeName" runat="server" />
                    (<asp:Literal ID="litMatricule" runat="server" />)
                </h4>
            </div>
            <div class="text-end">
                <ol class="breadcrumb m-0 py-0">
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlGestionPersonnel" runat="server" NavigateUrl="~/Default.aspx">
                    Gestion du Personnel
                        </asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlListeEmployes" runat="server" NavigateUrl="~/personnel/ListeEmployes.aspx">
                    Liste des Employés
                        </asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item active">Modifier Employé</li>
                </ol>
            </div>
        </div>

        <!-- Employee Status Alert -->
        <asp:Panel ID="pnlEmployeeStatus" runat="server" Visible="false" CssClass="row mb-3">
            <div class="col-12">
                <div class="alert alert-info">
                    <strong>Statut:</strong>
                    <asp:Literal ID="litEmployeeStatus" runat="server" />
                    <asp:Panel ID="pnlDateSortie" runat="server" Visible="false">
                        - Date de sortie:
                        <asp:Literal ID="litDateSortie" runat="server" />
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>

        <!-- Hidden field for employee ID -->
        <asp:HiddenField ID="hfEmployeeId" runat="server" />

        <!-- Société Selection -->
        <div class="row mb-3">
            <div class="col-12">
                <div class="card">
                    <div class="card-body">
                        <div class="row align-items-end">
                            <div class="col-md-12">
                                <asp:Label ID="lblSociete" runat="server" Text="Société" CssClass="form-label" AssociatedControlID="ddlSociete" />
                                <asp:DropDownList ID="ddlSociete" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="DdlSociete_SelectedIndexChanged">
                                    <asp:ListItem Value="" Text="-- Sélectionner Société --" />
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Navigation Tabs -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-body">
                        <div class="tab-content" id="pills-tabContent">
                            <!-- Information Tab -->
                            <div class="tab-pane fade show active" id="pills-info" role="tabpanel" aria-labelledby="pills-info-tab" tabindex="0">

                                <!-- Personal Information -->
                                <div class="card mb-4">
                                    <div class="card-header">
                                        <h5 class="card-title mb-0">
                                            <i class="fas fa-user me-2"></i>Informations Personnelles
                                        </h5>
                                    </div>
                                    <div class="card-body">
                                        <div class="row">
                                            <div class="col-lg-6">
                                                <div class="mb-3">
                                                    <asp:Label ID="lblNomPrenom" runat="server" Text="Nom" CssClass="form-label" AssociatedControlID="txtNom" />
                                                    <asp:TextBox ID="txtNom" runat="server" CssClass="form-control" />
                                                    <asp:RequiredFieldValidator ID="rfvNom" runat="server"
                                                        ControlToValidate="txtNom"
                                                        ErrorMessage="Le nom est obligatoire"
                                                        CssClass="text-danger"
                                                        Display="Dynamic" />
                                                </div>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblDateNaissance" runat="server" Text="Date de Naissance" CssClass="form-label" AssociatedControlID="txtDateNaissance" />
                                                    <asp:TextBox ID="txtDateNaissance" runat="server" CssClass="form-control" TextMode="Date" />
                                                    <asp:RequiredFieldValidator ID="rfvDateNaissance" runat="server"
                                                        ControlToValidate="txtDateNaissance"
                                                        ErrorMessage="La date de naissance est obligatoire"
                                                        CssClass="text-danger"
                                                        Display="Dynamic" />
                                                </div>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblNationalite" runat="server" Text="Nationalité" CssClass="form-label" AssociatedControlID="ddlNationalite" />
                                                    <asp:DropDownList ID="ddlNationalite" runat="server" CssClass="form-select">
                                                        <asp:ListItem Value="" Text="-- Nationalité --" />
                                                    </asp:DropDownList>
                                                </div>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblRegion" runat="server" Text="Région" CssClass="form-label" AssociatedControlID="ddlRegion" />
                                                    <asp:DropDownList ID="ddlRegion" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlRegion_SelectedIndexChanged">
                                                        <asp:ListItem Value="" Text="-- Sélectionner Région --" />
                                                    </asp:DropDownList>
                                                </div>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblTelephone" runat="server" Text="Téléphone" CssClass="form-label" AssociatedControlID="txtTelephone" />
                                                    <asp:TextBox ID="txtTelephone" runat="server" CssClass="form-control" TextMode="Phone" MaxLength="10" />
                                                    <asp:RegularExpressionValidator ID="revTelephone" runat="server"
                                                        ControlToValidate="txtTelephone"
                                                        ValidationExpression="^[0-9]{10}$"
                                                        ErrorMessage="Le téléphone doit contenir 10 chiffres"
                                                        CssClass="text-danger"
                                                        Display="Dynamic" />
                                                </div>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblCinPassport" runat="server" Text="Passport / CIN" CssClass="form-label" AssociatedControlID="txtCinPassport" />
                                                    <asp:TextBox ID="txtCinPassport" runat="server" CssClass="form-control" />
                                                    <asp:RequiredFieldValidator ID="rfvCinPassport" runat="server"
                                                        ControlToValidate="txtCinPassport"
                                                        ErrorMessage="Le CIN ou Passport est obligatoire"
                                                        CssClass="text-danger"
                                                        Display="Dynamic" />
                                                </div>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblAdresse" runat="server" Text="Adresse" CssClass="form-label" AssociatedControlID="txtAdresse" />
                                                    <asp:TextBox ID="txtAdresse" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                                                </div>
                                            </div>

                                            <div class="col-lg-6">
                                                <div class="mb-3">
                                                    <asp:Label ID="Label1" runat="server" Text="Prénom" CssClass="form-label" AssociatedControlID="txtPrenom" />
                                                    <asp:TextBox ID="txtPrenom" runat="server" CssClass="form-control" />
                                                    <asp:RequiredFieldValidator ID="RequiredFieldValidatorPrenom" runat="server"
                                                        ControlToValidate="txtPrenom"
                                                        ErrorMessage="Le prénom est obligatoire"
                                                        CssClass="text-danger"
                                                        Display="Dynamic" />
                                                </div>
                                                <div class="mb-3">
                                                    <asp:Label ID="lblSituationFamiliale" runat="server" Text="Situation Familiale" CssClass="form-label" AssociatedControlID="ddlSituationFamiliale" />
                                                    <asp:DropDownList ID="ddlSituationFamiliale" runat="server" CssClass="form-select">
                                                        <asp:ListItem Value="" Text="-- Situation Familiale --" />
                                                    </asp:DropDownList>
                                                </div>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblNombreEnfants" runat="server" Text="Nombre d'Enfants" CssClass="form-label" AssociatedControlID="txtNombreEnfants" />
                                                    <asp:TextBox ID="txtNombreEnfants" runat="server" CssClass="form-control" TextMode="Number" Text="0" />
                                                    <asp:RangeValidator ID="rvNombreEnfants" runat="server"
                                                        ControlToValidate="txtNombreEnfants"
                                                        MinimumValue="0"
                                                        MaximumValue="20"
                                                        Type="Integer"
                                                        ErrorMessage="Le nombre d'enfants doit être entre 0 et 20"
                                                        CssClass="text-danger"
                                                        Display="Dynamic" />
                                                </div>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblSexe" runat="server" Text="Sexe" CssClass="form-label" AssociatedControlID="ddlSexe" />
                                                    <asp:DropDownList ID="ddlSexe" runat="server" CssClass="form-select">
                                                        <asp:ListItem Value="" Text="-- Sexe --" />
                                                        <asp:ListItem Value="Homme" Text="Homme" />
                                                        <asp:ListItem Value="Femme" Text="Femme" />
                                                    </asp:DropDownList>
                                                    <asp:RequiredFieldValidator ID="rfvSexe" runat="server"
                                                        ControlToValidate="ddlSexe"
                                                        InitialValue=""
                                                        ErrorMessage="Le sexe est obligatoire"
                                                        CssClass="text-danger"
                                                        Display="Dynamic" />
                                                </div>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblLieuNaissance" runat="server" Text="Lieu de Naissance" CssClass="form-label" AssociatedControlID="ddlLieuNaissance" />
                                                    <asp:DropDownList ID="ddlLieuNaissance" runat="server" CssClass="form-select">
                                                        <asp:ListItem Value="" Text="-- Lieu de Naissance --" />
                                                    </asp:DropDownList>
                                                </div>
                                                <div class="mb-3">
                                                    <asp:Label ID="lblNiveauScolaire" runat="server" Text="Niveau Scolaire" CssClass="form-label" AssociatedControlID="ddlNiveauScolaire" />
                                                    <asp:DropDownList ID="ddlNiveauScolaire" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlNiveauScolaire_SelectedIndexChanged">
                                                        <asp:ListItem Value="" Text="-- Niveau Scolaire --" />
                                                    </asp:DropDownList>
                                                </div>

                                                <asp:Panel ID="pnlDiplome" runat="server" Visible="false">
                                                    <div class="mb-3">
                                                        <asp:Label ID="lblDiplome" runat="server" Text="Diplôme" CssClass="form-label" AssociatedControlID="ddlDiplome" />
                                                        <asp:DropDownList ID="ddlDiplome" runat="server" CssClass="form-select">
                                                            <asp:ListItem Value="" Text="-- Diplôme --" />
                                                        </asp:DropDownList>
                                                    </div>
                                                </asp:Panel>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Professional Information -->
                                <div class="card mb-4">
                                    <div class="card-header">
                                        <h5 class="card-title mb-0">
                                            <i class="fas fa-briefcase me-2"></i>Informations Professionnelles
                                        </h5>
                                    </div>
                                    <div class="card-body">
                                        <div class="row">
                                            <div class="col-lg-6">
                                                <div class="mb-3">
                                                    <asp:Label ID="lblDateEmbauche" runat="server" Text="Date d'Embauche" CssClass="form-label" AssociatedControlID="txtDateEmbauche" />
                                                    <asp:TextBox ID="txtDateEmbauche" runat="server" CssClass="form-control" TextMode="Date" />
                                                    <asp:RequiredFieldValidator ID="rfvDateEmbauche" runat="server"
                                                        ControlToValidate="txtDateEmbauche"
                                                        ErrorMessage="La date d'embauche est obligatoire"
                                                        CssClass="text-danger"
                                                        Display="Dynamic" />
                                                </div>

                                                <!-- Date de sortie field for editing -->
                                                <div class="mb-3">
                                                    <asp:Label ID="lblDateSortieEdit" runat="server" Text="Date de Sortie (optionnel)" CssClass="form-label" AssociatedControlID="txtDateSortieEdit" />
                                                    <asp:TextBox ID="txtDateSortieEdit" runat="server" CssClass="form-control" TextMode="Date" />
                                                    <small class="form-text text-muted">Laisser vide si l'employé est toujours actif</small>
                                                </div>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblSituationCNSS" runat="server" Text="Situation CNSS" CssClass="form-label" AssociatedControlID="ddlSituationCNSS" />
                                                    <asp:DropDownList ID="ddlSituationCNSS" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlSituationCNSS_SelectedIndexChanged">
                                                        <asp:ListItem Value="" Text="-- Situation CNSS --" />
                                                        <asp:ListItem Value="D" Text="Déclaré" />
                                                        <asp:ListItem Value="ND" Text="Non Déclaré" />
                                                    </asp:DropDownList>
                                                </div>

                                                <asp:Panel ID="pnlNumeroCNSS" runat="server" Visible="false">
                                                    <div class="mb-3">
                                                        <asp:Label ID="lblNumeroCNSS" runat="server" Text="Numéro CNSS" CssClass="form-label" AssociatedControlID="txtNumeroCNSS" />
                                                        <asp:TextBox ID="txtNumeroCNSS" runat="server" CssClass="form-control" />
                                                    </div>
                                                </asp:Panel>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblTypeEmploye" runat="server" Text="Type d'Employé" CssClass="form-label" AssociatedControlID="ddlTypeEmploye" />
                                                    <asp:DropDownList ID="ddlTypeEmploye" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlTypeEmploye_SelectedIndexChanged">
                                                        <asp:ListItem Value="" Text="-- Type Employé --" />
                                                        <asp:ListItem Value="OUVRIER" Text="Ouvrier" />
                                                        <asp:ListItem Value="AGENT_SAISIE" Text="Agent de Saisie" />
                                                        <asp:ListItem Value="RH" Text="Ressources Humaines" />
                                                        <asp:ListItem Value="DG_RH" Text="Directeur RH" />
                                                        <asp:ListItem Value="DG" Text="Directeur Général" />
                                                        <asp:ListItem Value="ADMINISTRATIF" Text="Administratif" />
                                                        <asp:ListItem Value="TECHNIQUE" Text="Technique" />
                                                        <asp:ListItem Value="QUALITE" Text="Qualité" />
                                                        <asp:ListItem Value="MANAGER" Text="Manager" />
                                                        <asp:ListItem Value="SUPERVISEUR" Text="Superviseur" />
                                                    </asp:DropDownList>
                                                    <asp:RequiredFieldValidator ID="rfvTypeEmploye" runat="server"
                                                        ControlToValidate="ddlTypeEmploye"
                                                        InitialValue=""
                                                        ErrorMessage="Le type d'employé est obligatoire"
                                                        CssClass="text-danger"
                                                        Display="Dynamic" />
                                                </div>

                                                <asp:Panel ID="pnlTypeOuvrier" runat="server" Visible="false">
                                                    <div class="mb-3">
                                                        <asp:Label ID="lblTypeOuvrier" runat="server" Text="Type d'Ouvrier" CssClass="form-label" AssociatedControlID="ddlTypeOuvrier" />
                                                        <asp:DropDownList ID="ddlTypeOuvrier" runat="server" CssClass="form-select">
                                                            <asp:ListItem Value="" Text="-- Type Ouvrier --" />
                                                        </asp:DropDownList>
                                                    </div>
                                                </asp:Panel>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblTypeContrat" runat="server" Text="Type de Contrat" CssClass="form-label" AssociatedControlID="ddlTypeContrat" />
                                                    <asp:DropDownList ID="ddlTypeContrat" runat="server" CssClass="form-select">
                                                        <asp:ListItem Value="" Text="-- Type Contrat --" />
                                                    </asp:DropDownList>
                                                </div>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblShift" runat="server" Text="Shift" CssClass="form-label" AssociatedControlID="ddlShift" />
                                                    <asp:DropDownList ID="ddlShift" runat="server" CssClass="form-select">
                                                        <asp:ListItem Value="" Text="-- Shift --" />
                                                        <asp:ListItem Value="JOUR" Text="JOUR" />
                                                        <asp:ListItem Value="NUIT" Text="NUIT" />
                                                    </asp:DropDownList>
                                                </div>
                                            </div>

                                            <div class="col-lg-6">
                                                <asp:UpdatePanel ID="upHierarchie" runat="server" UpdateMode="Conditional">
                                                    <ContentTemplate>
                                                        <div class="mb-3">
                                                            <asp:Label ID="lblDepartement" runat="server" Text="Département" CssClass="form-label" AssociatedControlID="ddlDepartement" />
                                                            <asp:DropDownList ID="ddlDepartement" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlDepartement_SelectedIndexChanged">
                                                                <asp:ListItem Value="" Text="-- Département --" />
                                                            </asp:DropDownList>
                                                        </div>

                                                        <div class="mb-3">
                                                            <asp:Label ID="lblService" runat="server" Text="Service" CssClass="form-label" AssociatedControlID="ddlService" />
                                                            <asp:DropDownList ID="ddlService" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlService_SelectedIndexChanged">
                                                                <asp:ListItem Value="" Text="-- Service --" />
                                                            </asp:DropDownList>
                                                        </div>

                                                        <div class="mb-3">
                                                            <asp:Label ID="lblCellule" runat="server" Text="Cellule" CssClass="form-label" AssociatedControlID="ddlCellule" />
                                                            <asp:DropDownList ID="ddlCellule" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCellule_SelectedIndexChanged">
                                                                <asp:ListItem Value="" Text="-- Cellule --" />
                                                            </asp:DropDownList>
                                                        </div>

                                                        <div class="mb-3">
                                                            <asp:Label ID="lblFonction" runat="server" Text="Fonction" CssClass="form-label" AssociatedControlID="ddlFonction" />
                                                            <asp:DropDownList ID="ddlFonction" runat="server" CssClass="form-select">
                                                                <asp:ListItem Value="" Text="-- Fonction --" />
                                                            </asp:DropDownList>
                                                        </div>
                                                    </ContentTemplate>
                                                </asp:UpdatePanel>

                                                <div class="mb-3">
                                                    <asp:Label ID="lblEmplacement" runat="server" Text="Emplacement" CssClass="form-label" AssociatedControlID="ddlEmplacement" />
                                                    <asp:DropDownList ID="ddlEmplacement" runat="server" CssClass="form-select">
                                                        <asp:ListItem Value="" Text="-- Emplacement --" />
                                                    </asp:DropDownList>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Action Buttons -->
                                <div class="row">
                                    <div class="col-6">
                                        <asp:Button ID="btnModifier" runat="server" Text="Enregistrer les modifications" CssClass="btn btn-primary w-100" OnClick="BtnModifier_Click" />
                                    </div>
                                    <div class="col-6">
                                        <asp:Button ID="btnAnnuler" runat="server" Text="Annuler" CssClass="btn btn-secondary w-100" OnClick="btnAnnuler_Click" CausesValidation="false" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
