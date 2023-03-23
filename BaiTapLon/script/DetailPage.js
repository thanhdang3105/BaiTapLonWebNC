
window.addEventListener("DOMContentLoaded", () => {
    getData();
})

async function getData() {
    const title = document.querySelector('.detail_title');
    const formData = new FormData();
    title.innerText = query.category || 'Tất cả';
    if (query.search) {
        formData.set('search', query.search)
        formData.set('limit', 20)
        title.innerText = `Kết quả tìm kiếm của "${query.search}" : `;
    } else if (query.category) {
        formData.set('category', query.category);
    } else {
        formData.set('category', '');
    }
    const res = await request('/server/HomePage.aspx', formData);
    const ul = document.querySelector('ul#Details');
    if (res.status === 200) {
        const data = res.data.data;
        const count = res.data.count;
        const title = ul.previousElementSibling
        title.innerHTML += " <span class='span_text'> (" + count +" đầu sách)</span>";
        if (data.length <= 0) {
            return ul.innerHTML = '<li class="item_list">No Data</li>'
        }
        data.map(item => {
            const li = document.createElement('li');
            li.className = "item_list";
            li.innerHTML = `<a href="/view/DetailPage.html" class="item_img">
                            <img src="https://th.bing.com/th/id/R.02fb9d33fe6274db44d306962b180e8a?rik=%2bsNFNzVh3jxgCQ&riu=http%3a%2f%2fi.desi.vn%2fl%2f2016%2f03%2f34%2f1.png&ehk=paVdyF1tqTwthbQqPtE0gQORb5CLd%2b61NnkeGgCdgz4%3d&risl=&pid=ImgRaw&r=0" title="logo" alt="logo" />
                        </a>
                        <div class="item_info">
                            <a href="/view/DetailPage.html" class="item_info-name text_link">${item.name}</a>
                            <span class="item_info-desc">${item.description}</span>
                            <div class="item_info-rate">
                                <span class="info_rate-like span_text">${item.like} <i class="fa fa-heart"></i></span>
                                <span class="info_rate-view span_text">${item.view} <i class="fa fa-eye"></i></span>
                            </div>
                        </div>`
            ul.appendChild(li)
        })
    } else {
        console.log(res);
        ul.innerHTML = '<li class="item_list">No Data</li>'
    }
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