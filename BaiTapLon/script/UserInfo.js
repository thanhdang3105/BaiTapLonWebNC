const element = {}
const api = {
    user_info: getUserInfo,
    user_read: getUserRead,
    user_like: getUserLike
}
let userInfo = sessionStorage.getItem("userInfo")
try {
    userInfo = JSON.parse(userInfo)
} catch {
}
// --------------------------------------------------------------------------------------------- \\
// ---------------------------------------User info--------------------------------------------- \\
// --------------------------------------------------------------------------------------------- \\


async function reLogin(token) {
    if (token) {
        const res = await request('/server/HandleAccount.aspx?action=checkToken', token);
        if (res.status === 200) {
            sessionStorage.setItem("userInfo", JSON.stringify(res.data))
            return res.data
        } else if (res.status === 400 && res.data === "Token expired!") {
            localStorage.removeItem("tokenWebSach");
            return null
        }
    }
}

async function getUserInfo() {
    if (!userInfo) {
        const token = localStorage.getItem("tokenWebSach")
        if (!token) {
            alert("Vui lòng đăng nhập.")
            window.navigation.navigate('/view/UserAccount.html?redirect=true')
            return
        } else {
            userInfo = await reLogin(token);
        }
    }

    if (userInfo) {
        const form = document.createElement('form')
        form.action = "/server/HandleUserInfo.aspx"
        form.method = "post"
        form.className = "form_userInfo"
        form.onsubmit = (event) => handleSubmitForm(event)
        form.innerHTML = `<h1 class="body_header-title">Thông tin tài khoản</h1>
                    <input type="text" hidden name="id" />
                    <label for="name">Họ tên:</label>
                    <div class="input_wrapper">
                        <input class="input" type="text" id="name" name="name" placeholder="Nhập họ và tên" required />
                        <div class="error_info">
                            Message:
                            <span>Error</span>
                        </div>
                    </div>
                    <label for="email">Email:</label>
                    <div class="input_wrapper">
                        <input class="input" type="email" disabled id="email" name="email" required />
                        <div class="error_info">
                            Message:
                            <span>Error</span>
                        </div>
                    </div>
                    <label for="birthday">Ngày sinh:</label>
                    <div class="input_wrapper">
                        <input class="input" type="date" id="birthday" name="birthday" required />
                        <div class="error_info">
                            Message:
                            <span>Error</span>
                        </div>
                    </div>
                    <label for="phone">Số điện thoại:</label>
                    <div class="input_wrapper">
                        <input class="input" type="text" id="phone" name="phone" placeholder="Nhập số điện thoại" required />
                        <div class="error_info">
                            Message:
                            <span>Error</span>
                        </div>
                    </div>
                    <label for="sex">Giới tính:</label>
                    <div class="input_wrapper">
                        <input type="radio" checked name="sex" value="male" required />
                        <label class="label_radio">Nam</label>
                        <input type="radio" name="sex" value="female" required />
                        <label class="label_radio">Nữ</label>
                        <div class="error_info">
                            Message:
                            <span>Error</span>
                        </div>
                    </div>
                    <div class="form_control">
                        <button type="submit" class="btn">Submit</button>
                    </div>`
        const form2 = document.createElement('form')
        form2.action = "/server/HandleUserInfo.aspx"
        form2.method = "post"
        form2.className = "form_userInfo"
        form2.onsubmit = async (event) => {
            const isSuccess = await handleSubmitForm(event)
            if (isSuccess) {
                form2.reset();
            }
        }
        form2.innerHTML = `<h1 class="body_header-title">Đổi mật khẩu</h1>
                    <input type="text" hidden name="id" />
                    <input type="email" hidden name="email" />
                    <label for="oldPassword">Mật khẩu cũ:</label>
                    <div class="input_wrapper">
                        <input class="input" type="password" id="oldPassword" name="oldPassword" placeholder="Nhập mật khẩu" required />
                        <div class="error_info">
                            Message:
                            <span>Error</span>
                        </div>
                    </div>
                    <label for="newPassword">Mật khẩu mới:</label>
                    <div class="input_wrapper">
                        <input class="input" type="password" id="newPassword" name="newPassword" placeholder="Nhập mật khẩu mới" required />
                        <div class="error_info">
                            Message:
                            <span>Error</span>
                        </div>
                    </div>
                    <label for="password-check">Nhập lại mật khẩu mới:</label>
                    <div class="input_wrapper">
                        <input class="input" type="password" id="password-check" name="password-check" placeholder="Nhập lại mật khẩu mới" required />
                        <div class="error_info">
                            Message:
                            <span>Error</span>
                        </div>
                    </div>
                    <div class="form_control">
                        <button type="submit" class="btn">Submit</button>
                        <button type="reset" class="btn">Reset</button>
                    </div>`

        for (let input of form.elements) {
            if (input.name === 'sex') {
                if (input.value == userInfo["sex"]) {
                    input.checked = true
                } else {
                    input.checked = false
                }
            } else {
                input.value = userInfo[input.name]
            }
            input.oninput = () => {
                setErrInput({
                    name: input.name,
                    status: null,
                    errMsg: null,
                    form
                })
            }
        }

        const div = document.createElement('div')
        div.id = 'user_info-wrapper'
        div.classList.add('wrapper_content')
        div.appendChild(form)
        div.appendChild(form2)

        element.user_info = {
            element: div
        } 
    } else {
        alert("Vui lòng đăng nhập.")
        window.navigation.navigate('/view/UserAccount.html?redirect=true')
    }
}

