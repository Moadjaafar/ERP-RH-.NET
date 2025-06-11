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
    public partial class MonEquipe : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        private int CurrentEmployeeId => GetCurrentUserId();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTeamInformation();
                LoadTeamMembers();
                LoadRecentActivity();
            }
        }

        #region Data Loading Methods

        private void LoadTeamInformation()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ce.IdChefEquipe, ce.NomEquipe, ce.DateAssignation,
                               s.NomSociete,
                               ISNULL(d.NomDepartement, 'Non assigné') AS NomDepartement,
                               (SELECT COUNT(*) FROM EMPLOYE_CHEF_EQUIPE ece 
                                WHERE ece.IdChefEquipe = ce.IdChefEquipe 
                                AND ece.is_deleted = 0 
                                AND ece.DateFinAssignation IS NULL) AS NombreEmployes
                        FROM CHEF_EQUIPE ce
                        INNER JOIN SOCIETE s ON ce.IdSociete = s.IdSociete
                        LEFT JOIN DEPARTEMENT d ON ce.IdDepartement = d.IdDepartement
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
                                // Show team information
                                litNomEquipe.Text = reader["NomEquipe"].ToString();
                                litSociete.Text = reader["NomSociete"].ToString();
                                litDepartement.Text = reader["NomDepartement"].ToString();
                                litDateAssignation.Text = Convert.ToDateTime(reader["DateAssignation"]).ToString("dd/MM/yyyy");
                                litNombreEmployes.Text = reader["NombreEmployes"].ToString();

                                pnlTeamInfo.Visible = true;
                                pnlTeamMembers.Visible = true;
                                pnlRecentActivity.Visible = true;
                                pnlNoTeam.Visible = false;
                            }
                            else
                            {
                                // No team assigned
                                pnlTeamInfo.Visible = false;
                                pnlTeamMembers.Visible = false;
                                pnlRecentActivity.Visible = false;
                                pnlNoTeam.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des informations de l'équipe: " + ex.Message);
            }
        }

        private void LoadTeamMembers()
        {
            if (!pnlTeamMembers.Visible) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT e.IdEmploye, e.Matricule, 
                               e.Nom + ' ' + e.Prenom AS NomComplet,
                               e.TypeEmploye, e.Telephone,
                               ISNULL(d.NomDepartement, 'Non assigné') AS NomDepartement,
                               ece.DateAssignation
                        FROM EMPLOYE_CHEF_EQUIPE ece
                        INNER JOIN EMPLOYE e ON ece.IdEmploye = e.IdEmploye
                        LEFT JOIN DEPARTEMENT d ON e.IdDepartement = d.IdDepartement
                        INNER JOIN CHEF_EQUIPE ce ON ece.IdChefEquipe = ce.IdChefEquipe
                        WHERE ce.IdEmploye = @IdChefEquipe
                        AND ece.is_deleted = 0 
                        AND ece.DateFinAssignation IS NULL
                        AND e.is_deleted = 0
                        ORDER BY e.Nom, e.Prenom";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@IdChefEquipe", CurrentEmployeeId);

                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        gvTeamMembers.DataSource = dt;
                        gvTeamMembers.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des membres de l'équipe: " + ex.Message);
            }
        }

        private void LoadRecentActivity()
        {
            if (!pnlRecentActivity.Visible) return;

            try
            {
                StringBuilder activity = new StringBuilder();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Get recent assignments
                    string query = @"
                        SELECT TOP 5 
                               e.Nom + ' ' + e.Prenom AS NomEmploye,
                               ece.DateAssignation,
                               'Assigné' AS Action
                        FROM EMPLOYE_CHEF_EQUIPE ece
                        INNER JOIN EMPLOYE e ON ece.IdEmploye = e.IdEmploye
                        INNER JOIN CHEF_EQUIPE ce ON ece.IdChefEquipe = ce.IdChefEquipe
                        WHERE ce.IdEmploye = @IdChefEquipe
                        AND ece.is_deleted = 0
                        ORDER BY ece.DateCreation DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdChefEquipe", CurrentEmployeeId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                activity.Append("<p class='text-muted'>Aucune activité récente</p>");
                            }
                            else
                            {
                                activity.Append("<ul class='list-group list-group-flush'>");
                                while (reader.Read())
                                {
                                    string employeeName = reader["NomEmploye"].ToString();
                                    DateTime dateAction = Convert.ToDateTime(reader["DateAssignation"]);
                                    string action = reader["Action"].ToString();

                                    activity.AppendFormat(
                                        "<li class='list-group-item d-flex justify-content-between align-items-center'>" +
                                        "<div><strong>{0}</strong> - {1}</div>" +
                                        "<small class='text-muted'>{2}</small>" +
                                        "</li>",
                                        employeeName, action, dateAction.ToString("dd/MM/yyyy")
                                    );
                                }
                                activity.Append("</ul>");
                            }
                        }
                    }
                }

                litRecentActivity.Text = activity.ToString();
            }
            catch (Exception ex)
            {
                litRecentActivity.Text = "<p class='text-danger'>Erreur lors du chargement de l'activité</p>";
            }
        }

        #endregion

        #region Event Handlers

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadTeamInformation();
            LoadTeamMembers();
            LoadRecentActivity();
            ShowSuccess("Données actualisées");
        }


        protected void gvTeamMembers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int employeeId = Convert.ToInt32(e.CommandArgument);

            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de l'action: " + ex.Message);
            }
        }

        #endregion

        #region Helper Methods


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

        #endregion
    }
}