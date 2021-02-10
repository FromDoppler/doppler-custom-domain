# Doppler Custom Domain Service

This API allows registering domains to be routed in our Traefik service and to negotiate Let's Encrypt Certificates.

## Continuous Deployment to test and production environments

We are following the same criteria that [Doppler
Forms](https://github.com/MakingSense/doppler-forms/blob/master/README.md#continuous-deployment-to-test-and-production-environments).

## VS Code REST Client's files

In the folder `http-queries` we have some [VS Code REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)'s files that could help us in our daily work.

Some of these files require a little setup in your user's VS Code settings:

![rest-client-user-settings0](./DopplerCustomDomain.Test/docs/rest-client-user-settings0.png)

There are also a project's configuration in `.vscode/settings.json`.

When you run the queries you can select the environment using VS Code UI:

![rest-client-environment-selection](DopplerCustomDomain.Test/docs/rest-client-environment-selection0.png)
