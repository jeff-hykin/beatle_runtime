from Phidget22.Phidget import *
from Phidget22.Devices.DigitalOutput import *
from Phidget22.Devices.DigitalInput import *
import time

def check_keys(di, do, i):
    ret = (-1, -1)
    for j in range(num_out):
        do[j].setState(1)
        time.sleep(0.05)
        if (di[i].getState() == 0):
            ret = (j, i)
            break
    for j in range(num_out):
        do[j].setState(0)
    time.sleep(0.05)
    return ret
   
key_values = [["1", "2", "3", "A"], ["4", "5", "6", "B"], ["7", "8", "9", "C"], ["*", "0", "#", "D"]]
def get_value(pos):
    return key_values[pos[0]][pos[1]]

num_in = 4
num_out = 4

digitalInput = [0] * num_in
digitalOutput = [0] * num_out

for i in range(num_in):
    digitalInput[i] = DigitalInput()
for i in range(num_out):
    digitalOutput[i] = DigitalOutput()

for i in range(num_in):
    # digitalInput[i].setDeviceSerialNumber(96781)
    digitalInput[i].setChannel(i)
for i in range(num_out):
    # digitalOutput[i].setDeviceSerialNumber(96781)
    digitalOutput[i].setChannel(i)

for i in range(num_in):
    digitalInput[i].openWaitForAttachment(5000)
for i in range(num_out):
    digitalOutput[i].openWaitForAttachment(5000)

last_vals = [0, 0, 0, 0]
curr_vals = [0, 0, 0, 0]
while (True):
    for i in range(num_in):
        curr_vals[i] = digitalInput[i].getState()
        if (curr_vals[i] != last_vals[i] and curr_vals[i] == 1):
            pos = check_keys(digitalInput, digitalOutput, i)
            print(pos, get_value(pos))
        last_vals[i] = curr_vals[i]

for i in range(num_in):
    digitalInput[i].close()
for i in range(num_out):
    digitalOutput[i].close()
