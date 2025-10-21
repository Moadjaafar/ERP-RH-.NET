using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.permissions
{
    public partial class Profile : Page
    {
        private string connectionString;
        private int currentUserId;

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

                // Check if user is authenticated
                if (Session["EmployeId"] == null)
                {
                    Response.Redirect("~/connexion.aspx");
                    return;
                }

                currentUserId = Convert.ToInt32(Session["EmployeId"]);

                if (!IsPostBack)
                {
                    LoadUserProfile();
                    LoadUserActivity();
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors du chargement de la page: {ex.Message}", "danger");
            }
        }

        private void LoadUserProfile()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            e.Matricule, 
                            e.Nom, 
                            e.Prenom, 
                            e.Email, 
                            e.Telephone,
                            e.Rolle,
                            e.DateEmbauche,
                            e.PasswordHash,
                            d.NomDepartement
                        FROM EMPLOYE e
                        LEFT JOIN DEPARTEMENT d ON e.IdDepartement = d.IdDepartement
                        WHERE e.IdEmploye = @EmployeId AND e.is_deleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeId", currentUserId);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Display user information
                                string prenom = reader["Prenom"]?.ToString() ?? "";
                                string nom = reader["Nom"]?.ToString() ?? "";
                                litFullName.Text = $"{prenom} {nom}".Trim();

                                litMatricule.Text = reader["Matricule"]?.ToString() ?? "N/A";
                                litRole.Text = reader["Rolle"]?.ToString() ?? "Aucun rôle";
                                litDepartement.Text = reader["NomDepartement"]?.ToString() ?? "N/A";

                                string email = reader["Email"]?.ToString() ?? "";
                                litCurrentEmail.Text = string.IsNullOrEmpty(email) ? "Aucun email" : email;
                                txtNewEmail.Text = email; // Pre-fill email field

                                litTelephone.Text = reader["Telephone"]?.ToString() ?? "N/A";

                                DateTime dateEmbauche = Convert.ToDateTime(reader["DateEmbauche"]);
                                litDateEmbauche.Text = dateEmbauche.ToString("dd/MM/yyyy");

                                string currentPassword = reader["PasswordHash"]?.ToString() ?? "";
                                litCurrentPassword.Text = string.IsNullOrEmpty(currentPassword) ? "Aucun mot de passe défini" : currentPassword;
                            }
                            else
                            {
                                ShowMessage("Impossible de charger les informations du profil.", "danger");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors du chargement du profil: {ex.Message}", "danger");
            }
        }

        private void LoadUserActivity()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Check if PASSWORD_AUDIT table exists
                    string checkTableQuery = @"
                        SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME = 'PASSWORD_AUDIT'";

                    using (SqlCommand checkCmd = new SqlCommand(checkTableQuery, conn))
                    {
                        conn.Open();
                        int tableExists = (int)checkCmd.ExecuteScalar();

                        if (tableExists == 0)
                        {
                            gvUserActivity.Visible = false;
                            return;
                        }
                    }

                    // Load user activity
                    string query = @"
                        SELECT TOP 20
                            Action,
                            ActionDate,
                            IPAddress
                        FROM PASSWORD_AUDIT
                        WHERE EmployeeId = @EmployeId
                        ORDER BY ActionDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeId", currentUserId);

                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        gvUserActivity.DataSource = dt;
                        gvUserActivity.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                // Hide activity section if there's an error
                gvUserActivity.Visible = false;
            }
        }

        protected void btnUpdateEmail_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsValid)
                    return;

                string newEmail = txtNewEmail.Text.Trim();

                if (string.IsNullOrEmpty(newEmail))
                {
                    ShowMessage("L'email est requis.", "danger");
                    return;
                }

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
                        cmd.Parameters.AddWithValue("@EmployeId", currentUserId);
                        cmd.Parameters.AddWithValue("@Email", newEmail);
                        cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ModifiePar", currentUserId);

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            // Log the action
                            LogUserActivity("Profile Email Updated");

                            ShowMessage("Email mis à jour avec succès!", "success");

                            // Refresh profile display
                            LoadUserProfile();
                        }
                        else
                        {
                            ShowMessage("Erreur lors de la mise à jour de l'email.", "danger");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors de la mise à jour de l'email: {ex.Message}", "danger");
            }
        }

        protected void btnUpdatePassword_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsValid)
                    return;

                string currentPassword = txtCurrentPassword.Text.Trim();
                string newPassword = txtNewPassword.Text.Trim();
                string confirmPassword = txtConfirmNewPassword.Text.Trim();

                // Validate inputs
                if (string.IsNullOrEmpty(currentPassword))
                {
                    ShowMessage("Le mot de passe actuel est requis.", "danger");
                    return;
                }

                if (string.IsNullOrEmpty(newPassword))
                {
                    ShowMessage("Le nouveau mot de passe est requis.", "danger");
                    return;
                }

                if (newPassword.Length < 4)
                {
                    ShowMessage("Le nouveau mot de passe doit contenir au moins 4 caractères.", "danger");
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    ShowMessage("Les nouveaux mots de passe ne correspondent pas.", "danger");
                    return;
                }

                // Verify current password
                if (!VerifyCurrentPassword(currentPassword))
                {
                    ShowMessage("Le mot de passe actuel est incorrect.", "danger");
                    return;
                }

                // Update password
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
                        cmd.Parameters.AddWithValue("@EmployeId", currentUserId);
                        cmd.Parameters.AddWithValue("@Password", newPassword); // Store plain text
                        cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ModifiePar", currentUserId);

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            // Log the action
                            LogUserActivity("Profile Password Changed");

                            ShowMessage("Mot de passe changé avec succès!", "success");

                            // Clear password fields
                            txtCurrentPassword.Text = "";
                            txtNewPassword.Text = "";
                            txtConfirmNewPassword.Text = "";

                            // Refresh profile display
                            LoadUserProfile();
                            LoadUserActivity();
                        }
                        else
                        {
                            ShowMessage("Erreur lors du changement de mot de passe.", "danger");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors du changement de mot de passe: {ex.Message}", "danger");
            }
        }

        private bool VerifyCurrentPassword(string enteredPassword)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT PasswordHash FROM EMPLOYE WHERE IdEmploye = @EmployeId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeId", currentUserId);
                        conn.Open();

                        string storedPassword = cmd.ExecuteScalar()?.ToString() ?? "";

                        // Simple comparison since passwords are stored as plain text
                        return storedPassword == enteredPassword;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        protected void btnGenerateRandomPassword_Click(object sender, EventArgs e)
        {
            try
            {
                string randomPassword = GenerateSecurePassword();

                // Fill the new password fields
                txtNewPassword.Text = randomPassword;
                txtConfirmNewPassword.Text = randomPassword;

                ShowMessage($"Mot de passe généré: <strong>{randomPassword}</strong><br/>" +
                           "<small class='text-warning'>⚠️ N'oubliez pas de cliquer sur 'Changer Mot de Passe' pour sauvegarder.</small>", "info");
            }
            catch (Exception ex)
            {
                ShowMessage($"Erreur lors de la génération du mot de passe: {ex.Message}", "danger");
            }
        }

        protected void btnClearPasswordForm_Click(object sender, EventArgs e)
        {
            txtCurrentPassword.Text = "";
            txtNewPassword.Text = "";
            txtConfirmNewPassword.Text = "";
            pnlMessages.Visible = false;
        }

        protected void gvUserActivity_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUserActivity.PageIndex = e.NewPageIndex;
            LoadUserActivity();
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

        private void LogUserActivity(string action)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Check if audit table exists first
                    string checkTableQuery = @"
                        SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME = 'PASSWORD_AUDIT'";

                    using (SqlCommand checkCmd = new SqlCommand(checkTableQuery, conn))
                    {
                        conn.Open();
                        int tableExists = (int)checkCmd.ExecuteScalar();
                        if (tableExists == 0)
                        {
                            // Create the audit table
                            string createTableQuery = @"
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
                                createCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // Insert audit record
                    string insertQuery = @"
                        INSERT INTO PASSWORD_AUDIT (EmployeeId, Action, PerformedBy, ActionDate, IPAddress)
                        VALUES (@EmployeeId, @Action, @PerformedBy, @ActionDate, @IPAddress)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", currentUserId);
                        cmd.Parameters.AddWithValue("@Action", action);
                        cmd.Parameters.AddWithValue("@PerformedBy", currentUserId);
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