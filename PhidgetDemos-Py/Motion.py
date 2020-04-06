from Phidget22.Phidget import *
from Phidget22.Devices.VoltageRatioInput import *
import time

#Declare any event handlers here. These will be called every time the associated event occurs.

def onSensorChange(self, sensorValue, sensorUnit):
    if sensorValue > -.100:
        print("SensorValue: " + str(sensorValue))
        print("Motion Detected")
        time.sleep(5)

def main():
	#Create your Phidget channels
	voltageRatioInput0 = VoltageRatioInput()

	#Set addressing parameters to specify which channel to open (if any)

	#Assign any event handlers you need before calling open so that no events are missed.
	voltageRatioInput0.setOnSensorChangeHandler(onSensorChange)

	#Open your Phidgets and wait for attachment
	voltageRatioInput0.openWaitForAttachment(5000)

	#Do stuff with your Phidgets here or in your event handlers.
	#Set the sensor type to match the analog sensor you are using after opening the Phidget
	voltageRatioInput0.setSensorType(VoltageRatioSensorType.SENSOR_TYPE_1111)

	try:
		input("Press Enter to Stop\n")
	except (Exception, KeyboardInterrupt):
		pass

	#Close your Phidgets once the program is done.
	voltageRatioInput0.close()

main()
