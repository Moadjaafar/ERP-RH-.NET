using System;
using System.Web.UI;

namespace APPLICATION_RH
{
    /// <summary>
    /// Base page class that provides role-based access control
    /// All pages should inherit from this class for automatic permission checking
    /// </summary>
    public class BasePage : System.Web.UI.Page
    {
        protected override void OnPreInit(EventArgs e)
        {
            // Check if user is authenticated
            if (Session["EmployeId"] == null)
            {
                Response.Redirect("~/connexion.aspx");
                return;
            }

            // Check page-specific permissions (only for non-public pages)
            if (!IsPublicPage())
            {
                CheckPageAccess();
            }
            
            base.OnPreInit(e);
        }

        private bool IsPublicPage()
        {
            string currentPage = Request.AppRelativeCurrentExecutionFilePath.ToLower();
            
            // Define public pages that don't require permission checking
            string[] publicPages = {
                "~/connexion.aspx",
                "~/default.aspx"
            };

            foreach (string publicPage in publicPages)
            {
                if (currentPage.Equals(publicPage, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void CheckPageAccess()
        {
            try
            {
                string currentPage = Request.AppRelativeCurrentExecutionFilePath;
                int employeeId = UserId;

                if (employeeId > 0)
                {
                    // Use MenuHelper to check page access
                    if (!MenuHelper.HasPageAccess(employeeId, currentPage))
                    {
                        // User doesn't have access to this page
                        Session["AccessDeniedMessage"] = $"Vous n'avez pas l'autorisation d'accéder à cette page: {currentPage}";
                        Response.Redirect("~/Default.aspx?access=denied");
                        return;
                    }
                }
                else
                {
                    // Invalid user ID, redirect to login
                    Response.Redirect("~/connexion.aspx");
                }
            }
            catch (Exception ex)
            {
                // Log error and redirect to login for safety
                System.Diagnostics.Debug.WriteLine($"Error checking page access: {ex.Message}");
                Response.Redirect("~/connexion.aspx");
            }
        }

        /// <summary>
        /// Helper method to check if current user has specific permission
        /// </summary>
        /// <param name="pageUrl">Page URL to check (e.g., "~/personnel/ListeEmployes.aspx")</param>
        /// <returns>True if user has access, false otherwise</returns>
        protected bool HasPermission(string pageUrl)
        {
            try
            {
                int employeeId = UserId;
                if (employeeId > 0)
                {
                    return MenuHelper.HasPageAccess(employeeId, pageUrl);
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking permission: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get current user's role
        /// </summary>
        protected string UserRole
        {
            get { return Session["EmployeRole"]?.ToString() ?? ""; }
        }

        /// <summary>
        /// Get current user's ID
        /// </summary>
        protected int UserId
        {
            get 
            { 
                if (int.TryParse(Session["EmployeId"]?.ToString(), out int id))
                    return id;
                return 0;
            }
        }

        /// <summary>
        /// Get current user's name
        /// </summary>
        protected string UserName
        {
            get 
            { 
                string nom = Session["EmployeNom"]?.ToString() ?? "";
                string prenom = Session["EmployePrenom"]?.ToString() ?? "";
                return $"{prenom} {nom}".Trim();
            }
        }
    }
}