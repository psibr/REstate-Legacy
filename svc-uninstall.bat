
netsh http delete urlacl url=http://+:80/

start cmd.exe /C "src\REstate.Services.Auth\bin\Release\REstate.Services.Auth.exe uninstall"
start cmd.exe /C "src\REstate.Services.Core\bin\Release\REstate.Services.Core.exe uninstall"
start cmd.exe /C "src\REstate.Services.Chrono\bin\Release\REstate.Services.Chrono.exe uninstall"
start cmd.exe /C "src\REstate.Services.ChronoConsumer\bin\Release\REstate.Services.ChronoConsumer.exe uninstall"
start cmd.exe /C "src\REstate.Service.AdminUI\bin\Release\REstate.Services.AdminUI.exe uninstall"