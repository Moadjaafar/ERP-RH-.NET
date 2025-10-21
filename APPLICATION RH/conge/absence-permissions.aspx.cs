using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.conge
{
    public partial class absence_permissions : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadEmployees();
                LoadAbsenceTypes();
                LoadRecentRequests();
            }
        }

        private void LoadEmployees(string filter = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT IdEmploye, 
                               CONCAT(Prenom, ' ', Nom, ' (', Matricule, ')') as NomComplet,
                               Prenom, Nom, Matricule, Email, Telephone
                        FROM EMPLOYE 
                        WHERE is_deleted = 0";

                    // Add filter condition if provided
                    if (!string.IsNullOrEmpty(filter))
                    {
                        query += @" AND (
                            Prenom LIKE @Filter OR 
                            Nom LIKE @Filter OR 
                            Matricule LIKE @Filter OR
                            CONCAT(Prenom, ' ', Nom) LIKE @Filter OR
                            CONCAT(Nom, ' ', Prenom) LIKE @Filter
                        )";
                    }

                    query += " ORDER BY Prenom, Nom";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    
                    if (!string.IsNullOrEmpty(filter))
                    {
                        cmd.Parameters.AddWithValue("@Filter", "%" + filter.Trim() + "%");
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    ddlEmploye.DataSource = dt;
                    ddlEmploye.DataTextField = "NomComplet";
                    ddlEmploye.DataValueField = "IdEmploye";
                    ddlEmploye.DataBind();

                    ddlEmploye.Items.Insert(0, new ListItem("-- Sélectionner un employé --", ""));

                    // Show filter result count
                    if (!string.IsNullOrEmpty(filter))
                    {
                        int count = dt.Rows.Count;
                        string countText = count == 0 ? "Aucun employé trouvé" : 
                                          count == 1 ? "1 employé trouvé" : 
                                          $"{count} employés trouvés";
                        
                        ScriptManager.RegisterStartupScript(this, GetType(), "filterResult", 
                            $"console.log('Filtre appliqué: {countText}');", true);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Erreur lors du chargement des employés: " + ex.Message, "error");
            }
        }

        private void LoadAbsenceTypes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT IdPermissionAbsence, TypeAbsence, Description 
                        FROM PERMISSION_ABSENCE 
                        WHERE is_deleted = 0 AND Actif = 1 
                        ORDER BY TypeAbsence";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    ddlTypeConge.DataSource = dt;
                    ddlTypeConge.DataTextField = "TypeAbsence";
                    ddlTypeConge.DataValueField = "IdPermissionAbsence";
                    ddlTypeConge.DataBind();

                    ddlTypeConge.Items.Insert(0, new ListItem("-- Sélectionner le type --", ""));
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Erreur lors du chargement des types d'absence: " + ex.Message, "error");
            }
        }

        private void LoadRecentRequests()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT TOP 20
                            pae.IdDemandeAbsence,
                            CONCAT(e.Prenom, ' ', e.Nom) as NomEmploye,
                            pa.TypeAbsence,
                            pae.DateDebut,
                            pae.DateFin,
                            pae.EstApprouvee,
                            pae.DateCreation,
                            pae.ApprouveePar
                        FROM PERMISSION_ABSENCE_EMPLOYE pae
                        INNER JOIN EMPLOYE e ON pae.IdEmploye = e.IdEmploye
                        INNER JOIN PERMISSION_ABSENCE pa ON pae.IdPermissionAbsence = pa.IdPermissionAbsence
                        WHERE pae.is_deleted = 0
                        ORDER BY pae.DateCreation DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvDemandes.DataSource = dt;
                    gvDemandes.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Erreur lors du chargement des demandes récentes: " + ex.Message, "error");
            }
        }

        protected void txtEmployeeFilter_TextChanged(object sender, EventArgs e)
        {
            string filter = txtEmployeeFilter.Text.Trim();
            LoadEmployees(filter);
            
            // Reset employee selection when filter changes
            lblEmployeInfo.Text = "Sélectionnez un employé pour voir ses informations";
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            txtEmployeeFilter.Text = "";
            LoadEmployees();
            lblEmployeInfo.Text = "Sélectionnez un employé pour voir ses informations";
        }

        protected void ddlEmploye_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlEmploye.SelectedValue))
            {
                LoadEmployeeInfo(Convert.ToInt32(ddlEmploye.SelectedValue));
            }
            else
            {
                lblEmployeInfo.Text = "Sélectionnez un employé pour voir ses informations";
            }
        }

        protected void ddlTypeConge_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlTypeConge.SelectedValue))
            {
                LoadTypeDescription(Convert.ToInt32(ddlTypeConge.SelectedValue));
            }
            else
            {
                lblTypeDescription.Text = "Sélectionnez un type pour voir sa description";
            }
        }

        protected void txtDateDebut_TextChanged(object sender, EventArgs e)
        {
            CalculateLeaveDuration();
        }

        protected void txtDateFin_TextChanged(object sender, EventArgs e)
        {
            CalculateLeaveDuration();
        }

        protected void btnSoumettreDemande_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = @"
                            INSERT INTO PERMISSION_ABSENCE_EMPLOYE 
                            (IdEmploye, IdPermissionAbsence, DateDebut, DateFin, EstApprouvee, ApprouveePar, DateCreation, DateModification, is_deleted) 
                            VALUES 
                            (@IdEmploye, @IdPermissionAbsence, @DateDebut, @DateFin, 0, NULL, GETDATE(), NULL, 0)";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@IdEmploye", Convert.ToInt32(ddlEmploye.SelectedValue));
                        cmd.Parameters.AddWithValue("@IdPermissionAbsence", Convert.ToInt32(ddlTypeConge.SelectedValue));
                        cmd.Parameters.AddWithValue("@DateDebut", Convert.ToDateTime(txtDateDebut.Text));
                        cmd.Parameters.AddWithValue("@DateFin", Convert.ToDateTime(txtDateFin.Text));

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowMessage("Demande de congé soumise avec succès! Elle est maintenant en attente d'approbation.", "success");
                            ClearForm();
                            LoadRecentRequests();
                        }
                        else
                        {
                            ShowMessage("Erreur lors de la soumission de la demande", "error");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage("Erreur lors de la soumission: " + ex.Message, "error");
                }
            }
        }

        protected void gvDemandes_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvDemandes.PageIndex = e.NewPageIndex;
            LoadRecentRequests();
        }

        protected void gvDemandes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idDemandeAbsence = Convert.ToInt32(e.CommandArgument);

            switch (e.CommandName)
            {
                case "ToggleStatus":
                    ToggleRequestStatus(idDemandeAbsence);
                    break;
                case "DeleteRequest":
                    DeleteRequest(idDemandeAbsence);
                    break;
            }
        }

        protected void gvDemandes_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Additional styling or logic can be added here
                LinkButton btnToggle = (LinkButton)e.Row.FindControl("btnToggleStatus");
                LinkButton btnDelete = (LinkButton)e.Row.FindControl("btnDelete");

                if (btnToggle != null && btnDelete != null)
                {
                    // Add any additional logic for button states
                }
            }
        }

        private void LoadEmployeeInfo(int idEmploye)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT e.Prenom, e.Nom, e.Matricule, e.Email, e.Telephone,
                               s.NomSociete, d.NomDepartement, ser.NomService, f.NomFonction
                        FROM EMPLOYE e
                        LEFT JOIN SOCIETE s ON e.IdSociete = s.IdSociete
                        LEFT JOIN DEPARTEMENT d ON e.IdDepartement = d.IdDepartement
                        LEFT JOIN SERVICE ser ON e.IdService = ser.IdService
                        LEFT JOIN FONCTION f ON e.IdFonction = f.IdFonction
                        WHERE e.IdEmploye = @Id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", idEmploye);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string info = $"<strong>{reader["Prenom"]} {reader["Nom"]}</strong><br/>";
                        info += $"<small>Matricule: {reader["Matricule"]}<br/>";
                        if (reader["NomFonction"] != DBNull.Value)
                            info += $"Fonction: {reader["NomFonction"]}<br/>";
                        if (reader["NomService"] != DBNull.Value)
                            info += $"Service: {reader["NomService"]}<br/>";
                        if (reader["Email"] != DBNull.Value)
                            info += $"Email: {reader["Email"]}";
                        info += "</small>";

                        lblEmployeInfo.Text = info;
                    }
                }
            }
            catch (Exception ex)
            {
                lblEmployeInfo.Text = "Erreur lors du chargement des informations de l'employé";
            }
        }

        private void LoadTypeDescription(int idPermissionAbsence)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT TypeAbsence, Description 
                        FROM PERMISSION_ABSENCE 
                        WHERE IdPermissionAbsence = @Id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", idPermissionAbsence);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string description = $"<strong>{reader["TypeAbsence"]}</strong><br/>";
                        if (reader["Description"] != DBNull.Value && !string.IsNullOrEmpty(reader["Description"].ToString()))
                        {
                            description += $"<small>{reader["Description"]}</small>";
                        }
                        else
                        {
                            description += "<small class='text-muted'>Aucune description disponible</small>";
                        }

                        lblTypeDescription.Text = description;
                    }
                }
            }
            catch (Exception ex)
            {
                lblTypeDescription.Text = "Erreur lors du chargement de la description";
            }
        }

        private void CalculateLeaveDuration()
        {
            if (!string.IsNullOrEmpty(txtDateDebut.Text) && !string.IsNullOrEmpty(txtDateFin.Text))
            {
                try
                {
                    DateTime startDate = Convert.ToDateTime(txtDateDebut.Text);
                    DateTime endDate = Convert.ToDateTime(txtDateFin.Text);

                    if (endDate >= startDate)
                    {
                        TimeSpan duration = endDate - startDate;
                        int days = duration.Days + 1; // +1 pour inclure le jour de fin

                        string durationText = "";
                        if (days == 1)
                            durationText = "1 jour";
                        else
                            durationText = $"{days} jours";

                        // Calculate working days (excluding weekends)
                        int workingDays = 0;
                        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                        {
                            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                                workingDays++;
                        }

                        if (workingDays != days)
                        {
                            durationText += $" ({workingDays} jours ouvrables)";
                        }

                        lblDureeConge.Text = durationText;
                    }
                    else
                    {
                        lblDureeConge.Text = "La date de fin doit être >= à la date de début";
                        lblDureeConge.CssClass = "text-danger";
                    }
                }
                catch
                {
                    lblDureeConge.Text = "Dates invalides";
                    lblDureeConge.CssClass = "text-danger";
                }
            }
            else
            {
                lblDureeConge.Text = "Sélectionnez les dates pour calculer la durée";
                lblDureeConge.CssClass = "";
            }
        }

        private void ClearForm()
        {
            txtEmployeeFilter.Text = "";
            LoadEmployees(); // Reload all employees
            ddlEmploye.SelectedIndex = 0;
            ddlTypeConge.SelectedIndex = 0;
            txtDateDebut.Text = "";
            txtDateFin.Text = "";
            txtCommentaire.Text = "";
            lblEmployeInfo.Text = "Sélectionnez un employé pour voir ses informations";
            lblTypeDescription.Text = "Sélectionnez un type pour voir sa description";
            lblDureeConge.Text = "Sélectionnez les dates pour calculer la durée";
            lblDureeConge.CssClass = "";
        }

        private void ToggleRequestStatus(int idDemandeAbsence)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // First get the current status
                    string selectQuery = "SELECT EstApprouvee FROM PERMISSION_ABSENCE_EMPLOYE WHERE IdDemandeAbsence = @Id";
                    SqlCommand selectCmd = new SqlCommand(selectQuery, conn);
                    selectCmd.Parameters.AddWithValue("@Id", idDemandeAbsence);
                    
                    conn.Open();
                    object currentStatus = selectCmd.ExecuteScalar();
                    
                    if (currentStatus != null)
                    {
                        bool isCurrentlyApproved = Convert.ToBoolean(currentStatus);
                        bool newStatus = !isCurrentlyApproved;
                        
                        // Get the current user ID from session for approval tracking
                        object approverEmployeeId = newStatus ? Session["EmployeId"] : DBNull.Value;
                        
                        // Update with the new status
                        string updateQuery = @"
                            UPDATE PERMISSION_ABSENCE_EMPLOYE 
                            SET EstApprouvee = @NewStatus,
                                ApprouveePar = @ApprouveePar,
                                DateModification = GETDATE()
                            WHERE IdDemandeAbsence = @Id";

                        SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                        updateCmd.Parameters.AddWithValue("@Id", idDemandeAbsence);
                        updateCmd.Parameters.AddWithValue("@NewStatus", newStatus);
                        updateCmd.Parameters.AddWithValue("@ApprouveePar", approverEmployeeId ?? DBNull.Value);

                        int result = updateCmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            string message = newStatus ? 
                                "Demande approuvée avec succès!" : 
                                "Demande rejetée avec succès!";
                            ShowMessage(message, "success");
                            LoadRecentRequests();
                        }
                        else
                        {
                            ShowMessage("Erreur lors de la mise à jour du statut", "error");
                        }
                    }
                    else
                    {
                        ShowMessage("Demande introuvable", "error");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Erreur lors de la mise à jour du statut: " + ex.Message, "error");
            }
        }

        private void DeleteRequest(int idDemandeAbsence)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Soft delete the request
                    string query = @"
                        UPDATE PERMISSION_ABSENCE_EMPLOYE 
                        SET is_deleted = 1, DateModification = GETDATE() 
                        WHERE IdDemandeAbsence = @Id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", idDemandeAbsence);

                    conn.Open();
                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowMessage("Demande de congé supprimée avec succès!", "success");
                        LoadRecentRequests();
                    }
                    else
                    {
                        ShowMessage("Erreur lors de la suppression", "error");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Erreur lors de la suppression: " + ex.Message, "error");
            }
        }

        public string CalculateDays(object startDate, object endDate)
        {
            try
            {
                if (startDate != DBNull.Value && endDate != DBNull.Value)
                {
                    DateTime start = Convert.ToDateTime(startDate);
                    DateTime end = Convert.ToDateTime(endDate);
                    
                    int totalDays = (end - start).Days + 1;
                    
                    // Calculate working days (excluding weekends)
                    int workingDays = 0;
                    for (DateTime date = start; date <= end; date = date.AddDays(1))
                    {
                        if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                            workingDays++;
                    }
                    
                    if (totalDays == 1)
                        return "1 jour";
                    else if (workingDays != totalDays)
                        return $"{totalDays} jours<br><small class='text-muted'>({workingDays} ouvrables)</small>";
                    else
                        return $"{totalDays} jours";
                }
                return "-";
            }
            catch
            {
                return "-";
            }
        }

        private void ShowMessage(string message, string type)
        {
            // Use System.Web.Script.Serialization for safe JavaScript string handling
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string safeMessage = serializer.Serialize(message);
            string safeType = serializer.Serialize(type);
            string script = $"showMessage({safeMessage}, {safeType});";
            ScriptManager.RegisterStartupScript(this, GetType(), "showMessage", script, true);
        }
    }
}