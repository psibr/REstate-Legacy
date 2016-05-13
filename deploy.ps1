cd $env:APPLICATION_PATH

./svc-start.bat

cd "src/REstate.Service.AdminUI/wwwroot"

npm install
npm run tsc

cd ../../..