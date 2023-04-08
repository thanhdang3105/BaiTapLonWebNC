
function toggleAction() {
    const switchView = document.querySelector('div.switch_view');
    const formLogin = document.querySelector('form#login');
    const formRegister = document.querySelector('form#register');
    if (switchView) {
        if (switchView.classList.contains('right')) {
            switchView.classList.remove('right')
            resetForm(formRegister)
        } else {
            switchView.classList.add('right')
            resetForm(formLogin)
        }
    }
}

function setErrorInfo(inputName, errMsg = 'Error', isErr = true, form = '') {
    const input = document.querySelector(`${form && '#' + form} input[name=${inputName}]`)
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



function resetForm(form) {
    form.reset();
    const listInputErr = form.querySelectorAll('.wrapper_input.showError input');
    for (let input of listInputErr) {
        setErrorInfo(input.name, undefined, false)
    }
}

window.addEventListener("DOMContentLoaded", (e) => {
    if (window.location.search && window.location.search.includes('action')) {
        const action = window.location.search.split('=')[1]
        if (action == 'register') {
            toggleAction()
        }
    }
    (() => {
        const listInput = document.querySelectorAll('div.wrapper_body input')
        for (let input of listInput) {
            input.oninput = (event) => {
                if (event.target.value) {
                    setErrorInfo(event.target.name, undefined, false, input.form.id)
                }
            }
        }
    })()
})

function checkValue(inputName, value, formData) {
    let isError = false
    if (inputName === 'birthday') {
        const age = new Date(value) - Date.now()
        if (age > -(1000 * 60 * 60 * 24 * 365 * 10) && age < 0) {
            setErrorInfo(inputName, 'Bạn còn chưa đủ 10 tuổi cơ hic :<');
            isError = true
        } else if (age > 0) {
            setErrorInfo(inputName, 'Bạn còn chưa đẻ cơ hic :<');
            isError = true
        }
    } else if (inputName === 'password-check') {
        if (value !== formData.get('password')) {
            setErrorInfo(inputName, 'Mật khẩu không khớp!');
            isError = true
        }
    } else if (inputName === 'OTP') {
        if (value.length < 6) {
            setErrorInfo(inputName, 'Mã xác thực gồm 6 chữ số!');
            isError = true
        }
    }
    return isError
}

const formResetPWD = `<label for="username">Email</label>
                    <div class="wrapper_input">
                        <input name="username" type="email" title="Vui lòng nhập Tài khoản của bạn" placeholder="VD: NguyenVanA@gmail.com" required />
                        <div class="error_info">
                            !
                            <span>Error</span>
                        </div>
                    </div>
                    <label for="password">Password</label>
                    <div class="wrapper_input">
                        <input name="password" type="password" title="Vui lòng nhập mật khẩu của bạn" placeholder="Nhập mật khẩu" required />
                        <div class="error_info">
                            !
                            <span>Error</span>
                        </div>
                    </div>
                    <label for="password-check">Re-Password</label>
                    <div class="wrapper_input">
                        <input name="password-check" type="password" title="Vui lòng nhập lại mật khẩu của bạn" placeholder="Nhập lại mật khẩu" required />
                        <div class="error_info">
                            !
                            <span>Error</span>
                        </div>
                    </div>
                    <button type="submit">Đổi mật khẩu</button>`

async function forgotPassword(event) {
    const modal = document.getElementById('modal_account');
    //const currentModalHtml = modal.innerHTML
    if (!modal.classList.contains("show")) {
        modal.classList.add('show');
    } else {
        event.preventDefault();
        const form = event.target
        const button = form.querySelector('button[type=submit]')
        const overlay = modal.querySelector('div.overlay')
        if (!form) return alert('System Error')
        button.disabled = true;
        overlay.onclick = () => { }; 
        const formData = new FormData(form)
        let data = {}
        let isError = false
        for (var p of formData) {
            let name = p[0];
            let value = p[1];
            if (checkValue(name, value, formData)) {
                isError = true
            }
            data[name] = value
        }
        if (isError) {
            button.disabled = false;
            overlay.onclick = closeModal
            return false
        }

        try {
            const res = await request('/server/HandleAccount.aspx?action=' + form.id, formData)
            if (res.status === 200) {
                console.log(res.data)
                if (res.data === 'Password is reset Successfuly!') {
                    overlay.onclick = closeModal
                    alert(res.data)
                    overlay.click()
                } else if (!formData.get("OTP")) {
                    const label = document.createElement("label", { for: "OTP", innerText: "Email" })
                    label.setAttribute("for", "OTP")
                    label.innerHTML = "OTP"
                    const wrapperInput = document.createElement("div")
                    wrapperInput.className = "wrapper_input"
                    wrapperInput.innerHTML = `<input name="OTP" type="text" title="Vui lòng nhập mã xác thực đuọc gửi tới mail của bạn" required />
                        <div class="error_info">
                            !
                            <span>Error</span>
                        </div>`
                    form.querySelector("input[name=username]").setAttribute("readonly", true)
                    form.insertBefore(label, button)
                    form.insertBefore(wrapperInput, button)
                } else if (res.data === "OTP isCorrect!") {
                    form.innerHTML = formResetPWD
                    const username = form.querySelector("input[name=username]")
                    username.value = formData.get("username")
                    username.setAttribute("readonly", true)
                }
            } else if (res.status === 400) {
                setErrorInfo('username', res.data,true, form.id)
            } else if (res.status === 500) {
                alert(res.data)
            }
            button.disabled = false;
            overlay.onclick = closeModal
        } catch (e) {
            console.log(e)
        }
    }
}

function closeModal(event) {
    const modal = event.target.parentElement
    modal.classList.remove("show");
    modal.innerHTML = `<div class="overlay" onclick="closeModal(event)"></div>
                <form id="forgotPassword" name="forgot" method="post" onsubmit="forgotPassword(event)">
                    <h1>Quên mật khẩu</h1>
                    <label for="username">Email</label>
                    <div class="wrapper_input">
                        <input name="username" type="email" title="Vui lòng nhập Tài khoản của bạn" placeholder="VD: NguyenVanA@gmail.com" required />
                        <div class="error_info">
                            !
                            <span>Error</span>
                        </div>
                    </div>
                    <button type="submit">Gửi mã xác nhận</button>
                </form>`
}


async function submitForm(event) {
    event.preventDefault()
    const button = event.target.querySelector('button[type=submit]')
    const form = event.target
    if (!form) return alert('System Error')
    button.disabled = true;
    const formData = new FormData(form)
    let data = {}
    let isError = false
    for (var p of formData) {
        let name = p[0];
        let value = p[1];
        if (checkValue(name, value, formData)) {
            isError = true
        }
        data[name] = value
    }
    if (isError) {
        button.disabled = false;
        return false
    }

    //const res = await request('/server/HandleAccount.aspx?action=' + form.id, formData);

    //console.log(res)
    const params = new URLSearchParams(window.location.search)

    let xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function () {
        if (this.readyState == 4) {
            button.disabled = false;
            if (this.status == 200) {
                let rs
                try {
                    rs = JSON.parse(this.response)
                    localStorage.setItem("tokenWebSach", rs.token)
                    rs = JSON.stringify(rs.data)
                } catch (err) {
                    rs = this.response
                    alert(rs)
                }
                sessionStorage.setItem('userInfo', rs)
                alert('Login Success!')
                if (params.get('redirect') === 'true') {
                    window.history.back()
                } else {
                    window.location.href = '/view/HomePage.html'
                }
                form.reset()
            } else {
                let rs
                try {
                    rs = JSON.parse(this.response)
                    alert(rs.msg)
                } catch (err) {
                    rs = this.response
                    alert(rs)
                }
            }
        }

    };
    xmlhttp.open("POST", '/server/HandleAccount.aspx?action=' + form.id , true);
    xmlhttp.send(formData);

    //const res = await fetch('/server/HandleAccount.aspx?action=' + form.id, {
    //    method: 'POST',
    //    body: formData,
    //    headers: {
    //        contentType: 'application/json'
    //        }
    //})
    //if (res.status === 200) {
    //    let rs
    //    try {
    //        rs = await res.json();
    //        rs = rs.data
    //    } catch (err) {
    //        console.error(err)
    //        rs = await res.text()
    //    }
    //    alert('Login Success')
    //    window.location.href = '/view/HomePage.html'
    //} else {
    //    let rs
    //    try {
    //        rs = await res.json();
    //        rs = rs.msg
    //    } catch (err) {
    //        console.error(err)
    //        rs = await res.text()
    //    }
    //    alert(rs)
    //}
} 

function request(url, body, method = "POST") {
    return new Promise((resolve, reject) => {
        let xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function () {
            if (this.readyState == 4) {
                let result = {
                    status: this.status,
                    statusText: this.statusText
                }
                if (this.status == 200) {
                    let rs
                    try {
                        rs = JSON.parse(this.response)
                        //localStorage.setItem("tokenWebSach", rs.token)
                        //rs = JSON.stringify(rs.data)
                        rs = rs.data
                    } catch (err) {
                        rs = this.response
                    }
                    result.data = rs
                    resolve(result)
                } else {
                    let rs
                    try {
                        rs = JSON.parse(this.response)
                        rs = rs.msg
                    } catch (err) {
                        rs = this.response
                    }
                    result.data = rs
                    resolve(result)
                }
            }

        };
        xmlhttp.open(method, url, true);
        xmlhttp.send(body);
    })
}