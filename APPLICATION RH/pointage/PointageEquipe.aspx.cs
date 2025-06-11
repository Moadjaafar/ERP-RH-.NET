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
    public partial class PointageEquipe : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        private int CurrentEmployeeId => GetCurrentUserId();
        private int? CurrentChefEquipeId { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InitializePage();
            }
            else
            {
                // Ensure chef equipe ID is available during postbacks
                if (Session["CurrentChefEquipeId"] != null)
                {
                    CurrentChefEquipeId = Convert.ToInt32(Session["CurrentChefEquipeId"]);
                }
            }
        }

        #region Initialization

        private void InitializePage()
        {
            txtDatePointage.Text = DateTime.Today.ToString("yyyy-MM-dd");
            LoadChefEquipeInfo();

            if (CurrentChefEquipeId.HasValue)
            {
                LoadPointageData();
            }
        }

        private void LoadChefEquipeInfo()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ce.IdChefEquipe, ce.NomEquipe, s.NomSociete
                        FROM CHEF_EQUIPE ce
                        INNER JOIN SOCIETE s ON ce.IdSociete = s.IdSociete
                        WHERE ce.IdEmploye = @IdEmploye 
                        AND ce.is_deleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", CurrentEmployeeId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                CurrentChefEquipeId = Convert.ToInt32(reader["IdChefEquipe"]);
                                Session["CurrentChefEquipeId"] = CurrentChefEquipeId;

                                litTeamName.Text = reader["NomEquipe"].ToString();

                                pnlPointage.Visible = true;
                                pnlNoTeam.Visible = false;
                            }
                            else
                            {
                                pnlPointage.Visible = false;
                                pnlNoTeam.Visible = true;
                                CurrentChefEquipeId = null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des informations: " + ex.Message);
            }
        }

        #endregion

        #region Data Loading

        private void LoadPointageData()
        {
            var currentChefEquipeId = GetCurrentChefEquipeId();
            if (!currentChefEquipeId.HasValue) return;

            try
            {
                DateTime datePointage = Convert.ToDateTime(txtDatePointage.Text);
                string shift = ddlShift.SelectedValue;

                litDatePointage.Text = datePointage.ToString("dd/MM/yyyy");
                litShift.Text = shift;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            e.IdEmploye,
                            e.Matricule,
                            e.Nom + ' ' + e.Prenom AS NomComplet,
                            ISNULL(p.IdPointage, 0) AS IdPointage,
                            ISNULL(p.StatutPresence, 'P') AS StatutPresence,
                            p.HeureEntree,
                            p.HeureSortie,
                            p.MotifAbsence,
                            ISNULL(p.Coefficient, 1.00) AS Coefficient
                        FROM EMPLOYE_CHEF_EQUIPE ece
                        INNER JOIN EMPLOYE e ON ece.IdEmploye = e.IdEmploye
                        LEFT JOIN POINTAGE p ON e.IdEmploye = p.IdEmploye 
                            AND p.DatePointage = @DatePointage 
                            AND p.Shift = @Shift
                        WHERE ece.IdChefEquipe = @IdChefEquipe
                        AND ece.is_deleted = 0 
                        AND ece.DateFinAssignation IS NULL
                        AND e.is_deleted = 0
                        ORDER BY e.Nom, e.Prenom";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@IdChefEquipe", currentChefEquipeId.Value);
                        adapter.SelectCommand.Parameters.AddWithValue("@DatePointage", datePointage);
                        adapter.SelectCommand.Parameters.AddWithValue("@Shift", shift);

                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            gvPointage.DataSource = dt;
                            gvPointage.DataBind();
                            pnlNoEmployees.Visible = false;
                            pnlSummary.Visible = true;
                            UpdateSummary(dt);

                            // Add JavaScript to update summary after page loads
                            ScriptManager.RegisterStartupScript(this, GetType(), "UpdateSummary",
                                "setTimeout(function() { updateSummaryCounts(); }, 100);", true);
                        }
                        else
                        {
                            gvPointage.DataSource = null;
                            gvPointage.DataBind();
                            pnlNoEmployees.Visible = true;
                            pnlSummary.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des données: " + ex.Message);
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

        private void UpdateSummaryFromGrid()
        {
            int presents = 0, absents = 0, demiJournees = 0, total = 0;

            foreach (GridViewRow row in gvPointage.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    total++;

                    CheckBox chkPresent = row.FindControl("chkPresent") as CheckBox;
                    CheckBox chkAbsent = row.FindControl("chkAbsent") as CheckBox;
                    CheckBox chkDemiJournee = row.FindControl("chkDemiJournee") as CheckBox;
                    HiddenField hdnStatut = row.FindControl("hdnStatutPresence") as HiddenField;

                    string statut = "P"; // Default
                    if (hdnStatut != null && !string.IsNullOrEmpty(hdnStatut.Value))
                    {
                        statut = hdnStatut.Value;
                    }
                    else
                    {
                        if (chkAbsent != null && chkAbsent.Checked) statut = "A";
                        else if (chkDemiJournee != null && chkDemiJournee.Checked) statut = "P/2";
                        else statut = "P";
                    }

                    switch (statut)
                    {
                        case "P": presents++; break;
                        case "A": absents++; break;
                        case "P/2": demiJournees++; break;
                    }
                }
            }

            litPresents.Text = presents.ToString();
            litAbsents.Text = absents.ToString();
            litDemiJournees.Text = demiJournees.ToString();
            litTotal.Text = total.ToString();
        }

        #endregion

        #region Event Handlers

        protected void txtDatePointage_TextChanged(object sender, EventArgs e)
        {
            LoadPointageData();
        }

        protected void ddlShift_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPointageData();
        }

        protected void btnCharger_Click(object sender, EventArgs e)
        {
            LoadPointageData();
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
                LoadPointageData(); // Refresh data

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
                    // List of predefined reasons
                    string[] predefinedReasons = { "Maladie", "Congé", "Formation", "Mission",
                                         "Urgence familiale", "Accident de travail", "Grève", "Autre" };

                    if (predefinedReasons.Contains(motifAbsence))
                    {
                        ddlMotifAbsence.SelectedValue = motifAbsence;
                    }
                    else
                    {
                        // It's a custom reason
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

        #region Helper Methods

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

                // Get MotifAbsence controls
                DropDownList ddlMotifAbsence = row.FindControl("ddlMotifAbsence") as DropDownList;
                TextBox txtMotifAutre = row.FindControl("txtMotifAutre") as TextBox;

                if (hdnIdEmploye == null) return false;

                int employeId = Convert.ToInt32(hdnIdEmploye.Value);
                int pointageId = Convert.ToInt32(hdnIdPointage.Value);

                // Determine status from checkboxes or hidden field
                string statut = "P"; // Default
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

                // Set default times for present employees
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
                        HeureEntree, HeureSortie, MotifAbsence, IdChefEquipe, PointePar
                    ) VALUES (
                        @IdEmploye, @DatePointage, @Shift, @StatutPresence, @Coefficient,
                        @HeureEntree, @HeureSortie, @MotifAbsence, @IdChefEquipe, @PointePar
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
                        IdChefEquipe = @IdChefEquipe,
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
                        cmd.Parameters.AddWithValue("@IdChefEquipe", GetCurrentChefEquipeId());
                        cmd.Parameters.AddWithValue("@PointePar", "Chef Equipe - " + CurrentEmployeeId);

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

        protected string FormatTime(object timeValue)
        {
            if (timeValue == null || timeValue == DBNull.Value)
                return "";

            if (timeValue is TimeSpan timeSpan)
                return timeSpan.ToString(@"hh\:mm");

            if (timeValue is DateTime dateTime)
                return dateTime.ToString("HH:mm");

            return timeValue.ToString();
        }

        private int GetCurrentUserId()
        {
            // Get from session/authentication
            if (Session["UserId"] != null)
                return Convert.ToInt32(Session["UserId"]);

            // For development/testing - replace with actual authentication
            return 1;
        }

        private void ShowSuccess(string message)
        {
            // Escape single quotes for JavaScript
            string escapedMessage = message.Replace("'", "\\'");
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowSuccessToast",
                $"Swal.fire({{ toast: true, position: 'top-end', icon: 'success', title: '{escapedMessage}', showConfirmButton: false, timer: 4000 }});", true);
        }

        private void ShowError(string message)
        {
            // Escape single quotes for JavaScript
            string escapedMessage = message.Replace("'", "\\'");
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowErrorToast",
                $"Swal.fire({{ toast: true, position: 'top-end', icon: 'error', title: '{escapedMessage}', showConfirmButton: false, timer: 4000 }});", true);
        }

        protected string GetMotifAutreText(object motifAbsence)
        {
            if (motifAbsence == null || motifAbsence == DBNull.Value)
                return "";

            string motif = motifAbsence.ToString();

            // List of predefined reasons
            string[] predefinedReasons = { "Maladie", "Congé", "Formation", "Mission", "Urgence familiale", "Accident de travail", "Grève", "Autre" };

            // If it's not a predefined reason, it's a custom reason
            if (!predefinedReasons.Contains(motif))
            {
                return motif;
            }

            return "";
        }

        #endregion

        #region Properties

        private int? GetCurrentChefEquipeId()
        {
            if (Session["CurrentChefEquipeId"] != null)
                return Convert.ToInt32(Session["CurrentChefEquipeId"]);
            return CurrentChefEquipeId ?? 1;
        }

        #endregion
    }
}