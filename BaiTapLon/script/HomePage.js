
window.addEventListener("DOMContentLoaded", (e) => {
    getData();
})

async function getData() {
    loading();
    const res = await request('/server/homePage.aspx',null, "GET");
    if (res.status === 200) {
        const data = res.data;
        if(!data) return
        Object.keys(data).map(key => {
            const ul = document.querySelector('ul#' + key.toUpperCase());
            data[key].map(item => {
                const li = document.createElement('li');
                li.className = "item_list";
                 li.innerHTML = `<a href="/view/DetailPage.html" class="item_img">
                            <img src="${item.imgSrc}" onerror="handleImgError(event)"  title="img-${item.name}" alt="img-${item.name}" />
                       </a>
                        <div class="item_info">
                            <a href="/view/DetailPage.html" class="item_info-name text_link">${item.name}</a>
                            <span class="item_info-desc">${item.desc.toString()}</span>
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
    unLoading();
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