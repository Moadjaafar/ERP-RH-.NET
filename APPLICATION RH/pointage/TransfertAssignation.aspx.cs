using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH.pointage
{
    public partial class TransfertAssignation : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSocietes();
                txtDateTransfert.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadTransferHistory();
            }
        }

        #region Load Data Methods

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
                        ddlSociete.Items.Insert(0, new ListItem("-- Sélectionner --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des sociétés: " + ex.Message);
            }
        }

        private void LoadSourceTeams()
        {
            ddlEquipeSource.Items.Clear();
            ddlEquipeSource.Items.Insert(0, new ListItem("-- Sélectionner Équipe Source --", ""));

            if (string.IsNullOrEmpty(ddlSociete.SelectedValue)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ce.IdChefEquipe, ce.NomEquipe,
                               e.Nom + ' ' + e.Prenom + ' - ' + ce.NomEquipe AS DisplayText,
                               (SELECT COUNT(*) FROM EMPLOYE_CHEF_EQUIPE ece 
                                WHERE ece.IdChefEquipe = ce.IdChefEquipe 
                                AND ece.is_deleted = 0 
                                AND ece.DateFinAssignation IS NULL) AS NombreEmployes
                        FROM CHEF_EQUIPE ce
                        INNER JOIN EMPLOYE e ON ce.IdEmploye = e.IdEmploye
                        WHERE ce.IdSociete = @IdSociete 
                        AND ce.is_deleted = 0
                        AND EXISTS (
                            SELECT 1 FROM EMPLOYE_CHEF_EQUIPE ece 
                            WHERE ece.IdChefEquipe = ce.IdChefEquipe 
                            AND ece.is_deleted = 0 
                            AND ece.DateFinAssignation IS NULL
                        )
                        ORDER BY ce.NomEquipe";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string displayText = $"{reader["DisplayText"]} ({reader["NombreEmployes"]} membres)";
                                ddlEquipeSource.Items.Add(new ListItem(displayText, reader["IdChefEquipe"].ToString()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des équipes sources: " + ex.Message);
            }
        }

        private void LoadDestinationTeams()
        {
            ddlEquipeDestination.Items.Clear();
            ddlEquipeDestination.Items.Insert(0, new ListItem("-- Sélectionner Équipe Destination --", ""));

            if (string.IsNullOrEmpty(ddlSociete.SelectedValue) || string.IsNullOrEmpty(ddlEquipeSource.SelectedValue)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ce.IdChefEquipe, ce.NomEquipe,
                               e.Nom + ' ' + e.Prenom + ' - ' + ce.NomEquipe AS DisplayText,
                               (SELECT COUNT(*) FROM EMPLOYE_CHEF_EQUIPE ece 
                                WHERE ece.IdChefEquipe = ce.IdChefEquipe 
                                AND ece.is_deleted = 0 
                                AND ece.DateFinAssignation IS NULL) AS NombreEmployes
                        FROM CHEF_EQUIPE ce
                        INNER JOIN EMPLOYE e ON ce.IdEmploye = e.IdEmploye
                        WHERE ce.IdSociete = @IdSociete 
                        AND ce.is_deleted = 0
                        AND ce.IdChefEquipe != @IdEquipeSource
                        ORDER BY ce.NomEquipe";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdSociete", ddlSociete.SelectedValue);
                        cmd.Parameters.AddWithValue("@IdEquipeSource", ddlEquipeSource.SelectedValue);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string displayText = $"{reader["DisplayText"]} ({reader["NombreEmployes"]} membres)";
                                ddlEquipeDestination.Items.Add(new ListItem(displayText, reader["IdChefEquipe"].ToString()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des équipes de destination: " + ex.Message);
            }
        }

        private void LoadSourceTeamMembers()
        {
            if (string.IsNullOrEmpty(ddlEquipeSource.SelectedValue))
            {
                pnlTransfer.Visible = false;
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ece.IdEmployeChefEquipe,
                               ece.IdEmploye,
                               e.Matricule,
                               e.Nom + ' ' + e.Prenom AS NomComplet,
                               ece.DateAssignation,
                               ce.NomEquipe,
                               chef.Nom + ' ' + chef.Prenom AS NomChef
                        FROM EMPLOYE_CHEF_EQUIPE ece
                        INNER JOIN EMPLOYE e ON ece.IdEmploye = e.IdEmploye
                        INNER JOIN CHEF_EQUIPE ce ON ece.IdChefEquipe = ce.IdChefEquipe
                        INNER JOIN EMPLOYE chef ON ce.IdEmploye = chef.IdEmploye
                        WHERE ece.IdChefEquipe = @IdChefEquipe
                        AND ece.is_deleted = 0 
                        AND ece.DateFinAssignation IS NULL
                        ORDER BY e.Nom, e.Prenom";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@IdChefEquipe", ddlEquipeSource.SelectedValue);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            // Load source team info
                            var firstRow = dt.Rows[0];
                            litSourceTeamName.Text = firstRow["NomEquipe"].ToString();
                            litSourceChefName.Text = firstRow["NomChef"].ToString();
                            litSourceMemberCount.Text = dt.Rows.Count.ToString();

                            // Bind grid
                            gvSourceMembers.DataSource = dt;
                            gvSourceMembers.DataBind();

                            pnlTransfer.Visible = true;
                            LoadDestinationTeams();
                        }
                        else
                        {
                            pnlTransfer.Visible = false;
                            ShowError("Cette équipe n'a aucun membre actuel");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement des membres: " + ex.Message);
            }
        }

        private void LoadDestinationTeamInfo()
        {
            if (string.IsNullOrEmpty(ddlEquipeDestination.SelectedValue))
            {
                pnlDestinationInfo.Visible = false;
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ce.NomEquipe,
                               e.Nom + ' ' + e.Prenom AS NomChef,
                               (SELECT COUNT(*) FROM EMPLOYE_CHEF_EQUIPE ece 
                                WHERE ece.IdChefEquipe = ce.IdChefEquipe 
                                AND ece.is_deleted = 0 
                                AND ece.DateFinAssignation IS NULL) AS NombreEmployes
                        FROM CHEF_EQUIPE ce
                        INNER JOIN EMPLOYE e ON ce.IdEmploye = e.IdEmploye
                        WHERE ce.IdChefEquipe = @IdChefEquipe";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdChefEquipe", ddlEquipeDestination.SelectedValue);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                litDestTeamName.Text = reader["NomEquipe"].ToString();
                                litDestChefName.Text = reader["NomChef"].ToString();
                                litDestMemberCount.Text = reader["NombreEmployes"].ToString();
                                pnlDestinationInfo.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur: " + ex.Message);
            }
        }

        private void LoadTransferHistory()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT TOP 20
                               e.Nom + ' ' + e.Prenom AS NomEmploye,
                               e.Matricule AS MatriculeEmploye,
                               ceSource.NomEquipe AS EquipeSource,
                               ceDest.NomEquipe AS EquipeDestination,
                               ece.DateAssignation AS DateTransfert,
                               ece.AssignePar AS TransferePar
                        FROM EMPLOYE_CHEF_EQUIPE ece
                        INNER JOIN EMPLOYE e ON ece.IdEmploye = e.IdEmploye
                        INNER JOIN CHEF_EQUIPE ceDest ON ece.IdChefEquipe = ceDest.IdChefEquipe
                        LEFT JOIN (
                            SELECT ece2.IdEmploye, ce2.NomEquipe, ece2.DateFinAssignation
                            FROM EMPLOYE_CHEF_EQUIPE ece2
                            INNER JOIN CHEF_EQUIPE ce2 ON ece2.IdChefEquipe = ce2.IdChefEquipe
                            WHERE ece2.DateFinAssignation IS NOT NULL
                        ) ceSource ON ece.IdEmploye = ceSource.IdEmploye 
                        AND ceSource.DateFinAssignation = ece.DateAssignation
                        WHERE ece.AssignePar LIKE 'TRANSFERT%'
                        ORDER BY ece.DateCreation DESC";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvTransferHistory.DataSource = dt;
                        gvTransferHistory.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du chargement de l'historique: " + ex.Message);
            }
        }

        #endregion

        #region Event Handlers

        protected void ddlSociete_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSourceTeams();
            pnlTransfer.Visible = false;
        }

        protected void ddlEquipeSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSourceTeamMembers();
        }

        protected void ddlEquipeDestination_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDestinationTeamInfo();
        }

        protected void btnSelectAll_Click(object sender, EventArgs e)
        {
            SetAllCheckboxes(true);
        }

        protected void btnUnselectAll_Click(object sender, EventArgs e)
        {
            SetAllCheckboxes(false);
        }

        protected void btnTransfer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlEquipeDestination.SelectedValue))
            {
                ShowError("Veuillez sélectionner une équipe de destination");
                return;
            }

            if (string.IsNullOrEmpty(txtDateTransfert.Text))
            {
                ShowError("Veuillez saisir la date de transfert");
                return;
            }

            List<TransferInfo> selectedMembers = GetSelectedMembers();
            if (selectedMembers.Count == 0)
            {
                ShowError("Veuillez sélectionner au moins un employé à transférer");
                return;
            }

            try
            {
                int transferredCount = TransferSelectedEmployees(selectedMembers);
                ShowSuccess($"{transferredCount} employé(s) transféré(s) avec succès!");

                // Refresh data
                LoadSourceTeamMembers();
                LoadDestinationTeamInfo();
                LoadTransferHistory();
            }
            catch (Exception ex)
            {
                ShowError("Erreur lors du transfert: " + ex.Message);
            }
        }

        protected void btnRefreshHistory_Click(object sender, EventArgs e)
        {
            LoadTransferHistory();
        }

        #endregion

        #region Helper Methods

        private void SetAllCheckboxes(bool isChecked)
        {
            foreach (GridViewRow row in gvSourceMembers.Rows)
            {
                CheckBox chkSelect = row.FindControl("chkSelect") as CheckBox;
                if (chkSelect != null)
                {
                    chkSelect.Checked = isChecked;
                }
            }
        }

        private List<TransferInfo> GetSelectedMembers()
        {
            List<TransferInfo> selectedMembers = new List<TransferInfo>();

            foreach (GridViewRow row in gvSourceMembers.Rows)
            {
                CheckBox chkSelect = row.FindControl("chkSelect") as CheckBox;
                HiddenField hfIdEmploye = row.FindControl("hfIdEmploye") as HiddenField;
                HiddenField hfIdAssignation = row.FindControl("hfIdAssignation") as HiddenField;

                if (chkSelect != null && hfIdEmploye != null && hfIdAssignation != null && chkSelect.Checked)
                {
                    selectedMembers.Add(new TransferInfo
                    {
                        IdEmploye = Convert.ToInt32(hfIdEmploye.Value),
                        IdAssignationActuelle = Convert.ToInt32(hfIdAssignation.Value)
                    });
                }
            }

            return selectedMembers;
        }

        private int TransferSelectedEmployees(List<TransferInfo> transferList)
        {
            int transferredCount = 0;
            DateTime transferDate = Convert.ToDateTime(txtDateTransfert.Text);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var transfer in transferList)
                        {
                            // 1. End current assignment
                            string endQuery = @"
                                UPDATE EMPLOYE_CHEF_EQUIPE 
                                SET DateFinAssignation = @DateTransfert 
                                WHERE IdEmployeChefEquipe = @IdAssignation";

                            using (SqlCommand endCmd = new SqlCommand(endQuery, conn, transaction))
                            {
                                endCmd.Parameters.AddWithValue("@IdAssignation", transfer.IdAssignationActuelle);
                                endCmd.Parameters.AddWithValue("@DateTransfert", transferDate);
                                endCmd.ExecuteNonQuery();
                            }

                            // 2. Create new assignment
                            string newQuery = @"
                                INSERT INTO EMPLOYE_CHEF_EQUIPE (IdEmploye, IdChefEquipe, AssignePar, DateAssignation, DateCreation) 
                                VALUES (@IdEmploye, @IdChefEquipe, @AssignePar, @DateAssignation, @DateCreation)";

                            using (SqlCommand newCmd = new SqlCommand(newQuery, conn, transaction))
                            {
                                newCmd.Parameters.AddWithValue("@IdEmploye", transfer.IdEmploye);
                                newCmd.Parameters.AddWithValue("@IdChefEquipe", ddlEquipeDestination.SelectedValue);
                                newCmd.Parameters.AddWithValue("@AssignePar", "TRANSFERT - " + GetCurrentUserId());
                                newCmd.Parameters.AddWithValue("@DateAssignation", transferDate);
                                newCmd.Parameters.AddWithValue("@DateCreation", DateTime.Now);

                                newCmd.ExecuteNonQuery();
                                transferredCount++;
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Erreur lors du transfert: {ex.Message}");
                    }
                }
            }

            return transferredCount;
        }

        private int GetCurrentUserId()
        {
            return Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
        }

        #endregion

        #region Message Methods

        private void ShowSuccess(string message)
        {
            // Escape single quotes for JavaScript
            string escapedMessage = message.Replace("'", "\\'");
            ClientScript.RegisterStartupScript(this.GetType(), "Success",
                $"Swal.fire({{ toast: true, position: 'top-end', icon: 'success', title: '{escapedMessage}', showConfirmButton: false, timer: 4000 }});", true);
        }

        private void ShowError(string message)
        {
            // Escape single quotes for JavaScript
            string escapedMessage = message.Replace("'", "\\'");
            ClientScript.RegisterStartupScript(this.GetType(), "Error",
                $"Swal.fire({{ toast: true, position: 'top-end', icon: 'error', title: '{escapedMessage}', showConfirmButton: false, timer: 4000 }});", true);
        }

        #endregion

        #region Helper Classes

        public class TransferInfo
        {
            public int IdEmploye { get; set; }
            public int IdAssignationActuelle { get; set; }
        }

        #endregion
    }
}