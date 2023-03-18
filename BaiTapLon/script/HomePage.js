const itemMenu = ({ href, label, onClick }) => {

    return `<li class="menu_item" onclick='(${onClick})(event)'>${href ? `<a class="text_link" href="${href}">${label}</a>` : label}</li>`
}

window.addEventListener("DOMContentLoaded", (e) => {
    checkUserLogin();
    getData();
})

async function getData() {
    const res = await request('/server/homePage.aspx');
    console.log(res);
    if (res.status === 200) {
        const data = res.data;
        if(!data) return
        Object.keys(data).map(key => {
            const ul = document.querySelector('ul#' + key.toUpperCase());
            data[key].map(item => {
                const li = document.createElement('li');
                li.className = "item_list";
                li.innerHTML = `<a href="HomePage.html" class="item_img">
                            <img src="https://th.bing.com/th/id/R.02fb9d33fe6274db44d306962b180e8a?rik=%2bsNFNzVh3jxgCQ&riu=http%3a%2f%2fi.desi.vn%2fl%2f2016%2f03%2f34%2f1.png&ehk=paVdyF1tqTwthbQqPtE0gQORb5CLd%2b61NnkeGgCdgz4%3d&risl=&pid=ImgRaw&r=0" title="logo" alt="logo" />
                        </a>
                        <div class="item_info">
                            <a href="" class="item_info-name text_link">${item.name}</a>
                            <span class="item_info-desc">${item.description}</span>
                            <div class="item_info-rate">
                                <span class="info_rate-like span_text">${item.like} <i class="fa fa-heart"></i></span>
                                <span class="info_rate-view span_text">${item.view} <i class="fa fa-eye"></i></span>
                            </div>
                        </div>`
                ul.appendChild(li)
            })
            if (data[key].length < 10) {
                ul.parentElement.querySelector('a.show_more').style.display = 'none';
            }
        })
    }
}

async function checkUserLogin() {
    let userInfo = sessionStorage.getItem("userInfo")
    const headerUser = document.querySelector("div#userInfo_header");
    const menuAccount = headerUser.querySelector("ul.header_menuAccount");
    const iconUser = menuAccount.previousElementSibling.firstElementChild
    const menuNoti = headerUser.querySelector("ul.header_notification")
    const iconNoti = menuNoti.previousElementSibling.firstElementChild
    let el = `${itemMenu({ href: "/view/UserAccount.html", label: "Đăng nhập" })}
            ${itemMenu({ href: "/view/UserAccount.html?action=register", label: "Đăng kí" })}`
    menuAccount.innerHTML = el
    iconUser.classList.remove("fa");
    iconUser.classList.add("fa-regular");
    if (userInfo) {
        userInfo = JSON.parse(userInfo)
        iconUser.classList.remove("fa-regular");
        iconUser.classList.add("fa");
        el = `${itemMenu({ href: "/view/UserAccount.html", label: userInfo.name })}
                      ${itemMenu({ label: "Đăng xuất", onClick: logout })}`
    } else {
        const token = localStorage.getItem("tokenWebSach");
        if (token) {
            const res = await request('/server/HandleAccount.aspx?action=checkToken', token);
            if (res.status === 200) {
                sessionStorage.setItem("userInfo", JSON.stringify(res.data))
                iconUser.classList.remove("fa-regular");
                iconUser.classList.add("fa");
                el = `${itemMenu({ href: "/view/UserAccount.html", label: res.data.name })}
                      ${itemMenu({ label: "Đăng xuất", onClick: logout })}`
            } else if (res.status === 400 && res.data === "Token expired!") {
                localStorage.removeItem("tokenWebSach");
            }
        }
    }
    menuAccount.innerHTML = el
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
                    reject(result)
                }
            }

        };
        xmlhttp.open(method, url, true);
        xmlhttp.send(body);
    })
}