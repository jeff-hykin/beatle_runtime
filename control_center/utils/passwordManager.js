let crypto = require("crypto")
let fs = require("fs")
let pathFor = require("../../pathFor")

const PASSWORD_LENGTH = 256
const SALT_LENGTH = 64
const ITERATIONS = 10000
const DIGEST = 'sha256'
const BYTE_TO_STRING_ENCODING = 'hex' // this could be base64, for instance

function hashPassword(password) {
    const salt = crypto.randomBytes(SALT_LENGTH).toString(BYTE_TO_STRING_ENCODING)
    let hash = crypto.pbkdf2Sync(password, salt, ITERATIONS, PASSWORD_LENGTH, DIGEST)
    return {
        salt,
        hash: hash.toString(BYTE_TO_STRING_ENCODING),
        iterations: ITERATIONS,
    }
}

function checkPassword(hashData, passwordAttempt) {
    let hash = crypto.pbkdf2Sync(passwordAttempt, hashData.salt, hashData.iterations, PASSWORD_LENGTH, DIGEST)
    return hashData.hash === hash.toString(BYTE_TO_STRING_ENCODING)
}

function checkIfNormalString(string, argumentSource) {
    if (!(typeof string == 'string')) {
        throw new Error(`${argumentSource} isn't a string, the value was: ${string}`)
    } else if (string.length == 0) {
        throw new Error(`${argumentSource} was the empty string which isn't allowed`)
    }
}

function setPin(pin) {
    checkIfNormalString(pin, "pin argument from setPin()")

    let privateData = JSON.parse(fs.readFileSync(pathFor.privateSystemData))
    // create/overwrite pin
    privateData.pin = hashPassword(pin)
    // save them to the file
    fs.writeFileSync(pathFor.privateSystemData, JSON.stringify(privateData))
}

function setUsernameAndPassword({ username, password }) {
    // 
    // check inputs
    // 
    checkIfNormalString(username, "username argument from setUsernameAndPassword()")
    checkIfNormalString(password, "password argument from setUsernameAndPassword()")

    let privateData = JSON.parse(fs.readFileSync(pathFor.privateSystemData))
    // ensure users exists
    privateData.users || (privateData.users = {})
    // create user and password
    privateData.users[username] = hashPassword(password)
    // save them to the file
    fs.writeFileSync(pathFor.privateSystemData, JSON.stringify(privateData))
}

function checkUsernameAndPassword({ username, password }) {
    // 
    // check inputs
    // 
    checkIfNormalString(username, "username argument from setUsernameAndPassword()")
    checkIfNormalString(password, "password argument from setUsernameAndPassword()")

    let privateData = JSON.parse(fs.readFileSync(pathFor.privateSystemData))
    // ensure users exists
    privateData.users || (privateData.users = {})
    let savedData = privateData.users[username]
    // check the data
    if (savedData instanceof Object) {
        return checkPassword(savedData, password)
    }
    return false
}

function doesAtLeastOneUserExist() {
    let privateData = JSON.parse(fs.readFileSync(pathFor.privateSystemData))
    let validUsers = Object.values(privateData.users).filter(each=>(each instanceof Object) && (each.salt) && (each.hash) && (each.iterations))
    return validUsers.length > 0
}

function resetAllUsers() {
    let privateData = JSON.parse(fs.readFileSync(pathFor.privateSystemData))
    // ensure users exists
    privateData.users = {}
    // save them to the file
    fs.writeFileSync(pathFor.privateSystemData, JSON.stringify(privateData))
}

module.exports = {
    doesAtLeastOneUserExist,
    setPin,
    setUsernameAndPassword,
    checkUsernameAndPassword,
    resetAllUsers,
}