import html from './convertToHtml.js'

const itemMenu = ({ href, label, onClick = () => { } }) => {
    const customEvent = new CustomEvent('handleClickComponents', {
        detail: {
            function: onClick,
            event: ''
        }
    })
    console.log(customEvent)

    const handleClick = (event) => {
        customEvent.detail.event = event
        window.dispatchEvent(customEvent)
    }

    return html`<li class="menu_item" onclick="(${handleClick})(event)">${href ? `<a class="text_link" href="${href}">${label}</a>` : label}</li>`
}

export default itemMenu