function setErrInput({ name, status, errMsg, form }) {
    const input = form.elements[name]
    const parent = input?.parentElement
    const errBox = parent?.querySelector(".error_info")
    const errText = errBox?.querySelector('span')
    if (!errBox || !errText) return
    if (status === "err") {
        errBox.classList.add('error')
        errBox.classList.add('show')
    } else if (status === 'info') {
        errBox.classList.add('show')
    } else {
        errBox.classList.remove('show')
    }
    errText.innerText = errMsg || 'Error!'
}

function checkValue(formData, form = document) {
    let isError = false
    for (let [key, value] of formData) {
        if (key === 'birthday') {
            const age = new Date(value) - Date.now()
            if (age > -(1000 * 60 * 60 * 24 * 365 * 10) && age < 0) {
                setErrInput({
                    name: key,
                    errMsg: 'Bạn còn chưa đủ 10 tuổi cơ hic :<',
                    status: 'err',
                    form
                });
                isError = true
            } else if (age > 0) {
                setErrInput({
                    name: key,
                    errMsg: 'Bạn còn chưa đẻ cơ hic :<',
                    status: 'err',
                    form
                });
                isError = true
            }
        } else if (key === 'password-check') {
            if (value !== formData.get('password') && value !== formData.get('newPassword')) {
                setErrInput({
                    name: key,
                    errMsg: 'Mật khẩu không khớp!',
                    status: 'err',
                    form
                });
                isError = true
            }
        } else if (key === 'OTP') {
            if (value.length < 6) {
                setErrInput({
                    name: key,
                    errMsg: 'Mã xác thực gồm 6 chữ số!',
                    status: 'err',
                    form
                });
                isError = true
            }
        }
    }
    return isError
}



async function handleSubmitForm(event) {
    event.preventDefault();
    const form = event.target

    const formData = new FormData(form)

    if (!formData.get('email')) {
        formData.set('email', userInfo["email"])
    }
    if (!formData.get('id')) {
        formData.set('id', userInfo["id"])
    }


    const checkErr = checkValue(formData,form)

    if (checkErr) {
        return
    }

    const url = form.action + '?action=updateUser'

    loading()

    const res = await request(url, formData)

    unLoading()

    if (res.status === 200) {
        Object.keys(userInfo).map(item => {
            if (formData.get(item)) {
                userInfo[item] = formData.get(item)
            }
        })
        try {
            if (res?.data?.token) {
                res.data.msg && alert(res.data.msg)
                localStorage.setItem('tokenWebSach', res.data.token)
            } else {
                alert(res.data)
            }
            sessionStorage.setItem('userInfo', JSON.stringify(userInfo))
        } catch {
            sessionStorage.setItem('userInfo', userInfo)
        }
        return true
    } else if (res.status === 401) {
        return false
    } else {
        alert(res.data)
        return false
    }
}

// --------------------------------------------------------------------------------------------- \\
// ---------------------------------------User read--------------------------------------------- \\
// --------------------------------------------------------------------------------------------- \\

