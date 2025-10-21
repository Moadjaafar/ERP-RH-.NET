<%@ Page Title="Pointage Mobile" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PointageMobile.aspx.cs" Inherits="APPLICATION_RH.PointageMobile.PointageMobile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .status-present { background-color: #d4edda; color: #155724; padding: 4px 8px; border-radius: 4px; font-size: 12px; }
        .status-absent { background-color: #f8d7da; color: #721c24; padding: 4px 8px; border-radius: 4px; font-size: 12px; }
        .status-demi_journee { background-color: #fff3cd; color: #856404; padding: 4px 8px; border-radius: 4px; font-size: 12px; }
        .status-traite { background-color: #cce5ff; color: #004085; padding: 4px 8px; border-radius: 4px; font-size: 12px; }
        .status-exporte { background-color: #e2e6ea; color: #383d41; padding: 4px 8px; border-radius: 4px; font-size: 12px; }
        .status-enattente { background-color: #f0f0f0; color: #6c757d; padding: 4px 8px; border-radius: 4px; font-size: 12px; }
        
        .table th { background-color: #f8f9fa; font-weight: 600; }
        .clickable-row { cursor: pointer; }
        .clickable-row:hover { background-color: #f5f5f5; }
        .table .selected-row { background-color: #cce5ff !important; }
        
        .details-panel {
            margin-top: 20px;
            border: 1px solid #dee2e6;
            border-radius: 8px;
        }
        
        .details-header {
            background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
            color: white;
            padding: 15px 20px;
            border-radius: 8px 8px 0 0;
            margin: 0;
        }
        
        .details-scroll {
            max-height: 400px;
            overflow-y: auto;
            border: 1px solid #dee2e6;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        
        <!-- En-tête -->
        <div class="row mb-4">
            <div class="col-12">
                <h3><i class="ti ti-device-mobile"></i> Pointages Mobile</h3>
            </div>
        </div>

        <!-- Total -->
        <div class="row mb-3">
            <div class="col-12">
                <asp:Label ID="lblTotal" runat="server" Text="Total: 0 dates" CssClass="h5"></asp:Label>
            </div>
        </div>

        <!-- Tableau des dates (Master) -->
        <div class="row">
            <div class="col-12">
                <div class="card">
                    <div class="card-header">
                        <h5 class="mb-0">Résumé par date - Cliquez sur une date pour voir les détails</h5>
                    </div>
                    <div class="card-body p-0">
                        <asp:GridView ID="gvDates" runat="server" 
                            CssClass="table table-striped mb-0"
                            AutoGenerateColumns="false"
                            OnSelectedIndexChanged="gvDates_SelectedIndexChanged"
                            DataKeyNames="DatePointage"
                            EmptyDataText="Aucune date trouvée">
                            
                            <Columns>
                                <asp:CommandField ShowSelectButton="True" SelectText="►" 
                                    ItemStyle-Width="50px" HeaderText="Voir" />
                                
                                <asp:BoundField DataField="DatePointage" HeaderText="Date" 
                                    DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Font-Bold="true" />
                                
                                <asp:BoundField DataField="Total" HeaderText="Total" 
                                    ItemStyle-CssClass="text-center fw-bold" />
                                
                                <asp:TemplateField HeaderText="Présent">
                                    <ItemTemplate>
                                        <span class="badge bg-success"><%# Eval("Present") %></span>
                                    </ItemTemplate>
                                    <ItemStyle CssClass="text-center" />
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Absent">
                                    <ItemTemplate>
                                        <span class="badge bg-danger"><%# Eval("Absent") %></span>
                                    </ItemTemplate>
                                    <ItemStyle CssClass="text-center" />
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Demi-journée">
                                    <ItemTemplate>
                                        <span class="badge bg-warning"><%# Eval("DemiJournee") %></span>
                                    </ItemTemplate>
                                    <ItemStyle CssClass="text-center" />
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Traité">
                                    <ItemTemplate>
                                        <span class="badge bg-info"><%# Eval("Traite") %></span>
                                    </ItemTemplate>
                                    <ItemStyle CssClass="text-center" />
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="En attente">
                                    <ItemTemplate>
                                        <span class="badge bg-secondary"><%# Eval("EnAttente") %></span>
                                    </ItemTemplate>
                                    <ItemStyle CssClass="text-center" />
                                </asp:TemplateField>
                            </Columns>
                            
                            <SelectedRowStyle CssClass="selected-row" />
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>

        <!-- Panel des détails (Detail) -->
        <asp:Panel ID="panelDetails" runat="server" CssClass="details-panel" Visible="false">
            <div class="row">
                <div class="col-12">
                    
                    <!-- En-tête des détails -->
                    <h4 class="details-header">
                        <i class="ti ti-list-details"></i>
                        <asp:Label ID="lblSelectedDate" runat="server" Text="Détails"></asp:Label>
                        
                        <div class="float-end">
                            <asp:Button ID="btnValiderDate" runat="server" Text="Valider cette date" 
                                CssClass="btn btn-warning btn-sm me-2" OnClick="btnValiderDate_Click"
                                OnClientClick="return confirm('Valider tous les pointages de cette date ?');" />
                            
                            <asp:Button ID="btnFermerDetails" runat="server" Text="✗ Fermer" 
                                CssClass="btn btn-light btn-sm" OnClick="btnFermerDetails_Click" />
                        </div>
                    </h4>
                    
                    <!-- Tableau des détails -->
                    <div class="details-scroll">
                        <asp:GridView ID="gvDetails" runat="server" 
                            CssClass="table table-striped table-sm mb-0"
                            AutoGenerateColumns="false"
                            OnRowCommand="gvDetails_RowCommand"
                            DataKeyNames="IdPointageMobile"
                            EmptyDataText="Aucun pointage trouvé">
                            
                            <Columns>
                                <asp:BoundField DataField="Matricule" HeaderText="Matricule" />
                                <asp:BoundField DataField="NomComplet" HeaderText="Nom Complet" />
                                
                                <asp:TemplateField HeaderText="Statut">
                                    <ItemTemplate>
                                        <span class="status-<%# Eval("Statut") %>">
                                            <%# GetStatutText(Eval("Statut").ToString()) %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="État">
                                    <ItemTemplate>
                                        <span class="status-<%# GetStatutClass(Eval("StatutTraitement").ToString()) %>">
                                            <%# Eval("StatutTraitement") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:BoundField DataField="DateCreation" HeaderText="Créé le" DataFormatString="{0:HH:mm}" />
                                <asp:BoundField DataField="DeviceInfo" HeaderText="Appareil" />
                                
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:Button ID="btnValider" runat="server" Text="✓" 
                                            CssClass="btn btn-sm btn-success me-1"
                                            CommandName="Valider" 
                                            CommandArgument='<%# Eval("IdPointageMobile") %>'
                                            Visible='<%# Eval("StatutTraitement").ToString() != "Traité" %>'
                                            ToolTip="Valider" />
                                        
                                        <asp:Button ID="btnSupprimer" runat="server" Text="✗" 
                                            CssClass="btn btn-sm btn-danger"
                                            CommandName="Supprimer" 
                                            CommandArgument='<%# Eval("IdPointageMobile") %>'
                                            OnClientClick="return confirm('Supprimer ce pointage ?');"
                                            ToolTip="Supprimer" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                    
                </div>
            </div>
        </asp:Panel>

    </div>

    <script runat="server">
        protected string GetStatutText(string statut)
        {
            switch (statut?.ToLower())
            {
                case "present": return "Présent";
                case "absent": return "Absent";
                case "demi_journee": return "Demi-journée";
                default: return statut;
            }
        }

        protected string GetStatutClass(string statutTraitement)
        {
            switch (statutTraitement?.ToLower())
            {
                case "traité": return "traite";
                case "exporté": return "exporte";
                case "en attente": return "enattente";
                default: return "enattente";
            }
        }
    </script>

</asp:Content>