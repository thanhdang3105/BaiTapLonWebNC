
function loading(wrapper = document.body) {
    if (wrapper.style) {
        wrapper.style.overflow = 'hidden'
        wrapper.style.position = 'relative'
    }
    const check = wrapper.querySelector('div.wrapper_loading')
    if (check) return
    const div = document.createElement('div')
    div.className = 'wrapper_loading'
    div.innerHTML = '<i class="fa fa-spinner fa-spin"></i>'
    wrapper.appendChild(div)
}

function unLoading(wrapper = document.body) {
    if (wrapper.style) {
        wrapper.style.overflow = 'unset'
    }
    const div = document.querySelector('div.wrapper_loading')
    div && wrapper.removeChild(div)
}