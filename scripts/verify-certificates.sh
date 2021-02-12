#!/bin/bash

print_help () {
    echo ""
    echo "Usage: sh verify-certificates.sh"
    echo ""
    echo "Read a list of domains from standard input, verify if the certificate is ok and return the results in a csv format"
    echo ""
    echo "Examples:"
    echo "  cat example-domains.txt | sh verify-certificates.sh"
}

function check_certificate {
  # TODO: add a specific endpoint with a wellknow and unambiguous response
  responseBody="$(curl \
    --request GET \
    --url "https://${1}/routing/version.txt" \
    --header 'host: apis.fromdoppler.com' \
    -s \
    )"
  resultcode=$?
  [[ "${responseBody}" == dopplerdock/doppler-custom-domain:* ]] && echo "${resultcode},YES" || echo "${resultcode},NO"
}
echo "Domain, ResultCode, Our Service, Certificate"
while read -r domain; do
  result="$(check_certificate "${domain}")"
  resultCode=$(echo "${result}" | cut -d',' -f 1)
  ourService=$(echo "${result}" | cut -d',' -f 2)
  certificate=$([[ "${resultCode}" == "0" ]] && echo "OK" || ([[ "${resultCode}" == "60" ]] && echo "WRONG" || echo "UNKNOWN"))
  # TODO: discriminate error result codes in a better way
  echo "${domain}, ${resultCode}, ${ourService}, ${certificate}"
done
