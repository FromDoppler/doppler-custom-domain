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

## Domain Validation

In order to avoid registering domains that will fail to negotiate certificates with _Let's Encrypt_, we are validating
that the domain resolves to our IP addresses.

For that reason, we need robust DNS servers configured in our hosts.

Based on the configuration done in our MTA servers we modified the file `/etc/resolve.conf` in our Swarm's nodes with this content:

```ini
search cloudspace.com
nameserver 1.1.1.1
nameserver 1.0.0.1
nameserver 173.203.4.8
nameserver 173.203.4.9
domain cloudspace.com
```

## Scripts

### `get-consul-https-domains.sh`

`get-consul-https-domains.sh` connects to our Consul KV DB and download the list of keys, filtering the domain names that Traefik uses to negotiate Let's Encrypt certificates.

It does not requires validation, only to be connected to our secure VPN.

Example:

```console
$ sh ./get-consul-https-domains.sh

relaytrk.mygreatdomain.com
relaytrk.testing.com
relaytrk.fromdoppler.com
```

### `verify-certificates.sh`

`verify-domains.sh` accepts a list of domains from _STDIN_ and check if they points to our service or not.

It uses two mechanism to validate:

- **Resolved IP address** - The IP is resolved using _Google DNS_, _Cloudflare DNS_ and _OpenDNS_ and compared with a list of our accepted IPs.

- **Verify server response** - We uses the _host_ header to try to resolve our _routing_ service in place of the domain's one and then the result is compared with our expected result.

By the moment we are being permissive: if one of these criteria is successful, we consider that the domain is well configured.

The result is a CSV with the intermediate result and the final verdict.

Finally, if the verdict is OK, he script checks a secure connecting to validate the certificate.

Example:

```console
$ cat example-domains.txt | sh ./verify-domains.sh

relaytrk.makingsense.com, 184.106.28.222, 184.106.28.222, 184.106.28.222, YES, OK, OK
trk.relaytrk.com, 184.106.28.222, 184.106.28.222, 184.106.28.222, YES, OK, WRONG
trk.fromdoppler.com, 184.106.28.222, 184.106.28.222, 184.106.28.222, YES, OK, OK
trk.dopplerrelay.com, 184.106.28.218, 184.106.28.218, 184.106.28.218, NO, FAIL, UNKNOWN
pirulo.com, 74.208.236.178, 74.208.236.178, 74.208.236.178, NO, FAIL, UNKNOWN
noexisto.com, , , , NO, FAIL, UNKNOWN
noexistdfdsjfhdkj231sss6o.com, , , , NO, FAIL, UNKNOWN
```

### `delete-domains.sh`

`delete-domains.sh` accepts a list of domains from _STDIN_ and call to our _routing_ API to de-register them.

It requires a environment variable `dopplerToken` with a valid `isSU` production JWT token.

It returns the list of domains with a status code, for example, if the token is wrong, the status code will be `401`.

```console
$ cat to-delete-domains.txt | sh ./delete-domains.sh

pirulo.com 200
noexisto.com 200
noexistdfdsjfhdkj231sss6o.com 200
```

### Combining the scripts

We can combining the scripts in the following way to delete all incorrect domains in our Consul DB:

```console
$ sh get-consul-https-domains.sh \
    | sh verify-domains.sh \
    | grep FAIL | cut -d',' -f 1 \
    | sh delete-domains.sh \
    > result.txt
```

Or we can do it storing the partial results:

```console
$ filesufix=202102181044
$ sh get-consul-https-domains.sh \
    | sh verify-domains.sh \
    > "domains-verified-${filesufix}.csv"

$ cat "domains-verified-${filesufix}.csv" \
    | grep FAIL | cut -d',' -f 1 \
    > "domains-to-delete-${filesufix}.txt"

$ cat "domains-to-delete-${filesufix}.txt" \
    | sh delete-domains.sh \
    > "domains-deleted-${filesufix}.txt"
```
