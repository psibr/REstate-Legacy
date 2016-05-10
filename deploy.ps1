cd $env:APPLICATION_PATH


cd "src/REstate.Service.AdminUI/wwwroot"

npm install
npm run tsc

cd ../../..

./svc-start.bat