using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Drawing;
using ClosedXML.Excel;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.Threading;

namespace APPLICATION_RH.reporting
{
    public partial class RaportPointagePort : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetDefaultDates();
                PopulatePortDropdown();
                pnlNoResults.Visible = true;
                pnlResults.Visible = false;
            }
        }


        private void PopulatePortDropdown()
        {
            // Clear existing items
            ddlPort.Items.Clear();

            // Add default option
            ddlPort.Items.Add(new ListItem("-- Toutes les Portes --", ""));

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            string query = "SELECT idPorte, Porte FROM PORTEUSINE ORDER BY Porte";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string portName = reader["Porte"].ToString();
                                string portId = reader["idPorte"].ToString();

                                // Add port to dropdown (you can use portId as value if needed)
                                ddlPort.Items.Add(new ListItem(portName, portName));
                            }
                        }
                    }
                }

                // Set default selection
                ddlPort.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                // Handle error - add error message to dropdown
                ddlPort.Items.Add(new ListItem("Erreur lors du chargement des portes", ""));

                // Log error for debugging
                System.Diagnostics.Debug.WriteLine($"Error loading ports: {ex.Message}");
            }
        }

        private void SetDefaultDates()
        {
            DateTime today = DateTime.Now;
            DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            txtDateFrom.Text = firstDayOfMonth.ToString("yyyy-MM-dd");
            txtDateTo.Text = today.ToString("yyyy-MM-dd");
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (ValidateDates())
            {
                LoadPointageData();
            }
        }

        protected void btnToday_Click(object sender, EventArgs e)
        {
            DateTime today = DateTime.Now;
            txtDateFrom.Text = today.ToString("yyyy-MM-dd");
            txtDateTo.Text = today.ToString("yyyy-MM-dd");
            LoadPointageData();
        }

        protected void btnYesterday_Click(object sender, EventArgs e)
        {
            DateTime yesterday = DateTime.Now.AddDays(-1);
            txtDateFrom.Text = yesterday.ToString("yyyy-MM-dd");
            txtDateTo.Text = yesterday.ToString("yyyy-MM-dd");
            LoadPointageData();
        }


        //protected void btnPrint_Command(object sender, CommandEventArgs e)
        //{
        //    if (e.CommandName == "PrintEmployee")
        //    {
        //        try
        //        {
        //            int employeeId = Convert.ToInt32(e.CommandArgument);
        //            GenerateEmployeePrintReport(employeeId);
        //        }
        //        catch (Exception ex)
        //        {
        //            ShowError("Erreur lors de la génération du rapport: " + ex.Message);
        //        }
        //    }
        //}

        protected void btnPrint_Command(object sender, CommandEventArgs e)
        {
            if (e.CommandName == "PrintEmployee")
            {
                try
                {
                    int employeeId = Convert.ToInt32(e.CommandArgument);

                    // Store the search parameters in session or pass via URL
                    Session["PrintEmployeeId"] = employeeId;
                    Session["PrintDateFrom"] = txtDateFrom.Text;
                    Session["PrintDateTo"] = txtDateTo.Text;
                    Session["PrintSelectedPort"] = ddlPort.SelectedValue;

                    // Redirect to the print page
                    string printPageUrl = "~/reporting/EmployeePrintReport.aspx";

                    // Alternative: Pass parameters via URL (comment out session approach above if using this)
                    /*
                    string printPageUrl = string.Format("~/reporting/EmployeePrintReport.aspx?empId={0}&dateFrom={1}&dateTo={2}&port={3}",
                        employeeId, 
                        Server.UrlEncode(txtDateFrom.Text),
                        Server.UrlEncode(txtDateTo.Text), 
                        Server.UrlEncode(ddlPort.SelectedValue));
                    */

                    // Open in new window using JavaScript
                    string script = string.Format("window.open('{0}', '_blank', 'width=800,height=600,scrollbars=yes,resizable=yes');",
                        ResolveUrl(printPageUrl));

                    ClientScript.RegisterStartupScript(this.GetType(), "OpenPrintWindow", script, true);
                }
                catch (Exception ex)
                {
                    ShowError("Erreur lors de l'ouverture de la page d'impression: " + ex.Message);
                }
            }
        }

        //private void GenerateEmployeePrintReport(int employeeId)
        //{
        //    try
        //    {
        //        DateTime dateFrom, dateTo;

        //        // Try to parse dates with more robust parsing
        //        if (string.IsNullOrEmpty(txtDateFrom.Text) || string.IsNullOrEmpty(txtDateTo.Text))
        //        {
        //            ShowError("Veuillez sélectionner les dates de début et fin");
        //            return;
        //        }

        //        try
        //        {
        //            dateFrom = DateTime.ParseExact(txtDateFrom.Text, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        //        }
        //        catch
        //        {
        //            if (!DateTime.TryParse(txtDateFrom.Text, out dateFrom))
        //            {
        //                ShowError("Format de date début invalide: " + txtDateFrom.Text);
        //                return;
        //            }
        //        }

        //        try
        //        {
        //            dateTo = DateTime.ParseExact(txtDateTo.Text, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        //        }
        //        catch
        //        {
        //            if (!DateTime.TryParse(txtDateTo.Text, out dateTo))
        //            {
        //                ShowError("Format de date fin invalide: " + txtDateTo.Text);
        //                return;
        //            }
        //        }

        //        // Validate date range
        //        if (dateFrom > dateTo)
        //        {
        //            ShowError("La date de début doit être antérieure à la date de fin");
        //            return;
        //        }

        //        string selectedPort = ddlPort.SelectedValue;
        //        var employeeData = GetEmployeePrintDataDirect(employeeId, dateFrom, dateTo, selectedPort);

        //        if (employeeData == null)
        //        {
        //            ShowError("Impossible de charger les données de l'employé");
        //            return;
        //        }

        //        // Generate HTML report
        //        string htmlReport = GenerateHtmlReport(employeeData, dateFrom, dateTo, selectedPort);

        //        // Create a temporary file or serve directly
        //        Response.Clear();
        //        Response.ContentType = "text/html; charset=utf-8";
        //        Response.AddHeader("Content-Disposition", "inline; filename=Rapport_Employe_" + employeeData.Matricule + "_" + DateTime.Now.ToString("yyyyMMdd") + ".html");
        //        Response.Write(htmlReport);
        //        Response.Flush();
        //        Response.End();
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowError("Erreur lors de la génération du rapport: " + ex.Message + " (Stack: " + ex.StackTrace + ")");
        //    }
        //}

        //private EmployeePrintDataDirect GetEmployeePrintDataDirect(int employeeId, DateTime dateFrom, DateTime dateTo, string selectedPort)
        //{
        //    var employeeData = new EmployeePrintDataDirect();

        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        // Get employee basic info
        //        string empQuery = @"
        //            SELECT 
        //                e.Matricule,
        //                e.Nom + ' ' + e.Prenom AS NomComplet,
        //                ISNULL(ep.NomPort, 'Aucun port assigné') AS NomPort
        //            FROM EMPLOYE e
        //            LEFT JOIN EMPLOYE_PORT ep ON e.IdEmploye = ep.IdEmploye AND ep.is_deleted = 0
        //            WHERE e.IdEmploye = @EmployeeId AND e.is_deleted = 0";

        //        using (SqlCommand empCmd = new SqlCommand(empQuery, conn))
        //        {
        //            empCmd.Parameters.AddWithValue("@EmployeeId", employeeId);
        //            conn.Open();

        //            using (SqlDataReader reader = empCmd.ExecuteReader())
        //            {
        //                if (reader.Read())
        //                {
        //                    employeeData.Matricule = reader["Matricule"].ToString();
        //                    employeeData.NomComplet = reader["NomComplet"].ToString();
        //                    employeeData.PortAssigne = reader["NomPort"].ToString();
        //                }
        //                else
        //                {
        //                    return null; // Employee not found
        //                }
        //            }
        //            conn.Close();
        //        }

        //        // Get detailed pointage data
        //        string pointageQuery = @"
        //            SELECT 
        //                TypePointage,
        //                DateHeure,
        //                ISNULL(ep.NomPort, 'N/A') AS Port
        //            FROM EMPLOYE_POINTAGE p
        //            LEFT JOIN EMPLOYE_PORT ep ON p.IdEmploye = ep.IdEmploye AND ep.is_deleted = 0
        //            WHERE p.IdEmploye = @EmployeeId 
        //            AND p.is_deleted = 0 
        //            AND CAST(p.DateHeure AS DATE) BETWEEN @DateFrom AND @DateTo";

        //        if (!string.IsNullOrEmpty(selectedPort))
        //        {
        //            pointageQuery += " AND ep.NomPort = @SelectedPort";
        //        }

        //        pointageQuery += " ORDER BY p.DateHeure";

        //        using (SqlCommand pointageCmd = new SqlCommand(pointageQuery, conn))
        //        {
        //            pointageCmd.Parameters.AddWithValue("@EmployeeId", employeeId);
        //            pointageCmd.Parameters.AddWithValue("@DateFrom", dateFrom.Date);
        //            pointageCmd.Parameters.AddWithValue("@DateTo", dateTo.Date);

        //            if (!string.IsNullOrEmpty(selectedPort))
        //            {
        //                pointageCmd.Parameters.AddWithValue("@SelectedPort", selectedPort);
        //            }

        //            conn.Open();
        //            var pointages = new List<PointageDetailDirect>();
        //            var dailyAnalysisDict = new Dictionary<DateTime, DailyAnalysisDirect>();
        //            var allEvents = new List<PointageEvent>();

        //            using (SqlDataReader reader = pointageCmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    DateTime dateHeure = Convert.ToDateTime(reader["DateHeure"]);
        //                    string type = reader["TypePointage"].ToString();
        //                    string port = reader["Port"].ToString();

        //                    pointages.Add(new PointageDetailDirect
        //                    {
        //                        DateFormatted = dateHeure.ToString("dd/MM/yyyy"),
        //                        HeureFormatted = dateHeure.ToString("HH:mm"),
        //                        Type = type,
        //                        Port = port
        //                    });

        //                    allEvents.Add(new PointageEvent { DateHeure = dateHeure, Type = type });

        //                    // Initialize daily analysis
        //                    DateTime date = dateHeure.Date;
        //                    if (!dailyAnalysisDict.ContainsKey(date))
        //                    {
        //                        dailyAnalysisDict[date] = new DailyAnalysisDirect
        //                        {
        //                            Date = date.ToString("dd/MM/yyyy"),
        //                            PremiereEntree = null,
        //                            DerniereSortie = null,
        //                            NombrePointages = 0,
        //                            Events = new List<PointageEvent>()
        //                        };
        //                    }

        //                    dailyAnalysisDict[date].NombrePointages++;
        //                    dailyAnalysisDict[date].Events.Add(new PointageEvent { DateHeure = dateHeure, Type = type });

        //                    if (type == "ENTREE" && dailyAnalysisDict[date].PremiereEntree == null)
        //                    {
        //                        dailyAnalysisDict[date].PremiereEntree = dateHeure.ToString("HH:mm");
        //                    }
        //                    if (type == "SORTIE")
        //                    {
        //                        dailyAnalysisDict[date].DerniereSortie = dateHeure.ToString("HH:mm");
        //                    }
        //                }
        //            }

        //            employeeData.Pointages = pointages;
        //            employeeData.TotalPointages = pointages.Count;
        //            employeeData.TotalEntrees = pointages.Count(p => p.Type == "ENTREE");
        //            employeeData.TotalSorties = pointages.Count(p => p.Type == "SORTIE");

        //            // Calculate total working hours
        //            employeeData.HeuresTravaillees = CalculateWorkingHoursFromEventsStatic(allEvents, dateFrom, dateTo);

        //            // Calculate daily hours
        //            foreach (var kvp in dailyAnalysisDict)
        //            {
        //                kvp.Value.HeuresTravaillees = CalculateDailyWorkingHours(kvp.Value.Events, kvp.Key);
        //            }

        //            employeeData.DailyAnalysis = dailyAnalysisDict.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
        //            conn.Close();
        //        }
        //    }

        //    return employeeData;
        //}

