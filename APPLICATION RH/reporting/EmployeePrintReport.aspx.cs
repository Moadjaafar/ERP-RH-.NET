using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.UI;

namespace APPLICATION_RH.reporting
{
    public partial class EmployeePrintReport : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadPrintData();
            }
        }

        private void LoadPrintData()
        {
            try
            {
                // Show loading
                pnlLoading.Visible = true;
                pnlError.Visible = false;
                pnlPrintContent.Visible = false;

                // Get parameters from session or URL
                int employeeId;
                DateTime dateFrom, dateTo;
                string selectedPort;

                if (!GetParameters(out employeeId, out dateFrom, out dateTo, out selectedPort))
                {
                    ShowError("Paramètres manquants ou invalides");
                    return;
                }

                // Load employee data
                var employeeData = GetEmployeePrintData(employeeId, dateFrom, dateTo, selectedPort);

                if (employeeData == null)
                {
                    ShowError("Impossible de charger les données de l'employé");
                    return;
                }

                // Populate the page
                PopulatePrintData(employeeData, dateFrom, dateTo, selectedPort);

                // Show content
                pnlLoading.Visible = false;
                pnlPrintContent.Visible = true;
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des données: " + ex.Message);
            }
        }

        private bool GetParameters(out int employeeId, out DateTime dateFrom, out DateTime dateTo, out string selectedPort)
        {
            employeeId = 0;
            dateFrom = DateTime.MinValue;
            dateTo = DateTime.MinValue;
            selectedPort = string.Empty;

            try
            {
                // Try to get from session first
                if (Session["PrintEmployeeId"] != null)
                {
                    employeeId = Convert.ToInt32(Session["PrintEmployeeId"]);
                    selectedPort = Session["PrintSelectedPort"]?.ToString() ?? "";

                    if (!DateTime.TryParse(Session["PrintDateFrom"]?.ToString(), out dateFrom))
                        return false;

                    if (!DateTime.TryParse(Session["PrintDateTo"]?.ToString(), out dateTo))
                        return false;

                    return true;
                }

                // Try to get from URL parameters
                if (Request.QueryString["empId"] != null)
                {
                    if (!int.TryParse(Request.QueryString["empId"], out employeeId))
                        return false;

                    selectedPort = Server.UrlDecode(Request.QueryString["port"] ?? "");

                    if (!DateTime.TryParse(Server.UrlDecode(Request.QueryString["dateFrom"] ?? ""), out dateFrom))
                        return false;

                    if (!DateTime.TryParse(Server.UrlDecode(Request.QueryString["dateTo"] ?? ""), out dateTo))
                        return false;

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private EmployeePrintData GetEmployeePrintData(int employeeId, DateTime dateFrom, DateTime dateTo, string selectedPort)
        {
            var employeeData = new EmployeePrintData();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Get employee basic info
                string empQuery = @"
                    SELECT 
                        e.Matricule,
                        e.Nom + ' ' + e.Prenom AS NomComplet,
                        ISNULL(ep.NomPort, 'Aucun port assigné') AS NomPort
                    FROM EMPLOYE e
                    LEFT JOIN EMPLOYE_PORT ep ON e.IdEmploye = ep.IdEmploye AND ep.is_deleted = 0
                    WHERE e.IdEmploye = @EmployeeId AND e.is_deleted = 0";

                using (SqlCommand empCmd = new SqlCommand(empQuery, conn))
                {
                    empCmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                    conn.Open();

                    using (SqlDataReader reader = empCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            employeeData.Matricule = reader["Matricule"].ToString();
                            employeeData.NomComplet = reader["NomComplet"].ToString();
                            employeeData.PortAssigne = reader["NomPort"].ToString();
                        }
                        else
                        {
                            return null; // Employee not found
                        }
                    }
                    conn.Close();
                }

                // Get detailed pointage data
                string pointageQuery = @"
                    SELECT 
                        TypePointage,
                        DateHeure,
                        ISNULL(ep.NomPort, 'N/A') AS Port
                    FROM EMPLOYE_POINTAGE p
                    LEFT JOIN EMPLOYE_PORT ep ON p.IdEmploye = ep.IdEmploye AND ep.is_deleted = 0
                    WHERE p.IdEmploye = @EmployeeId 
                    AND p.is_deleted = 0 
                    AND CAST(p.DateHeure AS DATE) BETWEEN @DateFrom AND @DateTo";

                if (!string.IsNullOrEmpty(selectedPort))
                {
                    pointageQuery += " AND ep.NomPort = @SelectedPort";
                }

                pointageQuery += " ORDER BY p.DateHeure";

                using (SqlCommand pointageCmd = new SqlCommand(pointageQuery, conn))
                {
                    pointageCmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                    pointageCmd.Parameters.AddWithValue("@DateFrom", dateFrom.Date);
                    pointageCmd.Parameters.AddWithValue("@DateTo", dateTo.Date);

                    if (!string.IsNullOrEmpty(selectedPort))
                    {
                        pointageCmd.Parameters.AddWithValue("@SelectedPort", selectedPort);
                    }

                    conn.Open();
                    var pointages = new List<PointageDetail>();
                    var dailyAnalysisDict = new Dictionary<DateTime, DailyAnalysis>();
                    var allEvents = new List<PointageEvent>();

                    using (SqlDataReader reader = pointageCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime dateHeure = Convert.ToDateTime(reader["DateHeure"]);
                            string type = reader["TypePointage"].ToString();
                            string port = reader["Port"].ToString();

                            pointages.Add(new PointageDetail
                            {
                                DateFormatted = dateHeure.ToString("dd/MM/yyyy"),
                                HeureFormatted = dateHeure.ToString("HH:mm"),
                                Type = type,
                                Port = port
                            });

                            allEvents.Add(new PointageEvent { DateHeure = dateHeure, Type = type });

                            // Initialize daily analysis
                            DateTime date = dateHeure.Date;
                            if (!dailyAnalysisDict.ContainsKey(date))
                            {
                                // Format date with French day name
                                var frenchCulture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
                                string formattedDate = date.ToString("dddd dd/MM/yyyy", frenchCulture);
                                // Capitalize first letter of day name
                                formattedDate = char.ToUpper(formattedDate[0]) + formattedDate.Substring(1);

                                dailyAnalysisDict[date] = new DailyAnalysis
                                {
                                    Date = formattedDate,
                                    PremiereEntree = null,
                                    DerniereSortie = null,
                                    NombrePointages = 0,
                                    Events = new List<PointageEvent>()
                                };
                            }

                            dailyAnalysisDict[date].NombrePointages++;
                            dailyAnalysisDict[date].Events.Add(new PointageEvent { DateHeure = dateHeure, Type = type });

                            if (type == "ENTREE" && dailyAnalysisDict[date].PremiereEntree == null)
                            {
                                dailyAnalysisDict[date].PremiereEntree = dateHeure.ToString("HH:mm");
                            }
                            if (type == "SORTIE")
                            {
                                dailyAnalysisDict[date].DerniereSortie = dateHeure.ToString("HH:mm");
                            }
                        }
                    }

                    employeeData.Pointages = pointages;
                    employeeData.TotalPointages = pointages.Count;
                    employeeData.TotalEntrees = pointages.Count(p => p.Type == "ENTREE");
                    employeeData.TotalSorties = pointages.Count(p => p.Type == "SORTIE");

                    // Calculate total working hours
                    employeeData.HeuresTravaillees = CalculateWorkingHoursFromEvents(allEvents, dateFrom, dateTo);

                    // Calculate daily hours and standard comparison
                    foreach (var kvp in dailyAnalysisDict)
                    {
                        kvp.Value.HeuresTravaillees = CalculateDailyWorkingHours(kvp.Value.Events, kvp.Key);

                        // Calculate standard hours based on day of week
                        kvp.Value.HeuresStandard = GetStandardHours(kvp.Key);

                        // Calculate difference
                        kvp.Value.Difference = CalculateHoursDifference(kvp.Value.HeuresTravaillees, kvp.Value.HeuresStandard);
                    }

                    employeeData.DailyAnalysis = dailyAnalysisDict.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
                    conn.Close();
                }
            }

            return employeeData;
        }

        private void PopulatePrintData(EmployeePrintData employeeData, DateTime dateFrom, DateTime dateTo, string selectedPort)
        {
            string currentDate = DateTime.Now.ToString("dd MMMM yyyy à HH:mm", CultureInfo.GetCultureInfo("fr-FR"));

            // Generation date
            litGenerationDate.Text = currentDate;
            litPrintDate.Text = currentDate;

            // Employee info
            litEmployeeName.Text = employeeData.NomComplet;
            litEmployeeMatricule.Text = employeeData.Matricule;
            litEmployeePort.Text = employeeData.PortAssigne;

            // Period info
            litPeriodFrom.Text = dateFrom.ToString("dd/MM/yyyy");
            litPeriodTo.Text = dateTo.ToString("dd/MM/yyyy");
            litPortFilter.Text = string.IsNullOrEmpty(selectedPort) ? "Tous les ports" : selectedPort;

            // Summary
            litTotalPointages.Text = employeeData.TotalPointages.ToString();
            litTotalEntrees.Text = employeeData.TotalEntrees.ToString();
            litTotalSorties.Text = employeeData.TotalSorties.ToString();
            litTotalHours.Text = employeeData.HeuresTravaillees;

            // Bind grids
            gvPointageDetail.DataSource = employeeData.Pointages;
            gvPointageDetail.DataBind();

            gvDailyAnalysis.DataSource = employeeData.DailyAnalysis;
            gvDailyAnalysis.DataBind();
        }

        private string CalculateWorkingHoursFromEvents(List<PointageEvent> events, DateTime dateFrom, DateTime dateTo)
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

            int hours = (int)totalHours.TotalHours;
            int minutes = totalHours.Minutes;
            return $"{hours}H{minutes:00}MIN";
        }

        private string CalculateDailyWorkingHours(List<PointageEvent> events, DateTime date)
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
            return $"{hours}H{minutes:00}MIN";
        }

        private string GetStandardHours(DateTime date)
        {
            // Get day of week (Sunday = 0, Monday = 1, ..., Saturday = 6)
            DayOfWeek dayOfWeek = date.DayOfWeek;

            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                case DayOfWeek.Friday:
                    return "8H00MIN";
                case DayOfWeek.Saturday:
                    return "3H30MIN";
                case DayOfWeek.Sunday:
                default:
                    return "0H00MIN";
            }
        }

        private string CalculateHoursDifference(string actualHours, string standardHours)
        {
            try
            {
                // Parse actual hours (format: "4H30MIN")
                var actualMinutes = ParseHoursToMinutes(actualHours);
                var standardMinutes = ParseHoursToMinutes(standardHours);

                // Calculate difference in minutes
                int differenceMinutes = actualMinutes - standardMinutes;

                // Convert back to hours and minutes
                bool isPositive = differenceMinutes >= 0;
                differenceMinutes = Math.Abs(differenceMinutes);

                int hours = differenceMinutes / 60;
                int minutes = differenceMinutes % 60;

                string sign = isPositive ? "+" : "-";
                return $"{sign}{hours}H{minutes:00}MIN";
            }
            catch
            {
                return "0H00MIN";
            }
        }

        private int ParseHoursToMinutes(string timeString)
        {
            try
            {
                // Parse format like "4H30MIN" or "0H00MIN"
                if (string.IsNullOrEmpty(timeString))
                    return 0;

                // Extract hours and minutes using regex or string manipulation
                var parts = timeString.Replace("MIN", "").Split('H');
                if (parts.Length == 2)
                {
                    int hours = int.Parse(parts[0]);
                    int minutes = int.Parse(parts[1]);
                    return (hours * 60) + minutes;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        protected string GetDifferenceClass(string difference)
        {
            if (string.IsNullOrEmpty(difference))
                return "text-muted";

            if (difference.StartsWith("+"))
                return "text-success fw-bold"; // Green for overtime
            else if (difference.StartsWith("-"))
                return "text-danger fw-bold";  // Red for under-time
            else
                return "text-muted";           // Gray for exact match
        }

        private void ShowError(string message)
        {
            pnlLoading.Visible = false;
            pnlPrintContent.Visible = false;
            pnlError.Visible = true;
            litError.Text = message;
        }

        // Helper classes
        public class EmployeePrintData
        {
            public string Matricule { get; set; }
            public string NomComplet { get; set; }
            public string PortAssigne { get; set; }
            public int TotalPointages { get; set; }
            public int TotalEntrees { get; set; }
            public int TotalSorties { get; set; }
            public string HeuresTravaillees { get; set; }
            public List<PointageDetail> Pointages { get; set; } = new List<PointageDetail>();
            public List<DailyAnalysis> DailyAnalysis { get; set; } = new List<DailyAnalysis>();
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
            public string HeuresStandard { get; set; }
            public string Difference { get; set; }
            public int NombrePointages { get; set; }
            public List<PointageEvent> Events { get; set; } = new List<PointageEvent>();
        }

        public class PointageEvent
        {
            public DateTime DateHeure { get; set; }
            public string Type { get; set; }
        }
    }
}