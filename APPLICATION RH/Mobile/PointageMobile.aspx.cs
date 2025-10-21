using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.Mobile
{
    public partial class PointageMobile : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDatesSummary();
                panelDetails.Visible = false;
            }
        }

        private void LoadDatesSummary()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            CAST(DatePointage as DATE) as DatePointage,
                            COUNT(*) as Total,
                            SUM(CASE WHEN Statut = 'present' THEN 1 ELSE 0 END) as Present,
                            SUM(CASE WHEN Statut = 'absent' THEN 1 ELSE 0 END) as Absent,
                            SUM(CASE WHEN Statut = 'demi_journee' THEN 1 ELSE 0 END) as DemiJournee,
                            SUM(CASE WHEN DateReception IS NOT NULL THEN 1 ELSE 0 END) as Traite,
                            SUM(CASE WHEN DateReception IS NULL THEN 1 ELSE 0 END) as EnAttente
                        FROM POINTAGE_MOBILE
                        WHERE is_deleted = 0
                        GROUP BY CAST(DatePointage as DATE)
                        ORDER BY CAST(DatePointage as DATE) DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvDates.DataSource = dt;
                    gvDates.DataBind();

                    lblTotal.Text = $"Total: {dt.Rows.Count} dates";
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Erreur: " + ex.Message, "error");
            }
        }

        protected void gvDates_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gvDates.SelectedIndex >= 0)
            {
                DateTime selectedDate = Convert.ToDateTime(gvDates.DataKeys[gvDates.SelectedIndex].Value);
                LoadPointagesForDate(selectedDate);

                lblSelectedDate.Text = "Pointages du " + selectedDate.ToString("dd/MM/yyyy");
                panelDetails.Visible = true;
            }
        }

        private void LoadPointagesForDate(DateTime selectedDate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            IdPointageMobile,
                            Matricule,
                            NomComplet,
                            Statut,
                            DateCreation,
                            DateExport,
                            DateReception,
                            DeviceInfo,
                            CASE 
                                WHEN DateReception IS NOT NULL THEN 'Traité'
                                WHEN DateExport IS NOT NULL THEN 'Exporté'
                                ELSE 'En attente'
                            END as StatutTraitement
                        FROM POINTAGE_MOBILE
                        WHERE is_deleted = 0 
                        AND CAST(DatePointage as DATE) = @SelectedDate
                        ORDER BY NomComplet";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SelectedDate", selectedDate);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvDetails.DataSource = dt;
                    gvDetails.DataBind();

                    ViewState["SelectedDate"] = selectedDate;
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Erreur: " + ex.Message, "error");
            }
        }

        protected void gvDetails_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idPointage = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Valider")
            {
                ValiderPointage(idPointage);
            }
            else if (e.CommandName == "Supprimer")
            {
                SupprimerPointage(idPointage);
            }
        }

        private void ValiderPointage(int idPointage)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE POINTAGE_MOBILE 
                        SET DateReception = GETDATE(), 
                            IdUtilisateurExport = @UserId,
                            updated_at = GETDATE()
                        WHERE IdPointageMobile = @IdPointage";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@IdPointage", idPointage);
                    cmd.Parameters.AddWithValue("@UserId", Session["UserId"] ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowMessage("Pointage validé!", "success");
                RefreshCurrentView();
            }
            catch (Exception ex)
            {
                ShowMessage("Erreur: " + ex.Message, "error");
            }
        }

        private void SupprimerPointage(int idPointage)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE POINTAGE_MOBILE 
                        SET is_deleted = 1, updated_at = GETDATE() 
                        WHERE IdPointageMobile = @IdPointage";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@IdPointage", idPointage);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowMessage("Pointage supprimé!", "success");
                RefreshCurrentView();
            }
            catch (Exception ex)
            {
                ShowMessage("Erreur: " + ex.Message, "error");
            }
        }

        protected void btnValiderDate_Click(object sender, EventArgs e)
        {
            if (ViewState["SelectedDate"] != null)
            {
                DateTime selectedDate = (DateTime)ViewState["SelectedDate"];

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = @"
                            UPDATE POINTAGE_MOBILE 
                            SET DateReception = GETDATE(), 
                                IdUtilisateurExport = @UserId,
                                updated_at = GETDATE()
                            WHERE CAST(DatePointage as DATE) = @SelectedDate 
                            AND DateReception IS NULL AND is_deleted = 0";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@SelectedDate", selectedDate);
                        cmd.Parameters.AddWithValue("@UserId", Session["UserId"] ?? DBNull.Value);

                        conn.Open();
                        int count = cmd.ExecuteNonQuery();

                        ShowMessage($"{count} pointages validés!", "success");
                        RefreshCurrentView();
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage("Erreur: " + ex.Message, "error");
                }
            }
        }

        protected void btnFermerDetails_Click(object sender, EventArgs e)
        {
            panelDetails.Visible = false;
            gvDates.SelectedIndex = -1;
        }

        private void RefreshCurrentView()
        {
            LoadDatesSummary();

            if (ViewState["SelectedDate"] != null)
            {
                DateTime selectedDate = (DateTime)ViewState["SelectedDate"];
                LoadPointagesForDate(selectedDate);
            }
        }

        private void ShowMessage(string message, string type)
        {
            string icon = type == "success" ? "success" : "error";
            ScriptManager.RegisterStartupScript(this, GetType(), "showMessage",
                $"Swal.fire('{message}', '', '{icon}');", true);
        }
    }
}


