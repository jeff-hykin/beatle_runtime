let crypto = require("crypto")

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

module.exports = {
    hashPassword,
    checkPassword,
}