async function getUserRead() {

    const currentState = element.user_read

    let sort = 'ID DESC'
    let page = 1
    let limit = 10

    if (currentState) {
        if (currentState.sort) sort = currentState.sort
        if (currentState.page) page = currentState.page
        if (currentState.limit) limit = currentState.limit
    }

    const params = new URLSearchParams()
    params.set('sort', sort)
    params.set("skip", page - 1)
    params.set('limit',limit)

    loading()
    const res = await request('/server/HandleUserInfo.aspx?action=getUserReadBook&' + params.toString(),null,"GET")
    unLoading()

    const div = document.createElement('div')
    div.id = 'user_read-wrapper'
    div.classList.add('wrapper_content')

    if (res.status === 200) {

        const pageCount = []

        for (let i = 1; i <= Math.ceil(res.count / 10); i++) {
            pageCount.push(1)
        }

        div.innerHTML = `<div class="body_header-control">
                        <h1 class="body_header-title">Sách đã đọc: (${res.count} sách)</h1>
                        <select id="control_sort" class="select_sort" onchange="handleChangeSort(event,'user_read')">
                            <option value="ID DESC" ${sort === 'ID DESC' ? 'selected' : ''}>Gần đây nhất</option>
                            <option value="ID ASC" ${sort === 'ID ASC' ? 'selected' : ''}>Cũ nhất</option>
                        </select>
                    </div>
                    <ul class="body_content-list">
                        ${Array.isArray(res.data) && res.data.map(item => `<li class="item_list">
                            <span class="time_badge">${ConvertDate(item.updatedAt)}</span>
                            <a href="/view/ReadingPage.aspx?id=${item.id}" class="item_list-img">
                                <img src="${item.imgSrc}" onerror="handleImgError(event)" title="img-${item.name}" alt="img-${item.name}" />
                            </a>
                            <div class="item_info">
                                <a href="/view/ReadingPage.aspx?id=${item.id}" class="item_info-name text_link">${item.name}</a>
                                <span class="item_info-desc span_text">${item.author}</span>
                                <span class="item_info-desc">${item.category.key}</span>
                                <div class="item_info-rate">
                                    <span class="info_rate-like span_text">${item.like} <i class="fa fa-heart"></i></span>
                                    <span class="info_rate-view span_text">${item.view} <i class="fa fa-eye"></i></span>
                                </div>
                            </div>
                        </li>`).join(" ")}
                    </ul>
                    <div class="body_footer-control">
                        <ul class="pagination_box">
                            ${pageCount.map((i, index) => `
                                <li class="pagination_item ${page == i + index ? 'active' : ''}" onclick="handleChangePage(event,${i + index})">${i + index}</li>
                            `).join(" ")}
                        </ul>
                    </div>`


    } else {
        if (res.msg) {
            alert(res.msg)
            div.append(res.msg)
        }
    }

    element.user_read = {
        element: div,
        sort,
        page,
        limit
    }
    return div
}

function ConvertDate(date) {
    const ms = Date.now() - new Date(date)
    const hour = (ms / 1000 / 60 / 60) - 7 

    if (hour < 1) {
        return Math.ceil(hour * 60) + ' Phút trước'
    } else if (hour > 24) {
        return new Date(date).toDateString()
    } else {
        return Math.ceil(hour) + ' Giờ trước'
    }
}

async function handleChangeSort(event,id) {
    const value = event.target.value
    const currentState = element[id]
    if (currentState) {
        if (currentState.sort === value) {
            return
        } else {
            currentState.sort = value
            loading()
            stateUserInfo.element = await api[id]()
            setActiveTab()
            unLoading()
        }
    }
}

async function handleChangePage(e, i, id) {
    const item = e.target
    if (item.classList.contains('active') || item.innerText == i || i < 1) {
        return
    } else {
        if (element[id]) {
            element[id].page = Number(i) || 1
            loading()
            stateUserInfo.element = await api[id]()
            setActiveTab()
            unLoading()
        }
    }
}


// --------------------------------------------------------------------------------------------- \\
// ---------------------------------------User like--------------------------------------------- \\
// --------------------------------------------------------------------------------------------- \\

