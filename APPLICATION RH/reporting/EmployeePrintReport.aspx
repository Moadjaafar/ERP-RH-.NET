<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EmployeePrintReport.aspx.cs" Inherits="APPLICATION_RH.reporting.EmployeePrintReport" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>Récapitulatif de Pointage Employé</title>
    <style>
        @media print {
            @page { 
                margin: 1cm; 
                size: A4;
            }
            body {
                -webkit-print-color-adjust: exact;
                color-adjust: exact;
            }
            .no-print {
                display: none !important;
            }
        }

        body { 
            font-family: Arial, sans-serif; 
            margin: 20px; 
            background-color: white;
            color: black;
        }

        .print-header { 
            text-align: center; 
            border-bottom: 2px solid #000; 
            padding-bottom: 20px; 
            margin-bottom: 20px; 
        }

        .print-header h1 { 
            margin: 0; 
            font-size: 24px; 
            color: #333;
        }

        .print-header h2 { 
            margin: 5px 0; 
            font-size: 18px; 
            color: #666; 
        }

        .employee-info { 
            background-color: #f8f9fa; 
            padding: 15px; 
            margin-bottom: 20px; 
            border: 1px solid #ddd; 
        }

        .info-row { 
            display: table; 
            width: 100%; 
            margin-bottom: 10px; 
        }

        .info-left { 
            display: table-cell; 
            width: 50%; 
            vertical-align: top;
        }

        .info-right { 
            display: table-cell; 
            width: 50%; 
            text-align: right; 
            vertical-align: top;
        }

        .summary { 
            background-color: #e7f3ff; 
            padding: 15px; 
            margin-bottom: 20px; 
            border: 1px solid #007bff; 
        }

        .summary-row { 
            display: table; 
            width: 100%; 
        }

        .summary-item { 
            display: table-cell; 
            text-align: center; 
            padding: 10px; 
        }

        .summary-item h3 { 
            margin: 0; 
            font-size: 24px; 
            color: #007bff; 
        }

        .summary-item p { 
            margin: 5px 0 0 0; 
            color: #666; 
        }

        table { 
            width: 100%; 
            border-collapse: collapse; 
            margin: 20px 0; 
        }

        th, td { 
            border: 1px solid #000; 
            padding: 8px; 
            text-align: left; 
        }

        th { 
            background-color: #f0f0f0; 
            font-weight: bold; 
        }

        .text-center { 
            text-align: center; 
        }

        .text-success { 
            color: green; 
            font-weight: bold; 
        }

        .text-danger { 
            color: red; 
            font-weight: bold; 
        }

        .footer { 
            margin-top: 30px; 
            text-align: center; 
            font-size: 12px; 
            color: #666; 
            border-top: 1px solid #ccc; 
            padding-top: 10px; 
        }

        h3 { 
            color: #333; 
            border-bottom: 1px solid #ddd; 
            padding-bottom: 5px; 
        }

        .print-buttons {
            text-align: center;
            margin: 20px 0;
            padding: 10px;
            background-color: #f8f9fa;
            border: 1px solid #ddd;
        }

        .print-buttons button {
            margin: 0 10px;
            padding: 10px 20px;
            font-size: 14px;
            cursor: pointer;
            border: none;
            border-radius: 4px;
        }

        .btn-print {
            background-color: #007bff;
            color: white;
        }

        .btn-print:hover {
            background-color: #0056b3;
        }

        .btn-close {
            background-color: #6c757d;
            color: white;
        }

        .btn-close:hover {
            background-color: #545b62;
        }

        .loading {
            text-align: center;
            padding: 50px;
            font-size: 18px;
            color: #666;
        }

        .error {
            text-align: center;
            padding: 20px;
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
            margin: 20px 0;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        
        <!-- Print Buttons (hidden when printing) -->
        <div class="print-buttons no-print">
            <button type="button" onclick="window.print();" class="btn-print">
                <i class="fas fa-print"></i> Imprimer
            </button>
            <button type="button" onclick="window.close();" class="btn-close">
                <i class="fas fa-times"></i> Fermer
            </button>
        </div>

        <!-- Loading Panel -->
        <asp:Panel ID="pnlLoading" runat="server" Visible="false" CssClass="loading">
            <i class="fas fa-spinner fa-spin"></i>
            <p>Chargement des données d'impression...</p>
        </asp:Panel>

        <!-- Error Panel -->
        <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error">
            <h4>Erreur</h4>
            <asp:Literal ID="litError" runat="server" />
        </asp:Panel>

        <!-- Print Content -->
        <asp:Panel ID="pnlPrintContent" runat="server" Visible="false">
            
            <!-- Header -->
            <div class="print-header">
                <h1>RÉCAPITULATIF DE POINTAGE EMPLOYÉ</h1>
                <h2>APPLICATION RH - SYSTÈME DE POINTAGE</h2>
                <p>Rapport généré le <asp:Literal ID="litGenerationDate" runat="server" /></p>
            </div>

            <!-- Employee Info -->
            <div class="employee-info">
                <div class="info-row">
                    <div class="info-left">
                        <h3>Informations Employé</h3>
                        <p><strong>Nom Complet:</strong> <asp:Literal ID="litEmployeeName" runat="server" /></p>
                        <p><strong>Matricule:</strong> <asp:Literal ID="litEmployeeMatricule" runat="server" /></p>
                        <p><strong>Port Assigné:</strong> <asp:Literal ID="litEmployeePort" runat="server" /></p>
                    </div>
                    <div class="info-right">
                        <h3>Période du Rapport</h3>
                        <p><strong>Du:</strong> <asp:Literal ID="litPeriodFrom" runat="server" /></p>
                        <p><strong>Au:</strong> <asp:Literal ID="litPeriodTo" runat="server" /></p>
                        <p><strong>Port Filtré:</strong> <asp:Literal ID="litPortFilter" runat="server" /></p>
                    </div>
                </div>
            </div>

            <!-- Summary -->
            <div class="summary">
                <h3>Résumé de la Période</h3>
                <div class="summary-row">
                    <div class="summary-item">
                        <h3><asp:Literal ID="litTotalPointages" runat="server" /></h3>
                        <p>Total Pointages</p>
                    </div>
                    <div class="summary-item">
                        <h3><asp:Literal ID="litTotalEntrees" runat="server" /></h3>
                        <p>Entrées</p>
                    </div>
                    <div class="summary-item">
                        <h3><asp:Literal ID="litTotalSorties" runat="server" /></h3>
                        <p>Sorties</p>
                    </div>
                    <div class="summary-item">
                        <h3><asp:Literal ID="litTotalHours" runat="server" /></h3>
                        <p>Heures Travaillées</p>
                    </div>
                </div>
            </div>

            <!-- Pointages Detail -->
            <h3>Détail des Pointages</h3>
            <asp:GridView ID="gvPointageDetail" runat="server" CssClass="table" 
                AutoGenerateColumns="false" 
                EmptyDataText="Aucun pointage trouvé pour cette période">
                <Columns>
                    <asp:BoundField DataField="DateFormatted" HeaderText="Date" />
                    <asp:BoundField DataField="HeureFormatted" HeaderText="Heure" />
                    <asp:BoundField DataField="Type" HeaderText="Type" />
                    <asp:BoundField DataField="Port" HeaderText="Port" />
                    <asp:TemplateField HeaderText="Statut">
                        <ItemTemplate>
                            <span class='<%# Eval("Type").ToString() == "ENTREE" ? "text-success" : "text-danger" %>'>
                                <%# Eval("Type").ToString() == "ENTREE" ? "ENTRÉE" : "SORTIE" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <!-- Daily Analysis -->
            <h3>Analyse par Jour</h3>
            <asp:GridView ID="gvDailyAnalysis" runat="server" CssClass="table" 
                AutoGenerateColumns="false" 
                EmptyDataText="Aucune analyse journalière disponible">
                <Columns>
                    <asp:BoundField DataField="Date" HeaderText="Date" />
                    <asp:BoundField DataField="PremiereEntree" HeaderText="Première Entrée" />
                    <asp:BoundField DataField="DerniereSortie" HeaderText="Dernière Sortie" />
                    <asp:BoundField DataField="HeuresStandard" HeaderText="Heures Standard" ItemStyle-CssClass="text-center" />
                    <asp:BoundField DataField="HeuresTravaillees" HeaderText="Heures Travaillées" ItemStyle-CssClass="text-center" />
                    <asp:TemplateField HeaderText="Différence" ItemStyle-CssClass="text-center">
                        <ItemTemplate>
                            <span class='<%# GetDifferenceClass(Eval("Difference").ToString()) %>'>
                                <%# Eval("Difference") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="NombrePointages" HeaderText="Nb Pointages" ItemStyle-CssClass="text-center" />
                </Columns>
            </asp:GridView>

            <!-- Footer -->
            <div class="footer">
                <p>Rapport généré automatiquement par le système APPLICATION RH</p>
                <p>Date d'impression: <asp:Literal ID="litPrintDate" runat="server" /></p>
            </div>

        </asp:Panel>

    </form>

    <script>
        // Auto-print option (uncomment if needed)
        // window.onload = function() { 
        //     window.print(); 
        // };

        // Handle print button click
        function printReport() {
            window.print();
        }

        // Handle close button click
        function closeWindow() {
            window.close();
        }
    </script>
</body>
</html>