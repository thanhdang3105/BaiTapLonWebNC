<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="quanly.aspx.cs" Inherits="BaiTapLon.admin.quanly" %>

<%@ Register Src="~/components/modalForm.ascx" TagPrefix="uc1" TagName="WebUserControl1" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>AdminPage</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.3.0/css/all.min.css" integrity="sha512-SzlrxWUlpfuzQ+pcUCosxcglQRNAq/DZjVsC0lE40xsADsfeQoEypE+enwcOiGjk/bSuGGKHEyjSoQ1zVisanQ==" crossorigin="anonymous" referrerpolicy="no-referrer" />
    <link rel="stylesheet" href="../style/QuanLy.css" />
    <script src="../script/handleImgError.js" type="text/javascript" defer></script>
    <script src="../script/loading.js" type="text/javascript" defer></script>
</head>
<body>
    <div class="wrapper_modal" onclick="onClose()">
        <uc1:WebUserControl1 runat="server" id="WebUserControl1"  />
    </div>
    <form id="form1" runat="server" onsubmit="event.preventDefault()">
        <div class="header">
            <h1><a href="/view/HomePage.html" class="text_link"><i class="fa fa-home"></i></a> Quản lý sách: <span class="span_text">(<span id="quanly_header-count" class="span_text"></span> sách)</span></h1>
            <button type="button" class="btn" onclick="openModal()"><i class="fa fa-plus"></i>Create</button>
        </div>
        <table border="1" id="tableSach">
            <thead>
                <tr id="header_row">
                    <td><button class="btn btn_ghost" type="button" onclick="sortBy(event,'ID',false)"><strong>ID</strong> <i class="fa fa-caret-down"></i></button></td>
                    <td><strong>Name</strong></td>
                    <td><strong>Category</strong></td>
                    <td><strong>Description</strong></td>
                    <td><strong>Image</strong></td>
                    <td><button class="btn btn_ghost" type="button" onclick="sortBy(event,'like',true)"><strong>Like</strong></button></td>
                    <td><button class="btn btn_ghost" type="button" onclick="sortBy(event,'view',true)"><strong>View</strong></button></td>
                    <td><strong>Actions</strong></td>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td></td>
                    <td></td>
                    <td></td>
                    <td></td>
                    <td></td>
                    <td></td>
                    <td></td>
                </tr>
            </tbody>
            <tfoot>
                <tr>
                    <td colspan="3"><button type="button" class="btn" onclick="handleChangePage(-1)"><i class="fa fa-arrow-left"></i>Prev</button></td>
                    <td colspan="2"><div>
                        <input type="number" name="pagination_input-page" class="pagination_page" value="1" oninput="validationInputPage(event)" onkeypress="handlePressEnter(event,getDataPage)" onblur="getDataPage(event.target.value)" />
                        of
                        <span class="span_text" id="quanly_page-count"></span>
                        </div></td>
                    <td colspan="3"><button type="button" class="btn" onclick="handleChangePage(1)">Next<i class="fa fa-arrow-right"></i></button></td>
                </tr>
            </tfoot>
        </table>
    </form>
    <script>
        
        function handlePressEnter(event,action) {
            if (event.code === 'Enter') {
                action(event.target.value)
            }
        }

        function validationInputPage(event) {
            const value = event.target.value;
            const pageCount = document.getElementById('quanly_page-count').innerText
            if (pageCount && value && value > Number(pageCount)) {
                event.target.value = pageCount
            } else if (pageCount && value && value < 1) {
                event.target.value = 1
            }
        }

        function handleChangePage(sub) {
            if (sub) {
                const indexedPage = document.querySelector('input[name=pagination_input-page]')
                const pageCount = document.getElementById('quanly_page-count')?.innerText
                const newPage = Number(indexedPage?.value || '') + sub
                if (newPage > Number(pageCount) || newPage < 1) {
                    return
                } else {
                    getDataPage(newPage)
                    indexedPage.value = newPage
                }
            }
        }

        async function getDataPage(page) {
            if(!page) return
            page = Number(page) - 1
            let url = "/admin/quanly.aspx/getData?skip=" + page
            if (window.quanly?.currentUrl && quanly.currentUrl.includes('?')) {
                url = quanly.currentUrl + "&skip=" + page
            }
            const res = await request(url, null, 'GET')
            if (res.status === 200) {
                bindDataTable(res?.data || [])
            }
        }
    
        function openModal() {
            let form = document.getElementById('formModal');
            const modal = form?.parentElement
            if (modal) {
                modal.classList.add('show')
                form.target = 'create'
                
            }
        }

        async function sortBy(e, field, sort) {
            const res = await request('/admin/quanly.aspx/getData?sort=' + field + " " + (sort ? "DESC" : "ASC"), null, 'GET')
            window.quanly = {
                currentUrl: '/admin/quanly.aspx/getData?sort=' + field + " " + (sort ? "DESC" : "ASC")
            }
            if (res.status === 200) {
                bindDataTable(res?.data)
                const button = e.target
                button.onclick = (event) => {
                    sortBy(event, field, !sort)
                }
                const listEnableSorts = document.querySelectorAll('#header_row button')

                Array.from(listEnableSorts).map(btn => {
                    const icon = btn.children[1]
                    icon && btn.removeChild(icon);
                })

                const newicon = document.createElement('i');
                newicon.className = 'fa fa-caret-down';
                if (newicon) {
                    if (!sort) {
                        newicon.className = newicon.className.replace('down', 'up')
                    }
                }
                button.appendChild(newicon)
            }
        }

        function bindDataTable(data) {
            let render = "";
            data?.map(item => {
                let JSONItem
                try {
                    JSONItem = JSON.stringify(item).replaceAll("\"", "'");
                } catch (e) {
                    JSONItem = item.toString()
                }
                render += `<tr id="row-${item.id}">
                    <td>${item.id}</td>
                    <td>${item.name}</td>
                    <td>${item.category}</td>
                    <td>${item.desc}</td>
                    <td><img class="preview_img" alt="preview-${item.name}" onerror="handleImgError(event)" title="img-${item.name}" src="${item.imgSrc}" /></td>
                    <td>${item.like}</td>
                    <td>${item.view}</td>
                    <td class='action_column'><i class='fa fa-edit' onclick="updateItem(event,${JSONItem})"></i><i class='fa fa-trash' onclick="deleteItem(event,${JSONItem})"></i></td>
                </tr>`
            })
            document.querySelector('#tableSach tbody').innerHTML = render
        }

        function bindFormValue(form, value) {

            for (let input of (form.elements || [])) {
                if (input.type === 'file' && typeof value[input?.name] !== 'object' && value[input?.name]) {
                    window.formFormModal = {
                        fileUpload: value[input?.name]
                    }
                    const parentInput = input.parentElement
                    const preview = parentInput.querySelector('div.wrapper_upload');
                    preview.innerHTML = `<label for="imgSrc" class="btn btn_upload" ><img src="${value[input?.name] || ''}" onerror="handleImgError(event)" alt="preview_img" /></label>
                                        <i class="remove_preview fa fa-close" onclick="removePreview(event)"></i>`
                } else {
                    input.value = (value[input?.name || ''] || '')
                }
            }

        }

        function updateItem(e, item) {
            const form = document.getElementById('formModal');
            const parent = form.parentElement
            bindFormValue(form,item)
            form.target = 'update'
            const title = form.querySelector('.header_modal h1')
            const button = form.querySelector('button[type=submit]')
            title.innerText = 'Cập nhật sách'
            button.innerText = 'Cập nhật'
            parent.classList.add('show')
        }

        function deleteItem(e, item) {
            let form = document.getElementById('formModal');
            const lists = Array.from(form.elements || [])
            const buttonSubmit = form.querySelector('button[type=submit]')
            lists.map(ele => {
                if (ele.type !== 'file') {
                    ele.value = item[ele?.name || ''] || 'action'
                }
            })
            if (item.id) {
                if (confirm('Bạn muốn xóa sách có id: ' + item.id + '?')) {
                    form.target = 'delete'
                    buttonSubmit.click();
                }
            }
        }

        async function getData() {
            loading()
            const res = await request('/admin/quanly.aspx/getData', null, 'GET')
            window.quanlySach = res.data
            if (res.status === 200) {
                bindDataTable(res?.data)
                if (res.count || res.count === 0) {
                    document.getElementById('quanly_header-count').innerText = res.count
                    document.getElementById('quanly_page-count').innerText = Math.ceil(res.count / 10) || 1
                }
            }
            unLoading();
        }

        window.addEventListener("DOMContentLoaded", () => {
            getData()
        })

    </script>
</body>
</html>
