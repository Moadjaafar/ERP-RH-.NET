using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace APPLICATION_RH
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Check if user is authenticated
            if (Session["EmployeId"] == null)
                {
                    Response.Redirect("~/connexion.aspx");
                    return;
                }

            if (!IsPostBack)
            {
                // Set user name in navigation
                string nom = Session["EmployeNom"]?.ToString() ?? "";
                string prenom = Session["EmployePrenom"]?.ToString() ?? "";
                litUserName.Text = $"{prenom} {nom}".Trim();

                if (string.IsNullOrEmpty(litUserName.Text))
                {
                    litUserName.Text = "Utilisateur";
                }
            }

        }
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // Log logout activity
            try
            {
                if (Session["EmployeId"] != null)
                {
                    // You can add logging here if needed
                }
            }
            catch { }

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
    }
}