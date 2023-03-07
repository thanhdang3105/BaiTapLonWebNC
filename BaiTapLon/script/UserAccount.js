(function handleInputChange() {
    const listInput = document.querySelectorAll('div.wrapper_body input')
    for (let input of listInput) {
        input.oninput = (event) => {
            if (event.target.value) {
                setErrorInfo(event.target.name, undefined, false)
            }
        }
    }
})()

function setErrorInfo(inputName, errMsg = 'Error', isErr = true) {
    const input = document.querySelector(`input[name=${inputName}]`)
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

function toggleAction() {
    const switchView = document.querySelector('div.switch_view');
    const formLogin = document.querySelector('form#login');
    const formRegister = document.querySelector('form#register');
    if (switchView) {
        if (switchView.classList.contains('right')) {
            switchView.classList.remove('right')
            switchView.innerText = 'Chào mừng bạn trở lại với Web Sách'
            resetForm(formRegister)
        } else {
            switchView.classList.add('right')
            switchView.innerText = 'Tạo tài khoản để trở thành 1 thành viên của Web Sách'
            resetForm(formLogin)
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