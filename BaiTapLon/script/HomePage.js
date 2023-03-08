

window.addEventListener("DOMContentLoaded", (e) => {
    checkUserLogin();
})

async function checkUserLogin() {
    let userInfo = sessionStorage.getItem("userInfo")
    const menu = document.querySelector("div#userInfo_header ul.header_menuAccount");
    let el = `<li class="menu_item"><a class="text_link" href="/view/UserAccount.html">Đăng nhập</a></li>
                      <li class="menu_item"><a class="text_link" href="/view/UserAccount.html?action=register">Đăng kí</a></li>`
    if (userInfo) {
        userInfo = JSON.parse(userInfo)
        el = `<li class="menu_item"><a class="text_link" href="">${userInfo.name}</a></li>
                      <li class="menu_item" onclick="logout()">Đăng xuất</li>`
    } else {
        const token = localStorage.getItem("tokenWebSach");
        if (token) {
            const res = await request('/server/HandleAccount.aspx?action=checkToken', token);
            if (res.status === 200) {
                sessionStorage.setItem("userInfo", JSON.stringify(res.data))
                el = `<li class="menu_item"><a class="text_link" href="">${res.data.name}</a></li>
                      <li class="menu_item" onclick="logout()">Đăng xuất</li>`
            }
        }
    }
    menu.innerHTML = el
}

function logout() {
    sessionStorage.removeItem("userInfo");
    localStorage.removeItem("tokenWebSach");
    checkUserLogin();
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
                        console.log(err)
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
                        console.log(err)
                        rs = this.response
                    }
                    result.data = rs
                    reject(result)
                }
            }

        };
        xmlhttp.open(method, url, true);
        xmlhttp.send(body);
    })
}