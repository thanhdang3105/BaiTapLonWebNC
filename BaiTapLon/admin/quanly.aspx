<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="quanly.aspx.cs" Inherits="BaiTapLon.admin.quanly" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" action="/admin/quanly.aspx" runat="server">


    <h2>Form Validation Example</h2>
    <asp:Label ID="lblName" runat="server" Text="Name: "></asp:Label>
    <asp:TextBox ID="txtName" runat="server"></asp:TextBox>
    <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName"
        ErrorMessage="Please enter your name."></asp:RequiredFieldValidator>
    <br /><br />
    <asp:Label ID="lblEmail" runat="server" Text="Email: "></asp:Label>
    <asp:TextBox ID="txtEmail" runat="server"></asp:TextBox>
    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
        ErrorMessage="Please enter your email."></asp:RequiredFieldValidator>
    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
        ErrorMessage="Please enter a valid email address." ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
    <br /><br />
    <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="btnAdd_Click" />
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" DataKeyNames="ID" DataSourceID="SqlDataSource1">
            <Columns>
                <asp:BoundField DataField="ID" HeaderText="ID" InsertVisible="False" ReadOnly="True" SortExpression="ID" />
                <asp:BoundField DataField="name" HeaderText="name" SortExpression="name" />
                <asp:BoundField DataField="category" HeaderText="category" SortExpression="category" />
                <asp:BoundField DataField="description" HeaderText="description" SortExpression="description" />
                <asp:BoundField DataField="imgSrc" HeaderText="imgSrc" SortExpression="imgSrc" />
                <asp:BoundField DataField="numberLike" HeaderText="numberLike" SortExpression="numberLike" />
                <asp:BoundField DataField="numberView" HeaderText="numberView" SortExpression="numberView" />
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:dbSachConnectionString %>" SelectCommand="SELECT [ID], [name], [category], [description], [imgSrc], [numberLike], [numberView] FROM [tblSach]"></asp:SqlDataSource>
        <br />
        <br />
    </form>
</body>
</html>
