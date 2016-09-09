from azure.storage.blob import BlobService
from oauth2client.client import GoogleCredentials
from googleapiclient.discovery import build
from googleapiclient import discovery
from googleapiclient import http
from os.path import join
import os, mimetypes

ACCOUNT_NAME = ""
ACCOUNT_KEY = ""
AZURE_CONTAINER_NAME = ""
GOOGLE_CONTAINER_NAME = ""
LOCAL_FOLDER = ""

def create_service():
    credentials = GoogleCredentials.get_application_default()
    return discovery.build('storage', 'v1', credentials=credentials)

def upload_object(filename):
    service = create_service()
    with open(filename, 'rb') as f:
        req = service.objects().insert(
            bucket=GOOGLE_CONTAINER_NAME, name=filename,
            media_body=http.MediaIoBaseUpload(f, mimetypes.guess_type(filename)[0]))
        resp = req.execute()
    return resp

blob_service = BlobService(account_name=ACCOUNT_NAME, account_key=ACCOUNT_KEY)
blobs = blob_service.list_blobs(AZURE_CONTAINER_NAME)

for blob in blobs:
    local_file = join(LOCAL_FOLDER, blob.name)
    local_path = os.path.dirname(local_file)
    if not os.path.exists(local_path):
        os.makedirs(local_path)
    try:
        blob_service.get_blob_to_path(AZURE_CONTAINER_NAME, blob.name, local_file)
        upload_object(LOCAL_FOLDER + "/" + blob.name)
    except:
        print "error while moving %s" % blob.name