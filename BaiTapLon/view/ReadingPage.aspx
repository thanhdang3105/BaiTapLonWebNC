<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReadingPage.aspx.cs" Inherits="BaiTapLon.ReadingPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Reading Page</title>
    <meta charset="utf-8" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.3.0/css/all.min.css" integrity="sha512-SzlrxWUlpfuzQ+pcUCosxcglQRNAq/DZjVsC0lE40xsADsfeQoEypE+enwcOiGjk/bSuGGKHEyjSoQ1zVisanQ==" crossorigin="anonymous" referrerpolicy="no-referrer" />
    <link href="../style/GlobalStyles.css" rel="stylesheet" />
    <link href="../style/Header.css" rel="stylesheet" />
    <link href="../style/ReadingPage.css" rel="stylesheet" />
    <link href="../style/Footer.css" rel="stylesheet" />
    <script src="../script/request.js" defer></script>
    <script src="../script/Header.js" defer></script>
    <script src="../script/loading.js" defer></script>
</head>
<body>
    <div class="container">
        <header class="header_container">
        </header>
        <form id="form" runat="server">
        <div class="storyContent">
            <nav class="nav_control">
                <ul class="breadcrumbs">
                    <li><a href="/view/HomePage.html"><i class="fa fa-home"></i></a></li>
                    <li><asp:Literal runat="server" id="category"></asp:Literal></li>
                    <li><asp:Literal runat="server" id="breadBookName"></asp:Literal></li>
                </ul>
                <asp:Literal runat="server" id="btnLike"></asp:Literal>
            </nav>
            <h1 class="title"><asp:Literal runat="server" ID="bookName"></asp:Literal></h1>
            <span class="span_text"><asp:Literal runat="server" ID="author"></asp:Literal></span>
            <div class="storyContent_body">
                <asp:Literal runat="server" id="storyContent"></asp:Literal>
            </div>
        </div>
        </form>
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
    <script>
        window.addEventListener("DOMContentLoaded", () => {
            handleAction(null, "view")
        })
        function handleAction(e,action) {
            const button = e?.target
            const icon = button?.querySelector('i.fa')
            const formData = new FormData()
            formData.set('action', action)
            button && loading(button)
            request("", formData).then(res => {
                if (res.status === 200) {
                    if (action === "like") {
                        button.className = button.className.replace("like", "unlike")
                        icon.classList.replace("fa-heart", "fa-heart-crack")
                        button.childNodes[1].textContent = "Bỏ thích"
                        button.onclick = (event) => handleAction(event, "unlike")
                    } else if (action === "unlike") {
                        button.className = button.className.replace("unlike", "like")
                        icon.classList.replace("fa-heart-crack", "fa-heart")
                        button.childNodes[1].textContent = "Thích"
                        button.onclick = (event) => handleAction(event, "like")
                    }
                } 
                button && unLoading(button)
            }).catch(res => {
                if (res.status === 401) {
                    alert(res.data)
                    window.navigation.navigate('/view/UserAccount.html?redirect=true')
                } else {
                    alert("Đẫ xảy ra lỗi vui lòng thử lại sau!")

                }
                button && unLoading(button)
            })
        }
    </script>
</body>
</html>
