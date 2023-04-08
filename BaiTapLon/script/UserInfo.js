const element = {}
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
        form.action = "/server/HandleAccount.aspx"
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
        form2.action = "/server/HandleAccount.aspx"
        form2.method = "post"
        form2.className = "form_userInfo"
        form2.onsubmit = async (event) => {
            const isSuccess = await handleSubmitForm(event)
            if (isSuccess) {
                form2.reset();
            }
        }
        form2.innerHTML = `<h1 class="body_header-title">Đổi mật khẩu</h1>
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
        //div.id = 'user_info'
        div.appendChild(form)
        div.appendChild(form2)

        element.user_info = div 
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

    const checkErr = checkValue(formData,form)

    if (checkErr) {
        return
    }

    const url = form.action + '?action=updateUser'

    loading()

    const res = await request(url, formData)

    unLoading()

    if (res.status === 200) {
        alert(res.data)
        Object.keys(userInfo).map(item => {
            if (formData.get(item)) {
                userInfo[item] = formData.get(item)
            }
        })
        try {
            sessionStorage.setItem('userInfo', JSON.stringify(userInfo))
        } catch {
            sessionStorage.setItem('userInfo', userInfo)
        }
        return true
    } else {
        alert(res.msg)
        return false
    }
}

// --------------------------------------------------------------------------------------------- \\
// ---------------------------------------User read--------------------------------------------- \\
// --------------------------------------------------------------------------------------------- \\

async function getUserRead() {
    const div = document.createElement('div')
    //div.id = 'user_read'
    element.user_read = div
}

// --------------------------------------------------------------------------------------------- \\
// ---------------------------------------User like--------------------------------------------- \\
// --------------------------------------------------------------------------------------------- \\

async function getUserLike() {
    const div = document.createElement('div')
    //div.id = 'user_like'
    element.user_like = div
}

// --------------------------------------------------------------------------------------------- \\
// ---------------------------------------User Main--------------------------------------------- \\
// --------------------------------------------------------------------------------------------- \\

window.addEventListener("DOMContentLoaded", async () => {
    loading()
    await getUserInfo()
    await getUserRead()
    await getUserLike()
    window.stateUserInfo = {
        active: 'user_info',
        element: element['user_info']
    }
    unLoading()
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
    if (root.children.length === 0) {
        root.appendChild(currentElement)
    }
    for (let tab of listTab) {
        console.log(root.contains(element[tab.id]))
        if (root.contains(element[tab.id])) {
            root.replaceChild(currentElement, element[tab.id])
        }
        tab.onclick = () => {
            if (stateUserInfo.active != tab.id) {
                stateUserInfo.active = tab.id
                stateUserInfo.element = element[tab.id]
                setActiveTab()
            }
        }
        if (tab.id === active) {
            tab.classList.add("active")
        } else {
            tab.classList.remove("active")
        }
    }
}

