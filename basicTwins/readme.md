# prereqs

az account set --subscription a280525c-d520-48ab-a1c9-6463fb958790

az dt list

# create models

az dt model create --models .\model.dtdl.json -n 'iotnorth-twin' 
az dt model create --models .\model2.dtdl.json -n 'iotnorth-twin' 

# create instances

az dt twin create --dtmi "dtmi:elastacloud:DigitalTwins:PowerMeter;1" --twin-id simulatedPowerMeter --properties .\instance.json -n iotnorth-twin
az dt twin create --dtmi "dtmi:elastacloud:DigitalTwins:MainsCircuit;1" --twin-id simulatedCircuit --properties .\instance2.json -n iotnorth-twin

# create relationship

az dt twin relationship create -n iotnorth-twin --relationship-id demorelid --relationship connectedTo --twin-id simulatedCircuit --target simulatedPowerMeter

# find it real quick

az dt twin relationship list -n iotnorth-twin --source simulatedCircuit