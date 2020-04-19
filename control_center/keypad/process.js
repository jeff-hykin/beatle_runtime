let phidget22 = require("phidget22")
// module.exports = {
//     setupListener: whenStarPressed
// }


// 
// fixed data
// 
const numberOfInputs = 4
const numberOfOutputs = 4
const timeoutDuration = 15 * 1000 // its in miliseconds
const serialNumber = 96781
const probablyAPortNumber = 5661

// 
// helper for setting up pins
// 
let onStateChange
let allPins = []
let openPromiseList = []
let conn = new phidget22.Connection(probablyAPortNumber, "localhost")
let createPins = (numberOfPins, inputOrOutput)=> {
    let pins = []
    
    for (let eachIndex in [...Array(numberOfInputs)]) {
        // setup
        let newPin = new phidget22[inputOrOutput]()
        if (serialNumber) {
            newPin.setDeviceSerialNumber(serialNumber)
        }
        newPin.setChannel(eachIndex)
        
        // connect all inputs to a single state-change function
        if (inputOrOutput == "DigitalInput") {
            newPin.onStateChange = (...args)=> onStateChange(newPin, eachIndex, ...args)
        }
        // log onAttach/Detach
        newPin.onAttach = ()=>console.log(`${inputOrOutput} pin ${eachIndex} was attached`)
        newPin.onDetach = ()=>console.log(`${inputOrOutput} pin ${eachIndex} was attached`)
        
        // try to open the pins
        let pinLoaded = newPin.open(timeoutDuration)
        openPromiseList.push(pinLoaded)

        // add to output array
        pins.push(newPin)
        // add to all-pin array
        allPins.push(newPin)
    }

    return pins
}
let closeAllPins = ()=> {
    for (let each of allPins) {
        each.close()
    }
}

// 
// helper for detecting which key
//
let wait = (seconds)=>new Promise(resolve=>setTimeout(_=>resolve(),seconds*1000))
let whichKey = async (inputPin, indexOfInput, outputPins) => {
    let setStateDelayTime = 0.05
    keyValues = [
        ["1", "2", "3", "A"],
        ["4", "5", "6", "B"],
        ["7", "8", "9", "C"],
        ["*", "0", "#", "D"]
    ]
    returnValue = null
    
    
    for (let eachOutputIndex in outputPins) {
        outputPins[eachOutputIndex].setState(1)
        await wait(setStateDelayTime)
        if (inputPin.getState() == 0) {
            returnValue = keyValues[indexOfInput][numberOfOutputs-(eachOutputIndex+1)]
        }
    }
    
    for (let eachOutput of outputPins) {
        eachOutput.setState(0)
    }
    await wait(setStateDelayTime)
    
    return returnValue
}


// 
// main function
// 
conn.connect().then(() => {

    // create pins
    outputPins = createPins(numberOfOutputs, "DigitalOutput")
    inputPins = createPins(numberOfOutputs, "DigitalInput")

    // what to do when an input value changes
    onStateChange = async (pin, pinIndex, state) => {
        try {
            console.log(`keypad state changed: pin:${pinIndex} state: ${state}`)
            if (state == true) {
                
            }
        } catch (error) {
            console.log(`error is:`,error)
        }
    }
    
    // what do do after all pins have connected (or failed)
    Promise.all(openPromiseList).then(()=> {
        // NOTE: un-commenting this crashes the code for some reason, even though its similar to the online example
        // for (let each of outputPins) {
        //     each.setDutyCycle(1).catch((err)=>{
        //         console.error('Error during setDutyCycle:', err)
        //     })
        // }
        console.log(`all pins opened`)
        // closeAllPins()
    })
}).catch(error=> console.error(error))


