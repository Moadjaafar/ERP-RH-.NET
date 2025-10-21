using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.permissions
{
    public partial class RollesManagment : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadRoles();
            }
        }

        #region Data Loading Methods

        private void LoadRoles()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT IdRole, RoleName, Description, IsActive 
                        FROM ROLES 
                        ORDER BY RoleName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            gvRoles.DataSource = dt;
                            gvRoles.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des rôles: {ex.Message}");
            }
        }

        #endregion

        #region Add Role Methods

        protected void btnAddRole_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        // Check if role name already exists
                        string checkQuery = "SELECT COUNT(1) FROM ROLES WHERE RoleName = @RoleName";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@RoleName", txtRoleName.Text.Trim());
                            conn.Open();

                            if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                            {
                                ShowWarning("Ce nom de rôle existe déjà.");
                                return;
                            }
                        }

                        // Insert new role
                        string insertQuery = @"
                            INSERT INTO ROLES (RoleName, Description, IsActive) 
                            VALUES (@RoleName, @Description, @IsActive)";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@RoleName", txtRoleName.Text.Trim());
                            cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(txtDescription.Text) ? DBNull.Value : (object)txtDescription.Text.Trim());
                            cmd.Parameters.AddWithValue("@IsActive", chkIsActive.Checked);

                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                ShowSuccess("Rôle ajouté avec succès!");
                                ClearForm();
                                LoadRoles();
                            }
                            else
                            {
                                ShowError("Erreur lors de l'ajout du rôle.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Erreur: {ex.Message}");
                }
            }
        }

        private void ClearForm()
        {
            txtRoleName.Text = "";
            txtDescription.Text = "";
            chkIsActive.Checked = true;
        }

        #endregion

        #region GridView Event Handlers

        protected void gvRoles_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvRoles.EditIndex = e.NewEditIndex;
            LoadRoles();
        }

        protected void gvRoles_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvRoles.EditIndex = -1;
            LoadRoles();
        }

        protected void gvRoles_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                GridViewRow row = gvRoles.Rows[e.RowIndex];
                int idRole = Convert.ToInt32(gvRoles.DataKeys[e.RowIndex].Value);

                TextBox txtEditRoleName = (TextBox)row.FindControl("txtEditRoleName");
                TextBox txtEditDescription = (TextBox)row.FindControl("txtEditDescription");
                CheckBox chkEditIsActive = (CheckBox)row.FindControl("chkEditIsActive");

                if (string.IsNullOrWhiteSpace(txtEditRoleName.Text))
                {
                    ShowWarning("Le nom du rôle ne peut pas être vide.");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Check if role name already exists (excluding current role)
                    string checkQuery = "SELECT COUNT(1) FROM ROLES WHERE RoleName = @RoleName AND IdRole != @IdRole";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@RoleName", txtEditRoleName.Text.Trim());
                        checkCmd.Parameters.AddWithValue("@IdRole", idRole);
                        conn.Open();

                        if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                        {
                            ShowWarning("Ce nom de rôle existe déjà.");
                            return;
                        }
                    }

                    // Update role
                    string updateQuery = @"
                        UPDATE ROLES 
                        SET RoleName = @RoleName, 
                            Description = @Description, 
                            IsActive = @IsActive 
                        WHERE IdRole = @IdRole";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoleName", txtEditRoleName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(txtEditDescription.Text) ? DBNull.Value : (object)txtEditDescription.Text.Trim());
                        cmd.Parameters.AddWithValue("@IsActive", chkEditIsActive.Checked);
                        cmd.Parameters.AddWithValue("@IdRole", idRole);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowSuccess("Rôle modifié avec succès!");
                            gvRoles.EditIndex = -1;
                            LoadRoles();
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
                ShowError($"Erreur: {ex.Message}");
            }
        }

        protected void gvRoles_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int idRole = Convert.ToInt32(gvRoles.DataKeys[e.RowIndex].Value);

                // Debug: Add logging
                System.Diagnostics.Debug.WriteLine($"Attempting to delete role ID: {idRole}");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Check if role has associated permissions that might indicate it's in use
                    // Since EMPLOYE table doesn't have IdRole column, we'll check role permissions instead
                    string checkQuery = "SELECT COUNT(1) FROM ROLE_PERMISSIONS WHERE IdRole = @IdRole";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@IdRole", idRole);
                        int permissionCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        System.Diagnostics.Debug.WriteLine($"Permissions for this role: {permissionCount}");

                        // Optional warning if role has permissions (you can remove this if you want to allow deletion regardless)
                        if (permissionCount > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Role has {permissionCount} permissions, but proceeding with deletion...");
                        }
                    }

                    // Use transaction for data integrity
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Delete permissions first
                            string deletePermQuery = "DELETE FROM ROLE_PERMISSIONS WHERE IdRole = @IdRole";
                            using (SqlCommand deletePermCmd = new SqlCommand(deletePermQuery, conn, transaction))
                            {
                                deletePermCmd.Parameters.AddWithValue("@IdRole", idRole);
                                int permDeleted = deletePermCmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Permissions deleted: {permDeleted}");
                            }

                            // Delete role
                            string deleteQuery = "DELETE FROM ROLES WHERE IdRole = @IdRole";
                            using (SqlCommand cmd = new SqlCommand(deleteQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@IdRole", idRole);
                                int result = cmd.ExecuteNonQuery();

                                System.Diagnostics.Debug.WriteLine($"Roles deleted: {result}");

                                if (result > 0)
                                {
                                    transaction.Commit();
                                    ShowSuccess("Rôle supprimé avec succès!");
                                    LoadRoles();
                                }
                                else
                                {
                                    transaction.Rollback();
                                    ShowError("Aucun rôle trouvé à supprimer.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            System.Diagnostics.Debug.WriteLine($"Transaction error: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete error: {ex.Message}");
                ShowError($"Erreur lors de la suppression: {ex.Message}");
            }
        }

        #endregion

        #region Other Event Handlers

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadRoles();
            ShowInfo("Liste actualisée.");
        }

        #endregion

        #region Helper Methods

        private void ShowSuccess(string message)
        {
            // Escape single quotes for JavaScript
            string escapedMessage = message.Replace("'", "\\'").Replace("\r\n", "\\n").Replace("\n", "\\n");
            ClientScript.RegisterStartupScript(this.GetType(), "Success", $"showToast('success', '{escapedMessage}');", true);
        }

        private void ShowError(string message)
        {
            // Escape single quotes for JavaScript
            string escapedMessage = message.Replace("'", "\\'").Replace("\r\n", "\\n").Replace("\n", "\\n");
            ClientScript.RegisterStartupScript(this.GetType(), "Error", $"showToast('error', '{escapedMessage}');", true);
        }
        
        private void ShowWarning(string message)
        {
            // Escape single quotes for JavaScript
            string escapedMessage = message.Replace("'", "\\'").Replace("\r\n", "\\n").Replace("\n", "\\n");
            ClientScript.RegisterStartupScript(this.GetType(), "Warning", $"showToast('warning', '{escapedMessage}');", true);
        }
        
        private void ShowInfo(string message)
        {
            // Escape single quotes for JavaScript
            string escapedMessage = message.Replace("'", "\\'").Replace("\r\n", "\\n").Replace("\n", "\\n");
            ClientScript.RegisterStartupScript(this.GetType(), "Info", $"showToast('info', '{escapedMessage}');", true);
        }

        #endregion
    }
}