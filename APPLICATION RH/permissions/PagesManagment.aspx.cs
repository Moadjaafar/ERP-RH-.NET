using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace APPLICATION_RH.permissions
{
    public partial class PagesManagment : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadPages();
            }
        }

        #region Data Loading Methods

        private void LoadPages()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT IdPage, PageName, PageURL, MenuGroup, IsActive 
                        FROM PAGES 
                        ORDER BY MenuGroup, PageName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            gvPages.DataSource = dt;
                            gvPages.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors du chargement des pages: {ex.Message}");
            }
        }

        #endregion

        #region Add Page Methods

        protected void btnAddPage_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        // Check if page URL already exists
                        string checkQuery = "SELECT COUNT(1) FROM PAGES WHERE PageURL = @PageURL";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@PageURL", txtPageURL.Text.Trim());
                            conn.Open();

                            if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                            {
                                ShowWarning("Cette URL de page existe déjà.");
                                return;
                            }
                        }

                        // Insert new page
                        string insertQuery = @"
                            INSERT INTO PAGES (PageName, PageURL, MenuGroup, IsActive) 
                            VALUES (@PageName, @PageURL, @MenuGroup, @IsActive)";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@PageName", txtPageName.Text.Trim());
                            cmd.Parameters.AddWithValue("@PageURL", txtPageURL.Text.Trim());
                            cmd.Parameters.AddWithValue("@MenuGroup", string.IsNullOrWhiteSpace(ddlMenuGroup.SelectedValue) ? DBNull.Value : (object)ddlMenuGroup.SelectedValue);
                            cmd.Parameters.AddWithValue("@IsActive", chkIsActive.Checked);

                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                ShowSuccess("Page ajoutée avec succès!");
                                ClearForm();
                                LoadPages();
                            }
                            else
                            {
                                ShowError("Erreur lors de l'ajout de la page.");
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
            txtPageName.Text = "";
            txtPageURL.Text = "";
            ddlMenuGroup.SelectedIndex = 0;
            chkIsActive.Checked = true;
        }

        #endregion

        #region GridView Event Handlers

        protected void gvPages_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvPages.EditIndex = e.NewEditIndex;
            LoadPages();
        }

        protected void gvPages_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvPages.EditIndex = -1;
            LoadPages();
        }

        protected void gvPages_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                GridViewRow row = gvPages.Rows[e.RowIndex];
                int idPage = Convert.ToInt32(gvPages.DataKeys[e.RowIndex].Value);

                TextBox txtEditPageName = (TextBox)row.FindControl("txtEditPageName");
                TextBox txtEditPageURL = (TextBox)row.FindControl("txtEditPageURL");
                DropDownList ddlEditMenuGroup = (DropDownList)row.FindControl("ddlEditMenuGroup");
                CheckBox chkEditIsActive = (CheckBox)row.FindControl("chkEditIsActive");

                if (string.IsNullOrWhiteSpace(txtEditPageName.Text))
                {
                    ShowWarning("Le nom de la page ne peut pas être vide.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEditPageURL.Text))
                {
                    ShowWarning("L'URL de la page ne peut pas être vide.");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Check if page URL already exists (excluding current page)
                    string checkQuery = "SELECT COUNT(1) FROM PAGES WHERE PageURL = @PageURL AND IdPage != @IdPage";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@PageURL", txtEditPageURL.Text.Trim());
                        checkCmd.Parameters.AddWithValue("@IdPage", idPage);
                        conn.Open();

                        if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                        {
                            ShowWarning("Cette URL de page existe déjà.");
                            return;
                        }
                    }

                    // Update page
                    string updateQuery = @"
                        UPDATE PAGES 
                        SET PageName = @PageName, 
                            PageURL = @PageURL, 
                            MenuGroup = @MenuGroup, 
                            IsActive = @IsActive 
                        WHERE IdPage = @IdPage";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PageName", txtEditPageName.Text.Trim());
                        cmd.Parameters.AddWithValue("@PageURL", txtEditPageURL.Text.Trim());
                        cmd.Parameters.AddWithValue("@MenuGroup", string.IsNullOrWhiteSpace(ddlEditMenuGroup.SelectedValue) ? DBNull.Value : (object)ddlEditMenuGroup.SelectedValue);
                        cmd.Parameters.AddWithValue("@IsActive", chkEditIsActive.Checked);
                        cmd.Parameters.AddWithValue("@IdPage", idPage);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowSuccess("Page modifiée avec succès!");
                            gvPages.EditIndex = -1;
                            LoadPages();
                        }
                        else
                        {
                            ShowError("Erreur lors de la modification de la page.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur: {ex.Message}");
            }
        }

        protected void gvPages_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int idPage = Convert.ToInt32(gvPages.DataKeys[e.RowIndex].Value);

                // Debug: Add logging
                System.Diagnostics.Debug.WriteLine($"Attempting to delete page ID: {idPage}");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Check if page has associated permissions that might indicate it's in use
                    string checkQuery = "SELECT COUNT(1) FROM ROLE_PERMISSIONS WHERE IdPage = @IdPage";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@IdPage", idPage);
                        int permissionCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        System.Diagnostics.Debug.WriteLine($"Permissions for this page: {permissionCount}");

                        if (permissionCount > 0)
                        {
                            ShowWarning($"Impossible de supprimer cette page. Elle a {permissionCount} permission(s) assignée(s). Supprimez d'abord les permissions associées.");
                            return;
                        }
                    }

                    // Delete page
                    string deleteQuery = "DELETE FROM PAGES WHERE IdPage = @IdPage";
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdPage", idPage);
                        int result = cmd.ExecuteNonQuery();

                        System.Diagnostics.Debug.WriteLine($"Pages deleted: {result}");

                        if (result > 0)
                        {
                            ShowSuccess("Page supprimée avec succès!");
                            LoadPages();
                        }
                        else
                        {
                            ShowError("Aucune page trouvée à supprimer.");
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
            LoadPages();
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

        #region Page Discovery Methods

        protected void btnDiscoverPages_Click(object sender, EventArgs e)
        {
            try
            {
                var discoveredPages = DiscoverAspxPages();
                PopulateDiscoveredPagesDropdown(discoveredPages);
                ShowSuccess($"Découvert {discoveredPages.Count} page(s) dans le projet!");
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors de la découverte des pages: {ex.Message}");
            }
        }

        private List<DiscoveredPage> DiscoverAspxPages()
        {
            var pages = new List<DiscoveredPage>();
            
            // Get the physical path of the application root
            string appPath = Server.MapPath("~/");
            
            // Directories to scan
            string[] directoriesToScan = {
                "personnel", "pointage", "pointagfix", "permissions", 
                "reporting", "admin", "Default.aspx" // Include root level pages
            };

            // Scan each directory
            foreach (var directory in directoriesToScan)
            {
                string fullPath = Path.Combine(appPath, directory);
                
                if (directory == "Default.aspx") // Handle root level files
                {
                    if (File.Exists(Path.Combine(appPath, "Default.aspx")))
                    {
                        pages.Add(new DiscoveredPage
                        {
                            PageName = "Dashboard",
                            PageURL = "~/Default.aspx",
                            MenuGroup = "Dashboard",
                            DirectoryName = ""
                        });
                    }
                    continue;
                }
                
                if (Directory.Exists(fullPath))
                {
                    ScanDirectory(fullPath, directory, pages);
                }
            }

            // Sort pages by menu group and name
            return pages.OrderBy(p => p.MenuGroup).ThenBy(p => p.PageName).ToList();
        }

        private void ScanDirectory(string directoryPath, string relativePath, List<DiscoveredPage> pages)
        {
            try
            {
                // Get all .aspx files in the directory
                string[] aspxFiles = Directory.GetFiles(directoryPath, "*.aspx", SearchOption.AllDirectories);

                foreach (string file in aspxFiles)
                {
                    // Skip master pages and user controls
                    if (file.EndsWith(".Master", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".ascx", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Get relative path from application root
                    string appPath = Server.MapPath("~/");
                    string relativeUrl = "~/" + file.Substring(appPath.Length).Replace('\\', '/');
                    
                    // Extract page name from file name
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string pageName = FormatPageName(fileName);
                    
                    // Determine menu group based on directory
                    string menuGroup = DetermineMenuGroup(relativePath);
                    
                    pages.Add(new DiscoveredPage
                    {
                        PageName = pageName,
                        PageURL = relativeUrl,
                        MenuGroup = menuGroup,
                        DirectoryName = relativePath
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scanning directory {directoryPath}: {ex.Message}");
            }
        }

        private string FormatPageName(string fileName)
        {
            // Convert file names to readable page names
            switch (fileName.ToLower())
            {
                case "default": return "Dashboard";
                case "listeemployes": return "Liste des Employés";
                case "ajouteremploye": return "Ajouter Employé";
                case "editemployee": return "Modifier Employé";
                case "rollesmanagment": return "Gestion des Rôles";
                case "pagesmanagment": return "Gestion des Pages";
                case "pointageequipe": return "Pointage d'Équipe";
                case "pointageproduction": return "Pointage Production";
                case "pointageemployesports": return "Pointage aux Ports";
                case "supervisionpointage": return "Supervision Pointage";
                case "assigneremployes": return "Assigner Employés";
                case "creerchefequipe": return "Créer Chef d'Équipe";
                default:
                    // Convert CamelCase/PascalCase to readable format
                    string result = fileName;
                    result = System.Text.RegularExpressions.Regex.Replace(result, "([a-z])([A-Z])", "$1 $2");
                    return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result.ToLower());
            }
        }

        private string DetermineMenuGroup(string directoryName)
        {
            switch (directoryName.ToLower())
            {
                case "personnel": return "Personnel";
                case "pointage": return "Pointage";
                case "pointagfix": return "Pointage";
                case "permissions": return "Permissions";
                case "reporting": return "Rapports";
                case "admin": return "Administration";
                default: return "Autres";
            }
        }

        private void PopulateDiscoveredPagesDropdown(List<DiscoveredPage> pages)
        {
            ddlDiscoveredPages.Items.Clear();
            ddlDiscoveredPages.Items.Add(new ListItem("-- Sélectionner une page découverte --", ""));

            foreach (var page in pages)
            {
                string displayText = $"[{page.MenuGroup}] {page.PageName}";
                string value = $"{page.PageName}|{page.PageURL}|{page.MenuGroup}";
                ddlDiscoveredPages.Items.Add(new ListItem(displayText, value));
            }
        }

        protected void btnUseDiscoveredPage_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ddlDiscoveredPages.SelectedValue))
                {
                    ShowWarning("Veuillez sélectionner une page de la liste.");
                    return;
                }

                string[] parts = ddlDiscoveredPages.SelectedValue.Split('|');
                if (parts.Length == 3)
                {
                    txtPageName.Text = parts[0];
                    txtPageURL.Text = parts[1];
                    
                    // Set menu group if it exists in dropdown
                    ListItem menuItem = ddlMenuGroup.Items.FindByValue(parts[2]);
                    if (menuItem != null)
                    {
                        ddlMenuGroup.SelectedValue = parts[2];
                    }
                    
                    ShowInfo("Informations de la page copiées dans le formulaire!");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors de l'utilisation de la page sélectionnée: {ex.Message}");
            }
        }

        #endregion
    }

    // Helper class for discovered pages
    public class DiscoveredPage
    {
        public string PageName { get; set; }
        public string PageURL { get; set; }
        public string MenuGroup { get; set; }
        public string DirectoryName { get; set; }
    }
}