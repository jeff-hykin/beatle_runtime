from Phidget22.Phidget import *
from Phidget22.Devices.DigitalOutput import *
from Phidget22.Devices.DigitalInput import *
import time

serial_number = 95358

digitalOutput = DigitalOutput()
digitalOutput.setChannel(4)
digitalOutput.openWaitForAttachment(5000)
digitalOutput.setState(0)
digitalOutput.setDeviceSerialNumber(serial_number)

while True:
    res = input()
    if (res == "on"):
        digitalOutput.setState(1)
    elif (res == "off"):
        digitalOutput.setState(0)
    elif (res == "quit"):
        break

digitalOutput.close()