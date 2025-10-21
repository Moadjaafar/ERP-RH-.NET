<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="connexion.aspx.cs" Inherits="APPLICATION_RH.connexion" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title><%: Page.Title %> | KING PELQGIAUE GROUP</title>

    <link rel="shortcut icon" href="~/assets/images/logo-sm.jpg" />
    <link href="~/assets/css/app.min.css" rel="stylesheet" type="text/css" />
    <link href="~/assets/css/icons.min.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="account-page">
            <div class="container p-0">
                <div class="row align-items-center g-0 px-3 py-3 vh-100">

                    <div class="col-xl-12">
                        <div class="row">
                            <div class="col-md-6 col-lg-5 mx-auto">
                                <div class="card">
                                    <div class="card-body">
                                        <div class="mb-0 p-0 p-lg-3">
                                            <div class="mb-0 border-0 p-md-4 p-lg-0">
                                                <div class="mb-4 p-0 text-center">
                                                    <div class="auth-brand">
                                                        <a href="index.html" class="logo logo-light">
                                                            <span class="logo-lg">
                                                                <asp:Image ID="imgLogoSmDark" runat="server"
                                                                    ImageUrl="~/assets/images/logo-lg2.png"
                                                                    AlternateText="AquaHR Logo"
                                                                    Style="width: 197px; height: auto;" />
                                                            </span>
                                                        </a>
                                                        <a href="index.html" class="logo logo-dark">
                                                            <span class="logo-lg">
                                                                <asp:Image ID="Image1" runat="server"
                                                                    ImageUrl="~/assets/images/logo-lg2.png"
                                                                    AlternateText="AquaHR Logo"
                                                                    Style="width: 197px; height: auto;" />
                                                            </span>
                                                        </a>
                                                    </div>
                                                </div>

                                                <div class="auth-title-section mb-4 text-center">
                                                    <h3 class="text-dark fw-semibold mb-3">Bienvenu à nouveau</h3>
                                                    <p class="text-muted fs-14 mb-0">King Pelagique a Group a Story, a family.</p>
                                                </div>

                                                <div class="pt-0">
                                                    <div class="form-group mb-3">
                                                        <asp:Label ID="lblEmail" runat="server" Text="Adresse e-mail" CssClass="form-label" AssociatedControlID="txtEmail" />
                                                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="SingleLine"
                                                            placeholder="Enter your email" ValidationGroup="LoginGroup" />
                                                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                                                            ErrorMessage="L'adresse e-mail est requise" Display="Dynamic"
                                                            CssClass="text-danger" ValidationGroup="LoginGroup" />
                                                    </div>

                                                    <div class="form-group mb-3">
                                                        <asp:Label ID="lblPassword" runat="server" Text="Mot de passe" CssClass="form-label" AssociatedControlID="txtPassword" />
                                                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password"
                                                            placeholder="Enter your password" ValidationGroup="LoginGroup" />
                                                        <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                                                            ErrorMessage="Le mot de passe est requis" Display="Dynamic"
                                                            CssClass="text-danger" ValidationGroup="LoginGroup" />
                                                    </div>

                                                    <div class="form-group d-flex mb-3">
                                                        <div class="col-sm-6">
                                                            <div class="form-check">
                                                                <asp:CheckBox ID="chkRememberMe" runat="server" CssClass="form-check-input" Checked="true" />
                                                                <asp:Label ID="lblRememberMe" runat="server" Text="Se souvenir de moi"
                                                                    CssClass="form-check-label" AssociatedControlID="chkRememberMe" />
                                                            </div>
                                                        </div>
                                                    </div>

                                                    <div class="form-group mb-0 row">
                                                        <div class="col-12">
                                                            <div class="d-grid">
                                                                <asp:Button ID="btnLogin" runat="server" Text="Se connecter"
                                                                    CssClass="btn btn-primary fw-semibold"
                                                                    OnClick="btnLogin_Click" ValidationGroup="LoginGroup" />
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                            </div>
                        </div>
                    </div>

                </div>
            </div>
        </div>
        <!-- Vendor -->
        <script src="/assets/libs/jquery/jquery.min.js"></script>
        <script src="/assets/libs/bootstrap/js/bootstrap.bundle.min.js"></script>
        <script src="/assets/libs/simplebar/simplebar.min.js"></script>
        <script src="/assets/libs/node-waves/waves.min.js"></script>
        <script src="/assets/libs/waypoints/lib/jquery.waypoints.min.js"></script>
        <script src="/assets/libs/jquery.counterup/jquery.counterup.min.js"></script>
        <script src="/assets/libs/feather-icons/feather.min.js"></script>

        <!-- App js-->
        <script src="/assets/js/app.js"></script>
        <script src="/assets/js/head.js"></script>
    </form>
</body>
</html>
