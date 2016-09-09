#/bin/bash

ID = ""

sudo docker build -t gcr.io/$ID/myvote .
gcloud docker push gcr.io/$ID/myvote
gcloud app deploy --image-url gcr.io/$ID/myvote -q
