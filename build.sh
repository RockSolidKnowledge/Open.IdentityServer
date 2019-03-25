mkdir nuget

cd ./src/Storage
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..
cp ./src/Storage/artifacts/* ./nuget

cd ./src/IdentityServer4
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..
cp ./src/IdentityServer4/artifacts/* ./nuget

cd ./src/EntityFramework.Storage
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..
cp ./src/EntityFramework.Storage/artifacts/* ./nuget

cd ./src/EntityFramework
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..
cp ./src/EntityFramework/artifacts/* ./nuget

cd ./src/AspNetIdentity
./build.sh

if [ $? -ne 0 ]; then
    echo "An error occured."
    exit 1
fi

cd ../..
cp ./src/AspNetIdentity/artifacts/* ./nuget