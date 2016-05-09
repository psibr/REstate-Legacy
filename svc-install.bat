
netsh http add urlacl url=http://+:80/ user="NT AUTHORITY\NETWORK SERVICE"

start cmd.exe /C "src\REstate.Services.Auth\bin\Release\REstate.Services.Auth.exe install"
start cmd.exe /C "src\REstate.Services.Core\bin\Release\REstate.Services.Core.exe install"
start cmd.exe /C "src\REstate.Services.Chrono\bin\Release\REstate.Services.Chrono.exe install"
start cmd.exe /C "src\REstate.Services.ChronoConsumer\bin\Release\REstate.Services.ChronoConsumer.exe install"
start cmd.exe /C "src\REstate.Service.AdminUI\bin\Release\REstate.Services.AdminUI.exe install"