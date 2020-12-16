# Unit Testing in local environment

In order to be able to run unit tests, if those require JWT to execute correctly, then you will be able to generate/update them by using the following private key located in this directory (`dev.priv.key`)

The steps needed to do so are the following

1. Go to [jwt.io](https://jwt.io/)
2. Select RS256 as the algorithm
3. Paste the token or enter the payload
4. Paste the key mentioned above in the private key section
5. 1. If the token was generated with another key (Doppler/Relay) it will generate a new one. Use that one to run the tests.
   2. If you entered the payload, it will generate a token ready to be used in the tests

> Note: The tokens generated for unit tests will be valid for that context only
