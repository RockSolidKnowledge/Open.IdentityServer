#!/usr/bin/env bash
# Define variables
DOTNET_VERSION=3.0.100-preview3-010431

SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/tools

###########################################################################
# INSTALL .NET CORE CLI
###########################################################################

export PATH="$SCRIPT_DIR/.dotnet":$PATH
INSTALLED_DOTNET_VERSION=$( dotnet --version 2>&1 )

if  [ "$INSTALLED_DOTNET_VERSION" != "$DOTNET_VERSION" ]; then
    echo "Installing .NET CLI..."
    if [ ! -d "$SCRIPT_DIR/.dotnet" ]; then
      mkdir "$SCRIPT_DIR/.dotnet"
    fi
    curl -Lsfo "$SCRIPT_DIR/.dotnet/dotnet-install.sh" https://dot.net/v1/dotnet-install.sh
    bash "$SCRIPT_DIR/.dotnet/dotnet-install.sh" --version $DOTNET_VERSION --install-dir .dotnet --no-path
fi

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0