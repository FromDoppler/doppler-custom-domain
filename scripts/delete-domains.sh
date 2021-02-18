#!/bin/bash

print_help () {
    echo ""
    echo "Usage: sh delete-domains.sh"
    echo ""
    echo "Read a list of domains from standard input, remove from custom domains API"
    echo ""
    echo "It requires the environment variable \`dopplerToken\` defined with a valid \`isSU\` production JWT token"
    echo ""
    echo "Examples:"
    echo "  cat wrong-domains.txt | sh delete-domains.sh"
}

for i in "$@" ; do
case $i in
    -h|--help)
    print_help
    exit 0
    ;;
esac
done

token=${dopplerToken:=};
if [ -z "${token}" ]
then
  echo "Error: \`dopplerToken\` environment variable is required"
  print_help
  exit 1
fi

while read -r domain; do
  echo "${domain} $(
  curl --request DELETE \
    -s -o /dev/null -w "%{http_code}" \
    --url "https://apis.fromdoppler.com/routing/${domain}" \
    --header "authorization: Bearer ${token}")"
done
