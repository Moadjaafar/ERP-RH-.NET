using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace APPLICATION_RH
{
    public partial class connexion : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            //// Check if user is already logged in
            if (Session["EmployeId"] != null)
            {
                Response.Redirect("~/Default.aspx"); // Redirect to main page
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Validate the page before processing
            string loginInput = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();
            bool rememberMe = chkRememberMe.Checked;

            // Authenticate user
            var employeeInfo = AuthenticateUser(loginInput, password);
            if (employeeInfo != null)
            {
                // Successful authentication
                CreateUserSession(employeeInfo, rememberMe);

                // Check user role and redirect accordingly
                string userRole = employeeInfo["Rolle"]?.ToString();

                if (userRole == "Pointage Porte")
                {
                    // Redirect securityPortA users directly to their specific page
                    Response.Redirect("~/pointagfix/PointageEmployesPorts.aspx");
                }
                else
                {
                    // Redirect other users to dashboard
                    Response.Redirect("~/Default.aspx");
                }
            }
            else
            {
                // Failed authentication
                ShowErrorMessage("Identifiants invalides. Veuillez vérifier votre matricule/email et mot de passe.");
            }
        }

        private Dictionary<string, object> AuthenticateUser(string loginInput, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Query to authenticate by email or matricule
                    string query = @"
                        SELECT IdEmploye, IdSociete, Matricule, Nom, Prenom, Email, 
                               Rolle, IdDepartement, IdService, IdCellule, IdFonction, 
                               TypeEmploye
                        FROM EMPLOYE 
                        WHERE Email = @loginInput 
                        AND PasswordHash = @password 
                        AND is_deleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@loginInput", loginInput);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var employeeInfo = new Dictionary<string, object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    employeeInfo[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                }

                                return employeeInfo;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Erreur lors de l'authentification: " + ex.Message);
                }
            }

            return null;
        }

        private void CreateUserSession(Dictionary<string, object> employeeInfo, bool rememberMe)
        {
            // Create session variables
            Session["EmployeId"] = employeeInfo["IdEmploye"];
            Session["EmployeNom"] = employeeInfo["Nom"];
            Session["EmployePrenom"] = employeeInfo["Prenom"];
            Session["EmployeMatricule"] = employeeInfo["Matricule"];
            Session["EmployeEmail"] = employeeInfo["Email"];
            Session["EmployeRole"] = employeeInfo["Rolle"];
            Session["EmployeType"] = employeeInfo["TypeEmploye"];
            Session["IdSociete"] = employeeInfo["IdSociete"];
            Session["IdDepartement"] = employeeInfo["IdDepartement"];
            Session["IdService"] = employeeInfo["IdService"];
            Session["IdCellule"] = employeeInfo["IdCellule"];
            Session["IdFonction"] = employeeInfo["IdFonction"];
            Session["LoginTime"] = DateTime.Now;

            // Set session timeout (30 minutes by default)
            Session.Timeout = 30;

            // If remember me is checked, create persistent cookies
            if (rememberMe)
            {
                HttpCookie userCookie = new HttpCookie("RH_User");
                userCookie["EmployeId"] = employeeInfo["IdEmploye"].ToString();
                userCookie["Matricule"] = employeeInfo["Matricule"].ToString();
                userCookie.Expires = DateTime.Now.AddDays(30); // Remember for 30 days
                userCookie.HttpOnly = true; // Security: prevent XSS
                Response.Cookies.Add(userCookie);
            }
        }



        private void ShowErrorMessage(string message)
        {
            string script = $"alert('{message.Replace("'", "\\'")}');";
            ClientScript.RegisterStartupScript(this.GetType(), "ErrorMessage", script, true);
        }

        // Method to check user permissions (to be used in other pages)
        public static bool HasPermission(string requiredRole)
        {
            if (HttpContext.Current.Session["EmployeRole"] != null)
            {
                string userRole = HttpContext.Current.Session["EmployeRole"].ToString();
                // Add your role-based permission logic here
                return !string.IsNullOrEmpty(userRole);
            }
            return false;
        }

        // Method to get current user info (to be used in other pages)
        public static Dictionary<string, object> GetCurrentUser()
        {
            var userInfo = new Dictionary<string, object>();

            if (HttpContext.Current.Session["EmployeId"] != null)
            {
                userInfo["IdEmploye"] = HttpContext.Current.Session["EmployeId"];
                userInfo["Nom"] = HttpContext.Current.Session["EmployeNom"];
                userInfo["Prenom"] = HttpContext.Current.Session["EmployePrenom"];
                userInfo["Matricule"] = HttpContext.Current.Session["EmployeMatricule"];
                userInfo["Email"] = HttpContext.Current.Session["EmployeEmail"];
                userInfo["Role"] = HttpContext.Current.Session["EmployeRole"];
                userInfo["Type"] = HttpContext.Current.Session["EmployeType"];
            }

            return userInfo;
        }

    }
}


