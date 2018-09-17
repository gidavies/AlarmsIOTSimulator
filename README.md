# AlarmsIOTSimulator
A .NET Core console app to publish simple alarm data to an Azure Event Grid topic. 

The alarm data consists of:

- device id
- status (red or blue)
- longtitude
- latitude
- image (a URL to an image related to the alarm)
- name 
- text

Resultant JSON Schema:

```JSON
{
    "properties": {
        "DeviceId": {
            "type": "number"
        },
        "Image": {
            "type": "string"
        },
        "Latitude": {
            "type": "number"
        },
        "Longitude": {
            "type": "number"
        },
        "Status": {
            "type": "string"
        },
        "Name": {
            "type": "string"
        },
        "Text": {
            "type": "string"
        }
    },
    "type": "object"
}
```

## Pre-reqs

You will need an [Azure Event Grid topic](https://docs.microsoft.com/en-us/azure/event-grid/custom-event-quickstart-portal#create-a-custom-topic).

## Usage

The following environment variables are required to be set before running from the command line:

- AlarmTopic - The Event Grid Topic EndPoint.
- AlarmResource - The path to the resource in the form: /subscriptions/[your subscription id]/resourceGroups/[your resource group name]/providers/Microsoft.EventGrid/topics/[your EventGrid topic name].
- AlarmKey - The Event Grid Topic key.
- AlarmFalseImage - The URL to an image that can be used for a false positive event.
- AlarmTrueImage - The URL to an image that can be used for a positive event.

The following environment variables are optional:

- AlarmInterval - The ms between alarm events, default = 5000.
- AlarmNumDevices - The number of alarms, default = 20.
- AlarmMaxLat AlarmMinLat AlarmMaxLong AlarmMinLong - Describes the area within which random cordinates will be created, default = central England. Latitude and Longitude must all be decimal with 6 significant points and all 4 must be provided.
- AlarmStatusWeight - Must be more than 2, the lower the weighting the proportionally more red status alerts. Default = 10.

Then from the command line run:

`dotnet run`
            
You can also build a Docker image using the included Dockerfile such as: 

`docker build --rm -f Dockerfile -t alarmsiotsimulator:latest .`

The [image is available on DockerHub](https://hub.docker.com/r/gdavi/alarms-iot-simulator/) to use immediately. To pass the environment variables into the docker container you can use the following:

`docker run gdavi/alarms-iot-simulator -e AlarmTopic='[TOPIC URL]' -e AlarmResource='[RESOURCE ID]' -e AlarmKey='[TOPIC KEY]' -e AlarmFalseImage='[FALSE IMAGE URL]' -e AlarmTrueImage='[TRUE IMAGE URL]'`

To run in Azure Container Instance via the Azure CLI or command shell:

`az container create --resource-group [RESOURCE GROUP] --name [NAME] --image gdavi/alarms-iot-simulator --restart-policy OnFailure --environment-variables AlarmTopic=[TOPIC URL] AlarmResource=[RESOURCE ID] AlarmKey=[TOPIC KEY] AlarmFalseImage=[FALSE IMAGE URL] AlarmTrueImage=[TRUE IMAGE URL]`

To stop and delete in Azure Container Instance via the Azure CLI or command shell:

`az container delete --name [NAME] -g [RESOURCE GROUP]`