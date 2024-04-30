#!/bin/bash
cd "$(dirname "$0")" || exit 1

dotnet pack ..
cp ../src/bin/Release/*.nupkg $KANAFKA_PACKAGES_DIR
