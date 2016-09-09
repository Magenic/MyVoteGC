#!/bin/bash

sudo docker build -t core .
sudo docker run -d --name core -p 80:5000 -t core
