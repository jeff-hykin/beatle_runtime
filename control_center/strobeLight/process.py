DRY_RUN = True
if DRY_RUN:
    while True:
        res = input()
        if (res == "on"):
            print('received on')
        elif (res == "off"):
            print('received on')
        elif (res == "quit"):
            print('received quit')


from Phidget22.Phidget import *
from Phidget22.Devices.DigitalOutput import *
from Phidget22.Devices.DigitalInput import *
import time

digitalOutput = DigitalOutput()

digitalOutput.setChannel(4)

digitalOutput.openWaitForAttachment(5000)

digitalOutput.setState(0)

while True:
    res = input()
    if (res == "on"):
        digitalOutput.setState(1)
    elif (res == "off"):
        digitalOutput.setState(0)
    elif (res == "quit"):
        break

digitalOutput.close()