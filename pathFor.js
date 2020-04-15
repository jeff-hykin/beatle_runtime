module.exports = {
    package: __dirname + "/package.json",
    homepage: __dirname + "/index.html", // used but file doesn't exist (doesn't need to exist)
    // control center
    processFolder: __dirname + "/control_center/",
    processManager: __dirname + "/control_center/processManager.js",
    systemDataStorage: __dirname + "/control_center/systemData.json",
    privateSystemData: __dirname + "/control_center/privateSystemData.json",
    passwordManager: __dirname + "/control_center/utils/passwordManager.js"
}