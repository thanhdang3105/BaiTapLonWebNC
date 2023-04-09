const headerHTML = `<div class="header">
                <div class="header_logo">
                    <a href="HomePage.html">
                        <img src="/images/logo.jpg" title="logo" alt="logo" />
                    </a>
                </div>
                <div class="header_search">
                    <div class="wrapper_search">
                        <input type="text" placeholder="Search..." class="input_search" name="header_input-search" oninput="handleInputSearch(event)" onkeydown="handleInputSearch(event)" />
                        <ul class="menu_search">
                            <li>No Data</li>
                        </ul>
                    </div>
                    <button class="header_btn-search btn_icon" onclick="handleClickSearch(event)" type="button" title="Search"><i class="fa fa-search"></i></button>
                </div>
                <div id="userInfo_header" class="header_account">
                    <div class="wrapper_menu">
                        <button class="btn_icon" type="button" title="Notifications"><i class="fa-regular fa-comment"></i></button>
                        <ul class="header_notification">
                            <li class="menu_item">No Notifications</li>
                        </ul>
                    </div>
                    <div class="wrapper_menu">
                        <button class="btn_icon" type="button" title="User"><i class="fa-regular fa-user"></i></button>
                        <ul class="header_menuAccount">
                        </ul>
                    </div>
                </div>
            </div>`

window.addEventListener("DOMContentLoaded", async () => {
    const header = document.querySelector('header.header_container');

    header.innerHTML = headerHTML;

    const subHeader = await createSubHeader();

    subHeader && header.appendChild(subHeader);

    checkUserLogin();
})

async function createSubHeader() {
    let search = new URLSearchParams(window.location.search)
    let query = {}
    if (search) {
        for (let [key, value] of search) {
            query[key] = value;
        }
    }
    if (!window.location.pathname.includes('DetailPage.html')) {
        query = ''
    }
    window.query = query;
    window.header = {
        category: {}
    }
    const res = await request('/server/homePage.aspx/getCate', null, "GET")
    if (res.status === 200) {
        const subHeader = document.createElement('nav')
        subHeader.className = 'sub_header'
        subHeader.innerHTML = ``
        const list = [{ key: 'Tất cả', value: "" }, ...(res?.data || [])]
        list.map(cate => {
            let child = document.createElement('span')
            if (query.category == cate?.value && !query.search) {
                child.classList.add('active')
            } else if (cate?.value === "") {
                if (query && !query.category && !query.search) {
                    child.classList.add('active')
                } else {
                    child = document.createElement('a')
                    child.href = "/view/DetailPage.html"
                }
            } else {
                child = document.createElement('a')
                child.href = "/view/DetailPage.html?category=" + cate?.value
            }
            child.classList.add('sub_header-item','text_link')
            child.innerText = cate?.key
            subHeader.appendChild(child)
            header.category[cate?.value] = cate?.key
        })
        sessionStorage.setItem("category", JSON.stringify(header.category || {}))
        return subHeader
    }
    return null
}


const itemMenu = ({ href, label, onClick }) => {

    return `<li class="menu_item" ${onClick && "onclick='("+onClick+")(event)'" }>${href ? `<a class="text_link" href="${href}">${label}</a>` : label}</li>`
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
        el = `${itemMenu({ href: "/view/UserInfo.html", label: userInfo.name })}
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

const handleInputSearch = (event) => {
    if (event.type === 'keydown' && event.key === "Enter" && event?.target?.value) {
        handleSearch(event.target.value)
    } else if (event.type === 'input' && !event.target.value) {
        handleSearch('')
    }
}

const handleClickSearch = (event) => {
    if (event?.target) {
        const inputSearch = document.getElementsByName('header_input-search');
        if (inputSearch && inputSearch[0] && inputSearch[0].value) {
            window.location.href = '/view/DetailPage.html?search=' + inputSearch[0].value
        }
    }
}
const handleSearch = async (value) => {
    const list = document.querySelector('ul.menu_search');
    if (!value) return list.innerHTML = 'No Data';
    list.innerHTML = '<i class="fa fa-spinner fa-spin"></i>';
    const formData = new FormData();
    formData.set("search", value);
    const res = await request('/server/DetailPage.aspx', formData);
    if (res.status === 200) {
        const data = res.data.data
        const count = res.data.count
        if (!data || data?.length === 0) {
            return list.innerHTML = 'No Data';
        }
        if (count > 9) {
            list.innerHTML = `<a href="/view/DetailPage.html?search=${value}">Xem thêm</a>`
        } else if(data.length > 0) {
            list.innerHTML = '';
        }
        for (let i = data.length - 1; i >= 0; i--) {
            let a = document.createElement('a');
            a.className = 'text_link'
            a.innerHTML = `
                <li class="menu_item">
                    <div class="item_img">
                        <img src="${data[i].imgSrc}" title="logo" alt="logo" />
                    </div>
                    <div class="item_info">
                        <strong>${data[i].name}</strong>
                        <span>${data[i].category.key}</span>
                        <br/>
                        <span>${data[i].author}</span>
                    </div>
                </li>
            `
            list.prepend(a)
        }
    }
}