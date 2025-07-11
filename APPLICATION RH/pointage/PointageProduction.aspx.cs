using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.pointage
{
    public partial class PointageProduction : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        private int CurrentEmployeeId => GetCurrentUserId();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InitializePage();
            }
        }

        #region Initialization

        private void InitializePage()
        {
            txtDatePointage.Text = DateTime.Today.ToString("yyyy-MM-dd");
            LoadSocietes();

            // Hide panels initially
            pnlPointage.Visible = false;
            pnlSummary.Visible = false;
            pnlNoResults.Visible = false;
            pnlFiltersSummary.Visible = false;
        }

        private void LoadSocietes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT IdSociete, NomSociete, CodeSociete 
                           FROM SOCIETE 
                           WHERE Actif = 1 
                           ORDER BY NomSociete";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            ddlSociete.Items.Clear();
                            ddlSociete.Items.Insert(0, new ListItem("-- Sélectionner Société --", ""));

                            while (reader.Read())
                            {
                                string text = $"{reader["NomSociete"]} ({reader["CodeSociete"]})";
                                string value = reader["IdSociete"].ToString();
                                ddlSociete.Items.Add(new ListItem(text, value));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des sociétés: {ex.Message}");
            }
        }

        private void LoadDepartements(int societeId)
        {
            ddlDepartement.Items.Clear();
            ddlDepartement.Items.Insert(0, new ListItem("-- Tous les Départements --", ""));

            if (societeId <= 0) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT IdDepartement, NomDepartement, CodeDepartement 
                           FROM DEPARTEMENT 
                           WHERE IdSociete = @IdSociete AND Actif = 1 
                           ORDER BY NomDepartement";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdSociete", societeId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string text = reader["CodeDepartement"] != DBNull.Value
                                    ? $"{reader["NomDepartement"]} ({reader["CodeDepartement"]})"
                                    : reader["NomDepartement"].ToString();
                                string value = reader["IdDepartement"].ToString();
                                ddlDepartement.Items.Add(new ListItem(text, value));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des départements: {ex.Message}");
            }
        }

        private void LoadServices(int departementId)
        {
            ddlService.Items.Clear();
            ddlService.Items.Insert(0, new ListItem("-- Tous les Services --", ""));

            if (departementId <= 0) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT IdService, NomService, CodeService 
                           FROM SERVICE 
                           WHERE IdDepartement = @IdDepartement AND Actif = 1 
                           ORDER BY NomService";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdDepartement", departementId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string text = reader["CodeService"] != DBNull.Value
                                    ? $"{reader["NomService"]} ({reader["CodeService"]})"
                                    : reader["NomService"].ToString();
                                string value = reader["IdService"].ToString();
                                ddlService.Items.Add(new ListItem(text, value));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des services: {ex.Message}");
            }
        }

        private void LoadCellules(int serviceId)
        {
            ddlCellule.Items.Clear();
            ddlCellule.Items.Insert(0, new ListItem("-- Toutes les Cellules --", ""));

            if (serviceId <= 0) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT IdCellule, NomCellule, CodeCellule 
                           FROM CELLULE 
                           WHERE IdService = @IdService AND Actif = 1 
                           ORDER BY NomCellule";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdService", serviceId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string text = reader["CodeCellule"] != DBNull.Value
                                    ? $"{reader["NomCellule"]} ({reader["CodeCellule"]})"
                                    : reader["NomCellule"].ToString();
                                string value = reader["IdCellule"].ToString();
                                ddlCellule.Items.Add(new ListItem(text, value));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des cellules: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers

        protected void ddlSociete_SelectedIndexChanged(object sender, EventArgs e)
        {
            int societeId = 0;
            int.TryParse(ddlSociete.SelectedValue, out societeId);

            LoadDepartements(societeId);

            // Clear subsequent dropdowns
            ddlService.Items.Clear();
            ddlService.Items.Insert(0, new ListItem("-- Tous les Services --", ""));
            ddlCellule.Items.Clear();
            ddlCellule.Items.Insert(0, new ListItem("-- Toutes les Cellules --", ""));

            // Hide results
            HideResults();
        }

        protected void ddlDepartement_SelectedIndexChanged(object sender, EventArgs e)
        {
            int departementId = 0;
            int.TryParse(ddlDepartement.SelectedValue, out departementId);

            LoadServices(departementId);

            // Clear subsequent dropdowns
            ddlCellule.Items.Clear();
            ddlCellule.Items.Insert(0, new ListItem("-- Toutes les Cellules --", ""));

            // Hide results
            HideResults();
        }

        protected void ddlService_SelectedIndexChanged(object sender, EventArgs e)
        {
            int serviceId = 0;
            int.TryParse(ddlService.SelectedValue, out serviceId);

            LoadCellules(serviceId);

            // Hide results
            HideResults();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (ValidateSearchCriteria())
            {
                LoadEmployeesData();
                UpdateActiveFiltersDisplay();
            }
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            ClearAllFilters();
            HideResults();
        }

        protected void btnSauvegarder_Click(object sender, EventArgs e)
        {
            try
            {
                int savedCount = 0;
                DateTime datePointage = Convert.ToDateTime(txtDatePointage.Text);
                string shift = ddlShift.SelectedValue;

                foreach (GridViewRow row in gvPointage.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        if (SavePointageRow(row, datePointage, shift))
                            savedCount++;
                    }
                }

                ShowSuccess($"Pointage sauvegardé pour {savedCount} employé(s)");
                LoadEmployeesData(); // Refresh data

                // Update summary after save
                ScriptManager.RegisterStartupScript(this, GetType(), "UpdateSummaryAfterSave",
                    "setTimeout(function() { updateSummaryCounts(); }, 200);", true);
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de la sauvegarde: " + ex.Message);
            }
        }

        protected void gvPointage_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView drv = (DataRowView)e.Row.DataItem;
                string statut = drv["StatutPresence"].ToString();
                string motifAbsence = drv["MotifAbsence"] != DBNull.Value ? drv["MotifAbsence"].ToString() : "";

                CheckBox chkPresent = e.Row.FindControl("chkPresent") as CheckBox;
                CheckBox chkAbsent = e.Row.FindControl("chkAbsent") as CheckBox;
                CheckBox chkDemiJournee = e.Row.FindControl("chkDemiJournee") as CheckBox;
                HiddenField hdnStatut = e.Row.FindControl("hdnStatutPresence") as HiddenField;
                DropDownList ddlMotifAbsence = e.Row.FindControl("ddlMotifAbsence") as DropDownList;
                TextBox txtMotifAutre = e.Row.FindControl("txtMotifAutre") as TextBox;

                // Set the appropriate checkbox based on current status
                if (chkPresent != null) chkPresent.Checked = (statut == "P");
                if (chkAbsent != null) chkAbsent.Checked = (statut == "A");
                if (chkDemiJournee != null) chkDemiJournee.Checked = (statut == "P/2");

                // Handle MotifAbsence dropdown
                if (ddlMotifAbsence != null && statut == "A" && !string.IsNullOrEmpty(motifAbsence))
                {
                    string[] predefinedReasons = { "Maladie", "Congé", "Formation", "Mission",
                                         "Urgence familiale", "Accident de travail", "Grève", "Autre" };

                    if (predefinedReasons.Contains(motifAbsence))
                    {
                        ddlMotifAbsence.SelectedValue = motifAbsence;
                    }
                    else
                    {
                        ddlMotifAbsence.SelectedValue = "Autre";
                        if (txtMotifAutre != null)
                        {
                            txtMotifAutre.Text = motifAbsence;
                            txtMotifAutre.Style["display"] = "block";
                        }
                    }
                }

                // Add CSS class for visual feedback
                string cssClass = GetStatusClass(statut);
                if (!string.IsNullOrEmpty(cssClass))
                {
                    e.Row.CssClass += " " + cssClass;
                }
            }
        }

        #endregion

        #region Data Loading

        private void LoadEmployeesData()
        {
            try
            {
                DateTime datePointage = Convert.ToDateTime(txtDatePointage.Text);
                string shift = ddlShift.SelectedValue;

                litDatePointage.Text = datePointage.ToString("dd/MM/yyyy");
                litShift.Text = shift;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = BuildEmployeesQuery();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        AddSearchParameters(cmd);
                        cmd.Parameters.AddWithValue("@DatePointage", datePointage);
                        cmd.Parameters.AddWithValue("@Shift", shift);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            litEmployeeCount.Text = dt.Rows.Count.ToString();

                            if (dt.Rows.Count > 0)
                            {
                                gvPointage.DataSource = dt;
                                gvPointage.DataBind();
                                pnlPointage.Visible = true;
                                pnlSummary.Visible = true;
                                pnlNoResults.Visible = false;
                                UpdateSummary(dt);

                                // Add JavaScript to update summary after page loads
                                ScriptManager.RegisterStartupScript(this, GetType(), "UpdateSummary",
                                    "setTimeout(function() { updateSummaryCounts(); }, 100);", true);
                            }
                            else
                            {
                                ShowNoResults();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des données: " + ex.Message);
            }
        }

        private string BuildEmployeesQuery()
        {
            StringBuilder query = new StringBuilder(@"
                SELECT 
                    e.IdEmploye,
                    e.Matricule,
                    e.Nom + ' ' + e.Prenom AS NomComplet,
                    s.NomSociete AS SocieteNom,
                    d.NomDepartement AS DepartementNom,
                    srv.NomService AS ServiceNom,
                    c.NomCellule AS CelluleNom,
                    ISNULL(p.IdPointage, 0) AS IdPointage,
                    ISNULL(p.StatutPresence, 'P') AS StatutPresence,
                    p.HeureEntree,
                    p.HeureSortie,
                    p.MotifAbsence,
                    ISNULL(p.Coefficient, 1.00) AS Coefficient
                FROM EMPLOYE e
                INNER JOIN SOCIETE s ON e.IdSociete = s.IdSociete
                LEFT JOIN DEPARTEMENT d ON e.IdDepartement = d.IdDepartement
                LEFT JOIN SERVICE srv ON e.IdService = srv.IdService
                LEFT JOIN CELLULE c ON e.IdCellule = c.IdCellule
                LEFT JOIN POINTAGE p ON e.IdEmploye = p.IdEmploye 
                    AND p.DatePointage = @DatePointage 
                    AND p.Shift = @Shift
                WHERE e.is_deleted = 0 AND s.Actif = 1");

            // Add dynamic where conditions
            query.Append(" AND s.IdSociete = @IdSociete");

            if (!string.IsNullOrEmpty(ddlDepartement.SelectedValue))
                query.Append(" AND e.IdDepartement = @IdDepartement");

            if (!string.IsNullOrEmpty(ddlService.SelectedValue))
                query.Append(" AND e.IdService = @IdService");

            if (!string.IsNullOrEmpty(ddlCellule.SelectedValue))
                query.Append(" AND e.IdCellule = @IdCellule");

            query.Append(" ORDER BY e.Nom, e.Prenom");

            return query.ToString();
        }

        private void AddSearchParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);

            if (!string.IsNullOrEmpty(ddlDepartement.SelectedValue))
                cmd.Parameters.AddWithValue("@IdDepartement", ddlDepartement.SelectedValue);

            if (!string.IsNullOrEmpty(ddlService.SelectedValue))
                cmd.Parameters.AddWithValue("@IdService", ddlService.SelectedValue);

            if (!string.IsNullOrEmpty(ddlCellule.SelectedValue))
                cmd.Parameters.AddWithValue("@IdCellule", ddlCellule.SelectedValue);
        }

        #endregion

        #region Helper Methods

        private bool ValidateSearchCriteria()
        {
            if (string.IsNullOrEmpty(ddlSociete.SelectedValue))
            {
                ShowError("Veuillez sélectionner une société de production.");
                return false;
            }

            if (string.IsNullOrEmpty(txtDatePointage.Text))
            {
                ShowError("Veuillez sélectionner une date.");
                return false;
            }

            return true;
        }

        private void UpdateActiveFiltersDisplay()
        {
            List<string> activeFilters = new List<string>();

            if (!string.IsNullOrEmpty(ddlSociete.SelectedValue))
                activeFilters.Add($"<span class='badge'>Société: {ddlSociete.SelectedItem.Text}</span>");

            if (!string.IsNullOrEmpty(ddlDepartement.SelectedValue))
                activeFilters.Add($"<span class='badge'>Département: {ddlDepartement.SelectedItem.Text}</span>");

            if (!string.IsNullOrEmpty(ddlService.SelectedValue))
                activeFilters.Add($"<span class='badge'>Service: {ddlService.SelectedItem.Text}</span>");

            if (!string.IsNullOrEmpty(ddlCellule.SelectedValue))
                activeFilters.Add($"<span class='badge'>Cellule: {ddlCellule.SelectedItem.Text}</span>");

            if (activeFilters.Count > 0)
            {
                litActiveFilters.Text = string.Join(" ", activeFilters);
                pnlFiltersSummary.Visible = true;
            }
            else
            {
                pnlFiltersSummary.Visible = false;
            }
        }

        private void ClearAllFilters()
        {
            ddlSociete.SelectedIndex = 0;
            ddlDepartement.Items.Clear();
            ddlDepartement.Items.Insert(0, new ListItem("-- Tous les Départements --", ""));
            ddlService.Items.Clear();
            ddlService.Items.Insert(0, new ListItem("-- Tous les Services --", ""));
            ddlCellule.Items.Clear();
            ddlCellule.Items.Insert(0, new ListItem("-- Toutes les Cellules --", ""));

            txtDatePointage.Text = DateTime.Today.ToString("yyyy-MM-dd");
            ddlShift.SelectedIndex = 0;

            litEmployeeCount.Text = "0";
            pnlFiltersSummary.Visible = false;
        }

        private void HideResults()
        {
            pnlPointage.Visible = false;
            pnlSummary.Visible = false;
            pnlNoResults.Visible = false;
            litEmployeeCount.Text = "0";
        }

        private void ShowNoResults()
        {
            pnlPointage.Visible = false;
            pnlSummary.Visible = false;
            pnlNoResults.Visible = true;
        }

        private bool SavePointageRow(GridViewRow row, DateTime datePointage, string shift)
        {
            try
            {
                HiddenField hdnIdEmploye = row.FindControl("hdnIdEmploye") as HiddenField;
                HiddenField hdnIdPointage = row.FindControl("hdnIdPointage") as HiddenField;
                HiddenField hdnStatutPresence = row.FindControl("hdnStatutPresence") as HiddenField;

                CheckBox chkPresent = row.FindControl("chkPresent") as CheckBox;
                CheckBox chkAbsent = row.FindControl("chkAbsent") as CheckBox;
                CheckBox chkDemiJournee = row.FindControl("chkDemiJournee") as CheckBox;

                DropDownList ddlMotifAbsence = row.FindControl("ddlMotifAbsence") as DropDownList;
                TextBox txtMotifAutre = row.FindControl("txtMotifAutre") as TextBox;

                if (hdnIdEmploye == null) return false;

                int employeId = Convert.ToInt32(hdnIdEmploye.Value);
                int pointageId = Convert.ToInt32(hdnIdPointage.Value);

                // Determine status
                string statut = "P";
                if (hdnStatutPresence != null && !string.IsNullOrEmpty(hdnStatutPresence.Value))
                {
                    statut = hdnStatutPresence.Value;
                }
                else
                {
                    if (chkAbsent != null && chkAbsent.Checked) statut = "A";
                    else if (chkDemiJournee != null && chkDemiJournee.Checked) statut = "P/2";
                    else statut = "P";
                }

                // Get MotifAbsence value
                string motifAbsence = null;
                if (statut == "A" && ddlMotifAbsence != null)
                {
                    if (ddlMotifAbsence.SelectedValue == "Autre" && txtMotifAutre != null && !string.IsNullOrEmpty(txtMotifAutre.Text))
                    {
                        motifAbsence = txtMotifAutre.Text.Trim();
                    }
                    else if (!string.IsNullOrEmpty(ddlMotifAbsence.SelectedValue))
                    {
                        motifAbsence = ddlMotifAbsence.SelectedValue;
                    }
                }

                decimal coefficient = statut == "P" ? 1.00m : statut == "P/2" ? 0.50m : 0.00m;

                // Set default times
                TimeSpan? heureEntree = null;
                TimeSpan? heureSortie = null;

                if (statut != "A")
                {
                    heureEntree = TimeSpan.Parse("08:00");
                    if (statut == "P")
                        heureSortie = TimeSpan.Parse("17:00");
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query;

                    if (pointageId == 0) // Insert new record
                    {
                        query = @"
                            INSERT INTO POINTAGE (
                                IdEmploye, DatePointage, Shift, StatutPresence, Coefficient,
                                HeureEntree, HeureSortie, MotifAbsence, PointePar
                            ) VALUES (
                                @IdEmploye, @DatePointage, @Shift, @StatutPresence, @Coefficient,
                                @HeureEntree, @HeureSortie, @MotifAbsence, @PointePar
                            )";
                    }
                    else // Update existing record
                    {
                        query = @"
                            UPDATE POINTAGE SET 
                                StatutPresence = @StatutPresence,
                                Coefficient = @Coefficient,
                                HeureEntree = @HeureEntree,
                                HeureSortie = @HeureSortie,
                                MotifAbsence = @MotifAbsence,
                                PointePar = @PointePar
                            WHERE IdPointage = @IdPointage";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeId);
                        cmd.Parameters.AddWithValue("@DatePointage", datePointage);
                        cmd.Parameters.AddWithValue("@Shift", shift);
                        cmd.Parameters.AddWithValue("@StatutPresence", statut);
                        cmd.Parameters.AddWithValue("@Coefficient", coefficient);
                        cmd.Parameters.AddWithValue("@HeureEntree", (object)heureEntree ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@HeureSortie", (object)heureSortie ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MotifAbsence", (object)motifAbsence ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PointePar", "Production Admin - " + CurrentEmployeeId);

                        if (pointageId > 0)
                            cmd.Parameters.AddWithValue("@IdPointage", pointageId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de la sauvegarde d'un employé: " + ex.Message);
                return false;
            }
        }

        private void UpdateSummary(DataTable dt)
        {
            int presents = 0, absents = 0, demiJournees = 0;

            foreach (DataRow row in dt.Rows)
            {
                string statut = row["StatutPresence"].ToString();
                switch (statut)
                {
                    case "P": presents++; break;
                    case "A": absents++; break;
                    case "P/2": demiJournees++; break;
                }
            }

            litPresents.Text = presents.ToString();
            litAbsents.Text = absents.ToString();
            litDemiJournees.Text = demiJournees.ToString();
            litTotal.Text = dt.Rows.Count.ToString();
        }

        protected string GetStatusClass(object statutPresence)
        {
            if (statutPresence == null) return "";

            string statut = statutPresence.ToString();
            switch (statut)
            {
                case "P": return "present";
                case "A": return "absent";
                case "P/2": return "partial";
                default: return "";
            }
        }

        protected string GetMotifAutreText(object motifAbsence)
        {
            if (motifAbsence == null || motifAbsence == DBNull.Value)
                return "";

            string motif = motifAbsence.ToString();
            string[] predefinedReasons = { "Maladie", "Congé", "Formation", "Mission",
                                          "Urgence familiale", "Accident de travail", "Grève", "Autre" };

            if (!predefinedReasons.Contains(motif))
            {
                return motif;
            }

            return "";
        }

        private int GetCurrentUserId()
        {
            if (Session["UserId"] != null)
                return Convert.ToInt32(Session["UserId"]);
            return 1; // Default for development
        }

        private void ShowSuccess(string message)
        {
            string escapedMessage = message.Replace("'", "\\'");
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowSuccessToast",
                $"Swal.fire({{ toast: true, position: 'top-end', icon: 'success', title: '{escapedMessage}', showConfirmButton: false, timer: 4000 }});", true);
        }

        private void ShowError(string message)
        {
            string escapedMessage = message.Replace("'", "\\'");
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowErrorToast",
                $"Swal.fire({{ toast: true, position: 'top-end', icon: 'error', title: '{escapedMessage}', showConfirmButton: false, timer: 4000 }});", true);
        }

        #endregion
    }
}