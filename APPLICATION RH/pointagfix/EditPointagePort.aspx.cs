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
    public partial class EditPointagePort : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Initialize page
                InitializePage();
                HandleQueryStringMessages();
            }
        }

        private void InitializePage()
        {
            PopulatePortDropdown();
            SetDefaultDateRange();
        }

        //private void PopulatePortDropdown()
        //{
        //    string userRole = Session["EmployeRole"]?.ToString();
        //    int EmployeId = Convert.ToInt32(Session["EmployeId"]?.ToString());

        //    // Clear existing items
        //    ddlPort.Items.Clear();

        //    if (userRole == "securityPortA")
        //    {
        //        // Only show Port A for securityPortA users
        //        ddlPort.Items.Add(new ListItem("Port A - Entrée Principale", "Port A - Entrée Principale"));
        //        ddlPort.SelectedIndex = 0;
        //        ddlPort.Enabled = false;
        //    }
        //    else
        //    {
        //        // Show all ports for other users
        //        ddlPort.Items.Add(new ListItem("toutes les portes", ""));
        //        ddlPort.Items.Add(new ListItem("Porte A - Entrée Principal", "Porte A - Entrée Principal"));
        //        ddlPort.Items.Add(new ListItem("Porte B - Entrée GSF", "Porte B - Entrée GSF"));
        //        ddlPort.Items.Add(new ListItem("Porte C - Entrée POI", "Porte C - Entrée POI"));
        //        ddlPort.Items.Add(new ListItem("Porte D - Entrée Technique", "Porte D - Entrée Technique"));
        //        ddlPort.Items.Add(new ListItem("Porte E - Entrée ERG PACKAGING", "Porte E - Entrée ERG PACKAGING"));
        //        ddlPort.Enabled = true;
        //    }
        //}

        private void PopulatePortDropdown()
        {
            int employeId = Convert.ToInt32(Session["EmployeId"]?.ToString());

            // Clear existing items
            ddlPort.Items.Clear();

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            string query = @"
        SELECT DISTINCT p.* 
        FROM AccessLogine al
        INNER JOIN PORTEUSINE p ON al.IDPorteAssigne = p.idPorte
        WHERE al.IdEmploye = @EmployeId";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeId", employeId);

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            int portCount = 0;

                            while (reader.Read())
                            {
                                // Use column index to avoid column name errors
                                string portName = reader[1].ToString(); // Adjust index based on your table structure
                                ddlPort.Items.Add(new ListItem(portName, portName));
                                portCount++;
                            }

                            if (portCount == 0)
                            {
                                ddlPort.Items.Add(new ListItem("Aucune porte assignée", ""));
                                ddlPort.Enabled = false;
                            }
                            else if (portCount == 1)
                            {
                                ddlPort.SelectedIndex = 0;
                                ddlPort.Enabled = false;
                            }
                            else
                            {
                                ddlPort.Items.Insert(0, new ListItem("Toutes mes portes assignées", ""));
                                ddlPort.Enabled = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ddlPort.Items.Add(new ListItem("Erreur lors du chargement des portes", ""));
                ddlPort.Enabled = false;
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        private void SetDefaultDateRange()
        {
            // Default to last 7 days
            txtDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
        }

        private void HandleQueryStringMessages()
        {
            string success = Request.QueryString["success"];
            string error = Request.QueryString["error"];

            if (!string.IsNullOrEmpty(success))
            {
                ShowSuccess(HttpUtility.UrlDecode(success));
            }

            if (!string.IsNullOrEmpty(error))
            {
                ShowError(HttpUtility.UrlDecode(error));
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            SearchPointages();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearFilters();
        }

        protected void btnSetToday_Click(object sender, EventArgs e)
        {
            txtDateFrom.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");
            SearchPointages();
        }

        private void ClearFilters()
        {
            if (ddlPort.Enabled)
            {
                ddlPort.SelectedIndex = 0;
            }
            txtEmployeeSearch.Text = "";
            ddlTypePointage.SelectedIndex = 0;
            SetDefaultDateRange();
            pnlResults.Visible = false;
            litResultCount.Text = "";
        }

        private void SearchPointages()
        {
            try
            {
                // Validate date range
                if (!ValidateDateRange())
                {
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = BuildSearchQuery();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        AddSearchParameters(cmd);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            gvPointages.DataSource = dt;
                            gvPointages.DataBind();

                            // Update result count
                            int totalCount = dt.Rows.Count;
                            int activeCount = dt.AsEnumerable().Count(row => !Convert.ToBoolean(row["is_deleted"]));
                            int canceledCount = totalCount - activeCount;

                            litResultCount.Text = $"<small class='text-muted'>{totalCount} résultat(s) trouvé(s) " +
                                                 $"({activeCount} actif(s), {canceledCount} annulé(s))</small>";

                            pnlResults.Visible = totalCount > 0;

                            if (totalCount == 0)
                            {
                                ShowWarning("Aucun pointage trouvé avec les critères sélectionnés.");
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

        private bool ValidateDateRange()
        {
            if (string.IsNullOrEmpty(txtDateFrom.Text) || string.IsNullOrEmpty(txtDateTo.Text))
            {
                ShowError("Veuillez sélectionner une période de recherche valide.");
                return false;
            }

            DateTime dateFrom, dateTo;
            if (!DateTime.TryParse(txtDateFrom.Text, out dateFrom) || !DateTime.TryParse(txtDateTo.Text, out dateTo))
            {
                ShowError("Format de date invalide.");
                return false;
            }

            if (dateFrom > dateTo)
            {
                ShowError("La date de début ne peut pas être postérieure à la date de fin.");
                return false;
            }

            // Limit search range to avoid performance issues
            if ((dateTo - dateFrom).TotalDays > 365)
            {
                ShowError("La période de recherche ne peut pas dépasser 365 jours.");
                return false;
            }

            return true;
        }

        private string BuildSearchQuery()
        {
            string query = @"
                SELECT 
                    p.IdPointage,
                    e.Nom + ' ' + e.Prenom AS NomEmploye,
                    e.Matricule AS MatriculeEmploye,
                    p.NomPort,
                    p.TypePointage,
                    p.DateHeure,
                    ISNULL(assigneur.Nom + ' ' + assigneur.Prenom, 'Système') AS AssignePar,
                    p.is_deleted,
                    p.DateCreation,
                    p.DateModification
                FROM EMPLOYE_POINTAGE p
                INNER JOIN EMPLOYE e ON p.IdEmploye = e.IdEmploye
                LEFT JOIN EMPLOYE assigneur ON p.AssignePar = assigneur.IdEmploye
                WHERE CAST(p.DateHeure AS DATE) BETWEEN @DateFrom AND @DateTo";

            // Add port filter
            if (!string.IsNullOrEmpty(ddlPort.SelectedValue))
            {
                query += " AND p.NomPort = @NomPort";
            }

            // Add employee filter
            if (!string.IsNullOrEmpty(txtEmployeeSearch.Text.Trim()))
            {
                query += " AND (e.Nom LIKE @EmployeeSearch OR e.Prenom LIKE @EmployeeSearch OR e.Matricule LIKE @EmployeeSearch)";
            }

            // Add type filter
            if (!string.IsNullOrEmpty(ddlTypePointage.SelectedValue))
            {
                query += " AND p.TypePointage = @TypePointage";
            }

            query += " ORDER BY p.DateHeure DESC, e.Nom, e.Prenom";

            return query;
        }

        private void AddSearchParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@DateFrom", DateTime.Parse(txtDateFrom.Text).Date);
            cmd.Parameters.AddWithValue("@DateTo", DateTime.Parse(txtDateTo.Text).Date);

            if (!string.IsNullOrEmpty(ddlPort.SelectedValue))
            {
                cmd.Parameters.AddWithValue("@NomPort", ddlPort.SelectedValue);
            }

            if (!string.IsNullOrEmpty(txtEmployeeSearch.Text.Trim()))
            {
                cmd.Parameters.AddWithValue("@EmployeeSearch", "%" + txtEmployeeSearch.Text.Trim() + "%");
            }

            if (!string.IsNullOrEmpty(ddlTypePointage.SelectedValue))
            {
                cmd.Parameters.AddWithValue("@TypePointage", ddlTypePointage.SelectedValue);
            }
        }

        protected void gvPointages_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "CancelPointage")
            {
                int pointageId = Convert.ToInt32(e.CommandArgument);
                CancelPointage(pointageId);
            }
            else if (e.CommandName == "RestorePointage")
            {
                int pointageId = Convert.ToInt32(e.CommandArgument);
                RestorePointage(pointageId);
            }
        }

        protected void gvPointages_RowEditing(object sender, GridViewEditEventArgs e)
        {
            // Check if this pointage can be edited
            int pointageId = Convert.ToInt32(gvPointages.DataKeys[e.NewEditIndex].Value);

            if (!CanEditPointage(pointageId))
            {
                ShowError("Ce pointage ne peut pas être modifié car cela pourrait causer des incohérences dans les données.");
                return;
            }

            gvPointages.EditIndex = e.NewEditIndex;
            SearchPointages();
        }

        protected void gvPointages_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int pointageId = Convert.ToInt32(gvPointages.DataKeys[e.RowIndex].Value);

                TextBox txtEditDate = (TextBox)gvPointages.Rows[e.RowIndex].FindControl("txtEditDate");
                TextBox txtEditTime = (TextBox)gvPointages.Rows[e.RowIndex].FindControl("txtEditTime");

                if (txtEditDate != null && txtEditTime != null)
                {
                    DateTime newDate = DateTime.Parse(txtEditDate.Text);
                    TimeSpan newTime = TimeSpan.Parse(txtEditTime.Text);
                    DateTime newDateTime = newDate.Date.Add(newTime);

                    if (UpdatePointage(pointageId, newDateTime))
                    {
                        gvPointages.EditIndex = -1;
                        SearchPointages();
                        ShowSuccess("Pointage mis à jour avec succès.");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de la mise à jour: " + ex.Message);
            }
        }

        protected void gvPointages_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvPointages.EditIndex = -1;
            SearchPointages();
        }

        private bool CanEditPointage(int pointageId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Get pointage details
                    string query = @"
                        SELECT IdEmploye, TypePointage, DateHeure, is_deleted
                        FROM EMPLOYE_POINTAGE 
                        WHERE IdPointage = @IdPointage";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdPointage", pointageId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Can't edit if already deleted
                                if (Convert.ToBoolean(reader["is_deleted"]))
                                {
                                    return false;
                                }

                                int employeeId = Convert.ToInt32(reader["IdEmploye"]);
                                string typePointage = reader["TypePointage"].ToString();
                                DateTime dateHeure = Convert.ToDateTime(reader["DateHeure"]);

                                // Additional business rules can be added here
                                // For example: Can't edit pointages older than X days
                                if ((DateTime.Now - dateHeure).TotalDays > 30)
                                {
                                    return false;
                                }

                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private bool UpdatePointage(int pointageId, DateTime newDateTime)
        {
            try
            {
                // Validate the new date/time
                if (!ValidatePointageDateTime(pointageId, newDateTime))
                {
                    return false;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE EMPLOYE_POINTAGE 
                        SET DateHeure = @DateHeure, 
                            DateModification = @DateModification,
                            ModifiePar = @ModifiePar
                        WHERE IdPointage = @IdPointage";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdPointage", pointageId);
                        cmd.Parameters.AddWithValue("@DateHeure", newDateTime);
                        cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ModifiePar", GetCurrentUserId());

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de la mise à jour: " + ex.Message);
                return false;
            }
        }

        private bool ValidatePointageDateTime(int pointageId, DateTime newDateTime)
        {
            // Basic validations
            if (newDateTime > DateTime.Now)
            {
                ShowError("La date/heure ne peut pas être dans le futur.");
                return false;
            }

            if (newDateTime < DateTime.Now.AddDays(-90))
            {
                ShowError("La date/heure ne peut pas être antérieure à 90 jours.");
                return false;
            }

            // Additional business logic validation can be added here
            // For example: Check if the new time doesn't conflict with other pointages

            return true;
        }

        private void CancelPointage(int pointageId)
        {
            try
            {
                if (!CanCancelPointage(pointageId))
                {
                    ShowError("Ce pointage ne peut pas être annulé car cela pourrait causer des incohérences dans les données.");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE EMPLOYE_POINTAGE 
                        SET is_deleted = 1, 
                            DateModification = @DateModification,
                            ModifiePar = @ModifiePar
                        WHERE IdPointage = @IdPointage";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdPointage", pointageId);
                        cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ModifiePar", GetCurrentUserId());

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            SearchPointages();
                            ShowSuccess("Pointage annulé avec succès.");
                        }
                        else
                        {
                            ShowError("Erreur lors de l'annulation du pointage.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de l'annulation: " + ex.Message);
            }
        }

        private bool CanCancelPointage(int pointageId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Get pointage details
                    string query = @"
                        SELECT IdEmploye, TypePointage, DateHeure, is_deleted
                        FROM EMPLOYE_POINTAGE 
                        WHERE IdPointage = @IdPointage";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdPointage", pointageId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Can't cancel if already deleted
                                if (Convert.ToBoolean(reader["is_deleted"]))
                                {
                                    return false;
                                }

                                int employeeId = Convert.ToInt32(reader["IdEmploye"]);
                                string typePointage = reader["TypePointage"].ToString();
                                DateTime dateHeure = Convert.ToDateTime(reader["DateHeure"]);

                                // Check if canceling this pointage would break the sequence
                                return ValidatePointageCancellation(employeeId, pointageId, typePointage, dateHeure);
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private bool ValidatePointageCancellation(int employeeId, int pointageId, string typePointage, DateTime dateHeure)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Check if there are subsequent pointages that would be affected
                    string query = @"
                        SELECT COUNT(*) 
                        FROM EMPLOYE_POINTAGE 
                        WHERE IdEmploye = @IdEmploye 
                        AND DateHeure > @DateHeure 
                        AND IdPointage <> @IdPointage
                        AND is_deleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                        cmd.Parameters.AddWithValue("@DateHeure", dateHeure);
                        cmd.Parameters.AddWithValue("@IdPointage", pointageId);

                        conn.Open();
                        int subsequentCount = (int)cmd.ExecuteScalar();

                        // If there are subsequent pointages, we need to validate the sequence
                        if (subsequentCount > 0)
                        {
                            // Additional validation logic can be added here
                            // For now, we'll allow cancellation but warn about potential issues
                            ShowWarning("Attention: L'annulation de ce pointage pourrait affecter la séquence des pointages suivants.");
                        }

                        return true; // Allow cancellation with warning
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private void RestorePointage(int pointageId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE EMPLOYE_POINTAGE 
                        SET is_deleted = 0, 
                            DateModification = @DateModification,
                            ModifiePar = @ModifiePar
                        WHERE IdPointage = @IdPointage";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdPointage", pointageId);
                        cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ModifiePar", GetCurrentUserId());

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            SearchPointages();
                            ShowSuccess("Pointage restauré avec succès.");
                        }
                        else
                        {
                            ShowError("Erreur lors de la restauration du pointage.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de la restauration: " + ex.Message);
            }
        }

        private int GetCurrentUserId()
        {
            if (Session["EmployeId"] == null)
            {
                Session.Clear();
                Response.Redirect("~/connexion.aspx");
                return 0;
            }

            return Convert.ToInt32(Session["EmployeId"]);
        }

        private void ShowSuccess(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Success", $"showToast('success', '{HttpUtility.JavaScriptStringEncode(message)}');", true);
        }

        private void ShowError(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Error", $"showToast('error', '{HttpUtility.JavaScriptStringEncode(message)}');", true);
        }

        private void ShowWarning(string message)
        {
            pnlWarning.Visible = true;
            litWarningMessage.Text = message;
        }
    }
}
