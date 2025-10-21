using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClosedXML.Excel;

namespace APPLICATION_RH.reporting
{
    public partial class RaportEmploiyerPointage : System.Web.UI.Page
    {
        #region Constants
        private const int MAX_PERIOD_DAYS = 31;
        private const double MAX_HOURS_PER_DAY = 24.0;
        private const int DEFAULT_END_WORK_HOUR = 18;
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const string DISPLAY_DATE_FORMAT = "dd/MM/yyyy";
        private const string TIME_FORMAT = "HH:mm";
        #endregion

        #region Properties
        protected string EmployeeId => Request.QueryString["id"] ?? "";
        protected string Matricule => Request.QueryString["matricule"] ?? "";
        protected string EmployeeName => HttpUtility.UrlDecode(Request.QueryString["nom"] ?? "");
        protected DateTime DateFrom
        {
            get
            {
                if (DateTime.TryParseExact(Request.QueryString["dateFrom"], DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    return date;
                return DateTime.Today.AddDays(-30);
            }
        }
        protected DateTime DateTo
        {
            get
            {
                if (DateTime.TryParseExact(Request.QueryString["dateTo"], DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    return date;
                return DateTime.Today;
            }
        }
        protected string PortFilter => HttpUtility.UrlDecode(Request.QueryString["port"] ?? "");
        #endregion

        #region Page Events
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    ValidateParameters();
                    LoadEmployeeData();
                    LoadPointageData();
                    LoadDailySummary();
                    SetDateRange();
                }
            }
            catch (Exception ex)
            {
                HandleError($"Erreur lors du chargement de la page: {ex.Message}");
            }
        }

        protected void btnFilterApply_Click(object sender, EventArgs e)
        {
            try
            {
                LoadPointageData();
                LoadDailySummary();
            }
            catch (Exception ex)
            {
                HandleError($"Erreur lors du filtrage: {ex.Message}");
            }
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                ExportToExcel();
            }
            catch (Exception ex)
            {
                HandleError($"Erreur lors de l'exportation: {ex.Message}");
            }
        }

        protected void lnkBack_Click(object sender, EventArgs e)
        {
            try
            {
                string backUrl = "RaportPointagePort.aspx";
                if (!string.IsNullOrEmpty(PortFilter))
                    backUrl += $"?port={HttpUtility.UrlEncode(PortFilter)}";

                Response.Redirect(backUrl, false);
            }
            catch (Exception ex)
            {
                HandleError($"Erreur lors de la navigation: {ex.Message}");
            }
        }
        #endregion

        #region Data Loading Methods
        private void ValidateParameters()
        {
            if (string.IsNullOrEmpty(EmployeeId) || string.IsNullOrEmpty(Matricule))
            {
                throw new ArgumentException("Paramètres employé manquants");
            }

            TimeSpan dateDifference = DateTo - DateFrom;
            if (dateDifference.Days > MAX_PERIOD_DAYS)
            {
                throw new ArgumentException($"La période sélectionnée ne peut pas dépasser {MAX_PERIOD_DAYS} jours");
            }
        }

