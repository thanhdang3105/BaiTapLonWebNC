
window.addEventListener("DOMContentLoaded", () => {
    getData();
})

async function getData() {
    const title = document.querySelector('.detail_title');
    const formData = new FormData();
    const select = document.getElementById('select_sort-detailPage');
    const listCate = JSON.parse(sessionStorage.getItem("category") || "{}")
    if (select) {
        select.value = query.sort || ""
    }
    formData.set('category', '');
    if (query.search) {
        formData.set('search', query.search)
        formData.set('limit', 20)
        title.innerText = `Kết quả tìm kiếm của "${query.search}" : `;
    } else if (query.category) {
        formData.set('category', query.category);
        title.innerText = listCate?.[query.category] || query.category || 'Tất cả';
    }

    if (query.sort) {
        formData.set('sort', query.sort)
    }

    if (query.page) {
        let page = Number(query.page)
        if (page > 0) {
            formData.set('skip',page - 1)
        }
    } 
    const ul = document.querySelector('ul#Details');
    loading(ul)
    const res = await request('/server/DetailPage.aspx', formData);
    
    if (res.status === 200) {

        const data = res.data.data;
        const count = res.data.count;
        renderPagination(count, 20, query?.page)
        title.innerHTML += " <span class='span_text'> (" + count + " đầu sách)</span>";
        if (data.length <= 0) {
            return ul.innerHTML = '<li class="item_list">Hiện chưa có sách nào!</li>'
        }
        data.map(item => {
            const li = document.createElement('li');
            li.className = "item_list";
            li.innerHTML = `<a href="/view/ReadingPage.aspx?id=${item.id}" class="item_img">
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
                        </div>`
            ul.appendChild(li)
        })
    } else {
        console.log(res);
        ul.innerHTML = '<li class="item_list">No Data</li>'
    }
    unLoading(ul)
    
}

function handleChangeSelectDetailPage(e) {
    handleSearchParams('sort',e.target.value)
}

function renderPagination(count, limit = 20, currentPage = 1) {
    const wrapper = document.getElementById('detailPage_pagination')
    if (!count) {
        return
    } else {
        const length = Math.ceil(Number(count) / Number(limit))
        for (let i = 1; i <= Number(length); i++) {
            const pageItem = document.createElement('button')
            pageItem.type = "button";
            pageItem.className = "btn btn_ghost"
            pageItem.onclick = () => {
                paginationChangePage(i)
            }
            pageItem.innerText = i
            if (i == currentPage) {
                pageItem.disabled = true
            } else {
                pageItem.disabled = false
            }
            wrapper.appendChild(pageItem)
        }
    }
}

function paginationChangePage(page) {
    if (page < 1) return
    handleSearchParams('page',page)
}

function handleSearchParams(key,value) {
    const paramsSearch = new URLSearchParams(window.location.search)
    if (value) {
        paramsSearch.set(key, value)
    } else {
        paramsSearch.delete(key)
    }
    window.location.search = paramsSearch.toString()
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