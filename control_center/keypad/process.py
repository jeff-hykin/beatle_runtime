import sys
import time
from Phidget22.Phidget import *
from Phidget22.Devices.DigitalOutput import *
from Phidget22.Devices.DigitalInput import *


number_of_inputs = 4
number_of_outputs = 4
timeout_duration = 15 * 1000 # its in miliseconds

# create input pins
input_pins  = [ DigitalInput() for each_index in range(number_of_inputs)  ]
for each_index, each in enumerate(input_pins):
    each.setChannel(each_index)
    each.openWaitForAttachment(timeout_duration)

# create ouput pins
output_pins = [ DigitalOutput() for each_index in range(number_of_outputs) ]
for each_index, each in enumerate(output_pins):
    each.setChannel(each_index)
    each.openWaitForAttachment(timeout_duration)

def which_key(input_pin, index_of_input):
    key_values = [
        ["1", "2", "3", "A"],
        ["4", "5", "6", "B"],
        ["7", "8", "9", "C"],
        ["*", "0", "#", "D"]
    ]
    return_value = None
    
    # do trial-and-error to find which output pin
    for index_of_output, each_output in enumerate(output_pins):
        each_output.setState(1)
        time.sleep(0.05)
        if input_pin.getState() == 0:
            return_value = key_values[index_of_input][-(index_of_output+1)]
            break
    
    # reset all the output pins
    for each in output_pins:
        each.setState(0)
    
    time.sleep(0.05)
    
    return return_value


previous_vals = [0, 0, 0, 0]
current_values = [0, 0, 0, 0]
while True:
    # find which one(s) changed 
    for index_of_input, (old_value, input_pin) in enumerate(zip(previous_vals, input_pins)):
        # update the input pin value right before checking it
        value = current_values[index_of_input] = input_pin.getState()
        if value == 1 and value != old_value:
            print(which_key(input_pin, index_of_input))
            sys.stdout.flush()
    
    # keep track of old states
    previous_vals = current_values

# clean up
for each in input_pins + output_pins:
    each.close()
