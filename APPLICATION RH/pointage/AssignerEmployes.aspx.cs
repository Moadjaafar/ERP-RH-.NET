using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace APPLICATION_RH.pointage
{
    public partial class AssignerEmployes : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSocietes();
                txtDateAssignation.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadRecentAssignments();
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
                        ddlSociete.Items.Insert(0, new ListItem("-- Sélectionner --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des sociétés: " + ex.Message);
            }
        }

        private void LoadDepartements()
        {
            ddlDepartement.Items.Clear();
            ddlDepartement.Items.Insert(0, new ListItem("-- Tous --", ""));

            if (string.IsNullOrEmpty(ddlSociete.SelectedValue)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT IdDepartement, NomDepartement FROM DEPARTEMENT WHERE IdSociete = @IdSociete AND Actif = 1 ORDER BY NomDepartement";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);
                        conn.Open();
                        ddlDepartement.DataSource = cmd.ExecuteReader();
                        ddlDepartement.DataTextField = "NomDepartement";
                        ddlDepartement.DataValueField = "IdDepartement";
                        ddlDepartement.DataBind();
                        ddlDepartement.Items.Insert(0, new ListItem("-- Tous --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des départements: " + ex.Message);
            }
        }

        private void LoadServices()
        {
            ddlService.Items.Clear();
            ddlService.Items.Insert(0, new ListItem("-- Tous --", ""));

            if (string.IsNullOrEmpty(ddlDepartement.SelectedValue)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT IdService, NomService FROM SERVICE WHERE IdDepartement = @IdDepartement AND Actif = 1 ORDER BY NomService";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdDepartement", ddlDepartement.SelectedValue);
                        conn.Open();
                        ddlService.DataSource = cmd.ExecuteReader();
                        ddlService.DataTextField = "NomService";
                        ddlService.DataValueField = "IdService";
                        ddlService.DataBind();
                        ddlService.Items.Insert(0, new ListItem("-- Tous --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des services: " + ex.Message);
            }
        }

        private void LoadCellules()
        {
            ddlCellule.Items.Clear();
            ddlCellule.Items.Insert(0, new ListItem("-- Toutes --", ""));

            if (string.IsNullOrEmpty(ddlService.SelectedValue)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT IdCellule, NomCellule FROM CELLULE WHERE IdService = @IdService AND Actif = 1 ORDER BY NomCellule";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdService", ddlService.SelectedValue);
                        conn.Open();
                        ddlCellule.DataSource = cmd.ExecuteReader();
                        ddlCellule.DataTextField = "NomCellule";
                        ddlCellule.DataValueField = "IdCellule";
                        ddlCellule.DataBind();
                        ddlCellule.Items.Insert(0, new ListItem("-- Toutes --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des cellules: " + ex.Message);
            }
        }

        private void LoadChefEquipes()
        {
            ddlChefEquipe.Items.Clear();
            ddlChefEquipe.Items.Insert(0, new ListItem("-- Sélectionner Chef d'Équipe --", ""));

            if (string.IsNullOrEmpty(ddlSociete.SelectedValue)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ce.IdChefEquipe, ce.NomEquipe,
                               e.Nom + ' ' + e.Prenom + ' - ' + ce.NomEquipe AS DisplayText
                        FROM CHEF_EQUIPE ce
                        INNER JOIN EMPLOYE e ON ce.IdEmploye = e.IdEmploye
                        WHERE ce.IdSociete = @IdSociete 
                        AND ce.is_deleted = 0
                        ORDER BY ce.NomEquipe";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);
                        conn.Open();
                        ddlChefEquipe.DataSource = cmd.ExecuteReader();
                        ddlChefEquipe.DataTextField = "DisplayText";
                        ddlChefEquipe.DataValueField = "IdChefEquipe";
                        ddlChefEquipe.DataBind();
                        ddlChefEquipe.Items.Insert(0, new ListItem("-- Sélectionner Chef d'Équipe --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des équipes: " + ex.Message);
            }
        }

        private void SearchEmployees()
        {
            if (string.IsNullOrEmpty(ddlSociete.SelectedValue))
            {
                ShowError("Veuillez sélectionner une société");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT e.IdEmploye, e.Matricule, 
                               e.Nom + ' ' + e.Prenom AS NomComplet,
                               e.TypeEmploye,
                               ISNULL(d.NomDepartement, 'Non assigné') AS NomDepartement,
                               ISNULL(s.NomService, 'Non assigné') AS NomService,
                               ISNULL(c.NomCellule, 'Non assigné') AS NomCellule
                        FROM EMPLOYE e
                        LEFT JOIN DEPARTEMENT d ON e.IdDepartement = d.IdDepartement
                        LEFT JOIN SERVICE s ON e.IdService = s.IdService
                        LEFT JOIN CELLULE c ON e.IdCellule = c.IdCellule
                        WHERE e.IdSociete = @IdSociete 
                        AND e.is_deleted = 0
                        AND e.DateSortie IS NULL
                        AND e.IdEmploye NOT IN (
                            SELECT ece.IdEmploye 
                            FROM EMPLOYE_CHEF_EQUIPE ece 
                            WHERE ece.is_deleted = 0 
                            AND ece.DateFinAssignation IS NULL
                        )";

                    // Add filters
                    if (!string.IsNullOrEmpty(ddlDepartement.SelectedValue))
                        query += " AND e.IdDepartement = @IdDepartement";

                    if (!string.IsNullOrEmpty(ddlService.SelectedValue))
                        query += " AND e.IdService = @IdService";

                    if (!string.IsNullOrEmpty(ddlCellule.SelectedValue))
                        query += " AND e.IdCellule = @IdCellule";

                    query += " ORDER BY e.Nom, e.Prenom";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);

                        if (!string.IsNullOrEmpty(ddlDepartement.SelectedValue))
                            cmd.Parameters.AddWithValue("@IdDepartement", ddlDepartement.SelectedValue);

                        if (!string.IsNullOrEmpty(ddlService.SelectedValue))
                            cmd.Parameters.AddWithValue("@IdService", ddlService.SelectedValue);

                        if (!string.IsNullOrEmpty(ddlCellule.SelectedValue))
                            cmd.Parameters.AddWithValue("@IdCellule", ddlCellule.SelectedValue);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                gvEmployees.DataSource = dt;
                                gvEmployees.DataBind();
                                litEmployeeCount.Text = dt.Rows.Count.ToString();
                                pnlResults.Visible = true;
                                pnlAssignment.Visible = true;
                                LoadChefEquipes();
                                ShowSuccess($"{dt.Rows.Count} employé(s) disponible(s) trouvé(s)");
                            }
                            else
                            {
                                pnlResults.Visible = false;
                                pnlAssignment.Visible = false;
                                ShowError("Aucun employé disponible trouvé avec ces critères");
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

        private void LoadTeamInfo()
        {
            if (string.IsNullOrEmpty(ddlChefEquipe.SelectedValue))
            {
                pnlTeamInfo.Visible = false;
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ce.NomEquipe,
                               e.Nom + ' ' + e.Prenom AS NomChef,
                               (SELECT COUNT(*) FROM EMPLOYE_CHEF_EQUIPE ece 
                                WHERE ece.IdChefEquipe = ce.IdChefEquipe 
                                AND ece.is_deleted = 0 
                                AND ece.DateFinAssignation IS NULL) AS NombreEmployes
                        FROM CHEF_EQUIPE ce
                        INNER JOIN EMPLOYE e ON ce.IdEmploye = e.IdEmploye
                        WHERE ce.IdChefEquipe = @IdChefEquipe";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdChefEquipe", ddlChefEquipe.SelectedValue);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                litNomEquipe.Text = reader["NomEquipe"].ToString();
                                litNomChef.Text = reader["NomChef"].ToString();
                                litNombreEmployes.Text = reader["NombreEmployes"].ToString();
                                pnlTeamInfo.Visible = true;
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

        private void LoadRecentAssignments()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT TOP 20 
                               ece.IdEmployeChefEquipe,
                               e.Nom + ' ' + e.Prenom AS NomEmploye,
                               e.Matricule AS MatriculeEmploye,
                               ce.NomEquipe,
                               chef.Nom + ' ' + chef.Prenom AS NomChef,
                               ece.DateAssignation
                        FROM EMPLOYE_CHEF_EQUIPE ece
                        INNER JOIN EMPLOYE e ON ece.IdEmploye = e.IdEmploye
                        INNER JOIN CHEF_EQUIPE ce ON ece.IdChefEquipe = ce.IdChefEquipe
                        INNER JOIN EMPLOYE chef ON ce.IdEmploye = chef.IdEmploye
                        WHERE ece.is_deleted = 0 
                        AND ece.DateFinAssignation IS NULL
                        ORDER BY ece.DateCreation DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvRecentAssignments.DataSource = dt;
                        gvRecentAssignments.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des assignations: " + ex.Message);
            }
        }

        #endregion

        #region Event Handlers

        protected void ddlSociete_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDepartements();
            ClearResults();
        }

        protected void ddlDepartement_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadServices();
        }

        protected void ddlService_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCellules();
        }

        protected void ddlChefEquipe_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTeamInfo();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            SearchEmployees();
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            ddlDepartement.SelectedIndex = 0;
            ddlService.SelectedIndex = 0;
            ddlCellule.SelectedIndex = 0;
            ClearResults();
        }

        protected void btnSelectAll_Click(object sender, EventArgs e)
        {
            SetAllCheckboxes(true);
        }

        protected void btnUnselectAll_Click(object sender, EventArgs e)
        {
            SetAllCheckboxes(false);
        }

        protected void btnAssignSelected_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlChefEquipe.SelectedValue))
            {
                ShowError("Veuillez sélectionner un chef d'équipe");
                return;
            }

            if (string.IsNullOrEmpty(txtDateAssignation.Text))
            {
                ShowError("Veuillez saisir la date d'assignation");
                return;
            }

            List<int> selectedEmployees = GetSelectedEmployees();
            if (selectedEmployees.Count == 0)
            {
                ShowError("Veuillez sélectionner au moins un employé");
                return;
            }

            try
            {
                int assignedCount = AssignSelectedEmployees(selectedEmployees);
                ShowSuccess($"{assignedCount} employé(s) assigné(s) avec succès!");

                // Refresh data
                SearchEmployees();
                LoadTeamInfo();
                LoadRecentAssignments();
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de l'assignation: " + ex.Message);
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadRecentAssignments();
        }

        protected void gvRecentAssignments_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "RetirerEmploye")
            {
                try
                {
                    int assignationId = Convert.ToInt32(e.CommandArgument);
                    RemoveEmployeeFromTeam(assignationId);
                    ShowSuccess("Employé retiré de l'équipe avec succès");
                    LoadRecentAssignments();

                    // Refresh team info if same team
                    if (pnlTeamInfo.Visible)
                        LoadTeamInfo();
                }
                catch (Exception ex)
                {
                    ShowError("Erreur lors du retrait: " + ex.Message);
                }
            }
        }

        #endregion

        #region Helper Methods

        private void ClearResults()
        {
            pnlResults.Visible = false;
            pnlAssignment.Visible = false;
            pnlTeamInfo.Visible = false;
        }

        private void SetAllCheckboxes(bool isChecked)
        {
            foreach (GridViewRow row in gvEmployees.Rows)
            {
                CheckBox chkSelect = row.FindControl("chkSelect") as CheckBox;
                if (chkSelect != null)
                {
                    chkSelect.Checked = isChecked;
                }
            }
        }

        private List<int> GetSelectedEmployees()
        {
            List<int> selectedEmployees = new List<int>();

            foreach (GridViewRow row in gvEmployees.Rows)
            {
                CheckBox chkSelect = row.FindControl("chkSelect") as CheckBox;
                HiddenField hfIdEmploye = row.FindControl("hfIdEmploye") as HiddenField;

                if (chkSelect != null && hfIdEmploye != null && chkSelect.Checked)
                {
                    selectedEmployees.Add(Convert.ToInt32(hfIdEmploye.Value));
                }
            }

            return selectedEmployees;
        }

        private int AssignSelectedEmployees(List<int> employeeIds)
        {
            int assignedCount = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (int employeeId in employeeIds)
                        {
                            // Check if employee is still available
                            if (!IsEmployeeAlreadyAssigned(employeeId, conn, transaction))
                            {
                                string query = @"
                                    INSERT INTO EMPLOYE_CHEF_EQUIPE (IdEmploye, IdChefEquipe, AssignePar, DateAssignation, DateCreation) 
                                    VALUES (@IdEmploye, @IdChefEquipe, @AssignePar, @DateAssignation, @DateCreation)";

                                using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                                    cmd.Parameters.AddWithValue("@IdChefEquipe", ddlChefEquipe.SelectedValue);
                                    cmd.Parameters.AddWithValue("@AssignePar", GetCurrentUserId());
                                    cmd.Parameters.AddWithValue("@DateAssignation", txtDateAssignation.Text);
                                    cmd.Parameters.AddWithValue("@DateCreation", DateTime.Now);

                                    cmd.ExecuteNonQuery();
                                    assignedCount++;
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Erreur lors de l'assignation en masse: {ex.Message}");
                    }
                }
            }

            return assignedCount;
        }

        private bool IsEmployeeAlreadyAssigned(int employeeId, SqlConnection conn, SqlTransaction transaction)
        {
            string query = @"
                SELECT COUNT(*) FROM EMPLOYE_CHEF_EQUIPE 
                WHERE IdEmploye = @IdEmploye 
                AND is_deleted = 0 
                AND DateFinAssignation IS NULL";

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private void RemoveEmployeeFromTeam(int assignationId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    UPDATE EMPLOYE_CHEF_EQUIPE 
                    SET DateFinAssignation = @DateFin 
                    WHERE IdEmployeChefEquipe = @IdAssignation";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdAssignation", assignationId);
                    cmd.Parameters.AddWithValue("@DateFin", DateTime.Now.Date);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private int GetCurrentUserId()
        {
            // TODO: Get from session/authentication
            return Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
        }

        #endregion

        #region Message Methods

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