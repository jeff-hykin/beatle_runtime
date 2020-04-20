let Audic = require("audic")

module.exports = {
    systemDisarmedSound: new Audic("./public/systemDisarmed.m4a"),
    systemArmedSound: new Audic("./public/systemArmed.m4a"),
    systemArmed10Seconds: new Audic("./public/systemArmed10Seconds.m4a"),
    incorrectCodeSound: new Audic("./public/incorrectCode.m4a"),
}