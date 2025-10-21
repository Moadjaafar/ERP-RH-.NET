using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace APPLICATION_RH
{
    public class MenuHelper
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        public class MenuPage
        {
            public int IdPage { get; set; }
            public string PageName { get; set; }
            public string PageURL { get; set; }
            public string MenuGroup { get; set; }
            public bool IsActive { get; set; }
        }

        public class MenuGroup
        {
            public string GroupName { get; set; }
            public string GroupIcon { get; set; }
            public List<MenuPage> Pages { get; set; }

            public MenuGroup()
            {
                Pages = new List<MenuPage>();
            }
        }

        /// <summary>
        /// Get all pages that a user has access to based on their role
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>List of accessible pages grouped by menu</returns>
        public static List<MenuGroup> GetUserMenus(int employeeId)
        {
            var menuGroups = new List<MenuGroup>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Query to get all pages the user has access to through their role
                    string query = @"
                        SELECT DISTINCT 
                            p.IdPage,
                            p.PageName,
                            p.PageURL,
                            p.MenuGroup,
                            p.IsActive
                        FROM PAGES p
                        INNER JOIN ROLE_PERMISSIONS rp ON p.IdPage = rp.IdPage
                        INNER JOIN ROLES r ON rp.IdRole = r.IdRole
                        INNER JOIN EMPLOYEES e ON e.IdRole = r.IdRole
                        WHERE e.IdEmploye = @EmployeeId
                        AND p.IsActive = 1
                        AND r.IsActive = 1
                        AND rp.HasAccess = 1
                        AND (e.is_deleted IS NULL OR e.is_deleted = 0)
                        ORDER BY p.MenuGroup, p.PageName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var pagesDict = new Dictionary<string, MenuGroup>();

                            while (reader.Read())
                            {
                                var page = new MenuPage
                                {
                                    IdPage = Convert.ToInt32(reader["IdPage"]),
                                    PageName = reader["PageName"].ToString(),
                                    PageURL = reader["PageURL"].ToString(),
                                    MenuGroup = reader["MenuGroup"].ToString(),
                                    IsActive = Convert.ToBoolean(reader["IsActive"])
                                };

                                // Group pages by MenuGroup
                                if (!pagesDict.ContainsKey(page.MenuGroup))
                                {
                                    pagesDict[page.MenuGroup] = new MenuGroup
                                    {
                                        GroupName = page.MenuGroup,
                                        GroupIcon = GetMenuGroupIcon(page.MenuGroup)
                                    };
                                }

                                pagesDict[page.MenuGroup].Pages.Add(page);
                            }

                            // Convert dictionary to list and sort by group priority
                            menuGroups = pagesDict.Values
                                .OrderBy(g => GetMenuGroupPriority(g.GroupName))
                                .ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error (you can implement logging as needed)
                System.Diagnostics.Debug.WriteLine($"Error loading user menus: {ex.Message}");
            }

            return menuGroups;
        }

        /// <summary>
        /// Check if a user has access to a specific page
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="pageUrl">Page URL to check</param>
        /// <returns>True if user has access</returns>
        public static bool HasPageAccess(int employeeId, string pageUrl)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT COUNT(*) 
                        FROM PAGES p
                        INNER JOIN ROLE_PERMISSIONS rp ON p.IdPage = rp.IdPage
                        INNER JOIN ROLES r ON rp.IdRole = r.IdRole
                        INNER JOIN EMPLOYEES e ON e.IdRole = r.IdRole
                        WHERE e.IdEmploye = @EmployeeId
                        AND p.PageURL = @PageUrl
                        AND p.IsActive = 1
                        AND r.IsActive = 1
                        AND rp.HasAccess = 1
                        AND (e.is_deleted IS NULL OR e.is_deleted = 0)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                        cmd.Parameters.AddWithValue("@PageUrl", pageUrl);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking page access: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get user role information
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Role name or empty string</returns>
        public static string GetUserRole(int employeeId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT r.RoleName 
                        FROM ROLES r
                        INNER JOIN EMPLOYEES e ON e.IdRole = r.IdRole
                        WHERE e.IdEmploye = @EmployeeId
                        AND r.IsActive = 1
                        AND (e.is_deleted IS NULL OR e.is_deleted = 0)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                        
                        object result = cmd.ExecuteScalar();
                        return result?.ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting user role: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// Get icon for menu group
        /// </summary>
        private static string GetMenuGroupIcon(string groupName)
        {
            switch (groupName?.ToLower())
            {
                case "dashboard":
                    return "home";
                case "personnel":
                    return "users";
                case "pointage":
                    return "clock";
                case "pointage fix":
                    return "clock";
                case "permissions":
                    return "shield";
                case "rapports":
                    return "bar-chart";
                case "administration":
                    return "settings";
                default:
                    return "folder";
            }
        }

        /// <summary>
        /// Get priority order for menu groups
        /// </summary>
        private static int GetMenuGroupPriority(string groupName)
        {
            switch (groupName?.ToLower())
            {
                case "dashboard":
                    return 1;
                case "personnel":
                    return 2;
                case "pointage":
                    return 3;
                case "pointage fix":
                    return 4;
                case "rapports":
                    return 5;
                case "permissions":
                    return 6;
                case "administration":
                    return 7;
                default:
                    return 99;
            }
        }
    }
}