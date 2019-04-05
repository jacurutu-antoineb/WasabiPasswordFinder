# Install:

## Get The Requirements

1. Get Git: https://git-scm.com/downloads
2. Get .NET Core 2.2 SDK: https://www.microsoft.com/net/download
  
## Get and build this software from source code

```sh
git clone https://github.com/lontivero/WasabiPasswordFinder.git
```

## Usage

```
``` 
Usage: dotnet run [OPTIONS]+

Options:
  -x, --password=VALUE       The password you thought you typed
  -s, --secret=VALUE         The secret from your .json file 
                               (EncryptedSecret).
  -o, --ofile=VALUE          Output file
  -t, --tc=VALUE             Test to perform 
                               	-t shift_test
                               	-t single_char_test
  -H                         Show Help
  -d                         Debug logging

You can find your encryptedSecret in your `Wallet.json` file, that you have previously created with Wasabi.

Example: 
  password: badpassword
  "EncryptedSecret": "6PYLGPcpMGHPDz1DHAFGn94f8dEZ9YjMz6LyJyRqTYuZqHZU8xyfJjskK9"

  dotnet run -s 6PYLGPcpMGHPDz1DHAFGn94f8dEZ9YjMz6LyJyRqTYuZqHZU8xyfJjskK9 -p baDpAssword -t shift_test

## NOTE

This process is rather slow and CPU heavy. Even for a 10 chars length password it can take significant time to run and
finding the error is not warranted in any case. Please review the code before running it.
