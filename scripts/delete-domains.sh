#!/bin/bash

print_help () {
    echo ""
    echo "Usage: sh delete-domains.sh"
    echo ""
    echo "Read a list of domains from standard input, remove from custom domains API"
    echo ""
    echo "Examples:"
    echo "  cat wrong-domains.txt | sh delete-domains.sh"
}

# TODO: complete the JWT token with an isSU one or do something to read it from an environment variable
# token="eyJhbGciOiJSUzI1NiIsI...
for i in "$@" ; do
case $i in
    -h|--help)
    print_help
    exit 0
    ;;
esac
done

while read -r domain; do
  echo "${domain}"
  curl --request DELETE \
    --url "https://apis.fromdoppler.com/routing/${domain}" \
    --header "authorization: Bearer ${token}"
  echo ""
done