async function getUserLike() {
    const currentState = element.user_like

    let sort = 'ID DESC'
    let page = 1
    let limit = 10

    if (currentState) {
        if (currentState.sort) sort = currentState.sort
        if (currentState.page) page = currentState.page
        if (currentState.limit) limit = currentState.limit
    }

    const params = new URLSearchParams()
    params.set('sort', sort)
    params.set("skip", page - 1)
    params.set('limit', limit)

    loading()
    const res = await request('/server/HandleUserInfo.aspx?action=getUserLikeBook&' + params.toString(), null, "GET")
    unLoading()

    const div = document.createElement('div')
    div.id = 'user_like-wrapper'
    div.classList.add('wrapper_content')

    if (res.status === 200) {

        const pageCount = []

        for (let i = 1; i <= Math.ceil(res.count / 10); i++) {
            pageCount.push(1)
        }

        div.innerHTML = `<div class="body_header-control">
                        <h1 class="body_header-title">Sách yêu thích: (${res.count} sách)</h1>
                        <select id="control_sort" class="select_sort" onchange="handleChangeSort(event,'user_like')">
                            <option value="ID DESC" ${sort === 'ID DESC' ? 'selected' : ''}>Gần đây nhất</option>
                            <option value="ID ASC" ${sort === 'ID ASC' ? 'selected' : ''}>Cũ nhất</option>
                        </select>
                    </div>
                    <ul class="body_content-list">
                        ${Array.isArray(res.data) && res.data.map(item => `<li class="item_list">
                            <a href="/view/DetailPage.html" class="item_list-img">
                                <img src="${item.imgSrc}" onerror="handleImgError(event)" title="img-${item.name}" alt="img-${item.name}" />
                            </a>
                            <div class="item_info">
                                <a href="/view/DetailPage.html" class="item_info-name text_link">${item.name}</a>
                                <span class="item_info-desc span_text">${item.author}</span>
                                <span class="item_info-desc">${item.category.key}</span>
                                <div class="item_info-rate">
                                    <span class="info_rate-like span_text">${item.like} <i class="fa fa-heart"></i></span>
                                    <span class="info_rate-view span_text">${item.view} <i class="fa fa-eye"></i></span>
                                </div>
                            </div>
                        </li>`).join(" ")}
                    </ul>
                    <div class="body_footer-control">
                        <ul class="pagination_box">
                            ${pageCount.map((i, index) => `
                                <li class="pagination_item ${page == i + index ? 'active' : ''}" onclick="handleChangePage(event,${i + index})">${i + index}</li>
                            `).join(" ")}
                        </ul>
                    </div>`


    } else {
        if (res.msg) {
            alert(res.msg)
            div.append(res.msg)
        }
    }

    element.user_like = {
        element: div,
        sort,
        page,
        limit
    }

    return div
}

// --------------------------------------------------------------------------------------------- \\
// ---------------------------------------User Main--------------------------------------------- \\
// --------------------------------------------------------------------------------------------- \\

window.addEventListener("DOMContentLoaded", async () => {
    await getUserInfo()
    window.stateUserInfo = {
        active: 'user_info',
        element: element['user_info'].element
    }
    setActiveTab()
})

function collapseSiderBar(event) {
    const container = document.querySelector('div.userInfo_container')
    for (let i of event.target.children) {
        if (i.nodeName === 'I') {
            if (i.className.includes('left')) {
                i.className = i.className.replace('left', 'right')
            } else if (i.className.includes('right')) {
                i.className = i.className.replace('right', 'left')
            }
        }
    }
    container.classList.toggle('collapse')
}



function setActiveTab() {
    const active = stateUserInfo.active
    const currentElement = stateUserInfo.element
    const listTab = document.querySelectorAll('.siderBar_content .siderBar_menu-item')
    const root = document.getElementById('root-body')
    for (let tab of listTab) {
        let ele = element[tab.id]?.element
        tab.onclick = async () => {
            if (stateUserInfo.active != tab.id) {
                stateUserInfo.active = tab.id
                if (ele) {
                    stateUserInfo.element = ele
                } else {
                    stateUserInfo.element = await api[tab.id]()
                }
                setActiveTab()
            }
        }
        if (tab.id === active) {
            tab.classList.add("active")
        } else {
            tab.classList.remove("active")
        }
    }
    if (root.children.length === 0) {
        root.appendChild(currentElement)
    } else {
        root.replaceChild(currentElement, root.children[0])
    }

}

