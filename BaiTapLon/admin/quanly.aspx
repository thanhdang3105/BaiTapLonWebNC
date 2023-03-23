<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="quanly.aspx.cs" Inherits="BaiTapLon.admin.quanly" %>

<%@ Register Src="~/components/WebUserControl1.ascx" TagPrefix="uc1" TagName="WebUserControl1" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.3.0/css/all.min.css" integrity="sha512-SzlrxWUlpfuzQ+pcUCosxcglQRNAq/DZjVsC0lE40xsADsfeQoEypE+enwcOiGjk/bSuGGKHEyjSoQ1zVisanQ==" crossorigin="anonymous" referrerpolicy="no-referrer" />
    <link rel="stylesheet" href="../style/QuanLy.css" />
</head>
<body>
    <div class="wrapper_modal" onclick="event.target.classList.remove('show')">
        <uc1:WebUserControl1 runat="server" id="WebUserControl1"  />
    </div>
    <form id="form1" runat="server">
        <div class="header">
            <h1>Quản lý sách:</h1>
            <button type="button" class="btn" onclick="openModal()"><i class="fa fa-plus"></i>Create</button>
        </div>
        <table border="1" id="tableSach">
            <thead>
                <tr>
                    <td><button class="btn btn_ghost" type="button"><strong>ID</strong> <i class="fa fa-sort-desc"></i></button></td>
                    <td><button class="btn btn_ghost" type="button"><strong>Name</strong> <i class="fa fa-plus"></i></button></td>
                    <td><button class="btn btn_ghost" type="button"><strong>Category</strong> <i class="fa fa-plus"></i></button></td>
                    <td><button class="btn btn_ghost" type="button"><strong>Description</strong> <i class="fa fa-plus"></i></button></td>
                    <td><button class="btn btn_ghost" type="button"><strong>Image</strong> <i class="fa fa-plus"></i></button></td>
                    <td><button class="btn btn_ghost" type="button"><strong>Like</strong> <i class="fa fa-plus"></i></button></td>
                    <td><button class="btn btn_ghost" type="button"><strong>View</strong> <i class="fa fa-plus"></i></button></td>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>1</td>
                    <td>1</td>
                    <td>1</td>
                    <td>1</td>
                    <td>1</td>
                    <td>1</td>
                    <td>1</td>
                </tr>
            </tbody>
        </table>
        <%--<asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:SqlDB %>" 
            SelectCommand="SELECT [ID], [name], [category], [description], [imgSrc], [numberLike], [numberView] FROM [tblSach] ORDER BY [ID] DESC"></asp:SqlDataSource>
        <asp:DataGrid ID="tableSach" runat="server" DataSourceID="SqlDataSource1" DataKeyNames="ID" AllowPaging="true" PageSize="15">
            <Columns>
                <asp:BoundColumn DataField="ID" HeaderText="ID" ReadOnly="True" SortExpression="ID" />
                <asp:BoundColumn DataField="name" HeaderText="name" SortExpression="name" />
                <asp:BoundColumn DataField="category" HeaderText="category" SortExpression="category" />
                <asp:BoundColumn DataField="description" HeaderText="description" SortExpression="description" />
                <asp:BoundColumn DataField="imgSrc" HeaderText="imgSrc" SortExpression="imgSrc" />
                <asp:BoundColumn DataField="numberLike" HeaderText="Like" SortExpression="numberLike" />
                <asp:BoundColumn DataField="numberView" HeaderText="View" SortExpression="numberView"  />
            </Columns>
        --%></asp:DataGrid>
       <%-- <asp:GridView ID="tableSach" runat="server" AutoGenerateColumns="False" DataKeyNames="ID" DataSourceID="SqlDataSource1" AllowPaging="True" PageSize="15" AllowSorting="True" CellSpacing="2">
            <Columns>
                <asp:BoundField DataField="ID" HeaderText="ID" InsertVisible="False" ReadOnly="True" SortExpression="ID" />
                <asp:BoundField DataField="name" HeaderText="name" SortExpression="name" />
                <asp:BoundField DataField="category" HeaderText="category" SortExpression="category" />
                <asp:BoundField DataField="description" HeaderText="description" SortExpression="description" />
                <asp:BoundField DataField="imgSrc" HeaderText="imgSrc" SortExpression="imgSrc" />
                <asp:BoundField DataField="numberLike" HeaderText="Like" SortExpression="numberLike" />
                <asp:BoundField DataField="numberView" HeaderText="View" SortExpression="numberView"  />
                <asp:ButtonField HeaderText="Actions" ButtonType="Button" Text="Xóa" ControlStyle-CssClass="btn" CommandName="buttin" />
            </Columns>
            <EmptyDataTemplate>
                No Data
            </EmptyDataTemplate>
        </asp:GridView>--%>
    </form>
    <script>
        function openModal() {
            const modal = document.querySelector('div.wrapper_modal')
            modal && modal.classList.add('show')
        }

        async function getData() {
            const res = await request('/admin/quanly.aspx/getData', null, 'GET')
            let data = "";
            res?.data?.map(item => {
                data += `<tr>
                    <td>${item.id}</td>
                    <td>${item.name}</td>
                    <td>${item.category}</td>
                    <td>${item.description}</td>
                    <td>${item.imgSrc}</td>
                    <td>${item.like}</td>
                    <td>${item.view}</td>
                </tr>`
            })
            document.querySelector('#tableSach tbody').innerHTML = data
        }
        getData()
    </script>
</body>
</html>
