#!/bin/bash
# Sync static assets from MkDocs theme root into sphinx_theme/static/
# Run this after updating any MkDocs theme assets.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
STATIC_DIR="$SCRIPT_DIR/static"

echo "Syncing assets from $REPO_ROOT to $STATIC_DIR..."

cp "$REPO_ROOT"/css/*.css "$STATIC_DIR/css/"
cp "$REPO_ROOT"/js/jquery-3.5.1.min.js "$REPO_ROOT"/js/theme.js "$STATIC_DIR/js/"
cp -r "$REPO_ROOT"/js/prism-1.15.0 "$STATIC_DIR/js/"
cp "$REPO_ROOT"/fonts/roboto-* "$STATIC_DIR/fonts/"
cp -r "$REPO_ROOT"/fonts/fontawesome-5.7.2 "$STATIC_DIR/fonts/"
cp "$REPO_ROOT"/img/* "$STATIC_DIR/img/"

echo "Asset sync complete."
