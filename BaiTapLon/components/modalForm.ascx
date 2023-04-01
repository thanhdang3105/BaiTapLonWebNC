<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="modalForm.ascx.cs" Inherits="BaiTapLon.WebUserControl1" %>

<link rel="stylesheet" href="../style/GlobalStyles.css" />
<link rel="stylesheet" href="../style/ModalQuanLy.css" />

<form id="formModal" target="create" class="modalForm" onsubmit="submitForm(event)" onclick="event.stopPropagation();">
        <div class="header_modal">
            <h1>Thêm Sách</h1>
            <i class="fa fa-close icon_button" onclick="onClose()"></i>
        </div>
        <input name="id" type="text" hidden />
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
                <option value="" selected>Chọn thể loại</option>
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
        <label for="imgSrc">Ảnh</label>
        <div class="wrapper_input">
            <div class="wrapper_upload">
                <label for="imgSrc" class="btn btn_upload" ><i class="fa fa-plus"></i></label>
            </div>
            <input name="imgSrc" type="file" onchange="uploadFile(event)" id="imgSrc" accept="image/*" title="Upload file" hidden />
            <div class="error_info">
                Message:
                <span>Error</span>
            </div>
        </div>
        <button class="btn" type="submit">Tạo mới</button>
    </form>

<script src="../script/request.js" type="text/javascript"></script>
<script src="../script/firebase.js" type="module"></script>
<script>

    function uploadFile(event) {
        const file = event.target.files[0]
        const label = event.target.labels[1]
        const wapperUpload = label?.parentElement
        const reader = new FileReader();
        const buttonModal = document.querySelector('#formModal button[type=submit]')
        buttonModal.disabled = true
        if (file?.size) {
            reader.readAsDataURL(file);
            reader.onload = () => {
                const check = Array.from(wapperUpload.children).find(item => item.tagName === 'I' && item.className === "remove_preview fa fa-close")
                if (!check) {
                    const i = document.createElement('i')
                    i.className = "remove_preview fa fa-close"
                    i.onclick = removePreview
                    wapperUpload.appendChild(i)
                }
                label.innerHTML = `<img src="${reader.result}" alt="preview_img" />`
                buttonModal.disabled = false
            }
        } else {
            label.innerHTML = '<i class="fa fa-plus"></i>' 
            buttonModal.disabled = false
        }
    }

    function removePreview(event) {
        const parent = event.target.parentElement
        const preview = event.target.previousElementSibling
        parent.removeChild(event.target)
        preview.innerHTML = '<i class="fa fa-plus"></i>'
        window.formFormModal = {
            fileUpload: ''
        }
    }

    function onClose() {
        const form = document.forms["formModal"]
        const parent = form.parentElement
        parent.classList.remove('show');
        const wrapper_upload = document.querySelector('.wrapper_upload')
        if (wrapper_upload) wrapper_upload.innerHTML = '<label for="imgSrc" class="btn btn_upload" ><i class="fa fa-plus"></i></label>'
        window.formFormModal = {
            fileUpload: ''
        }
        form.reset()
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

    async function getCategory() {
        const form = document.getElementById("formModal")
        const res = await request('/server/homePage.aspx/getCate', null, "GET")
        if (res.status === 200) {
            const select = form?.querySelector("select[name=category]")
            res?.data?.map(item => {
                const lastOption = select.querySelector('option#option_last');
                const option = document.createElement('option')
                option.value = item.value
                option.innerText = item.key
                select.insertBefore(option, lastOption)
                
            })
        } else {
            console.log(res)
        } 
    }

    window.addEventListener("DOMContentLoaded", () => {
        getCategory()
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
        const buttonModal = document.querySelector('#formModal button[type=submit]')
        if (value === 'newItem') {
            const form = document.createElement('form')
            form.className = "createItem"
            form.innerHTML = `
                <input name="nameCate" type="text" title="Vui lòng nhập Name" placeholder="Name" required />
                <input name="valueCate" type="text" title="Vui lòng nhập Value" placeholder="Value" required />
                <i class="fa fa-check icon_button" onclick="handleNewItem(event,true)"></i>
                <i class="fa fa-close icon_button" onclick="handleNewItem(event,false)"></i>
            `
            parent.appendChild(form)
            buttonModal.disabled = true
        } else {
            const form = parent.querySelector('form')
            form && parent.removeChild(form);
            buttonModal.disabled = false
        }
    }

    async function handleNewItem(event, create) {
        const form = event.target.parentElement
        const parent = form.parentElement
        const select = parent.querySelector("select[name=category]")
        const buttonModal = document.querySelector('#formModal button[type=submit]')
        if (create) {
            const formData = new FormData(form);
            const label = formData.get('nameCate');
            const value = formData.get('valueCate');
            if (!value || !label) {      
                return setErrorInfo('category', 'Vui lòng nhập đủ name và value!',true,'select');
            }
            formData.set('action', 'createCate')
            const res = await request('/admin/quanly.aspx', formData)
            if (res.status === 200) {
                const firstOptions = select.options[1];
                const option = document.createElement('option')
                option.value = value
                option.innerText = label
                select.insertBefore(option, firstOptions)
                select.value = option.value
                parent.removeChild(form)
            } else {
                if (typeof res.data === "string") {
                    setErrorInfo('category', res.data, true, 'select');
                } else {
                    setErrorInfo('category', "Create fail!", true, 'select');
                }
            }
        } else {
            select.value = ""
            parent.removeChild(form)
        }
        buttonModal.disabled = false
    }

    async function submitForm(event) {
        event.preventDefault();
        const form = event.target
        const formData = new FormData(form);
        const button = form.querySelector('button[type=submit]')
        formData.set('action', form.target)
        formData.set('desc', JSON.stringify(formData.get("desc")))
        const file = formData.get('imgSrc')
        button.disabled = true
        const loading = document.createElement('i');
        loading.className = 'fa fa-spinner fa-spin';
        button.appendChild(loading)
        if (file.size > 0) {
            const fileName = formData.get('name')
            const img = await firebaseUploadFile(file, fileName)
            formData.set('imgSrc', img);
        } else if (window.formFormModal?.fileUpload) {
            formData.set('imgSrc', formFormModal?.fileUpload);
        } else {
            formData.set('imgSrc', '');
        }
        
        const res = await request('/admin/quanly.aspx', formData)
        if (res.status === 200) {
            onClose()
            const count = document.getElementById('quanly_header-count').innerText
            if (form.target === 'create') {
                const newRow = {
                    id: res.data,
                    name: formData.get('name'),
                    category: formData.get('category'),
                    desc: formData.get('desc'),
                    imgSrc: window.formFormModal?.fileUpload || '',
                    like: 0,
                    view: 0
                }
                quanlySach.unshift(newRow)
                document.getElementById('quanly_header-count').innerText = Number(count) + 1 
            } else if (form.target === 'update') {
                quanlySach.forEach(item => {
                    if (item.id == formData.get('id')) {
                        const keys = Object.keys(item)
                        keys.map(key => {
                            if (formData.get(key) !== null) {
                                item[key] = formData.get(key)
                            }
                        })
                    }
                    return item
                })
            } else if (form.target === 'delete') {
                quanlySach = quanlySach.filter(sach => sach.id != formData.get('id'))
                document.getElementById('quanly_header-count').innerText = Number(count) - 1
            }
            bindDataTable(quanlySach)
            form.reset()
        } else {
            alert(res.data.msg);
        }
        button.disabled = false
        button.removeChild(loading)
    }
</script>