
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.permissions
{
    public partial class RolleEmployer : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadStatistics();
                LoadDropdowns();
                LoadEmployees();
            }
        }

        #region Data Loading Methods

        private void LoadStatistics()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Total active employees
                    string totalQuery = "SELECT COUNT(*) FROM EMPLOYE WHERE is_deleted = 0";
                    using (SqlCommand cmd = new SqlCommand(totalQuery, conn))
                    {
                        litTotalEmployees.Text = cmd.ExecuteScalar().ToString();
                    }

                    // Employees with roles (assuming we add IdRole to EMPLOYE table)
                    string withRolesQuery = @"
                        SELECT COUNT(*) FROM EMPLOYE e 
                        WHERE e.is_deleted = 0 
                        AND (e.Rolle IS NOT NULL OR EXISTS(
                            SELECT 1 FROM ROLES r WHERE r.RoleName = e.Rolle AND r.IsActive = 1
                        ))";
                    using (SqlCommand cmd = new SqlCommand(withRolesQuery, conn))
                    {
                        litEmployeesWithRoles.Text = cmd.ExecuteScalar().ToString();
                    }

                    // Employees without roles
                    string withoutRolesQuery = @"
                        SELECT COUNT(*) FROM EMPLOYE e 
                        WHERE e.is_deleted = 0 
                        AND (e.Rolle IS NULL OR NOT EXISTS(
                            SELECT 1 FROM ROLES r WHERE r.RoleName = e.Rolle AND r.IsActive = 1
                        ))";
                    using (SqlCommand cmd = new SqlCommand(withoutRolesQuery, conn))
                    {
                        litEmployeesWithoutRoles.Text = cmd.ExecuteScalar().ToString();
                    }

                    // Active roles count
                    string rolesQuery = "SELECT COUNT(*) FROM ROLES WHERE IsActive = 1";
                    using (SqlCommand cmd = new SqlCommand(rolesQuery, conn))
                    {
                        litActiveRoles.Text = cmd.ExecuteScalar().ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des statistiques: {ex.Message}");
            }
        }

        private void LoadDropdowns()
        {
            LoadSocietes();
            LoadDepartments();
            LoadRoles();
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
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            ddlSociete.DataSource = dt;
                            ddlSociete.DataTextField = "NomSociete";
                            ddlSociete.DataValueField = "IdSociete";
                            ddlSociete.DataBind();
                        }
                    }
                }
                ddlSociete.Items.Insert(0, new ListItem("-- Toutes les sociétés --", ""));
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des sociétés: {ex.Message}");
            }
        }

        private void LoadDepartments()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT IdDepartement, NomDepartement FROM DEPARTEMENT WHERE Actif = 1";
                    
                    if (!string.IsNullOrEmpty(ddlSociete.SelectedValue))
                        query += " AND IdSociete = @IdSociete";
                        
                    query += " ORDER BY NomDepartement";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(ddlSociete.SelectedValue))
                            cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            ddlDepartement.DataSource = dt;
                            ddlDepartement.DataTextField = "NomDepartement";
                            ddlDepartement.DataValueField = "IdDepartement";
                            ddlDepartement.DataBind();
                        }
                    }
                }
                ddlDepartement.Items.Insert(0, new ListItem("-- Tous les départements --", ""));
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des départements: {ex.Message}");
            }
        }

        private void LoadRoles()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT IdRole, RoleName FROM ROLES WHERE IsActive = 1 ORDER BY RoleName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            // Load bulk assignment dropdown
                            ddlBulkRole.DataSource = dt;
                            ddlBulkRole.DataTextField = "RoleName";
                            ddlBulkRole.DataValueField = "IdRole";
                            ddlBulkRole.DataBind();
                        }
                    }
                }
                ddlBulkRole.Items.Insert(0, new ListItem("-- Sélectionner un rôle --", ""));
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des rôles: {ex.Message}");
            }
        }

        private void LoadEmployees()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    StringBuilder query = new StringBuilder(@"
                        SELECT 
                            e.IdEmploye,
                            e.Matricule,
                            e.Nom,
                            e.Prenom,
                            e.Email,
                            e.TypeEmploye,
                            e.Rolle,
                            e.is_deleted,
                            s.NomSociete,
                            d.NomDepartement,
                            r.IdRole,
                            r.RoleName,
                            r.IsActive as RoleIsActive
                        FROM EMPLOYE e
                        LEFT JOIN SOCIETE s ON e.IdSociete = s.IdSociete
                        LEFT JOIN DEPARTEMENT d ON e.IdDepartement = d.IdDepartement
                        LEFT JOIN ROLES r ON e.Rolle = r.RoleName
                        WHERE e.is_deleted = 0");

                    // Apply filters
                    if (!string.IsNullOrEmpty(ddlSociete.SelectedValue))
                        query.Append(" AND e.IdSociete = @IdSociete");

                    if (!string.IsNullOrEmpty(ddlDepartement.SelectedValue))
                        query.Append(" AND e.IdDepartement = @IdDepartement");

                    // Role status filter
                    switch (ddlRoleStatus.SelectedValue)
                    {
                        case "WITH_ROLES":
                            query.Append(" AND e.Rolle IS NOT NULL");
                            break;
                        case "WITHOUT_ROLES":
                            query.Append(" AND e.Rolle IS NULL");
                            break;
                    }

                    // Search filter
                    if (!string.IsNullOrEmpty(txtSearch.Text))
                    {
                        query.Append(" AND (e.Nom LIKE @Search OR e.Prenom LIKE @Search OR e.Matricule LIKE @Search OR e.Email LIKE @Search)");
                    }

                    query.Append(" ORDER BY e.Nom, e.Prenom");

                    using (SqlCommand cmd = new SqlCommand(query.ToString(), conn))
                    {
                        if (!string.IsNullOrEmpty(ddlSociete.SelectedValue))
                            cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);

                        if (!string.IsNullOrEmpty(ddlDepartement.SelectedValue))
                            cmd.Parameters.AddWithValue("@IdDepartement", ddlDepartement.SelectedValue);

                        if (!string.IsNullOrEmpty(txtSearch.Text))
                            cmd.Parameters.AddWithValue("@Search", $"%{txtSearch.Text}%");

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            gvEmployees.DataSource = dt;
                            gvEmployees.DataBind();

                            litEmployeeCount.Text = $"({dt.Rows.Count} employé{(dt.Rows.Count > 1 ? "s" : "")})";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des employés: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers

        protected void ddlSociete_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDepartments();
            LoadEmployees();
        }

        protected void ddlDepartement_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadEmployees();
        }

        protected void ddlRoleStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadEmployees();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadEmployees();
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadStatistics();
            LoadEmployees();
            ShowInfo("Données actualisées.");
        }

        protected void btnBulkAssign_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ddlBulkRole.SelectedValue))
                {
                    ShowWarning("Veuillez sélectionner un rôle à attribuer.");
                    return;
                }

                int assignedCount = 0;
                string targetRoleName = ddlBulkRole.SelectedItem.Text;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    switch (ddlBulkTarget.SelectedValue)
                    {
                        case "SELECTED":
                            assignedCount = AssignRoleToSelectedEmployees(conn, targetRoleName);
                            break;
                        case "ALL_VISIBLE":
                            assignedCount = AssignRoleToAllVisibleEmployees(conn, targetRoleName);
                            break;
                        case "WITHOUT_ROLES":
                            assignedCount = AssignRoleToEmployeesWithoutRoles(conn, targetRoleName);
                            break;
                    }
                }

                ShowSuccess($"Rôle '{targetRoleName}' attribué à {assignedCount} employé(s).");
                LoadStatistics();
                LoadEmployees();
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors de l'attribution en masse: {ex.Message}");
            }
        }

        #endregion

        #region GridView Event Handlers

        protected void gvEmployees_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvEmployees.EditIndex = e.NewEditIndex;
            LoadEmployees();
        }

        protected void gvEmployees_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvEmployees.EditIndex = -1;
            LoadEmployees();
        }

        protected void gvEmployees_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                GridViewRow row = gvEmployees.Rows[e.RowIndex];
                int idEmploye = Convert.ToInt32(gvEmployees.DataKeys[e.RowIndex].Value);

                DropDownList ddlEditRole = (DropDownList)row.FindControl("ddlEditRole");
                
                if (ddlEditRole == null)
                {
                    ShowError("Erreur: Contrôle de rôle non trouvé.");
                    return;
                }

                string newRoleName = "";
                if (!string.IsNullOrEmpty(ddlEditRole.SelectedValue))
                {
                    newRoleName = ddlEditRole.SelectedItem.Text;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string updateQuery = "UPDATE EMPLOYE SET Rolle = @Rolle, DateModification = GETDATE(), ModifiePar = @ModifiePar WHERE IdEmploye = @IdEmploye";
                    
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Rolle", string.IsNullOrEmpty(newRoleName) ? (object)DBNull.Value : newRoleName);
                        cmd.Parameters.AddWithValue("@ModifiePar", "System"); // You can replace with current user
                        cmd.Parameters.AddWithValue("@IdEmploye", idEmploye);

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowSuccess($"Rôle modifié avec succès!");
                            gvEmployees.EditIndex = -1;
                            LoadStatistics();
                            LoadEmployees();
                        }
                        else
                        {
                            ShowError("Erreur lors de la modification du rôle.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors de la mise à jour: {ex.Message}");
            }
        }

        protected void gvEmployees_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && (e.Row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit)
            {
                // Populate role dropdown in edit mode
                DropDownList ddlEditRole = (DropDownList)e.Row.FindControl("ddlEditRole");
                if (ddlEditRole != null)
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            string query = "SELECT IdRole, RoleName FROM ROLES WHERE IsActive = 1 ORDER BY RoleName";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                                {
                                    DataTable dt = new DataTable();
                                    da.Fill(dt);

                                    ddlEditRole.DataSource = dt;
                                    ddlEditRole.DataTextField = "RoleName";
                                    ddlEditRole.DataValueField = "IdRole";
                                    ddlEditRole.DataBind();
                                }
                            }
                        }
                        
                        ddlEditRole.Items.Insert(0, new ListItem("-- Aucun rôle --", ""));
                        
                        // Set current value
                        DataRowView drv = (DataRowView)e.Row.DataItem;
                        if (drv != null && drv["IdRole"] != DBNull.Value)
                        {
                            ddlEditRole.SelectedValue = drv["IdRole"].ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Erreur lors du chargement des rôles pour l'édition: {ex.Message}");
                    }
                }
            }
        }

        protected void gvEmployees_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvEmployees.PageIndex = e.NewPageIndex;
            LoadEmployees();
        }

        protected void btnViewPermissions_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton btn = (LinkButton)sender;
                int idEmploye = Convert.ToInt32(btn.CommandArgument);

                string permissions = GetEmployeePermissions(idEmploye);
                litEmployeePermissions.Text = permissions;
                pnlPermissionsModal.Visible = true;
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des permissions: {ex.Message}");
            }
        }

        protected void btnCloseModal_Click(object sender, EventArgs e)
        {
            pnlPermissionsModal.Visible = false;
        }

        #endregion

        #region Helper Methods

        private int AssignRoleToSelectedEmployees(SqlConnection conn, string roleName)
        {
            int count = 0;
            foreach (GridViewRow row in gvEmployees.Rows)
            {
                CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                if (chkSelect != null && chkSelect.Checked)
                {
                    int idEmploye = Convert.ToInt32(gvEmployees.DataKeys[row.RowIndex].Value);
                    if (UpdateEmployeeRole(conn, idEmploye, roleName))
                        count++;
                }
            }
            return count;
        }

        private int AssignRoleToAllVisibleEmployees(SqlConnection conn, string roleName)
        {
            int count = 0;
            foreach (GridViewRow row in gvEmployees.Rows)
            {
                int idEmploye = Convert.ToInt32(gvEmployees.DataKeys[row.RowIndex].Value);
                if (UpdateEmployeeRole(conn, idEmploye, roleName))
                    count++;
            }
            return count;
        }

        private int AssignRoleToEmployeesWithoutRoles(SqlConnection conn, string roleName)
        {
            string query = "UPDATE EMPLOYE SET Rolle = @Rolle, DateModification = GETDATE(), ModifiePar = @ModifiePar WHERE is_deleted = 0 AND Rolle IS NULL";
            
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Rolle", roleName);
                cmd.Parameters.AddWithValue("@ModifiePar", "System");
                return cmd.ExecuteNonQuery();
            }
        }

        private bool UpdateEmployeeRole(SqlConnection conn, int idEmploye, string roleName)
        {
            try
            {
                string query = "UPDATE EMPLOYE SET Rolle = @Rolle, DateModification = GETDATE(), ModifiePar = @ModifiePar WHERE IdEmploye = @IdEmploye";
                
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Rolle", roleName);
                    cmd.Parameters.AddWithValue("@ModifiePar", "System");
                    cmd.Parameters.AddWithValue("@IdEmploye", idEmploye);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private string GetEmployeePermissions(int idEmploye)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT DISTINCT
                            p.PageName,
                            p.PageURL,
                            ISNULL(p.MenuGroup, 'Autres') as MenuGroup
                        FROM EMPLOYE e
                        INNER JOIN ROLES r ON e.Rolle = r.RoleName
                        INNER JOIN ROLE_PERMISSIONS rp ON r.IdRole = rp.IdRole AND rp.HasAccess = 1
                        INNER JOIN PAGES p ON rp.IdPage = p.IdPage AND p.IsActive = 1
                        WHERE e.IdEmploye = @IdEmploye AND e.is_deleted = 0 AND r.IsActive = 1
                        ORDER BY p.MenuGroup, p.PageName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", idEmploye);
                        
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count == 0)
                            {
                                return "<div class='alert alert-warning'>Aucune permission trouvée pour cet employé.</div>";
                            }

                            StringBuilder html = new StringBuilder();
                            html.Append("<div class='list-group'>");
                            
                            string currentGroup = "";
                            foreach (DataRow row in dt.Rows)
                            {
                                string menuGroup = row["MenuGroup"].ToString();
                                if (menuGroup != currentGroup)
                                {
                                    if (!string.IsNullOrEmpty(currentGroup))
                                        html.Append("</div>");
                                    
                                    html.AppendFormat("<div class='fw-bold text-primary mt-3 mb-2'>{0}</div><div>", menuGroup);
                                    currentGroup = menuGroup;
                                }
                                
                                html.AppendFormat(
                                    "<div class='list-group-item'><strong>{0}</strong><br><small class='text-muted'>{1}</small></div>",
                                    row["PageName"], row["PageURL"]);
                            }
                            
                            html.Append("</div></div>");
                            return html.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"<div class='alert alert-danger'>Erreur lors du chargement des permissions: {ex.Message}</div>";
            }
        }

        protected string GetRoleBadge(object roleName, object idRole)
        {
            if (roleName == null || roleName == DBNull.Value)
            {
                return "<span class='badge bg-secondary'>Aucun rôle</span>";
            }

            string role = roleName.ToString();
            bool isActive = idRole != null && idRole != DBNull.Value;
            
            return $"<span class='badge bg-{(isActive ? "primary" : "warning")}'>{role}</span>";
        }

        private void ShowSuccess(string message)
        {
            string escapedMessage = message.Replace("'", "\\'").Replace("\r\n", "\\n").Replace("\n", "\\n");
            ClientScript.RegisterStartupScript(this.GetType(), "Success", $"showToast('success', '{escapedMessage}');", true);
        }

        private void ShowError(string message)
        {
            string escapedMessage = message.Replace("'", "\\'").Replace("\r\n", "\\n").Replace("\n", "\\n");
            ClientScript.RegisterStartupScript(this.GetType(), "Error", $"showToast('error', '{escapedMessage}');", true);
        }
        
        private void ShowWarning(string message)
        {
            string escapedMessage = message.Replace("'", "\\'").Replace("\r\n", "\\n").Replace("\n", "\\n");
            ClientScript.RegisterStartupScript(this.GetType(), "Warning", $"showToast('warning', '{escapedMessage}');", true);
        }
        
        private void ShowInfo(string message)
        {
            string escapedMessage = message.Replace("'", "\\'").Replace("\r\n", "\\n").Replace("\n", "\\n");
            ClientScript.RegisterStartupScript(this.GetType(), "Info", $"showToast('info', '{escapedMessage}');", true);
        }

        #endregion
    }
}