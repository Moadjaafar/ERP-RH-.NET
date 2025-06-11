using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.personnel
{
    public partial class EditEmployee : System.Web.UI.Page
    {

        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        private int EmployeeId
        {
            get
            {
                if (!string.IsNullOrEmpty(hfEmployeeId.Value))
                    return Convert.ToInt32(hfEmployeeId.Value);
                return 0;
            }
            set
            {
                hfEmployeeId.Value = value.ToString();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Get employee ID from query string
                if (Request.QueryString["id"] != null)
                {
                    if (int.TryParse(Request.QueryString["id"], out int employeeId))
                    {
                        EmployeeId = employeeId;
                        LoadDropDowns();
                        LoadEmployeeData();
                    }
                    else
                    {
                        ShowErrorAndRedirect("ID d'employé invalide.");
                    }
                }
                else
                {
                    ShowErrorAndRedirect("ID d'employé manquant.");
                }
            }
        }

        private void ShowErrorAndRedirect(string message)
        {
            // Show error message and redirect to employee list
            Session["ErrorMessage"] = message;
            Response.Redirect("~/personnel/ListeEmployes.aspx");
        }

        private void LoadDropDowns()
        {
            LoadSocietes();
            LoadRegions();
            LoadSituationsFamiliales();
            LoadNationalites();
            LoadNiveauxScolaires();
            LoadTypesOuvriers();
            LoadTypesContrats();
        }

        private void LoadEmployeeData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT e.IdEmploye, e.IdSociete, e.Matricule, e.Nom, e.Prenom, e.DateNaissance, 
                               e.LieuNaissance, e.CinPassport, e.Sexe, e.SituationFamiliale, 
                               e.NombreEnfants, e.Adresse, e.Telephone, e.Email, e.Nationalite,
                               e.NiveauScolaire, e.Diplome, e.DateEmbauche, e.DateSortie, 
                               e.TypeContrat, e.TypeEmploye, e.IdTypeOuv, e.Shift, e.SituationCNSS, 
                               e.NumeroCNSS, e.IdDepartement, e.IdService, e.IdCellule, 
                               e.IdFonction, e.IdEmplacement, e.is_deleted,
                               s.NomSociete, d.NomDepartement
                        FROM EMPLOYE e
                        LEFT JOIN SOCIETE s ON e.IdSociete = s.IdSociete
                        LEFT JOIN DEPARTEMENT d ON e.IdDepartement = d.IdDepartement
                        WHERE e.IdEmploye = @IdEmploye";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", EmployeeId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Set employee name and matricule in header
                                litEmployeeName.Text = $"{reader["Nom"]} {reader["Prenom"]}";
                                litMatricule.Text = reader["Matricule"].ToString();

                                // Check if employee is deleted
                                bool isDeleted = Convert.ToBoolean(reader["is_deleted"]);
                                if (isDeleted)
                                {
                                    pnlEmployeeStatus.Visible = true;
                                    litEmployeeStatus.Text = "Employé désactivé";

                                    if (reader["DateSortie"] != DBNull.Value)
                                    {
                                        pnlDateSortie.Visible = true;
                                        litDateSortie.Text = Convert.ToDateTime(reader["DateSortie"]).ToString("dd/MM/yyyy");
                                    }
                                }

                                // Load society and hierarchy
                                int idSociete = Convert.ToInt32(reader["IdSociete"]);
                                LoadHierarchyForEmployee(
                                    idSociete,
                                    GetNullableInt(reader, "IdDepartement"),
                                    GetNullableInt(reader, "IdService"),
                                    GetNullableInt(reader, "IdCellule"),
                                    GetNullableInt(reader, "IdFonction"),
                                    GetNullableInt(reader, "IdEmplacement")
                                );

                                // Fill form fields with employee data
                                PopulateFormFields(reader);
                            }
                            else
                            {
                                ShowErrorAndRedirect("Employé introuvable.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorAndRedirect($"Erreur lors du chargement des données: {ex.Message}");
            }
        }

        // Updated PopulateFormFields method
        private void PopulateFormFields(SqlDataReader reader)
        {
            // Personal Information
            txtNom.Text = GetSafeString(reader, "Nom");
            txtPrenom.Text = GetSafeString(reader, "Prenom");

            if (reader["DateNaissance"] != DBNull.Value)
                txtDateNaissance.Text = Convert.ToDateTime(reader["DateNaissance"]).ToString("yyyy-MM-dd");

            txtCinPassport.Text = GetSafeString(reader, "CinPassport");
            txtTelephone.Text = GetSafeString(reader, "Telephone");
            txtAdresse.Text = GetSafeString(reader, "Adresse");
            txtNombreEnfants.Text = reader["NombreEnfants"].ToString();

            // Set dropdown values - MAKE SURE DROPDOWNS ARE POPULATED FIRST
            SetDropDownValue(ddlSexe, GetSafeString(reader, "Sexe"));

            // Load and set Situation Familiale
            LoadSituationsFamiliales(); // Make sure it's loaded
            SetDropDownByText(ddlSituationFamiliale, GetSafeString(reader, "SituationFamiliale"));

            // Load and set Nationalite
            LoadNationalites(); // Make sure it's loaded
            SetDropDownByText(ddlNationalite, GetSafeString(reader, "Nationalite"));

            // Load and set Niveau Scolaire
            LoadNiveauxScolaires(); // Make sure it's loaded
            SetDropDownByText(ddlNiveauScolaire, GetSafeString(reader, "NiveauScolaire"));

            // Handle diploma panel
            if (!string.IsNullOrEmpty(GetSafeString(reader, "NiveauScolaire")))
            {
                string niveau = GetSafeString(reader, "NiveauScolaire");
                if (niveau.Contains("Bac +") || niveau.Contains("Universitaire"))
                {
                    pnlDiplome.Visible = true;
                    LoadDiplomes();
                    SetDropDownByText(ddlDiplome, GetSafeString(reader, "Diplome"));
                }
            }

            // Professional Information
            if (reader["DateEmbauche"] != DBNull.Value)
                txtDateEmbauche.Text = Convert.ToDateTime(reader["DateEmbauche"]).ToString("yyyy-MM-dd");

            if (reader["DateSortie"] != DBNull.Value)
                txtDateSortieEdit.Text = Convert.ToDateTime(reader["DateSortie"]).ToString("yyyy-MM-dd");

            SetDropDownValue(ddlTypeEmploye, GetSafeString(reader, "TypeEmploye"));

            // Load and set Type Contrat
            LoadTypesContrats(); // Make sure it's loaded
            SetDropDownByText(ddlTypeContrat, GetSafeString(reader, "TypeContrat"));

            SetDropDownValue(ddlShift, GetSafeString(reader, "Shift"));
            SetDropDownValue(ddlSituationCNSS, GetSafeString(reader, "SituationCNSS"));

            // Handle TypeOuvrier panel
            if (GetSafeString(reader, "TypeEmploye") == "OUVRIER")
            {
                pnlTypeOuvrier.Visible = true;
                LoadTypesOuvriers();
                if (reader["IdTypeOuv"] != DBNull.Value)
                    SetDropDownValue(ddlTypeOuvrier, reader["IdTypeOuv"].ToString());
            }

            // Handle CNSS panel
            if (GetSafeString(reader, "SituationCNSS") == "D")
            {
                pnlNumeroCNSS.Visible = true;
                txtNumeroCNSS.Text = GetSafeString(reader, "NumeroCNSS");
            }

            // Load regions and cities if needed
            LoadRegions();
            if (!string.IsNullOrEmpty(GetSafeString(reader, "LieuNaissance")))
            {
                // You might need to set the region first, then load cities
                // This depends on your region/city relationship
                LoadVilles(); // or call based on the stored region
                SetDropDownByText(ddlLieuNaissance, GetSafeString(reader, "LieuNaissance"));
            }
        }

        // Updated SetDropDownByText method with better error handling
        private void SetDropDownByText(DropDownList ddl, string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            // Try exact match first
            ListItem item = ddl.Items.FindByText(text);
            if (item != null)
            {
                ddl.SelectedValue = item.Value;
                return;
            }

            // Try case-insensitive match
            foreach (ListItem listItem in ddl.Items)
            {
                if (string.Equals(listItem.Text, text, StringComparison.OrdinalIgnoreCase))
                {
                    ddl.SelectedValue = listItem.Value;
                    return;
                }
            }

            // If no match found, log for debugging
            System.Diagnostics.Debug.WriteLine($"Could not find '{text}' in dropdown {ddl.ID}");
        }

        // Updated SetDropDownValue method with better error handling
        private void SetDropDownValue(DropDownList ddl, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            ListItem item = ddl.Items.FindByValue(value);
            if (item != null)
            {
                ddl.SelectedValue = value;
            }
            else
            {
                // Log for debugging
                System.Diagnostics.Debug.WriteLine($"Could not find value '{value}' in dropdown {ddl.ID}");
            }
        }

        // Make sure LoadDropDowns calls all the required methods

        private string GetSafeString(SqlDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? "" : reader[columnName].ToString();
        }

        private int? GetNullableInt(SqlDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? (int?)null : Convert.ToInt32(reader[columnName]);
        }



        #region Event Handlers

        protected void ddlSociete_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDepartements();
            LoadEmplacements();
            ClearDependentDropdowns("societe");
        }

        protected void ddlRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadVilles();
        }

        protected void ddlNiveauScolaire_SelectedIndexChanged(object sender, EventArgs e)
        {
            string niveauSelectionne = ddlNiveauScolaire.SelectedItem.Text;
            if (niveauSelectionne.Contains("Bac +") || niveauSelectionne.Contains("Universitaire"))
            {
                pnlDiplome.Visible = true;
                LoadDiplomes();
            }
            else
            {
                pnlDiplome.Visible = false;
            }
        }

        protected void ddlSituationCNSS_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlNumeroCNSS.Visible = (ddlSituationCNSS.SelectedValue == "D");
        }

        protected void ddlTypeEmploye_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlTypeOuvrier.Visible = (ddlTypeEmploye.SelectedValue == "OUVRIER");
            if (pnlTypeOuvrier.Visible)
            {
                LoadTypesOuvriers();
            }
            else
            {
                ddlTypeOuvrier.Items.Clear();
                ddlTypeOuvrier.Items.Add(new ListItem("-- Type Ouvrier --", ""));
            }
        }

        protected void DdlSociete_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDepartements();
            LoadEmplacements();
            ClearDependentDropdowns("societe");
        }

        protected void ddlDepartement_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadServices();
            ClearDependentDropdowns("departement");
        }

        protected void ddlService_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCellules();
            ClearDependentDropdowns("service");
        }

        protected void ddlCellule_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadFonctions();
        }

        protected void BtnModifier_Click(object sender, EventArgs e)
        {
            if (Page.IsValid && ValidateEmployeeForm())
            {
                try
                {
                    ModifierEmploye();
                    // Show success message and redirect
                    Session["SuccessMessage"] = "Employé modifié avec succès!";
                    Response.Redirect("~/personnel/ListeEmployes.aspx");
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Erreur lors de la modification: {ex.Message}");
                }
            }
        }
        private bool ValidateEmployeeForm()
        {
            List<string> errors = new List<string>();

            // Required fields
            if (string.IsNullOrEmpty(ddlSociete.SelectedValue))
                errors.Add("La société est obligatoire");

            if (string.IsNullOrEmpty(txtNom.Text.Trim()))
                errors.Add("Le nom est obligatoire");

            if (string.IsNullOrEmpty(txtPrenom.Text.Trim()))
                errors.Add("Le prénom est obligatoire");

            if (string.IsNullOrEmpty(txtDateEmbauche.Text))
                errors.Add("La date d'embauche est obligatoire");

            if (string.IsNullOrEmpty(ddlTypeEmploye.SelectedValue))
                errors.Add("Le type d'employé est obligatoire");

            if (string.IsNullOrEmpty(txtCinPassport.Text.Trim()))
                errors.Add("Le CIN ou Passeport est obligatoire");

            if (string.IsNullOrEmpty(ddlSexe.SelectedValue))
                errors.Add("Le sexe est obligatoire");

            // Conditional validations
            if (ddlTypeEmploye.SelectedValue == "OUVRIER" && string.IsNullOrEmpty(ddlTypeOuvrier.SelectedValue))
                errors.Add("Le type d'ouvrier est obligatoire pour les ouvriers");

            if (ddlSituationCNSS.SelectedValue == "D" && string.IsNullOrEmpty(txtNumeroCNSS.Text.Trim()))
                errors.Add("Le numéro CNSS est obligatoire pour les employés déclarés");

            // Date validations
            if (!string.IsNullOrEmpty(txtDateEmbauche.Text) && DateTime.TryParse(txtDateEmbauche.Text, out DateTime dateEmbauche))
            {
                if (dateEmbauche > DateTime.Now)
                    errors.Add("La date d'embauche ne peut pas être dans le futur");
            }

            if (!string.IsNullOrEmpty(txtDateSortieEdit.Text) && !string.IsNullOrEmpty(txtDateEmbauche.Text))
            {
                if (DateTime.TryParse(txtDateSortieEdit.Text, out DateTime dateSortie) &&
                    DateTime.TryParse(txtDateEmbauche.Text, out DateTime dateEmb))
                {
                    if (dateSortie < dateEmb)
                        errors.Add("La date de sortie ne peut pas être antérieure à la date d'embauche");
                }
            }

            // Show errors if any
            if (errors.Count > 0)
            {
                string errorMessage = "Veuillez corriger les erreurs suivantes:\\n" + string.Join("\\n", errors);
                ShowErrorMessage(errorMessage);
                return false;
            }

            return true;
        }
        protected void btnAnnuler_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/personnel/ListeEmployes.aspx");
        }

        #endregion

        #region Data Loading Methods

        private void LoadDepartements()
        {
            ddlDepartement.Items.Clear();
            ddlDepartement.Items.Insert(0, new ListItem("-- Département --", ""));

            if (string.IsNullOrEmpty(ddlSociete.SelectedValue))
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT IdDepartement, NomDepartement, CodeDepartement 
                           FROM DEPARTEMENT 
                           WHERE IdSociete = @IdSociete AND Actif = 1 
                           ORDER BY NomDepartement";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string text = $"{reader["NomDepartement"]} ({reader["CodeDepartement"]})";
                                string value = reader["IdDepartement"].ToString();
                                ddlDepartement.Items.Add(new ListItem(text, value));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors du chargement des départements: {ex.Message}");
            }
        }

        private void LoadServices()
        {
            ddlService.Items.Clear();
            ddlService.Items.Insert(0, new ListItem("-- Service --", ""));

            if (string.IsNullOrEmpty(ddlDepartement.SelectedValue))
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT IdService, NomService, CodeService 
                           FROM SERVICE 
                           WHERE IdDepartement = @IdDepartement AND Actif = 1 
                           ORDER BY NomService";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdDepartement", ddlDepartement.SelectedValue);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string text = reader["CodeService"] != DBNull.Value
                                    ? $"{reader["NomService"]} ({reader["CodeService"]})"
                                    : reader["NomService"].ToString();
                                string value = reader["IdService"].ToString();
                                ddlService.Items.Add(new ListItem(text, value));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors du chargement des services: {ex.Message}");
            }
        }

        private void LoadCellules()
        {
            ddlCellule.Items.Clear();
            ddlCellule.Items.Insert(0, new ListItem("-- Cellule --", ""));

            if (string.IsNullOrEmpty(ddlService.SelectedValue))
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT IdCellule, NomCellule, CodeCellule 
                           FROM CELLULE 
                           WHERE IdService = @IdService AND Actif = 1 
                           ORDER BY NomCellule";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdService", ddlService.SelectedValue);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string text = reader["CodeCellule"] != DBNull.Value
                                    ? $"{reader["NomCellule"]} ({reader["CodeCellule"]})"
                                    : reader["NomCellule"].ToString();
                                string value = reader["IdCellule"].ToString();
                                ddlCellule.Items.Add(new ListItem(text, value));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors du chargement des cellules: {ex.Message}");
            }
        }

        private void LoadFonctions()
        {
            ddlFonction.Items.Clear();
            ddlFonction.Items.Insert(0, new ListItem("-- Fonction --", ""));

            if (string.IsNullOrEmpty(ddlService.SelectedValue) || string.IsNullOrEmpty(ddlCellule.SelectedValue))
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT IdFonction, NomFonction, CodeFonction, NiveauResponsabilite 
                           FROM FONCTION 
                           WHERE IdService = @IdService AND IdCellule = @IdCellule AND Actif = 1 
                           ORDER BY NiveauResponsabilite, NomFonction";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdService", ddlService.SelectedValue);
                        cmd.Parameters.AddWithValue("@IdCellule", ddlCellule.SelectedValue);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string niveauText = GetNiveauResponsabiliteText(Convert.ToInt32(reader["NiveauResponsabilite"]));
                                string text = reader["CodeFonction"] != DBNull.Value
                                    ? $"{reader["NomFonction"]} ({reader["CodeFonction"]}) - {niveauText}"
                                    : $"{reader["NomFonction"]} - {niveauText}";
                                string value = reader["IdFonction"].ToString();
                                ddlFonction.Items.Add(new ListItem(text, value));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors du chargement des fonctions: {ex.Message}");
            }
        }

        private void LoadEmplacements()
        {
            ddlEmplacement.Items.Clear();
            ddlEmplacement.Items.Insert(0, new ListItem("-- Emplacement --", ""));

            if (string.IsNullOrEmpty(ddlSociete.SelectedValue))
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT IdEmplacement, NomEmplacement, TypeEmplacement 
                           FROM EMPLACEMENT 
                           WHERE IdSociete = @IdSociete AND Actif = 1 
                           ORDER BY TypeEmplacement, NomEmplacement";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string text = reader["TypeEmplacement"] != DBNull.Value
                                    ? $"{reader["NomEmplacement"]} ({reader["TypeEmplacement"]})"
                                    : reader["NomEmplacement"].ToString();
                                string value = reader["IdEmplacement"].ToString();
                                ddlEmplacement.Items.Add(new ListItem(text, value));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors du chargement des emplacements: {ex.Message}");
            }
        }

        private string GetNiveauResponsabiliteText(int niveau)
        {
            switch (niveau)
            {
                case 1: return "Ouvrier";
                case 2: return "Superviseur";
                case 3: return "Chef";
                default: return "Non défini";
            }
        }

        private void ClearDependentDropdowns(string level)
        {
            switch (level.ToLower())
            {
                case "societe":
                    ddlService.Items.Clear();
                    ddlService.Items.Insert(0, new ListItem("-- Service --", ""));
                    ddlCellule.Items.Clear();
                    ddlCellule.Items.Insert(0, new ListItem("-- Cellule --", ""));
                    ddlFonction.Items.Clear();
                    ddlFonction.Items.Insert(0, new ListItem("-- Fonction --", ""));
                    break;

                case "departement":
                    ddlCellule.Items.Clear();
                    ddlCellule.Items.Insert(0, new ListItem("-- Cellule --", ""));
                    ddlFonction.Items.Clear();
                    ddlFonction.Items.Insert(0, new ListItem("-- Fonction --", ""));
                    break;

                case "service":
                    ddlFonction.Items.Clear();
                    ddlFonction.Items.Insert(0, new ListItem("-- Fonction --", ""));
                    break;
            }
        }

        private void LoadSocietes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT IdSociete, NomSociete, CodeSociete 
                           FROM SOCIETE 
                           WHERE Actif = 1 
                           ORDER BY NomSociete";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            ddlSociete.Items.Clear();
                            ddlSociete.Items.Insert(0, new ListItem("-- Sélectionner Société --", ""));

                            while (reader.Read())
                            {
                                string text = $"{reader["NomSociete"]} ({reader["CodeSociete"]})";
                                string value = reader["IdSociete"].ToString();
                                ddlSociete.Items.Add(new ListItem(text, value));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erreur lors du chargement des sociétés: {ex.Message}");
            }
        }

        public void LoadHierarchyForEmployee(int idSociete, int? idDepartement = null,
            int? idService = null, int? idCellule = null, int? idFonction = null, int? idEmplacement = null)
        {
            LoadSocietes();
            if (idSociete > 0)
            {
                ddlSociete.SelectedValue = idSociete.ToString();

                LoadDepartements();
                LoadEmplacements();

                if (idEmplacement.HasValue)
                    ddlEmplacement.SelectedValue = idEmplacement.ToString();

                if (idDepartement.HasValue)
                {
                    ddlDepartement.SelectedValue = idDepartement.ToString();

                    LoadServices();
                    if (idService.HasValue)
                    {
                        ddlService.SelectedValue = idService.ToString();

                        LoadCellules();
                        if (idCellule.HasValue)
                        {
                            ddlCellule.SelectedValue = idCellule.ToString();

                            LoadFonctions();
                            if (idFonction.HasValue)
                            {
                                ddlFonction.SelectedValue = idFonction.ToString();
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Helper Methods

        private void LoadRegions()
        {
            ddlRegion.Items.Add(new ListItem("Casablanca-Settat", "1"));
            ddlRegion.Items.Add(new ListItem("Rabat-Salé-Kénitra", "2"));
            ddlRegion.Items.Add(new ListItem("Souss-Massa", "3"));
        }

        private void LoadVilles()
        {
            ddlLieuNaissance.Items.Clear();
            ddlLieuNaissance.Items.Insert(0, new ListItem("-- Lieu de Naissance --", ""));

            if (ddlRegion.SelectedValue == "1")
            {
                ddlLieuNaissance.Items.Add(new ListItem("Casablanca", "1"));
                ddlLieuNaissance.Items.Add(new ListItem("Settat", "2"));
                ddlLieuNaissance.Items.Add(new ListItem("Mohammedia", "3"));
            }
        }

        private void LoadSituationsFamiliales()
        {
            ddlSituationFamiliale.Items.Add(new ListItem("Célibataire", "1"));
            ddlSituationFamiliale.Items.Add(new ListItem("Marié(e)", "2"));
            ddlSituationFamiliale.Items.Add(new ListItem("Divorcé(e)", "3"));
            ddlSituationFamiliale.Items.Add(new ListItem("Veuf(ve)", "4"));
        }

        private void LoadNationalites()
        {
            ddlNationalite.Items.Add(new ListItem("Marocaine", "1"));
            ddlNationalite.Items.Add(new ListItem("Française", "2"));
            ddlNationalite.Items.Add(new ListItem("Autre", "3"));
        }

        private void LoadNiveauxScolaires()
        {
            ddlNiveauScolaire.Items.Add(new ListItem("Primaire", "1"));
            ddlNiveauScolaire.Items.Add(new ListItem("Secondaire", "2"));
            ddlNiveauScolaire.Items.Add(new ListItem("Bac", "3"));
            ddlNiveauScolaire.Items.Add(new ListItem("Bac +2", "4"));
            ddlNiveauScolaire.Items.Add(new ListItem("Bac +3", "5"));
            ddlNiveauScolaire.Items.Add(new ListItem("Bac +5", "6"));
        }

        private void LoadDiplomes()
        {
            ddlDiplome.Items.Clear();
            ddlDiplome.Items.Insert(0, new ListItem("-- Diplôme --", ""));
            ddlDiplome.Items.Add(new ListItem("DUT Génie Industriel", "1"));
            ddlDiplome.Items.Add(new ListItem("BTS Maintenance", "2"));
            ddlDiplome.Items.Add(new ListItem("Licence Gestion", "3"));
            ddlDiplome.Items.Add(new ListItem("Master Management", "4"));
            ddlDiplome.Items.Add(new ListItem("Ingénieur Agro-alimentaire", "5"));
        }

        private void LoadTypesOuvriers()
        {
            ddlTypeOuvrier.Items.Clear();
            ddlTypeOuvrier.Items.Add(new ListItem("-- Type Ouvrier --", ""));

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT IdTypeOuv, NomTypeOuv 
                           FROM TYPEOUVRIER 
                           WHERE Actif = 1 
                           ORDER BY NomTypeOuv";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ddlTypeOuvrier.Items.Add(new ListItem(
                                    reader["NomTypeOuv"].ToString(),
                                    reader["IdTypeOuv"].ToString()
                                ));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Erreur lors du chargement des types d'ouvriers", ex);
                }
            }
        }

        private void LoadTypesContrats()
        {
            ddlTypeContrat.Items.Add(new ListItem("CDI", "1"));
            ddlTypeContrat.Items.Add(new ListItem("CDD", "2"));
            ddlTypeContrat.Items.Add(new ListItem("Stage", "3"));
            ddlTypeContrat.Items.Add(new ListItem("Temporaire", "4"));
            ddlTypeContrat.Items.Add(new ListItem("Saisonnier", "5"));
        }

        #endregion

        private void ModifierEmploye()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query = @"
                UPDATE EMPLOYE SET
                    IdSociete = @IdSociete,
                    Nom = @Nom,
                    Prenom = @Prenom,
                    DateNaissance = @DateNaissance,
                    LieuNaissance = @LieuNaissance,
                    CinPassport = @CinPassport,
                    Sexe = @Sexe,
                    SituationFamiliale = @SituationFamiliale,
                    NombreEnfants = @NombreEnfants,
                    Adresse = @Adresse,
                    Telephone = @Telephone,
                    Nationalite = @Nationalite,
                    NiveauScolaire = @NiveauScolaire,
                    Diplome = @Diplome,
                    DateEmbauche = @DateEmbauche,
                    DateSortie = @DateSortie,
                    TypeContrat = @TypeContrat,
                    TypeEmploye = @TypeEmploye,
                    IdTypeOuv = @IdTypeOuv,
                    IdDepartement = @IdDepartement,
                    IdService = @IdService,
                    IdCellule = @IdCellule,
                    IdFonction = @IdFonction,
                    IdEmplacement = @IdEmplacement,
                    Shift = @Shift,
                    SituationCNSS = @SituationCNSS,
                    NumeroCNSS = @NumeroCNSS,
                    DateModification = @DateModification,
                    ModifiePar = @ModifiePar
                WHERE IdEmploye = @IdEmploye";

                        using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                        {
                            // Required fields
                            cmd.Parameters.AddWithValue("@IdEmploye", EmployeeId);
                            cmd.Parameters.AddWithValue("@IdSociete", Convert.ToInt32(ddlSociete.SelectedValue));
                            cmd.Parameters.AddWithValue("@Nom", txtNom.Text.Trim());
                            cmd.Parameters.AddWithValue("@Prenom", txtPrenom.Text.Trim());
                            cmd.Parameters.AddWithValue("@DateEmbauche", Convert.ToDateTime(txtDateEmbauche.Text));
                            cmd.Parameters.AddWithValue("@TypeEmploye", ddlTypeEmploye.SelectedValue);
                            cmd.Parameters.AddWithValue("@DateModification", DateTime.Now);
                            cmd.Parameters.AddWithValue("@ModifiePar", Session["UserName"]?.ToString() ?? "System");

                            // Optional personal information
                            cmd.Parameters.AddWithValue("@DateNaissance",
                                string.IsNullOrEmpty(txtDateNaissance.Text) ? (object)DBNull.Value : Convert.ToDateTime(txtDateNaissance.Text));

                            cmd.Parameters.AddWithValue("@LieuNaissance",
                                string.IsNullOrEmpty(ddlLieuNaissance.SelectedValue) ? (object)DBNull.Value : ddlLieuNaissance.SelectedItem.Text);

                            cmd.Parameters.AddWithValue("@CinPassport",
                                string.IsNullOrEmpty(txtCinPassport.Text) ? (object)DBNull.Value : txtCinPassport.Text.Trim());

                            cmd.Parameters.AddWithValue("@Sexe",
                                string.IsNullOrEmpty(ddlSexe.SelectedValue) ? (object)DBNull.Value : ddlSexe.SelectedValue);

                            cmd.Parameters.AddWithValue("@SituationFamiliale",
                                string.IsNullOrEmpty(ddlSituationFamiliale.SelectedValue) ? (object)DBNull.Value : ddlSituationFamiliale.SelectedItem.Text);

                            cmd.Parameters.AddWithValue("@NombreEnfants",
                                string.IsNullOrEmpty(txtNombreEnfants.Text) ? 0 : Convert.ToInt32(txtNombreEnfants.Text));

                            cmd.Parameters.AddWithValue("@Adresse",
                                string.IsNullOrEmpty(txtAdresse.Text) ? (object)DBNull.Value : txtAdresse.Text.Trim());

                            cmd.Parameters.AddWithValue("@Telephone",
                                string.IsNullOrEmpty(txtTelephone.Text) ? (object)DBNull.Value : txtTelephone.Text.Trim());

                            cmd.Parameters.AddWithValue("@Nationalite",
                                string.IsNullOrEmpty(ddlNationalite.SelectedValue) ? (object)DBNull.Value : ddlNationalite.SelectedItem.Text);

                            // Education information
                            cmd.Parameters.AddWithValue("@NiveauScolaire",
                                string.IsNullOrEmpty(ddlNiveauScolaire.SelectedValue) ? (object)DBNull.Value : ddlNiveauScolaire.SelectedItem.Text);

                            cmd.Parameters.AddWithValue("@Diplome",
                                string.IsNullOrEmpty(ddlDiplome.SelectedValue) ? (object)DBNull.Value : ddlDiplome.SelectedItem.Text);

                            // Professional information
                            cmd.Parameters.AddWithValue("@DateSortie",
                                string.IsNullOrEmpty(txtDateSortieEdit.Text) ? (object)DBNull.Value : Convert.ToDateTime(txtDateSortieEdit.Text));

                            cmd.Parameters.AddWithValue("@TypeContrat",
                                string.IsNullOrEmpty(ddlTypeContrat.SelectedValue) ? (object)DBNull.Value : ddlTypeContrat.SelectedItem.Text);

                            cmd.Parameters.AddWithValue("@IdTypeOuv",
                                string.IsNullOrEmpty(ddlTypeOuvrier.SelectedValue) ? (object)DBNull.Value : Convert.ToInt32(ddlTypeOuvrier.SelectedValue));

                            // Organizational hierarchy
                            cmd.Parameters.AddWithValue("@IdDepartement",
                                string.IsNullOrEmpty(ddlDepartement.SelectedValue) ? (object)DBNull.Value : Convert.ToInt32(ddlDepartement.SelectedValue));

                            cmd.Parameters.AddWithValue("@IdService",
                                string.IsNullOrEmpty(ddlService.SelectedValue) ? (object)DBNull.Value : Convert.ToInt32(ddlService.SelectedValue));

                            cmd.Parameters.AddWithValue("@IdCellule",
                                string.IsNullOrEmpty(ddlCellule.SelectedValue) ? (object)DBNull.Value : Convert.ToInt32(ddlCellule.SelectedValue));

                            cmd.Parameters.AddWithValue("@IdFonction",
                                string.IsNullOrEmpty(ddlFonction.SelectedValue) ? (object)DBNull.Value : Convert.ToInt32(ddlFonction.SelectedValue));

                            cmd.Parameters.AddWithValue("@IdEmplacement",
                                string.IsNullOrEmpty(ddlEmplacement.SelectedValue) ? (object)DBNull.Value : Convert.ToInt32(ddlEmplacement.SelectedValue));

                            // Work schedule
                            cmd.Parameters.AddWithValue("@Shift",
                                string.IsNullOrEmpty(ddlShift.SelectedValue) ? (object)DBNull.Value : ddlShift.SelectedValue);

                            // CNSS information
                            cmd.Parameters.AddWithValue("@SituationCNSS",
                                string.IsNullOrEmpty(ddlSituationCNSS.SelectedValue) ? (object)DBNull.Value : ddlSituationCNSS.SelectedValue);

                            cmd.Parameters.AddWithValue("@NumeroCNSS",
                                string.IsNullOrEmpty(txtNumeroCNSS.Text) ? (object)DBNull.Value : txtNumeroCNSS.Text.Trim());

                            // Execute the update command
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                // Commit the transaction if successful
                                transaction.Commit();

                                // Log the modification (optional)
                                LogEmployeeModification(EmployeeId, Session["UserName"]?.ToString() ?? "System");
                            }
                            else
                            {
                                // Rollback if no rows were affected
                                transaction.Rollback();
                                throw new Exception("Aucune ligne n'a été mise à jour. L'employé n'existe peut-être plus.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Rollback transaction in case of any error
                        transaction.Rollback();
                        throw new Exception($"Erreur lors de la modification de l'employé: {ex.Message}", ex);
                    }
                }
            }
        }

        // Optional: Method to log employee modifications for audit trail
        private void LogEmployeeModification(int employeeId, string modifiedBy)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string logQuery = @"
                INSERT INTO LOG_EMPLOYE (IdEmploye, Action, DateAction, UtilisateurAction, Details)
                VALUES (@IdEmploye, 'MODIFICATION', @DateAction, @UtilisateurAction, @Details)";

                    using (SqlCommand cmd = new SqlCommand(logQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeeId);
                        cmd.Parameters.AddWithValue("@DateAction", DateTime.Now);
                        cmd.Parameters.AddWithValue("@UtilisateurAction", modifiedBy);
                        cmd.Parameters.AddWithValue("@Details", "Modification des informations employé");

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - logging failure shouldn't stop the main operation
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'enregistrement du log: {ex.Message}");
            }
        }

        private void ShowErrorMessage(string message)
        {
            // Escape single quotes for JavaScript
            string escapedMessage = message.Replace("'", "\\'");

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowErrorToast",
                $"Swal.fire({{ toast: true, position: 'top-end', icon: 'error', title: '{escapedMessage}', showConfirmButton: false, timer: 4000 }});", true);
        }
    }

}