//        private string GenerateHtmlReport(EmployeePrintDataDirect employeeData, DateTime dateFrom, DateTime dateTo, string selectedPort)
//        {
//            try
//            {
//                string currentDate = DateTime.Now.ToString("dd MMMM yyyy à HH:mm", System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
//                string dateFromFormatted = dateFrom.ToString("dd/MM/yyyy");
//                string dateToFormatted = dateTo.ToString("dd/MM/yyyy");
//                string portFilter = string.IsNullOrEmpty(selectedPort) ? "Tous les ports" : selectedPort;

//                StringBuilder html = new StringBuilder();

//                html.Append(@"
//<!DOCTYPE html>
//<html>
//<head>
//    <meta charset='utf-8' />
//    <title>Récapitulatif de Pointage - " + employeeData.NomComplet + @"</title>
//    <style>
//        @media print {
//            @page { margin: 1cm; }
//        }
//        body { font-family: Arial, sans-serif; margin: 20px; }
//        .print-header { text-align: center; border-bottom: 2px solid #000; padding-bottom: 20px; margin-bottom: 20px; }
//        .print-header h1 { margin: 0; font-size: 24px; }
//        .print-header h2 { margin: 5px 0; font-size: 18px; color: #666; }
//        .employee-info { background-color: #f8f9fa; padding: 15px; margin-bottom: 20px; border: 1px solid #ddd; }
//        .info-row { display: table; width: 100%; margin-bottom: 10px; }
//        .info-left { display: table-cell; width: 50%; }
//        .info-right { display: table-cell; width: 50%; text-align: right; }
//        .summary { background-color: #e7f3ff; padding: 15px; margin-bottom: 20px; border: 1px solid #007bff; }
//        .summary-row { display: table; width: 100%; }
//        .summary-item { display: table-cell; text-align: center; padding: 10px; }
//        .summary-item h3 { margin: 0; font-size: 24px; color: #007bff; }
//        .summary-item p { margin: 5px 0 0 0; color: #666; }
//        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
//        th, td { border: 1px solid #000; padding: 8px; text-align: left; }
//        th { background-color: #f0f0f0; font-weight: bold; }
//        .text-center { text-align: center; }
//        .text-success { color: green; font-weight: bold; }
//        .text-danger { color: red; font-weight: bold; }
//        .footer { margin-top: 30px; text-align: center; font-size: 12px; color: #666; border-top: 1px solid #ccc; padding-top: 10px; }
//        h3 { color: #333; border-bottom: 1px solid #ddd; padding-bottom: 5px; }
//    </style>
//    <script>window.onload = function() { window.print(); }</script>
//</head>
//<body>");

//                // Header
//                html.Append(@"
//    <div class='print-header'>
//        <h1>RÉCAPITULATIF DE POINTAGE EMPLOYÉ</h1>
//        <h2>APPLICATION RH - SYSTÈME DE POINTAGE</h2>
//        <p>Rapport généré le " + currentDate + @"</p>
//    </div>");

//                // Employee info
//                html.Append(@"
//    <div class='employee-info'>
//        <div class='info-row'>
//            <div class='info-left'>
//                <h3>Informations Employé</h3>
//                <p><strong>Nom Complet:</strong> " + employeeData.NomComplet + @"</p>
//                <p><strong>Matricule:</strong> " + employeeData.Matricule + @"</p>
//                <p><strong>Port Assigné:</strong> " + employeeData.PortAssigne + @"</p>
//            </div>
//            <div class='info-right'>
//                <h3>Période du Rapport</h3>
//                <p><strong>Du:</strong> " + dateFromFormatted + @"</p>
//                <p><strong>Au:</strong> " + dateToFormatted + @"</p>
//                <p><strong>Port Filtré:</strong> " + portFilter + @"</p>
//            </div>
//        </div>
//    </div>");

//                // Summary
//                html.Append(@"
//    <div class='summary'>
//        <h3>Résumé de la Période</h3>
//        <div class='summary-row'>
//            <div class='summary-item'>
//                <h3>" + employeeData.TotalPointages + @"</h3>
//                <p>Total Pointages</p>
//            </div>
//            <div class='summary-item'>
//                <h3>" + employeeData.TotalEntrees + @"</h3>
//                <p>Entrées</p>
//            </div>
//            <div class='summary-item'>
//                <h3>" + employeeData.TotalSorties + @"</h3>
//                <p>Sorties</p>
//            </div>
//            <div class='summary-item'>
//                <h3>" + employeeData.HeuresTravaillees + @"</h3>
//                <p>Heures Travaillées</p>
//            </div>
//        </div>
//    </div>");

