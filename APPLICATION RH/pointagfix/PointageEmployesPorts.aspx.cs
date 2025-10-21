using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.pointagfix
{
    public partial class PointageEmployesPorts : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        private int? QuickEmployeeId
        {
            get { return ViewState["QuickEmployeeId"] as int?; }
            set { ViewState["QuickEmployeeId"] = value; }
        }

        // Property to persist the selected date
        private DateTime PointageDate
        {
            get
            {
                if (ViewState["PointageDate"] != null)
                    return (DateTime)ViewState["PointageDate"];
                return DateTime.Now.Date;
            }
            set
            {
                ViewState["PointageDate"] = value;
            }
        }

        // Property to persist the time value
        private string PointageTime
        {
            get
            {
                return ViewState["PointageTime"] as string ?? DateTime.Now.ToString("HH:mm");
            }
            set
            {
                ViewState["PointageTime"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Initialize date and time inputs with current date/time
                txtPointageDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtPointageTime.Text = DateTime.Now.ToString("HH:mm");
                PointageDate = DateTime.Now.Date;
                PointageTime = txtPointageTime.Text;

                PopulatePortDropdown();
                HandleRedirectMessages();
                UpdateDateStatusIndicator();
                RestoreStateFromQueryString();
            }
            else
            {
                // Preserve the time value on postback
                if (!string.IsNullOrEmpty(txtPointageTime.Text))
                {
                    PointageTime = txtPointageTime.Text;
                }
                else
                {
                    txtPointageTime.Text = PointageTime;
                }

                // Preserve the date value on postback
                if (!string.IsNullOrEmpty(txtPointageDate.Text))
                {
                    if (DateTime.TryParse(txtPointageDate.Text, out DateTime selectedDate))
                    {
                        PointageDate = selectedDate.Date;
                    }
                }
                else
                {
                    txtPointageDate.Text = PointageDate.ToString("yyyy-MM-dd");
                }

                UpdateDateStatusIndicator();
            }
        }

        private void UpdateDateStatusIndicator()
        {
            DateTime today = DateTime.Now.Date;
            DateTime selectedDate = PointageDate;

            if (selectedDate == today)
            {
                litDateStatus.Text = "<span class='today-indicator'><i class='fas fa-calendar-check'></i> Aujourd'hui</span>";
            }
            else if (selectedDate < today)
            {
                int daysDiff = (today - selectedDate).Days;
                litDateStatus.Text = $"<span class='past-date-indicator'><i class='fas fa-calendar-minus'></i> Il y a {daysDiff} jour{(daysDiff > 1 ? "s" : "")}</span>";
            }
            else
            {
                int daysDiff = (selectedDate - today).Days;
                litDateStatus.Text = $"<span class='text-info'><i class='fas fa-calendar-plus'></i> Dans {daysDiff} jour{(daysDiff > 1 ? "s" : "")}</span>";
            }
        }

        //private void PopulatePortDropdown()
        //{
        //    string userRole = Session["EmployeRole"]?.ToString();

        //    // Clear existing items
        //    ddlPort.Items.Clear();

        //    if (userRole == "securityPortA")
        //    {
        //        // Only show Port A for securityPortA users
        //        ddlPort.Items.Add(new ListItem("Port A - Entrée Principale", "Port A - Entrée Principale"));
        //        ddlPort.SelectedIndex = 0; // Auto-select the only option
        //        ddlPort.Enabled = false; // Disable since there's only one option
        //    }
        //    else
        //    {

        //        // Show all ports for other users
        //        ddlPort.Items.Add(new ListItem("-- Sélectionner Porte --", ""));
        //        ddlPort.Items.Add(new ListItem("Porte A - Entrée Principal", "Porte A - Entrée Principal"));
        //        ddlPort.Items.Add(new ListItem("Porte B - Entrée GSF", "Porte B - Entrée GSF"));
        //        ddlPort.Items.Add(new ListItem("Porte C - Entrée POI", "Porte C - Entrée POI"));
        //        ddlPort.Items.Add(new ListItem("Porte D - Entrée Technique", "Porte D - Entrée Technique"));
        //        ddlPort.Items.Add(new ListItem("Porte E - Entrée ERG PACKAGING", "Porte E - Entrée ERG PACKAGING"));
        //        ddlPort.Enabled = true;
        //    }
        //}

        private void PopulatePortDropdown()
        {
            int employeId = Convert.ToInt32(Session["EmployeId"]?.ToString());

            // Clear existing items
            ddlPort.Items.Clear();

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            string query = @"
        SELECT DISTINCT p.* 
        FROM AccessLogine al
        INNER JOIN PORTEUSINE p ON al.IDPorteAssigne = p.idPorte
        WHERE al.IdEmploye = @EmployeId";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeId", employeId);

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            int portCount = 0;

                            while (reader.Read())
                            {
                                // Use column index to avoid column name errors
                                string portName = reader[1].ToString(); // Adjust index based on your table structure
                                ddlPort.Items.Add(new ListItem(portName, portName));
                                portCount++;
                            }

                            if (portCount == 0)
                            {
                                ddlPort.Items.Add(new ListItem("Aucune porte assignée", ""));
                                ddlPort.Enabled = false;
                            }
                            else if (portCount == 1)
                            {
                                ddlPort.SelectedIndex = 0;
                                ddlPort.Enabled = false;
                            }
                            else
                            {
                                ddlPort.Items.Insert(0, new ListItem("Toutes mes portes assignées", ""));
                                ddlPort.Enabled = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ddlPort.Items.Add(new ListItem("Erreur lors du chargement des portes", ""));
                ddlPort.Enabled = false;
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        private void HandleRedirectMessages()
        {
            string success = Request.QueryString["success"];
            string error = Request.QueryString["error"];

            if (!string.IsNullOrEmpty(success))
            {
                ShowSuccess(HttpUtility.UrlDecode(success));
            }

            if (!string.IsNullOrEmpty(error))
            {
                ShowError(HttpUtility.UrlDecode(error));
            }
        }

        private void RestoreStateFromQueryString()
        {
            string port = Request.QueryString["port"];
            string employee = Request.QueryString["employee"];
            string time = Request.QueryString["time"];
            string date = Request.QueryString["date"];

            if (!string.IsNullOrEmpty(port))
            {
                ddlPort.SelectedValue = port;
            }

            if (!string.IsNullOrEmpty(employee))
            {
                txtMatricule.Text = HttpUtility.UrlDecode(employee);
                SearchEmployeeByMatricule();
            }

            // Restore time value
            if (!string.IsNullOrEmpty(time))
            {
                txtPointageTime.Text = HttpUtility.UrlDecode(time);
                PointageTime = txtPointageTime.Text;
            }

            // Restore date value
            if (!string.IsNullOrEmpty(date))
            {
                if (DateTime.TryParse(HttpUtility.UrlDecode(date), out DateTime selectedDate))
                {
                    txtPointageDate.Text = selectedDate.ToString("yyyy-MM-dd");
                    PointageDate = selectedDate.Date;
                    UpdateDateStatusIndicator();
                }
            }

            // Load employees if port is selected
            if (!string.IsNullOrEmpty(ddlPort.SelectedValue))
            {
                LoadEmployeesForPort();
            }
        }

        private void RedirectWithMessage(string message, bool isSuccess = true)
        {
            string currentUrl = Request.Url.AbsolutePath;
            string queryString = BuildQueryString();

            string messageParam = isSuccess ? "success" : "error";
            string separator = string.IsNullOrEmpty(queryString) ? "?" : "&";

            string redirectUrl = $"{currentUrl}{queryString}{separator}{messageParam}={HttpUtility.UrlEncode(message)}";
            Response.Redirect(redirectUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private string BuildQueryString()
        {
            List<string> parameters = new List<string>();

            if (!string.IsNullOrEmpty(ddlPort.SelectedValue))
                parameters.Add($"port={HttpUtility.UrlEncode(ddlPort.SelectedValue)}");

            if (!string.IsNullOrEmpty(txtMatricule.Text))
                parameters.Add($"employee={HttpUtility.UrlEncode(txtMatricule.Text)}");

            // Add time parameter
            if (!string.IsNullOrEmpty(txtPointageTime.Text))
                parameters.Add($"time={HttpUtility.UrlEncode(txtPointageTime.Text)}");

            // Add date parameter
            if (!string.IsNullOrEmpty(txtPointageDate.Text))
                parameters.Add($"date={HttpUtility.UrlEncode(txtPointageDate.Text)}");

            return parameters.Count > 0 ? "?" + string.Join("&", parameters) : "";
        }

        private void LoadEmployeesForPort()
        {
            if (string.IsNullOrEmpty(ddlPort.SelectedValue))
            {
                pnlEmployeePointage.Visible = false;
                pnlDailySummary.Visible = false;
                return;
            }

            try
            {
                DateTime selectedDate = PointageDate; // Use selected date instead of today

                // Load both present and absent employees for the selected date
                LoadEmployeesPresent(selectedDate);
                LoadEmployeesAbsent(selectedDate);
                LoadDailySummary(selectedDate);

                pnlEmployeePointage.Visible = true;
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des employés: " + ex.Message);
            }
        }

        private void LoadEmployeesPresent(DateTime selectedDate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            e.IdEmploye,
                            e.Matricule,
                            e.Nom + ' ' + e.Prenom AS NomComplet,
                            ISNULL(dernierPointage.TypePointage, 'SORTIE') AS StatutActuel,
                            CASE 
                                WHEN dernierPointage.DateHeure IS NOT NULL 
                                THEN FORMAT(dernierPointage.DateHeure, 'dd/MM/yyyy HH:mm')
                                ELSE 'Aucun pointage'
                            END AS DerniereDateHeure,
                            ISNULL(heuresJour.TotalHeures, '0h00') AS HeuresJour,
                            CASE 
                                WHEN ISNULL(dernierPointage.TypePointage, 'SORTIE') = 'ENTREE' THEN 0
                                ELSE 1
                            END AS PeutEntrer,
                            CASE 
                                WHEN ISNULL(dernierPointage.TypePointage, 'SORTIE') = 'ENTREE' THEN 1
                                ELSE 0
                            END AS PeutSortir
                        FROM EMPLOYE e
                        INNER JOIN EMPLOYE_PORT ep ON e.IdEmploye = ep.IdEmploye 
                            AND ep.NomPort = @NomPort 
                            AND ep.is_deleted = 0
                        LEFT JOIN (
                            SELECT 
                                p1.IdEmploye,
                                p1.TypePointage,
                                p1.DateHeure
                            FROM EMPLOYE_POINTAGE p1
                            INNER JOIN (
                                SELECT IdEmploye, MAX(DateHeure) as MaxDate
                                FROM EMPLOYE_POINTAGE 
                                WHERE CAST(DateHeure AS DATE) <= @DateSelected
                                AND is_deleted = 0
                                GROUP BY IdEmploye
                            ) p2 ON p1.IdEmploye = p2.IdEmploye AND p1.DateHeure = p2.MaxDate
                            WHERE p1.is_deleted = 0
                        ) dernierPointage ON e.IdEmploye = dernierPointage.IdEmploye
                        LEFT JOIN (
                            SELECT 
                                IdEmploye,
                                dbo.fn_CalculateWorkingHours(IdEmploye, @DateSelected) AS TotalHeures
                            FROM EMPLOYE
                        ) heuresJour ON e.IdEmploye = heuresJour.IdEmploye
                        WHERE e.is_deleted = 0 
                        AND e.DateSortie IS NULL
                        AND ISNULL(dernierPointage.TypePointage, 'SORTIE') = 'ENTREE'
                        ORDER BY e.Nom, e.Prenom";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NomPort", ddlPort.SelectedValue);
                        cmd.Parameters.AddWithValue("@DateSelected", selectedDate.Date);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // Calculate working hours for each employee
                            foreach (DataRow row in dt.Rows)
                            {
                                int employeeId = Convert.ToInt32(row["IdEmploye"]);
                                row["HeuresJour"] = CalculateWorkingHours(employeeId, selectedDate);
                            }

                            gvEmployeesPresent.DataSource = dt;
                            gvEmployeesPresent.DataBind();
                            litPresentCount.Text = dt.Rows.Count.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des employés présents: " + ex.Message);
            }
        }

        private void LoadEmployeesAbsent(DateTime selectedDate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            e.IdEmploye,
                            e.Matricule,
                            e.Nom + ' ' + e.Prenom AS NomComplet,
                            ISNULL(dernierPointage.TypePointage, 'SORTIE') AS StatutActuel,
                            CASE 
                                WHEN dernierPointage.DateHeure IS NOT NULL 
                                THEN FORMAT(dernierPointage.DateHeure, 'dd/MM/yyyy HH:mm')
                                ELSE 'Aucun pointage'
                            END AS DerniereDateHeure,
                            ISNULL(heuresJour.TotalHeures, '0h00') AS HeuresJour,
                            CASE 
                                WHEN ISNULL(dernierPointage.TypePointage, 'SORTIE') = 'ENTREE' THEN 0
                                ELSE 1
                            END AS PeutEntrer,
                            CASE 
                                WHEN ISNULL(dernierPointage.TypePointage, 'SORTIE') = 'ENTREE' THEN 1
                                ELSE 0
                            END AS PeutSortir
                        FROM EMPLOYE e
                        INNER JOIN EMPLOYE_PORT ep ON e.IdEmploye = ep.IdEmploye 
                            AND ep.NomPort = @NomPort 
                            AND ep.is_deleted = 0
                        LEFT JOIN (
                            SELECT 
                                p1.IdEmploye,
                                p1.TypePointage,
                                p1.DateHeure
                            FROM EMPLOYE_POINTAGE p1
                            INNER JOIN (
                                SELECT IdEmploye, MAX(DateHeure) as MaxDate
                                FROM EMPLOYE_POINTAGE 
                                WHERE CAST(DateHeure AS DATE) <= @DateSelected
                                AND is_deleted = 0
                                GROUP BY IdEmploye
                            ) p2 ON p1.IdEmploye = p2.IdEmploye AND p1.DateHeure = p2.MaxDate
                            WHERE p1.is_deleted = 0
                        ) dernierPointage ON e.IdEmploye = dernierPointage.IdEmploye
                        LEFT JOIN (
                            SELECT 
                                IdEmploye,
                                dbo.fn_CalculateWorkingHours(IdEmploye, @DateSelected) AS TotalHeures
                            FROM EMPLOYE
                        ) heuresJour ON e.IdEmploye = heuresJour.IdEmploye
                        WHERE e.is_deleted = 0 
                        AND e.DateSortie IS NULL
                        AND ISNULL(dernierPointage.TypePointage, 'SORTIE') = 'SORTIE'
                        ORDER BY e.Nom, e.Prenom";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NomPort", ddlPort.SelectedValue);
                        cmd.Parameters.AddWithValue("@DateSelected", selectedDate.Date);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // Calculate working hours for each employee
                            foreach (DataRow row in dt.Rows)
                            {
                                int employeeId = Convert.ToInt32(row["IdEmploye"]);
                                row["HeuresJour"] = CalculateWorkingHours(employeeId, selectedDate);
                            }

                            gvEmployeesAbsent.DataSource = dt;
                            gvEmployeesAbsent.DataBind();
                            litAbsentCount.Text = dt.Rows.Count.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des employés absents: " + ex.Message);
            }
        }

        private string CalculateWorkingHours(int employeeId, DateTime date)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT TypePointage, DateHeure 
                        FROM EMPLOYE_POINTAGE 
                        WHERE IdEmploye = @IdEmploye 
                        AND CAST(DateHeure AS DATE) = @Date 
                        AND is_deleted = 0
                        ORDER BY DateHeure";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                        cmd.Parameters.AddWithValue("@Date", date.Date);

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<DateTime> entrees = new List<DateTime>();
                            List<DateTime> sorties = new List<DateTime>();

                            while (reader.Read())
                            {
                                DateTime dateHeure = Convert.ToDateTime(reader["DateHeure"]);
                                string type = reader["TypePointage"].ToString();

                                if (type == "ENTREE")
                                    entrees.Add(dateHeure);
                                else
                                    sorties.Add(dateHeure);
                            }

                            TimeSpan totalHours = TimeSpan.Zero;
                            for (int i = 0; i < Math.Min(entrees.Count, sorties.Count); i++)
                            {
                                if (entrees[i] < sorties[i])
                                {
                                    totalHours = totalHours.Add(sorties[i] - entrees[i]);
                                }
                            }

                            int hours = (int)totalHours.TotalHours;
                            int minutes = totalHours.Minutes;
                            return $"{hours}h{minutes:00}";
                        }
                    }
                }
            }
            catch
            {
                return "0h00";
            }
        }

        private void LoadDailySummary(DateTime date)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            COUNT(DISTINCT CASE WHEN p.TypePointage = 'ENTREE' AND p.DateHeure = lastEntree.MaxEntree THEN p.IdEmploye END) AS TotalPresents,
                            COUNT(CASE WHEN p.TypePointage = 'ENTREE' THEN 1 END) AS TotalEntrees,
                            COUNT(CASE WHEN p.TypePointage = 'SORTIE' THEN 1 END) AS TotalSorties
                        FROM EMPLOYE_POINTAGE p
                        LEFT JOIN (
                            SELECT IdEmploye, MAX(DateHeure) as MaxEntree
                            FROM EMPLOYE_POINTAGE 
                            WHERE CAST(DateHeure AS DATE) = @Date 
                            AND TypePointage = 'ENTREE'
                            AND is_deleted = 0
                            GROUP BY IdEmploye
                        ) lastEntree ON p.IdEmploye = lastEntree.IdEmploye
                        WHERE CAST(p.DateHeure AS DATE) = @Date 
                        AND p.is_deleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Date", date.Date);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                litTotalPresents.Text = reader["TotalPresents"].ToString();
                                litTotalEntrees.Text = reader["TotalEntrees"].ToString();
                                litTotalSorties.Text = reader["TotalSorties"].ToString();
                                litMoyenneHeures.Text = "7h30";
                            }
                        }
                    }

                    litSelectedDate.Text = date.ToString("dd/MM/yyyy");
                    pnlDailySummary.Visible = true;
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du calcul du résumé: " + ex.Message);
            }
        }

        private void SearchEmployeeByMatricule()
        {
            if (string.IsNullOrEmpty(txtMatricule.Text.Trim()))
            {
                ShowError("Veuillez saisir un nom ou prénom");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            e.IdEmploye,
                            e.Matricule,
                            e.Nom + ' ' + e.Prenom AS NomComplet,
                            ISNULL(dernierPointage.TypePointage, 'SORTIE') AS StatutActuel,
                            ep.NomPort
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
                                WHERE CAST(DateHeure AS DATE) <= @DateSelected
                                AND is_deleted = 0
                                GROUP BY IdEmploye
                            ) p2 ON p1.IdEmploye = p2.IdEmploye AND p1.DateHeure = p2.MaxDate
                            WHERE p1.is_deleted = 0
                        ) dernierPointage ON e.IdEmploye = dernierPointage.IdEmploye
                        WHERE (e.Nom LIKE @SearchTerm OR e.Prenom LIKE @SearchTerm OR (e.Nom + ' ' + e.Prenom) LIKE @SearchTerm)
                        AND e.is_deleted = 0 
                        AND e.DateSortie IS NULL";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SearchTerm", "%" + txtMatricule.Text.Trim() + "%");
                        cmd.Parameters.AddWithValue("@DateSelected", PointageDate);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                QuickEmployeeId = Convert.ToInt32(reader["IdEmploye"]);
                                litQuickMatricule.Text = reader["Matricule"].ToString();
                                litQuickNom.Text = reader["NomComplet"].ToString();

                                string statut = reader["StatutActuel"].ToString();
                                litQuickStatut.Text = $"<span class='badge {(statut == "ENTREE" ? "bg-success" : "bg-danger")}'>{(statut == "ENTREE" ? "PRÉSENT" : "SORTI")}</span>";

                                string entreeValidation = ValidatePointageSequence(QuickEmployeeId.Value, "ENTREE");
                                string sortieValidation = ValidatePointageSequence(QuickEmployeeId.Value, "SORTIE");

                                btnQuickEntree.Visible = string.IsNullOrEmpty(entreeValidation);
                                btnQuickSortie.Visible = string.IsNullOrEmpty(sortieValidation);

                                pnlQuickEmployee.Visible = true;
                                ShowSuccess("Employé trouvé");
                            }
                            else
                            {
                                pnlQuickEmployee.Visible = false;
                                ShowError("Aucun employé trouvé avec ce nom");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de la recherche: " + ex.Message);
            }
        }

        private bool CreatePointage(int employeeId, string typePointage, string portName = null, string customTime = null)
        {
            try
            {
                if (string.IsNullOrEmpty(portName))
                {
                    portName = GetEmployeeAssignedPort(employeeId);
                    if (string.IsNullOrEmpty(portName))
                    {
                        ShowError("Employé non assigné à un port");
                        return false;
                    }
                }

                string validationError = ValidatePointageSequence(employeeId, typePointage);
                if (!string.IsNullOrEmpty(validationError))
                {
                    ShowError(validationError);
                    return false;
                }

                // Get the time from input or use the passed customTime
                string timeToUse = customTime ?? txtPointageTime.Text;

                // Validate time format
                if (string.IsNullOrEmpty(timeToUse))
                {
                    ShowError("Veuillez spécifier une heure de pointage");
                    return false;
                }

                // Parse the time
                TimeSpan parsedTime;
                if (!TimeSpan.TryParse(timeToUse, out parsedTime))
                {
                    ShowError("Format d'heure invalide. Utilisez HH:mm");
                    return false;
                }

                // Validate selected date
                DateTime selectedDate = PointageDate;
                if (!ValidatePointageDate(selectedDate))
                {
                    return false;
                }

                // Combine selected date with the specified time
                DateTime pointageDateTime = selectedDate.Add(parsedTime);

                // Validate that the date/time is not in the future (only if selected date is today)
                if (selectedDate == DateTime.Now.Date && pointageDateTime > DateTime.Now)
                {
                    ShowError("L'heure de pointage ne peut pas être dans le futur");
                    return false;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                INSERT INTO EMPLOYE_POINTAGE (IdEmploye, NomPort, TypePointage, DateHeure, AssignePar, DateCreation, is_deleted)
                VALUES (@IdEmploye, @NomPort, @TypePointage, @DateHeure, @AssignePar, @DateCreation, 0)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                        cmd.Parameters.AddWithValue("@NomPort", portName);
                        cmd.Parameters.AddWithValue("@TypePointage", typePointage);
                        cmd.Parameters.AddWithValue("@DateHeure", pointageDateTime); // Use the selected date + time
                        cmd.Parameters.AddWithValue("@AssignePar", GetCurrentUserId());
                        cmd.Parameters.AddWithValue("@DateCreation", DateTime.Now);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        // Save the time for next use
                        PointageTime = timeToUse;

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du pointage: " + ex.Message);
                return false;
            }
        }

        private bool ValidatePointageDate(DateTime selectedDate)
        {
            DateTime today = DateTime.Now.Date;

            // Allow dates up to 30 days in the past
            if (selectedDate < today.AddDays(-30))
            {
                ShowError("La date de pointage ne peut pas être antérieure à 30 jours");
                return false;
            }

            // Allow dates up to 7 days in the future (for planned entries)
            if (selectedDate > today.AddDays(7))
            {
                ShowError("La date de pointage ne peut pas être supérieure à 7 jours dans le futur");
                return false;
            }

            return true;
        }

        private bool HasEntryOnDate(int employeeId, DateTime date)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT COUNT(*)
                        FROM EMPLOYE_POINTAGE
                        WHERE IdEmploye = @IdEmploye
                        AND CAST(DateHeure AS DATE) = @Date
                        AND TypePointage = 'ENTREE'
                        AND is_deleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                        cmd.Parameters.AddWithValue("@Date", date.Date);
                        conn.Open();

                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private bool CreatePreviousDayEntryExit(int employeeId, string portName)
        {
            try
            {
                DateTime previousDay = PointageDate.AddDays(-1);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Create ENTREE at 00:00 on previous day
                            DateTime entryDateTime = previousDay.Date;
                            string entryQuery = @"
                                INSERT INTO EMPLOYE_POINTAGE (IdEmploye, NomPort, TypePointage, DateHeure, AssignePar, DateCreation, is_deleted)
                                VALUES (@IdEmploye, @NomPort, @TypePointage, @DateHeure, @AssignePar, @DateCreation, 0)";

                            using (SqlCommand entryCmd = new SqlCommand(entryQuery, conn, transaction))
                            {
                                entryCmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                                entryCmd.Parameters.AddWithValue("@NomPort", portName);
                                entryCmd.Parameters.AddWithValue("@TypePointage", "ENTREE");
                                entryCmd.Parameters.AddWithValue("@DateHeure", entryDateTime);
                                entryCmd.Parameters.AddWithValue("@AssignePar", GetCurrentUserId());
                                entryCmd.Parameters.AddWithValue("@DateCreation", DateTime.Now);
                                entryCmd.ExecuteNonQuery();
                            }

                            // Create SORTIE at 11:59 on previous day
                            DateTime exitDateTime = previousDay.Date.AddHours(11).AddMinutes(59);
                            string exitQuery = @"
                                INSERT INTO EMPLOYE_POINTAGE (IdEmploye, NomPort, TypePointage, DateHeure, AssignePar, DateCreation, is_deleted)
                                VALUES (@IdEmploye, @NomPort, @TypePointage, @DateHeure, @AssignePar, @DateCreation, 0)";

                            using (SqlCommand exitCmd = new SqlCommand(exitQuery, conn, transaction))
                            {
                                exitCmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                                exitCmd.Parameters.AddWithValue("@NomPort", portName);
                                exitCmd.Parameters.AddWithValue("@TypePointage", "SORTIE");
                                exitCmd.Parameters.AddWithValue("@DateHeure", exitDateTime);
                                exitCmd.Parameters.AddWithValue("@AssignePar", GetCurrentUserId());
                                exitCmd.Parameters.AddWithValue("@DateCreation", DateTime.Now);
                                exitCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private string ValidatePointageSequence(int employeeId, string typePointage)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT TOP 1 TypePointage
                        FROM EMPLOYE_POINTAGE
                        WHERE IdEmploye = @IdEmploye
                        AND CAST(DateHeure AS DATE) <= @DateSelected
                        AND is_deleted = 0
                        ORDER BY DateHeure DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                        cmd.Parameters.AddWithValue("@DateSelected", PointageDate);
                        conn.Open();

                        object result = cmd.ExecuteScalar();
                        string dernierType = result?.ToString();

                        if (typePointage == "ENTREE")
                        {
                            if (!string.IsNullOrEmpty(dernierType) && dernierType == "ENTREE")
                            {
                                return "Employé déjà en ENTREE. Une SORTIE est requise avant une nouvelle ENTREE.";
                            }
                        }
                        else if (typePointage == "SORTIE")
                        {
                            // Check if there's an entry on the same day
                            if (!HasEntryOnDate(employeeId, PointageDate))
                            {
                                // No entry on current day, create previous day entry/exit
                                string portName = GetEmployeeAssignedPort(employeeId);
                                if (string.IsNullOrEmpty(portName))
                                {
                                    return "Impossible de créer l'entrée automatique: employé non assigné à un port";
                                }

                                if (CreatePreviousDayEntryExit(employeeId, portName))
                                {
                                    // Log the automatic creation for audit purposes
                                    System.Diagnostics.Debug.WriteLine($"Entrée/sortie automatique créée pour l'employé {employeeId} le {PointageDate.AddDays(-1):dd/MM/yyyy}");
                                }
                                else
                                {
                                    return "Erreur lors de la création automatique de l'entrée/sortie du jour précédent";
                                }
                            }
                            else
                            {
                                // Standard validation for same-day exit
                                if (string.IsNullOrEmpty(dernierType) || dernierType == "SORTIE")
                                {
                                    return "Employé déjà en SORTIE ou aucune ENTREE enregistrée. Une ENTREE est requise avant une SORTIE.";
                                }
                            }
                        }

                        return "";
                    }
                }
            }
            catch (Exception ex)
            {
                return "Erreur lors de la validation: " + ex.Message;
            }
        }

        private string GetEmployeeAssignedPort(int employeeId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT TOP 1 NomPort FROM EMPLOYE_PORT WHERE IdEmploye = @IdEmploye AND is_deleted = 0";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                        conn.Open();
                        return cmd.ExecuteScalar()?.ToString() ?? "";
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        // Event Handlers
        protected void btnSetToday_Click(object sender, EventArgs e)
        {
            txtPointageDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            PointageDate = DateTime.Now.Date;
            UpdateDateStatusIndicator();

            // Reload data for today
            if (!string.IsNullOrEmpty(ddlPort.SelectedValue))
            {
                LoadEmployeesForPort();
            }

            ShowSuccess("Date mise à jour: Aujourd'hui");
        }

        protected void txtPointageDate_TextChanged(object sender, EventArgs e)
        {
            if (DateTime.TryParse(txtPointageDate.Text, out DateTime selectedDate))
            {
                PointageDate = selectedDate.Date;
                UpdateDateStatusIndicator();

                // Reload data for the new date
                if (!string.IsNullOrEmpty(ddlPort.SelectedValue))
                {
                    LoadEmployeesForPort();
                }
            }
        }

        protected void btnSetCurrentTime_Click(object sender, EventArgs e)
        {
            txtPointageTime.Text = DateTime.Now.ToString("HH:mm");
            PointageTime = txtPointageTime.Text;
            ShowSuccess($"Heure mise à jour: {txtPointageTime.Text}");
        }

        protected void btnQuickEntree_Click(object sender, EventArgs e)
        {
            if (QuickEmployeeId.HasValue)
            {
                // Pass the time from the input
                if (CreatePointage(QuickEmployeeId.Value, "ENTREE", null, txtPointageTime.Text))
                {
                    RedirectWithMessage($"Entrée enregistrée avec succès le {PointageDate.ToString("dd/MM/yyyy")} à {txtPointageTime.Text}");
                }
                else
                {
                    RedirectWithMessage("Erreur lors de l'enregistrement de l'entrée", false);
                }
            }
        }

        protected void btnQuickSortie_Click(object sender, EventArgs e)
        {
            if (QuickEmployeeId.HasValue)
            {
                // Pass the time from the input
                if (CreatePointage(QuickEmployeeId.Value, "SORTIE", null, txtPointageTime.Text))
                {
                    RedirectWithMessage($"Sortie enregistrée avec succès le {PointageDate.ToString("dd/MM/yyyy")} à {txtPointageTime.Text}");
                }
                else
                {
                    RedirectWithMessage("Erreur lors de l'enregistrement de la sortie", false);
                }
            }
        }

        private void HandleEmployeeRowCommand(GridViewCommandEventArgs e)
        {
            int employeeId = Convert.ToInt32(e.CommandArgument);

            switch (e.CommandName)
            {
                case "PointageEntree":
                    if (CreatePointage(employeeId, "ENTREE", ddlPort.SelectedValue, txtPointageTime.Text))
                    {
                        RedirectWithMessage($"Entrée enregistrée avec succès le {PointageDate.ToString("dd/MM/yyyy")} à {txtPointageTime.Text}");
                    }
                    else
                    {
                        RedirectWithMessage("Erreur lors de l'enregistrement de l'entrée", false);
                    }
                    break;

                case "PointageSortie":
                    if (CreatePointage(employeeId, "SORTIE", ddlPort.SelectedValue, txtPointageTime.Text))
                    {
                        RedirectWithMessage($"Sortie enregistrée avec succès le {PointageDate.ToString("dd/MM/yyyy")} à {txtPointageTime.Text}");
                    }
                    else
                    {
                        RedirectWithMessage("Erreur lors de l'enregistrement de la sortie", false);
                    }
                    break;
            }
        }

        protected void gvEmployeesPresent_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            HandleEmployeeRowCommand(e);
        }

        protected void gvEmployeesAbsent_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            HandleEmployeeRowCommand(e);
        }

        protected void ddlPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadEmployeesForPort();
        }

        protected void btnSearchEmployee_Click(object sender, EventArgs e)
        {
            SearchEmployeeByMatricule();
        }

        protected void btnRefreshEmployees_Click(object sender, EventArgs e)
        {
            LoadEmployeesForPort();
        }

        private int GetCurrentUserId()
        {
            if (Session["EmployeId"] == null)
            {
                Session.Clear();
                Response.Redirect("~/connexion.aspx");
                return 0;
            }

            return Convert.ToInt32(Session["EmployeId"]);
        }

        private void ShowSuccess(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Success", $"showToast('success', '{message}');", true);
        }

        private void ShowError(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Error", $"showToast('error', '{message}');", true);
        }

        // Helper method for formatting date-time in GridView
        protected string FormatDateTime(object dateTimeValue)
        {
            if (dateTimeValue == null || dateTimeValue == DBNull.Value)
                return "<small class='text-muted'>Aucun pointage</small>";

            string dateTimeStr = dateTimeValue.ToString();
            if (string.IsNullOrEmpty(dateTimeStr) || dateTimeStr == "Aucun pointage")
                return "<small class='text-muted'>Aucun pointage</small>";

            // Try to parse the formatted date-time string
            if (DateTime.TryParseExact(dateTimeStr, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDateTime))
            {
                string dateStr = parsedDateTime.ToString("dd/MM/yyyy");
                string timeStr = parsedDateTime.ToString("HH:mm");

                // Check if it's today to highlight differently
                if (parsedDateTime.Date == DateTime.Now.Date)
                {
                    return $"<small class='datetime-display'><span class='date-part text-success'>Aujourd'hui</span><span class='time-part'>{timeStr}</span></small>";
                }
                else
                {
                    return $"<small class='datetime-display'><span class='date-part'>{dateStr}</span><span class='time-part'>{timeStr}</span></small>";
                }
            }

            // Fallback to original value if parsing fails
            return $"<small class='text-muted'>{dateTimeStr}</small>";
        }
    }
}
