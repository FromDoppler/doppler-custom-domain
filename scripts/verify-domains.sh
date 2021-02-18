#!/bin/bash

print_help () {
    echo ""
    echo "Usage: sh verify-domains.sh"
    echo ""
    echo "Read a list of domains from standard input, verify them and return the results in a csv format"
    echo ""
    echo "Examples:"
    echo "  cat example-domains.txt | sh verify-domains.sh"
    echo "  cat example-domains.txt | sh verify-domains.sh > verified-domains.csv"
    echo "  cat example-domains.txt | sh verify-domains.sh | grep FAIL | cut -d',' -f 1 | sh delete-domains.sh"
}

for i in "$@" ; do
case $i in
    -h|--help)
    print_help
    exit 0
    ;;
esac
done

acceptedIps=(184.106.28.222)
googleDNS="8.8.8.8"
cloudflareDNS="1.1.1.1"
openDNS="208.67.222.222"

function get_ipaddr {
  dnsserver="${1}"
  hostname="${2}"
  ip_address=""

  host -t "A" "${hostname}" &>/dev/null "${dnsserver}"
  if ! ip_address="$(host -t A "${hostname}" "${dnsserver}"| awk '/has.*address/{print $NF; exit}')"; then
    exit 1
  fi
  echo "${ip_address}"
}

function get_ipaddr_or_empty {
  if address="$(get_ipaddr "${1}" "${2}")"; then
    if [ -n "${address}" ]; then
      echo "$address ${hostname}"
    fi
  fi
}

function check_our_service_is_behind_the_ip {
  # TODO: add a specific endpoint with a wellknow and unambiguous response
  responseBody="$(curl \
    --request GET \
    --url "https://${1}/routing/version.txt" \
    --header 'host: apis.fromdoppler.com' \
    --insecure \
    -s \
    )"
  [[ "${responseBody}" == dopplerdock/doppler-custom-domain:* ]] && echo "YES" || echo "NO"
}

echo "Domain, Google DNS IP, Cloudflare  DNS IP, OpenDNS DNS IP, Access to our service, Veredict"
while read -r domain; do
  googleDnsIp="$(get_ipaddr "${googleDNS}" "${domain}")"
  cloudflareDnsIp="$(get_ipaddr "${cloudflareDNS}" "${domain}")"
  openDnsIp="$(get_ipaddr "${openDNS}" "${domain}")"
  accessToOurService="$(check_our_service_is_behind_the_ip "${domain}")"
  if [ "${accessToOurService}" = "YES" ] || [[ ${acceptedIps[*]} =~ (^|[[:space:]])"${googleDnsIp}"($|[[:space:]]) ]] || [[ ${acceptedIps[*]} =~ (^|[[:space:]])"${cloudflareDnsIp}"($|[[:space:]]) ]] || [[ ${acceptedIps[*]} =~ (^|[[:space:]])"${openDnsIp}"($|[[:space:]]) ]]; then
    veredict="OK"
  else
    veredict="FAIL"
  fi
  echo "${domain}, ${googleDnsIp}, ${cloudflareDnsIp}, ${openDnsIp}, ${accessToOurService}, ${veredict}"
done
