
const html = ([firstString, ...strings], ...values) => {
    return values.reduce((acc, cur) => acc.concat(cur, strings.shift()), [firstString]).filter(x => x && x !== true || x === 0).join('')
}

export default html