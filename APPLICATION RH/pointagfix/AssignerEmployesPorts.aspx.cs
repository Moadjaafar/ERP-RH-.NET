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
    public partial class AssignerEmployesPorts : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSocietes();
                PopulatePortDropdown();
                LoadRecentPortAssignments();
            }
        }

        #region Load Data Methods

        private void PopulatePortDropdown()
        {
            // Clear existing items
            ddlPort.Items.Clear();

            // Add default option
            ddlPort.Items.Add(new ListItem("-- Toutes les Portes --", ""));

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            string query = "SELECT idPorte, Porte FROM PORTEUSINE ORDER BY Porte";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string portName = reader["Porte"].ToString();
                                string portId = reader["idPorte"].ToString();

                                // Add port to dropdown (you can use portId as value if needed)
                                ddlPort.Items.Add(new ListItem(portName, portName));
                            }
                        }
                    }
                }

                // Set default selection
                ddlPort.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                // Handle error - add error message to dropdown
                ddlPort.Items.Add(new ListItem("Erreur lors du chargement des portes", ""));

                // Log error for debugging
                System.Diagnostics.Debug.WriteLine($"Error loading ports: {ex.Message}");
            }
        }
        private void LoadSocietes()
        {
            //try
            //{
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
            //}
            //catch (Exception ex)
            //{
            //    ShowError("Erreur lors du chargement des sociétés: " + ex.Message);
            //}
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

        // 2. Modifiez seulement cette partie dans votre méthode SearchEmployees existante

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
                       ISNULL(ep.NomPort, 'Aucun port assigné') AS PortActuel
                FROM EMPLOYE e
                LEFT JOIN DEPARTEMENT d ON e.IdDepartement = d.IdDepartement
                LEFT JOIN EMPLOYE_PORT ep ON e.IdEmploye = ep.IdEmploye 
                    AND ep.is_deleted = 0
                WHERE e.IdSociete = @IdSociete 
                AND e.is_deleted = 0
                AND e.DateSortie IS NULL";

                    // Filtres existants
                    if (!string.IsNullOrEmpty(ddlDepartement.SelectedValue))
                        query += " AND e.IdDepartement = @IdDepartement";

                    if (!string.IsNullOrEmpty(txtMatricule.Text.Trim()))
                        query += " AND e.Matricule LIKE @Matricule";

                    // NOUVEAU FILTRE - Ajoutez ces lignes
                    if (ddlStatutAssignation.SelectedValue == "ASSIGNE")
                        query += " AND ep.IdEmploye IS NOT NULL";
                    else if (ddlStatutAssignation.SelectedValue == "NON_ASSIGNE")
                        query += " AND ep.IdEmploye IS NULL";

                    query += " ORDER BY e.Nom, e.Prenom";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);

                        if (!string.IsNullOrEmpty(ddlDepartement.SelectedValue))
                            cmd.Parameters.AddWithValue("@IdDepartement", ddlDepartement.SelectedValue);

                        if (!string.IsNullOrEmpty(txtMatricule.Text.Trim()))
                            cmd.Parameters.AddWithValue("@Matricule", "%" + txtMatricule.Text.Trim() + "%");

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
                                ShowSuccess($"{dt.Rows.Count} employé(s) trouvé(s)");
                            }
                            else
                            {
                                pnlResults.Visible = false;
                                pnlAssignment.Visible = false;
                                ShowError("Aucun employé trouvé avec ces critères");
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

        // 3. Mettez à jour la méthode btnClearFilters_Click
        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            ddlDepartement.SelectedIndex = 0;
            ddlStatutAssignation.SelectedIndex = 0; // Ajoutez cette ligne
            txtMatricule.Text = "";
            txtNote.Text = "";
            ClearResults();
        }

        //private void SearchEmployees()
        //{
        //    if (string.IsNullOrEmpty(ddlSociete.SelectedValue))
        //    {
        //        ShowError("Veuillez sélectionner une société");
        //        return;
        //    }

        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(connectionString))
        //        {
        //            string query = @"
        //                SELECT e.IdEmploye, e.Matricule, 
        //                       e.Nom + ' ' + e.Prenom AS NomComplet,
        //                       e.TypeEmploye,
        //                       ISNULL(d.NomDepartement, 'Non assigné') AS NomDepartement,
        //                       ISNULL(ep.NomPort, 'Aucun port assigné') AS PortActuel
        //                FROM EMPLOYE e
        //                LEFT JOIN DEPARTEMENT d ON e.IdDepartement = d.IdDepartement
        //                LEFT JOIN EMPLOYE_PORT ep ON e.IdEmploye = ep.IdEmploye 
        //                    AND ep.is_deleted = 0
        //                WHERE e.IdSociete = @IdSociete 
        //                AND e.is_deleted = 0
        //                AND e.DateSortie IS NULL";

        //            // Add filters
        //            if (!string.IsNullOrEmpty(ddlDepartement.SelectedValue))
        //                query += " AND e.IdDepartement = @IdDepartement";

        //            if (!string.IsNullOrEmpty(txtMatricule.Text.Trim()))
        //                query += " AND e.Matricule LIKE @Matricule";

        //            query += " ORDER BY e.Nom, e.Prenom";

        //            using (SqlCommand cmd = new SqlCommand(query, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);

        //                if (!string.IsNullOrEmpty(ddlDepartement.SelectedValue))
        //                    cmd.Parameters.AddWithValue("@IdDepartement", ddlDepartement.SelectedValue);

        //                if (!string.IsNullOrEmpty(txtMatricule.Text.Trim()))
        //                    cmd.Parameters.AddWithValue("@Matricule", "%" + txtMatricule.Text.Trim() + "%");

        //                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
        //                {
        //                    DataTable dt = new DataTable();
        //                    adapter.Fill(dt);

        //                    if (dt.Rows.Count > 0)
        //                    {
        //                        gvEmployees.DataSource = dt;
        //                        gvEmployees.DataBind();
        //                        litEmployeeCount.Text = dt.Rows.Count.ToString();
        //                        pnlResults.Visible = true;
        //                        pnlAssignment.Visible = true;
        //                        ShowSuccess($"{dt.Rows.Count} employé(s) trouvé(s)");
        //                    }
        //                    else
        //                    {
        //                        pnlResults.Visible = false;
        //                        pnlAssignment.Visible = false;
        //                        ShowError("Aucun employé trouvé avec ces critères");
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowError("Erreur lors de la recherche: " + ex.Message);
        //    }
        //}

        private void LoadPortInfo()
        {
            if (string.IsNullOrEmpty(ddlPort.SelectedValue))
            {
                pnlPortInfo.Visible = false;
                return;
            }

            try
            {
                // Get port information (static data since ports are not in database)
                string portName = ddlPort.SelectedValue;

                // Get count of employees assigned to this port
                int employeeCount = GetPortEmployeeCount(portName);

                litNomPort.Text = portName;
                litTypePort.Text = GetPortType(portName);
                litNombreEmployesPort.Text = employeeCount.ToString();
                pnlPortInfo.Visible = true;
            }
            catch (Exception ex)
            {
                ShowError("Erreur: " + ex.Message);
            }
        }

        private int GetPortEmployeeCount(string portName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT COUNT(*) 
                        FROM EMPLOYE_PORT 
                        WHERE NomPort = @NomPort 
                        AND is_deleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NomPort", portName);
                        conn.Open();
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        private string GetPortType(string portName)
        {
            if (portName.Contains("Principal")) return "Entrée Principal";
            if (portName.Contains("GSF")) return "Entrée GSF";
            if (portName.Contains("POI")) return "Entrée POI";
            if (portName.Contains("Technique")) return "Entrée Technique";
            if (portName.Contains("ERG PACKAGING")) return "Entrée ERG PACKAGING";
            return "Entrée Standard";
        }

        private void LoadRecentPortAssignments()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT TOP 20 
                               ep.IdEmployePort,
                               e.Nom + ' ' + e.Prenom AS NomEmploye,
                               e.Matricule AS MatriculeEmploye,
                               ep.NomPort,
                               ep.DateCreation,
                               ISNULL(assigneur.Nom + ' ' + assigneur.Prenom, 'Système') AS AssignePar
                        FROM EMPLOYE_PORT ep
                        INNER JOIN EMPLOYE e ON ep.IdEmploye = e.IdEmploye
                        LEFT JOIN EMPLOYE assigneur ON ep.AssignePar = assigneur.IdEmploye
                        WHERE ep.is_deleted = 0
                        ORDER BY ep.DateCreation DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvRecentPortAssignments.DataSource = dt;
                        gvRecentPortAssignments.DataBind();
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
            // No cascading for ports - they are static
        }

        protected void ddlPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPortInfo();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            SearchEmployees();
        }

        //protected void btnClearFilters_Click(object sender, EventArgs e)
        //{
        //    ddlDepartement.SelectedIndex = 0;
        //    txtMatricule.Text = "";
        //    txtNote.Text = "";
        //    ClearResults();
        //}

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
            if (string.IsNullOrEmpty(ddlPort.SelectedValue))
            {
                ShowError("Veuillez sélectionner un port");
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
                int assignedCount = AssignSelectedEmployeesToPort(selectedEmployees);
                ShowSuccess($"{assignedCount} employé(s) assigné(s) au port avec succès!");

                // Refresh data
                SearchEmployees();
                LoadPortInfo();
                LoadRecentPortAssignments();
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de l'assignation: " + ex.Message);
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadRecentPortAssignments();
        }

        protected void gvRecentPortAssignments_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "RetirerEmployePort")
            {
                try
                {
                    int assignationId = Convert.ToInt32(e.CommandArgument);
                    RemoveEmployeeFromPort(assignationId);
                    ShowSuccess("Employé retiré du port avec succès");
                    LoadRecentPortAssignments();

                    // Refresh port info if same port
                    if (pnlPortInfo.Visible)
                        LoadPortInfo();
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
            pnlPortInfo.Visible = false;
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

        private int AssignSelectedEmployeesToPort(List<int> employeeIds)
        {
            int assignedCount = 0;
            string portName = ddlPort.SelectedValue;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (int employeeId in employeeIds)
                        {
                            // Remove any existing port assignment for this employee (soft delete)
                            RemoveExistingPortAssignment(employeeId, conn, transaction);

                            // Add new port assignment
                            string query = @"
                                INSERT INTO EMPLOYE_PORT (IdEmploye, NomPort, AssignePar, DateCreation, is_deleted) 
                                VALUES (@IdEmploye, @NomPort, @AssignePar, @DateCreation, 0)";

                            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                                cmd.Parameters.AddWithValue("@NomPort", portName);
                                cmd.Parameters.AddWithValue("@AssignePar", GetCurrentUserId());
                                cmd.Parameters.AddWithValue("@DateCreation", DateTime.Now);

                                cmd.ExecuteNonQuery();
                                assignedCount++;
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Erreur lors de l'assignation en masse au port: {ex.Message}");
                    }
                }
            }

            return assignedCount;
        }

        private void RemoveExistingPortAssignment(int employeeId, SqlConnection conn, SqlTransaction transaction)
        {
            string query = @"
                UPDATE EMPLOYE_PORT 
                SET is_deleted = 1, DateModification = @DateModification 
                WHERE IdEmploye = @IdEmploye 
                AND is_deleted = 0";

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
        }

        private void RemoveEmployeeFromPort(int assignationId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    UPDATE EMPLOYE_PORT 
                    SET is_deleted = 1, DateModification = @DateModification 
                    WHERE IdEmployePort = @IdAssignation";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdAssignation", assignationId);
                    cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private int GetCurrentUserId()
        {
            // TODO: Get from session/authentication
            return Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 65;
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