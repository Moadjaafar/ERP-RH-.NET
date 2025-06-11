using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.pointage
{
    public partial class SupervisionPointage : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        private int CurrentUserId => GetCurrentUserId();

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
        }

        private void LoadSocietes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT IdSociete, NomSociete FROM SOCIETE WHERE Actif = 1 ORDER BY NomSociete";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        ddlSociete.DataSource = cmd.ExecuteReader();
                        ddlSociete.DataTextField = "NomSociete";
                        ddlSociete.DataValueField = "IdSociete";
                        ddlSociete.DataBind();
                        ddlSociete.Items.Insert(0, new ListItem("-- Sélectionner --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des sociétés: " + ex.Message);
            }
        }

        private void LoadEquipes()
        {
            ddlEquipe.Items.Clear();
            ddlEquipe.Items.Insert(0, new ListItem("-- Toutes les équipes --", ""));

            if (string.IsNullOrEmpty(ddlSociete.SelectedValue)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ce.IdChefEquipe, ce.NomEquipe,
                               e.Nom + ' ' + e.Prenom + ' - ' + ce.NomEquipe AS DisplayText,
                               (SELECT COUNT(*) FROM EMPLOYE_CHEF_EQUIPE ece 
                                WHERE ece.IdChefEquipe = ce.IdChefEquipe 
                                AND ece.is_deleted = 0 
                                AND ece.DateFinAssignation IS NULL) AS NombreEmployes
                        FROM CHEF_EQUIPE ce
                        INNER JOIN EMPLOYE e ON ce.IdEmploye = e.IdEmploye
                        WHERE ce.IdSociete = @IdSociete 
                        AND ce.is_deleted = 0
                        ORDER BY ce.NomEquipe";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string displayText = $"{reader["DisplayText"]} ({reader["NombreEmployes"]} membres)";
                                ddlEquipe.Items.Add(new ListItem(displayText, reader["IdChefEquipe"].ToString()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des équipes: " + ex.Message);
            }
        }

        #endregion

        #region Data Loading

        private void LoadSupervisionData()
        {
            if (string.IsNullOrEmpty(ddlSociete.SelectedValue) || string.IsNullOrEmpty(txtDatePointage.Text))
            {
                pnlResults.Visible = false;
                return;
            }

            try
            {
                DateTime datePointage = Convert.ToDateTime(txtDatePointage.Text);
                string shift = ddlShift.SelectedValue;
                string equipeFilter = ddlEquipe.SelectedValue;

                litDateResults.Text = datePointage.ToString("dd/MM/yyyy");

                if (!string.IsNullOrEmpty(shift))
                {
                    litShiftResults.Text = shift;
                    pnlShiftInfo.Visible = true;
                }
                else
                {
                    pnlShiftInfo.Visible = false;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // ✅ CORRECTED SQL Query
                    string query = @"
                SELECT 
                    e.IdEmploye,
                    e.Matricule,
                    e.Nom + ' ' + e.Prenom AS NomComplet,
                    ce.IdChefEquipe,
                    ce.NomEquipe,
                    chef.Nom + ' ' + chef.Prenom AS NomChef,
                    ISNULL(p.IdPointage, 0) AS IdPointage,
                    CASE 
                        WHEN p.IdPointage IS NULL THEN 'P'  -- Default to Present if no record
                        ELSE ISNULL(p.StatutPresence, 'P')
                    END AS StatutPresence,
                    p.HeureEntree,
                    p.HeureSortie,
                    p.MotifAbsence,
                    CASE 
                        WHEN p.IdPointage IS NULL THEN 1.00
                        ELSE ISNULL(p.Coefficient, 1.00)
                    END AS Coefficient,
                    p.PointePar,
                    ISNULL(p.Shift, @DefaultShift) AS Shift,
                    p.DatePointage,  -- ✅ CORRECTED: Use DatePointage instead of DateCreationPointage
                    ece.DateAssignation,
                    ece.DateFinAssignation
                FROM EMPLOYE_CHEF_EQUIPE ece
                INNER JOIN EMPLOYE e ON ece.IdEmploye = e.IdEmploye AND e.is_deleted = 0
                INNER JOIN CHEF_EQUIPE ce ON ece.IdChefEquipe = ce.IdChefEquipe AND ce.is_deleted = 0
                INNER JOIN EMPLOYE chef ON ce.IdEmploye = chef.IdEmploye
                LEFT JOIN POINTAGE p ON e.IdEmploye = p.IdEmploye 
                    AND p.DatePointage = @DatePointage  -- ✅ CORRECTED: Use DatePointage
                    AND (@Shift IS NULL OR p.Shift = @Shift OR p.Shift IS NULL)
                WHERE ce.IdSociete = @IdSociete
                    AND ece.is_deleted = 0 
                    AND (ece.DateFinAssignation IS NULL OR ece.DateFinAssignation >= @DatePointage)  -- ✅ Active assignments only
                    AND ece.DateAssignation <= @DatePointage  -- ✅ Assignment started before or on this date";

                    // Add team filter if specified
                    if (!string.IsNullOrEmpty(equipeFilter))
                    {
                        query += " AND ce.IdChefEquipe = @IdEquipe";
                    }

                    query += " ORDER BY ce.NomEquipe, e.Nom, e.Prenom";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);
                        adapter.SelectCommand.Parameters.AddWithValue("@DatePointage", datePointage);

                        // ✅ IMPROVED: Default shift handling
                        string defaultShift = !string.IsNullOrEmpty(shift) ? shift : "JOUR";
                        adapter.SelectCommand.Parameters.AddWithValue("@DefaultShift", defaultShift);

                        // Handle null shift parameter
                        if (!string.IsNullOrEmpty(shift))
                            adapter.SelectCommand.Parameters.AddWithValue("@Shift", shift);
                        else
                            adapter.SelectCommand.Parameters.AddWithValue("@Shift", DBNull.Value);

                        if (!string.IsNullOrEmpty(equipeFilter))
                            adapter.SelectCommand.Parameters.AddWithValue("@IdEquipe", equipeFilter);

                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        // ✅ DEBUG: Log the data retrieved
                        System.Diagnostics.Debug.WriteLine($"Query returned {dt.Rows.Count} rows for date {datePointage:yyyy-MM-dd}");

                        if (dt.Rows.Count > 0)
                        {
                            // Debug: Show first few rows
                            for (int i = 0; i < Math.Min(3, dt.Rows.Count); i++)
                            {
                                var row = dt.Rows[i];
                                System.Diagnostics.Debug.WriteLine($"Row {i}: Employee={row["Matricule"]}, Team={row["NomEquipe"]}, Status={row["StatutPresence"]}, PointageId={row["IdPointage"]}");
                            }

                            // Load main grid
                            gvSupervisionPointage.DataSource = dt;
                            gvSupervisionPointage.DataBind();

                            // Load team summaries
                            LoadTeamsSummary(dt);

                            // Update global summary
                            UpdateGlobalSummary(dt);

                            pnlResults.Visible = true;
                            pnlNoResults.Visible = false;
                            pnlGlobalSummary.Visible = true;

                            // Add JavaScript to update summary after page loads
                            ScriptManager.RegisterStartupScript(this, GetType(), "UpdateSummary",
                                "setTimeout(function() { updateGlobalSummary(); }, 100);", true);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("No data found - checking parameters:");
                            System.Diagnostics.Debug.WriteLine($"  IdSociete: {ddlSociete.SelectedValue}");
                            System.Diagnostics.Debug.WriteLine($"  DatePointage: {datePointage:yyyy-MM-dd}");
                            System.Diagnostics.Debug.WriteLine($"  Shift: {shift ?? "NULL"}");

                            pnlResults.Visible = true;
                            pnlNoResults.Visible = true;
                            pnlGlobalSummary.Visible = false;
                            gvSupervisionPointage.DataSource = null;
                            gvSupervisionPointage.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadSupervisionData Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Erreur lors du chargement des données: " + ex.Message);
            }
        }

        private void LoadTeamsSummary(DataTable dt)
        {
            var teamSummary = dt.AsEnumerable()
                .GroupBy(row => new
                {
                    IdChefEquipe = row.Field<int>("IdChefEquipe"),
                    NomEquipe = row.Field<string>("NomEquipe"),
                    NomChef = row.Field<string>("NomChef")
                })
                .Select(group => new
                {
                    IdChefEquipe = group.Key.IdChefEquipe,
                    NomEquipe = group.Key.NomEquipe,
                    NomChef = group.Key.NomChef,
                    Total = group.Count(),
            // ✅ IMPROVED: Better handling of NULL and default values
            Presents = group.Count(row =>
                    {
                        var status = row["StatutPresence"];
                        if (status == DBNull.Value || status == null) return true; // Default to present
                return row.Field<string>("StatutPresence") == "P";
                    }),
                    Absents = group.Count(row =>
                    {
                        var status = row["StatutPresence"];
                        if (status == DBNull.Value || status == null) return false;
                        return row.Field<string>("StatutPresence") == "A";
                    }),
                    DemiJournees = group.Count(row =>
                    {
                        var status = row["StatutPresence"];
                        if (status == DBNull.Value || status == null) return false;
                        return row.Field<string>("StatutPresence") == "P/2";
                    })
                })
                .OrderBy(team => team.NomEquipe)
                .ToList();

            // ✅ DEBUG: Log team summary data
            System.Diagnostics.Debug.WriteLine($"Team Summary: {teamSummary.Count} teams found");
            foreach (var team in teamSummary)
            {
                System.Diagnostics.Debug.WriteLine($"  {team.NomEquipe}: {team.Presents}P + {team.Absents}A + {team.DemiJournees}D = {team.Total} total");
            }

            rptTeamsSummary.DataSource = teamSummary;
            rptTeamsSummary.DataBind();
        }

        private void UpdateGlobalSummary(DataTable dt)
        {
            var teamCount = dt.AsEnumerable()
                .Select(row => row.Field<int>("IdChefEquipe"))
                .Distinct()
                .Count();

            int total = dt.Rows.Count;

            // ✅ IMPROVED: Better handling of NULL values and defaults
            int presents = dt.AsEnumerable().Count(row =>
            {
                var status = row["StatutPresence"];
                if (status == DBNull.Value || status == null) return true; // Default to present
                return row.Field<string>("StatutPresence") == "P";
            });

            int absents = dt.AsEnumerable().Count(row =>
            {
                var status = row["StatutPresence"];
                if (status == DBNull.Value || status == null) return false;
                return row.Field<string>("StatutPresence") == "A";
            });

            int demiJournees = dt.AsEnumerable().Count(row =>
            {
                var status = row["StatutPresence"];
                if (status == DBNull.Value || status == null) return false;
                return row.Field<string>("StatutPresence") == "P/2";
            });

            // ✅ DEBUG: Log summary calculations
            System.Diagnostics.Debug.WriteLine($"Global Summary: Teams={teamCount}, Total={total}, P={presents}, A={absents}, P/2={demiJournees}");

            litTotalEquipes.Text = teamCount.ToString();
            litTotalEmployes.Text = total.ToString();
            litTotalPresents.Text = presents.ToString();
            litTotalAbsents.Text = absents.ToString();
            litTotalDemiJournees.Text = demiJournees.ToString();

            decimal tauxPresence = total > 0 ? Math.Round((decimal)(presents + (demiJournees * 0.5)) / total * 100, 1) : 0;
            litTauxPresence.Text = tauxPresence.ToString("0.0") + "%";
        }
        #endregion

        #region Event Handlers

        protected void ddlSociete_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadEquipes();
            pnlResults.Visible = false;
        }

        protected void txtDatePointage_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlSociete.SelectedValue))
                LoadSupervisionData();
        }

        protected void ddlShift_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlSociete.SelectedValue))
                LoadSupervisionData();
        }

        protected void ddlEquipe_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlSociete.SelectedValue))
                LoadSupervisionData();
        }

        protected void btnRechercher_Click(object sender, EventArgs e)
        {
            LoadSupervisionData();
        }

        protected void btnSauvegarderTout_Click(object sender, EventArgs e)
        {
            try
            {
                int savedCount = 0;
                int correctedCount = 0;
                DateTime datePointage = Convert.ToDateTime(txtDatePointage.Text);

                System.Diagnostics.Debug.WriteLine($"Starting save operation for date: {datePointage}, Rows: {gvSupervisionPointage.Rows.Count}");

                foreach (GridViewRow row in gvSupervisionPointage.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        var result = SaveSupervisionPointageRow(row, datePointage);
                        if (result.Saved)
                        {
                            savedCount++;
                            if (result.WasModified)
                                correctedCount++;
                        }
                    }
                }

                string message = $"Pointage sauvegardé pour {savedCount} employé(s)";
                if (correctedCount > 0)
                    message += $" ({correctedCount} correction(s))";

                ShowSuccess(message);
                LoadSupervisionData(); // Refresh data
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de la sauvegarde: " + ex.Message);
                System.Diagnostics.Debug.WriteLine($"Save error: {ex.Message}");
            }
        }

        protected void gvSupervisionPointage_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView drv = (DataRowView)e.Row.DataItem;

                // FIX: Handle NULL values for StatutPresence
                string statut = "P"; // Default value
                if (drv["StatutPresence"] != DBNull.Value)
                {
                    statut = drv["StatutPresence"].ToString();
                }

                string motifAbsence = drv["MotifAbsence"] != DBNull.Value ? drv["MotifAbsence"].ToString() : "";

                CheckBox chkPresent = e.Row.FindControl("chkPresent") as CheckBox;
                CheckBox chkAbsent = e.Row.FindControl("chkAbsent") as CheckBox;
                CheckBox chkDemiJournee = e.Row.FindControl("chkDemiJournee") as CheckBox;
                HiddenField hdnOriginalStatut = e.Row.FindControl("hdnOriginalStatut") as HiddenField;
                HiddenField hdnStatutPresence = e.Row.FindControl("hdnStatutPresence") as HiddenField;
                DropDownList ddlMotifAbsence = e.Row.FindControl("ddlMotifAbsence") as DropDownList;
                TextBox txtMotifAutre = e.Row.FindControl("txtMotifAutre") as TextBox;

                // Set the original status and current status in hidden fields
                if (hdnOriginalStatut != null)
                {
                    hdnOriginalStatut.Value = statut;
                }

                if (hdnStatutPresence != null)
                {
                    hdnStatutPresence.Value = statut;
                }

                // Set checkboxes based on current status
                if (chkPresent != null) chkPresent.Checked = (statut == "P");
                if (chkAbsent != null) chkAbsent.Checked = (statut == "A");
                if (chkDemiJournee != null) chkDemiJournee.Checked = (statut == "P/2");

                // Handle motif dropdown for absent employees
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

                System.Diagnostics.Debug.WriteLine($"Row bound - Employee: {drv["Matricule"]}, Status: {statut}, IdPointage: {drv["IdPointage"]}");
            }
        }

        #endregion

        #region Helper Methods

        private (bool Saved, bool WasModified) SaveSupervisionPointageRow(GridViewRow row, DateTime datePointage)
        {
            try
            {
                HiddenField hdnIdEmploye = row.FindControl("hdnIdEmploye") as HiddenField;
                HiddenField hdnIdPointage = row.FindControl("hdnIdPointage") as HiddenField;
                HiddenField hdnStatutPresence = row.FindControl("hdnStatutPresence") as HiddenField;
                HiddenField hdnIdChefEquipe = row.FindControl("hdnIdChefEquipe") as HiddenField;
                HiddenField hdnOriginalStatut = row.FindControl("hdnOriginalStatut") as HiddenField;

                CheckBox chkPresent = row.FindControl("chkPresent") as CheckBox;
                CheckBox chkAbsent = row.FindControl("chkAbsent") as CheckBox;
                CheckBox chkDemiJournee = row.FindControl("chkDemiJournee") as CheckBox;

                DropDownList ddlMotifAbsence = row.FindControl("ddlMotifAbsence") as DropDownList;
                TextBox txtMotifAutre = row.FindControl("txtMotifAutre") as TextBox;

                if (hdnIdEmploye == null)
                {
                    System.Diagnostics.Debug.WriteLine("hdnIdEmploye is null");
                    return (false, false);
                }

                int employeId = Convert.ToInt32(hdnIdEmploye.Value);
                int pointageId = Convert.ToInt32(hdnIdPointage.Value);
                int chefEquipeId = Convert.ToInt32(hdnIdChefEquipe.Value);

                // Determine current status from checkboxes
                string statut = "P"; // Default
                if (chkAbsent != null && chkAbsent.Checked) statut = "A";
                else if (chkDemiJournee != null && chkDemiJournee.Checked) statut = "P/2";
                else if (chkPresent != null && chkPresent.Checked) statut = "P";

                // Check if this is a modification from original
                string originalStatut = hdnOriginalStatut?.Value ?? "P";
                bool wasModified = originalStatut != statut;

                System.Diagnostics.Debug.WriteLine($"Employee {employeId}: Original={originalStatut}, Current={statut}, Modified={wasModified}, PointageId={pointageId}");

                // Get shift - use the one from dropdown or determine from existing data
                string shift = !string.IsNullOrEmpty(ddlShift.SelectedValue) ? ddlShift.SelectedValue : "JOUR";

                // Get motif absence
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
                    else if (statut == "P/2")
                        heureSortie = TimeSpan.Parse("12:30");
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query;
                    string pointePar = wasModified ? $"SUPERVISION - {CurrentUserId}" : $"Chef Equipe - {chefEquipeId}";

                    if (pointageId == 0) // Insert new record
                    {
                        // ✅ CORRECTED: Use DatePointage instead of DateCreationPointage
                        query = @"
                    INSERT INTO POINTAGE (
                        IdEmploye, DatePointage, Shift, StatutPresence, Coefficient,
                        HeureEntree, HeureSortie, MotifAbsence, IdChefEquipe, PointePar,
                        DateCreationPointage, HeureCreationPointage
                    ) VALUES (
                        @IdEmploye, @DatePointage, @Shift, @StatutPresence, @Coefficient,
                        @HeureEntree, @HeureSortie, @MotifAbsence, @IdChefEquipe, @PointePar,
                        GETDATE(), CAST(GETDATE() AS TIME)
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
                        PointePar = @PointePar,
                        IdChefEquipe = @IdChefEquipe,
                        Shift = @Shift
                    WHERE IdPointage = @IdPointage";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeId);
                        cmd.Parameters.AddWithValue("@DatePointage", datePointage);  // ✅ CORRECTED: Use DatePointage
                        cmd.Parameters.AddWithValue("@Shift", shift);
                        cmd.Parameters.AddWithValue("@StatutPresence", statut);
                        cmd.Parameters.AddWithValue("@Coefficient", coefficient);
                        cmd.Parameters.AddWithValue("@HeureEntree", (object)heureEntree ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@HeureSortie", (object)heureSortie ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MotifAbsence", (object)motifAbsence ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IdChefEquipe", chefEquipeId);
                        cmd.Parameters.AddWithValue("@PointePar", pointePar);

                        if (pointageId > 0)
                            cmd.Parameters.AddWithValue("@IdPointage", pointageId);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        System.Diagnostics.Debug.WriteLine($"SQL executed for employee {employeId}, rows affected: {rowsAffected}");

                        // ✅ ADDED: Log the SQL command for debugging
                        System.Diagnostics.Debug.WriteLine($"SQL Command: {cmd.CommandText}");
                        foreach (SqlParameter param in cmd.Parameters)
                        {
                            System.Diagnostics.Debug.WriteLine($"  {param.ParameterName}: {param.Value}");
                        }
                    }
                }

                return (true, wasModified);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save error for row: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ShowError("Erreur lors de la sauvegarde d'un employé: " + ex.Message);
                return (false, false);
            }
        }

        protected string GetStatusClass(object statutPresence)
        {
            if (statutPresence == null || statutPresence == DBNull.Value)
                return "present"; // Default to present styling

            string statut = statutPresence.ToString();
            switch (statut)
            {
                case "P": return "present";
                case "A": return "absent";
                case "P/2": return "partial";
                default: return "present";
            }
        }

        protected string GetMotifAutreText(object motifAbsence)
        {
            if (motifAbsence == null || motifAbsence == DBNull.Value)
                return "";

            string motif = motifAbsence.ToString();
            string[] predefinedReasons = { "Maladie", "Congé", "Formation", "Mission", "Urgence familiale", "Accident de travail", "Grève", "Autre" };

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
            return 1;
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