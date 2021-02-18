#!/bin/bash

print_help () {
    echo ""
    echo "Usage: sh get-consul-https-domains.sh"
    echo ""
    echo "Get the list of registered domains in consul and return them as a list."
    echo "Important: it requires to be connected to the VPN."
    echo ""
    echo "Examples:"
    echo "  sh get-consul-https-domains.sh"
    echo "  sh get-consul-https-domains.sh | sh verify-domains.sh > verified-domains.csv"
    echo "  sh get-consul-https-domains.sh | sh verify-domains.sh | grep FAIL | cut -d',' -f 1 | sh delete-domains.sh"
}

for i in "$@" ; do
case $i in
    -h|--help)
    print_help
    exit 0
    ;;
esac
done

# Stop script on NZEC
set -e
# Stop script if unbound variable found (use ${var:-} if intentional)
set -u

consulDomain=172.25.48.222:18500
json=$(curl --request GET \
  --url "http://${consulDomain}/v1/kv/traefik/http/routers/?keys" \
  -s)

echo "${json}" \
  | tr ',' '\n' \
  | grep -e '.*"traefik\/http\/routers\/https_\([^\/]*\)\/rule".*$' \
  | sed 's/.*"traefik\/http\/routers\/https_\([^\/]*\)\/rule".*$/\1/'
