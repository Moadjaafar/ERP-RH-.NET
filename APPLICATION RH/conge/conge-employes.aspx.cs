using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.conge
{
    public partial class conge_employes : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSocietes();
                LoadMotifs();
                LoadEmployees();
                LoadAbsences();
            }
        }

        #region Filter Methods

        private void LoadSocietes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT IdSociete, NomSociete FROM SOCIETE WHERE Actif = 1 ORDER BY NomSociete";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        ddlSociete.DataSource = cmd.ExecuteReader();
                        ddlSociete.DataTextField = "NomSociete";
                        ddlSociete.DataValueField = "IdSociete";
                        ddlSociete.DataBind();
                        ddlSociete.Items.Insert(0, new ListItem("-- Toutes --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des societes: " + ex.Message);
            }
        }

        private void LoadDepartements()
        {
            ddlDepartement.Items.Clear();
            ddlDepartement.Items.Insert(0, new ListItem("-- Tous --", ""));

            if (string.IsNullOrEmpty(ddlSociete.SelectedValue)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT IdDepartement, NomDepartement FROM DEPARTEMENT WHERE IdSociete = @IdSociete AND Actif = 1 ORDER BY NomDepartement";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);
                        conn.Open();
                        ddlDepartement.DataSource = cmd.ExecuteReader();
                        ddlDepartement.DataTextField = "NomDepartement";
                        ddlDepartement.DataValueField = "IdDepartement";
                        ddlDepartement.DataBind();
                        ddlDepartement.Items.Insert(0, new ListItem("-- Tous --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des departements: " + ex.Message);
            }
        }

        private void LoadServices()
        {
            ddlService.Items.Clear();
            ddlService.Items.Insert(0, new ListItem("-- Tous --", ""));

            if (string.IsNullOrEmpty(ddlDepartement.SelectedValue)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT IdService, NomService FROM SERVICE WHERE IdDepartement = @IdDepartement AND Actif = 1 ORDER BY NomService";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdDepartement", ddlDepartement.SelectedValue);
                        conn.Open();
                        ddlService.DataSource = cmd.ExecuteReader();
                        ddlService.DataTextField = "NomService";
                        ddlService.DataValueField = "IdService";
                        ddlService.DataBind();
                        ddlService.Items.Insert(0, new ListItem("-- Tous --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des services: " + ex.Message);
            }
        }

        private void LoadCellules()
        {
            ddlCellule.Items.Clear();
            ddlCellule.Items.Insert(0, new ListItem("-- Toutes --", ""));

            if (string.IsNullOrEmpty(ddlService.SelectedValue)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT IdCellule, NomCellule FROM CELLULE WHERE IdService = @IdService AND Actif = 1 ORDER BY NomCellule";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdService", ddlService.SelectedValue);
                        conn.Open();
                        ddlCellule.DataSource = cmd.ExecuteReader();
                        ddlCellule.DataTextField = "NomCellule";
                        ddlCellule.DataValueField = "IdCellule";
                        ddlCellule.DataBind();
                        ddlCellule.Items.Insert(0, new ListItem("-- Toutes --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des cellules: " + ex.Message);
            }
        }

        protected void ddlSociete_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDepartements();
            LoadEmployees();
        }

        protected void ddlDepartement_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadServices();
            LoadEmployees();
        }

        protected void ddlService_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCellules();
            LoadEmployees();
        }

        protected void ddlCellule_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadEmployees();
        }

        #endregion

        private void LoadMotifs()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT IdMotif, NomMotif FROM MOTIF_ABSENCE WHERE IsActive = 1 AND is_deleted = 0 ORDER BY NomMotif";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        ddlMotif.DataSource = cmd.ExecuteReader();
                        ddlMotif.DataTextField = "NomMotif";
                        ddlMotif.DataValueField = "IdMotif";
                        ddlMotif.DataBind();
                        ddlMotif.Items.Insert(0, new ListItem("-- Selectionner --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des motifs: " + ex.Message);
            }
        }

        private void LoadEmployees()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT IdEmploye, Matricule, Nom + ' ' + Prenom AS NomComplet
                        FROM EMPLOYE
                        WHERE is_deleted = 0 AND DateSortie IS NULL";

                    // Apply filters
                    if (!string.IsNullOrEmpty(ddlSociete.SelectedValue))
                        query += " AND IdSociete = @IdSociete";

                    if (!string.IsNullOrEmpty(ddlDepartement.SelectedValue))
                        query += " AND IdDepartement = @IdDepartement";

                    if (!string.IsNullOrEmpty(ddlService.SelectedValue))
                        query += " AND IdService = @IdService";

                    if (!string.IsNullOrEmpty(ddlCellule.SelectedValue))
                        query += " AND IdCellule = @IdCellule";

                    query += " ORDER BY Nom, Prenom";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(ddlSociete.SelectedValue))
                            cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);

                        if (!string.IsNullOrEmpty(ddlDepartement.SelectedValue))
                            cmd.Parameters.AddWithValue("@IdDepartement", ddlDepartement.SelectedValue);

                        if (!string.IsNullOrEmpty(ddlService.SelectedValue))
                            cmd.Parameters.AddWithValue("@IdService", ddlService.SelectedValue);

                        if (!string.IsNullOrEmpty(ddlCellule.SelectedValue))
                            cmd.Parameters.AddWithValue("@IdCellule", ddlCellule.SelectedValue);

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            ddlEmploye.Items.Clear();
                            ddlEmploye.Items.Add(new ListItem("-- Selectionner Employe --", ""));

                            while (reader.Read())
                            {
                                string text = reader["Matricule"].ToString() + " - " + reader["NomComplet"].ToString();
                                string value = reader["IdEmploye"].ToString();
                                ddlEmploye.Items.Add(new ListItem(text, value));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des employes: " + ex.Message);
            }
        }

        private void LoadAbsences()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT
                            a.IdAbsence,
                            e.Matricule,
                            e.Nom + ' ' + e.Prenom AS NomComplet,
                            m.NomMotif,
                            m.CouleurBadge,
                            a.DateDebut,
                            a.DateFin,
                            DATEDIFF(DAY, a.DateDebut, a.DateFin) + 1 AS NombreJours,
                            a.Description
                        FROM ABSENCE_EMPLOYE a
                        INNER JOIN EMPLOYE e ON a.IdEmploye = e.IdEmploye
                        INNER JOIN MOTIF_ABSENCE m ON a.IdMotif = m.IdMotif
                        WHERE a.is_deleted = 0
                        ORDER BY a.DateDebut DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvAbsences.DataSource = dt;
                        gvAbsences.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des absences: " + ex.Message);
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            try
            {
                DateTime dateDebut = DateTime.Parse(txtDateDebut.Text);
                DateTime dateFin = DateTime.Parse(txtDateFin.Text);

                if (dateFin < dateDebut)
                {
                    ShowError("La date de fin doit etre superieure ou egale a la date de debut");
                    return;
                }

                int employeId = Convert.ToInt32(ddlEmploye.SelectedValue);
                int idMotif = Convert.ToInt32(ddlMotif.SelectedValue);
                string description = txtDescription.Text.Trim();
                int currentUserId = GetCurrentUserId();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query;

                    if (string.IsNullOrEmpty(hfEditId.Value))
                    {
                        // Insert new absence
                        query = @"
                            INSERT INTO ABSENCE_EMPLOYE
                            (IdEmploye, IdMotif, DateDebut, DateFin, Description, CreePar, DateCreation, is_deleted)
                            VALUES
                            (@IdEmploye, @IdMotif, @DateDebut, @DateFin, @Description, @CreePar, GETDATE(), 0)";
                    }
                    else
                    {
                        // Update existing absence
                        query = @"
                            UPDATE ABSENCE_EMPLOYE SET
                                IdEmploye = @IdEmploye,
                                IdMotif = @IdMotif,
                                DateDebut = @DateDebut,
                                DateFin = @DateFin,
                                Description = @Description
                            WHERE IdAbsence = @IdAbsence AND is_deleted = 0";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdEmploye", employeId);
                        cmd.Parameters.AddWithValue("@IdMotif", idMotif);
                        cmd.Parameters.AddWithValue("@DateDebut", dateDebut);
                        cmd.Parameters.AddWithValue("@DateFin", dateFin);
                        cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);

                        if (string.IsNullOrEmpty(hfEditId.Value))
                        {
                            cmd.Parameters.AddWithValue("@CreePar", currentUserId);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@IdAbsence", Convert.ToInt32(hfEditId.Value));
                        }

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowSuccess(string.IsNullOrEmpty(hfEditId.Value) ? "Absence ajoutee avec succes" : "Absence modifiee avec succes");
                ClearForm();
                LoadAbsences();
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de l'enregistrement: " + ex.Message);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void gvAbsences_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int absenceId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditAbsence")
            {
                LoadAbsenceForEdit(absenceId);
            }
            else if (e.CommandName == "DeleteAbsence")
            {
                DeleteAbsence(absenceId);
            }
        }

        private void LoadAbsenceForEdit(int absenceId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT IdEmploye, IdMotif, DateDebut, DateFin, Description
                        FROM ABSENCE_EMPLOYE
                        WHERE IdAbsence = @IdAbsence AND is_deleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdAbsence", absenceId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ddlEmploye.SelectedValue = reader["IdEmploye"].ToString();
                                ddlMotif.SelectedValue = reader["IdMotif"].ToString();
                                txtDateDebut.Text = Convert.ToDateTime(reader["DateDebut"]).ToString("yyyy-MM-dd");
                                txtDateFin.Text = Convert.ToDateTime(reader["DateFin"]).ToString("yyyy-MM-dd");
                                txtDescription.Text = reader["Description"].ToString();
                                hfEditId.Value = absenceId.ToString();
                                litFormTitle.Text = "Modifier une Absence";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement de l'absence: " + ex.Message);
            }
        }

        private void DeleteAbsence(int absenceId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "UPDATE ABSENCE_EMPLOYE SET is_deleted = 1 WHERE IdAbsence = @IdAbsence";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdAbsence", absenceId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowSuccess("Absence supprimee avec succes");
                LoadAbsences();
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors de la suppression: " + ex.Message);
            }
        }

        private void ClearForm()
        {
            ddlEmploye.SelectedIndex = 0;
            ddlMotif.SelectedIndex = 0;
            txtDateDebut.Text = string.Empty;
            txtDateFin.Text = string.Empty;
            txtDescription.Text = string.Empty;
            hfEditId.Value = string.Empty;
            litFormTitle.Text = "Ajouter une Absence";
        }

        private int GetCurrentUserId()
        {
            if (Session["EmployeId"] != null)
            {
                return Convert.ToInt32(Session["EmployeId"]);
            }
            return 0;
        }

        private void ShowSuccess(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Success", $"alert('{message}');", true);
        }

        private void ShowError(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Error", $"alert('{message}');", true);
        }
    }
}
