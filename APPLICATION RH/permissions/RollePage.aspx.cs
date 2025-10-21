using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.permissions
{
    public partial class RollePage : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadStatistics();
                LoadDropdowns();
                InitializeMatrixDropdown();
                ShowEmptyMatrix();
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

                    // Get total active roles
                    string roleQuery = "SELECT COUNT(*) FROM ROLES WHERE IsActive = 1";
                    using (SqlCommand cmd = new SqlCommand(roleQuery, conn))
                    {
                        litTotalRoles.Text = cmd.ExecuteScalar().ToString();
                    }

                    // Get total active pages
                    string pageQuery = "SELECT COUNT(*) FROM PAGES WHERE IsActive = 1";
                    using (SqlCommand cmd = new SqlCommand(pageQuery, conn))
                    {
                        litTotalPages.Text = cmd.ExecuteScalar().ToString();
                    }

                    // Get total permissions granted
                    string permissionQuery = "SELECT COUNT(*) FROM ROLE_PERMISSIONS WHERE HasAccess = 1";
                    using (SqlCommand cmd = new SqlCommand(permissionQuery, conn))
                    {
                        litTotalPermissions.Text = cmd.ExecuteScalar().ToString();
                    }

                    // Calculate permission coverage percentage
                    string coverageQuery = @"
                        SELECT 
                            CASE 
                                WHEN (SELECT COUNT(*) FROM ROLES WHERE IsActive = 1) * (SELECT COUNT(*) FROM PAGES WHERE IsActive = 1) = 0 
                                THEN 0
                                ELSE CAST((SELECT COUNT(*) FROM ROLE_PERMISSIONS WHERE HasAccess = 1) * 100.0 / 
                                     ((SELECT COUNT(*) FROM ROLES WHERE IsActive = 1) * (SELECT COUNT(*) FROM PAGES WHERE IsActive = 1)) AS INT)
                            END";
                    using (SqlCommand cmd = new SqlCommand(coverageQuery, conn))
                    {
                        litPermissionPercentage.Text = cmd.ExecuteScalar().ToString() + "%";
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
            LoadRoleDropdown();
            LoadPageDropdown();
        }

        private void InitializeMatrixDropdown()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT IdRole, RoleName FROM ROLES WHERE IsActive = 1 ORDER BY RoleName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            ddlMatrixRole.DataSource = dt;
                            ddlMatrixRole.DataTextField = "RoleName";
                            ddlMatrixRole.DataValueField = "IdRole";
                            ddlMatrixRole.DataBind();
                        }
                    }
                }
                ddlMatrixRole.Items.Insert(0, new ListItem("-- Sélectionner un rôle --", ""));
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des rôles: {ex.Message}");
            }
        }

        private void ShowEmptyMatrix()
        {
            litPermissionMatrix.Text = @"
                <div class='alert alert-info text-center p-4'>
                    <i class='fas fa-info-circle fa-2x mb-3'></i>
                    <h5>Aucun rôle sélectionné</h5>
                    <p class='mb-0'>Veuillez sélectionner un rôle dans la liste déroulante ci-dessus pour afficher ses permissions.</p>
                </div>";
        }

        private void LoadRoleDropdown()
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

                            ddlRoleManagement.DataSource = dt;
                            ddlRoleManagement.DataTextField = "RoleName";
                            ddlRoleManagement.DataValueField = "IdRole";
                            ddlRoleManagement.DataBind();
                        }
                    }
                }
                ddlRoleManagement.Items.Insert(0, new ListItem("-- Sélectionner un rôle --", ""));
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des rôles: {ex.Message}");
            }
        }

        private void LoadPageDropdown()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT IdPage, PageName, MenuGroup FROM PAGES WHERE IsActive = 1 ORDER BY MenuGroup, PageName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            ddlPageManagement.DataSource = dt;
                            ddlPageManagement.DataTextField = "PageName";
                            ddlPageManagement.DataValueField = "IdPage";
                            ddlPageManagement.DataBind();

                            // Add menu group info to the text
                            foreach (ListItem item in ddlPageManagement.Items)
                            {
                                if (!string.IsNullOrEmpty(item.Value))
                                {
                                    DataRow[] rows = dt.Select($"IdPage = {item.Value}");
                                    if (rows.Length > 0)
                                    {
                                        string menuGroup = rows[0]["MenuGroup"].ToString();
                                        item.Text = $"[{menuGroup}] {item.Text}";
                                    }
                                }
                            }
                        }
                    }
                }
                ddlPageManagement.Items.Insert(0, new ListItem("-- Sélectionner une page --", ""));
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des pages: {ex.Message}");
            }
        }

        private void LoadPermissionMatrix(int selectedRoleId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Get selected role info
                    DataTable roleTable = new DataTable();
                    string roleQuery = "SELECT IdRole, RoleName FROM ROLES WHERE IdRole = @RoleId AND IsActive = 1";
                    using (SqlCommand cmd = new SqlCommand(roleQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoleId", selectedRoleId);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(roleTable);
                        }
                    }

                    if (roleTable.Rows.Count == 0)
                    {
                        ShowEmptyMatrix();
                        return;
                    }

                    // Get pages with permissions for the selected role
                    DataTable permissionsTable = new DataTable();
                    string permissionQuery = @"
                        SELECT 
                            p.IdPage,
                            p.PageName,
                            p.PageURL,
                            ISNULL(p.MenuGroup, 'Autres') as MenuGroup,
                            @RoleId as IdRole,
                            @RoleName as RoleName,
                            ISNULL(rp.HasAccess, 0) as HasAccess,
                            ISNULL(rp.IdRolePermission, 0) as IdRolePermission
                        FROM PAGES p
                        LEFT JOIN ROLE_PERMISSIONS rp ON p.IdPage = rp.IdPage AND rp.IdRole = @RoleId
                        WHERE p.IsActive = 1
                        ORDER BY p.MenuGroup, p.PageName";
                    using (SqlCommand cmd = new SqlCommand(permissionQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoleId", selectedRoleId);
                        cmd.Parameters.AddWithValue("@RoleName", roleTable.Rows[0]["RoleName"].ToString());
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(permissionsTable);
                        }
                    }

                    // Generate HTML table for single role
                    GenerateRolePermissionMatrix(roleTable.Rows[0], permissionsTable);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement de la matrice: {ex.Message}");
                litPermissionMatrix.Text = "<div class='alert alert-danger'>Erreur lors du chargement de la matrice des permissions.</div>";
            }
        }

        private void GeneratePermissionMatrix(DataTable roles, DataTable permissions)
        {
            if (roles.Rows.Count == 0 || permissions.Rows.Count == 0)
            {
                litPermissionMatrix.Text = "<div class='alert alert-info'>Aucune donnée disponible pour la matrice des permissions.</div>";
                return;
            }

            StringBuilder html = new StringBuilder();
            
            // Start table
            html.Append("<table class='permission-table table table-bordered'>");
            
            // Header row
            html.Append("<thead><tr>");
            html.Append("<th class='page-cell'>Page</th>");
            
            // Add role headers with toggle buttons
            foreach (DataRow role in roles.Rows)
            {
                html.AppendFormat(@"
                    <th>
                        <div>{0}</div>
                        <div class='mt-1'>
                            <button type='button' class='btn btn-success role-header-btn' 
                                    onclick='toggleRolePermissions({1}, true)' title='Accorder tout'>
                                ✓
                            </button>
                            <button type='button' class='btn btn-danger role-header-btn' 
                                    onclick='toggleRolePermissions({1}, false)' title='Révoquer tout'>
                                ✗
                            </button>
                        </div>
                    </th>", 
                    role["RoleName"], role["IdRole"]);
            }
            html.Append("</tr></thead>");
            
            // Body rows
            html.Append("<tbody>");
            
            // Group permissions by page
            var pageGroups = permissions.AsEnumerable()
                .GroupBy(row => new {
                    IdPage = row.Field<int>("IdPage"),
                    PageName = row.Field<string>("PageName"),
                    PageURL = row.Field<string>("PageURL"),
                    MenuGroup = row.Field<string>("MenuGroup")
                })
                .OrderBy(g => g.Key.MenuGroup)
                .ThenBy(g => g.Key.PageName);

            foreach (var pageGroup in pageGroups)
            {
                html.Append("<tr>");
                
                // Page cell
                html.AppendFormat(@"
                    <td class='page-cell'>
                        <span class='badge bg-info menu-badge me-2'>{0}</span>
                        <div><strong>{1}</strong></div>
                        <small class='text-muted'>{2}</small>
                        <div class='mt-1'>
                            <button type='button' class='btn btn-success role-header-btn' 
                                    onclick='togglePagePermissions({3}, true)' title='Accorder tout'>
                                ✓
                            </button>
                            <button type='button' class='btn btn-danger role-header-btn' 
                                    onclick='togglePagePermissions({3}, false)' title='Révoquer tout'>
                                ✗
                            </button>
                        </div>
                    </td>", 
                    pageGroup.Key.MenuGroup, 
                    pageGroup.Key.PageName, 
                    pageGroup.Key.PageURL,
                    pageGroup.Key.IdPage);
                
                // Permission cells for each role
                foreach (DataRow role in roles.Rows)
                {
                    int roleId = Convert.ToInt32(role["IdRole"]);
                    var permissionRow = pageGroup.FirstOrDefault(p => p.Field<int>("IdRole") == roleId);
                    
                    bool hasAccess = permissionRow != null && Convert.ToBoolean(permissionRow["HasAccess"]);
                    int idRolePermission = permissionRow != null ? Convert.ToInt32(permissionRow["IdRolePermission"]) : 0;
                    
                    html.AppendFormat(@"
                        <td>
                            <input type='checkbox' class='permission-checkbox' 
                                   name='permission_{0}_{1}' 
                                   data-role='{1}' 
                                   data-page='{0}' 
                                   data-permission-id='{2}'
                                   {3} />
                        </td>", 
                        pageGroup.Key.IdPage, 
                        roleId, 
                        idRolePermission,
                        hasAccess ? "checked" : "");
                }
                
                html.Append("</tr>");
            }
            
            html.Append("</tbody></table>");
            
            litPermissionMatrix.Text = html.ToString();
        }

        private void GenerateRolePermissionMatrix(DataRow selectedRole, DataTable permissions)
        {
            if (permissions.Rows.Count == 0)
            {
                litPermissionMatrix.Text = "<div class='alert alert-info'>Aucune page disponible pour ce rôle.</div>";
                return;
            }

            StringBuilder html = new StringBuilder();
            
            // Start table
            html.Append("<table class='permission-table table table-bordered'>");
            
            // Header row
            html.Append("<thead><tr>");
            html.Append("<th class='page-cell'>Page</th>");
            html.AppendFormat("<th class='text-center'>{0}<br/><small class='text-muted'>Permissions</small></th>", selectedRole["RoleName"]);
            html.Append("</tr></thead>");
            
            // Body rows
            html.Append("<tbody>");
            
            // Group by menu group
            var menuGroups = permissions.AsEnumerable()
                .GroupBy(row => row.Field<string>("MenuGroup"))
                .OrderBy(g => g.Key);

            foreach (var menuGroup in menuGroups)
            {
                // Menu group header
                html.AppendFormat("<tr class='table-secondary'><td colspan='2'><strong>📁 {0}</strong></td></tr>", menuGroup.Key);
                
                foreach (DataRow page in menuGroup.OrderBy(p => p.Field<string>("PageName")))
                {
                    bool hasAccess = Convert.ToBoolean(page["HasAccess"]);
                    int idRolePermission = Convert.ToInt32(page["IdRolePermission"]);
                    
                    html.Append("<tr>");
                    
                    // Page cell
                    html.AppendFormat(@"
                        <td class='page-cell'>
                            <div><strong>{0}</strong></div>
                            <small class='text-muted'>{1}</small>
                        </td>", 
                        page["PageName"], 
                        page["PageURL"]);
                    
                    // Permission cell
                    html.AppendFormat(@"
                        <td class='text-center'>
                            <input type='checkbox' class='permission-checkbox' 
                                   name='permission_{0}_{1}' 
                                   data-role='{1}' 
                                   data-page='{0}' 
                                   data-permission-id='{2}'
                                   {3} />
                        </td>", 
                        page["IdPage"], 
                        selectedRole["IdRole"], 
                        idRolePermission,
                        hasAccess ? "checked" : "");
                    
                    html.Append("</tr>");
                }
            }
            
            html.Append("</tbody></table>");
            
            litPermissionMatrix.Text = html.ToString();
        }

        #endregion

        #region Event Handlers

        protected void ddlMatrixRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlMatrixRole.SelectedValue))
            {
                int selectedRoleId = Convert.ToInt32(ddlMatrixRole.SelectedValue);
                LoadPermissionMatrix(selectedRoleId);
                
                // Show role info panel
                litSelectedRole.Text = ddlMatrixRole.SelectedItem.Text;
                pnlMatrixInfo.Visible = true;
            }
            else
            {
                ShowEmptyMatrix();
                pnlMatrixInfo.Visible = false;
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadStatistics();
            LoadDropdowns();
            InitializeMatrixDropdown();
            
            // Refresh matrix if a role is selected
            if (!string.IsNullOrEmpty(ddlMatrixRole.SelectedValue))
            {
                int selectedRoleId = Convert.ToInt32(ddlMatrixRole.SelectedValue);
                LoadPermissionMatrix(selectedRoleId);
                litSelectedRole.Text = ddlMatrixRole.SelectedItem.Text;
                pnlMatrixInfo.Visible = true;
            }
            else
            {
                ShowEmptyMatrix();
                pnlMatrixInfo.Visible = false;
            }
            
            ShowInfo("Données actualisées avec succès.");
        }

        protected void btnSaveAllPermissions_Click(object sender, EventArgs e)
        {
            try
            {
                int saved = 0;
                int errors = 0;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Process all permission checkboxes from the form
                    foreach (string key in Request.Form.AllKeys)
                    {
                        if (key.StartsWith("permission_"))
                        {
                            // Parse the key to get page and role IDs
                            string[] parts = key.Replace("permission_", "").Split('_');
                            if (parts.Length == 2)
                            {
                                int idPage = Convert.ToInt32(parts[0]);
                                int idRole = Convert.ToInt32(parts[1]);
                                bool hasAccess = Request.Form[key] != null && Request.Form[key].Contains("on");

                                if (SavePermission(conn, idRole, idPage, hasAccess))
                                    saved++;
                                else
                                    errors++;
                            }
                        }
                    }
                }

                if (errors == 0)
                {
                    ShowSuccess($"✅ {saved} permissions sauvegardées avec succès!");
                }
                else
                {
                    ShowWarning($"⚠️ {saved} permissions sauvegardées, {errors} erreurs rencontrées.");
                }

                LoadStatistics();
                
                // Refresh matrix if a role is selected
                if (!string.IsNullOrEmpty(ddlMatrixRole.SelectedValue))
                {
                    int selectedRoleId = Convert.ToInt32(ddlMatrixRole.SelectedValue);
                    LoadPermissionMatrix(selectedRoleId);
                }
            }
            catch (Exception ex)
            {
                ShowError($"❌ Erreur lors de la sauvegarde: {ex.Message}");
            }
        }

        protected void ddlRoleManagement_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlRoleManagement.SelectedValue))
            {
                LoadRolePermissions(Convert.ToInt32(ddlRoleManagement.SelectedValue));
                pnlRolePermissions.Visible = true;
            }
            else
            {
                pnlRolePermissions.Visible = false;
            }
        }

        protected void ddlPageManagement_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlPageManagement.SelectedValue))
            {
                LoadPagePermissions(Convert.ToInt32(ddlPageManagement.SelectedValue));
                pnlPagePermissions.Visible = true;
            }
            else
            {
                pnlPagePermissions.Visible = false;
            }
        }

        protected void btnSaveRolePermissions_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlRoleManagement.SelectedValue))
                return;

            try
            {
                int idRole = Convert.ToInt32(ddlRoleManagement.SelectedValue);
                int saved = 0;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (ListItem item in cblRolePages.Items)
                    {
                        int idPage = Convert.ToInt32(item.Value);
                        bool hasAccess = item.Selected;

                        if (SavePermission(conn, idRole, idPage, hasAccess))
                            saved++;
                    }
                }

                ShowSuccess($"✅ Permissions pour le rôle sauvegardées: {saved} modifications.");
                LoadStatistics();
                
                // Refresh matrix if a role is selected
                if (!string.IsNullOrEmpty(ddlMatrixRole.SelectedValue))
                {
                    int selectedRoleId = Convert.ToInt32(ddlMatrixRole.SelectedValue);
                    LoadPermissionMatrix(selectedRoleId);
                }
            }
            catch (Exception ex)
            {
                ShowError($"❌ Erreur lors de la sauvegarde: {ex.Message}");
            }
        }

        protected void btnSavePagePermissions_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlPageManagement.SelectedValue))
                return;

            try
            {
                int idPage = Convert.ToInt32(ddlPageManagement.SelectedValue);
                int saved = 0;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (ListItem item in cblPageRoles.Items)
                    {
                        int idRole = Convert.ToInt32(item.Value);
                        bool hasAccess = item.Selected;

                        if (SavePermission(conn, idRole, idPage, hasAccess))
                            saved++;
                    }
                }

                ShowSuccess($"✅ Permissions pour la page sauvegardées: {saved} modifications.");
                LoadStatistics();
                
                // Refresh matrix if a role is selected
                if (!string.IsNullOrEmpty(ddlMatrixRole.SelectedValue))
                {
                    int selectedRoleId = Convert.ToInt32(ddlMatrixRole.SelectedValue);
                    LoadPermissionMatrix(selectedRoleId);
                }
            }
            catch (Exception ex)
            {
                ShowError($"❌ Erreur lors de la sauvegarde: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private void LoadRolePermissions(int idRole)
        {
            try
            {
                cblRolePages.Items.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    string query = @"
                        SELECT p.IdPage, p.PageName, ISNULL(p.MenuGroup, 'Autres') as MenuGroup, ISNULL(rp.HasAccess, 0) as HasAccess
                        FROM PAGES p
                        LEFT JOIN ROLE_PERMISSIONS rp ON p.IdPage = rp.IdPage AND rp.IdRole = @IdRole
                        WHERE p.IsActive = 1
                        ORDER BY p.MenuGroup, p.PageName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdRole", idRole);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string menuGroup = reader["MenuGroup"].ToString();
                                string pageName = reader["PageName"].ToString();
                                string text = $"[{menuGroup}] {pageName}";
                                ListItem item = new ListItem(text, reader["IdPage"].ToString());
                                item.Selected = Convert.ToBoolean(reader["HasAccess"]);
                                cblRolePages.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des permissions du rôle: {ex.Message}");
            }
        }

        private void LoadPagePermissions(int idPage)
        {
            try
            {
                cblPageRoles.Items.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    
                    string query = @"
                        SELECT r.IdRole, r.RoleName, ISNULL(rp.HasAccess, 0) as HasAccess
                        FROM ROLES r
                        LEFT JOIN ROLE_PERMISSIONS rp ON r.IdRole = rp.IdRole AND rp.IdPage = @IdPage
                        WHERE r.IsActive = 1
                        ORDER BY r.RoleName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdPage", idPage);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string roleName = reader["RoleName"].ToString();
                                string roleId = reader["IdRole"].ToString();
                                ListItem item = new ListItem(roleName, roleId);
                                item.Selected = Convert.ToBoolean(reader["HasAccess"]);
                                cblPageRoles.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des permissions de la page: {ex.Message}");
            }
        }

        private bool SavePermission(SqlConnection conn, int idRole, int idPage, bool hasAccess)
        {
            try
            {
                // Check if permission already exists
                string checkQuery = "SELECT IdRolePermission FROM ROLE_PERMISSIONS WHERE IdRole = @IdRole AND IdPage = @IdPage";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@IdRole", idRole);
                    checkCmd.Parameters.AddWithValue("@IdPage", idPage);
                    object result = checkCmd.ExecuteScalar();

                    if (result != null)
                    {
                        // Update existing
                        string updateQuery = "UPDATE ROLE_PERMISSIONS SET HasAccess = @HasAccess WHERE IdRole = @IdRole AND IdPage = @IdPage";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@HasAccess", hasAccess);
                            updateCmd.Parameters.AddWithValue("@IdRole", idRole);
                            updateCmd.Parameters.AddWithValue("@IdPage", idPage);
                            return updateCmd.ExecuteNonQuery() > 0;
                        }
                    }
                    else if (hasAccess) // Only insert if granting access
                    {
                        // Insert new permission
                        string insertQuery = "INSERT INTO ROLE_PERMISSIONS (IdRole, IdPage, HasAccess) VALUES (@IdRole, @IdPage, @HasAccess)";
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@IdRole", idRole);
                            insertCmd.Parameters.AddWithValue("@IdPage", idPage);
                            insertCmd.Parameters.AddWithValue("@HasAccess", hasAccess);
                            return insertCmd.ExecuteNonQuery() > 0;
                        }
                    }
                }
                return true; // No change needed
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving permission: {ex.Message}");
                return false;
            }
        }

        private void ShowSuccess(string message)
        {
            string script = $"showToast('success', '{EscapeJavaScript(message)}');";
            ClientScript.RegisterStartupScript(this.GetType(), "Success", script, true);
        }

        private void ShowError(string message)
        {
            string script = $"showToast('error', '{EscapeJavaScript(message)}');";
            ClientScript.RegisterStartupScript(this.GetType(), "Error", script, true);
        }

        private void ShowWarning(string message)
        {
            string script = $"showToast('warning', '{EscapeJavaScript(message)}');";
            ClientScript.RegisterStartupScript(this.GetType(), "Warning", script, true);
        }

        private void ShowInfo(string message)
        {
            string script = $"showToast('info', '{EscapeJavaScript(message)}');";
            ClientScript.RegisterStartupScript(this.GetType(), "Info", script, true);
        }

        private string EscapeJavaScript(string text)
        {
            return text.Replace("'", "\\'").Replace("\r\n", "\\n").Replace("\n", "\\n").Replace("\"", "\\\"");
        }

        #endregion
    }
}