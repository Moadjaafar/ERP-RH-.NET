using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.conge
{
    public partial class GestionJoursFeries : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        private int currentYear = DateTime.Now.Year;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Force UTF-8 encoding for this page


            if (!IsPostBack)
            {
                // Get year from session or use current year
                if (Session["SelectedYear"] != null)
                {
                    currentYear = Convert.ToInt32(Session["SelectedYear"]);
                }
                else
                {
                    Session["SelectedYear"] = currentYear;
                }

                InitializePage();
            }
            else
            {
                // Restore year from session
                if (Session["SelectedYear"] != null)
                {
                    currentYear = Convert.ToInt32(Session["SelectedYear"]);
                }
            }
        }

        private void InitializePage()
        {
            btnCurrentYear.Text = currentYear.ToString();
            litCurrentYear.Text = currentYear.ToString();
            LoadStatistics();
            LoadHolidays();
        }

        #region Statistics

        private void LoadStatistics()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT
                            COUNT(CASE WHEN TypeJour = 'FERIE' AND YEAR(DateJour) = @Year THEN 1 END) as TotalHolidays,
                            COUNT(CASE WHEN TypeJour = 'SPECIAL' AND YEAR(DateJour) = @Year THEN 1 END) as TotalSpecial,
                            COUNT(CASE WHEN DateJour >= @Today THEN 1 END) as Upcoming,
                            COUNT(*) as TotalDaysOff
                        FROM JOURS_FERIES
                        WHERE is_deleted = 0 AND YEAR(DateJour) = @Year";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Year", currentYear);
                        cmd.Parameters.AddWithValue("@Today", DateTime.Today);

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                litTotalHolidays.Text = reader["TotalHolidays"].ToString();
                                litTotalSpecial.Text = reader["TotalSpecial"].ToString();
                                litUpcoming.Text = reader["Upcoming"].ToString();
                                litTotalDaysOff.Text = reader["TotalDaysOff"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des statistiques: " + ex.Message);
            }
        }

        #endregion

        #region Load Holidays

        private void LoadHolidays()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT
                            IdJourFerie,
                            Nom,
                            DateJour,
                            TypeJour,
                            IsPaid,
                            IsRecurrent,
                            RecurringType,
                            Description,
                            CreatedBy,
                            CreatedDate
                        FROM JOURS_FERIES
                        WHERE is_deleted = 0
                        AND YEAR(DateJour) = @Year";

                    // Apply filters
                    if (!string.IsNullOrEmpty(ddlFilterType.SelectedValue))
                    {
                        query += " AND TypeJour = @TypeJour";
                    }

                    if (!string.IsNullOrEmpty(ddlFilterStatus.SelectedValue))
                    {
                        switch (ddlFilterStatus.SelectedValue)
                        {
                            case "PAST":
                                query += " AND DateJour < @Today";
                                break;
                            case "UPCOMING":
                                query += " AND DateJour >= @Today";
                                break;
                            case "TODAY":
                                query += " AND CAST(DateJour AS DATE) = CAST(@Today AS DATE)";
                                break;
                        }
                    }

                    query += " ORDER BY DateJour ASC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Year", currentYear);
                        cmd.Parameters.AddWithValue("@Today", DateTime.Today);

                        if (!string.IsNullOrEmpty(ddlFilterType.SelectedValue))
                        {
                            cmd.Parameters.AddWithValue("@TypeJour", ddlFilterType.SelectedValue);
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            gvHolidays.DataSource = dt;
                            gvHolidays.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des jours fériés: " + ex.Message);
            }
        }

        #endregion

        #region Save Holiday

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                int editId = Convert.ToInt32(hfEditId.Value);
                bool isEdit = editId > 0;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query;

                    if (isEdit)
                    {
                        query = @"
                            UPDATE JOURS_FERIES
                            SET Nom = @Nom,
                                DateJour = @DateJour,
                                TypeJour = @TypeJour,
                                IsPaid = @IsPaid,
                                IsRecurrent = @IsRecurrent,
                                RecurringType = @RecurringType,
                                Description = @Description,
                                ModifiedBy = @UserId,
                                ModifiedDate = @ModifiedDate
                            WHERE IdJourFerie = @IdJourFerie";
                    }
                    else
                    {
                        query = @"
                            INSERT INTO JOURS_FERIES
                            (Nom, DateJour, TypeJour, IsPaid, IsRecurrent, RecurringType, Description, CreatedBy, CreatedDate, is_deleted)
                            VALUES
                            (@Nom, @DateJour, @TypeJour, @IsPaid, @IsRecurrent, @RecurringType, @Description, @UserId, @CreatedDate, 0)";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nom", txtNom.Text.Trim());
                        cmd.Parameters.AddWithValue("@DateJour", DateTime.Parse(txtDate.Text));
                        cmd.Parameters.AddWithValue("@TypeJour", ddlType.SelectedValue);
                        cmd.Parameters.AddWithValue("@IsPaid", ddlPaid.SelectedValue == "1");
                        cmd.Parameters.AddWithValue("@IsRecurrent", rbRecurrentYes.Checked);

                        // Fix C# 7.3 compatibility - explicit object cast
                        object recurringTypeValue = rbRecurrentYes.Checked ? (object)ddlRecurringType.SelectedValue : DBNull.Value;
                        cmd.Parameters.AddWithValue("@RecurringType", recurringTypeValue);

                        object descriptionValue = string.IsNullOrEmpty(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text.Trim();
                        cmd.Parameters.AddWithValue("@Description", descriptionValue);

                        cmd.Parameters.AddWithValue("@UserId", Session["EmployeId"] ?? (object)1);

                        if (isEdit)
                        {
                            cmd.Parameters.AddWithValue("@IdJourFerie", editId);
                            cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        }

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowSuccess(isEdit ? "Jour férié modifié avec succès!" : "Jour férié ajouté avec succès!");
                ClearForm();
                LoadStatistics();
                LoadHolidays();
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de l'enregistrement: " + ex.Message);
            }
        }

        #endregion

        #region Edit/Delete Holiday

        protected void gvHolidays_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int holidayId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditHoliday")
            {
                LoadHolidayForEdit(holidayId);
            }
            else if (e.CommandName == "DeleteHoliday")
            {
                DeleteHoliday(holidayId);
            }
        }

        private void LoadHolidayForEdit(int holidayId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT * FROM JOURS_FERIES
                        WHERE IdJourFerie = @IdJourFerie AND is_deleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdJourFerie", holidayId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfEditId.Value = holidayId.ToString();
                                txtNom.Text = reader["Nom"].ToString();
                                txtDate.Text = Convert.ToDateTime(reader["DateJour"]).ToString("yyyy-MM-dd");
                                ddlType.SelectedValue = reader["TypeJour"].ToString();
                                ddlPaid.SelectedValue = Convert.ToBoolean(reader["IsPaid"]) ? "1" : "0";

                                bool isRecurrent = Convert.ToBoolean(reader["IsRecurrent"]);
                                rbRecurrentYes.Checked = isRecurrent;
                                rbRecurrentNo.Checked = !isRecurrent;

                                if (isRecurrent && reader["RecurringType"] != DBNull.Value)
                                {
                                    ddlRecurringType.SelectedValue = reader["RecurringType"].ToString();
                                }

                                // Fix C# 7.3 compatibility
                                if (reader["Description"] != DBNull.Value)
                                {
                                    txtDescription.Text = reader["Description"].ToString();
                                }
                                else
                                {
                                    txtDescription.Text = "";
                                }

                                litFormTitle.Text = "Modifier Jour Férié / Jour Spécial";
                                btnSave.Text = "Mettre à jour";

                                // Scroll to form
                                ScriptManager.RegisterStartupScript(this, GetType(), "ScrollToForm",
                                    "window.scrollTo(0, 0);", true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement: " + ex.Message);
            }
        }

        private void DeleteHoliday(int holidayId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE JOURS_FERIES
                        SET is_deleted = 1,
                            ModifiedBy = @UserId,
                            ModifiedDate = @ModifiedDate
                        WHERE IdJourFerie = @IdJourFerie";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdJourFerie", holidayId);
                        cmd.Parameters.AddWithValue("@UserId", Session["EmployeId"] ?? 1);
                        cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowSuccess("Jour férié supprimé avec succès!");
                LoadStatistics();
                LoadHolidays();
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de la suppression: " + ex.Message);
            }
        }

        #endregion

        #region Year Navigation

        protected void btnPrevYear_Click(object sender, EventArgs e)
        {
            currentYear--;
            Session["SelectedYear"] = currentYear;
            InitializePage();
        }

        protected void btnCurrentYear_Click(object sender, EventArgs e)
        {
            currentYear = DateTime.Now.Year;
            Session["SelectedYear"] = currentYear;
            InitializePage();
        }

        protected void btnNextYear_Click(object sender, EventArgs e)
        {
            currentYear++;
            Session["SelectedYear"] = currentYear;
            InitializePage();
        }

        #endregion

        #region Filters

        protected void ddlFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadHolidays();
        }

        protected void ddlFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadHolidays();
        }

        #endregion

        #region Predefined Holiday Dropdown

        protected void ddlPredefinedHoliday_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedValue = ddlPredefinedHoliday.SelectedValue;

            if (string.IsNullOrEmpty(selectedValue))
            {
                // Manual entry - clear form
                ClearForm();
                return;
            }

            // Get current year from session
            int year = currentYear;
            if (Session["SelectedYear"] != null)
            {
                year = Convert.ToInt32(Session["SelectedYear"]);
            }

            // Auto-fill form based on selection
            switch (selectedValue)
            {
                case "NOUVEL_AN":
                    txtNom.Text = "Nouvel An";
                    txtDate.Text = new DateTime(year, 1, 1).ToString("yyyy-MM-dd");
                    txtDescription.Text = "Jour de l'An - Calendrier Grégorien";
                    break;
                case "MANIFESTE":
                    txtNom.Text = "Manifeste de l'Indépendance";
                    txtDate.Text = new DateTime(year, 1, 11).ToString("yyyy-MM-dd");
                    txtDescription.Text = "Commémoration du Manifeste de l'Indépendance (1944)";
                    break;
                case "FETE_TRAVAIL":
                    txtNom.Text = "Fête du Travail";
                    txtDate.Text = new DateTime(year, 5, 1).ToString("yyyy-MM-dd");
                    txtDescription.Text = "Journée Internationale des Travailleurs";
                    break;
                case "FETE_TRONE":
                    txtNom.Text = "Fête du Trône";
                    txtDate.Text = new DateTime(year, 7, 30).ToString("yyyy-MM-dd");
                    txtDescription.Text = "Anniversaire de l'intronisation de Sa Majesté le Roi Mohammed VI";
                    break;
                case "OUED_EDDAHAB":
                    txtNom.Text = "Journée de Oued Ed-Dahab";
                    txtDate.Text = new DateTime(year, 8, 14).ToString("yyyy-MM-dd");
                    txtDescription.Text = "Commémoration de la récupération de Oued Ed-Dahab (1979)";
                    break;
                case "REVOLUTION":
                    txtNom.Text = "Révolution du Roi et du Peuple";
                    txtDate.Text = new DateTime(year, 8, 20).ToString("yyyy-MM-dd");
                    txtDescription.Text = "Commémoration de la Révolution du Roi Mohammed V et du Peuple";
                    break;
                case "FETE_JEUNESSE":
                    txtNom.Text = "Fête de la Jeunesse";
                    txtDate.Text = new DateTime(year, 8, 21).ToString("yyyy-MM-dd");
                    txtDescription.Text = "Anniversaire de Sa Majesté le Roi Mohammed VI";
                    break;
                case "MARCHE_VERTE":
                    txtNom.Text = "Marche Verte";
                    txtDate.Text = new DateTime(year, 11, 6).ToString("yyyy-MM-dd");
                    txtDescription.Text = "Commémoration de la Marche Verte (1975)";
                    break;
                case "FETE_INDEPENDANCE":
                    txtNom.Text = "Fête de l'Indépendance";
                    txtDate.Text = new DateTime(year, 11, 18).ToString("yyyy-MM-dd");
                    txtDescription.Text = "Commémoration de l'Indépendance du Maroc (1956)";
                    break;
            }

            // Set defaults for official holidays
            ddlType.SelectedValue = "FERIE";
            ddlPaid.SelectedValue = "1";
            rbRecurrentYes.Checked = true;
            rbRecurrentNo.Checked = false;
            ddlRecurringType.SelectedValue = "YEARLY";
        }

        #endregion

        #region Add Predefined Holidays (Legacy - can be removed)

        protected void btnAddPredefined_Click(object sender, EventArgs e)
        {
            try
            {
                int addedCount = 0;

                // Fixed public holidays in Morocco
                var fixedHolidays = new List<(DateTime date, string name)>
                {
                    (new DateTime(currentYear, 1, 1), "Nouvel An"),
                    (new DateTime(currentYear, 1, 11), "Manifeste de l'Indépendance"),
                    (new DateTime(currentYear, 5, 1), "Fête du Travail"),
                    (new DateTime(currentYear, 7, 30), "Fête du Trône"),
                    (new DateTime(currentYear, 8, 14), "Journée de Oued Ed-Dahab"),
                    (new DateTime(currentYear, 8, 20), "Révolution du Roi et du Peuple"),
                    (new DateTime(currentYear, 8, 21), "Fête de la Jeunesse"),
                    (new DateTime(currentYear, 11, 6), "Marche Verte"),
                    (new DateTime(currentYear, 11, 18), "Fête de l'Indépendance")
                };

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (var holiday in fixedHolidays)
                    {
                        // Check if already exists
                        string checkQuery = "SELECT COUNT(*) FROM JOURS_FERIES WHERE DateJour = @DateJour AND is_deleted = 0";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@DateJour", holiday.date);
                            int exists = (int)checkCmd.ExecuteScalar();

                            if (exists == 0)
                            {
                                string insertQuery = @"
                                    INSERT INTO JOURS_FERIES
                                    (Nom, DateJour, TypeJour, IsPaid, IsRecurrent, RecurringType, Description, CreatedBy, CreatedDate, is_deleted)
                                    VALUES
                                    (@Nom, @DateJour, 'FERIE', 1, 1, 'YEARLY', 'Jour férié officiel du Maroc', @UserId, @CreatedDate, 0)";

                                using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@Nom", holiday.name);
                                    insertCmd.Parameters.AddWithValue("@DateJour", holiday.date);
                                    insertCmd.Parameters.AddWithValue("@UserId", Session["EmployeId"] ?? 1);
                                    insertCmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                                    insertCmd.ExecuteNonQuery();
                                    addedCount++;
                                }
                            }
                        }
                    }
                }

                if (addedCount > 0)
                {
                    ShowSuccess($"{addedCount} jour(s) férié(s) ajouté(s) avec succès!");
                    LoadStatistics();
                    LoadHolidays();
                }
                else
                {
                    ShowInfo("Tous les jours fériés officiels sont déjà présents pour cette année.");
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de l'ajout des jours fériés prédéfinis: " + ex.Message);
            }
        }

        #endregion

        #region Helper Methods

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfEditId.Value = "0";
            ddlPredefinedHoliday.SelectedIndex = 0; // Reset to "Saisie manuelle"
            txtNom.Text = "";
            txtDate.Text = "";
            ddlType.SelectedIndex = 0;
            ddlPaid.SelectedIndex = 0;
            rbRecurrentYes.Checked = false;
            rbRecurrentNo.Checked = true;
            ddlRecurringType.SelectedIndex = 0;
            txtDescription.Text = "";
            litFormTitle.Text = "Ajouter un Jour Férié / Jour Spécial";
            btnSave.Text = "Enregistrer";
        }

        public string GetStatusBadge(object dateJour)
        {
            DateTime date = Convert.ToDateTime(dateJour);
            DateTime today = DateTime.Today;

            if (date.Date == today)
            {
                return "<span class='badge bg-success'>Aujourd'hui</span>";
            }
            else if (date < today)
            {
                return "<span class='badge bg-secondary'>Passé</span>";
            }
            else
            {
                int daysUntil = (date - today).Days;
                if (daysUntil <= 7)
                {
                    return $"<span class='badge bg-warning'>Dans {daysUntil} jour(s)</span>";
                }
                else
                {
                    return "<span class='badge bg-info'>À venir</span>";
                }
            }
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

        private void ShowInfo(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Info",
                $"alert('{message.Replace("'", "\\'")}');", true);
        }

        #endregion
    }
}
