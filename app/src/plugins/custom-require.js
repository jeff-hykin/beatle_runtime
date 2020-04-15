let fs = require("fs")
global.customRequire = (name)=>eval(fs.readFileSync(name).toString('utf8'))