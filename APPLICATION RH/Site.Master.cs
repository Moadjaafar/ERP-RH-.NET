using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH
{
    public partial class SiteMaster : MasterPage
    {
        private string connectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Initialize connection string
                InitializeConnectionString();

                // Check if user is authenticated
                if (Session["EmployeId"] == null)
                {
                    Response.Redirect("~/connexion.aspx");
                    return;
                }

                if (!IsPostBack)
                {
                    // Set user name in navigation
                    SetUserName();

                    // Load and apply user permissions
                    ApplyUserPermissions();

                    //string rolle = GetUserRole();
                    //if (rolle == "Pointage Porte")
                    //{
                    //    Response.Redirect("~/pointagfix/PointageEmployesPorts.aspx");

                    //}
                }


            }
            catch (Exception ex)
            {
                // Log the error for debugging
                LogError("Page_Load", ex);

                // Show a user-friendly message or redirect
                // For debugging, you might want to show the error
                Response.Write($"<script>alert('Error: {ex.Message}');</script>");
            }
        }

        private void InitializeConnectionString()
        {
            try
            {
                // Try different possible connection string names
                connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;

                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"]?.ConnectionString;
                }

                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = ConfigurationManager.ConnectionStrings["ApplicationRHConnectionString"]?.ConnectionString;
                }

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("No valid connection string found. Please check web.config.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Connection string initialization failed: {ex.Message}");
            }
        }

        private void SetUserName()
        {
            try
            {
                if (litUserName != null)
                {
                    string nom = Session["EmployeNom"]?.ToString() ?? "";
                    string prenom = Session["EmployePrenom"]?.ToString() ?? "";
                    string fullName = $"{prenom} {nom}".Trim();

                    litUserName.Text = string.IsNullOrEmpty(fullName) ? "Utilisateur" : fullName;
                }
            }
            catch (Exception ex)
            {
                LogError("SetUserName", ex);
            }
        }

        private void ApplyUserPermissions()
        {
            try
            {
                // Get user's role from database
                string userRole = GetUserRole();

                if (string.IsNullOrEmpty(userRole))
                {
                    // If no role found, apply default behavior
                    ApplyDefaultPermissions();
                    return;
                }

                // Get user's accessible pages
                List<string> accessiblePages = GetUserAccessiblePages(userRole);

                // Apply permissions to menu items
                SetMenuVisibility(accessiblePages);
            }
            catch (Exception ex)
            {
                LogError("ApplyUserPermissions", ex);
                // On error, apply safe default permissions
                ApplyDefaultPermissions();
            }
        }

        private void ApplyDefaultPermissions()
        {
            try
            {
                // Hide all menu items by default for security
                HideAllMenuItems();

                // Show only basic dashboard access
                if (HyperLink1 != null)
                {
                    HyperLink1.Visible = true;
                }
            }
            catch (Exception ex)
            {
                LogError("ApplyDefaultPermissions", ex);
            }
        }

        private string GetUserRole()
        {
            string role = null;

            try
            {
                string employeId = Session["EmployeId"]?.ToString();

                if (string.IsNullOrEmpty(employeId))
                {
                    LogError("GetUserRole", new Exception("EmployeId is null or empty in session"));
                    return null;
                }

                string query = @"SELECT Rolle FROM EMPLOYE WHERE IdEmploye = @EmployeId AND is_deleted = 0";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeId", employeId);
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        role = result?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("GetUserRole", ex);
            }

            return role;
        }

        private List<string> GetUserAccessiblePages(string roleName)
        {
            List<string> accessiblePages = new List<string>();

            try
            {
                if (string.IsNullOrEmpty(roleName))
                    return accessiblePages;

                string query = @"
                    SELECT p.PageURL 
                    FROM ROLES r
                    INNER JOIN ROLE_PERMISSIONS rp ON r.IdRole = rp.IdRole
                    INNER JOIN PAGES p ON rp.IdPage = p.IdPage
                    WHERE r.RoleName = @RoleName 
                    AND r.IsActive = 1 
                    AND p.IsActive = 1 
                    AND rp.HasAccess = 1";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoleName", roleName);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string pageUrl = reader["PageURL"]?.ToString();
                                if (!string.IsNullOrEmpty(pageUrl))
                                {
                                    accessiblePages.Add(pageUrl);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("GetUserAccessiblePages", ex);
            }

            return accessiblePages;
        }

        private void SetMenuVisibility(List<string> accessiblePages)
        {
            try
            {
                // First, hide all menu items
                HideAllMenuItems();

                if (accessiblePages == null || accessiblePages.Count == 0)
                    return;

                // Dictionary to map page URLs to their corresponding menu controls
                var menuMapping = GetMenuMapping();

                // Track which sections have visible items
                bool hasDashboardItems = false;
                bool hasPersonnelItems = false;
                bool hasPointageItems = false;
                bool hasPointageFixItems = false;
                bool hasPermissionsItems = false;

                // Show controls for accessible pages
                foreach (string pageUrl in accessiblePages)
                {
                    if (menuMapping.ContainsKey(pageUrl))
                    {
                        foreach (Control control in menuMapping[pageUrl])
                        {
                            if (control != null)
                            {
                                control.Visible = true;
                            }
                        }

                        // Determine which sections have visible items
                        if (pageUrl.Contains("/Default.aspx") || pageUrl.Contains("/reporting/"))
                            hasDashboardItems = true;
                        else if (pageUrl.Contains("/personnel/"))
                            hasPersonnelItems = true;
                        else if (pageUrl.Contains("/pointage/"))
                            hasPointageItems = true;
                        else if (pageUrl.Contains("/pointagfix/"))
                            hasPointageFixItems = true;
                        else if (pageUrl.Contains("/permissions/"))
                            hasPermissionsItems = true;
                    }
                }

                // Show/hide section titles and menus based on whether they have visible items
                ShowSectionsWithItems(hasPersonnelItems, hasPointageItems, hasPointageFixItems, hasPermissionsItems);
            }
            catch (Exception ex)
            {
                LogError("SetMenuVisibility", ex);
            }
        }

        private Dictionary<string, Control[]> GetMenuMapping()
        {
            var menuMapping = new Dictionary<string, Control[]>();

            try
            {
                // Only add controls that are not null
                AddToMapping(menuMapping, "~/Default.aspx", HyperLink1);
                AddToMapping(menuMapping, "~/reporting/RaportPointagePort.aspx", HyperLink6);

                // Personnel items
                AddToMapping(menuMapping, "~/personnel/AjouterEmploye.aspx", hlAjouterEmploye);
                AddToMapping(menuMapping, "~/personnel/ListeEmployes.aspx", hlListeEmployes);

                // Pointage items
                AddToMapping(menuMapping, "~/pointage/AssignerEmployes.aspx", hlAssignerEmployes);
                AddToMapping(menuMapping, "~/pointage/TransfertAssignation.aspx", hlMonPointage);
                AddToMapping(menuMapping, "~/pointage/MonEquipe.aspx", hlMonEquipe);
                AddToMapping(menuMapping, "~/pointage/PointageProduction.aspx", HyperLink5);
                AddToMapping(menuMapping, "~/pointage/PointageEquipe.aspx", hlPointageEquipe);
                AddToMapping(menuMapping, "~/pointage/SupervisionPointage.aspx", HyperLink2);
                AddToMapping(menuMapping, "~/pointage/PointageHeuresSupp.aspx", hlRapportsPointage);

                // Pointage Fix items
                AddToMapping(menuMapping, "~/pointagfix/AssignerEmployesPorts.aspx", HyperLink3);
                AddToMapping(menuMapping, "~/pointagfix/PointageEmployesPorts.aspx", HyperLink4);
                AddToMapping(menuMapping, "~/pointagfix/EditPointagePort.aspx", EditPointagePort);

                // Permissions items
                AddToMapping(menuMapping, "~/permissions/RollesManagment.aspx", hlRolesManagement);
                AddToMapping(menuMapping, "~/permissions/PagesManagment.aspx", hlPagesManagement);
                AddToMapping(menuMapping, "~/permissions/RollePage.aspx", hlRolePagePermissions);
                AddToMapping(menuMapping, "~/permissions/RolleEmployer.aspx", hlEmployeeRoles);
                AddToMapping(menuMapping, "~/permissions/GestionMotsDePasse.aspx", hlGestionPassword);
            }
            catch (Exception ex)
            {
                LogError("GetMenuMapping", ex);
            }

            return menuMapping;
        }

        private void AddToMapping(Dictionary<string, Control[]> mapping, string url, params Control[] controls)
        {
            try
            {
                var validControls = controls.Where(c => c != null).ToArray();
                if (validControls.Length > 0)
                {
                    mapping[url] = validControls;
                }
            }
            catch (Exception ex)
            {
                LogError("AddToMapping", ex);
            }
        }

        private void ShowSectionsWithItems(bool hasPersonnelItems, bool hasPointageItems, bool hasPointageFixItems, bool hasPermissionsItems)
        {
            try
            {
                // Personnel section
                if (hasPersonnelItems)
                {
                    SetControlVisibility(liPersonnelTitle, true);
                    SetControlVisibility(liPersonnelMenu, true);
                }

                // Pointage section
                if (hasPointageItems)
                {
                    SetControlVisibility(liPointageTitle, true);
                    SetControlVisibility(liPointageMenu, true);
                }

                // Pointage Fix section
                if (hasPointageFixItems)
                {
                    SetControlVisibility(liPointageFixTitle, true);
                    SetControlVisibility(liPointageFixMenu, true);
                }

                // Permissions section
                if (hasPermissionsItems)
                {
                    SetControlVisibility(liPermissionsTitle, true);
                    SetControlVisibility(liPermissionsMenu, true);
                }
            }
            catch (Exception ex)
            {
                LogError("ShowSectionsWithItems", ex);
            }
        }

        private void SetControlVisibility(Control control, bool visible)
        {
            try
            {
                if (control != null)
                {
                    control.Visible = visible;
                }
            }
            catch (Exception ex)
            {
                LogError("SetControlVisibility", ex);
            }
        }

        private void HideAllMenuItems()
        {
            try
            {
                // Hide all section titles
                SetControlVisibility(liPersonnelTitle, false);
                SetControlVisibility(liPointageTitle, false);
                SetControlVisibility(liPointageFixTitle, false);
                SetControlVisibility(liPermissionsTitle, false);

                // Hide all section menus
                SetControlVisibility(liPersonnelMenu, false);
                SetControlVisibility(liPointageMenu, false);
                SetControlVisibility(liPointageFixMenu, false);
                SetControlVisibility(liPermissionsMenu, false);

                // Hide all individual menu items
                // Dashboard
                SetControlVisibility(HyperLink1, false);
                SetControlVisibility(HyperLink6, false);

                // Personnel
                SetControlVisibility(hlAjouterEmploye, false);
                SetControlVisibility(hlListeEmployes, false);

                // Pointage
                SetControlVisibility(hlAssignerEmployes, false);
                SetControlVisibility(hlMonPointage, false);
                SetControlVisibility(hlMonEquipe, false);
                SetControlVisibility(HyperLink5, false);
                SetControlVisibility(hlPointageEquipe, false);
                SetControlVisibility(HyperLink2, false);
                SetControlVisibility(hlRapportsPointage, false);

                // Pointage Fix
                SetControlVisibility(HyperLink3, false);
                SetControlVisibility(HyperLink4, false);
                SetControlVisibility(EditPointagePort, false);

                // Permissions
                SetControlVisibility(hlRolesManagement, false);
                SetControlVisibility(hlPagesManagement, false);
                SetControlVisibility(hlRolePagePermissions, false);
                SetControlVisibility(hlEmployeeRoles, false);
                SetControlVisibility(hlGestionPassword, false);
            }
            catch (Exception ex)
            {
                LogError("HideAllMenuItems", ex);
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                // Log logout activity
                if (Session["EmployeId"] != null)
                {
                    LogUserActivity("Logout", Session["EmployeId"].ToString());
                }

                // Clear session
                Session.Clear();
                Session.Abandon();

                // Clear remember me cookie if it exists
                if (Request.Cookies["RH_User"] != null)
                {
                    var cookie = new System.Web.HttpCookie("RH_User");
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(cookie);
                }

                // Redirect to login page with logout message
                Response.Redirect("~/connexion.aspx?logout=1");
            }
            catch (Exception ex)
            {
                LogError("btnLogout_Click", ex);
                // Still redirect even if there's an error
                Response.Redirect("~/connexion.aspx?logout=1");
            }
        }

        private void LogUserActivity(string action, string employeId)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(employeId))
                    return;

                // Check if USER_LOGS table exists first
                string checkTableQuery = @"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = 'USER_LOGS'";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand checkCmd = new SqlCommand(checkTableQuery, conn))
                    {
                        int tableExists = (int)checkCmd.ExecuteScalar();
                        if (tableExists == 0)
                            return; // Table doesn't exist, skip logging
                    }

                    string query = @"
                        INSERT INTO USER_LOGS (EmployeId, Action, ActionDate, IPAddress) 
                        VALUES (@EmployeId, @Action, @ActionDate, @IPAddress)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeId", employeId);
                        cmd.Parameters.AddWithValue("@Action", action);
                        cmd.Parameters.AddWithValue("@ActionDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@IPAddress", GetUserIPAddress());
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Silent fail for logging
            }
        }

        private string GetUserIPAddress()
        {
            try
            {
                string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ipAddress))
                    ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                return ipAddress ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private void LogError(string method, Exception ex)
        {
            try
            {
                // For debugging - you can write to a log file or event log
                string errorMessage = $"Error in {method}: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }

                // For now, just trace it (you can see this in debug output)
                System.Diagnostics.Trace.WriteLine($"[{DateTime.Now}] {errorMessage}");

                // You could also log to a file:
                // System.IO.File.AppendAllText(Server.MapPath("~/App_Data/errors.log"), 
                //     $"[{DateTime.Now}] {errorMessage}\r\n");
            }
            catch
            {
                // Silent fail for error logging
            }
        }
    }
}