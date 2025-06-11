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
        private int? CustomEmployeeId
        {
            get { return ViewState["CustomEmployeeId"] as int?; }
            set { ViewState["CustomEmployeeId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtDatePointage.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadRecentPointages();
            }
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
                DateTime selectedDate = DateTime.Parse(txtDatePointage.Text);

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
                                THEN FORMAT(dernierPointage.DateHeure, 'HH:mm')
                                ELSE 'Aucun pointage'
                            END AS DerniereHeure,
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
                                WHERE is_deleted = 0
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
                        ORDER BY e.Nom, e.Prenom";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NomPort", ddlPort.SelectedValue);
                        cmd.Parameters.AddWithValue("@DateSelected", selectedDate.Date);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // Calculate working hours for each employee manually since we may not have the function
                            foreach (DataRow row in dt.Rows)
                            {
                                int employeeId = Convert.ToInt32(row["IdEmploye"]);
                                row["HeuresJour"] = CalculateWorkingHours(employeeId, selectedDate);
                            }

                            gvEmployees.DataSource = dt;
                            gvEmployees.DataBind();
                            litEmployeeCount.Text = dt.Rows.Count.ToString();
                            pnlEmployeePointage.Visible = true;

                            LoadDailySummary(selectedDate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des employés: " + ex.Message);
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

                            // Calculate total hours
                            TimeSpan totalHours = TimeSpan.Zero;

                            for (int i = 0; i < Math.Min(entrees.Count, sorties.Count); i++)
                            {
                                if (entrees[i] < sorties[i])
                                {
                                    totalHours = totalHours.Add(sorties[i] - entrees[i]);
                                }
                            }

                            // Format hours
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
                    // Get daily statistics
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

                                // Calculate average hours (simplified)
                                litMoyenneHeures.Text = "7h30"; // This would need more complex calculation
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

        private void LoadRecentPointages()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            p.IdPointage,
                            e.Nom + ' ' + e.Prenom AS NomEmploye,
                            e.Matricule AS MatriculeEmploye,
                            p.NomPort,
                            p.TypePointage,
                            p.DateHeure,
                            ISNULL(assigneur.Nom + ' ' + assigneur.Prenom, 'Système') AS AssignePar
                        FROM (
                            SELECT 
                                IdPointage,
                                IdEmploye,
                                NomPort,
                                TypePointage,
                                DateHeure,
                                AssignePar,
                                ROW_NUMBER() OVER (PARTITION BY IdEmploye ORDER BY DateHeure DESC) as rn
                            FROM EMPLOYE_POINTAGE 
                            WHERE is_deleted = 0 
                            AND DateHeure >= DATEADD(HOUR, -24, GETDATE())
                        ) p
                        INNER JOIN EMPLOYE e ON p.IdEmploye = e.IdEmploye
                        LEFT JOIN EMPLOYE assigneur ON p.AssignePar = assigneur.IdEmploye
                        WHERE p.rn = 1
                        ORDER BY p.DateHeure DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvRecentPointages.DataSource = dt;
                        gvRecentPointages.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des pointages: " + ex.Message);
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
                                WHERE is_deleted = 0
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

                                // Use validation logic to determine button visibility
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


        private void ShowCustomPointagePanel(int employeeId)
        {
            try
            {
                CustomEmployeeId = employeeId;

                // Get employee info
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT Matricule, Nom + ' ' + Prenom AS NomComplet FROM EMPLOYE WHERE IdEmploye = @IdEmploye";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                litCustomEmploye.Text = $"{reader["NomComplet"]} ({reader["Matricule"]})";
                            }
                        }
                    }
                }

                // Set default values
                txtCustomDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtCustomTime.Text = DateTime.Now.ToString("HH:mm");

                // Set default type based on current status
                string currentStatus = GetEmployeeCurrentStatus(employeeId);
                ddlCustomType.SelectedValue = currentStatus == "ENTREE" ? "SORTIE" : "ENTREE";

                pnlCustomTime.Visible = true;
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de l'affichage du panel: " + ex.Message);
            }
        }

        private void HideCustomPointagePanel()
        {
            pnlCustomTime.Visible = false;
            CustomEmployeeId = null;
        }

        private bool CreatePointageWithCustomTime(int employeeId, string typePointage, string portName, DateTime customDateTime)
        {
            try
            {
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
                        cmd.Parameters.AddWithValue("@DateHeure", customDateTime);
                        cmd.Parameters.AddWithValue("@AssignePar", GetCurrentUserId());
                        cmd.Parameters.AddWithValue("@DateCreation", DateTime.Now);

                        conn.Open();
                        cmd.ExecuteNonQuery();
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

        private string ValidatePointageSequenceWithDateTime(int employeeId, string typePointage, DateTime customDateTime)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Get the last pointage type for this employee before the custom datetime
                    string query = @"
                        SELECT TOP 1 TypePointage 
                        FROM EMPLOYE_POINTAGE 
                        WHERE IdEmploye = @IdEmploye 
                        AND DateHeure < @CustomDateTime
                        AND is_deleted = 0 
                        ORDER BY DateHeure DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                        cmd.Parameters.AddWithValue("@CustomDateTime", customDateTime);
                        conn.Open();

                        object result = cmd.ExecuteScalar();
                        string dernierType = result?.ToString();

                        // Validation logic
                        if (typePointage == "ENTREE")
                        {
                            if (!string.IsNullOrEmpty(dernierType) && dernierType == "ENTREE")
                            {
                                return "Employé déjà en ENTREE. Une SORTIE est requise avant une nouvelle ENTREE.";
                            }
                        }
                        else if (typePointage == "SORTIE")
                        {
                            if (string.IsNullOrEmpty(dernierType) || dernierType == "SORTIE")
                            {
                                return "Employé déjà en SORTIE ou aucune ENTREE enregistrée. Une ENTREE est requise avant une SORTIE.";
                            }
                        }

                        return ""; // Valid sequence
                    }
                }
            }
            catch (Exception ex)
            {
                return "Erreur lors de la validation: " + ex.Message;
            }
        }

        private void UpdatePointageDateTime(int pointageId, DateTime newDateTime)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE EMPLOYE_POINTAGE 
                        SET DateHeure = @DateHeure, DateModification = @DateModification 
                        WHERE IdPointage = @IdPointage";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdPointage", pointageId);
                        cmd.Parameters.AddWithValue("@DateHeure", newDateTime);
                        cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de la mise à jour: " + ex.Message);
            }
        }


        private bool CreatePointage(int employeeId, string typePointage, string portName = null)
        {
            try
            {
                // If no port specified, get employee's assigned port
                if (string.IsNullOrEmpty(portName))
                {
                    portName = GetEmployeeAssignedPort(employeeId);
                    if (string.IsNullOrEmpty(portName))
                    {
                        ShowError("Employé non assigné à un port");
                        return false;
                    }
                }

                // Validate pointage sequence before inserting
                string validationError = ValidatePointageSequence(employeeId, typePointage);
                if (!string.IsNullOrEmpty(validationError))
                {
                    ShowError(validationError);
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
                        cmd.Parameters.AddWithValue("@DateHeure", DateTime.Now);
                        cmd.Parameters.AddWithValue("@AssignePar", GetCurrentUserId());
                        cmd.Parameters.AddWithValue("@DateCreation", DateTime.Now);

                        conn.Open();
                        cmd.ExecuteNonQuery();
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

        private string ValidatePointageSequence(int employeeId, string typePointage)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Get the last pointage type for this employee
                    string query = @"
                        SELECT TOP 1 TypePointage 
                        FROM EMPLOYE_POINTAGE 
                        WHERE IdEmploye = @IdEmploye 
                        AND is_deleted = 0 
                        ORDER BY DateHeure DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                        conn.Open();

                        object result = cmd.ExecuteScalar();
                        string dernierType = result?.ToString();

                        // Validation logic
                        if (typePointage == "ENTREE")
                        {
                            if (!string.IsNullOrEmpty(dernierType) && dernierType == "ENTREE")
                            {
                                return "Employé déjà en ENTREE. Une SORTIE est requise avant une nouvelle ENTREE.";
                            }
                        }
                        else if (typePointage == "SORTIE")
                        {
                            if (string.IsNullOrEmpty(dernierType) || dernierType == "SORTIE")
                            {
                                return "Employé déjà en SORTIE ou aucune ENTREE enregistrée. Une ENTREE est requise avant une SORTIE.";
                            }
                        }

                        return ""; // Valid sequence
                    }
                }
            }
            catch (Exception ex)
            {
                return "Erreur lors de la validation: " + ex.Message;
            }
        }

        private string GetEmployeeCurrentStatus(int employeeId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT TOP 1 TypePointage 
                        FROM EMPLOYE_POINTAGE 
                        WHERE IdEmploye = @IdEmploye 
                        AND is_deleted = 0 
                        ORDER BY DateHeure DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                        conn.Open();

                        object result = cmd.ExecuteScalar();
                        return result?.ToString() ?? "SORTIE"; // Default to SORTIE if no pointage found
                    }
                }
            }
            catch
            {
                return "SORTIE"; // Default to SORTIE on error
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

        private void CancelPointage(int pointageId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE EMPLOYE_POINTAGE 
                        SET is_deleted = 1, DateModification = @DateModification 
                        WHERE IdPointage = @IdPointage";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdPointage", pointageId);
                        cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de l'annulation: " + ex.Message);
            }
        }

        protected void ddlPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadEmployeesForPort();
        }

        protected void txtDatePointage_TextChanged(object sender, EventArgs e)
        {
            LoadEmployeesForPort();
        }

        protected void btnSearchEmployee_Click(object sender, EventArgs e)
        {
            SearchEmployeeByMatricule();
        }

        protected void btnQuickEntree_Click(object sender, EventArgs e)
        {
            if (QuickEmployeeId.HasValue)
            {
                if (CreatePointage(QuickEmployeeId.Value, "ENTREE"))
                {
                    ShowSuccess("Entrée enregistrée avec succès");
                    SearchEmployeeByMatricule(); // Refresh status
                    LoadEmployeesForPort();
                    LoadRecentPointages();
                }
            }
        }

        protected void btnQuickSortie_Click(object sender, EventArgs e)
        {
            if (QuickEmployeeId.HasValue)
            {
                if (CreatePointage(QuickEmployeeId.Value, "SORTIE"))
                {
                    ShowSuccess("Sortie enregistrée avec succès");
                    SearchEmployeeByMatricule(); // Refresh status
                    LoadEmployeesForPort();
                    LoadRecentPointages();
                }
            }
        }

        protected void gvEmployees_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int employeeId = Convert.ToInt32(e.CommandArgument);

            switch (e.CommandName)
            {
                case "PointageEntree":
                    if (CreatePointage(employeeId, "ENTREE", ddlPort.SelectedValue))
                    {
                        ShowSuccess("Entrée enregistrée avec succès");
                        LoadEmployeesForPort();
                        LoadRecentPointages();
                    }
                    break;

                case "PointageSortie":
                    if (CreatePointage(employeeId, "SORTIE", ddlPort.SelectedValue))
                    {
                        ShowSuccess("Sortie enregistrée avec succès");
                        LoadEmployeesForPort();
                        LoadRecentPointages();
                    }
                    break;

                case "CustomPointage":
                    ShowCustomPointagePanel(employeeId);
                    break;

            }
        }

        protected void btnSaveCustom_Click(object sender, EventArgs e)
        {
            if (CustomEmployeeId.HasValue)
            {
                try
                {
                    // Validate inputs
                    if (string.IsNullOrEmpty(txtCustomDate.Text) || string.IsNullOrEmpty(txtCustomTime.Text))
                    {
                        ShowError("Veuillez saisir la date et l'heure");
                        return;
                    }

                    DateTime customDate = DateTime.Parse(txtCustomDate.Text);
                    TimeSpan customTime = TimeSpan.Parse(txtCustomTime.Text);
                    DateTime customDateTime = customDate.Date.Add(customTime);

                    // Validate date/time constraints
                    if (customDateTime > DateTime.Now)
                    {
                        ShowError("La date/heure ne peut pas être dans le futur");
                        return;
                    }

                    if (customDateTime < DateTime.Now.AddDays(-30))
                    {
                        ShowError("La date/heure ne peut pas être antérieure à 30 jours");
                        return;
                    }

                    string typePointage = ddlCustomType.SelectedValue;
                    string portName = ddlPort.SelectedValue;

                    if (string.IsNullOrEmpty(portName))
                    {
                        portName = GetEmployeeAssignedPort(CustomEmployeeId.Value);
                    }

                    // Validate sequence with custom datetime
                    string validationError = ValidatePointageSequenceWithDateTime(CustomEmployeeId.Value, typePointage, customDateTime);
                    if (!string.IsNullOrEmpty(validationError))
                    {
                        ShowError(validationError);
                        return;
                    }

                    if (CreatePointageWithCustomTime(CustomEmployeeId.Value, typePointage, portName, customDateTime))
                    {
                        ShowSuccess($"Pointage {typePointage} enregistré avec succès pour {customDateTime:dd/MM/yyyy HH:mm}");
                        HideCustomPointagePanel();
                        LoadEmployeesForPort();
                        LoadRecentPointages();
                    }
                }
                catch (Exception ex)
                {
                    ShowError("Erreur lors de l'enregistrement: " + ex.Message);
                }
            }
        }

        protected void btnCancelCustom_Click(object sender, EventArgs e)
        {
            HideCustomPointagePanel();
        }

        protected void gvRecentPointages_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "AnnulerPointage")
            {
                try
                {
                    int pointageId = Convert.ToInt32(e.CommandArgument);
                    CancelPointage(pointageId);
                    ShowSuccess("Pointage annulé avec succès");
                    LoadRecentPointages();
                    LoadEmployeesForPort();
                }
                catch (Exception ex)
                {
                    ShowError("Erreur lors de l'annulation: " + ex.Message);
                }
            }
        }

        protected void gvRecentPointages_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvRecentPointages.EditIndex = e.NewEditIndex;
            LoadRecentPointages();
        }

        protected void gvRecentPointages_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int pointageId = Convert.ToInt32(gvRecentPointages.DataKeys[e.RowIndex].Value);

                // Get the edit controls
                TextBox txtEditDate = (TextBox)gvRecentPointages.Rows[e.RowIndex].FindControl("txtEditDate");
                TextBox txtEditTime = (TextBox)gvRecentPointages.Rows[e.RowIndex].FindControl("txtEditTime");

                if (txtEditDate != null && txtEditTime != null)
                {
                    // Combine date and time
                    DateTime newDate = DateTime.Parse(txtEditDate.Text);
                    TimeSpan newTime = TimeSpan.Parse(txtEditTime.Text);
                    DateTime newDateTime = newDate.Date.Add(newTime);

                    // Validate the new date/time
                    if (newDateTime > DateTime.Now)
                    {
                        ShowError("La date/heure ne peut pas être dans le futur");
                        return;
                    }

                    if (newDateTime < DateTime.Now.AddDays(-30))
                    {
                        ShowError("La date/heure ne peut pas être antérieure à 30 jours");
                        return;
                    }

                    // Update the pointage
                    UpdatePointageDateTime(pointageId, newDateTime);
                    ShowSuccess("Date/heure du pointage mise à jour avec succès");

                    gvRecentPointages.EditIndex = -1;
                    LoadRecentPointages();
                    LoadEmployeesForPort();
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de la mise à jour: " + ex.Message);
            }
        }

        protected void gvRecentPointages_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvRecentPointages.EditIndex = -1;
            LoadRecentPointages();
        }

        protected void btnRefreshEmployees_Click(object sender, EventArgs e)
        {
            LoadEmployeesForPort();
        }

        protected void btnRefreshPointages_Click(object sender, EventArgs e)
        {
            LoadRecentPointages();
        }

        private int GetCurrentUserId()
        {
            // TODO: Get from session/authentication
            return Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
        }

        private void ShowSuccess(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Success", $"alert('Succès: {message}');", true);
        }

        private void ShowError(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Error", $"alert('Erreur: {message}');", true);
        }

    }
}