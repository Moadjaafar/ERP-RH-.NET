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
    public partial class PointageHeuresSupp : System.Web.UI.Page
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
                CurrentChefEquipeId = 1;
            }
        }

        #region Initialization

        private void InitializePage()
        {
            txtDateHeureSupp.Text = DateTime.Today.ToString("yyyy-MM-dd");

            // Set default day type based on current date
            DateTime currentDate = DateTime.Today;
            if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
            {
                ddlTypeJour.SelectedValue = "WEEKEND";
            }

            LoadChefEquipeInfo();

            if (CurrentChefEquipeId.HasValue)
            {
                LoadHeuresSuppData();
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

                                pnlHeuresSupp.Visible = true;
                                pnlNoTeam.Visible = false;
                            }
                            else
                            {
                                pnlHeuresSupp.Visible = false;
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

        private decimal GetDefaultCoefficient(string typeJour)
        {
            switch (typeJour)
            {
                case "WEEKEND": return 1.50m;
                case "FERIE": return 2.00m;
                default: return 1.00m; // SEMAINE
            }
        }

        #endregion

        #region Data Loading

        private void LoadHeuresSuppData()
        {
            var currentChefEquipeId = GetCurrentChefEquipeId();
            if (!currentChefEquipeId.HasValue) return;

            try
            {
                DateTime dateHeureSupp = Convert.ToDateTime(txtDateHeureSupp.Text);
                string typeJour = ddlTypeJour.SelectedValue;

                litDateHeureSupp.Text = dateHeureSupp.ToString("dd/MM/yyyy");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            e.IdEmploye,
                            e.Matricule,
                            e.Nom + ' ' + e.Prenom AS NomComplet,
                            ISNULL(hs.IdHeureSupp, 0) AS IdHeureSupp,
                            hs.HeureDebut,
                            hs.HeureFin,
                            hs.NombreHeures,
                            ISNULL(hs.Coefficient, @DefaultCoef) AS Coefficient,
                            hs.Justification
                        FROM EMPLOYE_CHEF_EQUIPE ece
                        INNER JOIN EMPLOYE e ON ece.IdEmploye = e.IdEmploye
                        LEFT JOIN POINTAGE_HEURE_SUPP hs ON e.IdEmploye = hs.IdEmploye 
                            AND hs.DateHeureSupp = @DateHeureSupp
                        WHERE ece.IdChefEquipe = @IdChefEquipe
                        AND ece.is_deleted = 0 
                        AND ece.DateFinAssignation IS NULL
                        AND e.is_deleted = 0
                        ORDER BY e.Nom, e.Prenom";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@IdChefEquipe", currentChefEquipeId.Value);
                        adapter.SelectCommand.Parameters.AddWithValue("@DateHeureSupp", dateHeureSupp);
                        adapter.SelectCommand.Parameters.AddWithValue("@DefaultCoef", GetDefaultCoefficient(typeJour));

                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            gvHeuresSupp.DataSource = dt;
                            gvHeuresSupp.DataBind();
                            pnlNoEmployees.Visible = false;
                            pnlSummary.Visible = true;
                            UpdateSummary(dt);

                            // Add JavaScript to update summary after page loads
                            ScriptManager.RegisterStartupScript(this, GetType(), "UpdateSummary",
                                "setTimeout(function() { updateSummaryCounts(); }, 100);", true);
                        }
                        else
                        {
                            gvHeuresSupp.DataSource = null;
                            gvHeuresSupp.DataBind();
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
            int totalEmployes = 0;
            decimal totalHeures = 0;
            decimal heuresPonderees = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (row["NombreHeures"] != null && row["NombreHeures"] != DBNull.Value)
                {
                    decimal heures = Convert.ToDecimal(row["NombreHeures"]);
                    if (heures > 0)
                    {
                        totalEmployes++;
                        totalHeures += heures;

                        decimal coef = Convert.ToDecimal(row["Coefficient"]);
                        heuresPonderees += heures * coef;
                    }
                }
            }

            litTotalEmployes.Text = totalEmployes.ToString();
            litTotalHeures.Text = totalHeures.ToString("0.00");
            litHeuresPonderees.Text = heuresPonderees.ToString("0.00");

            if (totalHeures > 0)
            {
                decimal coeffMoyen = (heuresPonderees / totalHeures * 100);
                litCoeffMoyen.Text = coeffMoyen.ToString("0") + "%";
            }
            else
            {
                litCoeffMoyen.Text = "100%";
            }
        }

        #endregion

        #region Event Handlers

        protected void txtDateHeureSupp_TextChanged(object sender, EventArgs e)
        {
            // Update day type based on selected date
            DateTime selectedDate;
            if (DateTime.TryParse(txtDateHeureSupp.Text, out selectedDate))
            {
                if (selectedDate.DayOfWeek == DayOfWeek.Saturday || selectedDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    ddlTypeJour.SelectedValue = "WEEKEND";
                }
                else
                {
                    // You might want to check for holidays here
                    ddlTypeJour.SelectedValue = "SEMAINE";
                }
            }

            LoadHeuresSuppData();
        }

        protected void ddlTypeJour_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadHeuresSuppData();
        }

        protected void btnCharger_Click(object sender, EventArgs e)
        {
            LoadHeuresSuppData();
        }

        protected void btnSauvegarder_Click(object sender, EventArgs e)
        {
                int savedCount = 0;
                DateTime dateHeureSupp = Convert.ToDateTime(txtDateHeureSupp.Text);

                foreach (GridViewRow row in gvHeuresSupp.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        if (SaveHeuresSuppRow(row, dateHeureSupp))
                            savedCount++;
                    }
                }

                ShowSuccess($"Heures supplémentaires sauvegardées pour {savedCount} employé(s)");
                LoadHeuresSuppData(); // Refresh data

                // Update summary after save
                ScriptManager.RegisterStartupScript(this, GetType(), "UpdateSummaryAfterSave",
                    "setTimeout(function() { updateSummaryCounts(); }, 200);", true);
        }

        protected void gvHeuresSupp_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView drv = (DataRowView)e.Row.DataItem;

                // Set coefficient dropdown value
                DropDownList ddlCoefficient = e.Row.FindControl("ddlCoefficient") as DropDownList;
                if (ddlCoefficient != null)
                {
                    decimal coef = Convert.ToDecimal(drv["Coefficient"]);
                    ddlCoefficient.SelectedValue = coef.ToString("0.00");
                }

                // Highlight rows with hours
                if (drv["NombreHeures"] != null && drv["NombreHeures"] != DBNull.Value)
                {
                    decimal heures = Convert.ToDecimal(drv["NombreHeures"]);
                    if (heures > 0)
                    {
                        e.Row.CssClass += " has-hours";
                    }
                }
            }
        }

        #endregion

        #region Helper Methods

        private bool SaveHeuresSuppRow(GridViewRow row, DateTime dateHeureSupp)
        {

                HiddenField hdnIdEmploye = row.FindControl("hdnIdEmploye") as HiddenField;
                HiddenField hdnIdHeureSupp = row.FindControl("hdnIdHeureSupp") as HiddenField;
                TextBox txtHeureDebut = row.FindControl("txtHeureDebut") as TextBox;
                TextBox txtHeureFin = row.FindControl("txtHeureFin") as TextBox;
                TextBox txtNombreHeures = row.FindControl("txtNombreHeures") as TextBox;
                DropDownList ddlCoefficient = row.FindControl("ddlCoefficient") as DropDownList;
                TextBox txtJustification = row.FindControl("txtJustification") as TextBox;

                if (hdnIdEmploye == null) return false;

                // Check if we have either time-based OR hours-based entry
                bool hasTimeEntry = !string.IsNullOrEmpty(txtHeureDebut.Text) && !string.IsNullOrEmpty(txtHeureFin.Text);

                decimal heuresValue = 0;
                bool hasHoursEntry = !string.IsNullOrEmpty(txtNombreHeures.Text) &&
                                   decimal.TryParse(txtNombreHeures.Text, out heuresValue) &&
                                   heuresValue > 0;

                // Skip if no valid entry method
                if (!hasTimeEntry && !hasHoursEntry)
                {
                    // If there's an existing record, delete it
                    int idHeureSupp = Convert.ToInt32(hdnIdHeureSupp.Value);
                    if (idHeureSupp > 0)
                    {
                        DeleteHeuresSuppRecord(idHeureSupp);
                    }
                    return false;
                }

                int employeId = Convert.ToInt32(hdnIdEmploye.Value);
                int heuresSuppId = Convert.ToInt32(hdnIdHeureSupp.Value);

                // Handle time values - can be null if hours entered directly
                TimeSpan? heureDebut = null;
                TimeSpan? heureFin = null;

                if (hasTimeEntry)
                {
                    heureDebut = TimeSpan.Parse(txtHeureDebut.Text);
                    heureFin = TimeSpan.Parse(txtHeureFin.Text);
                }

                // Get number of hours
                decimal nombreHeures = 0;
                if (hasHoursEntry)
                {
                    nombreHeures = heuresValue;
                }
                else if (hasTimeEntry)
                {
                    // Calculate from times if hours not directly entered
                    if (!string.IsNullOrEmpty(txtNombreHeures.Text))
                    {
                        nombreHeures = Convert.ToDecimal(txtNombreHeures.Text);
                    }
                }

                decimal coefficient = Convert.ToDecimal(ddlCoefficient.SelectedValue);
                string justification = txtJustification.Text.Trim();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query;

                    if (heuresSuppId == 0) // Insert new record
                    {
                        query = @"
                    INSERT INTO POINTAGE_HEURE_SUPP (
                        IdEmploye, DateHeureSupp, HeureDebut, HeureFin, 
                        NombreHeures, Coefficient, Justification, 
                        IdChefEquipe, SaisiPar, DateSaisie
                    ) VALUES (
                        @IdEmploye, @DateHeureSupp, @HeureDebut, @HeureFin,
                        @NombreHeures, @Coefficient, @Justification,
                        @IdChefEquipe, @SaisiPar, GETDATE()
                    )";
                    }
                    else // Update existing record
                    {
                        query = @"
                    UPDATE POINTAGE_HEURE_SUPP SET 
                        HeureDebut = @HeureDebut,
                        HeureFin = @HeureFin,
                        NombreHeures = @NombreHeures,
                        Coefficient = @Coefficient,
                        Justification = @Justification,
                        DateModification = GETDATE(),
                        ModifiePar = @SaisiPar
                    WHERE IdHeureSupp = @IdHeureSupp";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeId);
                        cmd.Parameters.AddWithValue("@DateHeureSupp", dateHeureSupp);

                        // Handle nullable time parameters
                        cmd.Parameters.AddWithValue("@HeureDebut", heureDebut.HasValue ? (object)heureDebut.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@HeureFin", heureFin.HasValue ? (object)heureFin.Value : DBNull.Value);

                        cmd.Parameters.AddWithValue("@NombreHeures", nombreHeures);
                        cmd.Parameters.AddWithValue("@Coefficient", coefficient);
                        cmd.Parameters.AddWithValue("@Justification", string.IsNullOrEmpty(justification) ? DBNull.Value : (object)justification);
                        cmd.Parameters.AddWithValue("@IdChefEquipe", GetCurrentChefEquipeId());
                        cmd.Parameters.AddWithValue("@SaisiPar", "Chef Equipe - " + CurrentEmployeeId);

                        if (heuresSuppId > 0)
                            cmd.Parameters.AddWithValue("@IdHeureSupp", heuresSuppId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;

        }
        private void DeleteHeuresSuppRecord(int idHeureSupp)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM POINTAGE_HEURE_SUPP WHERE IdHeureSupp = @IdHeureSupp";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdHeureSupp", idHeureSupp);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de la suppression: " + ex.Message);
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