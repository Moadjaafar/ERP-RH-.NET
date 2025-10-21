using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH
{
    public partial class _Default : Page
    {
        #region Properties
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        private JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
        #endregion

        #region Page Events
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDashboardData(30); // Default: last 30 days
            }
        }

        protected void FilterButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int days = Convert.ToInt32(btn.CommandArgument);
            LoadDashboardData(days);
        }
        #endregion

        #region Dashboard Data Loading
        private void LoadDashboardData(int periodDays)
        {
            try
            {
                // Get data for the specified period
                string dailyPunchesJson = GetDailyPunchesData(periodDays);
                string portDistributionJson = GetPortDistributionData(periodDays);
                string hourlyDistributionJson = GetHourlyDistributionData(Math.Min(periodDays, 30));
                string employeeActivityJson = GetTopEmployeeActivityData(periodDays);
                string weeklyTrendJson = GetWeeklyTrendData(Math.Min(periodDays / 7 * 2, 24)); // Show proportional weeks
                string punchTypeDistributionJson = GetPunchTypeDistributionData(periodDays);
                string summaryStatsJson = GetSummaryStats(periodDays);

                // Register script to initialize charts
                string script = $@"
                    var dashboardData = {{
                        dailyPunches: {dailyPunchesJson},
                        portDistribution: {portDistributionJson},
                        hourlyDistribution: {hourlyDistributionJson},
                        employeeActivity: {employeeActivityJson},
                        weeklyTrend: {weeklyTrendJson},
                        punchTypeDistribution: {punchTypeDistributionJson},
                        summaryStats: {summaryStatsJson},
                        period: {periodDays}
                    }};
                    
                    window.dashboardDataLoaded = true;
                    
                    // Initialize dashboard when ready
                    if (document.readyState === 'complete') {{
                        setTimeout(initializeDashboard, 100);
                    }}
                ";

                ClientScript.RegisterStartupScript(this.GetType(), "DashboardData", script, true);
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                string errorScript = $@"
                    console.error('Dashboard error: {ex.Message.Replace("'", "\\'")}');
                    var dashboardData = {{
                        dailyPunches: [],
                        portDistribution: [],
                        hourlyDistribution: [],
                        employeeActivity: [],
                        weeklyTrend: [],
                        punchTypeDistribution: [],
                        summaryStats: {{todayPunches: 0, todayActiveEmployees: 0, monthPunches: 0, activePorts: 0}},
                        period: {periodDays}
                    }};
                    window.dashboardDataLoaded = true;
                ";
                ClientScript.RegisterStartupScript(this.GetType(), "DashboardDataError", errorScript, true);
            }
        }
        #endregion

        #region Data Retrieval Methods

        private string GetDailyPunchesData(int days)
        {
            var data = new List<object>();

            try
            {
                string query = $@"
                    SELECT 
                        CAST(DateHeure AS DATE) as PunchDate,
                        COUNT(*) as PunchCount
                    FROM EMPLOYE_POINTAGE 
                    WHERE is_deleted = 0 
                        AND DateHeure >= DATEADD(day, -{days}, GETDATE())
                    GROUP BY CAST(DateHeure AS DATE)
                    ORDER BY PunchDate";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    date = Convert.ToDateTime(reader["PunchDate"]).ToString("yyyy-MM-dd"),
                                    count = Convert.ToInt32(reader["PunchCount"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return empty data on error
            }

            return jsonSerializer.Serialize(data);
        }

        private string GetPortDistributionData(int days)
        {
            var data = new List<object>();

            try
            {
                string query = $@"
                    SELECT 
                        ISNULL(NomPort, 'Unknown') as NomPort,
                        COUNT(*) as PunchCount,
                        COUNT(DISTINCT IdEmploye) as UniqueEmployees
                    FROM EMPLOYE_POINTAGE 
                    WHERE is_deleted = 0 
                        AND DateHeure >= DATEADD(day, -{days}, GETDATE())
                    GROUP BY NomPort
                    ORDER BY PunchCount DESC";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    port = reader["NomPort"].ToString(),
                                    punchCount = Convert.ToInt32(reader["PunchCount"]),
                                    uniqueEmployees = Convert.ToInt32(reader["UniqueEmployees"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return empty data on error
            }

            return jsonSerializer.Serialize(data);
        }

        private string GetHourlyDistributionData(int days)
        {
            var data = new List<object>();

            try
            {
                string query = $@"
                    SELECT 
                        DATEPART(HOUR, DateHeure) as PunchHour,
                        COUNT(*) as PunchCount
                    FROM EMPLOYE_POINTAGE 
                    WHERE is_deleted = 0 
                        AND DateHeure >= DATEADD(day, -{days}, GETDATE())
                    GROUP BY DATEPART(HOUR, DateHeure)
                    ORDER BY PunchHour";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    hour = Convert.ToInt32(reader["PunchHour"]),
                                    count = Convert.ToInt32(reader["PunchCount"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return empty data on error
            }

            return jsonSerializer.Serialize(data);
        }

        private string GetTopEmployeeActivityData(int days)
        {
            var data = new List<object>();

            try
            {
                string query = $@"
                    SELECT TOP 10
                        COALESCE(e.Nom + ' ' + e.Prenom, 'Employee #' + CAST(ep.IdEmploye AS VARCHAR)) as EmployeeName,
                        COALESCE(e.Matricule, 'N/A') as Matricule,
                        COUNT(ep.IdPointage) as PunchCount,
                        COUNT(DISTINCT ep.NomPort) as PortsWorked
                    FROM EMPLOYE_POINTAGE ep
                    LEFT JOIN EMPLOYE e ON ep.IdEmploye = e.IdEmploye AND e.is_deleted = 0
                    WHERE ep.is_deleted = 0 
                        AND ep.DateHeure >= DATEADD(day, -{days}, GETDATE())
                    GROUP BY ep.IdEmploye, e.Nom, e.Prenom, e.Matricule
                    ORDER BY PunchCount DESC";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    employeeName = reader["EmployeeName"].ToString(),
                                    matricule = reader["Matricule"].ToString(),
                                    punchCount = Convert.ToInt32(reader["PunchCount"]),
                                    portsWorked = Convert.ToInt32(reader["PortsWorked"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return empty data on error
            }

            return jsonSerializer.Serialize(data);
        }

        private string GetWeeklyTrendData(int weeks)
        {
            var data = new List<object>();

            try
            {
                string query = $@"
                    SELECT 
                        DATEPART(WEEK, DateHeure) as WeekNumber,
                        YEAR(DateHeure) as Year,
                        COUNT(*) as PunchCount,
                        COUNT(DISTINCT IdEmploye) as ActiveEmployees
                    FROM EMPLOYE_POINTAGE 
                    WHERE is_deleted = 0 
                        AND DateHeure >= DATEADD(week, -{weeks}, GETDATE())
                    GROUP BY DATEPART(WEEK, DateHeure), YEAR(DateHeure)
                    ORDER BY Year, WeekNumber";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    week = $"S{reader["WeekNumber"]}-{reader["Year"]}",
                                    punchCount = Convert.ToInt32(reader["PunchCount"]),
                                    activeEmployees = Convert.ToInt32(reader["ActiveEmployees"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return empty data on error
            }

            return jsonSerializer.Serialize(data);
        }

        private string GetPunchTypeDistributionData(int days)
        {
            var data = new List<object>();

            try
            {
                string query = $@"
                    SELECT 
                        TypePointage,
                        COUNT(*) as Count
                    FROM EMPLOYE_POINTAGE 
                    WHERE is_deleted = 0 
                        AND DateHeure >= DATEADD(day, -{days}, GETDATE())
                    GROUP BY TypePointage";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    type = reader["TypePointage"].ToString(),
                                    count = Convert.ToInt32(reader["Count"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return empty data on error
            }

            return jsonSerializer.Serialize(data);
        }

        public string GetSummaryStats(int days)
        {
            var stats = new Dictionary<string, object>();

            try
            {
                string query = $@"
                    DECLARE @Today DATETIME = CAST(GETDATE() AS DATE);
                    
                    SELECT 
                        -- Today's stats
                        (SELECT COUNT(*) FROM EMPLOYE_POINTAGE WHERE is_deleted = 0 AND CAST(DateHeure AS DATE) = CAST(@Today AS DATE)) as TodayPunches,
                        (SELECT COUNT(DISTINCT IdEmploye) FROM EMPLOYE_POINTAGE WHERE is_deleted = 0 AND CAST(DateHeure AS DATE) = CAST(@Today AS DATE)) as TodayActiveEmployees,
                        
                        -- Period stats
                        (SELECT COUNT(*) FROM EMPLOYE_POINTAGE WHERE is_deleted = 0 AND DateHeure >= DATEADD(day, -{days}, GETDATE())) as PeriodPunches,
                        (SELECT COUNT(DISTINCT NomPort) FROM EMPLOYE_POINTAGE WHERE is_deleted = 0 AND DateHeure >= DATEADD(day, -{days}, GETDATE())) as ActivePorts";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                stats["todayPunches"] = Convert.ToInt32(reader["TodayPunches"]);
                                stats["todayActiveEmployees"] = Convert.ToInt32(reader["TodayActiveEmployees"]);
                                stats["monthPunches"] = Convert.ToInt32(reader["PeriodPunches"]);
                                stats["activePorts"] = Convert.ToInt32(reader["ActivePorts"]);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                stats["todayPunches"] = 0;
                stats["todayActiveEmployees"] = 0;
                stats["monthPunches"] = 0;
                stats["activePorts"] = 0;
            }

            return jsonSerializer.Serialize(stats);
        }

        #endregion
    }
}


