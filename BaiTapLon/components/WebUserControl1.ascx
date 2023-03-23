<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WebUserControl1.ascx.cs" Inherits="BaiTapLon.WebUserControl1" %>

<link rel="stylesheet" href="../style/GlobalStyles.css" />
<link rel="stylesheet" href="../style/ModalQuanLy.css" />

<form id="formModal" class="modalForm" onsubmit="submitForm(event)" onclick="event.stopPropagation();">
        <div class="header_modal">
            <h1>Thêm Sách</h1>
            <i class="fa fa-close icon_button" onclick="onClose()"></i>
        </div>
        <label for="name">Tên sách</label>
        <div class="wrapper_input">
            <input name="name" type="text" title="Vui lòng nhập tên sách!"  placeholder="Tên sách..." required />
            <div class="error_info">
                Message: 
                <span>Error</span>
            </div>
        </div>
        <label for="category">Thể loại</label>
        <div class="wrapper_input">
            <select name="category" required onchange="handleSelect(event)">
                <option value="">Chọn thể loại</option>
                <option value="action">Hành động</option>
                <option id="option_last" value="newItem">Thêm thể loại</option>
            </select>
            <div class="error_info">
                 Message: 
                <span>Error</span>
            </div>
        </div>
        <label for="desc">Mô tả</label>
        <div class="wrapper_input">
            <textarea name="desc" title="Vui lòng nhập mô tả sách!" placeholder="Mô tả"></textarea>
            <div class="error_info">
                Message:
                <span>Error</span>
            </div>
        </div>
        <label for="imgSach">Ảnh</label>
        <div class="wrapper_input">
            <input name="imgSach" type="file" accept="image/*" title="Upload file" />
            <div class="error_info">
                Message:
                <span>Error</span>
            </div>
        </div>
        <button class="btn" type="submit">Tạo mới</button>
    </form>

<script src="../script/request.js" type="text/javascript"></script>
<script>

    function onClose() {
        const parent = document.forms["formModal"].parentElement
        parent.classList.remove('show');    
    }

    function setErrorInfo(inputName, errMsg = 'Error', isErr = true,tag = 'input') {
        const input = document.querySelector(`${tag}[name=${inputName}]`)
        if (input) {
            const wrapperInput = input.parentElement
            if (isErr) {
                if (wrapperInput) {
                    wrapperInput.classList.add('showError')
                    const spanErr = wrapperInput.querySelector('div.error_info > span')
                    if (spanErr) {
                        spanErr.innerText = errMsg
                    }
                }
            } else {
                if (wrapperInput) {
                    wrapperInput.classList.remove('showError')
                    const spanErr = wrapperInput.querySelector('div.error_info > span')
                    if (spanErr) {
                        spanErr.innerText = errMsg
                    }
                }
            }

        }
    }

    window.addEventListener("DOMContentLoaded", () => {
        const listInput = document.querySelectorAll('.modalForm input')
        for (let input of listInput) {
            input.oninput = (event) => {
                if (event.target.value) {
                    setErrorInfo(event.target.name, undefined, false)
                }
            }
        }
    })

    function handleSelect(event) {
        const parent = event.target.parentElement
        const value = event.target.value
        setErrorInfo(event.target.name, undefined, false, 'select')
        if (value === 'newItem') {
            const form = document.createElement('form')
            form.className = "createItem"
            form.innerHTML = `
                <input name="keyCate" type="text" title="Vui lòng nhập key" placeholder="Key" required />
                <input name="valueCate" type="text" title="Vui lòng nhập Value" placeholder="Value" required />
                <i class="fa fa-check icon_button" onclick="handleNewItem(event,true)"></i>
                <i class="fa fa-close icon_button" onclick="handleNewItem(event,false)"></i>
            `
            parent.appendChild(form)
        } else {
            const form = parent.querySelector('form')
            form && parent.removeChild(form);
        }
    }

    async function handleNewItem(event, create) {
        const form = event.target.parentElement
        const parent = form.parentElement
        const select = parent.querySelector("select[name=category]")
        if (create) {
            const formData = new FormData(form);
            const value = formData.get('keyCate');
            const label = formData.get('valueCate');
            if (!value || !label) {      
                return setErrorInfo('category', 'Vui lòng nhập đủ key và value!',true,'select');
            }
            formData.set('action', 'createCate')
            const res = await request('/admin/quanly.aspx', formData)
            if (res.status === 200) {
                const lastOption = select.querySelector('option#option_last');
                const option = document.createElement('option')
                option.value = value
                option.innerText = label
                select.insertBefore(option, lastOption)
                select.value = option.value
            }
            parent.removeChild(form)
        } else {
            select.value = ""
            parent.removeChild(form)

        }
    }

    async function submitForm(event) {
        event.preventDefault();
        const form = event.target
        const formData = new FormData(form);
        const bodyTable = document.querySelector('#tableSach tbody') 
        formData.set('action', 'create')
        formData.set('desc', formData.get("desc").trim())
        const res = await request('/admin/quanly.aspx', formData)
        console.log(res)
        if (res.status === 200) {
            onClose()
            const row = document.createElement('tr')
            row.innerHTML = `
                    <td>${res.data}</td>
                    <td>${formData.get('name')}</td>
                    <td>${formData.get('category')}</td>
                    <td>${formData.get('desc')}</td>
                    <td>${formData.get('imgSach')}</td>
                    <td>0</td>
                    <td>0</td>
            `
            bodyTable.prepend(row);
            form.reset()
        } else {
            alert(res.data.msg);
        }
    }
</script>