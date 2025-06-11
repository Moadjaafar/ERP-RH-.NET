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
    public partial class CreerChefEquipe : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSocietes();
                txtDateAssignation.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadChefEquipesList();
            }
        }

        #region Load Data Methods

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
                        ddlSociete.Items.Insert(0, new ListItem("-- Sélectionner Société --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des sociétés: " + ex.Message);
            }
        }

        private void SearchEmployees()
        {
            ddlEmploye.Items.Clear();
            ddlEmploye.Items.Insert(0, new ListItem("-- Sélectionner Employé --", ""));
            pnlEmployeeSelection.Visible = false;
            pnlEmployeeInfo.Visible = false;

            if (string.IsNullOrEmpty(ddlSociete.SelectedValue))
            {
                ShowError("Veuillez d'abord sélectionner une société");
                return;
            }

            string searchTerm = txtSearchEmployee.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                ShowError("Veuillez saisir un terme de recherche");
                return;
            }

            // Check if searching by matricule (if search term is numeric or matches matricule pattern)
            bool isMatriculeSearch = IsNumeric(searchTerm) || searchTerm.Length >= 3;

            // For name search, require at least 2 characters
            if (!isMatriculeSearch && searchTerm.Length < 2)
            {
                ShowError("Veuillez saisir au moins 2 caractères pour la recherche par nom");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                SELECT e.IdEmploye, e.Matricule, e.Nom, e.Prenom,
                       e.Nom + ' ' + e.Prenom + ' (' + e.Matricule + ')' AS NomComplet
                FROM EMPLOYE e
                WHERE e.IdSociete = @IdSociete 
                AND e.is_deleted = 0
                AND e.DateSortie IS NULL
                AND e.IdEmploye NOT IN (SELECT IdEmploye FROM CHEF_EQUIPE WHERE is_deleted = 0)
                AND (
                    e.Nom LIKE @SearchTerm 
                    OR e.Prenom LIKE @SearchTerm 
                    OR e.Matricule LIKE @MatriculeSearch
                    OR e.Matricule = @ExactMatricule
                )
                ORDER BY 
                    CASE 
                        WHEN e.Matricule = @ExactMatricule THEN 1
                        WHEN e.Matricule LIKE @MatriculeSearch THEN 2
                        WHEN e.Nom LIKE @SearchTerm THEN 3
                        WHEN e.Prenom LIKE @SearchTerm THEN 4
                        ELSE 5
                    END,
                    e.Nom, e.Prenom";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);
                        cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                        cmd.Parameters.AddWithValue("@MatriculeSearch", searchTerm + "%"); // Starts with
                        cmd.Parameters.AddWithValue("@ExactMatricule", searchTerm); // Exact match

                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                string text = reader["NomComplet"].ToString();
                                string value = reader["IdEmploye"].ToString();
                                ddlEmploye.Items.Add(new ListItem(text, value));
                                count++;
                            }

                            if (count > 0)
                            {
                                pnlEmployeeSelection.Visible = true;
                                ShowSuccess($"{count} employé(s) trouvé(s)");
                            }
                            else
                            {
                                ShowError("Aucun employé trouvé avec ce critère de recherche");
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

        // Helper method to check if string is numeric
        private bool IsNumeric(string value)
        {
            return int.TryParse(value, out _) || double.TryParse(value, out _);
        }

        private void LoadEmployeeInfo()
        {
            if (string.IsNullOrEmpty(ddlEmploye.SelectedValue))
            {
                pnlEmployeeInfo.Visible = false;
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT e.Matricule, e.TypeEmploye, d.NomDepartement
                        FROM EMPLOYE e
                        LEFT JOIN DEPARTEMENT d ON e.IdDepartement = d.IdDepartement
                        WHERE e.IdEmploye = @IdEmploye";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", ddlEmploye.SelectedValue);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                litMatricule.Text = reader["Matricule"].ToString();
                                litTypeEmploye.Text = reader["TypeEmploye"].ToString();
                                litDepartementEmploye.Text = reader["NomDepartement"]?.ToString() ?? "Non assigné";
                                pnlEmployeeInfo.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur: " + ex.Message);
            }
        }

        private void LoadChefEquipesList()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ce.IdChefEquipe, ce.NomEquipe, ce.DateAssignation,
                               e.Matricule AS MatriculeChef,
                               e.Nom + ' ' + e.Prenom AS NomChef,
                               s.NomSociete,
                               ISNULL(d.NomDepartement, 'Non assigné') AS NomDepartement,
                               (SELECT COUNT(*) FROM EMPLOYE_CHEF_EQUIPE ece 
                                WHERE ece.IdChefEquipe = ce.IdChefEquipe 
                                AND ece.is_deleted = 0 
                                AND ece.DateFinAssignation IS NULL) AS NombreEmployes
                        FROM CHEF_EQUIPE ce
                        INNER JOIN EMPLOYE e ON ce.IdEmploye = e.IdEmploye
                        INNER JOIN SOCIETE s ON ce.IdSociete = s.IdSociete
                        LEFT JOIN DEPARTEMENT d ON ce.IdDepartement = d.IdDepartement
                        WHERE ce.is_deleted = 0
                        ORDER BY ce.DateCreation DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvChefEquipes.DataSource = dt;
                        gvChefEquipes.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement de la liste: " + ex.Message);
            }
        }

        #endregion

        #region Event Handlers

        protected void ddlSociete_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear employee search when society changes
            txtSearchEmployee.Text = "";
            pnlEmployeeSelection.Visible = false;
            pnlEmployeeInfo.Visible = false;
        }

        protected void btnSearchEmployee_Click(object sender, EventArgs e)
        {
            SearchEmployees();
        }

        protected void ddlEmploye_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadEmployeeInfo();
        }

        protected void btnCreer_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            // Simple validation
            if (string.IsNullOrEmpty(txtNomEquipe.Text.Trim()))
            {
                ShowError("Le nom de l'équipe est obligatoire");
                return;
            }

            if (string.IsNullOrEmpty(ddlEmploye.SelectedValue))
            {
                ShowError("Veuillez rechercher et sélectionner un employé");
                return;
            }

            // Check if team name already exists
            if (TeamNameExists())
            {
                ShowError("Ce nom d'équipe existe déjà");
                return;
            }

            try
            {
                CreateChefEquipe();
                ShowSuccess("Chef d'équipe créé avec succès!");
                ClearForm();
                LoadChefEquipesList();
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de la création: " + ex.Message);
            }
        }

        protected void btnAnnuler_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/pointage/GestionEquipes.aspx");
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadChefEquipesList();
        }

        protected void gvChefEquipes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int chefEquipeId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "SupprimerChef")
            {
                try
                {
                    DeleteChefEquipe(chefEquipeId);
                    ShowSuccess("Chef d'équipe supprimé avec succès");
                    LoadChefEquipesList();
                }
                catch (Exception ex)
                {
                    ShowError("Erreur lors de la suppression: " + ex.Message);
                }
            }
        }

        #endregion

        #region Business Methods

        private bool TeamNameExists()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT COUNT(*) FROM CHEF_EQUIPE 
                        WHERE NomEquipe = @NomEquipe 
                        AND IdSociete = @IdSociete 
                        AND is_deleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NomEquipe", txtNomEquipe.Text.Trim());
                        cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);
                        conn.Open();
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private void CreateChefEquipe()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO CHEF_EQUIPE (IdEmploye, NomEquipe, AssignePar, IdSociete, IdDepartement, DateAssignation, DateCreation) 
                    VALUES (@IdEmploye, @NomEquipe, @AssignePar, @IdSociete, @IdDepartement, @DateAssignation, @DateCreation)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdEmploye", ddlEmploye.SelectedValue);
                    cmd.Parameters.AddWithValue("@NomEquipe", txtNomEquipe.Text.Trim());
                    cmd.Parameters.AddWithValue("@AssignePar", GetCurrentUserId());
                    cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);
                    cmd.Parameters.AddWithValue("@IdDepartement", DBNull.Value); // No department selection
                    cmd.Parameters.AddWithValue("@DateAssignation", txtDateAssignation.Text);
                    cmd.Parameters.AddWithValue("@DateCreation", DateTime.Now);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DeleteChefEquipe(int chefEquipeId)
        {
            // Check if has employees first
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string checkQuery = @"
                    SELECT COUNT(*) FROM EMPLOYE_CHEF_EQUIPE 
                    WHERE IdChefEquipe = @IdChefEquipe AND is_deleted = 0 AND DateFinAssignation IS NULL";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@IdChefEquipe", chefEquipeId);
                    conn.Open();
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        throw new Exception($"Impossible de supprimer. Ce chef d'équipe a {count} employé(s) assigné(s).");
                    }
                }

                // Delete chef equipe
                string deleteQuery = "UPDATE CHEF_EQUIPE SET is_deleted = 1 WHERE IdChefEquipe = @IdChefEquipe";
                using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                {
                    deleteCmd.Parameters.AddWithValue("@IdChefEquipe", chefEquipeId);
                    deleteCmd.ExecuteNonQuery();
                }
            }
        }

        private void ClearForm()
        {
            txtNomEquipe.Text = "";
            txtSearchEmployee.Text = "";
            ddlEmploye.Items.Clear();
            ddlEmploye.Items.Insert(0, new ListItem("-- Sélectionner Employé --", ""));
            pnlEmployeeSelection.Visible = false;
            pnlEmployeeInfo.Visible = false;
        }

        private int GetCurrentUserId()
        {
            // TODO: Get from session/authentication
            return Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
        }

        #endregion

        #region Helper Methods

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