//                // Pointages detail
//                html.Append(@"
//    <h3>Détail des Pointages</h3>
//    <table>
//        <thead>
//            <tr>
//                <th>Date</th>
//                <th>Heure</th>
//                <th>Type</th>
//                <th>Port</th>
//                <th>Statut</th>
//            </tr>
//        </thead>
//        <tbody>");

//                if (employeeData.Pointages.Any())
//                {
//                    foreach (var pointage in employeeData.Pointages)
//                    {
//                        string statusText = pointage.Type == "ENTREE" ? "ENTRÉE" : "SORTIE";
//                        string statusClass = pointage.Type == "ENTREE" ? "text-success" : "text-danger";

//                        html.Append($@"
//            <tr>
//                <td><strong>{pointage.DateFormatted}</strong></td>
//                <td><strong>{pointage.HeureFormatted}</strong></td>
//                <td>{pointage.Type}</td>
//                <td>{pointage.Port}</td>
//                <td class='{statusClass}'>{statusText}</td>
//            </tr>");
//                    }
//                }
//                else
//                {
//                    html.Append(@"
//            <tr>
//                <td colspan='5' style='text-align: center; color: #666; font-style: italic;'>
//                    Aucun pointage trouvé pour cette période
//                </td>
//            </tr>");
//                }

//                html.Append(@"
//        </tbody>
//    </table>");

//                // Daily analysis
//                html.Append(@"
//    <h3>Analyse par Jour</h3>
//    <table>
//        <thead>
//            <tr>
//                <th>Date</th>
//                <th>Première Entrée</th>
//                <th>Dernière Sortie</th>
//                <th>Heures Travaillées</th>
//                <th class='text-center'>Nombre Pointages</th>
//            </tr>
//        </thead>
//        <tbody>");

//                if (employeeData.DailyAnalysis.Any())
//                {
//                    foreach (var day in employeeData.DailyAnalysis)
//                    {
//                        html.Append($@"
//            <tr>
//                <td><strong>{day.Date}</strong></td>
//                <td>{day.PremiereEntree ?? "-"}</td>
//                <td>{day.DerniereSortie ?? "-"}</td>
//                <td><strong>{day.HeuresTravaillees}</strong></td>
//                <td class='text-center'>{day.NombrePointages}</td>
//            </tr>");
//                    }
//                }

//                html.Append(@"
//        </tbody>
//    </table>");

//                // Footer
//                html.Append(@"
//    <div class='footer'>
//        <p>Rapport généré automatiquement par le système APPLICATION RH</p>
//        <p>Date d'impression: " + currentDate + @"</p>
//    </div>
//</body>
//</html>");

