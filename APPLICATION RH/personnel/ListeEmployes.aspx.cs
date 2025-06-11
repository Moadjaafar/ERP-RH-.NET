using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.Web.Script.Services;

namespace APPLICATION_RH.personnel
{
    public partial class ListEmploye : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSocieties();
                InitializeDepartmentDropdown(); // Initialize as disabled
                BindEmployeesGrid();
            }
        }

        private void LoadSocieties()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT IdSociete, NomSociete FROM SOCIETE ORDER BY NomSociete";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            ddlSociety.DataSource = reader;
                            ddlSociety.DataTextField = "NomSociete";
                            ddlSociety.DataValueField = "IdSociete";
                            ddlSociety.DataBind();
                        }
                    }
                }

                // Add default item
                ddlSociety.Items.Insert(0, new ListItem("Toutes les sociétés", ""));
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Erreur lors du chargement des sociétés: " + ex.Message);
            }
        }

        private void InitializeDepartmentDropdown()
        {
            ddlDepartment.Items.Clear();
            ddlDepartment.Items.Add(new ListItem("Sélectionnez d'abord une société", ""));
            ddlDepartment.Enabled = false;
            ddlDepartment.CssClass = "form-select disabled-dropdown";
        }

        private void LoadDepartments(string societyId)
        {
            try
            {
                ddlDepartment.Items.Clear();

                if (string.IsNullOrEmpty(societyId))
                {
                    // No society selected - disable department dropdown
                    ddlDepartment.Items.Add(new ListItem("Sélectionnez d'abord une société", ""));
                    ddlDepartment.Enabled = false;
                    ddlDepartment.CssClass = "form-select disabled-dropdown";
                    return;
                }

                // Society selected - enable and populate department dropdown
                ddlDepartment.Enabled = true;
                ddlDepartment.CssClass = "form-select"; // Remove disabled styling
                ddlDepartment.Items.Add(new ListItem("Tous les départements de cette société", ""));

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT IdDepartement, NomDepartement FROM DEPARTEMENT WHERE IdSociete = @SocietyId ORDER BY NomDepartement";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SocietyId", societyId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ddlDepartment.Items.Add(new ListItem(
                                    reader["NomDepartement"].ToString(),
                                    reader["IdDepartement"].ToString()
                                ));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Erreur lors du chargement des départements: " + ex.Message);
            }
        }

        private void BindEmployeesGrid(string searchTerm = "", string society = "", string department = "",
                                     string employeeType = "", string contractType = "", DateTime? dateFrom = null,
                                     DateTime? dateTo = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    StringBuilder query = new StringBuilder(@"
                    SELECT e.IdEmploye, e.Matricule, e.Nom, e.Prenom, e.Email, e.Telephone, 
                           e.DateEmbauche, e.TypeContrat, e.TypeEmploye,
                           e.DateCreation, s.NomSociete, d.NomDepartement
                    FROM EMPLOYE e
                    LEFT JOIN SOCIETE s ON e.IdSociete = s.IdSociete
                    LEFT JOIN DEPARTEMENT d ON e.IdDepartement = d.IdDepartement
                    WHERE e.is_deleted = 0");

                    var parameters = new List<SqlParameter>();

                    // Quick search
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        query.Append(@" AND (e.Nom LIKE @SearchTerm OR e.Prenom LIKE @SearchTerm 
                                        OR e.Matricule LIKE @SearchTerm OR e.Email LIKE @SearchTerm)");
                        parameters.Add(new SqlParameter("@SearchTerm", "%" + searchTerm + "%"));
                    }

                    // Society filter
                    if (!string.IsNullOrEmpty(society))
                    {
                        query.Append(" AND e.IdSociete = @Society");
                        parameters.Add(new SqlParameter("@Society", society));
                    }

                    // Department filter
                    if (!string.IsNullOrEmpty(department))
                    {
                        query.Append(" AND e.IdDepartement = @Department");
                        parameters.Add(new SqlParameter("@Department", department));
                    }

                    // Employee type filter
                    if (!string.IsNullOrEmpty(employeeType))
                    {
                        query.Append(" AND e.TypeEmploye = @EmployeeType");
                        parameters.Add(new SqlParameter("@EmployeeType", employeeType));
                    }

                    // Contract type filter
                    if (!string.IsNullOrEmpty(contractType))
                    {
                        query.Append(" AND e.TypeContrat = @ContractType");
                        parameters.Add(new SqlParameter("@ContractType", contractType));
                    }

                    // Date range filter
                    if (dateFrom.HasValue)
                    {
                        query.Append(" AND e.DateEmbauche >= @DateFrom");
                        parameters.Add(new SqlParameter("@DateFrom", dateFrom.Value));
                    }

                    if (dateTo.HasValue)
                    {
                        query.Append(" AND e.DateEmbauche <= @DateTo");
                        parameters.Add(new SqlParameter("@DateTo", dateTo.Value));
                    }

                    query.Append(" ORDER BY s.NomSociete, d.NomDepartement, e.Nom, e.Prenom");

                    using (SqlCommand cmd = new SqlCommand(query.ToString(), conn))
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            gvEmployees.DataSource = dt;
                            gvEmployees.DataBind();

                            // Update record count
                            lblTotalCount.Text = dt.Rows.Count.ToString();

                            // Show search results info
                            if (HasActiveFilters(searchTerm, society, department, employeeType, contractType, dateFrom, dateTo))
                            {
                                searchResultsInfo.Visible = true;
                                lblSearchResults.Text = $"Résultats de recherche: {dt.Rows.Count} employé(s) trouvé(s)";
                            }
                            else
                            {
                                searchResultsInfo.Visible = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Erreur lors du chargement des employés: " + ex.Message);
            }
        }

        // Search button event handlers
        protected void btnQuickSearch_Click(object sender, EventArgs e)
        {
            BindEmployeesGrid(searchTerm: txtQuickSearch.Text.Trim());
        }

        protected void btnAdvancedSearch_Click(object sender, EventArgs e)
        {
            DateTime? dateFrom = null;
            DateTime? dateTo = null;

            if (!string.IsNullOrEmpty(txtDateFrom.Text))
                dateFrom = Convert.ToDateTime(txtDateFrom.Text);

            if (!string.IsNullOrEmpty(txtDateTo.Text))
                dateTo = Convert.ToDateTime(txtDateTo.Text);

            BindEmployeesGrid(
                searchTerm: txtQuickSearch.Text.Trim(),
                society: ddlSociety.SelectedValue,
                department: ddlDepartment.SelectedValue,
                employeeType: ddlEmployeeType.SelectedValue,
                contractType: ddlContractType.SelectedValue,
                dateFrom: dateFrom,
                dateTo: dateTo
            );
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            // Clear all filters
            txtQuickSearch.Text = "";
            ddlSociety.SelectedIndex = 0;
            ddlEmployeeType.SelectedIndex = 0;
            ddlContractType.SelectedIndex = 0;
            txtDateFrom.Text = "";
            txtDateTo.Text = "";

            // Reset department dropdown to disabled state
            InitializeDepartmentDropdown();

            // Reload all data
            BindEmployeesGrid();
        }

        // Dropdown change handlers
        protected void ddlSociety_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reset department selection when society changes
            string selectedSociety = ddlSociety.SelectedValue;

            if (string.IsNullOrEmpty(selectedSociety))
            {
                // "Toutes les sociétés" selected - disable department dropdown
                InitializeDepartmentDropdown();
            }
            else
            {
                // Specific society selected - enable and populate departments
                LoadDepartments(selectedSociety);
            }

            // Trigger search with new society filter
            btnAdvancedSearch_Click(sender, e);
        }

        protected void ddlDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Only trigger search if department dropdown is enabled
            if (ddlDepartment.Enabled)
            {
                btnAdvancedSearch_Click(sender, e);
            }
        }

        protected void ddlEmployeeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnAdvancedSearch_Click(sender, e);
        }

        protected void ddlContractType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnAdvancedSearch_Click(sender, e);
        }

        protected void btnShowAll_Click(object sender, EventArgs e)
        {
            btnClearFilters_Click(sender, e);
        }

        // Helper methods
        private bool HasActiveFilters(string searchTerm, string society, string department, string employeeType,
                                     string contractType, DateTime? dateFrom, DateTime? dateTo)
        {
            return !string.IsNullOrEmpty(searchTerm) || !string.IsNullOrEmpty(society) ||
                   !string.IsNullOrEmpty(department) || !string.IsNullOrEmpty(employeeType) ||
                    !string.IsNullOrEmpty(contractType) ||
                   dateFrom.HasValue || dateTo.HasValue;
        }

        // GridView event handlers
        protected void gvEmployees_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int employeeId = Convert.ToInt32(e.CommandArgument);

                switch (e.CommandName)
                {
                    case "ViewDetails":
                        LoadEmployeeDetailsForModal(employeeId);
                        ShowEmployeeModal();
                        break;

                    case "EditEmployee":
                        Response.Redirect($"~/personnel/EditEmployee.aspx?id={employeeId}");
                        break;

                    case "DeleteEmployee":
                        DeleteEmployee(employeeId);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Erreur lors de l'exécution de l'action: " + ex.Message);
            }
        }
        // Load employee details into modal controls
        private void LoadEmployeeDetailsForModal(int employeeId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT e.IdEmploye, e.Matricule, e.Nom, e.Prenom, e.DateNaissance, 
                       e.LieuNaissance, e.CinPassport, e.Sexe, e.SituationFamiliale, 
                       e.NombreEnfants, e.Adresse, e.Telephone, e.Email, e.Nationalite,
                       e.NiveauScolaire, e.Diplome, e.DateEmbauche, e.DateSortie, 
                       e.TypeContrat, e.TypeEmploye, e.Shift, e.SituationCNSS, 
                       e.NumeroCNSS, s.NomSociete, d.NomDepartement
                FROM EMPLOYE e
                LEFT JOIN SOCIETE s ON e.IdSociete = s.IdSociete
                LEFT JOIN DEPARTEMENT d ON e.IdDepartement = d.IdDepartement
                WHERE e.IdEmploye = @IdEmploye AND e.is_deleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeeId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Store employee ID for edit button
                                hfSelectedEmployeeId.Value = employeeId.ToString();

                                // Populate personal information
                                litMatricule.Text = GetSafeString(reader, "Matricule");
                                litNom.Text = GetSafeString(reader, "Nom");
                                litPrenom.Text = GetSafeString(reader, "Prenom");
                                litDateNaissance.Text = GetSafeDate(reader, "DateNaissance");
                                litLieuNaissance.Text = GetSafeString(reader, "LieuNaissance");
                                litCinPassport.Text = GetSafeString(reader, "CinPassport");
                                litSexe.Text = GetSafeString(reader, "Sexe");
                                litSituationFamiliale.Text = GetSafeString(reader, "SituationFamiliale");
                                litNombreEnfants.Text = GetSafeInt(reader, "NombreEnfants").ToString();
                                litTelephone.Text = GetSafeString(reader, "Telephone");
                                litEmail.Text = GetSafeString(reader, "Email");
                                litNationalite.Text = GetSafeString(reader, "Nationalite");

                                // Populate professional information
                                litDateEmbauche.Text = GetSafeDate(reader, "DateEmbauche");
                                litDateSortie.Text = GetSafeDate(reader, "DateSortie");
                                litTypeContrat.Text = GetSafeString(reader, "TypeContrat");
                                litTypeEmploye.Text = GetSafeString(reader, "TypeEmploye");
                                litNomSociete.Text = GetSafeString(reader, "NomSociete");
                                litNomDepartement.Text = GetSafeString(reader, "NomDepartement");
                                litShift.Text = GetSafeString(reader, "Shift");
                                litSituationCNSS.Text = GetSafeString(reader, "SituationCNSS");
                                litNumeroCNSS.Text = GetSafeString(reader, "NumeroCNSS");
                                litNiveauScolaire.Text = GetSafeString(reader, "NiveauScolaire");
                                litDiplome.Text = GetSafeString(reader, "Diplome");

                                // Handle address
                                string adresse = GetSafeString(reader, "Adresse");
                                if (!string.IsNullOrEmpty(adresse))
                                {
                                    litAdresse.Text = adresse;
                                    pnlAdresse.Visible = true;
                                }
                                else
                                {
                                    pnlAdresse.Visible = false;
                                }

                                // Update modal title
                                litModalTitle.Text = $"Détails de {litNom.Text} {litPrenom.Text}";

                                // Show edit button
                                btnEditFromModal.Visible = true;
                            }
                            else
                            {
                                ShowErrorMessage("Employé non trouvé.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Erreur lors du chargement des détails: " + ex.Message);
            }
        }
        // Helper methods to safely extract data from DataReader
        private string GetSafeString(SqlDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? "-" : reader[columnName].ToString();
        }

        private string GetSafeDate(SqlDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? "-" : Convert.ToDateTime(reader[columnName]).ToString("dd/MM/yyyy");
        }

        private int GetSafeInt(SqlDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? 0 : Convert.ToInt32(reader[columnName]);
        }

        // Show modal using JavaScript
        private void ShowEmployeeModal()
        {
            ClientScript.RegisterStartupScript(this.GetType(), "ShowEmployeeModal", "showEmployeeModal();", true);
        }

        // Edit button click event from modal
        protected void btnEditFromModal_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(hfSelectedEmployeeId.Value))
            {
                int employeeId = Convert.ToInt32(hfSelectedEmployeeId.Value);
                Response.Redirect($"~/personnel/EditEmployee.aspx?id={employeeId}");
            }
        }
        protected void gvEmployees_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                gvEmployees.PageIndex = e.NewPageIndex;

                // Maintain current search filters during pagination
                DateTime? dateFrom = null;
                DateTime? dateTo = null;

                if (!string.IsNullOrEmpty(txtDateFrom.Text))
                    dateFrom = Convert.ToDateTime(txtDateFrom.Text);

                if (!string.IsNullOrEmpty(txtDateTo.Text))
                    dateTo = Convert.ToDateTime(txtDateTo.Text);

                BindEmployeesGrid(
                    searchTerm: txtQuickSearch.Text.Trim(),
                    society: ddlSociety.SelectedValue,
                    department: ddlDepartment.SelectedValue,
                    employeeType: ddlEmployeeType.SelectedValue,
                    contractType: ddlContractType.SelectedValue,
                    dateFrom: dateFrom,
                    dateTo: dateTo
                );
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Erreur lors du changement de page: " + ex.Message);
            }
        }

        private void DeleteEmployee(int employeeId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Soft delete - mark as inactive instead of deleting
                    string query = @"
                        UPDATE EMPLOYE 
                        SET is_deleted = 1, 
                            DateModification = @DateModification
                        WHERE IdEmploye = @IdEmploye";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                        cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            ShowSuccessMessage("Employé désactivé avec succès.");

                            // Refresh the grid with current filters
                            DateTime? dateFrom = null;
                            DateTime? dateTo = null;

                            if (!string.IsNullOrEmpty(txtDateFrom.Text))
                                dateFrom = Convert.ToDateTime(txtDateFrom.Text);

                            if (!string.IsNullOrEmpty(txtDateTo.Text))
                                dateTo = Convert.ToDateTime(txtDateTo.Text);

                            BindEmployeesGrid(
                                searchTerm: txtQuickSearch.Text.Trim(),
                                society: ddlSociety.SelectedValue,
                                department: ddlDepartment.SelectedValue,
                                employeeType: ddlEmployeeType.SelectedValue,
                                contractType: ddlContractType.SelectedValue,
                                dateFrom: dateFrom,
                                dateTo: dateTo
                            );
                        }
                        else
                        {
                            ShowErrorMessage("Erreur lors de la désactivation de l'employé.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Erreur lors de la désactivation: " + ex.Message);
            }
        }

        protected void btnAddEmployee_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/personnel/AjouterEmploye.aspx");
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            ExportFilteredDataToExcel();
        }

        private void ExportFilteredDataToExcel()
        {
            try
            {
                DateTime? dateFrom = null;
                DateTime? dateTo = null;

                if (!string.IsNullOrEmpty(txtDateFrom.Text))
                    dateFrom = Convert.ToDateTime(txtDateFrom.Text);

                if (!string.IsNullOrEmpty(txtDateTo.Text))
                    dateTo = Convert.ToDateTime(txtDateTo.Text);

                // Get filtered data
                DataTable dt = GetFilteredEmployeeData(
                    txtQuickSearch.Text.Trim(),
                    ddlSociety.SelectedValue,
                    ddlDepartment.SelectedValue,
                    ddlEmployeeType.SelectedValue,
                    ddlContractType.SelectedValue,
                    dateFrom,
                    dateTo
                );

                if (dt.Rows.Count == 0)
                {
                    ShowErrorMessage("Aucune donnée à exporter.");
                    return;
                }

                // Create Excel content
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Société\tDépartement\tMatricule\tNom\tPrénom\tEmail\tTéléphone\tDate Embauche\tType Contrat\tType Employé\tStatut");

                foreach (DataRow row in dt.Rows)
                {
                    sb.AppendLine($"{row["NomSociete"]}\t{row["NomDepartement"]}\t{row["Matricule"]}\t" +
                                $"{row["Nom"]}\t{row["Prenom"]}\t{row["Email"]}\t{row["Telephone"]}\t" +
                                $"{(row["DateEmbauche"] != DBNull.Value ? Convert.ToDateTime(row["DateEmbauche"]).ToString("dd/MM/yyyy") : "")}\t" +
                                $"{row["TypeContrat"]}\t{row["TypeEmploye"]}\t" +
                                $"{(Convert.ToBoolean(row["Actif"]) ? "Actif" : "Inactif")}");
                }

                // Send file to browser
                Response.Clear();
                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("Content-Disposition", $"attachment; filename=Employes_{DateTime.Now:yyyyMMdd_HHmm}.xls");
                Response.Write(sb.ToString());
                Response.End();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Erreur lors de l'export: " + ex.Message);
            }
        }

        private DataTable GetFilteredEmployeeData(string searchTerm, string society, string department,
                                                 string employeeType, string contractType,
                                                 DateTime? dateFrom, DateTime? dateTo)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                StringBuilder query = new StringBuilder(@"
                    SELECT e.Matricule, e.Nom, e.Prenom, e.Email, e.Telephone, 
                           e.DateEmbauche, e.TypeContrat, e.TypeEmploye, e.Actif,
                           s.NomSociete, d.NomDepartement
                    FROM EMPLOYE e
                    LEFT JOIN SOCIETE s ON e.IdSociete = s.IdSociete
                    LEFT JOIN DEPARTEMENT d ON e.IdDepartement = d.IdDepartement
                    WHERE 1=1");

                var parameters = new List<SqlParameter>();

                // Apply same filters as grid
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query.Append(@" AND (e.Nom LIKE @SearchTerm OR e.Prenom LIKE @SearchTerm 
                                    OR e.Matricule LIKE @SearchTerm OR e.Email LIKE @SearchTerm)");
                    parameters.Add(new SqlParameter("@SearchTerm", "%" + searchTerm + "%"));
                }

                if (!string.IsNullOrEmpty(society))
                {
                    query.Append(" AND e.IdSociete = @Society");
                    parameters.Add(new SqlParameter("@Society", society));
                }

                if (!string.IsNullOrEmpty(department))
                {
                    query.Append(" AND e.IdDepartement = @Department");
                    parameters.Add(new SqlParameter("@Department", department));
                }

                if (!string.IsNullOrEmpty(employeeType))
                {
                    query.Append(" AND e.TypeEmploye = @EmployeeType");
                    parameters.Add(new SqlParameter("@EmployeeType", employeeType));
                }

                if (!string.IsNullOrEmpty(contractType))
                {
                    query.Append(" AND e.TypeContrat = @ContractType");
                    parameters.Add(new SqlParameter("@ContractType", contractType));
                }

                if (dateFrom.HasValue)
                {
                    query.Append(" AND e.DateEmbauche >= @DateFrom");
                    parameters.Add(new SqlParameter("@DateFrom", dateFrom.Value));
                }

                if (dateTo.HasValue)
                {
                    query.Append(" AND e.DateEmbauche <= @DateTo");
                    parameters.Add(new SqlParameter("@DateTo", dateTo.Value));
                }

                query.Append(" ORDER BY s.NomSociete, d.NomDepartement, e.Nom, e.Prenom");

                using (SqlCommand cmd = new SqlCommand(query.ToString(), conn))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        private void ShowErrorMessage(string message)
        {
            // Escape single quotes for JavaScript
            string escapedMessage = message.Replace("'", "\\'");

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowErrorToast",
                $"Swal.fire({{ toast: true, position: 'top-end', icon: 'error', title: '{escapedMessage}', showConfirmButton: false, timer: 4000 }});", true);
        }

        private void ShowSuccessMessage(string message)
        {
            // Escape single quotes for JavaScript
            string escapedMessage = message.Replace("'", "\\'");

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowSuccessToast",
                $"Swal.fire({{ toast: true, position: 'top-end', icon: 'success', title: '{escapedMessage}', showConfirmButton: false, timer: 3000 }});", true);
        }

        private void ShowInfoMessage(string message)
        {
            // Escape single quotes for JavaScript
            string escapedMessage = message.Replace("'", "\\'");

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowInfoToast",
                $"Swal.fire({{ toast: true, position: 'top-end', icon: 'info', title: '{escapedMessage}', showConfirmButton: false, timer: 3500 }});", true);
        }

        private void ShowWarningMessage(string message)
        {
            // Escape single quotes for JavaScript
            string escapedMessage = message.Replace("'", "\\'");

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowWarningToast",
                $"Swal.fire({{ toast: true, position: 'top-end', icon: 'warning', title: '{escapedMessage}', showConfirmButton: false, timer: 3500 }});", true);
        }
    }
}