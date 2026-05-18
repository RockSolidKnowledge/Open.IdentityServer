rm -r ./_build

docker build -t identityserver-docs .
docker create --name docs-build identityserver-docs
mkdir -p ./_build
docker cp docs-build:/docs/_build/html ./_build/html
docker rm docs-build