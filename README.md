# ADT Quickstart

This is a set of really basic ADT work that I put together to run through the basics as part of interactive demoing of the capability of ADT.

## Links that were mentioned in the talk

[DTDL specification](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md)
[RealEstateCore Ontology](https://doc.realestatecore.io/3.3/full.html)

## Setup

This requires azure cli and functions cli, and a local python install. 

> I'm running on Windows 10, via VS Code at the versions listed below. I don't think it's particularly version sensitive, but here's what I have. 

### Az CLI

```json
{
  "azure-cli": "2.9.1",
  "azure-cli-command-modules-nspkg": "2.0.3",
  "azure-cli-core": "2.9.1",
  "azure-cli-nspkg": "3.0.4",
  "azure-cli-telemetry": "1.0.4",
  "extensions": {
    "azure-iot": "0.10.8"
  }
}
```

### Functions CLI

```json
3.0.2996
```

### Python

```python
Python 3.8.5
```

## Process

1. Start with the prerequisites of deploying some environments, see the arm in PreReqs folder
1. Create an IoT Hub device called 'simulatedPowerMeter'. I did this in the Azure Portal.
    1. It needs to match the name of the twin you create in the next step for this to work seemlessly. In reality you'd do some work here to make this way more robust. 
    1. Copy the value of the IoTHub connectionstring for this device. Keep the connectionstring handy, you'll need it for the simulator.
    1. While you're here, go to the Built-in Endpoints part of the IoTHub, copy the Eventhub Compatible Endpoint connectionstring into local.settings.json in the `funcs` folder in the "IoTNorth" property
1. If you want to do EventGrid integration (points 10 onwards) setup an Endpoint and Route on the Azure Digital Twin. The one I use has a filter of:
`
type = 'microsoft.iot.telemetry'
AND
STARTS_WITH($body.$metadata.$model, 'dtmi:elastacloud:DigitalTwins:PowerMeter')
AND
$body.Consumption > 0.1
`
1. Setup the newly created ADT using the [basicTwins setup](/basicTwins/)
1. Run the Simulator with a cmdline such as 
`python simulator.py [iothub-connection-string] [double value from 0 - 1]`
1. Open a new terminal window
1. `func start` in the funcs/ folder to locally debug functions. This will take the Simulated data and update the twin
1. Check the twin value in something like [ADT Explorer](https://docs.microsoft.com/en-us/samples/azure-samples/digital-twins-explorer/digital-twins-explorer/#:~:text=%20Using%20adt-explorer%20%201%20First%20run.%20Clicking,Save%20icon%20next%20to%20the%20Run...%20More)
1. Stop the local func (ctrl+c)
1. Deploy the az functions to Azure
`func azure functionApp publish [functionAppName]`
1. Go to the Azure portal, find the function, go to the TwinToUpstreamServices function, go to integrations and set up the EventGrid Subscription to the Topic in the resource group
1. Start and stop the Simulator, making sure the values are larger than 0.1 by changing the double value.
1. View the logs on the TwinToUpstreamServices function to see that you can get the messages out of the twin. 

