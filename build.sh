cd ./src/Storage
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..

cd ./src/EntityFramework.Storage
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..

cd ./src/EntityFramework
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..

cd ./src/IdentityServer4
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..

cd ./src/AspNetIdentity
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..