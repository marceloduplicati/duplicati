
# Environment variables for tests

On github actions these are mapped 1:1 to secrets, even the non password fields are stored in secrets.

Tests like FTP, SSH and Webdav are self-contained, they do not require any environment variable.

## General

These control the size and number of files generated.

```
MAX_FILE_SIZE    default is 1000kb
NUMBER_OF_FILES  default is 20
```

## Google Drive:

Google Drive credentials are mapped to the following environment variables:

```
TESTCREDENTIAL_GOOGLEDRIVE_FOLDER
TESTCREDENTIAL_GOOGLEDRIVE_TOKEN
```
## S3

S3 credentials are mapped to the following environment variables:

Attention: **AWS TESTCREDENTIAL_S3_SECRET is URI escaped automatically, supply the raw value.**

```
TESTCREDENTIAL_S3_KEY
TESTCREDENTIAL_S3_SECRET
TESTCREDENTIAL_S3_BUCKETNAME
TESTCREDENTIAL_S3_REGION
```

## Dropbox

Dropbox credentials are mapped to the following environment variables:

```
TESTCREDENTIAL_DROPBOX_FOLDER
TESTCREDENTIAL_DROPBOX_TOKEN
```

## Azure Blob

Attention: **TESTCREDENTIAL_AZURE_ACCESSKEY is URI escaped automatically, supply the raw value.**


```
TESTCREDENTIAL_AZURE_ACCOUNTNAME
TESTCREDENTIAL_AZURE_ACCESSKEY
TESTCREDENTIAL_AZURE_CONTAINERNAME
```

## Running the tests

[TestContainers](https://testcontainers.org/) is a pre-requisite for SSH, FTP and Webdav tests. It is not required for the other tests.

Set the environment variables as described above, then run the tests using the following commands:

Minimal Verbosity:

`dotnet test Duplicati.Backend.Tests.sln --logger:"console;verbosity=normal"`

Running with full verbosity (useful if tests are failing):

`dotnet test Duplicati.Backend.Tests.sln --logger:"console;verbosity=detailed"`

Running specific tests:

`dotnet test Duplicati.Backend.Tests.sln --logger:"console;verbosity=detailed" --filter="Name=TestDropBox"`


