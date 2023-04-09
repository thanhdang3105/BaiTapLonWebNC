<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReadingPage.aspx.cs" Inherits="BaiTapLon.ReadingPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Reading</title>
    <meta charset="utf-8" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.3.0/css/all.min.css" integrity="sha512-SzlrxWUlpfuzQ+pcUCosxcglQRNAq/DZjVsC0lE40xsADsfeQoEypE+enwcOiGjk/bSuGGKHEyjSoQ1zVisanQ==" crossorigin="anonymous" referrerpolicy="no-referrer" />
    <link href="../style/GlobalStyles.css" rel="stylesheet" />
    <link href="../style/Header.css" rel="stylesheet" />
    <link href="../style/DetailPage.css" rel="stylesheet" />
    <link href="../style/Footer.css" rel="stylesheet" />
    <script src="../script/Header.js" defer></script>
    <script src="../script/loading.js" defer></script>
</head>
<body>
<form id="form" runat="server">
    <div class="container">
        <header class="header_container">
        </header>
        <div class="storyContent">
            <nav class="breadcrumbs">
                <ul>
                    <li><a href="/"><i class="fa fa-home"></i></a></li>
                    <li><asp:Literal runat="server" id="Literal1"></asp:Literal></li>
                    <li><asp:Literal runat="server" id="Literal2"></asp:Literal></li>
                </ul>
            </nav>
            <h1><asp:Literal runat="server" ID="Literal3"></asp:Literal></h1>
            <span class="span_text"><asp:Literal runat="server" ID="Literal4">Tác Giả</asp:Literal></span>
            <br />
            <asp:Literal runat="server" id="storyContent"></asp:Literal>
        </div>
        <footer class="footer_container">
            <div class="footer_content">
                <div class="logo_footer">
                    <a href="HomePage.html">
                        <img src="/images/logo.jpg" title="logo" alt="logo" />
                    </a>
                </div>
                <div class="footer_content-box">
                    <a class="text_link">Liên hệ bản quyền</a>
                    <a class="text_link">Chính sách bảo mật</a>
                </div>
            </div>
            <div class="footer_signal">
                Design By Thanh Dang
            </div>
        </footer>
    </div>
</form>
</body>
</html>
