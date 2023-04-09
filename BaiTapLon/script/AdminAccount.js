window.addEventListener("DOMContentLoaded", () => {
    window.adminAccount = {
        limit: 10,
        skip: 0
    }
    getData()
})

window.onclick = (e) => {
    const event = new CustomEvent('windowClick', { detail: e.target })
    window.dispatchEvent(event)
}

async function getData() {
    let url = '/admin/quanly.aspx/getAccounts'
    const searchParams = new URLSearchParams();
    const filter = adminAccount.filter
    const sort = adminAccount.sort
    const limit = adminAccount.limit
    const skip = adminAccount.skip
    if (filter) {
        searchParams.set('filter', filter)
    }
    if (sort) {
        searchParams.set('sort', sort)
    }
    if (limit) {
        searchParams.set('limit', limit)
    }
    if (skip >= 0) {
        searchParams.set('skip', skip)
    }

    if (searchParams.toString()) {
        url += '?' + searchParams.toString()
    }

    loading()
    const res = await request(url, null, 'GET')
    if (res.status === 200) {
        adminAccount.count = res?.count
        bindDataTable(res?.data)
    }
    unLoading();
    return res.status
}

function bindDataTable(data) {
    adminAccount.dataTable = data

    const count = adminAccount.count
    const limit = adminAccount.limit
    const skip = adminAccount.skip
    const countPage = Math.ceil(count / limit)

    adminAccount.countPage = countPage

    let render = "";
    Array.isArray(data) && data.map(item => {
        let JSONItem
        try {
            JSONItem = JSON.stringify(item).replaceAll("\"", "'");
        } catch (e) {
            JSONItem = item.toString()
        }
        render += `<tr id="row-${item.id}">
                    <td>${item.id}</td>
                    <td>${item.name}</td>
                    <td>${item.email}</td>
                    <td>${item.birthday}</td>
                    <td>${item.phone}</td>
                    <td>${item.sex}</td>
                    <td>${item.role != 'system' ? `<input type="checkbox" ${item.role === 'admin' && 'checked'} onclick="changeRoleAccount(event,${item.id})" />` : 'System account'}</td>
                    <td>${item.locked}</td>
                    <td class='action_column'>${renderMenuAction(item)}</td>
                </tr>`
    })
    document.querySelector('#tableAccount tbody').innerHTML = render

    const inputPage = document.querySelector('input[name=pagination_input-page]')
    if (inputPage) inputPage.value = skip + 1
    document.getElementById('account_header-count').innerText = count
    document.getElementById('account_page-count').innerText = countPage
}

function renderMenuAction(item) {
    const html = `
                <button class="btn_icon" type="button" onclick="toggleMenu(event)" title="action"><i class="fa fa-list"></i></button>
                <ul class="menu_action">
                    <li class="item_menu" onclick="handleMenuAction('resetPassword',${item.id})"><i class="fa fa-key"></i> Reset password</li>
                    ${item.role == 'system' ? '' :
            item.locked ? '<li class="item_menu" onclick="handleMenuAction(\'unlockAccount\',' + item.id + ')"><i class="fa fa-lock-open"></i> Mở khóa</li>'
                :
                '<li class="item_menu" onclick="handleMenuAction(\'lockAccount\',' + item.id + ')"><i class="fa fa-lock"></i> Khóa tài khoản</li>'}
                </ul>
            `
    return html
}

async function handleMenuAction(action, id, value) {
    const dataTable = adminAccount?.dataTable
    let error = null;
    let newData;
    const formData = new FormData();
    formData.set('action', action)
    if (!id) {
        error = 'ID is required!'
    }

    if (value) {
        formData.set("value", value)
    }
    formData.set('ID', id)
    let confirmMsg = "";
    let enableConfirm = true
    switch (action) {
        case 'resetPassword':
            confirmMsg = 'Bạn muốn reset mật khẩu của tài khoản có ID là: ' + id
            break;
        case 'lockAccount':
            confirmMsg = 'Bạn muốn khóa tài khoản có ID là: ' + id
            if (dataTable) {
                newData = dataTable.map(item => {
                    if (item.id === id) {
                        item.locked = true;
                    }
                    return item
                })
            }
            break;
        case 'unlockAccount':
            confirmMsg = 'Bạn muốn mở khóa tài khoản có ID là: ' + id
            if (dataTable) {
                newData = dataTable.map(item => {
                    if (item.id === id) {
                        item.locked = false;
                    }
                    return item
                })
            }
            break;
        case 'changeRoleAccount':
            enableConfirm = false
            if (dataTable && value) {
                newData = dataTable.map(item => {
                    if (item.id === id) {
                        item.role = value;
                    }
                    return item
                })
            }
            break;
        default:
            error = "Action is not supported!"
            break;
    }

    if (error) {
        console.error(error);
        return
    } else if (enableConfirm) {
        if (!confirm(confirmMsg)) {
            return
        }
    }

    loading()

    const res = await request('/admin/quanly.aspx', formData)

    if (res.status === 200) {
        document.body.click();
        if (newData) {
            bindDataTable(newData)
        }
        alert(res.data)
    } else {
        document.body.click();
        alert(res.msg)
    }
    unLoading()
}

function toggleMenu(event) {
    const parent = event.target.parentElement
    const menu = event.target.nextElementSibling
    if (menu) {
        menu.classList.toggle('show');
        if (menu.classList.contains('show')) {
            window.addEventListener("windowClick", (e) => {
                if (!parent.contains(e.detail)) {
                    menu.classList.remove('show')
                }
            })
        }
    }

}

function openModal() {
    const modal = document.querySelector('.wrapper_modal')
    if (modal) modal.style.display = 'flex'
}

async function sortBy(e, field, desc) {
    const button = e.target
    let sort
    if (desc) {
        sort = `${field} DESC`
    } else {
        sort = `${field} ASC`
    }

    adminAccount.sort = sort

    const status = await getData()
    if (status === 200) {
        button.onclick = (event) => {
            sortBy(event, field, !desc)
        }
        const listEnableSorts = document.querySelectorAll('#header_row button')

        for (let btn of listEnableSorts) {
            const icon = btn.children[1]
            icon && btn.removeChild(icon);
        }

        const newicon = document.createElement('i');
        newicon.className = 'fa fa-caret-down';
        if (newicon) {
            if (!desc) {
                newicon.className = newicon.className.replace('down', 'up')
            }
        }
        button.appendChild(newicon)
    }
}

function handleChangePage(sub) {
    const countPage = adminAccount.countPage
    let skip = adminAccount.skip
    if (skip == 0 && sub < 0) {
        return
    } else if (skip == countPage - 1 && sub > 0) {
        return
    } else {
        skip = skip + sub
        adminAccount.skip = skip
        getData()
    }
}

function handlePressEnter(event, callback) {
    const input = event.target
    const countPage = adminAccount.countPage
    const skip = adminAccount.skip
    if (event.code === 'Enter') {
        if (input.value < 1 || input.value > countPage) {
            input.value = skip + 1
        } else {
            callback(input.value)
        }
    }
}

function getDataPage(value) {
    const countPage = adminAccount.countPage
    if (value < 1 || value > countPage) {
        return
    } else {
        adminAccount.skip = value - 1
        getData()
    }
}

function changeRoleAccount(event, id) {
    if (confirm("Bạn muốn chỉnh sửa quyền Admin của Account có ID là: " + id + "?")) {
        const isAdmin = event.target.checked
        handleMenuAction("changeRoleAccount", id, isAdmin ? 'admin' : 'user')
    } else {
        event.preventDefault()
    }
}