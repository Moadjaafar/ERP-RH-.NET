using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace APPLICATION_RH.Pages.personnel
{
    public partial class AjouterEmploye : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDropDowns();
            }
        }

        private void LoadDropDowns()
        {
            LoadSocietes();
            LoadRegions();
            LoadSituationsFamiliales();
            LoadNationalites();
            LoadNiveauxScolaires();
            LoadDepartements();
            LoadTypesOuvriers();
            LoadTypesContrats();
        }

        #region Event Handlers

        protected void ddlSociete_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadEmplacements();
        }

        protected void ddlRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadVilles();
        }

        protected void ddlNiveauScolaire_SelectedIndexChanged(object sender, EventArgs e)
        {
            string niveauSelectionne = ddlNiveauScolaire.SelectedItem.Text;

            // Afficher diplôme pour niveaux universitaires
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
            // Afficher numéro CNSS si déclaré
            pnlNumeroCNSS.Visible = (ddlSituationCNSS.SelectedValue == "D");
        }

        // Method to load societes (already implemented, but improved)
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
                ShowNotification($"Erreur lors du chargement des sociétés: {ex.Message}", "alert-danger");
            }
        }

        // Method to load departments based on selected society
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
                ShowNotification($"Erreur lors du chargement des départements: {ex.Message}", "alert-danger");
            }
        }

        // Method to load services based on selected department
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
                ShowNotification($"Erreur lors du chargement des services: {ex.Message}", "alert-danger");
            }
        }

        // Method to load cellules based on selected service
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
                ShowNotification($"Erreur lors du chargement des cellules: {ex.Message}", "alert-danger");
            }
        }

        // Method to load functions based on selected service and cellule
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
                ShowNotification($"Erreur lors du chargement des fonctions: {ex.Message}", "alert-danger");
            }
        }

        // Method to load emplacements based on selected society
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
                ShowNotification($"Erreur lors du chargement des emplacements: {ex.Message}", "alert-danger");
            }
        }

        // Helper method to convert responsibility level to text
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

        // Updated event handlers with proper cascade clearing
        protected void DdlSociete_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDepartements();
            LoadEmplacements();

            // Clear dependent dropdowns
            ClearDependentDropdowns("societe");
        }

        protected void ddlDepartement_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadServices();

            // Clear dependent dropdowns
            ClearDependentDropdowns("departement");
        }

        protected void ddlService_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCellules();

            // Clear dependent dropdowns
            ClearDependentDropdowns("service");
        }

        protected void ddlCellule_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadFonctions();
        }

        // Helper method to clear dependent dropdowns
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

        // Method to populate all hierarchy dropdowns if editing an existing employee
        public void LoadHierarchyForEmployee(int idSociete, int? idDepartement = null,
            int? idService = null, int? idCellule = null, int? idFonction = null, int? idEmplacement = null)
        {
            // Load societes first
            LoadSocietes();
            if (idSociete > 0)
            {
                ddlSociete.SelectedValue = idSociete.ToString();

                // Load departments
                LoadDepartements();
                LoadEmplacements();

                if (idEmplacement.HasValue)
                    ddlEmplacement.SelectedValue = idEmplacement.ToString();

                if (idDepartement.HasValue)
                {
                    ddlDepartement.SelectedValue = idDepartement.ToString();

                    // Load services
                    LoadServices();
                    if (idService.HasValue)
                    {
                        ddlService.SelectedValue = idService.ToString();

                        // Load cellules
                        LoadCellules();
                        if (idCellule.HasValue)
                        {
                            ddlCellule.SelectedValue = idCellule.ToString();

                            // Load functions
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

        protected void btnAjouter_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                try
                {
                    // Logique d'ajout employé
                    AjoutEmploye();

                    ShowNotification("Employé ajouté avec succès!", "alert-success");
                    ViderFormulaire();
                }
                catch (Exception ex)
                {
                    ShowNotification($"Erreur lors de l'ajout: {ex.Message}", "alert-danger");
                }
            }
        }

        protected void btnAnnuler_Click(object sender, EventArgs e)
        {
            ViderFormulaire();
        }



        #endregion

        #region Helper Methods


        private void LoadRegions()
        {
            // TODO: Charger depuis la base de données
            ddlRegion.Items.Add(new ListItem("Casablanca-Settat", "1"));
            ddlRegion.Items.Add(new ListItem("Rabat-Salé-Kénitra", "2"));
            ddlRegion.Items.Add(new ListItem("Souss-Massa", "3"));
        }

        private void LoadVilles()
        {
            ddlLieuNaissance.Items.Clear();
            ddlLieuNaissance.Items.Insert(0, new ListItem("-- Lieu de Naissance --", ""));

            // TODO: Charger les villes selon la région sélectionnée
            if (ddlRegion.SelectedValue == "1") // Casablanca-Settat
            {
                ddlLieuNaissance.Items.Add(new ListItem("Casablanca", "1"));
                ddlLieuNaissance.Items.Add(new ListItem("Settat", "2"));
                ddlLieuNaissance.Items.Add(new ListItem("Mohammedia", "3"));
            }
            // Ajouter d'autres régions...
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
                    // Log error or show message
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
        protected void ddlTypeEmploye_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Show TypeOuvrier panel only if TypeEmploye is "OUVRIER"
            pnlTypeOuvrier.Visible = (ddlTypeEmploye.SelectedValue == "OUVRIER");

            if (pnlTypeOuvrier.Visible)
            {
                LoadTypesOuvriers(); // Load types when panel becomes visible
            }
            else
            {
                ddlTypeOuvrier.Items.Clear();
                ddlTypeOuvrier.Items.Add(new ListItem("-- Type Ouvrier --", ""));
            }
        }
        private void AjoutEmploye()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {

                        // Generate unique matricule for the selected society
                        string matricule = GenerateMatricule(transaction, conn);

                        string query = @"
                    INSERT INTO EMPLOYE (
                        IdSociete, Matricule, Nom, Prenom, DateNaissance, LieuNaissance,
                        CinPassport, Sexe, SituationFamiliale, NombreEnfants, Adresse,
                        Telephone, Nationalite, NiveauScolaire, Diplome, DateEmbauche,
                        TypeContrat, TypeEmploye, IdTypeOuv, IdDepartement, IdService,
                        IdCellule, IdFonction, IdEmplacement, Shift, SituationCNSS,
                        NumeroCNSS, DateCreation, ModifiePar
                    ) VALUES (
                        @IdSociete, @Matricule, @Nom, @Prenom, @DateNaissance, @LieuNaissance,
                        @CinPassport, @Sexe, @SituationFamiliale, @NombreEnfants, @Adresse,
                        @Telephone, @Nationalite, @NiveauScolaire, @Diplome, @DateEmbauche,
                        @TypeContrat, @TypeEmploye, @IdTypeOuv, @IdDepartement, @IdService,
                        @IdCellule, @IdFonction, @IdEmplacement, @Shift, @SituationCNSS,
                        @NumeroCNSS, @DateCreation, @ModifiePar
                    )";

                        using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                        {
                            // Required fields
                            cmd.Parameters.AddWithValue("@IdSociete", Convert.ToInt32(ddlSociete.SelectedValue));
                            cmd.Parameters.AddWithValue("@Matricule", matricule);
                            cmd.Parameters.AddWithValue("@Nom", txtNom.Text.Trim());
                            cmd.Parameters.AddWithValue("@Prenom", txtPrenom.Text.Trim());
                            cmd.Parameters.AddWithValue("@DateEmbauche", Convert.ToDateTime(txtDateEmbauche.Text));
                            cmd.Parameters.AddWithValue("@TypeEmploye", ddlTypeEmploye.SelectedValue);
                            cmd.Parameters.AddWithValue("@DateCreation", DateTime.Now);
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

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                transaction.Commit();
                                ShowNotification($"Employé ajouté avec succès! Matricule: {matricule}", "alert-success");
                                ViderFormulaire();
                            }
                            else
                            {
                                transaction.Rollback();
                                ShowNotification("Erreur lors de l'ajout de l'employé", "alert-danger");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Erreur lors de l'ajout de l'employé: {ex.Message}", ex);
                    }
                }
            }
        }

        private string GenerateMatricule(SqlTransaction transaction, SqlConnection conn)
        {
            try
            {
                // Get the highest existing matricule and increment by 1
                string getMaxMatriculeQuery = @"
            SELECT ISNULL(MAX(CAST(Matricule AS INT)), 0) + 1
            FROM EMPLOYE 
            WHERE ISNUMERIC(Matricule) = 1";

                int nextMatricule = 1;
                using (SqlCommand cmd = new SqlCommand(getMaxMatriculeQuery, conn, transaction))
                {
                    object result = cmd.ExecuteScalar();
                    nextMatricule = result != null ? Convert.ToInt32(result) : 1;
                }

                // Ensure uniqueness (in case of concurrent inserts)
                string checkUniqueQuery = "SELECT COUNT(*) FROM EMPLOYE WHERE Matricule = @Matricule";
                using (SqlCommand cmd = new SqlCommand(checkUniqueQuery, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Matricule", nextMatricule.ToString());

                    while (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                    {
                        nextMatricule++;
                        cmd.Parameters["@Matricule"].Value = nextMatricule.ToString();
                    }
                }

                return nextMatricule.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la génération du matricule: {ex.Message}", ex);
            }
        }
        
        // Enhanced form validation method
        private bool ValidateEmployeeForm()
        {
            bool isValid = true;
            List<string> errors = new List<string>();

            // Required field validations
            if (string.IsNullOrEmpty(ddlSociete.SelectedValue))
            {
                errors.Add("La société est obligatoire");
                isValid = false;
            }

            if (string.IsNullOrEmpty(txtNom.Text.Trim()))
            {
                errors.Add("Le nom est obligatoire");
                isValid = false;
            }

            if (string.IsNullOrEmpty(txtPrenom.Text.Trim()))
            {
                errors.Add("Le prénom est obligatoire");
                isValid = false;
            }

            if (string.IsNullOrEmpty(txtDateEmbauche.Text))
            {
                errors.Add("La date d'embauche est obligatoire");
                isValid = false;
            }
            else if (DateTime.TryParse(txtDateEmbauche.Text, out DateTime dateEmbauche))
            {
                if (dateEmbauche > DateTime.Now)
                {
                    errors.Add("La date d'embauche ne peut pas être dans le futur");
                    isValid = false;
                }
            }

            if (string.IsNullOrEmpty(ddlTypeEmploye.SelectedValue))
            {
                errors.Add("Le type d'employé est obligatoire");
                isValid = false;
            }

            // TypeOuvrier validation for OUVRIER type
            if (ddlTypeEmploye.SelectedValue == "OUVRIER" && string.IsNullOrEmpty(ddlTypeOuvrier.SelectedValue))
            {
                errors.Add("Le type d'ouvrier est obligatoire pour les ouvriers");
                isValid = false;
            }

            // CNSS number validation
            if (ddlSituationCNSS.SelectedValue == "D" && string.IsNullOrEmpty(txtNumeroCNSS.Text.Trim()))
            {
                errors.Add("Le numéro CNSS est obligatoire pour les employés déclarés");
                isValid = false;
            }

            // Date validation
            if (!string.IsNullOrEmpty(txtDateNaissance.Text) && !string.IsNullOrEmpty(txtDateEmbauche.Text))
            {
                if (DateTime.TryParse(txtDateNaissance.Text, out DateTime dateNaissance) &&
                    DateTime.TryParse(txtDateEmbauche.Text, out DateTime dateEmb))
                {
                    int age = dateEmb.Year - dateNaissance.Year;
                    if (dateEmb < dateNaissance.AddYears(age)) age--;

                    if (age < 16)
                    {
                        errors.Add("L'employé doit avoir au moins 16 ans à la date d'embauche");
                        isValid = false;
                    }
                }
            }

            // Display validation errors
            if (!isValid)
            {
                string errorMessage = "Veuillez corriger les erreurs suivantes:<br/>" + string.Join("<br/>", errors);
                ShowNotification(errorMessage, "alert-danger");
            }

            return isValid;
        }

        // Enhanced button click handler
        protected void BtnAjouter_Click(object sender, EventArgs e)
        {
            if (Page.IsValid && ValidateEmployeeForm())
            {
                try
                {
                    AjoutEmploye();
                }
                catch (Exception ex)
                {
                    ShowNotification($"Erreur lors de l'ajout: {ex.Message}", "alert-danger");
                }
            }
        }

        // Enhanced clear form method
        private void ViderFormulaire()
        {
            // Personal information
            txtNom.Text = "";
            txtPrenom.Text = "";
            txtDateNaissance.Text = "";
            ddlRegion.SelectedIndex = 0;
            ddlLieuNaissance.Items.Clear();
            ddlLieuNaissance.Items.Insert(0, new ListItem("-- Lieu de Naissance --", ""));
            txtTelephone.Text = "";
            txtCinPassport.Text = "";
            ddlSituationFamiliale.SelectedIndex = 0;
            txtNombreEnfants.Text = "0";
            ddlSexe.SelectedIndex = 0;
            ddlNationalite.SelectedIndex = 0;
            ddlNiveauScolaire.SelectedIndex = 0;
            ddlDiplome.SelectedIndex = 0;
            pnlDiplome.Visible = false;
            txtAdresse.Text = "";

            // Professional information
            txtDateEmbauche.Text = "";
            ddlSituationCNSS.SelectedIndex = 0;
            pnlNumeroCNSS.Visible = false;
            txtNumeroCNSS.Text = "";
            ddlTypeEmploye.SelectedIndex = 0;
            ddlTypeOuvrier.SelectedIndex = 0;
            pnlTypeOuvrier.Visible = false;
            ddlTypeContrat.SelectedIndex = 0;

            // Organizational hierarchy
            ddlDepartement.Items.Clear();
            ddlDepartement.Items.Insert(0, new ListItem("-- Département --", ""));
            ddlService.Items.Clear();
            ddlService.Items.Insert(0, new ListItem("-- Service --", ""));
            ddlCellule.Items.Clear();
            ddlCellule.Items.Insert(0, new ListItem("-- Cellule --", ""));
            ddlFonction.Items.Clear();
            ddlFonction.Items.Insert(0, new ListItem("-- Fonction --", ""));
            ddlEmplacement.Items.Clear();
            ddlEmplacement.Items.Insert(0, new ListItem("-- Emplacement --", ""));
            ddlShift.SelectedIndex = 0;

            // Hide notification
        }


        private void ShowNotification(string message, string cssClass)
        {
            lblNotification.Text = message;
            pnlNotification.CssClass = $"alert {cssClass} alert-dismissible fade show";
            pnlNotification.Visible = true;
        }

        #endregion
    }
}