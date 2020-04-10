import io from "socket.io-client"
let socket = io("http://localhost:3000")



socket.on("connect", () => {
    console.log("socket is connected")
    // TODO: show the user this status somehow
    window.$root.connectedToBackend = true
    // ask the backend for the most-up-to-date info
    socket.emit('requestSystemData', {})
})
socket.on("disconnect", () => {
    console.log("socket disconnected")
    window.$root.connectedToBackend = false
})

// update the app state whenever there are changes
socket.on("dataDidChange", (backendSystemData) => {
    console.log(`backend data changed: `,backendSystemData)
    // 
    // update systemData from backend
    // 
    if (Object.keys(backendSystemData).length > 0) {
        window.$root.systemData = backendSystemData
    }
    // changes were just confirmed
    window.$root.changesAreUnconfirmed = false    
})

// server responding to request
socket.on("providingSystemData", (backendSystemData) => {
    console.log(`receiving backend data: `,backendSystemData)
    // make sure frontend is up to date
    if (Object.keys(backendSystemData).length > 0) {
        window.$root.systemData = backendSystemData
    }
    // changes were just confirmed
    window.$root.changesAreUnconfirmed = false    
})


export default socket