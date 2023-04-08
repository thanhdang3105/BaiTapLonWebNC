function request(url, body, method = "POST") {
    return new Promise((resolve) => {
        let xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function () {
            if (this.readyState == 4) {
                let result = {
                    status: this.status,
                    statusText: this.statusText
                }
                if (this.status == 200) {
                    let rs
                    let count
                    try {
                        rs = JSON.parse(this.response)
                        //localStorage.setItem("tokenWebSach", rs.token)
                        //rs = JSON.stringify(rs.data)
                        count = rs.count
                        rs = rs.data
                    } catch (err) {
                        rs = this.response
                    }
                    result.data = rs
                    result.count = count
                    resolve(result)
                } else if (this.status === 401) {
                    alert(this.responseText)
                    result.data = this.responseText
                    window.navigation.navigate('/view/UserAccount.html?redirect=true')
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
        const token = localStorage.getItem('tokenWebSach')
        if (token) {
            xmlhttp.setRequestHeader("Authorization",token)
        }
        xmlhttp.send(body);
    })
}