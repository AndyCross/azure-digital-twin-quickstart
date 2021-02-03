import sys
import time
from datetime import datetime
import asyncio
import random
from azure.iot.device import IoTHubDeviceClient, Message
from azure.iot.device.aio import IoTHubDeviceClient


# Will prepare a client ready to stablish connection.

def get_client(connection_string):

    device_client = IoTHubDeviceClient.create_from_connection_string(
        connection_string
    )
    return device_client


# creating a dictionary with values depending of the hour for a household
mydict = {'00:00': 0.071, '00:30': 0.102, '01:00': 0.07, '01:30': 0.099, '02:00': 0.101, '02:30': 0.131, '03:00': 0.127, '03:30': 0.114, '04:00': 0.117, '04:30': 0.07, '05:00': 0.101, '05:30': 0.289, '06:00': 0.183, '06:30': 0.261, '07:00': 0.274, '07:30': 0.144, '08:00': 0.206, '08:30': 0.155, '09:00': 0.163, '09:30': 0.188, '10:00': 0.141, '10:30': 0.133, '11:00': 0.068, '11:30': 0.1,
          '12:00': 0.072, '12:30': 0.108, '13:00': 0.09, '13:30': 0.099, '14:00': 0.112, '14:30': 0.213, '15:00': 0.162, '15:30': 0.111, '16:00': 0.194, '16:30': 0.12, '17:00': 0.149, '17:30': 0.08, '18:00': 0.141, '18:30': 0.511, '19:00': 0.504, '19:30': 0.315, '20:00': 0.374, '20:30': 0.383, '21:00': 0.358, '21:30': 0.533, '22:00': 0.735, '22:30': 0.504, '23:00': 0.398, '23:30': 0.095}

# adjusts hour to data in dict


def round_time(produced_hour):
    if int(produced_hour[-2:]) > 30:
        rdd = (str(int(produced_hour[:2])+1)+":00")
        if rdd == 24:
            rdd = "00:00"
    else:
        rdd = (produced_hour[:2]+":00")

    return rdd


async def main(connection_string, adjust):

    try:
        client = get_client(connection_string)
        await client.connect()

        print("Simulating Power Consumption for Digital Twins")
        print("To stop press Ctrl + C")

        while True:

            # Telemetry
            message = Message("{\"Consumption\": "+str(
                round(
                    float(adjust)*float(mydict[round_time(datetime.now().strftime("%H:%M"))]), 3)
            )+"}", content_encoding="utf-8", content_type="application/json")

            # Send the message each 5 seconds.
            await client.send_message(message)
            print("Message successfully sent waiting 5 sec.")
            print("{\"Consumption\": "+str(
                round(
                    float(adjust)*float(mydict[round_time(datetime.now().strftime("%H:%M"))]), 3)
            )+"}")
            time.sleep(5)

    except KeyboardInterrupt:
        print("IoTHubClient sample stopped")


if __name__ == "__main__":
    asyncio.run(main(sys.argv[1], sys.argv[2]))
# first arg is the connection string, second one is a double that multiplies the values of
# consumption to generate different but reasonable data for each simulator