//                return html.ToString();
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Erreur lors de la génération du HTML: " + ex.Message, ex);
//            }
//        }

        //[WebMethod]
        //public static object GetEmployeePrintData(string employeeId, string dateFrom, string dateTo, string selectedPort)
        //{
        //    try
        //    {
        //        // Validate input parameters
        //        if (string.IsNullOrEmpty(employeeId))
        //            return new { success = false, message = "ID employé manquant" };

        //        if (string.IsNullOrEmpty(dateFrom) || string.IsNullOrEmpty(dateTo))
        //            return new { success = false, message = "Dates manquantes" };

        //        int empId;
        //        if (!int.TryParse(employeeId, out empId))
        //            return new { success = false, message = "ID employé invalide: " + employeeId };

        //        DateTime dateFromParsed, dateToParsed;
        //        if (!DateTime.TryParse(dateFrom, out dateFromParsed))
        //            return new { success = false, message = "Date début invalide: " + dateFrom };

        //        if (!DateTime.TryParse(dateTo, out dateToParsed))
        //            return new { success = false, message = "Date fin invalide: " + dateTo };

        //        var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        //        if (string.IsNullOrEmpty(connectionString))
        //            return new { success = false, message = "Chaîne de connexion manquante" };

        //        var employeeData = new EmployeePrintData();

        //        using (SqlConnection conn = new SqlConnection(connectionString))
        //        {
        //            // Get employee basic info and port assignment
        //            string empQuery = @"
        //                SELECT 
        //                    e.Matricule,
        //                    e.Nom + ' ' + e.Prenom AS NomComplet,
        //                    ISNULL(ep.NomPort, 'Aucun port assigné') AS NomPort
        //                FROM EMPLOYE e
        //                LEFT JOIN EMPLOYE_PORT ep ON e.IdEmploye = ep.IdEmploye AND ep.is_deleted = 0
        //                WHERE e.IdEmploye = @EmployeeId AND e.is_deleted = 0";

        //            using (SqlCommand empCmd = new SqlCommand(empQuery, conn))
        //            {
        //                empCmd.Parameters.AddWithValue("@EmployeeId", empId);
        //                conn.Open();

        //                using (SqlDataReader reader = empCmd.ExecuteReader())
        //                {
        //                    if (reader.Read())
        //                    {
        //                        employeeData.portAssgine = reader["NomPort"].ToString();
        //                    }
        //                }
        //                conn.Close();
        //            }

        //            // Get detailed pointage data
        //            string pointageQuery = @"
        //                SELECT 
        //                    TypePointage,
        //                    DateHeure,
        //                    ISNULL(ep.NomPort, 'N/A') AS Port
        //                FROM EMPLOYE_POINTAGE p
        //                LEFT JOIN EMPLOYE_PORT ep ON p.IdEmploye = ep.IdEmploye AND ep.is_deleted = 0
        //                WHERE p.IdEmploye = @EmployeeId 
        //                AND p.is_deleted = 0 
        //                AND CAST(p.DateHeure AS DATE) BETWEEN @DateFrom AND @DateTo";

        //            if (!string.IsNullOrEmpty(selectedPort))
        //            {
        //                pointageQuery += " AND ep.NomPort = @SelectedPort";
        //            }

        //            pointageQuery += " ORDER BY p.DateHeure";

        //            using (SqlCommand pointageCmd = new SqlCommand(pointageQuery, conn))
        //            {
        //                pointageCmd.Parameters.AddWithValue("@EmployeeId", empId);
        //                pointageCmd.Parameters.AddWithValue("@DateFrom", dateFromParsed.Date);
        //                pointageCmd.Parameters.AddWithValue("@DateTo", dateToParsed.Date);

        //                if (!string.IsNullOrEmpty(selectedPort))
        //                {
        //                    pointageCmd.Parameters.AddWithValue("@SelectedPort", selectedPort);
        //                }

        //                conn.Open();
        //                var pointages = new List<PointageDetail>();
        //                var dailyAnalysisDict = new Dictionary<DateTime, DailyAnalysis>();
        //                var allEvents = new List<PointageEvent>();

        //                using (SqlDataReader reader = pointageCmd.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        DateTime dateHeure = Convert.ToDateTime(reader["DateHeure"]);
        //                        string type = reader["TypePointage"].ToString();
        //                        string port = reader["Port"].ToString();

        //                        pointages.Add(new PointageDetail
        //                        {
        //                            DateFormatted = dateHeure.ToString("dd/MM/yyyy"),
        //                            HeureFormatted = dateHeure.ToString("HH:mm"),
        //                            Type = type,
        //                            Port = port
        //                        });

        //                        allEvents.Add(new PointageEvent { DateHeure = dateHeure, Type = type });

        //                        // Initialize daily analysis
        //                        DateTime date = dateHeure.Date;
        //                        if (!dailyAnalysisDict.ContainsKey(date))
        //                        {
        //                            dailyAnalysisDict[date] = new DailyAnalysis
        //                            {
        //                                Date = date.ToString("dd/MM/yyyy"),
        //                                PremiereEntree = null,
        //                                DerniereSortie = null,
        //                                NombrePointages = 0,
        //                                Events = new List<PointageEvent>()
        //                            };
        //                        }

        //                        dailyAnalysisDict[date].NombrePointages++;
        //                        dailyAnalysisDict[date].Events.Add(new PointageEvent { DateHeure = dateHeure, Type = type });

        //                        if (type == "ENTREE" && dailyAnalysisDict[date].PremiereEntree == null)
        //                        {
        //                            dailyAnalysisDict[date].PremiereEntree = dateHeure.ToString("HH:mm");
        //                        }
        //                        if (type == "SORTIE")
        //                        {
        //                            dailyAnalysisDict[date].DerniereSortie = dateHeure.ToString("HH:mm");
        //                        }
        //                    }
        //                }

        //                employeeData.pointages = pointages;
        //                employeeData.totalPointages = pointages.Count;
        //                employeeData.totalEntrees = pointages.Count(p => p.Type == "ENTREE");
        //                employeeData.totalSorties = pointages.Count(p => p.Type == "SORTIE");

        //                // Calculate total working hours
        //                employeeData.heuresTravaillees = CalculateWorkingHoursFromEventsStatic(allEvents, dateFromParsed, dateToParsed);

        //                // Calculate daily hours
        //                foreach (var kvp in dailyAnalysisDict)
        //                {
        //                    kvp.Value.HeuresTravaillees = CalculateDailyWorkingHours(kvp.Value.Events, kvp.Key);
        //                }

        //                employeeData.dailyAnalysis = dailyAnalysisDict.Values.OrderBy(d => DateTime.Parse(d.Date)).ToList();
        //                conn.Close();
        //            }
        //        }

        //        return new { success = true, data = employeeData };
        //    }
        //    catch (SqlException sqlEx)
        //    {
        //        return new { success = false, message = "Erreur base de données: " + sqlEx.Message };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new { success = false, message = "Erreur serveur: " + ex.Message + " - " + ex.GetType().Name };
        //    }
        //}




        protected void btnThisWeek_Click(object sender, EventArgs e)
        {
            DateTime today = DateTime.Now;
            DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);

            txtDateFrom.Text = startOfWeek.ToString("yyyy-MM-dd");
            txtDateTo.Text = today.ToString("yyyy-MM-dd");
            LoadPointageData();
        }

        protected void btnThisMonth_Click(object sender, EventArgs e)
        {
            DateTime today = DateTime.Now;
            DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            txtDateFrom.Text = firstDayOfMonth.ToString("yyyy-MM-dd");
            txtDateTo.Text = today.ToString("yyyy-MM-dd");
            LoadPointageData();
        }

        private bool ValidateDates()
        {
            DateTime dateFrom, dateTo;

            if (!DateTime.TryParse(txtDateFrom.Text, out dateFrom))
            {
                ShowError("Date de début invalide");
                return false;
            }

            if (!DateTime.TryParse(txtDateTo.Text, out dateTo))
            {
                ShowError("Date de fin invalide");
                return false;
            }

            if (dateFrom > dateTo)
            {
                ShowError("La date de début doit être antérieure à la date de fin");
                return false;
            }

            if (dateTo > DateTime.Now.Date)
            {
                ShowError("La date de fin ne peut pas être dans le futur");
                return false;
            }

            if ((dateTo - dateFrom).TotalDays > 90)
            {
                ShowError("La période ne peut pas dépasser 3 mois");
                return false;
            }

            return true;
        }

        private void LoadPointageData()
        {
            try
            {
                DateTime dateFrom, dateTo;

                if (!DateTime.TryParse(txtDateFrom.Text, out dateFrom) ||
                    !DateTime.TryParse(txtDateTo.Text, out dateTo))
                {
                    ShowError("Format de date invalide");
                    return;
                }

                string selectedPort = ddlPort.SelectedValue;
                DataTable dt = GetPointageData(dateFrom, dateTo, selectedPort);

                if (dt.Rows.Count > 0)
                {
                    gvPointageReport.DataSource = dt;
                    gvPointageReport.DataBind();

                    UpdateSummaryInfo(dt, dateFrom, dateTo, selectedPort);

                    pnlResults.Visible = true;
                    pnlNoResults.Visible = false;
                }
                else
                {
                    pnlResults.Visible = false;
                    pnlNoResults.Visible = true;
                    ShowError("Aucun employé trouvé pour les critères sélectionnés");
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des données: " + ex.Message);
            }
        }

        private DataTable GetPointageData(DateTime dateFrom, DateTime dateTo, string port)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                StringBuilder query = new StringBuilder(@"
                    SELECT 
                        e.IdEmploye,
                        e.Matricule,
                        e.Nom + ' ' + e.Prenom AS NomComplet,
                        ISNULL(ep.NomPort, 'Aucun port assigné') AS NomPort,
                        ISNULL(dernierPointage.TypePointage, 'SORTIE') AS StatutActuel,
                        CASE 
                            WHEN dernierPointage.DateHeure IS NOT NULL 
                            THEN FORMAT(dernierPointage.DateHeure, 'dd/MM/yyyy HH:mm')
                            ELSE 'Aucun pointage'
                        END AS DernierPointage,
                        ISNULL(pointageCount.NombrePointages, 0) AS NombrePointages,
                        '0h00' AS HeuresTravaillees, -- Will be calculated
                        ISNULL(pointageHistory.HistoriquePointages, 'Aucun pointage sur la période') AS HistoriquePointages
                    FROM EMPLOYE e
                    LEFT JOIN EMPLOYE_PORT ep ON e.IdEmploye = ep.IdEmploye AND ep.is_deleted = 0
                    LEFT JOIN (
                        SELECT 
                            p1.IdEmploye,
                            p1.TypePointage,
                            p1.DateHeure
                        FROM EMPLOYE_POINTAGE p1
                        INNER JOIN (
                            SELECT IdEmploye, MAX(DateHeure) as MaxDate
                            FROM EMPLOYE_POINTAGE 
                            WHERE is_deleted = 0
                            GROUP BY IdEmploye
                        ) p2 ON p1.IdEmploye = p2.IdEmploye AND p1.DateHeure = p2.MaxDate
                        WHERE p1.is_deleted = 0
                    ) dernierPointage ON e.IdEmploye = dernierPointage.IdEmploye
                    LEFT JOIN (
                        SELECT 
                            IdEmploye,
                            COUNT(*) AS NombrePointages
                        FROM EMPLOYE_POINTAGE 
                        WHERE is_deleted = 0 
                        AND CAST(DateHeure AS DATE) BETWEEN @DateFrom AND @DateTo
                        GROUP BY IdEmploye
                    ) pointageCount ON e.IdEmploye = pointageCount.IdEmploye
                    LEFT JOIN (
                        SELECT 
                            IdEmploye,
                            STUFF((
                                SELECT ', ' + FORMAT(DateHeure, 'dd/MM HH:mm') + ' (' + TypePointage + ')'
                                FROM EMPLOYE_POINTAGE p2
                                WHERE p2.IdEmploye = p1.IdEmploye 
                                AND p2.is_deleted = 0 
                                AND CAST(p2.DateHeure AS DATE) BETWEEN @DateFrom AND @DateTo
                                ORDER BY p2.DateHeure
                                FOR XML PATH('')
                            ), 1, 2, '') AS HistoriquePointages
                        FROM (SELECT DISTINCT IdEmploye FROM EMPLOYE_POINTAGE WHERE is_deleted = 0) p1
                    ) pointageHistory ON e.IdEmploye = pointageHistory.IdEmploye
                    WHERE e.is_deleted = 0 AND e.DateSortie IS NULL");

                if (!string.IsNullOrEmpty(port))
                {
                    query.Append(" AND ep.NomPort = @Port");
                }

                query.Append(" ORDER BY e.Nom, e.Prenom");

                using (SqlCommand cmd = new SqlCommand(query.ToString(), conn))
                {
                    cmd.Parameters.AddWithValue("@DateFrom", dateFrom.Date);
                    cmd.Parameters.AddWithValue("@DateTo", dateTo.Date);

                    if (!string.IsNullOrEmpty(port))
                    {
                        cmd.Parameters.AddWithValue("@Port", port);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        // Calculate working hours efficiently for all employees
                        CalculateWorkingHoursForAllEmployees(dt, dateFrom, dateTo);

                        return dt;
                    }
                }
            }
        }

        private void CalculateWorkingHoursForAllEmployees(DataTable dt, DateTime dateFrom, DateTime dateTo)
        {
            if (dt.Rows.Count == 0) return;

            // Get all employee IDs
            var employeeIds = dt.AsEnumerable()
                .Select(row => Convert.ToInt32(row["IdEmploye"]))
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Calculating hours for {employeeIds.Count} employees from {dateFrom:yyyy-MM-dd} to {dateTo:yyyy-MM-dd}");

            // Get all working hours in one query
            var workingHours = GetWorkingHoursForEmployees(employeeIds, dateFrom, dateTo);

            System.Diagnostics.Debug.WriteLine($"Retrieved working hours for {workingHours.Count} employees");

            // Update the DataTable
            foreach (DataRow row in dt.Rows)
            {
                int employeeId = Convert.ToInt32(row["IdEmploye"]);
                string employeeName = row["NomComplet"].ToString();

                if (workingHours.ContainsKey(employeeId))
                {
                    row["HeuresTravaillees"] = workingHours[employeeId];
                    System.Diagnostics.Debug.WriteLine($"Employee {employeeName} (ID: {employeeId}): {workingHours[employeeId]}");
                }
                else
                {
                    row["HeuresTravaillees"] = "0h00";
                    System.Diagnostics.Debug.WriteLine($"Employee {employeeName} (ID: {employeeId}): No data found - set to 0h00");
                }
            }
        }

        private Dictionary<int, string> GetWorkingHoursForEmployees(List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
        {
            var result = new Dictionary<int, string>();

            if (employeeIds == null || employeeIds.Count == 0)
            {
                return result;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Use parameterized query to avoid SQL injection
                var parameters = new List<SqlParameter>();
                var paramNames = new List<string>();

                for (int i = 0; i < employeeIds.Count; i++)
                {
                    string paramName = "@empId" + i;
                    paramNames.Add(paramName);
                    parameters.Add(new SqlParameter(paramName, employeeIds[i]));
                }

                string query = @"
                    SELECT IdEmploye, TypePointage, DateHeure 
                    FROM EMPLOYE_POINTAGE 
                    WHERE IdEmploye IN (" + string.Join(",", paramNames) + @")
                    AND CAST(DateHeure AS DATE) BETWEEN @DateFrom AND @DateTo
                    AND is_deleted = 0
                    ORDER BY IdEmploye, DateHeure";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                    cmd.Parameters.AddWithValue("@DateFrom", dateFrom.Date);
                    cmd.Parameters.AddWithValue("@DateTo", dateTo.Date);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var employeeEvents = new Dictionary<int, List<(DateTime dateHeure, string type)>>();

                        while (reader.Read())
                        {
                            int employeeId = Convert.ToInt32(reader["IdEmploye"]);
                            DateTime dateHeure = Convert.ToDateTime(reader["DateHeure"]);
                            string type = reader["TypePointage"].ToString().Trim().ToUpper();

                            if (!employeeEvents.ContainsKey(employeeId))
                            {
                                employeeEvents[employeeId] = new List<(DateTime, string)>();
                            }

                            employeeEvents[employeeId].Add((dateHeure, type));
                        }

                        // Calculate working hours for each employee
                        foreach (var employeeId in employeeIds)
                        {
                            if (employeeEvents.ContainsKey(employeeId) && employeeEvents[employeeId].Count > 0)
                            {
                                result[employeeId] = CalculateWorkingHoursFromEvents(employeeEvents[employeeId], dateFrom, dateTo);
                            }
                            else
                            {
                                result[employeeId] = "0h00";
                            }
                        }
                    }
                }
            }

            return result;
        }

        private string CalculateWorkingHoursFromEvents(List<(DateTime dateHeure, string type)> events, DateTime dateFrom, DateTime dateTo)
        {
            if (events == null || events.Count == 0) return "0h00";

            // Sort events by date to ensure proper chronological order
            events = events.OrderBy(e => e.dateHeure).ToList();

            TimeSpan totalHours = TimeSpan.Zero;
            DateTime? currentEntryTime = null;

            // Debug: Log the events for troubleshooting
            System.Diagnostics.Debug.WriteLine($"Processing {events.Count} events:");
            foreach (var evt in events)
            {
                System.Diagnostics.Debug.WriteLine($"  {evt.dateHeure:yyyy-MM-dd HH:mm:ss} - {evt.type}");
            }

            foreach (var eventItem in events)
            {
                string eventType = eventItem.type.Trim().ToUpper();

                if (eventType == "ENTREE")
                {
                    // If there's already an unclosed entry, we may have missed a SORTIE
                    // For now, just update to the new entry time
                    currentEntryTime = eventItem.dateHeure;
                    System.Diagnostics.Debug.WriteLine($"  ENTRY logged at: {eventItem.dateHeure}");
                }
                else if (eventType == "SORTIE" && currentEntryTime.HasValue)
                {
                    TimeSpan workPeriod = eventItem.dateHeure - currentEntryTime.Value;

                    System.Diagnostics.Debug.WriteLine($"  EXIT logged at: {eventItem.dateHeure}, Work period: {workPeriod.TotalHours:F2} hours");

                    // Only add positive work periods (prevent negative times from data issues)
                    if (workPeriod.TotalMinutes > 0 && workPeriod.TotalHours <= 24)
                    {
                        totalHours = totalHours.Add(workPeriod);
                        System.Diagnostics.Debug.WriteLine($"  Added {workPeriod.TotalHours:F2} hours. Total now: {totalHours.TotalHours:F2}");
                    }
                    currentEntryTime = null;
                }
                else if (eventType == "SORTIE" && !currentEntryTime.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"  SORTIE without ENTREE at: {eventItem.dateHeure}");
                }
            }

            // Handle unclosed entry (employee still at work)
            if (currentEntryTime.HasValue)
            {
                System.Diagnostics.Debug.WriteLine($"  Unclosed entry detected at: {currentEntryTime.Value}");

                // Only calculate remaining time if the entry is today or before
                if (currentEntryTime.Value.Date <= DateTime.Now.Date)
                {
                    DateTime endTime;

                    // If entry is from a previous day, use end of that day
                    if (currentEntryTime.Value.Date < DateTime.Now.Date)
                    {
                        endTime = currentEntryTime.Value.Date.AddHours(18); // Assume 6 PM end of workday
                    }
                    else
                    {
                        endTime = DateTime.Now;
                    }

                    TimeSpan remainingTime = endTime - currentEntryTime.Value;

                    System.Diagnostics.Debug.WriteLine($"  Remaining time calculated: {remainingTime.TotalHours:F2} hours");

                    // Only add reasonable work time (max 16 hours per day)
                    if (remainingTime.TotalMinutes > 0 && remainingTime.TotalHours <= 16)
                    {
                        totalHours = totalHours.Add(remainingTime);
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"  Final total hours: {totalHours.TotalHours:F2}");

            int hours = (int)totalHours.TotalHours;
            int minutes = totalHours.Minutes;
            string result = $"{hours}h{minutes:00}";

            System.Diagnostics.Debug.WriteLine($"  Formatted result: {result}");
            return result;
        }

        private void UpdateSummaryInfo(DataTable dt, DateTime dateFrom, DateTime dateTo, string port)
        {
            litRecordCount.Text = dt.Rows.Count.ToString();
            litPeriode.Text = $"{dateFrom:dd/MM/yyyy} - {dateTo:dd/MM/yyyy}";
            litPortSelected.Text = string.IsNullOrEmpty(port) ? "Tous les Ports" : port;

            int employeesPresent = dt.AsEnumerable().Count(row => row.Field<string>("StatutActuel") == "ENTREE");
            int employeesAbsent = dt.AsEnumerable().Count(row => row.Field<string>("StatutActuel") == "SORTIE");

            litTotalEntrees.Text = employeesPresent.ToString();
            litTotalSorties.Text = employeesAbsent.ToString();
        }

        protected string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text == "Aucun pointage sur la période")
                return "Aucun pointage";

            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dateFrom, dateTo;

                if (!DateTime.TryParse(txtDateFrom.Text, out dateFrom) ||
                    !DateTime.TryParse(txtDateTo.Text, out dateTo))
                {
                    ShowError("Format de date invalide");
                    return;
                }

                string selectedPort = ddlPort.SelectedValue;
                DataTable dt = GetPointageData(dateFrom, dateTo, selectedPort);

                if (dt.Rows.Count == 0)
                {
                    ShowError("Aucune donnée à exporter");
                    return;
                }

                ExportToExcel(dt, dateFrom, dateTo, selectedPort);
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de l'export: " + ex.Message);
            }
        }

        private void ExportToExcel(DataTable dt, DateTime dateFrom, DateTime dateTo, string port)
        {
            // Get all daily data in one efficient query
            var dailyData = GetDailyDataForAllEmployees(dt, dateFrom, dateTo);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Rapport Pointage");

                // Generate date range
                List<DateTime> dateRange = new List<DateTime>();
                for (DateTime date = dateFrom; date <= dateTo; date = date.AddDays(1))
                {
                    dateRange.Add(date);
                }

                // Calculate total columns
                int totalColumns = 3 + (dateRange.Count * 2) + 1;

                // Title
                worksheet.Cell(1, 1).Value = "RAPPORT EMPLOYÉS PAR PORT - DÉTAIL JOURNALIER";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Range(1, 1, 1, totalColumns).Merge();

                // Report info
                worksheet.Cell(3, 1).Value = "Période:";
                worksheet.Cell(3, 2).Value = $"{dateFrom:dd/MM/yyyy} - {dateTo:dd/MM/yyyy}";
                worksheet.Cell(4, 1).Value = "Port:";
                worksheet.Cell(4, 2).Value = string.IsNullOrEmpty(port) ? "Tous les Ports" : port;
                worksheet.Cell(5, 1).Value = "Date d'export:";
                worksheet.Cell(5, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                // Headers
                int headerRow = 7;
                int colIndex = 1;

                worksheet.Cell(headerRow, colIndex++).Value = "Matricule";
                worksheet.Cell(headerRow, colIndex++).Value = "Employé";
                worksheet.Cell(headerRow, colIndex++).Value = "Port Assigné";

                // Date headers
                foreach (DateTime date in dateRange)
                {
                    string dateHeader = date.ToString("dd/MM/yyyy");
                    worksheet.Cell(headerRow - 1, colIndex).Value = dateHeader;
                    worksheet.Cell(headerRow - 1, colIndex).Style.Font.Bold = true;
                    worksheet.Cell(headerRow - 1, colIndex).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Range(headerRow - 1, colIndex, headerRow - 1, colIndex + 1).Merge();
                    worksheet.Cell(headerRow - 1, colIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(headerRow, colIndex++).Value = "Heures";
                    worksheet.Cell(headerRow, colIndex++).Value = "Statut";
                }

                worksheet.Cell(headerRow, colIndex).Value = "Total Heures";

                // Style headers
                for (int i = 1; i <= colIndex; i++)
                {
                    worksheet.Cell(headerRow, i).Style.Font.Bold = true;
                    worksheet.Cell(headerRow, i).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(headerRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(headerRow, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Data rows
                int currentRow = headerRow + 1;
                foreach (DataRow row in dt.Rows)
                {
                    colIndex = 1;
                    int employeeId = Convert.ToInt32(row["IdEmploye"]);

                    worksheet.Cell(currentRow, colIndex++).Value = row["Matricule"].ToString();
                    worksheet.Cell(currentRow, colIndex++).Value = row["NomComplet"].ToString();
                    worksheet.Cell(currentRow, colIndex++).Value = row["NomPort"].ToString();

                    TimeSpan totalPeriodHours = TimeSpan.Zero;

                    foreach (DateTime date in dateRange)
                    {
                        var dayData = GetDailyDataForEmployee(dailyData, employeeId, date);

                        // Hours
                        worksheet.Cell(currentRow, colIndex).Value = dayData.Hours;
                        if (dayData.TotalMinutes > 0)
                        {
                            worksheet.Cell(currentRow, colIndex).Style.Font.Bold = true;
                            worksheet.Cell(currentRow, colIndex).Style.Font.FontColor = XLColor.Blue;
                        }
                        colIndex++;

                        // Status
                        worksheet.Cell(currentRow, colIndex).Value = dayData.Status;

                        // Apply color from StatusColor property
                        try
                        {
                            var bgColor = XLColor.FromHtml(dayData.StatusColor);
                            worksheet.Cell(currentRow, colIndex).Style.Fill.BackgroundColor = bgColor;

                            // Set text color based on status type
                            if (dayData.Status == "PRÉSENT")
                            {
                                worksheet.Cell(currentRow, colIndex).Style.Font.FontColor = XLColor.DarkGreen;
                            }
                            else if (dayData.Status == "ABSENT")
                            {
                                // Keep old ABSENT style: light pink background with dark red text
                                worksheet.Cell(currentRow, colIndex).Style.Font.FontColor = XLColor.DarkRed;
                            }
                            else
                            {
                                // For holidays and absence permissions, use white text on colored background
                                worksheet.Cell(currentRow, colIndex).Style.Font.FontColor = XLColor.White;
                            }

                            worksheet.Cell(currentRow, colIndex).Style.Font.Bold = true;
                        }
                        catch
                        {
                            // Fallback to default colors if color parsing fails
                            worksheet.Cell(currentRow, colIndex).Style.Fill.BackgroundColor = XLColor.LightGray;
                            worksheet.Cell(currentRow, colIndex).Style.Font.FontColor = XLColor.Black;
                        }

                        colIndex++;

                        totalPeriodHours = totalPeriodHours.Add(TimeSpan.FromMinutes(dayData.TotalMinutes));
                    }

                    // Total hours
                    int totalHours = (int)totalPeriodHours.TotalHours;
                    int totalMinutes = totalPeriodHours.Minutes;
                    worksheet.Cell(currentRow, colIndex).Value = $"{totalHours}h{totalMinutes:00}";
                    worksheet.Cell(currentRow, colIndex).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, colIndex).Style.Font.FontColor = XLColor.DarkBlue;
                    worksheet.Cell(currentRow, colIndex).Style.Fill.BackgroundColor = XLColor.LightCyan;

                    // Borders
                    for (int i = 1; i <= colIndex; i++)
                    {
                        worksheet.Cell(currentRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(currentRow, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    currentRow++;
                }

                worksheet.ColumnsUsed().AdjustToContents();

                // Summary section
                AddSummarySection(worksheet, currentRow + 2, dateRange, dailyData);

                // Download
                string fileName = $"Rapport_Employes_Port_Journalier_{dateFrom:yyyyMMdd}_{dateTo:yyyyMMdd}.xlsx";

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment;filename={fileName}");

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    Response.BinaryWrite(stream.ToArray());
                }

                Response.Flush();
                Response.End();
            }
        }
        // Helper classes
        public class DailyWorkData
        {
            public string Hours { get; set; } = "0h00";
            public string Status { get; set; } = "ABSENT";
            public int TotalMinutes { get; set; } = 0;
            public string StatusColor { get; set; } = "#ffcdd2"; // Light pink for ABSENT
        }

        // Efficient method to get all daily data in one query
        private Dictionary<(int employeeId, DateTime date), DailyWorkData> GetDailyDataForAllEmployees(DataTable dt, DateTime dateFrom, DateTime dateTo)
        {
            var result = new Dictionary<(int, DateTime), DailyWorkData>();

            if (dt.Rows.Count == 0) return result;

            var employeeIds = dt.AsEnumerable().Select(row => Convert.ToInt32(row["IdEmploye"])).ToList();

            // Load holidays for the date range
            var holidays = new Dictionary<DateTime, string>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string holidayQuery = @"
                    SELECT DateJour, Nom
                    FROM JOURS_FERIES
                    WHERE CAST(DateJour AS DATE) BETWEEN @DateFrom AND @DateTo
                    AND is_deleted = 0";

                using (SqlCommand cmd = new SqlCommand(holidayQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@DateFrom", dateFrom.Date);
                    cmd.Parameters.AddWithValue("@DateTo", dateTo.Date);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime dateJour = Convert.ToDateTime(reader["DateJour"]).Date;
                            string nom = reader["Nom"].ToString();
                            holidays[dateJour] = nom;
                        }
                    }
                }
            }

            // Load absences for all employees in the date range with colors
            var absences = new Dictionary<(int, DateTime), string>();
            var absenceColors = new Dictionary<(int, DateTime), string>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string absenceQuery = @"
                    SELECT ae.IdEmploye, ae.DateDebut, ae.DateFin, m.NomMotif, m.CouleurBadge
                    FROM ABSENCE_EMPLOYE ae
                    INNER JOIN MOTIF_ABSENCE m ON ae.IdMotif = m.IdMotif
                    WHERE ae.IdEmploye IN (" + string.Join(",", employeeIds) + @")
                    AND ae.is_deleted = 0
                    AND m.is_deleted = 0
                    AND (
                        (ae.DateDebut BETWEEN @DateFrom AND @DateTo) OR
                        (ae.DateFin BETWEEN @DateFrom AND @DateTo) OR
                        (ae.DateDebut <= @DateFrom AND ae.DateFin >= @DateTo)
                    )";

                using (SqlCommand cmd = new SqlCommand(absenceQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@DateFrom", dateFrom.Date);
                    cmd.Parameters.AddWithValue("@DateTo", dateTo.Date);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int employeeId = Convert.ToInt32(reader["IdEmploye"]);
                            DateTime dateDebut = Convert.ToDateTime(reader["DateDebut"]).Date;
                            DateTime dateFin = Convert.ToDateTime(reader["DateFin"]).Date;
                            string nomMotif = reader["NomMotif"].ToString();
                            string couleurBadge = reader["CouleurBadge"].ToString();

                            // Mark all dates in the absence range
                            for (DateTime d = dateDebut; d <= dateFin; d = d.AddDays(1))
                            {
                                if (d >= dateFrom.Date && d <= dateTo.Date)
                                {
                                    absences[(employeeId, d)] = nomMotif;
                                    absenceColors[(employeeId, d)] = couleurBadge;
                                }
                            }
                        }
                    }
                }
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT IdEmploye, TypePointage, DateHeure
                    FROM EMPLOYE_POINTAGE
                    WHERE IdEmploye IN (" + string.Join(",", employeeIds) + @")
                    AND CAST(DateHeure AS DATE) BETWEEN @DateFrom AND @DateTo
                    AND is_deleted = 0
                    ORDER BY IdEmploye, DateHeure";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DateFrom", dateFrom.Date);
                    cmd.Parameters.AddWithValue("@DateTo", dateTo.Date);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var employeeDailyEvents = new Dictionary<(int, DateTime), List<(DateTime, string)>>();

                        while (reader.Read())
                        {
                            int employeeId = Convert.ToInt32(reader["IdEmploye"]);
                            DateTime dateHeure = Convert.ToDateTime(reader["DateHeure"]);
                            DateTime date = dateHeure.Date;
                            string type = reader["TypePointage"].ToString();

                            var key = (employeeId, date);
                            if (!employeeDailyEvents.ContainsKey(key))
                            {
                                employeeDailyEvents[key] = new List<(DateTime, string)>();
                            }
                            employeeDailyEvents[key].Add((dateHeure, type));
                        }

                        // Calculate daily hours for each employee and date
                        for (DateTime date = dateFrom; date <= dateTo; date = date.AddDays(1))
                        {
                            foreach (int employeeId in employeeIds)
                            {
                                var key = (employeeId, date);
                                if (employeeDailyEvents.ContainsKey(key))
                                {
                                    result[key] = CalculateDailyHours(employeeDailyEvents[key], date, employeeId, holidays, absences, absenceColors);
                                }
                                else
                                {
                                    // No pointage events - check if it's a holiday or absence
                                    DailyWorkData dailyData = new DailyWorkData();

                                    if (holidays.ContainsKey(date))
                                    {
                                        dailyData.Status = holidays[date];
                                        dailyData.StatusColor = "#FFA500"; // Orange for holidays
                                    }
                                    else if (absences.ContainsKey((employeeId, date)))
                                    {
                                        dailyData.Status = absences[(employeeId, date)];
                                        dailyData.StatusColor = absenceColors.ContainsKey((employeeId, date))
                                            ? absenceColors[(employeeId, date)]
                                            : "#6c757d"; // Default gray
                                    }

                                    result[key] = dailyData;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private DailyWorkData CalculateDailyHours(List<(DateTime dateHeure, string type)> events, DateTime date, int employeeId, Dictionary<DateTime, string> holidays, Dictionary<(int, DateTime), string> absences, Dictionary<(int, DateTime), string> absenceColors)
        {
            if (events.Count == 0)
                return new DailyWorkData();

            // Sort events by time to ensure proper chronological order
            events = events.OrderBy(e => e.dateHeure).ToList();

            TimeSpan dailyHours = TimeSpan.Zero;
            DateTime? currentEntryTime = null;

            foreach (var eventItem in events)
            {
                if (eventItem.type == "ENTREE")
                {
                    // If there's already an unclosed entry, don't add time between entries
                    // Just update to the new entry time
                    currentEntryTime = eventItem.dateHeure;
                }
                else if (eventItem.type == "SORTIE" && currentEntryTime.HasValue)
                {
                    TimeSpan workPeriod = eventItem.dateHeure - currentEntryTime.Value;

                    // Only add positive work periods and reasonable daily hours (max 16 hours)
                    if (workPeriod.TotalMinutes > 0 && workPeriod.TotalHours <= 16)
                    {
                        dailyHours = dailyHours.Add(workPeriod);
                    }
                    currentEntryTime = null;
                }
            }

            // Handle unclosed entry
            if (currentEntryTime.HasValue)
            {
                DateTime endTime = (date.Date == DateTime.Now.Date) ?
                    DateTime.Now : date.Date.AddHours(23).AddMinutes(59);

                TimeSpan remainingTime = endTime - currentEntryTime.Value;

                // Only add reasonable remaining time (max 16 hours per day)
                if (remainingTime.TotalMinutes > 0 && remainingTime.TotalHours <= 16)
                {
                    dailyHours = dailyHours.Add(remainingTime);
                }
            }

            // Cap daily hours at 16 hours maximum
            if (dailyHours.TotalHours > 16)
            {
                dailyHours = TimeSpan.FromHours(16);
            }

            int hours = (int)dailyHours.TotalHours;
            int minutes = dailyHours.Minutes;

            // Determine status and color: check holiday first, then absence, then present/absent
            string status = "PRÉSENT";
            string statusColor = "#c8e6c9"; // Light green for PRÉSENT

            if (events.Count == 0)
            {
                if (holidays.ContainsKey(date))
                {
                    status = holidays[date];
                    statusColor = "#FFA500"; // Orange for holidays
                }
                else if (absences.ContainsKey((employeeId, date)))
                {
                    status = absences[(employeeId, date)];
                    statusColor = absenceColors.ContainsKey((employeeId, date))
                        ? absenceColors[(employeeId, date)]
                        : "#6c757d"; // Default gray
                }
                else
                {
                    status = "ABSENT";
                    statusColor = "#ffcdd2"; // Light pink for ABSENT
                }
            }

            return new DailyWorkData
            {
                Hours = $"{hours}h{minutes:00}",
                Status = status,
                TotalMinutes = (int)dailyHours.TotalMinutes,
                StatusColor = statusColor
            };
        }

        private DailyWorkData GetDailyDataForEmployee(Dictionary<(int, DateTime), DailyWorkData> dailyData, int employeeId, DateTime date)
        {
            var key = (employeeId, date);
            return dailyData.ContainsKey(key) ? dailyData[key] : new DailyWorkData();
        }

        private void AddSummarySection(IXLWorksheet worksheet, int startRow, List<DateTime> dateRange, Dictionary<(int, DateTime), DailyWorkData> dailyData)
        {
            worksheet.Cell(startRow, 1).Value = "RÉSUMÉ PAR JOUR:";
            worksheet.Cell(startRow, 1).Style.Font.Bold = true;
            worksheet.Cell(startRow, 1).Style.Font.FontSize = 14;

            startRow++;
            worksheet.Cell(startRow, 1).Value = "Date";
            worksheet.Cell(startRow, 2).Value = "Employés Présents";
            worksheet.Cell(startRow, 3).Value = "Employés Absents";
            worksheet.Cell(startRow, 4).Value = "Total Heures Travaillées";

            for (int i = 1; i <= 4; i++)
            {
                worksheet.Cell(startRow, i).Style.Font.Bold = true;
                worksheet.Cell(startRow, i).Style.Fill.BackgroundColor = XLColor.DarkGray;
                worksheet.Cell(startRow, i).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(startRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            startRow++;

            foreach (DateTime date in dateRange)
            {
                var dayStats = CalculateDailySummary(dailyData, date);

                worksheet.Cell(startRow, 1).Value = date.ToString("dd/MM/yyyy");
                worksheet.Cell(startRow, 2).Value = dayStats.PresentCount;
                worksheet.Cell(startRow, 3).Value = dayStats.AbsentCount;
                worksheet.Cell(startRow, 4).Value = dayStats.TotalHours;

                for (int i = 1; i <= 4; i++)
                {
                    worksheet.Cell(startRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(startRow, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                startRow++;
            }
        }

        private (int PresentCount, int AbsentCount, string TotalHours) CalculateDailySummary(Dictionary<(int, DateTime), DailyWorkData> dailyData, DateTime date)
        {
            int presentCount = 0;
            int absentCount = 0;
            TimeSpan totalHours = TimeSpan.Zero;

            foreach (var kvp in dailyData.Where(x => x.Key.Item2 == date))
            {
                if (kvp.Value.Status == "PRÉSENT")
                {
                    presentCount++;
                    totalHours = totalHours.Add(TimeSpan.FromMinutes(kvp.Value.TotalMinutes));
                }
                else
                {
                    absentCount++;
                }
            }

            int hours = (int)totalHours.TotalHours;
            int minutes = totalHours.Minutes;

            return (presentCount, absentCount, $"{hours}h{minutes:00}");
        }

        private void ShowError(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Error",
                $"alert('{message.Replace("'", "\\'")}');", true);
        }

        private void ShowSuccess(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Success",
                $"alert('{message.Replace("'", "\\'")}');", true);
        }


        [WebMethod]

        private static string CalculateWorkingHoursFromEventsStatic(List<PointageEvent> events, DateTime dateFrom, DateTime dateTo)
        {
            if (events.Count == 0) return "0h00";

            // Sort events by date to ensure proper chronological order
            events = events.OrderBy(e => e.DateHeure).ToList();

            TimeSpan totalHours = TimeSpan.Zero;
            DateTime? currentEntryTime = null;

            foreach (var eventItem in events)
            {
                if (eventItem.Type == "ENTREE")
                {
                    // If there's already an unclosed entry, don't add time between entries
                    // Just update to the new entry time
                    currentEntryTime = eventItem.DateHeure;
                }
                else if (eventItem.Type == "SORTIE" && currentEntryTime.HasValue)
                {
                    TimeSpan workPeriod = eventItem.DateHeure - currentEntryTime.Value;

                    // Only add positive work periods (prevent negative times from data issues)
                    if (workPeriod.TotalMinutes > 0 && workPeriod.TotalHours <= 24)
                    {
                        totalHours = totalHours.Add(workPeriod);
                    }
                    currentEntryTime = null;
                }
            }

            // Handle unclosed entry (employee still at work)
            if (currentEntryTime.HasValue)
            {
                // Only calculate remaining time if the entry is today or earlier
                if (currentEntryTime.Value.Date <= DateTime.Now.Date)
                {
                    DateTime endTime = DateTime.Now;

                    // If entry is from a previous day, use end of that day
                    if (currentEntryTime.Value.Date < DateTime.Now.Date)
                    {
                        endTime = currentEntryTime.Value.Date.AddHours(23).AddMinutes(59);
                    }

                    TimeSpan remainingTime = endTime - currentEntryTime.Value;

                    // Only add reasonable work time (max 16 hours per day)
                    if (remainingTime.TotalMinutes > 0 && remainingTime.TotalHours <= 16)
                    {
                        totalHours = totalHours.Add(remainingTime);
                    }
                }
            }

            // Cap maximum hours at reasonable limit
            double maxHours = (dateTo - dateFrom).TotalDays * 8; // 8 hours max per day
            if (totalHours.TotalHours > maxHours)
            {
                totalHours = TimeSpan.FromHours(Math.Min(totalHours.TotalHours, maxHours));
            }

            int hours = (int)totalHours.TotalHours;
            int minutes = totalHours.Minutes;
            return $"{hours}h{minutes:00}";
        }
        private static string CalculateDailyWorkingHours(List<PointageEvent> events, DateTime date)
        {
            if (events.Count == 0) return "0h00";

            // Sort events by time to ensure proper chronological order
            events = events.OrderBy(e => e.DateHeure).ToList();

            TimeSpan dailyHours = TimeSpan.Zero;
            DateTime? currentEntryTime = null;

            foreach (var eventItem in events)
            {
                if (eventItem.Type == "ENTREE")
                {
                    // If there's already an unclosed entry, don't add time between entries
                    // Just update to the new entry time
                    currentEntryTime = eventItem.DateHeure;
                }
                else if (eventItem.Type == "SORTIE" && currentEntryTime.HasValue)
                {
                    TimeSpan workPeriod = eventItem.DateHeure - currentEntryTime.Value;

                    // Only add positive work periods and reasonable daily hours (max 16 hours)
                    if (workPeriod.TotalMinutes > 0 && workPeriod.TotalHours <= 16)
                    {
                        dailyHours = dailyHours.Add(workPeriod);
                    }
                    currentEntryTime = null;
                }
            }

            // Handle unclosed entry for the day
            if (currentEntryTime.HasValue)
            {
                DateTime endTime = (date.Date == DateTime.Now.Date) ?
                    DateTime.Now : date.Date.AddHours(23).AddMinutes(59);

                TimeSpan remainingTime = endTime - currentEntryTime.Value;

                // Only add reasonable remaining time (max 16 hours per day)
                if (remainingTime.TotalMinutes > 0 && remainingTime.TotalHours <= 16)
                {
                    dailyHours = dailyHours.Add(remainingTime);
                }
            }

            // Cap daily hours at 16 hours maximum
            if (dailyHours.TotalHours > 16)
            {
                dailyHours = TimeSpan.FromHours(16);
            }

            int hours = (int)dailyHours.TotalHours;
            int minutes = dailyHours.Minutes;
            return $"{hours}h{minutes:00}";
        }

        // Helper classes for print data
        public class EmployeePrintData
        {
            public string portAssgine { get; set; }
            public int totalPointages { get; set; }
            public int totalEntrees { get; set; }
            public int totalSorties { get; set; }
            public string heuresTravaillees { get; set; }
            public List<PointageDetail> pointages { get; set; }
            public List<DailyAnalysis> dailyAnalysis { get; set; }
        }

        public class PointageDetail
        {
            public string DateFormatted { get; set; }
            public string HeureFormatted { get; set; }
            public string Type { get; set; }
            public string Port { get; set; }
        }

        public class DailyAnalysis
        {
            public string Date { get; set; }
            public string PremiereEntree { get; set; }
            public string DerniereSortie { get; set; }
            public string HeuresTravaillees { get; set; }
            public int NombrePointages { get; set; }
            public List<PointageEvent> Events { get; set; }
        }

        public class PointageEvent
        {
            public DateTime DateHeure { get; set; }
            public string Type { get; set; }
        }

        // Additional classes for direct printing
        public class EmployeePrintDataDirect
        {
            public string Matricule { get; set; }
            public string NomComplet { get; set; }
            public string PortAssigne { get; set; }
            public int TotalPointages { get; set; }
            public int TotalEntrees { get; set; }
            public int TotalSorties { get; set; }
            public string HeuresTravaillees { get; set; }
            public List<PointageDetailDirect> Pointages { get; set; }
            public List<DailyAnalysisDirect> DailyAnalysis { get; set; }
        }

        public class PointageDetailDirect
        {
            public string DateFormatted { get; set; }
            public string HeureFormatted { get; set; }
            public string Type { get; set; }
            public string Port { get; set; }
        }

        public class DailyAnalysisDirect
        {
            public string Date { get; set; }
            public string PremiereEntree { get; set; }
            public string DerniereSortie { get; set; }
            public string HeuresTravaillees { get; set; }
            public int NombrePointages { get; set; }
            public List<PointageEvent> Events { get; set; }
        }
    }
}


