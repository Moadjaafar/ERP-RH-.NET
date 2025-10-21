using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.permissions
{
    public partial class GestionMotsDePasse : System.Web.UI.Page
    {
        private string connectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Initialize connection string
                connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"]?.ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("Connection string not found.");
                }

                // Check if user is authenticated and has permission
                if (Session["EmployeId"] == null)
                {
                    Response.Redirect("~/connexion.aspx");
                    return;
                }

                // Check if user has admin privileges
                if (!HasPasswordManagementPermission())
                {
                    Response.Redirect("~/AccessDenied.aspx");
                    return;
                }

                if (!IsPostBack)
                {
                    LoadRoles();
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors du chargement de la page: {ex.Message}", "danger");
            }
        }

        private bool HasPasswordManagementPermission()
        {
            try
            {
                string currentUserRole = GetCurrentUserRole();
                // Only Admin and specific security roles can manage passwords
                return currentUserRole == "Admin" || currentUserRole == "Dev";
            }
            catch
            {
                return false;
            }
        }

        private string GetCurrentUserRole()
        {
            try
            {
                string employeId = Session["EmployeId"]?.ToString();
                if (string.IsNullOrEmpty(employeId)) return null;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT Rolle FROM EMPLOYE WHERE IdEmploye = @EmployeId AND is_deleted = 0";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeId", employeId);
                        conn.Open();
                        return cmd.ExecuteScalar()?.ToString();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private void LoadRoles()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT DISTINCT Rolle FROM EMPLOYE WHERE is_deleted = 0 AND Rolle IS NOT NULL ORDER BY Rolle";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            ddlRoleFilter.Items.Clear();
                            ddlRoleFilter.Items.Add(new ListItem("Tous les Rôles", ""));

                            while (reader.Read())
                            {
                                string role = reader["Rolle"].ToString();
                                if (!string.IsNullOrEmpty(role))
                                {
                                    ddlRoleFilter.Items.Add(new ListItem(role, role));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors du chargement des rôles: {ex.Message}", "danger");
            }
        }

        private void LoadUsers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = BuildUserQuery();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        AddSearchParameters(cmd);

                        conn.Open();
                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        // Add computed column for full name
                        dt.Columns.Add("NomPrenom", typeof(string));
                        foreach (DataRow row in dt.Rows)
                        {
                            string nom = row["Nom"]?.ToString() ?? "";
                            string prenom = row["Prenom"]?.ToString() ?? "";
                            row["NomPrenom"] = $"{prenom} {nom}".Trim();
                        }

                        gvUsers.DataSource = dt;
                        gvUsers.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors du chargement des utilisateurs: {ex.Message}", "danger");
            }
        }

        private string BuildUserQuery()
        {
            StringBuilder query = new StringBuilder();
            query.Append(@"
                SELECT 
                    IdEmploye, 
                    Matricule, 
                    Nom, 
                    Prenom, 
                    Email, 
                    Rolle, 
                    PasswordHash,
                    DateCreation
                FROM EMPLOYE 
                WHERE is_deleted = 0 
                    AND Rolle IS NOT NULL");

            // Add search filter
            string searchText = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                query.Append(" AND (Nom LIKE @SearchText OR Prenom LIKE @SearchText OR Matricule LIKE @SearchText OR Email LIKE @SearchText)");
            }

            // Add role filter
            if (!string.IsNullOrEmpty(ddlRoleFilter.SelectedValue))
            {
                query.Append(" AND Rolle = @RoleFilter");
            }

            // Add password status filter
            string passwordStatus = ddlPasswordStatus.SelectedValue;
            if (passwordStatus == "HasPassword")
            {
                query.Append(" AND PasswordHash IS NOT NULL AND PasswordHash != ''");
            }
            else if (passwordStatus == "NoPassword")
            {
                query.Append(" AND (PasswordHash IS NULL OR PasswordHash = '')");
            }

            query.Append(" ORDER BY Nom, Prenom");
            return query.ToString();
        }

        private void AddSearchParameters(SqlCommand cmd)
        {
            string searchText = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                cmd.Parameters.AddWithValue("@SearchText", "%" + searchText + "%");
            }

            if (!string.IsNullOrEmpty(ddlRoleFilter.SelectedValue))
            {
                cmd.Parameters.AddWithValue("@RoleFilter", ddlRoleFilter.SelectedValue);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }

        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadUsers();
        }

        protected void ddlRoleFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadUsers();
        }

        protected void ddlPasswordStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadUsers();
        }

        protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsers.PageIndex = e.NewPageIndex;
            LoadUsers();
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int employeeId = Convert.ToInt32(e.CommandArgument);

                if (e.CommandName == "EditUser")
                {
                    ShowEditForm(employeeId);
                }
                else if (e.CommandName == "GeneratePassword")
                {
                    GenerateAndSaveRandomPassword(employeeId);
                }
                else if (e.CommandName == "ResetEmail")
                {
                    ResetEmployeeEmail(employeeId);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur: {ex.Message}", "danger");
            }
        }

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //if (e.Row.RowType == DataRowType.DataRow)
            //{
            //    // You can add any additional row formatting here if needed
            //}
        }

        private void ShowEditForm(int employeeId)
        {
            try
            {
                // Get employee information
                var employeeInfo = GetEmployeeInfo(employeeId);

                // Set form values
                hfSelectedEmployeeId.Value = employeeId.ToString();
                litSelectedEmployee.Text = $"{employeeInfo.Prenom} {employeeInfo.Nom}".Trim();

                // Set current email
                txtEmail.Text = employeeInfo.Email ?? "";

                // Clear password fields
                txtNewPassword.Text = "";
                txtConfirmPassword.Text = "";
                chkForcePasswordChange.Checked = false;

                // Show the edit form panel
                pnlPasswordForm.Visible = true;

                // Hide any previous messages
                pnlMessages.Visible = false;

                // Scroll to the form (optional)
                ScriptManager.RegisterStartupScript(this, GetType(), "ScrollToForm",
                    "document.getElementById('" + pnlPasswordForm.ClientID + "').scrollIntoView();", true);
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors de l'affichage du formulaire: {ex.Message}", "danger");
            }
        }

        protected void btnCancelPasswordChange_Click(object sender, EventArgs e)
        {
            // Hide the form
            pnlPasswordForm.Visible = false;

            // Clear form fields
            txtEmail.Text = "";
            txtNewPassword.Text = "";
            txtConfirmPassword.Text = "";
            chkForcePasswordChange.Checked = false;
            hfSelectedEmployeeId.Value = "";

            // Hide messages
            pnlMessages.Visible = false;
        }

        protected void btnGeneratePassword_Click(object sender, EventArgs e)
        {
            try
            {
                // Generate a secure password
                string newPassword = GenerateSecurePassword();

                // Set the password in the form fields
                txtNewPassword.Text = newPassword;
                txtConfirmPassword.Text = newPassword;

                // Show success message
                ShowMessage($"Mot de passe généré: <strong>{newPassword}</strong><br/>" +
                           "<small class='text-warning'>⚠️ Notez ce mot de passe pour le communiquer à l'utilisateur.</small>", "info");
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors de la génération: {ex.Message}", "danger");
            }
        }

        private void GenerateAndSaveRandomPassword(int employeeId)
        {
            try
            {
                string newPassword = GenerateSecurePassword();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE EMPLOYE 
                        SET PasswordHash = @Password, 
                            DateModification = @DateModification,
                            ModifiePar = @ModifiePar
                        WHERE IdEmploye = @EmployeId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeId", employeeId);
                        cmd.Parameters.AddWithValue("@Password", newPassword); // Store plain text
                        cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ModifiePar", Session["EmployeId"].ToString());

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            // Log the action
                            LogPasswordAction(employeeId, "Password Generated", Session["EmployeId"].ToString());

                            string employeeName = GetEmployeeName(employeeId);
                            string message = $"Mot de passe généré pour <strong>{employeeName}</strong>.<br/>" +
                                           $"Nouveau mot de passe: <strong>{newPassword}</strong><br/>" +
                                           "<small class='text-warning'>⚠️ Notez ce mot de passe et communiquez-le à l'utilisateur de manière sécurisée.</small>";

                            ShowMessage(message, "success");
                            LoadUsers(); // Refresh grid
                        }
                        else
                        {
                            ShowMessage("Erreur lors de la génération du mot de passe.", "danger");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors de la génération du mot de passe: {ex.Message}", "danger");
            }
        }

        private string GetEmployeeName(int employeeId)
        {
            var info = GetEmployeeInfo(employeeId);
            string fullName = $"{info.Prenom} {info.Nom}".Trim();
            return string.IsNullOrEmpty(fullName) ? "Utilisateur" : fullName;
        }

        protected void btnSavePassword_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsValid)
                    return;

                int employeeId = Convert.ToInt32(hfSelectedEmployeeId.Value);
                string newEmail = txtEmail.Text.Trim();
                string newPassword = txtNewPassword.Text.Trim();
                string confirmPassword = txtConfirmPassword.Text.Trim();

                // Server-side validation for email
                if (string.IsNullOrEmpty(newEmail))
                {
                    ShowMessage("L'email est requis.", "danger");
                    return;
                }

                // Validate password only if provided
                bool updatePassword = !string.IsNullOrEmpty(newPassword);
                if (updatePassword)
                {
                    if (newPassword.Length < 4) // Minimal validation
                    {
                        ShowMessage("Le mot de passe doit contenir au moins 4 caractères.", "danger");
                        return;
                    }

                    if (newPassword != confirmPassword)
                    {
                        ShowMessage("Les mots de passe ne correspondent pas.", "danger");
                        return;
                    }
                }

                // Build update query
                string query;
                if (updatePassword)
                {
                    query = @"
                        UPDATE EMPLOYE 
                        SET Email = @Email,
                            PasswordHash = @Password, 
                            DateModification = @DateModification,
                            ModifiePar = @ModifiePar
                        WHERE IdEmploye = @EmployeId";
                }
                else
                {
                    query = @"
                        UPDATE EMPLOYE 
                        SET Email = @Email,
                            DateModification = @DateModification,
                            ModifiePar = @ModifiePar
                        WHERE IdEmploye = @EmployeId";
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeId", employeeId);
                        cmd.Parameters.AddWithValue("@Email", newEmail);
                        cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ModifiePar", Session["EmployeId"].ToString());

                        if (updatePassword)
                        {
                            cmd.Parameters.AddWithValue("@Password", newPassword); // Store plain text
                        }

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            // Log the action
                            string action = updatePassword ? "Email and Password Updated" : "Email Updated";
                            LogPasswordAction(employeeId, action, Session["EmployeId"].ToString());

                            string employeeName = GetEmployeeName(employeeId);
                            string message = updatePassword
                                ? $"Email et mot de passe modifiés avec succès pour <strong>{employeeName}</strong>."
                                : $"Email modifié avec succès pour <strong>{employeeName}</strong>.";

                            ShowMessage(message, "success");

                            // Clear and hide form
                            txtEmail.Text = "";
                            txtNewPassword.Text = "";
                            txtConfirmPassword.Text = "";
                            chkForcePasswordChange.Checked = false;
                            hfSelectedEmployeeId.Value = "";
                            pnlPasswordForm.Visible = false;

                            LoadUsers(); // Refresh grid
                        }
                        else
                        {
                            ShowMessage("Erreur lors de la modification des informations.", "danger");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors de la sauvegarde: {ex.Message}", "danger");
            }
        }

        private void ResetEmployeeEmail(int employeeId)
        {
            try
            {
                string defaultEmail = GenerateDefaultEmail(employeeId);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE EMPLOYE 
                        SET Email = @Email, 
                            DateModification = @DateModification,
                            ModifiePar = @ModifiePar
                        WHERE IdEmploye = @EmployeId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeId", employeeId);
                        cmd.Parameters.AddWithValue("@Email", defaultEmail);
                        cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ModifiePar", Session["EmployeId"].ToString());

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            // Log the action
                            LogPasswordAction(employeeId, "Email Reset", Session["EmployeId"].ToString());

                            string employeeName = GetEmployeeName(employeeId);
                            ShowMessage($"Email réinitialisé pour <strong>{employeeName}</strong>.<br/>Nouvel email: <strong>{defaultEmail}</strong>", "success");
                            LoadUsers(); // Refresh grid
                        }
                        else
                        {
                            ShowMessage("Erreur lors de la réinitialisation de l'email.", "danger");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors de la réinitialisation de l'email: {ex.Message}", "danger");
            }
        }

        private string GenerateDefaultEmail(int employeeId)
        {
            try
            {
                var employeeInfo = GetEmployeeInfo(employeeId);
                // Generate email like: prenom.nom@company.com
                string prenom = employeeInfo.Prenom?.ToLower().Replace(" ", "") ?? "user";
                string nom = employeeInfo.Nom?.ToLower().Replace(" ", "") ?? employeeId.ToString();
                return $"{prenom}.{nom}@kingpelqgiaue.com";
            }
            catch
            {
                return $"user{employeeId}@kingpelqgiaue.com";
            }
        }

        protected void btnResetEmail_Click(object sender, EventArgs e)
        {
            try
            {
                int employeeId = Convert.ToInt32(hfSelectedEmployeeId.Value);
                string defaultEmail = GenerateDefaultEmail(employeeId);
                txtEmail.Text = defaultEmail;

                ShowMessage($"Email réinitialisé à: <strong>{defaultEmail}</strong><br/><small>N'oubliez pas de cliquer sur 'Enregistrer Modifications' pour sauvegarder.</small>", "info");
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors de la réinitialisation: {ex.Message}", "danger");
            }
        }

        private (string Nom, string Prenom, string Email, string PasswordHash) GetEmployeeInfo(int employeeId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT Nom, Prenom, Email, PasswordHash FROM EMPLOYE WHERE IdEmploye = @EmployeId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeId", employeeId);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return (
                                    reader["Nom"]?.ToString() ?? "",
                                    reader["Prenom"]?.ToString() ?? "",
                                    reader["Email"]?.ToString() ?? "",
                                    reader["PasswordHash"]?.ToString() ?? ""
                                );
                            }
                        }
                    }
                }
            }
            catch { }
            return ("", "", "", "");
        }

        private string GenerateSecurePassword()
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string specialChars = "!@#$%";

            Random random = new Random();
            StringBuilder password = new StringBuilder();

            // Ensure at least one character from each type
            password.Append(uppercase[random.Next(uppercase.Length)]);
            password.Append(lowercase[random.Next(lowercase.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(specialChars[random.Next(specialChars.Length)]);

            // Fill the rest with random characters
            string allChars = lowercase + uppercase + digits + specialChars;
            for (int i = password.Length; i < 8; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the password
            char[] chars = password.ToString().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                int randomIndex = random.Next(chars.Length);
                char temp = chars[i];
                chars[i] = chars[randomIndex];
                chars[randomIndex] = temp;
            }

            return new string(chars);
        }

        private void LogPasswordAction(int employeeId, string action, string performedBy)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Check if audit table exists (create if it doesn't)
                    string createTableQuery = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PASSWORD_AUDIT' AND xtype='U')
                        CREATE TABLE PASSWORD_AUDIT (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            EmployeeId INT NOT NULL,
                            Action VARCHAR(100) NOT NULL,
                            PerformedBy INT NOT NULL,
                            ActionDate DATETIME NOT NULL,
                            IPAddress VARCHAR(45)
                        )";

                    using (SqlCommand createCmd = new SqlCommand(createTableQuery, conn))
                    {
                        conn.Open();
                        createCmd.ExecuteNonQuery();
                    }

                    // Insert audit record
                    string insertQuery = @"
                        INSERT INTO PASSWORD_AUDIT (EmployeeId, Action, PerformedBy, ActionDate, IPAddress)
                        VALUES (@EmployeeId, @Action, @PerformedBy, @ActionDate, @IPAddress)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                        cmd.Parameters.AddWithValue("@Action", action);
                        cmd.Parameters.AddWithValue("@PerformedBy", performedBy);
                        cmd.Parameters.AddWithValue("@ActionDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@IPAddress", GetClientIPAddress());

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Silent fail for audit logging
            }
        }

        private string GetClientIPAddress()
        {
            try
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                }
                return ipAddress ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private void ShowMessage(string message, string alertType)
        {
            litMessage.Text = message;

            // Set alert class based on type
            string alertClass = "alert-info";
            switch (alertType.ToLower())
            {
                case "success":
                    alertClass = "alert-success";
                    break;
                case "danger":
                case "error":
                    alertClass = "alert-danger";
                    break;
                case "warning":
                    alertClass = "alert-warning";
                    break;
                case "info":
                    alertClass = "alert-info";
                    break;
            }

            alertDiv.Attributes["class"] = $"alert {alertClass} alert-dismissible fade show";
            pnlMessages.Visible = true;
        }
    }
}