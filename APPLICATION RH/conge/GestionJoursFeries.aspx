<%@ Page Title="Gestion Jours Feries" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GestionJoursFeries.aspx.cs" Inherits="APPLICATION_RH.conge.GestionJoursFeries" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .calendar-container {
            background-color: #fff;
            border-radius: 0.375rem;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }
        .holiday-card {
            border-left: 4px solid #dc3545;
            transition: all 0.3s ease;
        }
        .holiday-card:hover {
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            transform: translateY(-2px);
        }
        .special-day-card {
            border-left: 4px solid #ffc107;
        }
        .filter-section {
            background-color: #f8f9fa;
            border-radius: 0.375rem;
            padding: 20px;
            margin-bottom: 20px;
        }
        .stat-card {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border-radius: 0.375rem;
            padding: 20px;
            margin-bottom: 20px;
        }
        .stat-card h3 {
            font-size: 2rem;
            margin: 0;
        }
        .stat-card p {
            margin: 5px 0 0 0;
            opacity: 0.9;
        }
        .badge-holiday {
            background-color: #dc3545;
            color: white;
            padding: 0.5em 0.75em;
            border-radius: 0.25rem;
            font-size: 0.875rem;
        }
        .badge-special {
            background-color: #ffc107;
            color: #000;
            padding: 0.5em 0.75em;
            border-radius: 0.25rem;
            font-size: 0.875rem;
        }
        .table-hover tbody tr:hover {
            background-color: rgba(0,123,255,0.05);
        }
        .action-buttons .btn {
            margin-right: 5px;
        }
        .form-section {
            background-color: #e7f3ff;
            border-radius: 0.375rem;
            padding: 20px;
            margin-bottom: 20px;
        }
        .recurring-options {
            background-color: #fff;
            border: 1px solid #dee2e6;
            border-radius: 0.375rem;
            padding: 15px;
            margin-top: 15px;
        }
        .date-preview {
            background-color: #d1ecf1;
            border: 1px solid #bee5eb;
            border-radius: 0.375rem;
            padding: 10px;
            margin-top: 10px;
            font-size: 0.875rem;
        }
        .year-selector {
            text-align: center;
            margin-bottom: 20px;
        }
        .year-selector .btn-group {
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        .legend-item {
            display: inline-block;
            margin-right: 20px;
            margin-bottom: 10px;
        }
        .legend-color {
            display: inline-block;
            width: 20px;
            height: 20px;
            border-radius: 3px;
            margin-right: 5px;
            vertical-align: middle;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">

        <!-- Page Header -->
        <div class="py-3 d-flex align-items-sm-center flex-sm-row flex-column">
            <div class="flex-grow-1">
                <h4 class="fs-18 fw-semibold m-0">Gestion des Jours Feries et Jours Speciaux</h4>
            </div>
            <div class="text-end">
                <ol class="breadcrumb m-0 py-0">
                    <li class="breadcrumb-item">
                        <asp:HyperLink ID="hlDashboard" runat="server" NavigateUrl="~/Default.aspx">Dashboard</asp:HyperLink>
                    </li>
                    <li class="breadcrumb-item">Conges</li>
                    <li class="breadcrumb-item active">Jours Feries</li>
                </ol>
            </div>
        </div>

        <!-- Statistics Cards -->
        <div class="row">
            <div class="col-md-3">
                <div class="stat-card" style="background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);">
                    <h3><asp:Literal ID="litTotalHolidays" runat="server" Text="0" /></h3>
                    <p>Jours Feries (<asp:Literal ID="litCurrentYear" runat="server" />)</p>
                </div>
            </div>
            <div class="col-md-3">
                <div class="stat-card" style="background: linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%);">
                    <h3><asp:Literal ID="litTotalSpecial" runat="server" Text="0" /></h3>
                    <p>Jours Speciaux</p>
                </div>
            </div>
            <div class="col-md-3">
                <div class="stat-card" style="background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);">
                    <h3><asp:Literal ID="litUpcoming" runat="server" Text="0" /></h3>
                    <p>A Venir</p>
                </div>
            </div>
            <div class="col-md-3">
                <div class="stat-card" style="background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);">
                    <h3><asp:Literal ID="litTotalDaysOff" runat="server" Text="0" /></h3>
                    <p>Total Jours Repos</p>
                </div>
            </div>
        </div>

        <!-- Add New Holiday/Special Day -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header bg-primary text-white">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-plus-circle me-2"></i>
                            <asp:Literal ID="litFormTitle" runat="server" Text="Ajouter un Jour Ferie / Jour Special" />
                        </h5>
                    </div>
                    <div class="card-body form-section">
                        <div class="row">
                            <div class="col-md-12">
                                <div class="mb-3">
                                    <label for="ddlPredefinedHoliday" class="form-label">Jours Feries Predefinis (Maroc)</label>
                                    <asp:DropDownList ID="ddlPredefinedHoliday" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlPredefinedHoliday_SelectedIndexChanged">
                                        <asp:ListItem Value="" Selected="True">-- Saisie manuelle --</asp:ListItem>
                                        <asp:ListItem Value="NOUVEL_AN">Nouvel An (01 Janvier)</asp:ListItem>
                                        <asp:ListItem Value="MANIFESTE">Manifeste de l'Independance (11 Janvier)</asp:ListItem>
                                        <asp:ListItem Value="FETE_TRAVAIL">Fete du Travail (01 Mai)</asp:ListItem>
                                        <asp:ListItem Value="FETE_TRONE">Fete du Trone (30 Juillet)</asp:ListItem>
                                        <asp:ListItem Value="OUED_EDDAHAB">Journee de Oued Ed-Dahab (14 Aout)</asp:ListItem>
                                        <asp:ListItem Value="REVOLUTION">Revolution du Roi et du Peuple (20 Aout)</asp:ListItem>
                                        <asp:ListItem Value="FETE_JEUNESSE">Fete de la Jeunesse (21 Aout)</asp:ListItem>
                                        <asp:ListItem Value="MARCHE_VERTE">Marche Verte (06 Novembre)</asp:ListItem>
                                        <asp:ListItem Value="FETE_INDEPENDANCE">Fete de l'Independance (18 Novembre)</asp:ListItem>
                                    </asp:DropDownList>
                                    <small class="text-muted">Selectionnez un jour ferie officiel du Maroc ou laissez sur "Saisie manuelle" pour entrer vos propres donnees</small>
                                </div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-3">
                                    <label for="txtNom" class="form-label">Nom du Jour Ferie / Special *</label>
                                    <asp:TextBox ID="txtNom" runat="server" CssClass="form-control" placeholder="Ex: Aid Al Fitr, Fete du Travail" MaxLength="200" />
                                    <asp:RequiredFieldValidator ID="rfvNom" runat="server" ControlToValidate="txtNom"
                                        ErrorMessage="Le nom est obligatoire" CssClass="text-danger" Display="Dynamic" />
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="ddlType" class="form-label">Type *</label>
                                    <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select" AutoPostBack="false">
                                        <asp:ListItem Value="FERIE" Selected="True">Jour Ferie</asp:ListItem>
                                        <asp:ListItem Value="SPECIAL">Jour Special</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="mb-3">
                                    <label for="ddlPaid" class="form-label">Paye *</label>
                                    <asp:DropDownList ID="ddlPaid" runat="server" CssClass="form-select">
                                        <asp:ListItem Value="1" Selected="True">Oui (Paye)</asp:ListItem>
                                        <asp:ListItem Value="0">Non (Non Paye)</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-4">
                                <div class="mb-3">
                                    <label for="txtDate" class="form-label">Date *</label>
                                    <asp:TextBox ID="txtDate" runat="server" CssClass="form-control" TextMode="Date" />
                                    <asp:RequiredFieldValidator ID="rfvDate" runat="server" ControlToValidate="txtDate"
                                        ErrorMessage="La date est obligatoire" CssClass="text-danger" Display="Dynamic" />
                                </div>
                            </div>
                            <div class="col-md-4">
                                <div class="mb-3">
                                    <label class="form-label d-block">Recurrent</label>
                                    <div class="form-check form-check-inline">
                                        <asp:RadioButton ID="rbRecurrentYes" runat="server" GroupName="Recurrent"
                                            CssClass="form-check-input" onclick="toggleRecurringOptions(true);" />
                                        <label class="form-check-label" for="<%= rbRecurrentYes.ClientID %>">Oui</label>
                                    </div>
                                    <div class="form-check form-check-inline">
                                        <asp:RadioButton ID="rbRecurrentNo" runat="server" GroupName="Recurrent"
                                            CssClass="form-check-input" Checked="true" onclick="toggleRecurringOptions(false);" />
                                        <label class="form-check-label" for="<%= rbRecurrentNo.ClientID %>">Non</label>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-4">
                                <div class="mb-3" id="divRecurringType" style="display:none;">
                                    <label for="ddlRecurringType" class="form-label">Type de Recurrence</label>
                                    <asp:DropDownList ID="ddlRecurringType" runat="server" CssClass="form-select">
                                        <asp:ListItem Value="YEARLY">Annuel (meme date)</asp:ListItem>
                                        <asp:ListItem Value="MONTHLY">Mensuel</asp:ListItem>
                                        <asp:ListItem Value="CUSTOM">Personnalise</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <div class="mb-3">
                                    <label for="txtDescription" class="form-label">Description</label>
                                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control"
                                        TextMode="MultiLine" Rows="2" placeholder="Description du jour ferie..." />
                                </div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <asp:Button ID="btnSave" runat="server" Text="Enregistrer" CssClass="btn btn-success" OnClick="btnSave_Click" />
                                <asp:Button ID="btnCancel" runat="server" Text="Annuler" CssClass="btn btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
                                <asp:HiddenField ID="hfEditId" runat="server" Value="0" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Filter Section -->
        <div class="row">
            <div class="col-12">
                <div class="filter-section">
                    <div class="row align-items-center">
                        <div class="col-md-4">
                            <div class="year-selector">
                                <div class="btn-group" role="group">
                                    <asp:Button ID="btnPrevYear" runat="server" Text="◄" CssClass="btn btn-outline-primary"
                                        OnClick="btnPrevYear_Click" CausesValidation="false" />
                                    <asp:Button ID="btnCurrentYear" runat="server" CssClass="btn btn-primary"
                                        OnClick="btnCurrentYear_Click" CausesValidation="false" />
                                    <asp:Button ID="btnNextYear" runat="server" Text="►" CssClass="btn btn-outline-primary"
                                        OnClick="btnNextYear_Click" CausesValidation="false" />
                                </div>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <asp:DropDownList ID="ddlFilterType" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlFilterType_SelectedIndexChanged">
                                <asp:ListItem Value="">Tous les Types</asp:ListItem>
                                <asp:ListItem Value="FERIE">Jours Feries</asp:ListItem>
                                <asp:ListItem Value="SPECIAL">Jours Speciaux</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-4">
                            <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlFilterStatus_SelectedIndexChanged">
                                <asp:ListItem Value="">Tous</asp:ListItem>
                                <asp:ListItem Value="PAST">Passes</asp:ListItem>
                                <asp:ListItem Value="UPCOMING">A Venir</asp:ListItem>
                                <asp:ListItem Value="TODAY">Aujourd'hui</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="row mt-3">
                        <div class="col-12">
                            <div class="legend-item">
                                <span class="legend-color" style="background-color: #dc3545;"></span>
                                <span>Jour Ferie</span>
                            </div>
                            <div class="legend-item">
                                <span class="legend-color" style="background-color: #ffc107;"></span>
                                <span>Jour Special</span>
                            </div>
                            <div class="legend-item">
                                <span class="legend-color" style="background-color: #28a745;"></span>
                                <span>Recurrent</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Holidays List -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="card-title mb-0">
                            <i class="fas fa-calendar-alt me-2"></i>Liste des Jours Feries et Speciaux
                        </h5>
                    </div>
                    <div class="card-body p-0">
                        <div class="table-responsive">
                            <asp:GridView ID="gvHolidays" runat="server" CssClass="table table-hover mb-0"
                                AutoGenerateColumns="false" DataKeyNames="IdJourFerie"
                                OnRowCommand="gvHolidays_RowCommand"
                                EmptyDataText="Aucun jour ferie trouve pour cette periode">
                                <Columns>
                                    <asp:TemplateField HeaderText="Date">
                                        <ItemTemplate>
                                            <div>
                                                <strong><%# Convert.ToDateTime(Eval("DateJour")).ToString("dd/MM/yyyy") %></strong>
                                                <br />
                                                <small class="text-muted"><%# Convert.ToDateTime(Eval("DateJour")).ToString("dddd", new System.Globalization.CultureInfo("fr-FR")) %></small>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Nom" HeaderText="Nom" />
                                    <asp:TemplateField HeaderText="Type">
                                        <ItemTemplate>
                                            <%# Eval("TypeJour").ToString() == "FERIE"
                                                ? "<span class='badge-holiday'>Jour Ferie</span>"
                                                : "<span class='badge-special'>Jour Special</span>" %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Paye">
                                        <ItemTemplate>
                                            <%# Convert.ToBoolean(Eval("IsPaid"))
                                                ? "<span class='badge bg-success'>Oui</span>"
                                                : "<span class='badge bg-warning'>Non</span>" %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Recurrent">
                                        <ItemTemplate>
                                            <%# Convert.ToBoolean(Eval("IsRecurrent"))
                                                ? "<span class='badge bg-info'>" + Eval("RecurringType") + "</span>"
                                                : "<span class='badge bg-secondary'>Non</span>" %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Description" HeaderText="Description" />
                                    <asp:TemplateField HeaderText="Statut">
                                        <ItemTemplate>
                                            <%# GetStatusBadge(Eval("DateJour")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <div class="action-buttons">
                                                <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-sm btn-outline-primary"
                                                    CommandName="EditHoliday" CommandArgument='<%# Eval("IdJourFerie") %>'
                                                    ToolTip="Modifier" CausesValidation="false">
                                                    <i class="fas fa-edit"></i>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                    CommandName="DeleteHoliday" CommandArgument='<%# Eval("IdJourFerie") %>'
                                                    ToolTip="Supprimer" CausesValidation="false"
                                                    OnClientClick="return confirm('Etes-vous sur de vouloir supprimer ce jour ferie?');">
                                                    <i class="fas fa-trash"></i>
                                                </asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate>
                                    <div class="text-center py-5">
                                        <i class="fas fa-calendar-times fa-3x text-muted mb-3"></i>
                                        <p class="text-muted">Aucun jour ferie trouve pour cette periode</p>
                                        <small class="text-muted">Utilisez le formulaire ci-dessus pour ajouter des jours feries</small>
                                    </div>
                                </EmptyDataTemplate>
                            </asp:GridView>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>

    <script type="text/javascript">
        function toggleRecurringOptions(show) {
            var div = document.getElementById('divRecurringType');
            if (div) {
                div.style.display = show ? 'block' : 'none';
            }
        }

        // Initialize on page load
        document.addEventListener('DOMContentLoaded', function () {
            var rbYes = document.getElementById('<%= rbRecurrentYes.ClientID %>');
            if (rbYes && rbYes.checked) {
                toggleRecurringOptions(true);
            }
        });
    </script>
</asp:Content>
