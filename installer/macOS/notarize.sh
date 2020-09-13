#!/bin/bash

# Certificates and signing.
APP_KEY="Developer ID Application: Xxx Xxxx (XXXXXXXXXX)"
INSTALLER_KEY="Developer ID Installer: Xxx Xxxx (XXXXXXXXXX)"
PROVISION_PROFILE="DeveloperIdApplication.provisionprofile"

# from https://appleid.apple.com/#!&page=signin  > Security > APP-SPECIFIC PASSWORDS > Generate password…
TWO_FAC_UN="xx@xxx.com"
TWO_FAC_PW="xx-xx-xx-xx"

############################################################################################################
############################################################################################################
############################################################################################################

red=$'\e[1;31m'
grn=$'\e[1;32m'
blu=$'\e[1;34m'
mag=$'\e[1;35m'
cyn=$'\e[1;36m'
white=$'\e[0m'
PlistBuddy=/usr/libexec/PlistBuddy
pathtome=$PWD
_self="${0##*/}"


echo ${cyn} "Uploading for notarization..." ${white}

xcrun altool -t osx -f "target/pkg-signed/air-tools-macos-installer-x64-1.0.0.pkg" --primary-bundle-id "air-tools" --notarize-app -u "$TWO_FAC_UN" -p "$TWO_FAC_PW" --output-format xml > "notarize_result.plist"
notarize_exit=$?
if [ "${notarize_exit}" != "0" ]; then
    echo ${red} "Notarization failed: ${notarize_exit}" ${white}
    cat "notarize_result.plist"
    exit 1
fi

request_uuid="$("${PlistBuddy}" -c "Print notarization-upload:RequestUUID"  "notarize_result.plist")"
echo ${cyn} "Notarization UUID: ${request_uuid} result: $("${PlistBuddy}" -c "Print success-message"  "notarize_result.plist")" ${white}

for (( ; ; ))
do
    xcrun altool --notarization-info "${request_uuid}" -u "$TWO_FAC_UN" -p "$TWO_FAC_PW" --output-format xml > "notarize_status.plist"

    notarize_exit=$?
    if [ "${notarize_exit}" != "0" ]; then
        echo ${red} "Notarization failed: ${notarize_exit}" ${white}
        cat "notarize_status.plist"
        exit 1
    fi
    notarize_status="$("${PlistBuddy}" -c "Print notarization-info:Status"  "notarize_status.plist")"
    if [ "${notarize_status}" == "in progress" ]; then
        echo ${cyn} "Waiting for notarization to complete" ${white}
        sleep 10
    else
        echo ${cyn} "Notarization status: ${notarize_status}" ${white}
        break
    fi
done

notarization_log_url="$("${PlistBuddy}" -c "Print notarization-info:LogFileURL"  "notarize_status.plist")"
echo ${cyn} "Notarization log URL: ${notarization_log_url}" ${white}

if [ "${notarize_status}" != "success" ]; then
    echo ${red} "Notarization failed."${white}
    if [ ! -z "${notarization_log_url}" ]; then
        curl "${notarization_log_url}"
    fi
    exit 1
fi

echo ${cyn} "Stapling notarization result..." ${white}

for (( ; ; ))
do
    xcrun stapler staple -q "target/pkg-signed/air-tools-macos-installer-x64-1.0.0.pkg"
    stapler_status=$?
    if [ "${stapler_status}" = "65" ]; then
        echo ${cyn} "Waiting for stapling to find record" ${white}
        sleep 10
    else
        if [ "${stapler_status}" != "0" ]; then
            echo ${red} "Stapling failed: ${notarize_exit}" ${white}
            exit 1
        else
            echo ${cyn} "Stapling OK" ${white}
        fi
        break
    fi
done

rm -r "notarize_status.plist"
rm -r "notarize_result.plist"

echo ${grn} "All Done!" ${white}