        private void LoadEmployeeData()
        {
            // Set basic employee info from query string
            var litEmployeeName = FindControl("litEmployeeName") as Literal;
            var litMatricule = FindControl("litMatricule") as Literal;
            var litPortAssigne = FindControl("litPortAssigne") as Literal;
            var litCurrentStatus = FindControl("litCurrentStatus") as Literal;
            var litPeriode = FindControl("litPeriode") as Literal;

            if (litEmployeeName != null) litEmployeeName.Text = EmployeeName;
            if (litMatricule != null) litMatricule.Text = Matricule;
            if (litPortAssigne != null) litPortAssigne.Text = string.IsNullOrEmpty(PortFilter) ? "Tous les ports" : PortFilter;
            if (litCurrentStatus != null) litCurrentStatus.Text = "Actif"; // Default status, could be loaded from database
            if (litPeriode != null) litPeriode.Text = $"{DateFrom:dd/MM/yyyy} - {DateTo:dd/MM/yyyy}";

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT e.IdEmploye, e.Matricule, e.Nom, e.Prenom, e.Email, e.Telephone
                    FROM EMPLOYE e
                    WHERE e.IdEmploye = @IdEmploye AND e.is_deleted = 0";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdEmploye", EmployeeId);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Additional employee information could be populated here
                            // For now, we use the query string values
                        }
                        else
                        {
                            throw new Exception("Employé introuvable");
                        }
                    }
                }
            }
        }

        private void LoadPointageData()
        {
            DateTime filterDateFrom = DateFrom;
            DateTime filterDateTo = DateTo;

            // Get filtered dates if date controls have values
            if (DateTime.TryParse(txtFilterDateFrom.Text, out DateTime fromControl))
                filterDateFrom = fromControl;
            if (DateTime.TryParse(txtFilterDateTo.Text, out DateTime toControl))
                filterDateTo = toControl;

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        p.IdPointage,
                        p.IdEmploye,
                        e.Matricule,
                        e.Nom + ' ' + e.Prenom AS NomComplet,
                        CAST(p.DateHeure AS DATE) AS DatePointage,
                        CAST(p.DateHeure AS TIME) AS HeurePointage,
                        p.TypePointage AS Type,
                        'Actif' AS StatusPointage,
                        FORMAT(p.DateHeure, 'dd/MM/yyyy') AS DateFormatted,
                        FORMAT(p.DateHeure, 'HH:mm:ss') AS HeureFormatted,
                        DATENAME(WEEKDAY, p.DateHeure) AS JourSemaine,
                        ISNULL(p.NomPort, 'N/A') AS Port
                    FROM EMPLOYE_POINTAGE p
                    INNER JOIN EMPLOYE e ON p.IdEmploye = e.IdEmploye AND e.is_deleted = 0
                    WHERE p.IdEmploye = @IdEmploye 
                    AND CAST(p.DateHeure AS DATE) >= @DateFrom 
                    AND CAST(p.DateHeure AS DATE) <= @DateTo
                    AND p.is_deleted = 0
                    ORDER BY p.DateHeure DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdEmploye", EmployeeId);
                    command.Parameters.AddWithValue("@DateFrom", filterDateFrom);
                    command.Parameters.AddWithValue("@DateTo", filterDateTo);

                    connection.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        gvPointageDetails.DataSource = dataTable;
                        gvPointageDetails.DataBind();

                        UpdateStatistics(dataTable);
                    }
                }
            }
        }

        private void LoadDailySummary()
        {
            DateTime filterDateFrom = DateFrom;
            DateTime filterDateTo = DateTo;

            // Get filtered dates if date controls have values
            if (DateTime.TryParse(txtFilterDateFrom.Text, out DateTime fromControl))
                filterDateFrom = fromControl;
            if (DateTime.TryParse(txtFilterDateTo.Text, out DateTime toControl))
                filterDateTo = toControl;

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        CAST(p.DateHeure AS DATE) AS DatePointage,
                        FORMAT(p.DateHeure, 'dd/MM/yyyy') AS DateFormatted,
                        DATENAME(WEEKDAY, p.DateHeure) AS JourSemaine,
                        COUNT(*) AS NombrePointages,
                        SUM(CASE WHEN p.TypePointage = 'ENTREE' THEN 1 ELSE 0 END) AS NombreEntrees,
                        SUM(CASE WHEN p.TypePointage = 'SORTIE' THEN 1 ELSE 0 END) AS NombreSorties,
                        MIN(CASE WHEN p.TypePointage = 'ENTREE' THEN FORMAT(p.DateHeure, 'HH:mm') END) AS PremiereEntree,
                        MAX(CASE WHEN p.TypePointage = 'SORTIE' THEN FORMAT(p.DateHeure, 'HH:mm') END) AS DerniereSortie,
                        MIN(CASE WHEN p.TypePointage = 'ENTREE' THEN CAST(p.DateHeure AS TIME) END) AS PremiereEntreeTime,
                        MAX(CASE WHEN p.TypePointage = 'SORTIE' THEN CAST(p.DateHeure AS TIME) END) AS DerniereSortieTime
                    FROM EMPLOYE_POINTAGE p
                    WHERE p.IdEmploye = @IdEmploye 
                    AND CAST(p.DateHeure AS DATE) >= @DateFrom 
                    AND CAST(p.DateHeure AS DATE) <= @DateTo
                    AND p.is_deleted = 0
                    GROUP BY CAST(p.DateHeure AS DATE), FORMAT(p.DateHeure, 'dd/MM/yyyy'), DATENAME(WEEKDAY, p.DateHeure)
                    ORDER BY CAST(p.DateHeure AS DATE) DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdEmploye", EmployeeId);
                    command.Parameters.AddWithValue("@DateFrom", filterDateFrom);
                    command.Parameters.AddWithValue("@DateTo", filterDateTo);

                    connection.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Add calculated working hours and status
                        dataTable.Columns.Add("HeuresTravaillees", typeof(string));
                        dataTable.Columns.Add("HeuresTravaileesNumeric", typeof(double));
                        dataTable.Columns.Add("StatusText", typeof(string));

                        foreach (DataRow row in dataTable.Rows)
                        {
                            var workingHours = CalculateWorkingHours(row);
                            row["HeuresTravaillees"] = workingHours.Item1;
                            row["HeuresTravaileesNumeric"] = workingHours.Item2;

                            // Set status based on working hours
                            double hours = workingHours.Item2;
                            if (hours >= 8.0)
                                row["StatusText"] = "Journée complète";
                            else if (hours >= 4.0)
                                row["StatusText"] = "Demi-journée";
                            else if (hours > 0)
                                row["StatusText"] = "Partiel";
                            else
                                row["StatusText"] = "Incomplet";
                        }

                        rptDailySummary.DataSource = dataTable;
                        rptDailySummary.DataBind();
                    }
                }
            }
        }

        private void UpdateStatistics(DataTable pointageData)
        {
            int totalPointages = pointageData.Rows.Count;
            int entrees = pointageData.AsEnumerable().Count(row => row.Field<string>("Type") == "ENTREE");
            int sorties = pointageData.AsEnumerable().Count(row => row.Field<string>("Type") == "SORTIE");

            // Calculate total working hours
            var dailyHours = CalculateTotalWorkingHours(pointageData);
            double totalHours = dailyHours.Sum();

            // Update statistics literals
            var litTotalPointages = FindControl("litTotalPointages") as Literal;
            var litTotalEntrees = FindControl("litTotalEntrees") as Literal;
            var litTotalSorties = FindControl("litTotalSorties") as Literal;
            var litHeuresTravaillees = FindControl("litHeuresTravaillees") as Literal;
            var litRecordCount = FindControl("litRecordCount") as Literal;

            if (litTotalPointages != null) litTotalPointages.Text = totalPointages.ToString();
            if (litTotalEntrees != null) litTotalEntrees.Text = entrees.ToString();
            if (litTotalSorties != null) litTotalSorties.Text = sorties.ToString();
            if (litHeuresTravaillees != null) litHeuresTravaillees.Text = $"{totalHours:F1}h";
            if (litRecordCount != null) litRecordCount.Text = totalPointages.ToString();
        }
        #endregion

        #region Working Hours Calculation
        private Tuple<string, double> CalculateWorkingHours(DataRow dailySummaryRow)
        {
            object premiereEntree = dailySummaryRow["PremiereEntreeTime"];
            object derniereSortie = dailySummaryRow["DerniereSortieTime"];

            if (premiereEntree == DBNull.Value || derniereSortie == DBNull.Value)
            {
                return new Tuple<string, double>("N/A", 0.0);
            }

            try
            {
                TimeSpan entree = (TimeSpan)premiereEntree;
                TimeSpan sortie = (TimeSpan)derniereSortie;

                double hoursWorked = (sortie - entree).TotalHours;

                if (hoursWorked < 0 || hoursWorked > MAX_HOURS_PER_DAY)
                {
                    return new Tuple<string, double>("N/A", 0.0);
                }

                return new Tuple<string, double>($"{hoursWorked:F1}h", hoursWorked);
            }
            catch
            {
                return new Tuple<string, double>("N/A", 0.0);
            }
        }

        private List<double> CalculateTotalWorkingHours(DataTable pointageData)
        {
            var dailyHours = new List<double>();

            // Group by date
            var groupedByDate = pointageData.AsEnumerable()
                .GroupBy(row => row.Field<DateTime>("DatePointage").Date);

            foreach (var dateGroup in groupedByDate)
            {
                var dayPointages = dateGroup.OrderBy(row => row.Field<TimeSpan>("HeurePointage")).ToList();

                TimeSpan? firstEntry = null;
                TimeSpan? lastExit = null;

                foreach (var pointage in dayPointages)
                {
                    string type = pointage.Field<string>("Type");
                    TimeSpan time = pointage.Field<TimeSpan>("HeurePointage");

                    if (type == "ENTREE" && firstEntry == null)
                        firstEntry = time;
                    else if (type == "SORTIE")
                        lastExit = time;
                }

                if (firstEntry.HasValue && lastExit.HasValue)
                {
                    double hoursWorked = (lastExit.Value - firstEntry.Value).TotalHours;
                    if (hoursWorked >= 0 && hoursWorked <= MAX_HOURS_PER_DAY)
                        dailyHours.Add(hoursWorked);
                }
            }

            return dailyHours;
        }
        #endregion

        #region Export Functionality
        private void ExportToExcel()
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Rapport Pointage");

                    // Title
                    worksheet.Cell(1, 1).Value = $"Rapport de Pointage - {EmployeeName}";
                    worksheet.Range(1, 1, 1, 8).Merge().Style.Font.SetBold().Font.SetFontSize(16)
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    // Employee info
                    worksheet.Cell(3, 1).Value = "Matricule:";
                    worksheet.Cell(3, 2).Value = Matricule;
                    worksheet.Cell(4, 1).Value = "Période:";
                    worksheet.Cell(4, 2).Value = $"{DateFrom:dd/MM/yyyy} - {DateTo:dd/MM/yyyy}";

                    // Headers
                    int headerRow = 6;
                    worksheet.Cell(headerRow, 1).Value = "Date";
                    worksheet.Cell(headerRow, 2).Value = "Heure";
                    worksheet.Cell(headerRow, 3).Value = "Type";
                    worksheet.Cell(headerRow, 4).Value = "Status";

                    worksheet.Range(headerRow, 1, headerRow, 4).Style.Font.SetBold()
                        .Fill.SetBackgroundColor(XLColor.LightGray)
                        .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                    // Data
                    LoadPointageDataForExport(worksheet, (headerRow + 1));

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Send file
                    using (MemoryStream stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        Response.Clear();
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("Content-Disposition",
                            $"attachment; filename=Pointage_{Matricule}_{DateTime.Now:yyyyMMdd}.xlsx");
                        Response.BinaryWrite(stream.ToArray());
                        Response.End();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'exportation Excel: {ex.Message}");
            }
        }

        private void LoadPointageDataForExport(IXLWorksheet worksheet, int startRowNumber)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        p.DateHeure,
                        FORMAT(p.DateHeure, 'dd/MM/yyyy') AS DatePointage, 
                        FORMAT(p.DateHeure, 'HH:mm:ss') AS HeurePointage, 
                        p.TypePointage, 
                        'Actif' AS StatusPointage,
                        ISNULL(p.NomPort, 'N/A') AS Port
                    FROM EMPLOYE_POINTAGE p
                    WHERE p.IdEmploye = @IdEmploye 
                    AND CAST(p.DateHeure AS DATE) >= @DateFrom 
                    AND CAST(p.DateHeure AS DATE) <= @DateTo
                    AND p.is_deleted = 0
                    ORDER BY p.DateHeure DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdEmploye", EmployeeId);
                    command.Parameters.AddWithValue("@DateFrom", DateFrom);
                    command.Parameters.AddWithValue("@DateTo", DateTo);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int excelRowIndex = startRowNumber;
                        while (reader.Read())
                        {
                            try
                            {
                                var dateValue = Convert.ToDateTime(reader["DatePointage"]).ToString(DISPLAY_DATE_FORMAT);
                                var timeValue = ((TimeSpan)reader["HeurePointage"]).ToString(TIME_FORMAT);
                                var typeValue = reader["TypePointage"]?.ToString() ?? "";
                                var statusValue = reader["StatusPointage"]?.ToString() ?? "";

                                worksheet.Cell($"A{excelRowIndex}").Value = dateValue;
                                worksheet.Cell($"B{excelRowIndex}").Value = timeValue;
                                worksheet.Cell($"C{excelRowIndex}").Value = typeValue;
                                worksheet.Cell($"D{excelRowIndex}").Value = statusValue;
                                excelRowIndex++;
                            }
                            catch (Exception ex)
                            {
                                // Skip problematic rows and continue
                                continue;
                            }
                        }
                    }
                }
            }
        }

        protected void gvPointageDetails_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView row = (DataRowView)e.Row.DataItem;

                // Set duration worked in each row
                Literal litDuree = (Literal)e.Row.FindControl("litDureeTravaillee");
                if (litDuree != null)
                {
                    // Calculate duration for this specific entry (simplified)
                    litDuree.Text = ""; // Will be calculated based on entry/exit pairs
                }

                // Set observations
                Literal litObs = (Literal)e.Row.FindControl("litObservations");
                if (litObs != null)
                {
                    string type = row["Type"]?.ToString();
                    litObs.Text = type == "ENTREE" ? "Arrivée" : "Départ";
                }
            }
        }
        #endregion

        #region Helper Methods
        private void SetDateRange()
        {
            txtFilterDateFrom.Text = DateFrom.ToString(DATE_FORMAT);
            txtFilterDateTo.Text = DateTo.ToString(DATE_FORMAT);
        }

        private void HandleError(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "showError",
                $"alert('{EscapeJavaScriptString(message)}');", true);
        }

        private string EscapeJavaScriptString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Replace("\\", "\\\\")
                       .Replace("'", "\\'")
                       .Replace("\"", "\\\"")
                       .Replace("\r", "\\r")
                       .Replace("\n", "\\n")
                       .Replace("\t", "\\t");
        }

        [System.Web.Services.WebMethod]
        public static List<PointageDetail> GetDayDetails(string employeeId, string date)
        {
            var details = new List<PointageDetail>();

            try
            {
                DateTime targetDate;
                if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate))
                {
                    return details;
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            p.DateHeure,
                            p.TypePointage,
                            FORMAT(p.DateHeure, 'dd/MM/yyyy') AS DateFormatted,
                            FORMAT(p.DateHeure, 'HH:mm:ss') AS HeureFormatted,
                            FORMAT(p.DateHeure, 'dd/MM/yyyy HH:mm:ss') AS DateHeureComplete,
                            ISNULL(p.NomPort, 'N/A') AS Port,
                            DATENAME(WEEKDAY, p.DateHeure) AS JourSemaine
                        FROM EMPLOYE_POINTAGE p
                        WHERE p.IdEmploye = @IdEmploye 
                        AND CAST(p.DateHeure AS DATE) = @TargetDate
                        AND p.is_deleted = 0
                        ORDER BY p.DateHeure";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdEmploye", employeeId);
                        command.Parameters.AddWithValue("@TargetDate", targetDate.Date);

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                details.Add(new PointageDetail
                                {
                                    DateFormatted = reader["DateFormatted"].ToString(),
                                    HeureFormatted = reader["HeureFormatted"].ToString(),
                                    DateHeureComplete = reader["DateHeureComplete"].ToString(),
                                    Type = reader["TypePointage"].ToString(),
                                    Port = reader["Port"].ToString(),
                                    JourSemaine = reader["JourSemaine"].ToString(),
                                    DateHeure = Convert.ToDateTime(reader["DateHeure"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list on error
            }

            return details;
        }

        public class PointageDetail
        {
            public string DateFormatted { get; set; }
            public string HeureFormatted { get; set; }
            public string DateHeureComplete { get; set; }
            public string Type { get; set; }
            public string Port { get; set; }
            public string JourSemaine { get; set; }
            public DateTime DateHeure { get; set; }
        }
        #endregion
    }
}


