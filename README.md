# Kafka .Net Core Producer and Consumer 

## Introduction
This sample app contains a .Net Core application that produces sample data on to a Kafka topic and the also consumes the topic data. The Kafka broker, zookeeper & control center is docker images that gets pulled in the docker-compose file

## Prerequisites
* [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
* [Docker CE](https://docs.docker.com/docker-for-windows/install/)
* [Docker Compose](https://docs.docker.com/compose/install/)

## Getting started
To get you the application running just execute the following in cmd/bash:
```
docker-compose up
```

## Running App in Docker
This can be used if you have your own Kafka environment already setup.

```
docker build --pull -t kafka-app .
```
```
docker run -it --rm -p 5000:80 --name kafka-app kafka-app
```
___
## Future Changes
I have commented out the SSL authentication for the producer and the consumer because I need to set up the SSL on the broker.
### Creating SSL certificates for Kafka
1. Generate a key file that you will use to generate a certificate signing request.
2. Create a certificate signing request to send to a certificate authority
3. Download a pem file from certificate authority in OpenSSL format, rename it to secure.pem
4. Open .pem file in a text editor of your choice and remove all the Subject information from the file so that you have this left:
```
-----BEGIN CERTIFICATE-----
[ENCODED INFORMATION OMITTED]
-----END CERTIFICATE-----
-----BEGIN CERTIFICATE-----
[ENCODED INFORMATION OMITTED]
-----END CERTIFICATE-----
-----BEGIN CERTIFICATE-----
[ENCODED INFORMATION OMITTED]
-----END CERTIFICATE-----
```

5. Open the .key file and copy its contents and paste it at the bottom of your .pem file so that it finally looks like this:
```
[ENCODED INFORMATION OMITTED]
-----END CERTIFICATE-----
-----BEGIN CERTIFICATE-----
[ENCODED INFORMATION OMITTED]
-----END CERTIFICATE-----
-----BEGIN CERTIFICATE-----
[ENCODED INFORMATION OMITTED]
-----END CERTIFICATE-----
-----BEGIN PRIVATE KEY-----
[ENCODED INFORMATION OMITTED]
-----END PRIVATE KEY-----
```
